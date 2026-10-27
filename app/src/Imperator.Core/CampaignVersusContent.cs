using System.Text.Json;

namespace Imperator.Core;

// Chapter 14: every part of content/campaign.yaml, including the short-arc and force-selection rules.
public sealed record CampaignStartDef(string Force, int RosterMax, int Requisition, int Intel,
    int Standing, int Evidence, List<string> Text);
public sealed record ForceSelectionDef(string Id, string Name, bool Default, string Deploy, string Replace);
public sealed record CampaignTurnStepDef(string Step, string Text);
public sealed record CampaignResourceDef(string Id, string Name, int? Max, string Text);
public sealed record CampaignStandingDef(int Level, string Name, int Income, int Support, string Text);
public sealed record CampaignSupportDef(string Id, string Name, string Text);
public sealed record CampaignIntelUseDef(int Cost, string Name, string Text);
public sealed record CampaignArcColumnDef(int Column, int Nodes, int Tier, bool Revealed, bool Target = false);
public sealed record CampaignArcDef(int Turns, List<CampaignArcColumnDef> Columns, List<string> Procedure,
    List<string> Taking, List<string> End);
public sealed record CampaignNodeDef(string Id, string Name, string Setting, string Mission, string Reward,
    D6Range? Roll = null, string? Vessel = null, string? Environment = null);
public sealed record CampaignRewardDef(string Result, string Text);
public sealed record CampaignInjuryModifierDef(int Value, string Text);
public sealed record CampaignInjuryDef(D6Range Roll, string Id, string Name, int? Out, string Text);
public sealed record CampaignXpDef(int Amount, string Text);
public sealed record CampaignAdvanceRowDef(D6Range Roll, string Text);
public sealed record CampaignAdvancesDef(int Every, List<string> Text, List<CampaignAdvanceRowDef> Table,
    List<string> Skills);
public sealed record CampaignHonourDef(string Name, string Text);
public sealed record CampaignHonourListDef(string Id, string Name, string Text);
// The base (decision 7.14): an outpost or a ship, its facilities, the threat track, attacks, defence and recapture.
public sealed record CampaignBaseKindDef(string Id, string Name, string Text);
public sealed record CampaignFacilityLevelDef(int Cost, string Text);
public sealed record CampaignFacilityDef(string Id, string Name, List<CampaignFacilityLevelDef> Levels);
public sealed record CampaignThreatChangeDef(string Source, int Amount);
public sealed record CampaignThreatDef(int Max, int AttackAt, List<CampaignThreatChangeDef> Raises, List<CampaignThreatChangeDef> Lowers);
public sealed record CampaignDefenceGradesDef(string Decisive, string Marginal, string Failure);
public sealed record CampaignDefenceDef(List<string> Board, List<string> Deploy, List<string> Enemy, string Losing,
    CampaignDefenceGradesDef Grades);
public sealed record CampaignBaseOutcomeDef(string Result, string Text);
public sealed record CampaignBasesDef(List<CampaignBaseKindDef> Kinds, List<string> Rules, List<CampaignFacilityDef> Facilities,
    CampaignThreatDef Threat, List<string> Attacks, CampaignDefenceDef Defence, List<CampaignBaseOutcomeDef> Outcomes,
    List<string> Recapture);
// Costs are authored as either an integer or explanatory text (e.g. Requisition by troop TV).
public sealed record CampaignCostDef(string Item, JsonElement Cost);
public sealed record CampaignEventDef(D6Range Roll, string Name, string Text);
public sealed record CampaignShortArcDef(int Missions, List<string> Text);

public sealed record CampaignContent(
    string Version,
    CampaignStartDef Start,
    List<ForceSelectionDef> ForceSelection,
    List<CampaignTurnStepDef> Turn,
    List<CampaignResourceDef> Resources,
    List<CampaignStandingDef> Standing,
    List<CampaignSupportDef> Support,
    List<CampaignIntelUseDef> IntelUses,
    CampaignArcDef Arc,
    List<CampaignNodeDef> Nodes,
    List<CampaignNodeDef> SpecialNodes,
    List<CampaignRewardDef> Rewards,
    List<string> Casualties,
    List<CampaignInjuryModifierDef> InjuryModifiers,
    List<CampaignInjuryDef> Injuries,
    List<CampaignXpDef> Xp,
    CampaignAdvancesDef Advances,
    List<CampaignHonourDef> Honours,
    List<CampaignHonourListDef> HonourList,
    CampaignBasesDef Bases,
    List<CampaignCostDef> Costs,
    List<CampaignEventDef> Events,
    List<string> Evidence,
    CampaignShortArcDef ShortArc);

// Chapter 16: every part of content/versus.yaml. Text stays content, not logic in the app.
public sealed record VersusSizeDef(string Id, string Name, int Tv, string Models, string Board, string Clock);
public sealed record VersusMissionRollDef(D6Range Roll, string Mission);
public sealed record VersusMissionDef(string Id, string Kind, string Name, string Summary,
    string Hidden, string HiddenText, string? Deploy = null, string? Scoring = null,
    string? RaidType = null);
public sealed record VersusRaidSecretDef(string Mission, string Text);
public sealed record VersusRaidDef(List<string> Setup, List<VersusRaidSecretDef> Secrets,
    List<string> Alarm, List<string> Alert, List<string> Grades);
public sealed record VersusBlindDecoysDef(int Even, int Defender);
public sealed record VersusBlindsDef(VersusBlindDecoysDef Decoys, List<string> Rules);

public sealed record VersusContent(
    string Version,
    List<VersusSizeDef> Sizes,
    List<string> Forces,
    List<VersusMissionRollDef> MissionRoll,
    List<VersusMissionDef> Missions,
    VersusRaidDef Raid,
    VersusBlindsDef Blinds,
    List<string> CasualtyPoints,
    List<string> End,
    List<string> EvenGrades,
    List<string> Changes);
