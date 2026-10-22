namespace Imperator.Core.Layouts;

/// <summary>A generated deck with the facts the reference statistics are measured on.</summary>
/// <param name="Compartments">Compartment tiles placed, counting the heart.</param>
/// <param name="HasLoop">More than one route between some rooms (the door graph has a cycle).</param>
public sealed record GeneratedDeck(Deck Deck, bool Connected, bool HasLoop, int Compartments);

/// <summary>
/// The chapter 11 deck layout procedure (content/layouts.yaml), seeded through <see cref="Rng"/>.
/// A port of tools/generate_layout.py, the reference implementation: the same tables, steps and order.
/// The paper procedure's player choices ("toward the side with more open board") are made the same way;
/// true ties are settled by a random draw. Python's and .NET's random streams differ, so decks match the
/// reference statistically, not seed for seed.
/// </summary>
public static class DeckGenerator
{
    public static GeneratedDeck Generate(ContentSet content, string vesselId, int seed) =>
        new DeckBuilder(content, content.Vessel(vesselId), seed).Build();
}

/// <summary>A cell of the 3" grid: x grows east, y grows north; (0, 0) is the south-west corner.</summary>
internal readonly record struct Cell(int X, int Y)
{
    public Cell Step(Dir d, int n = 1) => new(X + d.Dx() * n, Y + d.Dy() * n);
}

/// <summary>Compass directions, in the reference's iteration order.</summary>
internal enum Dir { E, N, W, S }

internal static class DirExtensions
{
    public static readonly Dir[] All = [Dir.E, Dir.N, Dir.W, Dir.S];
    public static int Dx(this Dir d) => d switch { Dir.E => 1, Dir.W => -1, _ => 0 };
    public static int Dy(this Dir d) => d switch { Dir.N => 1, Dir.S => -1, _ => 0 };
    public static Dir Left(this Dir d) => d switch { Dir.E => Dir.N, Dir.N => Dir.W, Dir.W => Dir.S, _ => Dir.E };
    public static Dir Right(this Dir d) => d switch { Dir.N => Dir.E, Dir.W => Dir.N, Dir.S => Dir.W, _ => Dir.S };
}

internal enum TileKind { Passage, Junction, Compartment, Small }

internal sealed class DeckBuilder(ContentSet content, VesselDef vessel, int seed)
{
    private sealed class Tile(TileKind kind, List<Cell> cells, string? room)
    {
        public TileKind Kind { get; } = kind;
        public List<Cell> Cells { get; } = cells;
        public string? Room { get; set; } = room;
        public bool Heart { get; set; }
        public string Id { get; set; } = "";
        public (double X, double Y, double W, double H) Box { get; set; }
    }

    /// <summary>A connection between two tiles. CellsA/CellsB are the touching cells (two each on a passage's long side).</summary>
    private sealed class Link(int a, int b, Cell[] cellsA, Cell[] cellsB, bool open)
    {
        public int A { get; } = a;
        public int B { get; } = b;
        public Cell[] CellsA { get; } = cellsA;
        public Cell[] CellsB { get; } = cellsB;
        /// <summary>A plain opening rather than a door.</summary>
        public bool Open { get; } = open;
        public string State { get; set; } = "";
        public string? Security { get; set; }
    }

    private const string Docking = "docking";
    private readonly LayoutsContent _l = content.Layouts;
    private readonly Rng _rng = new(seed);
    private readonly int _w = vessel.Board[0] / 3, _h = vessel.Board[1] / 3;
    private readonly Dictionary<Cell, int> _cells = [];
    private readonly List<Tile> _tiles = [];
    private readonly List<Link> _links = [];
    private readonly List<int> _spine = [];
    private readonly List<string> _notes = [];
    private Cell _entry;
    private int? _heart;
    private string? _hazard;
    private int _rolls;

    // ------------------------------------------------------------ dice and geometry

    private int D6()
    {
        _rolls++;
        return _rng.D(6);
    }

    private static T Lookup<T>(IEnumerable<T> rows, Func<T, D6Range> roll, int face) =>
        rows.FirstOrDefault(r => roll(r).Contains(face)) ?? throw new InvalidOperationException($"No table row for {face}");

    private string Result(List<LayoutRollRow> table) => Lookup(table, r => r.Roll, D6()).Result;

    private bool Free(IEnumerable<Cell> cells) =>
        cells.All(c => c.X >= 0 && c.X < _w && c.Y >= 0 && c.Y < _h && !_cells.ContainsKey(c));

    /// <summary>How much open board lies beyond a cell in a direction (for "the side with more room").</summary>
    private int OpenSpace(Cell cell, Dir d, int depth)
    {
        int n = 0;
        for (int i = 0; i < depth; i++)
        {
            cell = cell.Step(d);
            if (!Free([cell])) break;
            n++;
        }
        return n;
    }

    private int Place(TileKind kind, List<Cell> cells, string? room = null)
    {
        int idx = _tiles.Count;
        foreach (var c in cells) _cells[c] = idx;
        _tiles.Add(new Tile(kind, cells, room));
        return idx;
    }

    private void Connect(int a, int b, Cell[] ca, Cell[] cb, bool open = false) => _links.Add(new Link(a, b, ca, cb, open));

    /// <summary>Sides sorted by open board, most first; ties are the player's choice, made here by a random draw.</summary>
    private List<Dir> SideOrder(Cell cell, IEnumerable<Dir> sides, int depth = 2) =>
        sides.Select(s => (Open: OpenSpace(cell, s, depth), Tie: _rng.NextDouble(), Side: s)).ToList()
            .OrderByDescending(k => k.Open).ThenByDescending(k => k.Tie).Select(k => k.Side).ToList();

    private int CountCompartments() => _tiles.Count(t => t.Kind == TileKind.Compartment);

    // ------------------------------------------------------------ procedure

    public GeneratedDeck Build()
    {
        // 1. Entry and docking passage, at the middle of the west edge.
        int x0 = 0, y0 = (_h - 1) / 2;
        var d = Dir.E;
        int dock = Place(TileKind.Passage, [new(x0, y0), new(x0 + 1, y0)], Docking);
        _entry = new Cell(x0, y0);
        _spine.Add(dock);
        Cell head = new(x0 + 1, y0);
        int last = dock;

        // 2. Spine: junction and passage pairs.
        for (int k = 0; k < vessel.Spine; k++)
        {
            var j = head.Step(d);
            if (!Free([j])) break;
            int ji = Place(TileKind.Junction, [j]);
            Connect(last, ji, [head], [j], open: true);
            _spine.Add(ji);
            if (k > 0 && Result(_l.SpineTurn) == "turn")
                d = SideOrder(j, [d.Left(), d.Right()], 4)[0];
            bool placed = false;
            foreach (var nd in new List<Dir> { d }.Concat(SideOrder(j, [d.Left(), d.Right()], 4)).ToList())
            {
                List<Cell> cells = [j.Step(nd), j.Step(nd, 2)];
                if (!Free(cells)) continue;
                int pi = Place(TileKind.Passage, cells);
                Connect(ji, pi, [j], [cells[0]], open: true);
                _spine.Add(pi);
                (d, head, last, placed) = (nd, cells[1], pi, true);
                break;
            }
            if (!placed)
            {
                (head, last) = (j, ji);
                break;
            }
        }

        // 3. The heart at the far end.
        _heart = AttachCompartment(last, head, d, vessel.Heart, heart: true);
        if (_heart is null)
            foreach (int t in Enumerable.Reverse(_spine))
                if ((_heart = CompartmentBeside(t, vessel.Heart, heart: true)) is not null) break;

        // 4. Branches along the spine.
        int cmin = vessel.Compartments[0], cmax = vessel.Compartments[1];
        foreach (int t in _spine)
        {
            var tile = _tiles[t];
            if (tile.Room == Docking) continue;
            if (tile.Kind == TileKind.Passage)
            {
                var r = Result(_l.PassageBranch);
                var sides = PassageSides(t);
                if (r == "compartment") TryCompartments(t, sides, 1, cmax);
                else if (r == "compartment_both") TryCompartments(t, sides, 2, cmax);
                else if (r == "compartment_and_small")
                {
                    var got = TryCompartments(t, sides, 1, cmax);
                    var rest = sides.Where(s => !got.Contains(s)).ToList();
                    if (rest.Count > 0) SmallBeside(t, rest);
                }
            }
            else
            {
                int roll = Math.Min(6, D6() + vessel.JunctionBonus);
                var r = Lookup(_l.JunctionBranch, x => x.Roll, roll).Result;
                var sides = JunctionSides(t);
                if (r == "small" && sides.Count > 0) SmallBeside(t, sides);
                else if (r == "side_passage" && sides.Count > 0) SidePassage(t, sides, cmax);
            }
        }

        // 5. Fill to the minimum, from the far end back.
        foreach (int t in Enumerable.Reverse(_spine))
        {
            if (CountCompartments() >= cmin) break;
            if (_tiles[t].Room == Docking) continue;
            CompartmentBeside(t);
        }
        if (CountCompartments() < cmin)
            _notes.Add("Fewer compartments than the minimum fit: draw the rest on the dry-erase map.");

        // 6. Rooms, second doors, door states, hazard.
        RollRooms();
        Loops();
        RollDoors();
        var h = Result(_l.HazardRoll);
        _hazard = h == "none" ? null : vessel.Hazards[h == "first" ? 0 : 1];

        return new GeneratedDeck(ToDeck(), IsConnected(), _links.Count - _tiles.Count + 1 > 0, CountCompartments());
    }

    private List<Dir> PassageSides(int t)
    {
        var (a, b) = (_tiles[t].Cells[0], _tiles[t].Cells[1]);
        var axis = a.Y == b.Y ? Dir.E : Dir.N;
        return SideOrder(a, [axis.Left(), axis.Right()]);
    }

    private List<Dir> JunctionSides(int t)
    {
        var c = _tiles[t].Cells[0];
        return SideOrder(c, DirExtensions.All.Where(s => Free([c.Step(s)])));
    }

    private List<Dir> TryCompartments(int t, List<Dir> sides, int want, int cmax)
    {
        var got = new List<Dir>();
        foreach (var s in sides)
        {
            if (got.Count >= want || CountCompartments() >= cmax) break;
            if (CompartmentOnPassage(t, s) is not null) got.Add(s);
        }
        return got;
    }

    private int? CompartmentOnPassage(int t, Dir s, string? room = null)
    {
        var (a, b) = (_tiles[t].Cells[0], _tiles[t].Cells[1]);
        List<Cell> cells = [a.Step(s), b.Step(s), a.Step(s, 2), b.Step(s, 2)];
        if (!Free(cells)) return null;
        int ci = Place(TileKind.Compartment, cells, room);
        Connect(t, ci, [a, b], [cells[0], cells[1]]);
        return ci;
    }

    /// <summary>A compartment against one cell face (a junction side or a passage end), offset either way.</summary>
    private int? AttachCompartment(int t, Cell cell, Dir d, string? room = null, bool heart = false)
    {
        var left = d.Left();
        var n1 = cell.Step(d);
        var options = new List<List<Cell>>();
        foreach (int off in new[] { 0, -1 })
        {
            var b = n1.Step(left, off);
            List<Cell> cells = [b, b.Step(left), b.Step(d), b.Step(left).Step(d)];
            if (Free(cells)) options.Add(cells);
        }
        if (options.Count == 0) return null;
        var chosen = options.Count == 1 || _rng.NextDouble() < 0.5 ? options[0] : options[1];
        int ci = Place(TileKind.Compartment, chosen, room);
        if (heart) _tiles[ci].Heart = true;
        Connect(t, ci, [cell], [n1]);
        return ci;
    }

    private int? CompartmentBeside(int t, string? room = null, bool heart = false)
    {
        var tile = _tiles[t];
        if (tile.Kind == TileKind.Passage)
        {
            foreach (var s in PassageSides(t))
                if (CompartmentOnPassage(t, s, room) is int ci)
                {
                    if (heart) _tiles[ci].Heart = true;
                    return ci;
                }
        }
        else
        {
            foreach (var s in JunctionSides(t))
                if (AttachCompartment(t, tile.Cells[0], s, room, heart) is int ci) return ci;
        }
        return null;
    }

    private int? SmallBeside(int t, List<Dir> sides)
    {
        foreach (var s in sides)
            foreach (var c in _tiles[t].Cells)
            {
                var n = c.Step(s);
                if (!Free([n])) continue;
                int si = Place(TileKind.Small, [n]);
                Connect(t, si, [c], [n]);
                return si;
            }
        return null;
    }

    private int? SidePassage(int t, List<Dir> sides, int cmax)
    {
        var c = _tiles[t].Cells[0];
        foreach (var s in sides)
        {
            List<Cell> cells = [c.Step(s), c.Step(s, 2)];
            if (!Free(cells)) continue;
            int pi = Place(TileKind.Passage, cells, "side");
            Connect(t, pi, [c], [cells[0]], open: true);
            if (CountCompartments() < cmax)
            {
                if (AttachCompartment(pi, cells[1], s) is null) SmallBeside(pi, [s]);
            }
            else SmallBeside(pi, [s]);
            return pi;
        }
        return null;
    }

    private void RollRooms()
    {
        var used = new HashSet<string>();
        foreach (var t in _tiles)
        {
            if (t.Kind == TileKind.Compartment)
            {
                if (t.Heart)
                {
                    t.Room = vessel.Heart;
                    used.Add(t.Room);
                    continue;
                }
                // A unique room already on the deck moves on to the next row of the table.
                int roll = D6();
                string room = "";
                for (int step = 0; step < 6; step++)
                {
                    room = Lookup(vessel.Rooms, r => r.Roll, (roll - 1 + step) % 6 + 1).Room;
                    if (!(content.Room(room).Unique && used.Contains(room))) break;
                }
                t.Room = room;
                used.Add(room);
            }
            else if (t.Kind == TileKind.Small)
                t.Room = Lookup(_l.SmallRooms, r => r.Roll, D6()).Room;
        }
    }

    /// <summary>Second doors: each compartment rolls once for each other tile it touches but has no door to.</summary>
    private void Loops()
    {
        for (int i = 0; i < _tiles.Count; i++)
        {
            if (_tiles[i].Kind != TileKind.Compartment) continue;
            var linked = _links.Where(l => l.A == i || l.B == i).Select(l => l.A == i ? l.B : l.A).ToHashSet();
            foreach (var c in _tiles[i].Cells)
                foreach (var d in DirExtensions.All)
                {
                    var n = c.Step(d);
                    if (!_cells.TryGetValue(n, out int j) || j == i || linked.Contains(j) || _tiles[j].Kind == TileKind.Small) continue;
                    linked.Add(j);
                    if (Result(_l.LoopDoor) == "door") Connect(i, j, [c], [n]);
                    break;
                }
        }
    }

    private void RollDoors()
    {
        foreach (var dr in _links)
        {
            if (dr.Open)
            {
                dr.State = "open";
                continue;
            }
            if (_tiles[dr.A].Heart || _tiles[dr.B].Heart)
            {
                dr.State = "locked";
                dr.Security = vessel.HeartSecurity;
                continue;
            }
            dr.State = Result(_l.DoorState);
            if (dr.State == "locked")
                dr.Security = new[] { dr.A, dr.B }
                    .Select(x => _tiles[x].Room is { } id ? _l.Rooms.FirstOrDefault(r => r.Id == id)?.Security : null)
                    .FirstOrDefault(s => !string.IsNullOrEmpty(s)) ?? vessel.Security;
        }
    }

    private bool IsConnected()
    {
        var adj = Enumerable.Range(0, _tiles.Count).ToDictionary(i => i, _ => new HashSet<int>());
        foreach (var l in _links)
        {
            adj[l.A].Add(l.B);
            adj[l.B].Add(l.A);
        }
        var seen = new HashSet<int> { 0 };
        var stack = new Stack<int>([0]);
        while (stack.Count > 0)
            foreach (int j in adj[stack.Pop()])
                if (seen.Add(j)) stack.Push(j);
        return seen.Count == _tiles.Count;
    }

    // ------------------------------------------------------------ output (the scenario format)

    private static double R2(double v) => Math.Round(v, 2);

    private Deck ToDeck()
    {
        var prefix = new Dictionary<TileKind, string> { [TileKind.Passage] = "P", [TileKind.Junction] = "J", [TileKind.Compartment] = "C", [TileKind.Small] = "S" };
        var counters = new Dictionary<TileKind, int>();
        var tiles = new List<DeckTile>();
        foreach (var t in _tiles)
        {
            counters[t.Kind] = counters.GetValueOrDefault(t.Kind) + 1;
            t.Id = $"{prefix[t.Kind]}{counters[t.Kind]}";
            int minX = t.Cells.Min(c => c.X), maxX = t.Cells.Max(c => c.X), minY = t.Cells.Min(c => c.Y), maxY = t.Cells.Max(c => c.Y);
            double x = minX * 3, y = minY * 3, w = (maxX - minX + 1) * 3, h = (maxY - minY + 1) * 3;
            string name = t.Room == Docking ? "Docking passage"
                : t.Kind == TileKind.Passage ? "Passage"
                : t.Kind == TileKind.Junction ? "Junction"
                : content.Room(t.Room!).Name;
            string size = t.Kind switch { TileKind.Compartment => "6x6", TileKind.Passage => "6x3", _ => "3x3" };
            tiles.Add(new DeckTile(t.Id, name, size, x, y, w, h, t.Kind.ToString().ToLowerInvariant(),
                t.Room is Docking or "side" ? null : t.Room));
            t.Box = (x, y, w, h);
        }

        var doors = new List<DeckDoor>();
        foreach (var dr in _links)
        {
            if (dr.Open && _spine.Contains(dr.A) && _spine.Contains(dr.B)) continue; // spine joints are plain openings
            double x, y;
            string orientation;
            if (dr.CellsA.Length == 2)
            {
                // A passage's long side: the middle of the 6" wall.
                var (a1, a2, b1) = (dr.CellsA[0], dr.CellsA[1], dr.CellsB[0]);
                double mx = (a1.X + a2.X + 1) * 1.5, my = (a1.Y + a2.Y + 1) * 1.5;
                if (b1.Y != a1.Y) (x, y, orientation) = (mx, Math.Max(a1.Y, b1.Y) * 3, "h");
                else (x, y, orientation) = (Math.Max(a1.X, b1.X) * 3, my, "v");
            }
            else
            {
                var (ca, cb) = (dr.CellsA[0], dr.CellsB[0]);
                if (ca.Y != cb.Y) (x, y, orientation) = (ca.X * 3 + 1.5, Math.Max(ca.Y, cb.Y) * 3, "h");
                else (x, y, orientation) = (Math.Max(ca.X, cb.X) * 3, ca.Y * 3 + 1.5, "v");
            }
            doors.Add(new DeckDoor($"D{doors.Count + 1}", R2(x), R2(y), orientation, dr.State, dr.Security,
                $"{_tiles[dr.A].Id} to {_tiles[dr.B].Id}"));
        }

        // Cover: the heavy piece in the middle, light pieces in the corners farthest from the doors.
        var cover = new List<DeckCover>();
        var doorPoints = doors.Select(dd => (dd.X, dd.Y)).ToList();
        foreach (var t in _tiles)
        {
            if (t.Kind is not (TileKind.Compartment or TileKind.Small) || t.Room is null) continue;
            var (x, y, w, h) = t.Box;
            var pieces = content.Room(t.Room).Cover;
            var heavy = pieces.Where(p => p.Type == "heavy").Take(1);
            var light = pieces.Where(p => p.Type == "light");
            List<(double X, double Y)> slots = t.Kind == TileKind.Small
                ? [(x + 0.9, y + 0.9)]
                : new List<(double X, double Y)> { (x + 1.5, y + 1.5), (x + 4.5, y + 4.5), (x + 1.5, y + 4.5), (x + 4.5, y + 1.5) }
                    .OrderByDescending(p => doorPoints.Count == 0 ? 0 : doorPoints.Min(q => (p.X - q.X) * (p.X - q.X) + (p.Y - q.Y) * (p.Y - q.Y)))
                    .ToList();
            var centre = (X: x + w / 2, Y: y + h / 2);
            var plan = heavy.Select(p => (p, centre)).Concat(light.Zip(slots));
            foreach (var (p, (cx, cy)) in plan)
                cover.Add(new DeckCover($"T{cover.Count + 1}", R2(cx - p.W / 2), R2(cy - p.H / 2), p.W, p.H, p.Type, p.Label, p.Piece, t.Id));
        }

        DeckObjective? objective = null;
        if (_heart is int hi)
        {
            var (hx, hy, hw, hh) = _tiles[hi].Box;
            objective = new DeckObjective(hx + hw / 2, hy > 0 ? hy + hh * 0.75 : hy + hh * 0.25, content.Room(_tiles[hi].Room!).Name);
        }

        var version = content.Core.Version;
        var counts = new Dictionary<string, int>();
        foreach (var t in _tiles)
        {
            var key = t.Kind == TileKind.Small ? "junction" : t.Kind.ToString().ToLowerInvariant();
            counts[key] = counts.GetValueOrDefault(key) + 1;
        }

        return new Deck(
            Id: $"deck-{vessel.Id}-{seed}",
            Title: $"{vessel.Name}, deck {seed}",
            RulesVersion: version[..version.LastIndexOf('.')],
            ContentVersion: version,
            Kind: "deck",
            Vessel: vessel.Id,
            Seed: seed,
            Situation: vessel.Text,
            Board: new DeckBoard(vessel.Board[0], vessel.Board[1]),
            Tiles: tiles,
            Doors: doors,
            Entry: new DeckEntry(_entry.X * 3, _entry.Y * 3 + 1.5, "v", "Airlock (entry and exit)"),
            Cover: cover,
            Objective: objective,
            Hazard: _hazard is null ? "None" : content.Hazard(_hazard).Text,
            Rolls: _rolls,
            Notes: _notes,
            TileCounts: counts);
    }
}
