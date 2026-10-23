using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Imperator.Api.Data;
using Imperator.Core.Campaigns;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Imperator.Api.Tests;

/// <summary>
/// Real PostgreSQL integration: set IMPERATOR_TEST_CONNECTION to a disposable test database.
/// CI enables it against its ephemeral PostgreSQL service.
/// </summary>
public class SoloCampaignIntegrationTests
{
    private sealed class DatabaseApiFactory(string connection, Guid playerId) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("ConnectionStrings:Imperator", connection);
            builder.UseSetting("SoloCampaigns:LocalPlayerId", playerId.ToString());
        }

        public async Task ApplyMigrations(CancellationToken ct)
        {
            using var scope = Services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<ImperatorDb>().Database.MigrateAsync(ct);
        }
    }

    [Fact]
    public async Task SoloCampaignSurvivesRestartAndReplaysSameEvents()
    {
        var connection = Environment.GetEnvironmentVariable("IMPERATOR_TEST_CONNECTION");
        if (string.IsNullOrEmpty(connection))
        {
            // The default unit test suite needs no Postgres; CI always supplies an ephemeral connection.
            return;
        }

        var player = Guid.NewGuid();
        var otherPlayer = Guid.NewGuid();
        Guid id;
        string before;
        const string renamed = "Foundry Moon — continued";
        var ct = TestContext.Current.CancellationToken;

        using (var firstFactory = new DatabaseApiFactory(connection, player))
        {
            await firstFactory.ApplyMigrations(ct);
            var first = firstFactory.CreateClient();
            var created = await first.PostAsJsonAsync("/campaigns", new { name = " Foundry Moon " }, ct);
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            using var createdBody = JsonDocument.Parse(await created.Content.ReadAsStringAsync(ct));
            id = createdBody.RootElement.GetProperty("id").GetGuid();
            Assert.Equal("Foundry Moon", createdBody.RootElement.GetProperty("name").GetString());
            Assert.Equal(1, createdBody.RootElement.GetProperty("version").GetInt64());

            var renamedResponse = await first.PatchAsJsonAsync($"/campaigns/{id}/name", new { name = renamed }, ct);
            Assert.Equal(HttpStatusCode.OK, renamedResponse.StatusCode);
            Assert.Contains("2", await renamedResponse.Content.ReadAsStringAsync(ct));

            var get = await first.GetAsync($"/campaigns/{id}", ct);
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            before = await get.Content.ReadAsStringAsync(ct);
        }

        // A fresh WebApplicationFactory models restarting the API: no cached state may be required.
        using (var restartedFactory = new DatabaseApiFactory(connection, player))
        {
            var restarted = restartedFactory.CreateClient();
            var get = await restarted.GetAsync($"/campaigns/{id}", ct);
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            Assert.Equal(before, await get.Content.ReadAsStringAsync(ct));

            var list = await restarted.GetFromJsonAsync<JsonElement>("/campaigns", ct);
            Assert.Contains(list.EnumerateArray(), c => c.GetProperty("id").GetGuid() == id &&
                c.GetProperty("name").GetString() == renamed && c.GetProperty("version").GetInt64() == 2);

            var history = await restarted.GetFromJsonAsync<JsonElement>($"/campaigns/{id}/events", ct);
            Assert.Equal(2, history.GetArrayLength());
            var replay = SoloCampaignLifecycle.Replay(history.EnumerateArray().Select(e =>
                new SoloCampaignEvent(e.GetProperty("sequence").GetInt64(),
                    e.GetProperty("type").GetString()!, e.GetProperty("payload").GetString()!)));
            using var body = JsonDocument.Parse(before);
            Assert.Equal(body.RootElement.GetProperty("id").GetGuid(), replay.Id);
            Assert.Equal(body.RootElement.GetProperty("name").GetString(), replay.Name);
            Assert.Equal(body.RootElement.GetProperty("contentVersion").GetString(), replay.ContentVersion);
            Assert.Equal(body.RootElement.GetProperty("version").GetInt64(), replay.Version);

            var bad = await restarted.PostAsJsonAsync("/campaigns", new { name = "   " }, ct);
            Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
        }

        using (var separateFactory = new DatabaseApiFactory(connection, otherPlayer))
        {
            var other = separateFactory.CreateClient();
            Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync($"/campaigns/{id}", ct)).StatusCode);
            var list = await other.GetFromJsonAsync<JsonElement>("/campaigns", ct);
            Assert.DoesNotContain(list.EnumerateArray(), c => c.GetProperty("id").GetGuid() == id);
        }
    }
}
