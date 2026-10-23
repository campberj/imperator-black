using Imperator.Core.Values;

namespace Imperator.Core.Forces;

/// <summary>All force totals and chapter 9/14/16 checks live here, never in the browser.</summary>
public sealed class ForceCheck(ContentSet content)
{
    private readonly TroopValueCalculator _values = new(content);
    private readonly Dictionary<string, TroopDef> _troops = content.Troops.Troops.ToDictionary(t => t.Id);
    private readonly Dictionary<string, TeamDef> _teams = content.Rosters.Teams.ToDictionary(t => t.Id);
    private readonly Dictionary<string, ElementDef> _elements = content.Factions.Elements.ToDictionary(e => e.Id);

    public ForceResult Check(ForcePlan plan)
    {
        var findings = new List<ForceFinding>();
        var teamTotals = new List<ForceTeamTotal>();
        var characterTotals = new List<ForceCharacterTotal>();
        var troopValues = new Dictionary<string, int>();
        int tv = 0, models = 0, teamCount = 0;
        void Warn(string code, string? subject, string message) => findings.Add(new(code, subject, message));
        bool Valid(string id)
        {
            if (!_troops.TryGetValue(id, out var troop))
            {
                Warn("wrong_faction", id, $"Unknown troop '{id}'.");
                return false;
            }
            if (troop.Faction != plan.Faction)
                Warn("wrong_faction", id, $"{troop.Name} is not a {content.Factions.Factions.FirstOrDefault(f => f.Id == plan.Faction)?.Name ?? plan.Faction} troop.");
            return true;
        }
        int Value(string id)
        {
            if (!Valid(id)) return 0;
            if (!troopValues.TryGetValue(id, out int price))
                troopValues[id] = price = _values.Calculate(id).Tv;
            return price;
        }

        bool byFireteam = plan.Setting != "soldier";
        if (plan.Source == "versus" && !byFireteam)
            Warn("setting", null, "Two-player forces must be selected by fireteam.");

        if (byFireteam)
        {
            foreach (var team in plan.Fireteams)
            {
                int teamTv = 0, teamModels = 0;
                foreach (var slot in team.Slots)
                {
                    if (slot.Count < 0) { Warn("slot_limits", team.Id, $"{team.Name}: a slot cannot have fewer than 0 models."); continue; }
                    int cost = Value(slot.Troop);
                    teamTv += cost * slot.Count;
                    teamModels += slot.Count;
                }
                tv += teamTv;
                models += teamModels;
                teamCount++;
                teamTotals.Add(new(team.Id, teamTv, teamModels));

                var joined = plan.Characters.Where(c => c.Joins == team.Id).ToList();
                if (joined.Count > 1)
                    Warn("two_characters", team.Id, $"{team.Name}: {joined.Count} characters; at most one.");

                // The joined Leader character fills the leader role for fitting a leaderless standard team.
                bool joinedLeader = joined.Any(c => _troops.GetValueOrDefault(c.Troop)?.Skills.ContainsKey("leader") == true);
                var members = team.Slots.Where(s => s.Count > 0).Select(s => new FireteamMember(s.Troop, s.Count)).ToList();
                if (joinedLeader && !members.Any(m => _troops.GetValueOrDefault(m.Troop)?.Class == "leader"))
                    members.Add(new FireteamMember(joined.First(c => _troops.GetValueOrDefault(c.Troop)?.Skills.ContainsKey("leader") == true).Troop, 1));

                foreach (var issue in Fireteams.Check(content, team.Type, members))
                {
                    // Character fit is checked explicitly above, with a distinct finding code.
                    if (issue == "at most one character") continue;
                    Warn("type_limits", team.Id, $"{team.Name}: {issue}.");
                }

                var standard = team.Standard is null ? null
                    : _teams.GetValueOrDefault(team.Standard) as object ?? _elements.GetValueOrDefault(team.Standard);
                var original = standard switch
                {
                    TeamDef t => t.Slots.Select(x => (x.Troop, x.Count)).ToList(),
                    ElementDef e => e.Composition.Select(x => (x.Troop, x.Count)).ToList(),
                    _ => null,
                };
                if (original is not null)
                {
                    int slots = original.Sum(x => x.Count);
                    int minimum = (slots + 1) / 2;
                    if (teamModels < minimum)
                        Warn("half_slots", team.Id, $"{team.Name}: {teamModels} of {slots} slots; at least {minimum}.");
                    bool originalHasLeader = original.Any(x => _troops.GetValueOrDefault(x.Troop)?.Class == "leader");
                    bool chosenLeader = team.Slots.Any(x => x.Count > 0 && _troops.GetValueOrDefault(x.Troop)?.Class == "leader");
                    if (originalHasLeader && !chosenLeader && !joinedLeader)
                        Warn("no_leader", team.Id, $"{team.Name}: its Leader must be fielded.");
                    foreach (var slot in team.Slots)
                    {
                        int max = original.Where(x => x.Troop == slot.Troop).Sum(x => x.Count);
                        if (slot.Count > max)
                            Warn("slot_limits", team.Id, $"{team.Name}: {slot.Count} {slot.Troop}; at most {max} in the standard fireteam.");
                    }
                }
            }
        }
        else
        {
            foreach (var soldier in plan.Soldiers)
            {
                tv += Value(soldier.Troop);
                models++;
            }
            foreach (var group in plan.Soldiers.Where(s => s.Group > 0).GroupBy(s => s.Group))
            {
                teamCount++;
                int count = group.Count() + plan.Characters.Count(c => c.Joins == group.Key.ToString());
                if (count > 6) Warn("group_size", group.Key.ToString(), $"Group {group.Key}: {count} models; at most 6.");
            }
            foreach (var chars in plan.Characters.Where(c => c.Joins is not null).GroupBy(c => c.Joins))
            {
                if (chars.Count() > 1)
                    Warn("two_characters", chars.Key, $"Group {chars.Key}: {chars.Count()} characters; at most one.");
            }
        }

        for (int i = 0; i < plan.Characters.Count; i++)
        {
            var c = plan.Characters[i];
            int value = Value(c.Troop);
            tv += value;
            models++;
            characterTotals.Add(new(i, value));
            if (c.Joins is null) teamCount++; // A lone character is a fireteam of one.
            if (_troops.GetValueOrDefault(c.Troop)?.Class is not ("character" or null))
                Warn("type_limits", c.Troop, $"{_troops[c.Troop].Name} is not a character.");
            if (c.Joins is not null && (byFireteam
                    ? plan.Fireteams.All(t => t.Id != c.Joins)
                    : !int.TryParse(c.Joins, out int group) || plan.Soldiers.All(s => s.Group != group)))
                Warn("group_size", c.Troop, $"{_troops.GetValueOrDefault(c.Troop)?.Name ?? c.Troop}: its joined fireteam does not exist.");
        }

        if (plan.TvLimit is { } maxTv && tv > maxTv)
            Warn("over_tv", null, $"{tv} TV: {tv - maxTv} over the {maxTv} limit.");
        if (plan.ModelLimit is { } maxModels && models > maxModels)
            Warn("over_models", null, $"{models} models: {models - maxModels} over the limit of {maxModels}.");

        return new(tv, models, teamCount, plan.Characters.Count, findings, teamTotals, characterTotals, troopValues);
    }
}
