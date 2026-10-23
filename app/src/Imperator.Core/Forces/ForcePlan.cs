namespace Imperator.Core.Forces;

/// <summary>One named force; the web serialises this record into the URL hash. No database state.</summary>
public sealed record ForcePlan(
    string Name, string Faction, string Setting, string Source, string? SourceId,
    int? TvLimit, int? ModelLimit,
    List<ForceTeam> Fireteams, List<ForceCharacter> Characters, List<ForceSoldier> Soldiers);

/// <summary>Named instance allows more than one copy of the same standard fireteam.</summary>
public sealed record ForceTeam(string Id, string Name, string Type, string? Standard,
    List<ForceSlot> Slots);
public sealed record ForceSlot(string Troop, int Count, string? Weapon = null, string? Armour = null,
    List<string>? Gear = null);
/// <summary>Joins is a fireteam ID, or the group number (as text) for by-soldier; null means alone.</summary>
public sealed record ForceCharacter(string Troop, string? Joins = null, string? Weapon = null,
    string? Armour = null, List<string>? Gear = null);
public sealed record ForceSoldier(string Troop, int Group, string? Weapon = null, string? Armour = null,
    List<string>? Gear = null);

public sealed record ForceFinding(string Code, string? Subject, string Message);
public sealed record ForceTeamTotal(string Id, int Tv, int Models);
public sealed record ForceCharacterTotal(int Index, int Tv);
public sealed record ForceResult(int Tv, int Models, int Fireteams, int Characters,
    IReadOnlyList<ForceFinding> Findings, IReadOnlyList<ForceTeamTotal> TeamTotals,
    IReadOnlyList<ForceCharacterTotal> CharacterTotals,
    IReadOnlyDictionary<string, int> TroopValues);
