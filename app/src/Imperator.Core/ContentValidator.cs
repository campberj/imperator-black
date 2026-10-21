namespace Imperator.Core;

/// <summary>
/// Cross-reference checks run at load time: every referenced id exists and ids are unique.
/// Mirrors tools/validate_content.py, with the same error messages; keep the two in step.
/// </summary>
public static class ContentValidator
{
    public static IReadOnlyList<string> Validate(ContentSet c)
    {
        var errors = new List<string>();

        HashSet<string> Ids<T>(IEnumerable<T> items, Func<T, string> id, string where)
        {
            var seen = new HashSet<string>();
            foreach (var it in items)
                if (!seen.Add(id(it))) errors.Add($"{where}: duplicate id '{id(it)}'");
            return seen;
        }

        void Need(string value, IReadOnlySet<string> pool, string where)
        {
            if (!pool.Contains(value)) errors.Add($"{where}: unknown id '{value}'");
        }

        var demolitions = c.Weapons.Demolitions ?? [];
        var ranged = Ids(c.Weapons.Ranged, w => w.Id, "weapons.ranged");
        var melee = Ids(c.Weapons.Melee, w => w.Id, "weapons.melee");
        var thrown = Ids(c.Weapons.Thrown, w => w.Id, "weapons.thrown");
        Ids(demolitions, w => w.Id, "weapons.demolitions");
        var armour = Ids(c.Armour.Armour, a => a.Id, "armour");
        var kwWeapon = Ids(c.Keywords.Weapon, k => k.Id, "keywords.weapon");
        var kwArmour = Ids(c.Keywords.Armour, k => k.Id, "keywords.armour");
        var skillIds = Ids(c.Skills.Skills, s => s.Id, "skills");
        var skills = c.Skills.Skills.DistinctBy(s => s.Id).ToDictionary(s => s.Id);

        void Level(string skill, int lvl, string where)
        {
            if (skills.TryGetValue(skill, out var def) && !(1 <= lvl && lvl <= def.Levels.Count))
                errors.Add($"{where}: {skill} level {lvl} out of range");
        }

        var ranks = Ids(c.Core.Ranks, r => r.Id, "core.ranks");
        var attrs = c.Core.Attributes.Select(a => a.Id).ToList();
        var gear = Ids(c.Gear.Gear, g => g.Id, "gear");
        var behaviors = Ids(c.Behaviors.Behaviors, b => b.Id, "behaviors");
        var factions = Ids(c.Factions.Factions, f => f.Id, "factions");
        var troops = Ids(c.Troops.Troops, t => t.Id, "troops");
        Ids(c.Factions.Elements, e => e.Id, "elements");
        var teams = Ids(c.Rosters.Teams, t => t.Id, "rosters.teams");
        Ids(c.Rosters.StartingForces, f => f.Id, "rosters.starting_forces");

        var weaponKeywords = c.Weapons.Ranged.Select(w => (w.Id, w.Keywords))
            .Concat(c.Weapons.Thrown.Select(w => (w.Id, w.Keywords)))
            .Concat(demolitions.Select(w => (w.Id, w.Keywords)));
        foreach (var (id, keywords) in weaponKeywords)
            foreach (var k in keywords) Need(k, kwWeapon, $"weapon {id} keyword");
        foreach (var a in c.Armour.Armour)
            foreach (var k in a.Keywords) Need(k, kwArmour, $"armour {a.Id} keyword");

        foreach (var g in c.Gear.Gear)
        {
            foreach (var (k, lvl) in g.GrantsSkills ?? [])
            {
                Need(k, skillIds, $"gear {g.Id} grants");
                Level(k, lvl, $"gear {g.Id}");
            }
            foreach (var k in g.WeaponKeywords ?? []) Need(k, kwWeapon, $"gear {g.Id} weapon keyword");
            if (g.Melee is not null) Need(g.Melee, melee, $"gear {g.Id} melee");
            if (g.Status is not ("settled" or "proposed")) errors.Add($"gear {g.Id}: status must be settled or proposed");
        }

        foreach (var t in c.Troops.Troops)
        {
            var w = $"troop {t.Id}";
            Need(t.Faction, factions, $"{w} faction");
            Need(t.Rank, ranks, $"{w} rank");
            var stats = Stats(t.Stats);
            if (!stats.Keys.Order().SequenceEqual(attrs.Order()))
                errors.Add($"{w}: stats must be exactly [{string.Join(", ", attrs)}]");
            bool automated = t.Skills.ContainsKey("automated");
            foreach (var (a, val) in stats)
            {
                if (val is null)
                {
                    if (!automated || a is "move" or "shoot" or "wounds" or "size")
                        errors.Add($"{w}: {a} may be null only on Automated models (and never move/shoot/wounds/size)");
                }
                else if (a == "move" && val is not (>= 0 and <= 10)) errors.Add($"{w}: move out of range");
                else if (a is "wounds" or "size" && val is not (>= 1 and <= 6)) errors.Add($"{w}: {a} out of range");
                else if (a is not ("move" or "wounds" or "size") && val is not (>= -1 and <= 4))
                    errors.Add($"{w}: {a} out of range (-1 to 4)");
            }
            Need(t.Armour, armour, $"{w} armour");
            foreach (var a in t.ArmourOptions ?? []) Need(a, armour, $"{w} armour option");
            if (t.Weapon is not null) Need(t.Weapon, ranged, $"{w} weapon");
            else if (t.Melee is null) errors.Add($"{w}: a model with no ranged weapon needs a melee weapon");
            foreach (var o in t.WeaponOptions ?? []) Need(o, ranged, $"{w} weapon option");
            if (t.Melee is not null) Need(t.Melee, melee, $"{w} melee");
            else if (!automated) errors.Add($"{w}: only Automated models may have no melee weapon");
            foreach (var g in t.Grenades) Need(g, thrown, $"{w} grenade");
            if (t.Grenades.Count > 2) errors.Add($"{w}: carries more than two grenades");
            foreach (var g in t.Gear.Concat(t.GearOptions ?? [])) Need(g, gear, $"{w} gear");
            foreach (var (k, lvl) in t.Skills)
            {
                Need(k, skillIds, $"{w} skill");
                Level(k, lvl, w);
            }
        }

        foreach (var e in c.Factions.Elements)
        {
            var w = $"element {e.Id}";
            Need(e.Faction, factions, $"{w} faction");
            Need(e.Behavior, behaviors, $"{w} behavior");
            if (e.Tier is not (>= 1 and <= 4)) errors.Add($"{w}: tier must be 1-4");
            foreach (var part in e.Composition) Need(part.Troop, troops, $"{w} troop");
            foreach (var alt in e.Alternatives ?? []) Need(alt, troops, $"{w} alternative");
        }

        var b = c.Behaviors;
        foreach (var bh in b.Behaviors)
        {
            var w = $"behavior {bh.Id}";
            foreach (var (field, list) in new[] { ("engaged", bh.Engaged), ("alerted", bh.Alerted), ("reactions", bh.Reactions) })
                if (list.Count == 0) errors.Add($"{w}: needs a non-empty '{field}' list");
            // close_combat, investigate and summary text, and the surrenders and close_quarters booleans,
            // are required by the deserializer, which rejects a missing or mistyped value.
            if (bh.Status is not ("settled" or "proposed")) errors.Add($"{w}: status must be settled or proposed");
        }
        foreach (var r in ranks)
            if (b.Drills.Count(d => d.Rank == r) != 1) errors.Add($"behaviors.drills: rank '{r}' needs exactly one drill");
        Ids(b.Drills, d => d.Id, "behaviors.drills");
        Ids(b.Noise, n => n.Id, "behaviors.noise");
        Ids(b.Awareness, a => a.Id, "behaviors.awareness");
        foreach (var (section, list) in new[]
        {
            ("marker", b.Marker), ("alarm", b.Alarm), ("activation_order", b.ActivationOrder), ("targets", b.Targets),
            ("moving", b.Moving), ("close_quarters", b.CloseQuarters), ("surrender", b.Surrender), ("tie_breakers", b.TieBreakers),
        })
            if (list.Count == 0) errors.Add($"behaviors.{section}: missing or empty");

        Ids(c.Core.Security.Levels, l => l.Id, "core.security");
        foreach (var lv in c.Core.Security.Levels)
            if (lv.Target is not (>= 2 and <= 12) || lv.Successes < 1)
                errors.Add($"security level {lv.Id}: target 2-12 and successes 1+");

        foreach (var tm in c.Rosters.Teams)
            foreach (var s in tm.Slots) Need(s.Troop, troops, $"team {tm.Id} slot");
        foreach (var sf in c.Rosters.StartingForces)
        {
            foreach (var tm in sf.Teams) Need(tm, teams, $"starting force {sf.Id} team");
            foreach (var tr in sf.Attachments?.From ?? []) Need(tr, troops, $"starting force {sf.Id} attachment");
        }

        foreach (var s in c.Skills.Skills)
            if (s.Status is not ("settled" or "proposed")) errors.Add($"skill {s.Id}: status must be settled or proposed");

        void CoversD6(IEnumerable<D6Range> rolls, string where)
        {
            if (!rolls.SelectMany(r => r.Faces).Order().SequenceEqual([1, 2, 3, 4, 5, 6]))
                errors.Add($"{where}: d6 rows must cover 1-6 exactly once");
        }

        // Layouts (chapter 11).
        var ly = c.Layouts;
        var roomIds = Ids(ly.Rooms, r => r.Id, "layouts.rooms");
        var secIds = c.Core.Security.Levels.Select(l => l.Id).ToHashSet();
        var hazIds = Ids(ly.Hazards, h => h.Id, "layouts.hazards");
        var vesselIds = Ids(ly.Vessels, v => v.Id, "layouts.vessels");
        foreach (var (table, rows) in new[]
        {
            ("spine_turn", ly.SpineTurn), ("passage_branch", ly.PassageBranch), ("junction_branch", ly.JunctionBranch),
            ("loop_door", ly.LoopDoor), ("door_state", ly.DoorState), ("hazard_roll", ly.HazardRoll),
        })
            CoversD6(rows.Select(r => r.Roll), $"layouts.{table}");
        CoversD6(ly.SmallRooms.Select(r => r.Roll), "layouts.small_rooms");
        CoversD6(ly.VesselRoll.Select(r => r.Roll), "layouts.vessel_roll");
        foreach (var r in ly.SmallRooms) Need(r.Room, roomIds, "layouts.small_rooms room");
        foreach (var r in ly.VesselRoll) Need(r.Vessel, vesselIds, "layouts.vessel_roll vessel");
        foreach (var r in ly.Rooms)
        {
            if (!string.IsNullOrEmpty(r.Security)) Need(r.Security, secIds, $"room {r.Id} security");
            foreach (var cover in r.Cover)
                if (cover.Type is not ("light" or "heavy")) errors.Add($"room {r.Id}: cover type must be light or heavy");
        }
        foreach (var v in ly.Vessels)
        {
            var w = $"vessel {v.Id}";
            CoversD6(v.Rooms.Select(r => r.Roll), $"{w} rooms");
            foreach (var r in v.Rooms) Need(r.Room, roomIds, $"{w} room");
            Need(v.Heart, roomIds, $"{w} heart");
            Need(v.Security, secIds, $"{w} security");
            Need(v.HeartSecurity, secIds, $"{w} heart_security");
            foreach (var h in v.Hazards) Need(h, hazIds, $"{w} hazard");
            if (v.Board.Any(x => x % 3 != 0)) errors.Add($"{w}: board must be in multiples of 3 inches");
        }

        // Land terrain (chapter 12).
        var land = c.Terrain;
        Ids(land.Types, t => t.Id, "terrain.types");
        var envIds = Ids(land.Environments, e => e.Id, "terrain.environments");
        foreach (var e in land.Environments) CoversD6(e.Table.Select(r => r.Roll), $"terrain environment {e.Id}");
        CoversD6(land.EnvironmentRoll.Select(r => r.Roll), "terrain.environment_roll");
        CoversD6(land.Conditions.Select(r => r.Roll), "terrain.conditions");
        foreach (var r in land.EnvironmentRoll) Need(r.Environment, envIds, "terrain.environment_roll environment");
        foreach (var t in land.Types)
            foreach (var (k, text) in new[] { ("cover", t.Cover), ("move", t.Move), ("sight", t.Sight) })
                if (string.IsNullOrEmpty(text)) errors.Add($"terrain type {t.Id}: needs '{k}'");
        foreach (var t in b.Terms)
            if (string.IsNullOrEmpty(t.Term) || string.IsNullOrEmpty(t.Text)) errors.Add("behaviors.terms: each needs term and text");

        // Not mirrored: validate_content.py also builds 200 decks per vessel with tools/generate_layout.py and checks
        // each is connected and within the tile inventory. That needs the generator, which is not yet ported to Core.

        return errors;
    }

    private static Dictionary<string, int?> Stats(TroopStats s) => new()
    {
        ["move"] = s.Move, ["shoot"] = s.Shoot, ["fight"] = s.Fight, ["physique"] = s.Physique,
        ["nerve"] = s.Nerve, ["tech"] = s.Tech, ["wounds"] = s.Wounds, ["size"] = s.Size,
    };
}
