using Imperator.Core.Layouts;

namespace Imperator.Core.Land;

// A land table (rules chapter 12, "Setting up a land table"), in the scenario format of docs/scenarios/missions/,
// so the map and printed sheets read the same shape. Distances are inches from the south-west corner (the player's
// bottom left): x grows east, y grows north toward the enemy.

/// <summary>A 12" sector, named A1 (the player's bottom left) onward, with what its roll put there.</summary>
public sealed record LandSector(string Name, string Text);

/// <summary>
/// A terrain piece. Type is how it is drawn (light, heavy, hill, area, ditch, water, wire); Terrain is the chapter 12
/// terrain type id; Piece describes it for the terrain table.
/// </summary>
public sealed record LandCover(string Id, double X, double Y, double W, double H, string Type, string Label, string Terrain, string Piece, string Sector);

/// <summary>A generated land table. Seed and ContentVersion reproduce it exactly.</summary>
/// <param name="Rolls">The roll log, in order, as the reference writes it.</param>
public sealed record LandTable(
    string Id, string Title, string RulesVersion, string ContentVersion, string Kind,
    string Environment, string Size, int Seed, DeckBoard Board,
    List<LandSector> Sectors, List<LandCover> Cover, string Conditions, List<string> Rolls);

/// <summary>
/// The chapter 12 land table procedure, a port of Mission.build_land in tools/generate_mission.py: environment
/// (named, or rolled; the roll table has no Fortified, so it appears only when named), board by size, one d6 per
/// 12" sector, the density check, piece placement, and the conditions roll.
/// </summary>
public static class LandTableGenerator
{
    /// <summary>Board side in inches by mission size: small 36" × 36", large 48" × 48".</summary>
    public static readonly IReadOnlyDictionary<string, int> BoardSides = new Dictionary<string, int> { ["small"] = 36, ["large"] = 48 };

    /// <summary>
    /// How each chapter 12 terrain type is drawn, as in tools/generate_mission.py (DRAW). Presentation, kept with the
    /// generator so its output matches the reference's scenario format.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> DrawClass = new Dictionary<string, string>
    {
        ["scrub"] = "area", ["woods"] = "area", ["rubble"] = "area", ["low_wall"] = "light", ["hedge"] = "light", ["wire"] = "wire",
        ["solid"] = "heavy", ["building"] = "heavy", ["bunker"] = "heavy", ["crater"] = "ditch", ["hill"] = "hill", ["water"] = "water",
    };

    public static LandTable Generate(ContentSet content, string? environment, string size, int seed) =>
        Generate(content, environment, size, seed, new Rng(seed), []);

    /// <summary>Builds a table on a shared random stream and roll log (the mission generator rolls on the same stream).</summary>
    public static LandTable Generate(ContentSet content, string? environment, string size, int seed, Rng rng, List<string> log)
    {
        if (!BoardSides.TryGetValue(size, out int side)) throw new ArgumentException($"Unknown size '{size}'; sizes: small, large", nameof(size));
        int D6(string what)
        {
            int r = rng.D(6);
            log.Add($"{what}: {r}");
            return r;
        }
        // Roll once, then read the table row for that face.
        static T Row<T>(IEnumerable<T> table, Func<T, D6Range> roll, int face) => table.Single(r => roll(r).Contains(face));

        var envId = environment ?? Row(content.Terrain.EnvironmentRoll, r => r.Roll, D6("Environment")).Environment;
        var env = content.Environment(envId);
        int cols = side / 12, rows = side / 12;

        // One d6 per sector, row by row from the player's edge.
        var sectors = new Dictionary<(int C, int R), (string Name, string Text, List<TerrainPieceDef> Pieces)>();
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                var name = $"{"ABCDE"[c]}{r + 1}";
                var row = Row(env.Table, x => x.Roll, D6($"Sector {name}"));
                sectors[(c, r)] = (name, row.Text, row.Pieces ?? []);
            }

        // Density check: three sectors in a line with no cover -> the middle one becomes the fill; repeat until none.
        var lines = Enumerable.Range(0, rows).Select(r => Enumerable.Range(0, cols).Select(c => (c, r)).ToList())
            .Concat(Enumerable.Range(0, cols).Select(c => Enumerable.Range(0, rows).Select(r => (c, r)).ToList()))
            .ToList();
        bool changed = true;
        while (changed)
        {
            changed = false;
            foreach (var line in lines)
                for (int k = 0; k + 2 < line.Count; k++)
                {
                    var run = line.GetRange(k, 3);
                    if (run.All(s => sectors[s].Pieces.Count == 0))
                    {
                        var mid = sectors[run[1]];
                        sectors[run[1]] = (mid.Name, $"{env.Fill} (density check)", env.FillPieces ?? []);
                        log.Add($"Density check: sector {mid.Name} becomes the fill");
                        changed = true;
                    }
                }
        }

        // Pieces near each sector's middle: the first centred; rocks on a hill on it; others offset clear of earlier pieces.
        (double, double)[] offsets = [(0, 4.5), (0, -4.5), (4.5, 0), (-4.5, 0), (4, 4), (-4, -4), (4, -4), (-4, 4)];
        (double, double)[] onHill = [(-2, 0), (2, 0), (0, 1.5)];
        var cover = new List<LandCover>();
        foreach (var ((c, r), s) in sectors.OrderBy(kv => kv.Key.R).ThenBy(kv => kv.Key.C))
        {
            double cx = c * 12 + 6, cy = r * 12 + 6;
            var placed = new List<(double X, double Y, double W, double H)>();
            for (int k = 0; k < s.Pieces.Count; k++)
            {
                var pc = s.Pieces[k];
                double w = pc.W, h = pc.H;
                if (w != h && rng.NextDouble() < 0.5) (w, h) = (h, w); // which way it faces: the player's choice in the rules
                double x, y;
                if (k == 0) (x, y) = (cx - w / 2, cy - h / 2);
                else if (pc.Type == "solid" && s.Pieces[0].Type == "hill")
                {
                    var (ox, oy) = onHill[k - 1];
                    (x, y) = (cx + ox - w / 2, cy + oy - h / 2);
                }
                else
                {
                    (x, y) = (cx - w / 2, cy - h / 2);
                    foreach (var (ox, oy) in offsets)
                    {
                        // min(max(…)) as in the reference: a piece wider than the sector's margin sits at the upper bound.
                        double tx = Math.Min(Math.Max(cx + ox - w / 2, c * 12 + 0.3), c * 12 + 11.7 - w);
                        double ty = Math.Min(Math.Max(cy + oy - h / 2, r * 12 + 0.3), r * 12 + 11.7 - h);
                        if (!placed.Any(p => tx < p.X + p.W && p.X < tx + w && ty < p.Y + p.H && p.Y < ty + h))
                        {
                            (x, y) = (tx, ty);
                            break;
                        }
                    }
                }
                placed.Add((x, y, w, h));
                cover.Add(new LandCover($"T{cover.Count + 1}", Math.Round(x, 2), Math.Round(y, 2), w, h, DrawClass[pc.Type], pc.Label, pc.Type,
                    $"{content.TerrainType(pc.Type).Name}. Sector {s.Name}: {s.Text}", s.Name));
            }
        }

        var conditions = Row(content.Terrain.Conditions, x => x.Roll, D6("Conditions"));
        var version = content.Core.Version;
        return new LandTable(
            Id: $"land-{env.Id}-{size}-{seed}",
            Title: $"{env.Name}, table {seed}",
            RulesVersion: version[..version.LastIndexOf('.')],
            ContentVersion: version,
            Kind: "land",
            Environment: env.Id,
            Size: size,
            Seed: seed,
            Board: new DeckBoard(side, side),
            Sectors: sectors.OrderBy(kv => kv.Key.R).ThenBy(kv => kv.Key.C).Select(kv => new LandSector(kv.Value.Name, kv.Value.Text)).ToList(),
            Cover: cover,
            Conditions: conditions.Id,
            Rolls: log);
    }
}
