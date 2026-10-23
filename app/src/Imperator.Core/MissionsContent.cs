namespace Imperator.Core;

// Missions (rules chapter 13), from content/missions.yaml: mission types, threat tiers and their contact
// tables, the alert track, complications, the mission clock, placement and contact rules, and the briefing.

public sealed record MissionRollRow(D6Range Roll, string Mission);

/// <summary>
/// A mission type. Objectives is how many objective markers it places; Ship and Land say where. The rules text
/// fields default to empty so ContentValidator reports a missing one as tools/validate_content.py does.
/// </summary>
public sealed record MissionDef(string Id, string Name, string Summary, int Objectives,
    string Ship = "", string Land = "", string Task = "", string Lockdown = "",
    string Decisive = "", string Marginal = "", string Failure = "");

public sealed record TierRollRow(D6Range Roll, int Tier);

/// <summary>A contact table row: an element id, or a special result (false_alarm, booby_trap).</summary>
public sealed record ContactRollRow(D6Range Roll, string Result);

/// <summary>
/// A threat tier: how many hidden contacts to place, its d6 contact table (also used for the guard), the element
/// that answers alert 4, and the troop used as a capture mission's high-value target.
/// </summary>
public sealed record TierDef(int Tier, int Contacts, string Reinforcement, string Hvt, List<ContactRollRow> Table);

/// <summary>Another enemy faction's threat tiers (tiers 1 to 4, as the default tiers). A mission names it to use it.</summary>
public sealed record OppositionDef(string Id, string Name, string Summary, List<TierDef> Tiers);

/// <summary>A contact table result that is not an element (false alarm, booby trap).</summary>
public sealed record ContactResultDef(string Id, string Name, string Text);

public sealed record AlertRaiseDef(string Source, int Amount);

public sealed record AlertThresholdDef(int At, string Effect);

public sealed record AlertDef(int Max, List<AlertRaiseDef> Raises, List<AlertThresholdDef> Thresholds);

public sealed record ComplicationDef(D6Range Roll, string Id, string Name, string Text);

/// <summary>A mission size: the clock die rolled at the end of each round ("d4") and the time limit in minutes.</summary>
public sealed record ClockSizeDef(string Id, string Name, string Force, string Die, int Limit)
{
    /// <summary>The clock die as dice ("d4" is one four-sided die).</summary>
    public DiceSpec DieSpec => DiceSpec.Parse("1" + Die);
}

/// <summary>The mission clock: sizes, and check-ins every CheckInEvery minutes from FirstCheckIn.</summary>
public sealed record ClockDef(List<ClockSizeDef> Sizes, int CheckInEvery, int FirstCheckIn, List<string> Rules);

public sealed record BriefingHeadingDef(string Heading, string Text);

public sealed record MissionsContent(
    List<MissionRollRow> MissionRoll, List<MissionDef> Missions, List<TierRollRow> TierRoll, List<TierDef> Tiers,
    List<ContactResultDef> ContactResults, AlertDef Alert, List<ComplicationDef> Complications, ClockDef Clock,
    List<string> Placement, List<string> Contacts, List<BriefingHeadingDef> Briefing,
    List<OppositionDef>? Oppositions = null);
