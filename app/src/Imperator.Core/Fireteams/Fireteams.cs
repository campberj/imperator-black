namespace Imperator.Core;

/// <summary>One selected model type and number in a fireteam.</summary>
public sealed record FireteamMember(string Troop, int Count);

/// <summary>A standard fireteam, whether the source calls it a team or an enemy element.</summary>
public sealed record FireteamEntry(string Id, string Name, string Faction, string Type, string Source,
    IReadOnlyList<FireteamMember> Models, IReadOnlyList<string> Findings,
    int? Tier = null, string? Behavior = null);

/// <summary>Chapter 7 pinning text. Null on Automated models, for which Suppression does not apply.</summary>
public sealed record TroopPinning(string Troop, string? Text);

public sealed record FireteamReference(IReadOnlyList<TroopPinning> Pinning, IReadOnlyList<FireteamEntry> Fireteams);

/// <summary>
/// Chapter 9 fireteam fit. One rule shared by validation, the reference API and the future force planner.
/// Uses the definitions in rosters.fireteam_types and each troop's class, not fixed numbers.
/// </summary>
public static class Fireteams
{
    public static IReadOnlyList<string> Check(ContentSet content, string? type, IEnumerable<FireteamMember> models)
    {
        var findings = new List<string>();
        var types = (content.Rosters.FireteamTypes ?? []).ToDictionary(t => t.Id);
        var classes = (content.Rosters.Classes ?? []).Select(c => c.Id).ToHashSet();
        var troopClasses = content.Troops.Troops.ToDictionary(t => t.Id, t => t.Class);
        var counts = classes.ToDictionary(c => c, _ => 0);
        foreach (var model in models)
            if (troopClasses.GetValueOrDefault(model.Troop) is { } cls && counts.ContainsKey(cls))
                counts[cls] += model.Count;
        int Count(string cls) => counts.GetValueOrDefault(cls);

        if (type == "character")
        {
            if (Count("character") != 1 || counts.Values.Sum() != 1)
                findings.Add("type character is one character acting alone");
            return findings;
        }
        if (type is null || !types.TryGetValue(type, out var definition))
        {
            findings.Add($"type must be one of [{string.Join(", ", types.Keys.Order())}] or character");
            return findings;
        }
        if (Count("character") > 1) findings.Add("at most one character");
        foreach (var (cls, range) in new[]
        {
            ("leader", definition.Leader),
            ("trooper", definition.Troopers),
            ("specialist", definition.Specialists),
        })
            if (Count(cls) < range[0] || Count(cls) > range[1])
                findings.Add($"{Count(cls)} {cls}(s), but a {type} fireteam takes {range[0]} to {range[1]}");
        if (Count("leader") + Count("trooper") + Count("specialist") > definition.MaxModels)
            findings.Add($"more than {definition.MaxModels} models (characters not counted)");
        return findings;
    }

    /// <summary>Use the same pinning calculation as an active Soldier, including gear-granted skills.</summary>
    public static string? PinningText(ContentSet content, string troopId)
    {
        var soldier = Soldier.FromTroop(content, troopId);
        if (soldier.Automated) return null;
        if (soldier.PinnedAt > (content.Core.Suppression?.Max ?? 5)) return "Never Pinned";
        return $"Pinned at {soldier.PinnedAt} Suppression";
    }

    /// <summary>Read-only API data. Standard fireteams are checked by the same method as authored content.</summary>
    public static FireteamReference Reference(ContentSet content)
    {
        var pinning = content.Troops.Troops.Select(t => new TroopPinning(t.Id, PinningText(content, t.Id))).ToList();
        var standard = new List<FireteamEntry>();
        foreach (var team in content.Rosters.Teams)
        {
            var models = team.Slots.Select(s => new FireteamMember(s.Troop, s.Count)).ToList();
            standard.Add(new FireteamEntry(team.Id, team.Name, team.Faction, team.Type ?? "",
                "team", models, Check(content, team.Type, models)));
        }
        foreach (var element in content.Factions.Elements)
        {
            var models = element.Composition.Select(s => new FireteamMember(s.Troop, s.Count)).ToList();
            standard.Add(new FireteamEntry(element.Id, element.Name, element.Faction, element.Type ?? "",
                "element", models, Check(content, element.Type, models), element.Tier, element.Behavior));
        }
        return new FireteamReference(pinning, standard);
    }
}
