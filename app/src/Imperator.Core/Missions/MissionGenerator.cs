using Imperator.Core.Land;
using Imperator.Core.Layouts;

namespace Imperator.Core.Missions;

/// <summary>What the player names; everything left null is rolled.</summary>
public sealed record MissionRequest(string Setting, int Seed, string? Mission = null, int? Tier = null, string? Vessel = null,
    string? Environment = null, string Size = "small", string? Opposition = null);

/// <summary>
/// The chapter 13 procedure, "Building a mission": a port of tools/generate_mission.py, the reference. In the paper
/// order: mission type and threat tier (named or rolled), the board (a chapter 11 deck or a chapter 12 table),
/// complication, objectives, guard, contact chips, enemy entry, clock and briefing.
/// </summary>
/// <remarks>
/// The mission rolls on its own stream, seeded from "mission-{setting}-{seed}" as the reference does. A ship mission's
/// deck is the chapter 11 deck for the same seed (GET /decks/{vessel}?seed=N); a land table is rolled on the mission's
/// stream. Choices the paper procedure leaves to the player (ties between equally far rooms, which way a piece faces)
/// are settled by the seed.
/// </remarks>
public static class MissionGenerator
{
    public static readonly string[] Settings = ["ship", "land"];

    /// <summary>
    /// Where models cannot stand: guards, contact chips and objectives keep clear of these drawn terrain types
    /// (NO_STAND in the reference). Aboard ship, all room cover blocks.
    /// </summary>
    private static readonly HashSet<string> NoStand = ["heavy", "light", "wire", "water"];

    public static Mission Generate(ContentSet content, MissionRequest request) => new Builder(content, request).Build();

    private sealed class Builder
    {
        private readonly ContentSet _c;
        private readonly MissionRequest _want;
        private readonly MissionsContent _m;
        private readonly Rng _rng;
        private readonly List<string> _log = [];
        private readonly ClockSizeDef _size;

        private MissionDef _mission = null!;
        private TierDef _tier = null!;
        private ComplicationDef _complication = null!;
        private ElementDef _guard = null!;
        private string? _hvt;
        private string _ground = "";
        private readonly List<MissionObjective> _objectives = [];
        private readonly List<MissionEnemy> _enemies = [];
        private readonly List<MissionContact> _contacts = [];
        private MissionEntry _enemyEntry = null!;
        private List<string> _secondExits = [];
        private List<(Box Box, string Where)> _places = [];
        private List<Box> _blocked = [];
        private Box _guardBox;

        // Ship
        private Deck? _deck;
        private VesselDef? _vessel;
        private string? _hazardId;
        // Land
        private LandTable? _land;
        private ConditionDef? _conditions;

        private readonly record struct Box(double X, double Y, double W, double H)
        {
            public (double X, double Y) Centre => (X + W / 2, Y + H / 2);
        }

        public Builder(ContentSet content, MissionRequest want)
        {
            if (!Settings.Contains(want.Setting)) throw new ArgumentException($"Unknown setting '{want.Setting}'; settings: ship, land", nameof(want));
            if (want.Setting == "ship" && want.Size != "small")
                throw new ArgumentException("Decks are small missions (chapter 11 boards hold up to 8 a side)", nameof(want));
            _c = content;
            _want = want;
            _m = content.Missions;
            _size = content.ClockSize(want.Size);
            if (want.Opposition is not null) _ = content.Missions.Oppositions?.Single(o => o.Id == want.Opposition)
                ?? throw new ArgumentException($"Unknown opposition '{want.Opposition}'", nameof(want));
            _rng = Rng.FromKey($"mission-{want.Setting}-{want.Seed}");
        }

        private int D6(string what)
        {
            int r = _rng.D(6);
            _log.Add($"{what}: {r}");
            return r;
        }

        private static T Row<T>(IEnumerable<T> table, Func<T, D6Range> roll, int face) => table.Single(r => roll(r).Contains(face));

        private bool ComplicationIs(string id) => _complication?.Id == id;

        public Mission Build()
        {
            RollTypeAndTier();
            if (_want.Setting == "ship") BuildShip();
            else BuildLand();
            _complication = Row(_m.Complications, r => r.Roll, D6("Complication"));
            if (_want.Setting == "ship")
            {
                var first = _vessel!.Hazards[0];
                if (ComplicationIs("hazard") && _hazardId != first)
                {
                    _hazardId = first;
                    _log.Add("Complication: the vessel's first hazard replaces the deck's hazard roll");
                }
                _ground = $"{_vessel.Name}, deck {_want.Seed}. Hazard: {HazardText}";
            }
            else
            {
                if (ComplicationIs("hazard"))
                {
                    _conditions = _c.Terrain.Conditions.Single(x => x.Id == "night");
                    _log.Add("Complication: Night replaces the conditions roll");
                }
                int w = _land!.Board.Width, h = _land.Board.Height;
                _ground = $"{_c.Environment(_land.Environment).Name}, {w}\" × {h}\" table ({w / 12} × {h / 12} sectors). " +
                          $"Conditions: {_conditions!.Name}: {_conditions.Text}";
            }
            PlaceForces();
            return ToMission();
        }

        private string HazardText => _hazardId is null ? "None" : _c.Hazard(_hazardId).Text;

        private void RollTypeAndTier()
        {
            if (_want.Mission is { } mid)
            {
                _mission = _c.Mission(mid);
                _log.Add($"Mission type: named, {_mission.Name}");
            }
            else _mission = _c.Mission(Row(_m.MissionRoll, r => r.Roll, D6("Mission type")).Mission);
            if (_want.Tier is { } tier)
            {
                _tier = _c.Tier(tier, _want.Opposition);
                _log.Add($"Threat tier: named, {_tier.Tier}");
            }
            else _tier = _c.Tier(Row(_m.TierRoll, r => r.Roll, D6("Threat tier")).Tier, _want.Opposition);
        }

        // ------------------------------------------------------------ geometry

        private static double Dist((double X, double Y) a, (double X, double Y) b) => Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

        /// <summary>
        /// The free point in box nearest to pref inches from target, on a 0.5" grid, at least 0.5" clear of blocked
        /// pieces and spacing from taken points (free_point in the reference).
        /// </summary>
        private (double X, double Y)? FreePoint(Box box, (double X, double Y) target, List<(double X, double Y)> taken,
            double pref = 0, double? rmax = null, double spacing = 1.3)
        {
            (double X, double Y)? best = null;
            (double D, double Y, double X) bestKey = default;
            int stepsX = (int)((box.W - 1.2) / 0.5) + 1, stepsY = (int)((box.H - 1.2) / 0.5) + 1;
            for (int i = 0; i < Math.Max(stepsX, 1); i++)
                for (int j = 0; j < Math.Max(stepsY, 1); j++)
                {
                    double px = box.X + 0.6 + i * 0.5, py = box.Y + 0.6 + j * 0.5;
                    if (_blocked.Any(b => b.X - 0.5 <= px && px <= b.X + b.W + 0.5 && b.Y - 0.5 <= py && py <= b.Y + b.H + 0.5)) continue;
                    if (taken.Any(q => Dist((px, py), q) < spacing)) continue;
                    double dist = Dist((px, py), target);
                    if (rmax is not null && dist > rmax) continue;
                    var key = (Math.Abs(dist - pref), py, px);
                    if (best is null || key.CompareTo(bestKey) < 0)
                    {
                        best = (Math.Round(px, 2), Math.Round(py, 2));
                        bestKey = key;
                    }
                }
            return best;
        }

        // ------------------------------------------------------------ ship

        private void BuildShip()
        {
            var vid = _want.Vessel ?? Row(_c.Layouts.VesselRoll, r => r.Roll, D6("Vessel")).Vessel;
            _vessel = _c.Vessel(vid);
            var d = DeckGenerator.Generate(_c, vid, _want.Seed).Deck;
            _deck = d;
            _hazardId = d.HazardId;
            _log.Add($"Deck: {_vessel.Name}, seed {_want.Seed} ({d.Rolls} rolls, chapter 11)");

            var tiles = d.Tiles;
            var index = tiles.Select((t, i) => (t.Id, i)).ToDictionary(x => x.Id, x => x.i);
            var boxes = tiles.Select(t => new Box(t.X, t.Y, t.W, t.H)).ToList();
            var hops = d.RouteFrom(tiles[0].Id);
            var entry = (d.Entry.X, d.Entry.Y);
            // Farthest from the airlock: by route (tile steps), then straight-line distance, then tile order.
            (int, double, int) Far(int i) => (hops[tiles[i].Id], Dist(boxes[i].Centre, entry), i);

            var comps = Enumerable.Range(0, tiles.Count).Where(i => tiles[i].Kind == "compartment").ToList();
            int heart = d.Heart is { } h ? index[h] : comps.MaxBy(Far);
            List<int> objs;
            switch (_mission.Id)
            {
                case "seize" or "capture":
                    objs = [heart];
                    break;
                case "recover":
                    var others = comps.Where(i => i != heart).ToList();
                    objs = [others.Count > 0 ? others.MaxBy(Far) : heart];
                    break;
                default: // sabotage: the heart and the compartment farthest from it by route
                    var fromHeart = d.RouteFrom(tiles[heart].Id);
                    var rest = comps.Where(i => i != heart).ToList();
                    if (rest.Count == 0) rest = Enumerable.Range(0, tiles.Count).Where(i => tiles[i].Kind is "junction" or "small" && i != heart).ToList();
                    objs = [heart, rest.MaxBy(i => (fromHeart[tiles[i].Id], i))];
                    break;
            }

            _blocked = d.Cover.Select(c => new Box(c.X, c.Y, c.W, c.H)).ToList();
            for (int n = 1; n <= objs.Count; n++)
            {
                int i = objs[n - 1];
                var pt = d.Heart is not null && i == index[d.Heart] && d.Objective is { } o
                    ? (o.X, o.Y)
                    : FreePoint(boxes[i], boxes[i].Centre, []) ?? throw new InvalidOperationException($"No room for the objective in {tiles[i].Id}");
                _objectives.Add(new MissionObjective(n, pt.Item1, pt.Item2, tiles[i].Id, $"{tiles[i].Name} ({tiles[i].Id})",
                    $"the {tiles[i].Name.ToLowerInvariant()} ({tiles[i].Id})"));
            }
            _guardBox = boxes[objs[0]];

            int spineFar = d.Spine.Select(id => index[id]).MaxBy(Far);
            var (ex, ey) = boxes[spineFar].Centre;
            _enemyEntry = new MissionEntry(ex, ey, tiles[spineFar].Id, $"Enemy entry and exit ({tiles[spineFar].Id})");
            _secondExits = tiles.Where(t => t.Room is "secondary_airlock" or "escape_pod").Select(t => $"{t.Name} ({t.Id})").ToList();

            // Contact places: compartments that are not objectives, farthest first; then junctions and small rooms.
            var nearSpine = d.Spine.Take(2).Select(id => index[id]).ToHashSet();
            var first = comps.Where(i => !objs.Contains(i)).OrderByDescending(Far).ToList();
            var second = Enumerable.Range(0, tiles.Count)
                .Where(i => tiles[i].Kind is "junction" or "small" && !objs.Contains(i) && !nearSpine.Contains(i))
                .OrderByDescending(Far).ToList();
            _places = first.Concat(second).Select(i => (boxes[i], tiles[i].Id)).ToList();
        }

        // ------------------------------------------------------------ land

        private void BuildLand()
        {
            var t = LandTableGenerator.Generate(_c, _want.Environment, _want.Size, _want.Seed, _rng, _log);
            _land = t;
            _conditions = _c.Terrain.Conditions.Single(x => x.Id == t.Conditions);
            int W = t.Board.Width, H = t.Board.Height, cols = W / 12, rows = H / 12, far = rows - 1;
            _blocked = t.Cover.Where(c => NoStand.Contains(c.Type)).Select(c => new Box(c.X, c.Y, c.W, c.H)).ToList();
            string Name(int c, int r) => $"{"ABCDE"[c]}{r + 1}";

            List<int> objCols;
            switch (_mission.Id)
            {
                case "seize" or "capture":
                    int middle = cols % 2 == 1 ? cols / 2 : cols / 2 - (D6("Middle sector (even row): 1–3 left, 4–6 right") <= 3 ? 1 : 0);
                    objCols = [middle];
                    break;
                case "recover":
                    int r = D6("Hostage sector");
                    objCols = [r <= 2 ? 0 : r <= 4 ? cols / 2 : cols - 1];
                    break;
                default:
                    objCols = [0, cols - 1];
                    break;
            }
            for (int n = 1; n <= objCols.Count; n++)
            {
                int c = objCols[n - 1];
                var pt = FreePoint(new Box(c * 12, far * 12, 12, 12), (c * 12 + 6, far * 12 + 6), [])
                         ?? throw new InvalidOperationException($"No room for the objective in sector {Name(c, far)}");
                _objectives.Add(new MissionObjective(n, pt.X, pt.Y, Name(c, far), $"Sector {Name(c, far)}", $"sector {Name(c, far)}"));
            }
            var objSectors = objCols.Select(c => (c, far)).ToHashSet();
            var o1 = _objectives[0];
            _guardBox = new Box(o1.X - 3.5, o1.Y - 3.5, 7, 7);
            _enemyEntry = new MissionEntry(W / 2.0, H - 0.4, "edge", "Enemy entry and exit (middle of the far edge)");

            // Contact places: sectors of the far two rows (not the objective's, never within 12" of the player edge),
            // farthest from the middle of the player's edge first.
            var cand = new[] { rows - 1, rows - 2 }
                .SelectMany(r => Enumerable.Range(0, cols).Select(c => (c, r)))
                .Where(s => !objSectors.Contains(s) && s.r * 12 >= 12)
                .OrderBy(s => -Dist((s.c * 12 + 6, s.r * 12 + 6), (W / 2.0, 0))).ThenBy(s => s.c)
                .ToList();
            _places = cand.Select(s => (new Box(s.c * 12, s.r * 12, 12, 12), Name(s.c, s.r))).ToList();
        }

        // ------------------------------------------------------------ forces

        private void PlaceForces()
        {
            // The guard: one roll on the tier table, rerolling false alarms and booby traps.
            var special = _m.ContactResults.Select(x => x.Id).ToHashSet();
            string result;
            do result = Row(_tier.Table, r => r.Roll, D6("Guard")).Result;
            while (special.Contains(result));
            _guard = _c.Element(result);

            var o = _objectives[0];
            var taken = _objectives.Select(x => (x.X, x.Y)).ToList();
            var models = _guard.Composition.SelectMany(p => Enumerable.Repeat(p.Troop, p.Count)).ToList();
            if (_mission.Id == "capture")
            {
                _hvt = _tier.Hvt;
                if (!models.Contains(_hvt)) models.Add(_hvt);
            }
            for (int n = 1; n <= models.Count; n++)
            {
                var pt = FreePoint(_guardBox, (o.X, o.Y), taken, pref: 2.0, rmax: 3.0)
                         ?? FreePoint(_guardBox, (o.X, o.Y), taken, pref: 2.0, spacing: 1.0)
                         ?? throw new InvalidOperationException($"No room for guard model {n} near the objective");
                taken.Add(pt);
                var state = (_guard.Behavior == "automated" ? "Alerted" : "Unaware") + (models[n - 1] == _hvt ? " (the HVT)" : "");
                _enemies.Add(new MissionEnemy(n, models[n - 1], _guard.Id, pt.X, pt.Y, state));
            }

            // Contacts: the tier's number (one more if Reinforced), one per place, farthest first, round again if needed.
            int k = _tier.Contacts + (ComplicationIs("extra_contact") ? 1 : 0);
            var chipTaken = new List<(double X, double Y)>(taken);
            for (int i = 0; i < k; i++)
            {
                var (box, where) = _places[i % _places.Count];
                // A second chip in a small room may not fit 2" from the first: then 1", as for the guard.
                var pt = FreePoint(box, box.Centre, chipTaken, spacing: 2.0)
                         ?? FreePoint(box, box.Centre, chipTaken, spacing: 1.0)
                         ?? throw new InvalidOperationException($"No room for contact {i + 1} in {where}");
                chipTaken.Add(pt);
                _contacts.Add(new MissionContact(i + 1, pt.X, pt.Y, where));
            }
        }

        // ------------------------------------------------------------ output

        /// <summary>The factions on the tier's contact table, in order of first appearance.</summary>
        private string Holders()
        {
            var names = new List<string>();
            foreach (var r in _tier.Table)
                if (_c.Factions.Elements.FirstOrDefault(e => e.Id == r.Result) is { } e && !names.Contains(_c.Faction(e.Faction).Name))
                    names.Add(_c.Faction(e.Faction).Name);
            return string.Join(" and ", names);
        }

        private static string Cap(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];

        private int Limit => _size.Limit - (ComplicationIs("tight_window") ? 5 : 0);

        private int AlertStart => ComplicationIs("early_warning") ? 1 : 0;

        // The briefing's sentences are the reference generator's (tools/generate_mission.py), around content text.
        private Dictionary<string, string> BriefingTexts()
        {
            var m = _mission;
            var where = string.Join(" and ", _objectives.Select(x => x.Prose));
            var env = _want.Setting == "land" ? _c.Environment(_land!.Environment).Name.Split(",")[0].ToLowerInvariant() : "";
            var place = _want.Setting == "ship"
                ? $"a {_vessel!.Name.Split(" / ")[0].ToLowerInvariant()}"
                : $"{("aeiou".Contains(env[0]) ? "an" : "a")} {env} position";
            var holder = _want.Opposition is null ? Holders() : _c.Missions.Oppositions!.Single(o => o.Id == _want.Opposition).Name;
            var hvtName = _hvt is null ? null : _c.Troop(_hvt).Name;
            var why = m.Id switch
            {
                "seize" => $"Data or cargo the force needs is in {where}. The holders will move it soon.",
                "recover" => $"One of ours is held in {where}, bound and under guard. They will be moved soon.",
                "sabotage" => $"{Cap(where)} keep the place running. Both have to go before it can be used against us.",
                _ => $"The {hvtName?.ToLowerInvariant() ?? "target"} is in {where} with a guard. Taken alive, they will talk.",
            };
            var outText = _want.Setting == "ship" ? "Out by the airlock (P1)." : "Out by the player's own edge.";
            var exits = $" Enemy entry and exit: {_enemyEntry.Label.Split('(')[^1].TrimEnd(')')}.";
            if (_secondExits.Count > 0) exits += " Second enemy exit: " + string.Join(", ", _secondExits) + ".";
            bool hvtAdded = _hvt is not null && _guard.Composition.All(p => p.Troop != _hvt);
            var guardText = $"{_guard.Name} ({_enemies.Count - (hvtAdded ? 1 : 0)} models)";
            if (hvtAdded) guardText += $" with the {hvtName}";
            var comp = _complication;
            return new()
            {
                ["Tasking"] = $"{m.Name}. {m.Summary}",
                ["Situation"] = $"{holder} hold {place}. {why}",
                ["Objective"] = $"{Cap(where)}. {m.Task}",
                ["Opposition"] = $"{holder}, threat tier {_tier.Tier}. Guard (known): {guardText}, at the objective, {(_guard.Behavior == "automated" ? "Alerted" : "Unaware")}. " +
                                 $"Contacts (unknown): {_contacts.Count} chips.",
                ["Ground"] = _ground,
                ["Complication"] = comp.Id == "none" ? "None." : $"{comp.Name}: " + (comp.Id == "hazard" ? "see Ground." : comp.Text),
                ["Extraction and time"] = $"{outText}{exits} {_size.Name}: {Limit} minutes, clock die {_size.Die}. " +
                                          $"Check-ins every {_m.Clock.CheckInEvery} minutes from minute {_m.Clock.FirstCheckIn}. " +
                                          $"Alert track starts at {AlertStart}.",
                ["Grades"] = $"Decisive: {m.Decisive} Marginal: {m.Marginal} Failure: {m.Failure}",
            };
        }

        private Mission ToMission()
        {
            var texts = BriefingTexts();
            // Headings in the content's briefing order (missions.yaml); every heading has its text.
            var briefing = _m.Briefing.Select(b => new MissionBriefing(b.Heading, texts[b.Heading])).ToList();
            var version = _c.Core.Version;
            var setting = _want.Setting;
            var titlePlace = setting == "ship" ? _vessel!.Name : _c.Environment(_land!.Environment).Name;
            List<MissionCover> cover = setting == "ship"
                ? _deck!.Cover.Select(c => new MissionCover(c.Id, c.X, c.Y, c.W, c.H, c.Type, c.Label, c.Piece, Room: c.Room)).ToList()
                : _land!.Cover.Select(c => new MissionCover(c.Id, c.X, c.Y, c.W, c.H, c.Type, c.Label, c.Piece, Terrain: c.Terrain, Sector: c.Sector)).ToList();
            var board = setting == "ship" ? _deck!.Board : _land!.Board;

            return new Mission(
                Id: $"mission-{setting}-{_want.Seed}",
                Title: $"{_mission.Name}: {titlePlace.Split(" / ")[0]}, mission {_want.Seed}",
                Kind: "mission",
                KindLabel: "mission",
                RulesVersion: version[..version.LastIndexOf('.')],
                ContentVersion: version,
                Setting: setting,
                Seed: _want.Seed,
                MissionType: _mission.Id,
                Tier: _tier.Tier,
                Opposition: _want.Opposition,
                Size: _size.Id,
                Clock: new MissionClock(_size.Die, Limit, _m.Clock.CheckInEvery, _m.Clock.FirstCheckIn),
                AlertStart: AlertStart,
                Complication: _complication.Id,
                Situation: texts["Situation"],
                Briefing: briefing,
                Board: board,
                Tiles: setting == "ship" ? _deck!.Tiles : [],
                Doors: setting == "ship" ? _deck!.Doors : [],
                Cover: cover,
                Objectives: _objectives,
                EnemyEntry: _enemyEntry,
                SecondExits: _secondExits,
                Enemies: _enemies,
                Contacts: _contacts,
                Reinforcement: _tier.Reinforcement,
                Hvt: _hvt,
                Rolls: _log,
                Entry: _deck?.Entry,
                Vessel: _vessel?.Id,
                Hazard: setting == "ship" ? HazardText : null,
                DeckNotes: _deck?.Notes,
                Links: _deck?.Links,
                Environment: _land?.Environment,
                Conditions: _conditions?.Id,
                Deploy: setting == "land" ? [new MissionDeploy(0.3, 0.3, board.Width - 0.6, 5.7, "player", "within 6\" of this edge")] : null,
                Sectors: _land?.Sectors.Select(s => new MissionSector(s.Name, s.Text)).ToList());
        }
    }
}
