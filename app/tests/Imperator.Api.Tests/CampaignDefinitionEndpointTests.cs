using System.Net;
using System.Text.Json;
using Imperator.Core;

namespace Imperator.Api.Tests;

public class CampaignDefinitionEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly ContentSet Content = ContentSet.LoadDefault(AppContext.BaseDirectory);

    private async Task<(HttpResponseMessage Response, JsonElement Body)> Get(string url)
    {
        var response = await factory.CreateClient().GetAsync(url, TestContext.Current.CancellationToken);
        var text = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        return (response, JsonDocument.Parse(text).RootElement.Clone());
    }

    [Fact]
    public async Task ListsBothDefinitions()
    {
        var (response, list) = await Get("/campaign-definitions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Content.Core.Version, response.Headers.GetValues("Imperator-Content-Version").Single());
        var byId = list.EnumerateArray().ToDictionary(c => c.GetProperty("id").GetString()!);
        Assert.Equal(["foundry-moon", "outer-reach-patrol"], byId.Keys.Order());

        var fm = byId["foundry-moon"];
        Assert.Equal(("Foundry Moon", "bespoke", "short_arc", "republic", "separatist"),
            (fm.GetProperty("name").GetString(), fm.GetProperty("source").GetString(), fm.GetProperty("format").GetString(),
             fm.GetProperty("faction").GetString(), fm.GetProperty("opposition").GetString()));
        var orp = byId["outer-reach-patrol"];
        Assert.Equal("procedural", orp.GetProperty("source").GetString());
        Assert.Equal(JsonValueKind.Null, orp.GetProperty("opposition").ValueKind);
    }

    [Theory]
    [InlineData("foundry-moon")]
    [InlineData("outer-reach-patrol")]
    public async Task ServesEachWholeDefinition(string id)
    {
        var (response, body) = await Get($"/campaign-definitions/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Content.Core.Version, response.Headers.GetValues("Imperator-Content-Version").Single());
        Assert.Equal(id, body.GetProperty("id").GetString());
        Assert.Equal(Content.Core.Version, body.GetProperty("content_version").GetString());
        Assert.False(string.IsNullOrEmpty(body.GetProperty("summary").GetString()));
    }

    [Fact]
    public async Task FoundryMoonEmbedsItsMissions()
    {
        var (_, body) = await Get("/campaign-definitions/foundry-moon");

        Assert.Equal("1", body.GetProperty("start").GetString());
        var missions = body.GetProperty("missions").EnumerateArray().ToList();
        Assert.Equal(4, missions.Count);
        var twoA = missions.Single(m => m.GetProperty("label").GetString() == "2A").GetProperty("mission");
        Assert.True(twoA.TryGetProperty("objectives", out _));
        var decisive = twoA.GetProperty("next").EnumerateArray().Single(n => n.GetProperty("grade").GetString() == "Decisive");
        Assert.Equal("3", decisive.GetProperty("to").GetString());
        Assert.Equal(2, decisive.GetProperty("effects").GetProperty("remove_contacts").GetInt32());
    }

    [Fact]
    public async Task UnknownDefinitionIsNotFound()
    {
        var (response, body) = await Get("/campaign-definitions/nowhere");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("foundry-moon", body.GetProperty("detail").GetString());
    }
}
