using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Net.Http.Json;
using Imperator.Core.Forces;
using Imperator.Core;

namespace Imperator.Api.Tests;

public class EndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly string ContentPath = ContentSet.FindDefaultPath(AppContext.BaseDirectory);

    [Fact]
    public async Task ContentServesCompiledJsonWithVersion()
    {
        var response = await factory.CreateClient().GetAsync("/content", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(ContentSet.LoadFile(ContentPath).Core.Version, response.Headers.GetValues("Imperator-Content-Version").Single());
        Assert.NotNull(response.Headers.ETag);
        Assert.Equal(await File.ReadAllBytesAsync(ContentPath, TestContext.Current.CancellationToken),
            await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GymTwoForceCheckReturnsReferenceTVAndWarnings()
    {
        var plan = new ForcePlan("Gym 2", "republic", "fireteam", "custom", null, 350, 8,
            [new ForceTeam("clone-1", "Clone trooper squad", "line", "clone_squad",
                [new("rep_clone_sergeant", 1), new("rep_clone_trooper", 3), new("rep_clone_heavy", 1)])],
            [new ForceCharacter("rep_ahsoka", "clone-1")], []);
        var client = factory.CreateClient();
        var ok = await client.PostAsJsonAsync("/forces/check", plan, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        using var doc = JsonDocument.Parse(await ok.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(305, doc.RootElement.GetProperty("tv").GetInt32());
        Assert.Equal(6, doc.RootElement.GetProperty("models").GetInt32());
        Assert.Empty(doc.RootElement.GetProperty("findings").EnumerateArray());
        var over = await client.PostAsJsonAsync("/forces/check", plan with { TvLimit = 300 }, TestContext.Current.CancellationToken);
        using var other = JsonDocument.Parse(await over.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Contains(other.RootElement.GetProperty("findings").EnumerateArray(),
            f => f.GetProperty("code").GetString() == "over_tv");
    }

    [Fact]
    public async Task ValuesAreCalculatedByCore()
    {
        var response = await factory.CreateClient().GetAsync("/values", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var rows = body.RootElement.EnumerateArray().ToList();
        var set = ContentSet.LoadFile(ContentPath);
        Assert.Equal(set.Troops.Troops.Count, rows.Count);
        var clone = rows.Single(row => row.GetProperty("troop").GetString() == "rep_clone_trooper");
        Assert.Equal(41, clone.GetProperty("tv").GetInt32());
        Assert.Equal(5, clone.GetProperty("requisition").GetInt32());
    }

    [Fact]
    public async Task ContentIsNotModifiedWhenETagMatches()
    {
        var client = factory.CreateClient();
        var first = await client.GetAsync("/content", TestContext.Current.CancellationToken);

        var request = new HttpRequestMessage(HttpMethod.Get, "/content");
        request.Headers.IfNoneMatch.Add(first.Headers.ETag!);
        var second = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotModified, second.StatusCode);
    }

    [Fact]
    public async Task ContentIsServedWhenETagIsStale()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/content");
        request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue("\"0.0.0-stale\""));
        var response = await factory.CreateClient().SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthReportsUnhealthyWithoutDatabase()
    {
        var response = await factory.CreateClient().GetAsync("/health", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal("Unhealthy", body.RootElement.GetProperty("status").GetString());
        Assert.Equal("Unhealthy", body.RootElement.GetProperty("checks").GetProperty("database").GetString());
        Assert.Equal(ContentSet.LoadFile(ContentPath).Core.Version, body.RootElement.GetProperty("contentVersion").GetString());
    }
}
