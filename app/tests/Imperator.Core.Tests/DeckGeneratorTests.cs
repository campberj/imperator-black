using System.Text.Json;
using Imperator.Core;
using Imperator.Core.Layouts;
using static Imperator.Core.Tests.TestContent;

namespace Imperator.Core.Tests;

public class DeckGeneratorTests
{
    private const int Seeds = 2000;

    public static TheoryData<string> VesselIds() => new(Set.Layouts.Vessels.Select(v => v.Id));

    private static readonly Lazy<JsonElement> Reference = new(() =>
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (!File.Exists(Path.Combine(dir!.FullName, "content", "dist", "content.json"))) dir = dir.Parent;
        var path = Path.Combine(dir.FullName, "app", "tests", "Imperator.Core.Tests", "Fixtures", "layout-reference.json");
        return JsonDocument.Parse(File.ReadAllText(path)).RootElement.Clone();
    });

    private static List<GeneratedDeck> Decks(string vessel, int n = Seeds) =>
        Enumerable.Range(1, n).Select(s => DeckGenerator.Generate(Set, vessel, s)).ToList();

    [Fact]
    public void ReferenceFixtureMatchesContent()
    {
        Assert.Equal(Seeds, Reference.Value.GetProperty("seeds").GetInt32());
        Assert.Equal(Rules.Version, Reference.Value.GetProperty("content_version").GetString());
    }

    // tools/generate_layout.py is the yardstick (tools/layout_reference.py writes the fixture). The two generators
    // draw different random streams, so the comparison is statistical; tolerances sit well above sampling noise
    // over 2,000 decks (about 0.02 on a mean, 1.5 points on a percentage) but catch a step ported wrongly.
    [Theory]
    [MemberData(nameof(VesselIds))]
    public void MatchesReferenceStatistics(string vessel)
    {
        var r = Reference.Value.GetProperty("vessels").GetProperty(vessel);
        var decks = Decks(vessel);
        var v = Set.Vessel(vessel);
        double Mean(Func<GeneratedDeck, double> f) => decks.Average(f);
        double Share(Func<GeneratedDeck, bool> f) => decks.Count(f) / (double)decks.Count;
        int Tiles(GeneratedDeck d, string kind) => d.Deck.TileCounts.GetValueOrDefault(kind);

        Assert.Equal(r.GetProperty("compartments_mean").GetDouble(), Mean(d => d.Compartments), 0.15);
        Assert.Equal(r.GetProperty("below_minimum").GetDouble(), Share(d => d.Compartments < v.Compartments[0]), 0.02);
        Assert.Equal(r.GetProperty("with_loop").GetDouble(), Share(d => d.HasLoop), 0.05);
        Assert.Equal(r.GetProperty("rolls_mean").GetDouble(), Mean(d => d.Deck.Rolls), 1.0);
        foreach (var kind in new[] { "compartment", "passage", "junction" })
        {
            Assert.Equal(r.GetProperty("tiles_mean").GetProperty(kind).GetDouble(), Mean(d => Tiles(d, kind)), 0.25);
            Assert.InRange(decks.Max(d => Tiles(d, kind)), 0, r.GetProperty("tiles_max").GetProperty(kind).GetInt32() + 1);
        }
        Assert.InRange(decks.Min(d => d.Compartments), r.GetProperty("compartments_min").GetInt32() - 1, v.Compartments[1]);
        Assert.InRange(decks.Max(d => d.Compartments), v.Compartments[0], v.Compartments[1]);
    }

    [Theory]
    [MemberData(nameof(VesselIds))]
    public void EveryDeckIsConnectedAndWithinTheTileInventory(string vessel)
    {
        var owned = Set.Layouts.Tiles.ToDictionary(t => t.Id, t => t.Owned);
        foreach (var d in Decks(vessel))
        {
            Assert.True(d.Connected, $"seed {d.Deck.Seed} disconnected");
            foreach (var (kind, count) in d.Deck.TileCounts) Assert.True(count <= owned[kind], $"seed {d.Deck.Seed}: {count} {kind}");
        }
    }

    [Theory]
    [MemberData(nameof(VesselIds))]
    public void SameSeedSameDeck(string vessel)
    {
        string Json(int seed) => JsonSerializer.Serialize(DeckGenerator.Generate(Set, vessel, seed).Deck);
        Assert.Equal(Json(7), Json(7));
        Assert.True(Enumerable.Range(1, 20).Select(Json).Distinct().Count() > 10, "different seeds give different decks");
    }

    [Theory]
    [MemberData(nameof(VesselIds))]
    public void DecksFollowTheProcedure(string vessel)
    {
        var v = Set.Vessel(vessel);
        var securityIds = Rules.Security.Levels.Select(l => l.Id).ToHashSet();
        foreach (var g in Decks(vessel, 300))
        {
            var d = g.Deck;
            var at = $"seed {d.Seed}";
            Assert.Equal($"deck-{vessel}-{d.Seed}", d.Id);
            Assert.Equal(Rules.Version, d.ContentVersion);
            Assert.Equal((v.Board[0], v.Board[1]), (d.Board.Width, d.Board.Height));

            // Tiles sit on the board, on the 3" grid, without overlapping; ids are unique.
            Assert.Equal(d.Tiles.Count, d.Tiles.Select(t => t.Id).Distinct().Count());
            var cells = new HashSet<(double, double)>();
            foreach (var t in d.Tiles)
            {
                Assert.True(t.X >= 0 && t.Y >= 0 && t.X + t.W <= d.Board.Width && t.Y + t.H <= d.Board.Height, $"{at}: {t.Id} off the board");
                for (double x = t.X; x < t.X + t.W; x += 3)
                    for (double y = t.Y; y < t.Y + t.H; y += 3)
                        Assert.True(cells.Add((x, y)), $"{at}: {t.Id} overlaps at {x},{y}");
            }
            Assert.Equal("Docking passage", d.Tiles[0].Name);
            Assert.Equal((0.0, d.Tiles[0].Y + 1.5), (d.Entry.X, d.Entry.Y));

            // The heart is the vessel's heart room, behind locked doors at the heart's security level.
            // (Heart rooms are not on any vessel's room table, so the heart room appears once at most.)
            var heartTile = d.Tiles.SingleOrDefault(t => t.Room == v.Heart);
            Assert.Equal(heartTile is null, d.Objective is null);
            if (d.Objective is { } o)
            {
                Assert.Equal(Set.Room(v.Heart).Name, o.Label);
                Assert.True(o.X > heartTile!.X && o.X < heartTile.X + heartTile.W && o.Y > heartTile.Y && o.Y < heartTile.Y + heartTile.H);
            }

            // Doors: numbered in order, at a known state; locked doors carry a security level.
            Assert.Equal(Enumerable.Range(1, d.Doors.Count).Select(i => $"D{i}"), d.Doors.Select(x => x.Id));
            foreach (var door in d.Doors)
            {
                Assert.Contains(door.State, new[] { "open", "closed", "locked" });
                Assert.Contains(door.Orientation, new[] { "h", "v" });
                Assert.Equal(door.State == "locked", door.Security is not null);
                if (door.Security is not null) Assert.Contains(door.Security, securityIds);
                var ends = door.Note.Split(" to ");
                Assert.All(ends, id => Assert.Contains(d.Tiles, t => t.Id == id));
                if (heartTile is not null && ends.Contains(heartTile.Id))
                    Assert.Equal(("locked", v.HeartSecurity), (door.State, door.Security));
            }

            // Cover lies inside its room, and every compartment and small room is a room from the content.
            foreach (var c in d.Cover)
            {
                var room = d.Tiles.Single(t => t.Id == c.Room);
                Assert.True(c.X >= room.X && c.Y >= room.Y && c.X + c.W <= room.X + room.W + 1e-9 && c.Y + c.H <= room.Y + room.H + 1e-9,
                    $"{at}: {c.Id} outside {room.Id}");
                Assert.Contains(c.Type, new[] { "light", "heavy" });
            }
            foreach (var t in d.Tiles.Where(t => t.Kind is "compartment" or "small"))
                Assert.Equal(t.Kind, Set.Room(t.Room!).Size);

            // Hazard is none or one of the vessel's; rolls counted.
            Assert.Contains(d.Hazard, v.Hazards.Select(h => Set.Hazard(h).Text).Append("None"));
            Assert.InRange(d.Rolls, 5, 60);
        }
    }

    // The deck graph (task 11 routes over it): links, spine, heart and hazard id.
    [Theory]
    [MemberData(nameof(VesselIds))]
    public void DeckGraphMatchesTheLayout(string vessel)
    {
        var v = Set.Vessel(vessel);
        foreach (var g in Decks(vessel, 300))
        {
            var d = g.Deck;
            var at = $"seed {d.Seed}";
            var ids = d.Tiles.Select(t => t.Id).ToHashSet();
            Assert.All(d.Links, l => Assert.True(ids.Contains(l.A) && ids.Contains(l.B), $"{at}: link {l.A}-{l.B}"));

            // Every door is one link, joining the tiles its note names; the other links are spine openings.
            Assert.Equal(d.Doors.Select(x => x.Id).Order(), d.Links.Where(l => l.Door is not null).Select(l => l.Door!).Order());
            foreach (var l in d.Links.Where(l => l.Door is not null))
                Assert.Equal($"{l.A} to {l.B}", d.Doors.Single(x => x.Id == l.Door).Note);
            foreach (var l in d.Links.Where(l => l.Door is null))
                Assert.True(d.Spine.Contains(l.A) && d.Spine.Contains(l.B), $"{at}: opening {l.A}-{l.B} off the spine");

            // The graph is connected, has a loop exactly when the generator says so, and the spine is a chain from the airlock.
            var route = d.RouteFrom(d.Spine[0]);
            Assert.True(route.Values.All(x => x < 99), $"{at}: unreachable tile");
            Assert.Equal(g.HasLoop, d.Links.Count - d.Tiles.Count + 1 > 0);
            Assert.Equal("Docking passage", d.Tiles.Single(t => t.Id == d.Spine[0]).Name);
            for (int k = 1; k < d.Spine.Count; k++)
            {
                Assert.Contains(d.Links, l => (l.A, l.B) == (d.Spine[k - 1], d.Spine[k]) || (l.A, l.B) == (d.Spine[k], d.Spine[k - 1]));
                Assert.InRange(route[d.Spine[k]], 1, k);
            }

            // Heart and hazard.
            Assert.Equal(d.Tiles.SingleOrDefault(t => t.Room == v.Heart)?.Id, d.Heart);
            Assert.Equal(d.HazardId is null ? "None" : Set.Hazard(d.HazardId).Text, d.Hazard);
            if (d.HazardId is not null) Assert.Contains(d.HazardId, v.Hazards);
        }
    }

    [Fact]
    public void RouteFromCountsTileSteps()
    {
        var d = DeckGenerator.Generate(Set, "freighter", 7).Deck;
        var route = d.RouteFrom("P1");
        Assert.Equal(0, route["P1"]);
        foreach (var l in d.Links)
            Assert.InRange(Math.Abs(route[l.A] - route[l.B]), 0, 1);
        // Every tile but the start has a neighbour one step nearer.
        foreach (var t in d.Tiles.Where(t => t.Id != "P1"))
            Assert.Contains(d.Links, l => (l.A == t.Id && route[l.B] == route[t.Id] - 1) || (l.B == t.Id && route[l.A] == route[t.Id] - 1));
    }

    [Fact]
    public void UniqueRoomsAppearAtMostOnce()
    {
        var unique = Set.Layouts.Rooms.Where(r => r.Unique).Select(r => r.Id).ToHashSet();
        foreach (var vessel in Set.Layouts.Vessels)
            foreach (var g in Decks(vessel.Id, 500))
                foreach (var dup in g.Deck.Tiles.Where(t => t.Room is not null && unique.Contains(t.Room)).GroupBy(t => t.Room).Where(x => x.Count() > 1))
                    Assert.Fail($"{vessel.Id} seed {g.Deck.Seed}: {dup.Key} appears {dup.Count()} times");
    }

    [Fact]
    public void UnknownVesselFailsLoudly() =>
        Assert.Contains("'nope'", Assert.Throws<KeyNotFoundException>(() => DeckGenerator.Generate(Set, "nope", 1)).Message);
}
