using System.Text.Json;
using Imperator.Core.Land;
using static Imperator.Core.Tests.TestContent;

namespace Imperator.Core.Tests;

public class LandTableGeneratorTests
{
    private const int Seeds = 1000;

    public static TheoryData<string, string> EnvironmentsAndSizes()
    {
        var data = new TheoryData<string, string>();
        foreach (var e in Set.Terrain.Environments)
            foreach (var size in new[] { "small", "large" }) data.Add(e.Id, size);
        return data;
    }

    private static IEnumerable<LandTable> Tables(string? env, string size, int n = Seeds) =>
        Enumerable.Range(1, n).Select(s => LandTableGenerator.Generate(Set, env, size, s));

    private static (int C, int R) Cell(string sector) => ("ABCDE".IndexOf(sector[0]), int.Parse(sector[1..]) - 1);

    [Theory]
    [MemberData(nameof(EnvironmentsAndSizes))]
    public void NoLineOfThreeEmptySectorsRemains(string env, string size)
    {
        foreach (var t in Tables(env, size))
        {
            int n = t.Board.Width / 12;
            var empty = t.Sectors.Select(s => s.Name).Where(name => t.Cover.All(c => c.Sector != name)).Select(Cell).ToHashSet();
            for (int a = 0; a < n; a++)
                for (int k = 0; k + 2 < n; k++)
                {
                    Assert.False(empty.Contains((k, a)) && empty.Contains((k + 1, a)) && empty.Contains((k + 2, a)), $"seed {t.Seed}: row {a + 1}");
                    Assert.False(empty.Contains((a, k)) && empty.Contains((a, k + 1)) && empty.Contains((a, k + 2)), $"seed {t.Seed}: column {"ABCDE"[a]}");
                }
        }
    }

    [Theory]
    [MemberData(nameof(EnvironmentsAndSizes))]
    public void EveryPieceLiesInsideItsSector(string env, string size)
    {
        const double e = 1e-9;
        foreach (var t in Tables(env, size))
            foreach (var c in t.Cover)
            {
                var (col, row) = Cell(c.Sector);
                Assert.True(c.X >= col * 12 - e && c.Y >= row * 12 - e && c.X + c.W <= col * 12 + 12 + e && c.Y + c.H <= row * 12 + 12 + e,
                    $"seed {t.Seed}: {c.Id} {c.Label} at {c.X},{c.Y} {c.W}x{c.H} outside {c.Sector}");
            }
    }

    [Theory]
    [MemberData(nameof(EnvironmentsAndSizes))]
    public void TablesFollowTheProcedure(string env, string size)
    {
        var e = Set.Environment(env);
        int side = size == "small" ? 36 : 48;
        var draw = LandTableGenerator.DrawClass;
        foreach (var t in Tables(env, size, 200))
        {
            Assert.Equal((side, side), (t.Board.Width, t.Board.Height));
            Assert.Equal((env, size, Rules.Version), (t.Environment, t.Size, t.ContentVersion));
            var names = Enumerable.Range(0, side / 12).SelectMany(r => Enumerable.Range(0, side / 12).Select(c => $"{"ABCDE"[c]}{r + 1}"));
            Assert.Equal(names, t.Sectors.Select(s => s.Name));
            Assert.Equal(Enumerable.Range(1, t.Cover.Count).Select(i => $"T{i}"), t.Cover.Select(c => c.Id));
            Assert.Contains(t.Conditions, Set.Terrain.Conditions.Select(x => x.Id));

            // Each sector holds its roll's pieces (or the fill's, after the density check), drawn by terrain type.
            foreach (var s in t.Sectors)
            {
                var pieces = s.Text.EndsWith(" (density check)") ? e.FillPieces! : e.Table.Single(r => r.Text == s.Text).Pieces!;
                var placed = t.Cover.Where(c => c.Sector == s.Name).ToList();
                Assert.Equal(pieces.Select(p => (p.Type, p.Label)), placed.Select(c => (c.Terrain, c.Label)));
                for (int i = 0; i < pieces.Count; i++)
                    Assert.True((placed[i].W, placed[i].H) == (pieces[i].W, pieces[i].H) || (placed[i].W, placed[i].H) == (pieces[i].H, pieces[i].W));
            }
            foreach (var c in t.Cover)
            {
                Assert.Equal(draw[c.Terrain], c.Type);
                Assert.StartsWith($"{Set.TerrainType(c.Terrain).Name}. Sector {c.Sector}: ", c.Piece);
            }

            // One d6 per sector and one for conditions, logged in order.
            Assert.Equal(t.Sectors.Count + 1, t.Rolls.Count(r => !r.StartsWith("Density check")));
            Assert.StartsWith("Sector A1: ", t.Rolls[0]);
            Assert.StartsWith("Conditions: ", t.Rolls[^1]);
        }
    }

    [Fact]
    public void ARolledEnvironmentIsNeverFortified()
    {
        var seen = Tables(null, "small", 500).Select(t => t.Environment).ToHashSet();
        Assert.DoesNotContain("fortified", seen);
        Assert.Equal(Set.Terrain.EnvironmentRoll.Select(r => r.Environment).ToHashSet(), seen);
        Assert.StartsWith("Environment: ", LandTableGenerator.Generate(Set, null, "small", 1).Rolls[0]);
        Assert.Equal("fortified", LandTableGenerator.Generate(Set, "fortified", "small", 1).Environment);
    }

    [Fact]
    public void ThePlayerChoosesWhichWayAPieceFaces()
    {
        // A non-square piece is laid both ways across seeds; square pieces never turn.
        var containers = Tables("industrial", "small", 300).SelectMany(t => t.Cover).Where(c => c.Label == "Container").Select(c => (c.W, c.H)).ToHashSet();
        Assert.Equal(new HashSet<(double, double)> { (6, 2.5), (2.5, 6) }, containers);
    }

    [Fact]
    public void TheDensityCheckFillsEmptyLines()
    {
        // Woodland and desert have open-ground rolls; some tables need the fill.
        var fills = Tables("desert", "large", 300).Count(t => t.Rolls.Any(r => r.StartsWith("Density check")));
        Assert.True(fills > 0);
    }

    [Fact]
    public void SameSeedSameTable()
    {
        string Json(int seed) => JsonSerializer.Serialize(LandTableGenerator.Generate(Set, "urban", "small", seed));
        Assert.Equal(Json(11), Json(11));
        Assert.True(Enumerable.Range(1, 20).Select(Json).Distinct().Count() > 10);
    }

    [Fact]
    public void UnknownSizeOrEnvironmentFailsLoudly()
    {
        Assert.Throws<ArgumentException>(() => LandTableGenerator.Generate(Set, "urban", "huge", 1));
        Assert.Contains("'moon'", Assert.Throws<KeyNotFoundException>(() => LandTableGenerator.Generate(Set, "moon", "small", 1)).Message);
    }
}
