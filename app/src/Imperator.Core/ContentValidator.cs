using Imperator.Core.Layouts;

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
        var elements = Ids(c.Factions.Elements, e => e.Id, "elements");
        var teams = Ids(c.Rosters.Teams, t => t.Id, "rosters.teams");
        Ids(c.Rosters.StartingForces, f => f.Id, "rosters.starting_forces");

        var weaponKeywords = c.Weapons.Ranged.Select(w => (w.Id, w.Keywords))
            .Concat(c.Weapons.Thrown.Select(w => (w.Id, w.Keywords)))
            .Concat(demolitions.Select(w => (w.Id, w.Keywords)));
        foreach (var (id, keywords) in weaponKeywords)
            foreach (var k in keywords) Need(k, kwWeapon, $"weapon {id} keyword");
        foreach (var a in c.Armour.Armour)
            foreach (var k in a.Keywords) Need(k, kwArmour, $"armour {a.Id} keyword");

        // Damage dice: a matched pair (2d4 to 2d12) or a half-step mixed pair (decision 6.14).
        string[] damageOk = ["2d4", "2d6", "2d8", "2d10", "2d12", "d6+d8", "d8+d10", "d10+d12"];
        var damages = c.Weapons.Ranged.Select(w => (w.Id, (string?)w.Damage))
            .Concat(c.Weapons.Melee.Select(w => (w.Id, (string?)w.Damage)))
            .Concat(c.Weapons.Thrown.Select(w => (w.Id, w.Damage)))
            .Concat(c.Weapons.Ranged.SelectMany(w => (w.Rounds ?? []).Select(r => ($"{w.Id} round {r.Id}", (string?)r.Damage))));
        foreach (var (id, dmg) in damages)
            if (dmg is not null && !damageOk.Contains(dmg))
                errors.Add($"weapon {id}: damage '{dmg}' is not a pair or a half-step (d6+d8, d8+d10, d10+d12)");
        foreach (var w in c.Weapons.Ranged)
        {
            if (w.Explosion is < 1 or > 6) errors.Add($"weapon {w.Id}: explosion must be a radius of 1 to 6 inches");
            foreach (var r in w.Rounds ?? [])
                if (r.Explosion is < 1 or > 6) errors.Add($"weapon {w.Id} round {r.Id}: explosion must be 1 to 6");
            if (w.Rounds is { Count: > 0 } && string.IsNullOrEmpty(w.Round))
                errors.Add($"weapon {w.Id}: a weapon with rounds names its own first round ('round')");
        }

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

        // Fireteams (decision 6.18): every troop has a class; standard fireteams (teams and elements) fit their type.
        var types = (c.Rosters.FireteamTypes ?? []).ToDictionary(t => t.Id);
        var classes = (c.Rosters.Classes ?? []).Select(x => x.Id).ToHashSet();
        var troopClass = c.Troops.Troops.ToDictionary(t => t.Id, t => t.Class);
        foreach (var t in c.Troops.Troops)
            if (t.Class is null || !classes.Contains(t.Class)) errors.Add($"troop {t.Id}: class must be one of [{string.Join(", ", classes.Order())}]");
        void CheckFireteam(string where, string? type, IEnumerable<(string Troop, int Count)> parts)
        {
            var result = Fireteams.Check(c, type, parts.Select(p => new FireteamMember(p.Troop, p.Count)));
            foreach (string finding in result) errors.Add($"{where}: {finding}");
        }
        foreach (var tm in c.Rosters.Teams) CheckFireteam($"team {tm.Id}", tm.Type, tm.Slots.Select(s => (s.Troop, s.Count)));
        foreach (var e in c.Factions.Elements)
        {
            CheckFireteam($"element {e.Id}", e.Type, e.Composition.Select(p => (p.Troop, p.Count)));
            foreach (var alt in e.Alternatives ?? [])
            {
                // An alternative replaces one model: of the same class if the element has one, else of its first part.
                var parts = e.Composition.Select(p => (p.Troop, p.Count)).ToList();
                int i = Math.Max(0, parts.FindIndex(p => troopClass.GetValueOrDefault(p.Troop) == troopClass.GetValueOrDefault(alt)));
                var swapped = parts.Select((p, k) => (p.Troop, k == i ? p.Count - 1 : p.Count)).Append((alt, 1));
                CheckFireteam($"element {e.Id} with {alt}", e.Type, swapped);
            }
        }
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
        // Generator footprints: every sector roll and fill lists its pieces ([] for open ground), each a known type that fits a sector.
        var terrainIds = land.Types.Select(t => t.Id).ToHashSet();
        foreach (var e in land.Environments)
        {
            var sets = new[] { ($"terrain environment {e.Id} fill", e.FillPieces) }
                .Concat(e.Table.Select(r => ($"terrain environment {e.Id} roll {r.Roll}", r.Pieces)));
            foreach (var (w, pieces) in sets)
            {
                if (pieces is null)
                {
                    errors.Add($"{w}: needs 'pieces' (the generator's footprint; [] for open ground)");
                    continue;
                }
                foreach (var pc in pieces)
                {
                    Need(pc.Type, terrainIds, $"{w} piece type");
                    if (!(pc.W is > 0 and <= 12 && pc.H is > 0 and <= 12)) errors.Add($"{w}: piece '{pc.Label}' must fit a 12\" sector");
                }
            }
            if (e.FillPieces is not { Count: > 0 }) errors.Add($"terrain environment {e.Id}: the fill must have cover");
        }
        foreach (var t in land.Types)
            foreach (var (k, text) in new[] { ("cover", t.Cover), ("move", t.Move), ("sight", t.Sight) })
                if (string.IsNullOrEmpty(text)) errors.Add($"terrain type {t.Id}: needs '{k}'");
        foreach (var t in b.Terms)
            if (string.IsNullOrEmpty(t.Term) || string.IsNullOrEmpty(t.Text)) errors.Add("behaviors.terms: each needs term and text");

        // Missions (chapter 13).
        var m = c.Missions;
        var missionIds = Ids(m.Missions, x => x.Id, "missions.missions");
        CoversD6(m.MissionRoll.Select(r => r.Roll), "missions.mission_roll");
        CoversD6(m.TierRoll.Select(r => r.Roll), "missions.tier_roll");
        CoversD6(m.Complications.Select(r => r.Roll), "missions.complications");
        foreach (var r in m.MissionRoll) Need(r.Mission, missionIds, "missions.mission_roll mission");
        foreach (var z in m.Clock.Sizes)
            if (z.Die is not ("d4" or "d6" or "d8") || z.Limit <= m.Clock.FirstCheckIn)
                errors.Add($"missions.clock size {z.Id}: needs a die (d4/d6/d8) and a whole-minute limit past the first check-in");
        var special = m.ContactResults.Select(x => x.Id).ToHashSet();
        Ids(m.Oppositions ?? [], x => x.Id, "missions.oppositions");
        foreach (var o in m.Oppositions ?? [])
            if (!o.Tiers.Select(t => t.Tier).Order().SequenceEqual([1, 2, 3, 4]))
                errors.Add($"missions.oppositions {o.Id}: needs tiers 1 to 4");
        var allTiers = m.Tiers.Select(t => ("", t))
            .Concat((m.Oppositions ?? []).SelectMany(o => o.Tiers.Select(t => (o.Id + " ", t))));
        foreach (var (pre, tier) in allTiers)
        {
            var w = $"mission {pre}tier {tier.Tier}";
            CoversD6(tier.Table.Select(r => r.Roll), w);
            foreach (var r in tier.Table)
                if (!special.Contains(r.Result)) Need(r.Result, elements, $"{w} contact result");
            Need(tier.Reinforcement, elements, $"{w} reinforcement");
            Need(tier.Hvt, troops, $"{w} hvt");
        }
        foreach (var r in m.TierRoll)
            if (m.Tiers.All(t => t.Tier != r.Tier)) errors.Add($"missions.tier_roll: tier {r.Tier} has no table");
        foreach (var x in m.Missions)
            foreach (var (k, text) in new[] { ("task", x.Task), ("lockdown", x.Lockdown), ("decisive", x.Decisive),
                         ("marginal", x.Marginal), ("failure", x.Failure), ("ship", x.Ship), ("land", x.Land) })
                if (string.IsNullOrEmpty(text)) errors.Add($"mission {x.Id}: needs '{k}'");

        // Campaign (chapter 14) and two-player missions (chapter 16): mirrors tools/validate_content.py.
        void CoversRolls(IEnumerable<D6Range> rolls, string where, int start, int end)
        {
            if (!rolls.SelectMany(r => r.Faces).Order().SequenceEqual(Enumerable.Range(start, end - start + 1)))
                errors.Add($"{where}: {(start == 1 ? "d6" : "2d6")} rows must cover {start}-{end} exactly once");
        }
        var cp = c.Campaign;
        CoversRolls(cp.Nodes.Select(n => n.Roll ?? new D6Range(0, 0)), "campaign.nodes", 1, 6);
        CoversRolls(cp.Advances.Table.Select(x => x.Roll), "campaign.advances.table", 1, 6);
        CoversRolls(cp.Injuries.Select(x => x.Roll), "campaign.injuries", 2, 12);
        CoversRolls(cp.Events.Select(x => x.Roll), "campaign.events", 2, 12);
        Ids(cp.Nodes.Concat(cp.SpecialNodes), n => n.Id, "campaign nodes");
        foreach (var n in cp.Nodes.Concat(cp.SpecialNodes))
        {
            Need(n.Mission, missionIds, $"campaign node {n.Id} mission");
            if (n.Vessel is not null) Need(n.Vessel, vesselIds, $"campaign node {n.Id} vessel");
            if (n.Environment is not null) Need(n.Environment, envIds, $"campaign node {n.Id} environment");
        }
        foreach (string sk in cp.Advances.Skills) Need(sk, skillIds, "campaign.advances.skills");
        Need(cp.Start.Force, c.Rosters.StartingForces.Select(x => x.Id).ToHashSet(), "campaign.start force");
        if (!cp.Standing.Select(x => x.Level).SequenceEqual(Enumerable.Range(0, 7)))
            errors.Add("campaign.standing: levels must run 0 to 6");
        if (cp.Arc.Columns.Sum(x => x.Nodes) != 6 || cp.Arc.Columns.Count(x => x.Target) != 1)
            errors.Add("campaign.arc: six nodes in all, one of them the target");
        Ids(cp.Support, x => x.Id, "campaign.support");
        Ids(cp.Bases.Kinds, x => x.Id, "campaign.bases.kinds");
        Ids(cp.Bases.Facilities, x => x.Id, "campaign.bases.facilities");
        foreach (var fa in cp.Bases.Facilities)
            if (fa.Levels.Count != 3) errors.Add($"campaign facility {fa.Id}: three levels");
        if (cp.Bases.Threat.AttackAt < 1 || cp.Bases.Threat.AttackAt > cp.Bases.Threat.Max)
            errors.Add("campaign.bases.threat: attack_at must be 1 to max");
        foreach (var (k, text) in new[] { ("decisive", cp.Bases.Defence.Grades.Decisive), ("marginal", cp.Bases.Defence.Grades.Marginal),
                     ("failure", cp.Bases.Defence.Grades.Failure) })
            if (string.IsNullOrEmpty(text)) errors.Add($"campaign.bases.defence: needs a {k} grade");
        if (!cp.Bases.Outcomes.Select(o => o.Result).SequenceEqual(["Decisive", "Marginal", "Failure"]))
            errors.Add("campaign.bases.outcomes: Decisive, Marginal and Failure, in order");
        foreach (var ev in cp.Events)
            if (string.IsNullOrEmpty(ev.Text)) errors.Add($"campaign event {ev.Roll}: needs text");

        var vs = c.Versus;
        var vsIds = Ids(vs.Missions, x => x.Id, "versus.missions");
        CoversRolls(vs.MissionRoll.Select(x => x.Roll), "versus.mission_roll", 1, 6);
        foreach (var roll in vs.MissionRoll) Need(roll.Mission, vsIds, "versus.mission_roll mission");
        var clockIds = m.Clock.Sizes.Select(x => x.Id).ToHashSet();
        foreach (var z in vs.Sizes) Need(z.Clock, clockIds, $"versus size {z.Id} clock");
        foreach (var mission in vs.Missions)
        {
            if (mission.Kind is not ("even" or "raid") || mission.Hidden is not ("none" or "blinds" or "defender"))
                errors.Add($"versus mission {mission.Id}: kind even/raid and hidden none/blinds/defender");
            if (mission.Kind == "raid" && mission.RaidType is not null &&
                !missionIds.Contains(mission.RaidType) && mission.RaidType != "recover_or_capture")
                errors.Add($"versus mission {mission.Id}: unknown raid_type");
        }

        // With everything else valid, build 200 decks per vessel: each must be connected and within the tile inventory.
        if (errors.Count == 0)
        {
            var owned = ly.Tiles.ToDictionary(t => t.Id, t => t.Owned);
            foreach (var v in ly.Vessels)
                for (int seed = 1; seed <= 200; seed++)
                {
                    var deck = DeckGenerator.Generate(c, v.Id, seed);
                    if (!deck.Connected || owned.Any(o => deck.Deck.TileCounts.GetValueOrDefault(o.Key) > o.Value))
                    {
                        errors.Add($"layout {v.Id} seed {seed}: disconnected or over the tile inventory");
                        break;
                    }
                }
        }

        return errors;
    }

    private static Dictionary<string, int?> Stats(TroopStats s) => new()
    {
        ["move"] = s.Move, ["shoot"] = s.Shoot, ["fight"] = s.Fight, ["physique"] = s.Physique,
        ["nerve"] = s.Nerve, ["tech"] = s.Tech, ["wounds"] = s.Wounds, ["size"] = s.Size,
    };
}
