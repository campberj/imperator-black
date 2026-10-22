using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Imperator.Core;
using static Imperator.Core.Tests.TestContent;

namespace Imperator.Core.Tests;

public class ContentLoadingTests
{
    public static TheoryData<string> ElementIds() => new(Set.Factions.Elements.Select(e => e.Id));
    public static TheoryData<string> TeamIds() => new(Set.Rosters.Teams.Select(t => t.Id));
    public static TheoryData<string> StartingForceIds() => new(Set.Rosters.StartingForces.Select(f => f.Id));

    [Fact]
    public void CountsMatchPythonValidator()
    {
        // tools/validate_content.py: "62 troop types, 28 elements, 8 teams, 11 gear items, 9 behavior profiles, 4 vessels."
        Assert.Equal(62, Set.Troops.Troops.Count);
        Assert.Equal(28, Set.Factions.Elements.Count);
        Assert.Equal(8, Set.Rosters.Teams.Count);
        Assert.Equal(11, Set.Gear.Gear.Count);
        Assert.Equal(9, Set.Behaviors.Behaviors.Count);
        Assert.Equal(4, Set.Layouts.Vessels.Count);
    }

    [Fact]
    public void ShippedContentHasNoErrors() => Assert.Empty(ContentValidator.Validate(Set));

    [Fact]
    public void LoadsKeywordsBehaviorsAndDemolitions()
    {
        Assert.Contains(Set.Keywords.Weapon, k => k.Id == "silenced");
        Assert.Contains(Set.Keywords.Armour, k => k.Id == "sealed");
        Assert.Equal(["breach"], Set.Weapons.Demolitions!.Single(d => d.Id == "breaching_charge").Keywords);
        Assert.Equal([1, 2, 3, 4], Set.Factions.ThreatTiers.Select(t => t.Tier));
    }

    [Fact]
    public void LoadsSoloOpponentProcedures()
    {
        var b = Set.Behaviors;
        Assert.Equal(["unaware", "alerted", "engaged", "setup"], b.Awareness.Select(a => a.Id));
        Assert.Equal(["quiet", "loud", "very_loud"], b.Noise.Select(n => n.Id));
        Assert.Equal([0, 1, 2], b.Noise.Select(n => n.Alert));
        Assert.Equal("Fire and manoeuvre", Set.DrillFor("regular").Name);
        foreach (var r in Set.Core.Ranks) Set.DrillFor(r.Id);
        Assert.All(new[] { b.Marker, b.Alarm, b.ActivationOrder, b.Targets, b.Moving, b.CloseQuarters, b.Surrender, b.TieBreakers },
            Assert.NotEmpty);
    }

    [Fact]
    public void LoadsBehaviorProfiles()
    {
        var cowed = Set.Behavior("cowed");
        Assert.Equal("Cowed", cowed.Name);
        Assert.True(cowed.Surrenders);
        Assert.False(cowed.CloseQuarters);
        Assert.Equal("Breaks after any lost exchange.", cowed.CloseCombat);
        Assert.All(Set.Behaviors.Behaviors, bh =>
        {
            Assert.NotEmpty(bh.Engaged);
            Assert.NotEmpty(bh.Alerted);
            Assert.NotEmpty(bh.Reactions);
            Assert.False(string.IsNullOrWhiteSpace(bh.Investigate));
        });
    }

    [Fact]
    public void LoadsShootScaleAndTerms()
    {
        Assert.Equal(["0", "1", "2", "3", "4 or more"], Set.Core.ShootScale!.Select(s => s.Value));
        Assert.Equal(["Area", "Approach", "Exit"], Set.Behaviors.Terms.Select(t => t.Term));
    }

    [Fact]
    public void LoadsLayoutTables()
    {
        var ly = Set.Layouts;
        Assert.Equal(["compartment", "passage", "junction"], ly.Tiles.Select(t => t.Id));
        Assert.All(ly.Tiles, t => Assert.Equal(12, t.Owned));
        Assert.Equal([2, 1], Set.Tile("passage").Cells);
        Assert.Equal(new D6Range(1, 2), ly.VesselRoll[0].Roll);
        Assert.Equal("freighter", ly.VesselRoll.Single(r => r.Roll.Contains(2)).Vessel);
        Assert.Equal("compartment_both", ly.PassageBranch.Single(r => r.Roll.Contains(5)).Result);
        Assert.Equal("hardened", Set.Room("armoury").Security);
        Assert.True(Set.Room("bridge").Unique);
        Assert.Equal(["heavy", "light", "light"], Set.Room("cargo_hold").Cover.Select(c => c.Type));
        Assert.Equal(1.5, Set.Room("cargo_hold").Cover[0].W);
        Assert.All(ly.SmallRooms, r => Assert.Equal("small", Set.Room(r.Room).Size));
    }

    public static TheoryData<string> VesselIds() => new(Set.Layouts.Vessels.Select(v => v.Id));

    [Theory]
    [MemberData(nameof(VesselIds))]
    public void VesselResolves(string id)
    {
        var v = Set.Vessel(id);
        Assert.Equal("compartment", Set.Room(v.Heart).Size);
        Set.SecurityLevel(v.Security);
        Set.SecurityLevel(v.HeartSecurity);
        foreach (var h in v.Hazards) Set.Hazard(h);
        Assert.All(v.Rooms, r => Assert.Equal("compartment", Set.Room(r.Room).Size));
        Assert.Equal([1, 2, 3, 4, 5, 6], v.Rooms.SelectMany(r => r.Roll.Faces).Order());
        Assert.All(v.Board, x => Assert.Equal(0, x % 3));
        Assert.True(v.Compartments[0] <= v.Compartments[1]);
    }

    [Fact]
    public void StationModuleHasJunctionBonus()
    {
        Assert.Equal(1, Set.Vessel("station_module").JunctionBonus);
        Assert.Equal(0, Set.Vessel("freighter").JunctionBonus);
        Assert.Equal([36, 18], Set.Vessel("freighter").Board);
    }

    [Fact]
    public void LoadsTerrain()
    {
        var tr = Set.Terrain;
        Assert.Equal(13, tr.Types.Count);
        Assert.Equal("difficult", Set.TerrainType("woods").Move);
        Assert.Equal("linear", Set.TerrainType("low_wall").Kind);
        Assert.Equal((3, 24), (tr.Height.Level, tr.Noise.LoudRange));
        Assert.Equal(["clear", "rain", "haze", "night"], tr.Conditions.Select(c => c.Id));
        Assert.All(tr.Environments, e => Assert.Equal([1, 2, 3, 4, 5, 6], e.Table.SelectMany(r => r.Roll.Faces).Order()));
        Assert.All(tr.EnvironmentRoll, r => Set.Environment(r.Environment));
        Assert.Equal("Two containers (solid).", Set.Environment("industrial").Fill);
        Assert.Equal(3, tr.Boards.Count);
    }

    [Theory]
    [InlineData("3", 3, 3)]
    [InlineData("1-4", 1, 4)]
    [InlineData(" 5-6 ", 5, 6)]
    public void ParsesD6Range(string text, int lo, int hi) => Assert.Equal(new D6Range(lo, hi), D6Range.Parse(text));

    [Theory]
    [InlineData("")]
    [InlineData("x")]
    [InlineData("1-x")]
    public void RejectsBadD6Range(string text) => Assert.Throws<FormatException>(() => D6Range.Parse(text));

    [Fact]
    public void AcceptsANumericRoll()
    {
        // YAML `roll: 1` (unquoted) compiles to a JSON number; validate_content.py accepts it, so the loader does too.
        var set = LoadMutated(root => Replace(root, "layouts.small_rooms[0].roll", "1"));
        Assert.Equal(new D6Range(1, 1), set.Layouts.SmallRooms[0].Roll);
    }

    [Fact]
    public void BadRollTextFailsLoudly()
    {
        var e = Assert.Throws<ContentException>(() => LoadMutated(root => Replace(root, "layouts.door_state[0].roll", "\"one\"")));
        Assert.Contains(e.Errors, err => err.StartsWith("layouts:") && err.Contains("Bad d6 roll 'one'"));
    }

    [Fact]
    public void DecksOverTheTileInventoryFailLoudly()
    {
        // With one tile of each size owned, no vessel's deck fits: the generator check reports each vessel once.
        var e = Assert.Throws<ContentException>(() => LoadMutated(root =>
        {
            foreach (var tile in Node(root, "layouts.tiles").AsArray()) tile!["owned"] = 1;
        }));
        foreach (var v in Set.Layouts.Vessels)
            Assert.Contains($"layout {v.Id} seed 1: disconnected or over the tile inventory", e.Errors);
    }

    [Theory]
    [InlineData("layouts")]
    [InlineData("terrain")]
    public void MissingSectionFailsLoudly(string section)
    {
        var e = Assert.Throws<ContentException>(() => LoadMutated(root => root.AsObject().Remove(section)));
        Assert.Contains($"{section}: section missing", e.Errors);
    }

    [Fact]
    public void LoadsSecurityLevels()
    {
        var sec = Set.Core.Security;
        Assert.Equal((1, 3), (sec.WorkingBonusPerFailure, sec.WorkingBonusMax));
        Assert.Equal(["basic", "secured", "hardened", "military"], sec.Levels.Select(l => l.Id));
        Assert.Equal((9, 1), (Set.SecurityLevel("secured").Target, Set.SecurityLevel("secured").Successes));
        Assert.Equal((10, 2), (Set.SecurityLevel("military").Target, Set.SecurityLevel("military").Successes));
    }

    [Theory]
    [MemberData(nameof(ElementIds))]
    public void ElementResolves(string id)
    {
        var e = Set.Element(id);
        Assert.Equal("enemy", Set.Faction(e.Faction).Side);
        Set.Behavior(e.Behavior);
        Assert.Contains(Set.Factions.ThreatTiers, t => t.Tier == e.Tier);
        Assert.NotEmpty(e.Composition);
        foreach (var part in e.Composition)
        {
            Assert.True(part.Count >= 1);
            Soldier.FromTroop(Set, part.Troop);
        }
        foreach (var alt in e.Alternatives ?? []) Soldier.FromTroop(Set, alt);
    }

    [Theory]
    [MemberData(nameof(TeamIds))]
    public void TeamResolves(string id)
    {
        var team = Set.Team(id);
        Assert.Equal("player", Set.Faction(team.Faction).Side);
        foreach (var slot in team.Slots)
        {
            Assert.True(slot.Count >= 1);
            Assert.Equal(team.Faction, Set.Troop(slot.Troop).Faction);
            Soldier.FromTroop(Set, slot.Troop);
        }
    }

    [Theory]
    [MemberData(nameof(StartingForceIds))]
    public void StartingForceResolves(string id)
    {
        var force = Set.StartingForce(id);
        Set.Faction(force.Faction);
        foreach (var team in force.Teams) Set.Team(team);
        foreach (var troop in force.Attachments?.From ?? []) Soldier.FromTroop(Set, troop);
    }

    [Fact]
    public void PocStartingForceIsSixModels()
    {
        var force = Set.StartingForce("poc_boarding_element");
        int models = force.Teams.Sum(t => Set.Team(t).Slots.Sum(s => s.Count)) + (force.Attachments?.Choose ?? 0);
        Assert.Equal(6, models);
    }

    [Fact]
    public void UnknownLookupNamesTheId()
    {
        var e = Assert.Throws<KeyNotFoundException>(() => Set.Element("no_such_element"));
        Assert.Contains("'no_such_element'", e.Message);
    }

    // Each case breaks one reference in the shipped content.json; messages match tools/validate_content.py.
    [Theory]
    [InlineData("factions.elements[0].composition[0].troop", "element salvage_crew troop: unknown id 'nope'")]
    [InlineData("factions.elements[0].behavior", "element salvage_crew behavior: unknown id 'nope'")]
    [InlineData("factions.elements[0].faction", "element salvage_crew faction: unknown id 'nope'")]
    [InlineData("factions.elements[5].alternatives[0]", "element automated_defences alternative: unknown id 'nope'")]
    [InlineData("rosters.teams[0].slots[0].troop", "team vat_fireteam slot: unknown id 'nope'")]
    [InlineData("rosters.starting_forces[0].teams[0]", "starting force poc_boarding_element team: unknown id 'nope'")]
    [InlineData("rosters.starting_forces[0].attachments.from[0]", "starting force poc_boarding_element attachment: unknown id 'nope'")]
    [InlineData("troops.troops[0].faction", "troop tfb_vat_leader faction: unknown id 'nope'")]
    [InlineData("troops.troops[0].weapon", "troop tfb_vat_leader weapon: unknown id 'nope'")]
    [InlineData("troops.troops[0].gear[0]", "troop tfb_vat_leader gear: unknown id 'nope'")]
    [InlineData("weapons.ranged[1].keywords[0]", "weapon smg keyword: unknown id 'nope'")]
    [InlineData("weapons.demolitions[0].keywords[0]", "weapon breaching_charge keyword: unknown id 'nope'")]
    [InlineData("armour.armour[2].keywords[0]", "armour void_suit keyword: unknown id 'nope'")]
    [InlineData("gear.gear[1].melee", "gear breaching_tool melee: unknown id 'nope'")]
    [InlineData("layouts.small_rooms[0].room", "layouts.small_rooms room: unknown id 'nope'")]
    [InlineData("layouts.vessel_roll[0].vessel", "layouts.vessel_roll vessel: unknown id 'nope'")]
    [InlineData("layouts.rooms[7].security", "room armoury security: unknown id 'nope'")]
    [InlineData("layouts.vessels[0].rooms[0].room", "vessel freighter room: unknown id 'nope'")]
    [InlineData("layouts.vessels[0].heart", "vessel freighter heart: unknown id 'nope'")]
    [InlineData("layouts.vessels[2].security", "vessel corvette security: unknown id 'nope'")]
    [InlineData("layouts.vessels[0].heart_security", "vessel freighter heart_security: unknown id 'nope'")]
    [InlineData("layouts.vessels[0].hazards[0]", "vessel freighter hazard: unknown id 'nope'")]
    [InlineData("terrain.environment_roll[0].environment", "terrain.environment_roll environment: unknown id 'nope'")]
    public void BadReferenceFailsLoudly(string path, string expected)
    {
        var e = Assert.Throws<ContentException>(() => LoadMutated(root =>
        {
            Assert.Equal(JsonValueKind.String, Node(root, path).GetValueKind());
            Replace(root, path, "\"nope\"");
        }));
        Assert.Contains(e.Errors, err => err.EndsWith(expected));
    }

    // Solo opponent and security checks, rules v0.5; messages match tools/validate_content.py.
    [Theory]
    [InlineData("behaviors.behaviors[0].engaged", "[]", "behavior cowed: needs a non-empty 'engaged' list")]
    [InlineData("behaviors.behaviors[2].reactions", "[]", "behavior aggressive: needs a non-empty 'reactions' list")]
    [InlineData("behaviors.behaviors[1].status", "\"draft\"", "behavior sentry: status must be settled or proposed")]
    [InlineData("behaviors.drills[1].rank", "\"green\"", "behaviors.drills: rank 'regular' needs exactly one drill")]
    [InlineData("behaviors.drills[1].rank", "\"green\"", "behaviors.drills: rank 'green' needs exactly one drill")]
    [InlineData("behaviors.drills[2].id", "\"green\"", "behaviors.drills: duplicate id 'green'")]
    [InlineData("behaviors.noise[1].id", "\"quiet\"", "behaviors.noise: duplicate id 'quiet'")]
    [InlineData("behaviors.awareness[1].id", "\"unaware\"", "behaviors.awareness: duplicate id 'unaware'")]
    [InlineData("behaviors.tie_breakers", "[]", "behaviors.tie_breakers: missing or empty")]
    [InlineData("behaviors.close_quarters", "[]", "behaviors.close_quarters: missing or empty")]
    [InlineData("core.security.levels[0].target", "13", "security level basic: target 2-12 and successes 1+")]
    [InlineData("core.security.levels[0].target", "1", "security level basic: target 2-12 and successes 1+")]
    [InlineData("core.security.levels[3].successes", "0", "security level military: target 2-12 and successes 1+")]
    [InlineData("core.security.levels[1].id", "\"basic\"", "core.security: duplicate id 'basic'")]
    [InlineData("layouts.vessel_roll[0].roll", "\"1\"", "layouts.vessel_roll: d6 rows must cover 1-6 exactly once")]
    [InlineData("layouts.spine_turn[1].roll", "\"4-6\"", "layouts.spine_turn: d6 rows must cover 1-6 exactly once")]
    [InlineData("layouts.small_rooms[5].roll", "\"5\"", "layouts.small_rooms: d6 rows must cover 1-6 exactly once")]
    [InlineData("layouts.door_state[2].roll", "\"6-7\"", "layouts.door_state: d6 rows must cover 1-6 exactly once")]
    [InlineData("layouts.vessels[0].rooms[0].roll", "\"1-2\"", "vessel freighter rooms: d6 rows must cover 1-6 exactly once")]
    [InlineData("layouts.rooms[0].cover[0].type", "\"medium\"", "room cargo_hold: cover type must be light or heavy")]
    [InlineData("layouts.vessels[0].board[0]", "35", "vessel freighter: board must be in multiples of 3 inches")]
    [InlineData("layouts.rooms[1].id", "\"cargo_hold\"", "layouts.rooms: duplicate id 'cargo_hold'")]
    [InlineData("layouts.hazards[1].id", "\"darkness\"", "layouts.hazards: duplicate id 'darkness'")]
    [InlineData("layouts.vessels[1].id", "\"freighter\"", "layouts.vessels: duplicate id 'freighter'")]
    [InlineData("terrain.types[1].id", "\"open\"", "terrain.types: duplicate id 'open'")]
    [InlineData("terrain.environments[1].id", "\"urban\"", "terrain.environments: duplicate id 'urban'")]
    [InlineData("terrain.environments[0].table[0].roll", "\"2\"", "terrain environment urban: d6 rows must cover 1-6 exactly once")]
    [InlineData("terrain.environment_roll[0].roll", "\"6\"", "terrain.environment_roll: d6 rows must cover 1-6 exactly once")]
    [InlineData("terrain.conditions[0].roll", "\"1-2\"", "terrain.conditions: d6 rows must cover 1-6 exactly once")]
    [InlineData("terrain.types[0].cover", "\"\"", "terrain type open: needs 'cover'")]
    [InlineData("behaviors.terms[0].text", "\"\"", "behaviors.terms: each needs term and text")]
    public void InvalidValueFailsLoudly(string path, string json, string expected)
    {
        var e = Assert.Throws<ContentException>(() => LoadMutated(root => Replace(root, path, json)));
        Assert.Contains(expected, e.Errors);
    }

    [Theory]
    [InlineData("behaviors", "marker", "behaviors.marker: missing or empty")]
    [InlineData("behaviors.behaviors[3]", "alerted", "behavior tactical: needs a non-empty 'alerted' list")]
    [InlineData("terrain.types[2]", "sight", "terrain type woods: needs 'sight'")]
    [InlineData("terrain.types[4]", "move", "terrain type low_wall: needs 'move'")]
    [InlineData("behaviors.terms[1]", "term", "behaviors.terms: each needs term and text")]
    public void MissingFieldFailsWithValidatorMessage(string parent, string key, string expected)
    {
        var e = Assert.Throws<ContentException>(() => LoadMutated(root => Node(root, parent).AsObject().Remove(key)));
        Assert.Contains(expected, e.Errors);
    }

    [Theory]
    [InlineData("surrenders")]
    [InlineData("investigate")]
    [InlineData("close_combat")]
    public void MissingBehaviorFieldFailsLoudly(string key)
    {
        var e = Assert.Throws<ContentException>(() => LoadMutated(root => Node(root, "behaviors.behaviors[0]").AsObject().Remove(key)));
        Assert.Contains(e.Errors, err => err.StartsWith("behaviors:") && err.Contains(key));
    }

    [Fact]
    public void MistypedBehaviorFlagFailsLoudly()
    {
        var e = Assert.Throws<ContentException>(() => LoadMutated(root => Node(root, "behaviors.behaviors[0]")["surrenders"] = "yes"));
        Assert.Contains(e.Errors, err => err.StartsWith("behaviors:"));
    }

    [Fact]
    public void MissingSecurityFailsLoudly()
    {
        var e = Assert.Throws<ContentException>(() => LoadMutated(root => Node(root, "core").AsObject().Remove("security")));
        Assert.Contains(e.Errors, err => err.StartsWith("core:") && err.Contains("security"));
    }

    [Fact]
    public void DuplicateIdFailsLoudly()
    {
        var e = Assert.Throws<ContentException>(() => LoadMutated(root => Replace(root, "behaviors.behaviors[1].id", "\"cowed\"")));
        Assert.Contains("behaviors: duplicate id 'cowed'", e.Errors);
    }

    [Fact]
    public void MissingRequiredFieldFailsLoudly()
    {
        var e = Assert.Throws<ContentException>(() => LoadMutated(root => Node(root, "troops.troops[0]").AsObject().Remove("weapon")));
        Assert.Contains(e.Errors, err => err.StartsWith("troops:") && err.Contains("weapon"));
    }

    [Fact]
    public void UnknownStatFailsLoudly()
    {
        var e = Assert.Throws<ContentException>(() => LoadMutated(root => Node(root, "troops.troops[0].stats")["luck"] = 2));
        Assert.Contains(e.Errors, err => err.StartsWith("troops:") && err.Contains("luck"));
    }

    [Fact]
    public void OutOfRangeStatFailsLoudly()
    {
        var e = Assert.Throws<ContentException>(() => LoadMutated(root => Node(root, "troops.troops[0].stats")["shoot"] = 7));
        Assert.Contains("troop tfb_vat_leader: shoot out of range (-1 to 4)", e.Errors);
    }

    private static ContentSet LoadMutated(Action<JsonNode> mutate)
    {
        var path = Path.Combine(FindRepoRoot(), "content", "dist", "content.json");
        var root = JsonNode.Parse(File.ReadAllText(path))!;
        mutate(root);
        return ContentSet.Parse(root.ToJsonString());
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (!File.Exists(Path.Combine(dir!.FullName, "content", "dist", "content.json"))) dir = dir.Parent;
        return dir.FullName;
    }

    // Path syntax: dotted keys with [n] indexes, e.g. "factions.elements[0].composition[0].troop".
    private static JsonNode Node(JsonNode root, string path)
    {
        var node = root;
        foreach (Match m in Regex.Matches(path, @"(\w+)|\[(\d+)\]"))
            node = (m.Groups[1].Success ? node[m.Groups[1].Value] : node[int.Parse(m.Groups[2].Value)])
                ?? throw new ArgumentException($"no node at '{m.Value}' in {path}");
        return node;
    }

    private static void Replace(JsonNode root, string path, string json) => Node(root, path).ReplaceWith(JsonNode.Parse(json));
}
