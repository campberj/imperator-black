using System.Net;
using System.Text.Json;
using Imperator.Core;
using Imperator.Core.Layouts;

namespace Imperator.Api.Tests;

public class DeckEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly ContentSet Content = ContentSet.LoadDefault(AppContext.BaseDirectory);

    public static TheoryData<string> VesselIds() => new(Content.Layouts.Vessels.Select(v => v.Id));

    private async Task<(HttpResponseMessage Response, JsonElement Body)> Get(string url)
    {
        var response = await factory.CreateClient().GetAsync(url, TestContext.Current.CancellationToken);
        var text = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        return (response, JsonDocument.Parse(text).RootElement.Clone());
    }

    [Theory]
    [MemberData(nameof(VesselIds))]
    public async Task ReturnsADeckForEachVessel(string vessel)
    {
        var (response, deck) = await Get($"/decks/{vessel}?seed=7");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Content.Core.Version, response.Headers.GetValues("Imperator-Content-Version").Single());
        Assert.Equal(vessel, deck.GetProperty("vessel").GetString());
        Assert.Equal(7, deck.GetProperty("seed").GetInt32());
        Assert.Equal(Content.Core.Version, deck.GetProperty("content_version").GetString());
        Assert.Equal("deck", deck.GetProperty("kind").GetString());

        // The same deck Core builds, in the scenario format's snake_case names.
        var expected = DeckGenerator.Generate(Content, vessel, 7).Deck;
        Assert.Equal(expected.Id, deck.GetProperty("id").GetString());
        Assert.Equal(expected.Tiles.Count, deck.GetProperty("tiles").GetArrayLength());
        Assert.Equal(expected.Doors.Count, deck.GetProperty("doors").GetArrayLength());
        Assert.Equal(expected.Rolls, deck.GetProperty("rolls").GetInt32());
        Assert.Equal(expected.TileCounts["compartment"], deck.GetProperty("tile_counts").GetProperty("compartment").GetInt32());
        Assert.Equal(expected.Board.Width, deck.GetProperty("board").GetProperty("width").GetInt32());
        Assert.Equal(expected.Links.Count, deck.GetProperty("links").GetArrayLength());
        Assert.Equal(expected.Spine, deck.GetProperty("spine").EnumerateArray().Select(x => x.GetString()!).ToList());
        if (expected.Heart is not null) Assert.Equal(expected.Heart, deck.GetProperty("heart").GetString());
    }

    [Fact]
    public async Task SameSeedSameDeck()
    {
        var client = factory.CreateClient();
        var a = await client.GetStringAsync("/decks/corvette?seed=42", TestContext.Current.CancellationToken);
        var b = await client.GetStringAsync("/decks/corvette?seed=42", TestContext.Current.CancellationToken);
        var c = await client.GetStringAsync("/decks/corvette?seed=43", TestContext.Current.CancellationToken);
        Assert.Equal(a, b);
        Assert.NotEqual(a, c);
    }

    [Fact]
    public async Task LeavesOutNullsAsTheScenarioFormatDoes()
    {
        var (_, deck) = await Get("/decks/freighter?seed=1");
        foreach (var door in deck.GetProperty("doors").EnumerateArray())
            Assert.Equal(door.GetProperty("state").GetString() == "locked", door.TryGetProperty("security", out _));
        foreach (var tile in deck.GetProperty("tiles").EnumerateArray())
            if (tile.TryGetProperty("room", out var room)) Assert.Equal(JsonValueKind.String, room.ValueKind);
    }

    [Fact]
    public async Task RequiresASeed()
    {
        var (response, problem) = await Get("/decks/freighter");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("seed is required", problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task UnknownVesselIsNotFound()
    {
        var (response, problem) = await Get("/decks/battleship?seed=1");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("'battleship'", problem.GetProperty("detail").GetString());
        Assert.Contains("freighter", problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task NonNumericSeedIsABadRequest()
    {
        var response = await factory.CreateClient().GetAsync("/decks/freighter?seed=seven", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
