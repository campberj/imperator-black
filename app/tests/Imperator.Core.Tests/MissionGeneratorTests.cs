using System.Text.Json;
using Imperator.Core.Layouts;
using Imperator.Core.Missions;
using static Imperator.Core.Tests.TestContent;

namespace Imperator.Core.Tests;

public class MissionGeneratorTests
{
    private const int Seeds = 2000;

    public static TheoryData<string> Settings() => new(MissionGenerator.Settings);

    private static readonly Dictionary<string, List<Mission>> Cache = [];

    private static List<Mission> Missions(string setting)
    {
        lock (Cache)
        {
            if (!Cache.TryGetValue(setting, out var list))
                Cache[setting] = list = Enumerable.Range(1, Seeds).Select(s => MissionGenerator.Generate(Set, new MissionRequest(setting, s))).ToList();
            return list;
        }
    }

    private static double Share<T>(IEnumerable<T> xs, Func<T, bool> f) => xs.Count(f) / (double)xs.Count();
    private static double Faces(D6Range r) => r.Faces.Count() / 6.0;

    // Done when: over 2,000 seeds per setting, the mission and tier frequencies match their d6 tables.
    [Theory]
    [MemberData(nameof(Settings))]
    public void MissionAndTierFrequenciesMatchTheirTables(string setting)
    {
        var ms = Missions(setting);
        foreach (var g in Set.Missions.MissionRoll.GroupBy(r => r.Mission))
            Assert.Equal(g.Sum(r => Faces(r.Roll)), Share(ms, m => m.MissionType == g.Key), 0.04);
        foreach (var t in Set.Missions.Tiers)
            Assert.Equal(Set.Missions.TierRoll.Where(r => r.Tier == t.Tier).Sum(r => Faces(r.Roll)), Share(ms, m => m.Tier == t.Tier), 0.04);
        foreach (var c in Set.Missions.Complications)
            Assert.Equal(Faces(c.Roll), Share(ms, m => m.Complication == c.Id), 0.04);
    }

    // Done when: every contact chip and guard model stands inside the board, clear of solid cover.
    [Theory]
    [MemberData(nameof(Settings))]
    public void ChipsAndGuardsStandOnTheBoardClearOfCover(string setting)
    {
        string[] noStand = ["heavy", "light", "wire", "water"];
        foreach (var m in Missions(setting))
        {
            var blocked = m.Cover.Where(c => setting == "ship" || noStand.Contains(c.Type)).ToList();
            var points = m.Enemies.Select(e => (e.X, e.Y, What: $"guard {e.N}")).Concat(m.Contacts.Select(c => (c.X, c.Y, What: $"contact ?{c.N}")))
                .Concat(m.Objectives.Select(o => (o.X, o.Y, What: $"objective {o.N}")));
            foreach (var (x, y, what) in points)
            {
                Assert.True(x > 0 && x < m.Board.Width && y > 0 && y < m.Board.Height, $"seed {m.Seed}: {what} off the board");
                // Each stands clear of cover (the deck's objective, its heart marker, is the deck's own).
                if (what.StartsWith("objective") && setting == "ship") continue;
                foreach (var c in blocked)
                    Assert.False(x > c.X && x < c.X + c.W && y > c.Y && y < c.Y + c.H, $"seed {m.Seed}: {what} on {c.Id} {c.Label}");
            }
            // Aboard ship every model and chip is on a tile: chips in the tile they were placed in.
            if (setting == "ship")
            {
                bool OnTile(double x, double y, DeckTile t) => x >= t.X && x <= t.X + t.W && y >= t.Y && y <= t.Y + t.H;
                foreach (var c in m.Contacts) Assert.True(OnTile(c.X, c.Y, m.Tiles.Single(t => t.Id == c.Where)), $"seed {m.Seed}: ?{c.N} outside {c.Where}");
                foreach (var e in m.Enemies) Assert.Contains(m.Tiles, t => OnTile(e.X, e.Y, t));
            }
            // Chips are at least 2" apart, or 1" when a second chip shares a small room.
            var all = m.Contacts.Select(c => (c.X, c.Y)).ToList();
            for (int i = 0; i < all.Count; i++)
                for (int j = i + 1; j < all.Count; j++)
                    Assert.True(Math.Sqrt(Math.Pow(all[i].X - all[j].X, 2) + Math.Pow(all[i].Y - all[j].Y, 2)) >= 1.0 - 1e-9, $"seed {m.Seed}: chips too close");
        }
    }

    // Done when: objective counts match the mission type.
    [Theory]
    [MemberData(nameof(Settings))]
    public void ObjectiveCountsMatchTheMissionType(string setting)
    {
        foreach (var m in Missions(setting))
        {
            Assert.Equal(Set.Mission(m.MissionType).Objectives, m.Objectives.Count);
            Assert.Equal(Enumerable.Range(1, m.Objectives.Count), m.Objectives.Select(o => o.N));
        }
    }

    [Theory]
    [MemberData(nameof(Settings))]
    public void TheGuardIsOneElementNearTheFirstObjective(string setting)
    {
        int near = 0, total = 0;
        foreach (var m in Missions(setting))
        {
            var guard = Set.Element(m.Enemies[0].Element);
            Assert.All(m.Enemies, e => Assert.Equal(guard.Id, e.Element));
            Assert.Contains(guard.Id, Set.Tier(m.Tier).Table.Select(r => r.Result));
            var models = guard.Composition.SelectMany(p => Enumerable.Repeat(p.Troop, p.Count)).ToList();
            if (m.MissionType == "capture")
            {
                Assert.Equal(Set.Tier(m.Tier).Hvt, m.Hvt);
                if (!models.Contains(m.Hvt!)) models.Add(m.Hvt!);
                Assert.Contains(m.Enemies, e => e.Troop == m.Hvt && e.State == "Unaware (the HVT)");
            }
            else Assert.Null(m.Hvt);
            Assert.Equal(models, m.Enemies.Select(e => e.Troop));
            var o = m.Objectives[0];
            foreach (var e in m.Enemies)
            {
                total++;
                if (Math.Sqrt(Math.Pow(e.X - o.X, 2) + Math.Pow(e.Y - o.Y, 2)) <= 3.0 + 1e-9) near++;
            }
        }
        // Within 3" of the objective (the reference relaxes that only if the objective's surroundings are full; not in these seeds).
        Assert.Equal(total, near);
    }

    [Theory]
    [MemberData(nameof(Settings))]
    public void ComplicationsChangeTheMission(string setting)
    {
        var size = Set.ClockSize("small");
        foreach (var m in Missions(setting))
        {
            int contacts = Set.Tier(m.Tier).Contacts + (m.Complication == "extra_contact" ? 1 : 0);
            Assert.Equal(contacts, m.Contacts.Count);
            Assert.Equal(m.Complication == "early_warning" ? 1 : 0, m.AlertStart);
            Assert.Equal(size.Limit - (m.Complication == "tight_window" ? 5 : 0), m.Clock.Limit);
            Assert.Equal((size.Die, 5, 10), (m.Clock.Die, m.Clock.CheckInEvery, m.Clock.FirstCheckIn));
            if (m.Complication == "hazard")
            {
                if (setting == "ship") Assert.Equal(Set.Hazard(Set.Vessel(m.Vessel!).Hazards[0]).Text, m.Hazard);
                else Assert.Equal("night", m.Conditions);
            }
        }
    }

    [Fact]
    public void SeparatistMissionsUseOnlySeparatistElementsAndTheirHvt()
    {
        foreach (int tier in Enumerable.Range(1, 4))
        {
            var mission = MissionGenerator.Generate(Set, new MissionRequest("land", 8 + tier, Mission: "capture", Tier: tier,
                Environment: "industrial", Opposition: "separatist"));
            Assert.Equal("separatist", mission.Opposition);
            Assert.Equal(Set.Tier(tier, "separatist").Hvt, mission.Hvt);
            Assert.All(mission.Enemies, enemy => Assert.Equal("separatist", Set.Element(enemy.Element).Faction));
            Assert.Equal("separatist", Set.Element(mission.Reinforcement).Faction);
        }
        var top = MissionGenerator.Generate(Set, new MissionRequest("land", 42, Mission: "capture", Tier: 4,
            Environment: "industrial", Opposition: "separatist"));
        Assert.Equal("sep_grievous", top.Hvt);
        Assert.Contains("Separatist Alliance", top.Situation);
        Assert.Contains("an industrial position", top.Situation);
    }

    [Fact]
    public void GuardsWithAutomatedBehaviourBeginAlerted()
    {
        var found = Enumerable.Range(1, 500).Select(i => MissionGenerator.Generate(Set,
            new MissionRequest("land", i, Tier: 2, Environment: "desert")))
            .First(m => Set.Element(m.Enemies[0].Element).Behavior == "automated");
        Assert.All(found.Enemies, enemy => Assert.StartsWith("Alerted", enemy.State));
        Assert.Contains("Alerted", found.Briefing.Single(b => b.Heading == "Opposition").Text);
    }

    [Fact]
    public void ShipObjectivesFollowTheMissionType()
    {
        foreach (var m in Missions("ship"))
        {
            var deck = DeckGenerator.Generate(Set, m.Vessel!, m.Seed).Deck;
            var hops = deck.RouteFrom("P1");
            var comps = deck.Tiles.Where(t => t.Kind == "compartment").ToList();
            var obj = m.Objectives.Select(o => o.Tile).ToList();
            switch (m.MissionType)
            {
                case "seize" or "capture":
                    if (deck.Heart is not null) Assert.Equal([deck.Heart], obj);
                    break;
                case "recover":
                    var others = comps.Where(t => t.Id != deck.Heart).ToList();
                    if (others.Count > 0) Assert.Equal(others.Max(t => hops[t.Id]), hops[obj[0]]);
                    break;
                case "sabotage":
                    if (deck.Heart is not null) Assert.Equal(deck.Heart, obj[0]);
                    Assert.NotEqual(obj[0], obj[1]);
                    break;
            }
            // The enemy entry is the spine tile farthest from the airlock by route.
            Assert.Equal(deck.Spine.Max(id => hops[id]), hops[m.EnemyEntry.Tile]);
            Assert.Contains(m.EnemyEntry.Tile, deck.Spine);
        }
    }

    [Fact]
    public void LandObjectivesAreInTheFarRow()
    {
        foreach (var m in Missions("land"))
        {
            int rows = m.Board.Height / 12;
            Assert.All(m.Objectives, o => Assert.EndsWith(rows.ToString(), o.Tile));
            if (m.MissionType == "sabotage") Assert.Equal(["A" + rows, "ABCDE"[m.Board.Width / 12 - 1] + rows.ToString()], m.Objectives.Select(o => o.Tile));
            Assert.Equal("edge", m.EnemyEntry.Tile);
            Assert.Equal((m.Board.Width / 2.0, m.Board.Height - 0.4), (m.EnemyEntry.X, m.EnemyEntry.Y));
            // Contacts never within 12" of the player edge, never in an objective's sector.
            Assert.All(m.Contacts, c => Assert.True(c.Y >= 12));
            Assert.All(m.Contacts, c => Assert.DoesNotContain(c.Where, m.Objectives.Select(o => o.Tile)));
            Assert.Single(m.Deploy!);
        }
    }

    [Theory]
    [MemberData(nameof(Settings))]
    public void TheBriefingFollowsTheContentHeadings(string setting)
    {
        foreach (var m in Missions(setting).Take(300))
        {
            Assert.Equal(Set.Missions.Briefing.Select(b => b.Heading), m.Briefing.Select(b => b.Heading));
            Assert.All(m.Briefing, b => Assert.False(string.IsNullOrWhiteSpace(b.Text)));
            var mission = Set.Mission(m.MissionType);
            Assert.StartsWith($"{mission.Name}. {mission.Summary}", m.Briefing[0].Text);
            Assert.Contains(mission.Task, m.Briefing.Single(b => b.Heading == "Objective").Text);
            Assert.Contains($"threat tier {m.Tier}", m.Briefing.Single(b => b.Heading == "Opposition").Text);
            Assert.Contains($"Contacts (unknown): {m.Contacts.Count} chips.", m.Briefing.Single(b => b.Heading == "Opposition").Text);
            Assert.Contains($"{m.Clock.Limit} minutes, clock die {m.Clock.Die}", m.Briefing.Single(b => b.Heading == "Extraction and time").Text);
            Assert.Equal(m.Situation, m.Briefing.Single(b => b.Heading == "Situation").Text);
        }
    }

    [Fact]
    public void AShipMissionUsesTheDeckForItsSeed()
    {
        var m = MissionGenerator.Generate(Set, new MissionRequest("ship", 42, Vessel: "corvette"));
        var deck = DeckGenerator.Generate(Set, "corvette", 42).Deck;
        Assert.Equal(JsonSerializer.Serialize(deck.Tiles), JsonSerializer.Serialize(m.Tiles));
        Assert.Equal(JsonSerializer.Serialize(deck.Doors), JsonSerializer.Serialize(m.Doors));
        Assert.Equal(deck.Cover.Count, m.Cover.Count);
        Assert.Contains($"Deck: {Set.Vessel("corvette").Name}, seed 42 ({deck.Rolls} rolls, chapter 11)", m.Rolls);
    }

    [Fact]
    public void NamedChoicesAreKept()
    {
        var m = MissionGenerator.Generate(Set, new MissionRequest("land", 5, Mission: "sabotage", Tier: 4, Environment: "fortified", Size: "large"));
        Assert.Equal(("sabotage", 4, "fortified", "large", 48), (m.MissionType, m.Tier, m.Environment, m.Size, m.Board.Width));
        Assert.Equal("Mission type: named, Sabotage", m.Rolls[0]);
        Assert.Equal("Threat tier: named, 4", m.Rolls[1]);
        Assert.Equal(Set.ClockSize("large").Die, m.Clock.Die);

        var s = MissionGenerator.Generate(Set, new MissionRequest("ship", 5, Mission: "capture", Tier: 3, Vessel: "station_module"));
        Assert.Equal(("capture", 3, "station_module"), (s.MissionType, s.Tier, s.Vessel));
        Assert.Equal(Set.Tier(3).Hvt, s.Hvt);
    }

    [Fact]
    public void SameRequestSameMission()
    {
        string Json(int seed) => JsonSerializer.Serialize(MissionGenerator.Generate(Set, new MissionRequest("land", seed)));
        Assert.Equal(Json(11), Json(11));
        Assert.NotEqual(Json(11), Json(12));
        // Ship and land missions with the same seed number roll different dice.
        var ship = MissionGenerator.Generate(Set, new MissionRequest("ship", 11));
        var land = MissionGenerator.Generate(Set, new MissionRequest("land", 11));
        Assert.Equal(("mission-ship-11", "mission-land-11"), (ship.Id, land.Id));
    }

    [Fact]
    public void BadRequestsFailLoudly()
    {
        Assert.Throws<ArgumentException>(() => MissionGenerator.Generate(Set, new MissionRequest("space", 1)));
        Assert.Throws<ArgumentException>(() => MissionGenerator.Generate(Set, new MissionRequest("ship", 1, Size: "large")));
        Assert.Contains("'heist'", Assert.Throws<KeyNotFoundException>(() => MissionGenerator.Generate(Set, new MissionRequest("ship", 1, Mission: "heist"))).Message);
        Assert.Throws<KeyNotFoundException>(() => MissionGenerator.Generate(Set, new MissionRequest("ship", 1, Tier: 9)));
    }

    [Fact]
    public void FromKeyIsStable()
    {
        // FNV-1a of "mission-ship-42": the same on every run and platform.
        Assert.Equal(Rng.FromKey("mission-ship-42").D(1_000_000), Rng.FromKey("mission-ship-42").D(1_000_000));
        // Computed independently in Python: h = 2166136261; for each char: h ^= ord(c); h = h * 16777619 mod 2^32; h & 0x7fffffff.
        Assert.Equal(1_028_338_807, Rng.FromKey("mission-ship-42").Seed);
    }
}
