using System.Net;
using System.Text.Json;
using Imperator.Core;
using Imperator.Core.Missions;

namespace Imperator.Api.Tests;

public class MissionEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly ContentSet Content = ContentSet.LoadDefault(AppContext.BaseDirectory);

    private async Task<(HttpResponseMessage Response, JsonElement Body)> Get(string url)
    {
        var response = await factory.CreateClient().GetAsync(url, TestContext.Current.CancellationToken);
        var text = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        return (response, JsonDocument.Parse(text).RootElement.Clone());
    }

    [Theory]
    [InlineData("ship")]
    [InlineData("land")]
    public async Task ReturnsAMission(string setting)
    {
        var (response, m) = await Get($"/missions?setting={setting}&seed=42");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var expected = MissionGenerator.Generate(Content, new MissionRequest(setting, 42));
        Assert.Equal(expected.Id, m.GetProperty("id").GetString());
        Assert.Equal(expected.MissionType, m.GetProperty("mission").GetString());
        Assert.Equal(expected.Objectives.Count, m.GetProperty("objectives").GetArrayLength());
        Assert.Equal(expected.Enemies.Count, m.GetProperty("enemies").GetArrayLength());
        Assert.Equal(expected.Contacts.Count, m.GetProperty("contacts").GetArrayLength());
        Assert.Equal(expected.Briefing.Select(b => b.Heading), m.GetProperty("briefing").EnumerateArray().Select(b => b.GetProperty("heading").GetString()));
        Assert.Equal(expected.Rolls.Count, m.GetProperty("rolls").GetArrayLength());
        Assert.Equal(Content.Core.Version, m.GetProperty("content_version").GetString());
        Assert.Equal(setting == "ship", m.TryGetProperty("vessel", out _));
        Assert.Equal(setting == "land", m.TryGetProperty("sectors", out _));
    }

    [Fact]
    public async Task NamedOppositionUsesItsOwnThreatTiers()
    {
        var (response, mission) = await Get("/missions?setting=land&seed=42&mission=capture&tier=4&environment=industrial&opposition=separatist");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("separatist", mission.GetProperty("opposition").GetString());
        Assert.Equal("sep_grievous", mission.GetProperty("hvt").GetString());
        Assert.Equal("sep_bx_squad", mission.GetProperty("reinforcement").GetString());
        Assert.All(mission.GetProperty("enemies").EnumerateArray(), e =>
            Assert.Equal("separatist", Content.Element(e.GetProperty("element").GetString()!).Faction));
    }
    [Fact]
    public async Task NamedChoicesAreKept()
    {
        var (_, m) = await Get("/missions?setting=land&seed=5&mission=sabotage&tier=4&environment=fortified&size=large");
        Assert.Equal("sabotage", m.GetProperty("mission").GetString());
        Assert.Equal(4, m.GetProperty("tier").GetInt32());
        Assert.Equal("fortified", m.GetProperty("environment").GetString());
        Assert.Equal(48, m.GetProperty("board").GetProperty("width").GetInt32());
        var (_, s) = await Get("/missions?setting=ship&seed=5&vessel=corvette&mission=capture");
        Assert.Equal("corvette", s.GetProperty("vessel").GetString());
        Assert.Equal(Content.Tier(s.GetProperty("tier").GetInt32()).Hvt, s.GetProperty("hvt").GetString());
    }

    [Theory]
    [InlineData("/missions?seed=1", "setting ''")]
    [InlineData("/missions?setting=space&seed=1", "setting 'space'")]
    [InlineData("/missions?setting=ship", "seed is required")]
    [InlineData("/missions?setting=ship&seed=1&mission=heist", "mission 'heist'")]
    [InlineData("/missions?setting=ship&seed=1&tier=9", "tier '9'")]
    [InlineData("/missions?setting=ship&seed=1&vessel=battleship", "vessel 'battleship'")]
    [InlineData("/missions?setting=land&seed=1&environment=moon", "environment 'moon'")]
    [InlineData("/missions?setting=land&seed=1&size=huge", "size 'huge'")]
    [InlineData("/missions?setting=ship&seed=1&size=large", "Decks are small missions")]
    public async Task RejectsBadRequests(string url, string detail)
    {
        var (response, problem) = await Get(url);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(detail, problem.GetProperty("detail").GetString());
    }
}
