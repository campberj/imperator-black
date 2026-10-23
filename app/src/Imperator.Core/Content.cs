using System.Text.Json;
using System.Text.Json.Serialization;

namespace Imperator.Core;

// Game content, compiled from content/*.yaml by tools/build_content.py into content/dist/content.json.
// Positional parameters without a default are required in the JSON; ContentValidator checks cross-references.

public sealed record RankDef(string Id, string Name, string Typical, string Text);

public sealed record AttributeDef(string Id, string Name, string Short, string Text);

public sealed record WoundRowDef(int Row, int Min, int Max, string Id, string Text);

public sealed record HitModifierDef(string Id, int Value, string Text);

public sealed record AutoFireDef(int Zone, int ExtraTargetModifier);

/// <summary>What a Shoot value means: a unit's class sets its baseline Shoot (decision 3.21).</summary>
public sealed record ShootScaleDef(string Value, string Text);

/// <summary>A hacking security level: Interact, 2d6 + Tech against Target, needing Successes successes in all.</summary>
public sealed record SecurityLevelDef(string Id, string Name, int Target, int Successes, string Examples);

/// <summary>Hacking security (chapter 3). Each failed attempt on a target gives +WorkingBonusPerFailure later, up to WorkingBonusMax.</summary>
public sealed record SecurityDef(int WorkingBonusPerFailure, int WorkingBonusMax, List<SecurityLevelDef> Levels);

public sealed record CoreContent(
    string Version,
    int HitTarget,
    int TestTarget,
    int NaturalMiss,
    List<RankDef> Ranks,
    List<AttributeDef> Attributes,
    List<HitModifierDef> HitModifiers,
    List<WoundRowDef> WoundTable,
    SecurityDef Security,
    AutoFireDef? AutoFire = null,
    List<ShootScaleDef>? ShootScale = null,
    int OverwatchReactions = 1,
    SuppressiveFireDef? SuppressiveFire = null,
    SuppressionDef? Suppression = null);

/// <summary>
/// Suppression (rules chapter 7): the cap, the Pinned threshold and where Overwatch is lost. With PinnedByNerve set,
/// a model is Pinned at its Nerve (times PinnedNerveFactor) plus that many markers (never fewer than PinnedAtLeast) instead of a flat PinnedAt
/// (Automated models never gain any).
/// </summary>
public sealed record SuppressionDef(int ModifierPerMarker = -1, int Max = 5, int PinnedAt = 3, int CancelsOverwatchAt = 2, int? PinnedByNerve = null,
    int PinnedAtLeast = 1, int PinnedNerveFactor = 1);

/// <summary>Suppressive Fire (rules chapter 4): ignores cover, 2 Suppression on a strong hit, and a hit stops reactions.</summary>
public sealed record SuppressiveFireDef(bool IgnoresCover = true, int StrongMargin = 3, bool RemovesReactions = true);

/// <summary>A launcher's extra round (rules chapter 6): chosen before each Shoot instead of the weapon's own round.</summary>
public sealed record RoundDef(string Id, string Name, string Damage, int Ap, int? Explosion = null, List<string>? Keywords = null)
{
    public DiceSpec DamageDice => DiceSpec.Parse(Damage);
}

/// <summary>
/// A ranged weapon. Explosion is the X in Explosion (X); Round names the weapon's own round when it has other
/// Rounds (launchers: anti-armour, high explosive).
/// </summary>
public sealed record RangedWeapon(string Id, string Name, string Damage, int ShortRange, int Ap, List<string> Keywords,
    List<int>? RangePenalty = null, int? Auto = null, int? Explosion = null, string? Round = null, List<RoundDef>? Rounds = null)
{
    /// <summary>Rate of fire: how many different models one Shoot or Suppressive Fire may engage (Auto N).</summary>
    public int Rate => Has("auto") ? Math.Max(1, Auto ?? 1) : 1;
    public DiceSpec DamageDice => DiceSpec.Parse(Damage);
    public bool Has(string keyword) => Keywords.Contains(keyword);
    public int PenaltyBeyondShort => RangePenalty is { Count: > 0 } p ? p[0] : -1;
    public int PenaltyBeyondDouble => RangePenalty is { Count: > 1 } p ? p[1] : -2;

    /// <summary>Stand-in for a model with no ranged weapon (for example a Jedi with only a lightsaber): it cannot Shoot.</summary>
    public static readonly RangedWeapon None = new("none", "No ranged weapon", "2d4", 0, 0, ["no_ranged"]);

    /// <summary>Range modifier at a given distance in inches.</summary>
    public int RangeModifier(double inches) =>
        inches > 2 * ShortRange ? PenaltyBeyondDouble : inches > ShortRange ? PenaltyBeyondShort : 0;
}

public sealed record MeleeWeapon(string Id, string Name, string Damage, int Ap)
{
    public DiceSpec DamageDice => DiceSpec.Parse(Damage);
}

public sealed record ThrownWeapon(string Id, string Name, int ThrowRange, int BlastRadius, List<string> Keywords,
    string? Damage = null, int Ap = 0, string? Effect = null)
{
    public DiceSpec? DamageDice => Damage is null ? null : DiceSpec.Parse(Damage);
}

public sealed record DemolitionDef(string Id, string Name, string Effect, List<string> Keywords, string? Damage = null, int Ap = 0);

public sealed record WeaponsContent(List<RangedWeapon> Ranged, List<MeleeWeapon> Melee, List<ThrownWeapon> Thrown,
    List<DemolitionDef>? Demolitions = null);

public sealed record KeywordDef(string Id, string Name, string Text);

public sealed record KeywordsContent(List<KeywordDef> Weapon, List<KeywordDef> Armour);

public sealed record ArmourDef(string Id, string Name, int Value, List<string> Keywords);

public sealed record ArmourContent(List<ArmourDef> Armour);

public sealed record GearDef(string Id, string Name, string Text, string Status,
    Dictionary<string, int>? GrantsSkills = null, List<string>? WeaponKeywords = null, string? Melee = null);

public sealed record GearContent(List<GearDef> Gear);

public sealed record SkillDef(string Id, string Name, string Status, List<string> Levels);

public sealed record SkillsContent(List<SkillDef> Skills);

/// <summary>Attributes; null means "never used" (Automated models only).</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record TroopStats(int Move, int Shoot, int? Fight, int? Physique, int? Nerve, int? Tech, int Wounds, int Size);

public sealed record TroopDef(
    string Id, string Faction, string Name, string Role, string Rank, TroopStats Stats,
    string Armour, string? Weapon, string? Melee, List<string> Grenades, List<string> Gear, Dictionary<string, int> Skills,
    List<string>? ArmourOptions = null, List<string>? WeaponOptions = null, List<string>? GearOptions = null,
    string? Miniatures = null);

public sealed record TroopsContent(List<TroopDef> Troops);

public sealed record FactionDef(string Id, string Name, string Side, string Summary);

public sealed record ElementPart(string Troop, int Count);

/// <summary>An enemy element: the building block the mission generator draws enemy forces from.</summary>
public sealed record ElementDef(string Id, string Name, string Faction, int Tier, string Behavior, List<ElementPart> Composition,
    List<string>? Alternatives = null, bool ReactionForce = false);

public sealed record ThreatTierDef(int Tier, string Name, string Summary);

public sealed record FactionsContent(List<FactionDef> Factions, List<ElementDef> Elements, List<ThreatTierDef> ThreatTiers);

/// <summary>A role slot in a team; GearChoice is how many gear options each model in the slot picks.</summary>
public sealed record TeamSlot(string Troop, int Count, int GearChoice = 0);

public sealed record TeamDef(string Id, string Name, string Faction, string Summary, List<TeamSlot> Slots);

public sealed record AttachmentChoice(int Choose, List<string> From);

public sealed record StartingForceDef(string Id, string Name, string Faction, string Summary, List<string> Teams,
    AttachmentChoice? Attachments = null, List<string>? RecommendedGear = null);

public sealed record RostersContent(List<TeamDef> Teams, List<StartingForceDef> StartingForces);

// The solo opponent (rules chapter 10). Everything is rules text for now; the app shows it, it does not run it.

public sealed record AwarenessDef(string Id, string Name, string Text);

/// <summary>A term the priority lists use (area, approach, exit), defined aboard ship and on land.</summary>
/// <remarks>Both default to empty so ContentValidator reports a missing one as tools/validate_content.py does.</remarks>
public sealed record TermDef(string Term = "", string Text = "");

public sealed record NoiseDef(string Id, string Name, string Sources, string Heard, int Alert);

/// <summary>An element's drill, set by the lowest rank in the element.</summary>
public sealed record DrillDef(string Id, string Rank, string Name, string Text);

/// <summary>
/// One behavior profile. Engaged (target in sight), Alerted (no target) and Reactions are priority lists:
/// read top-down and take the first line that applies.
/// </summary>
/// <remarks>Lists default to empty rather than being required, so ContentValidator reports a missing list
/// with the same message as tools/validate_content.py.</remarks>
public sealed record BehaviorDef
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Summary { get; init; }
    public required string Status { get; init; }
    public required bool Surrenders { get; init; }
    /// <summary>Whether the profile uses the general close-quarters list first.</summary>
    public required bool CloseQuarters { get; init; }
    public required string Investigate { get; init; }
    public List<string> Engaged { get; init; } = [];
    public List<string> Alerted { get; init; } = [];
    public List<string> Reactions { get; init; } = [];
    public required string CloseCombat { get; init; }
}

/// <summary>General procedures that apply to every profile, then the profiles themselves.</summary>
public sealed record BehaviorsContent
{
    public List<AwarenessDef> Awareness { get; init; } = [];
    public List<string> Marker { get; init; } = [];
    public List<string> Alarm { get; init; } = [];
    public List<NoiseDef> Noise { get; init; } = [];
    public List<DrillDef> Drills { get; init; } = [];
    public List<string> ActivationOrder { get; init; } = [];
    public List<string> Targets { get; init; } = [];
    public List<string> Moving { get; init; } = [];
    public List<string> CloseQuarters { get; init; } = [];
    public List<string> Surrender { get; init; } = [];
    public List<string> TieBreakers { get; init; } = [];
    public List<TermDef> Terms { get; init; } = [];
    public List<BehaviorDef> Behaviors { get; init; } = [];
}

/// <summary>Content failed to load: malformed JSON, a missing field, or an unknown or duplicate id.</summary>
public sealed class ContentException(IReadOnlyList<string> errors)
    : Exception($"Invalid content ({errors.Count} error{(errors.Count == 1 ? "" : "s")}):\n" + string.Join("\n", errors))
{
    public IReadOnlyList<string> Errors { get; } = errors;
}

public sealed class ContentSet
{
    public required CoreContent Core { get; init; }
    public required WeaponsContent Weapons { get; init; }
    public required ArmourContent Armour { get; init; }
    public required GearContent Gear { get; init; }
    public required SkillsContent Skills { get; init; }
    public required TroopsContent Troops { get; init; }
    public required KeywordsContent Keywords { get; init; }
    public required FactionsContent Factions { get; init; }
    public required RostersContent Rosters { get; init; }
    public required BehaviorsContent Behaviors { get; init; }
    public required LayoutsContent Layouts { get; init; }
    public required TerrainContent Terrain { get; init; }
    public required MissionsContent Missions { get; init; }

    public RangedWeapon Ranged(string id) => Find(Weapons.Ranged, w => w.Id, id, "ranged weapon");
    public MeleeWeapon Melee(string id) => Find(Weapons.Melee, w => w.Id, id, "melee weapon");
    public ThrownWeapon Thrown(string id) => Find(Weapons.Thrown, w => w.Id, id, "thrown weapon");
    public ArmourDef ArmourById(string id) => Find(Armour.Armour, a => a.Id, id, "armour");
    public GearDef GearById(string id) => Find(Gear.Gear, g => g.Id, id, "gear");
    public SkillDef Skill(string id) => Find(Skills.Skills, s => s.Id, id, "skill");
    public TroopDef Troop(string id) => Find(Troops.Troops, t => t.Id, id, "troop");
    public FactionDef Faction(string id) => Find(Factions.Factions, f => f.Id, id, "faction");
    public ElementDef Element(string id) => Find(Factions.Elements, e => e.Id, id, "element");
    public TeamDef Team(string id) => Find(Rosters.Teams, t => t.Id, id, "team");
    public StartingForceDef StartingForce(string id) => Find(Rosters.StartingForces, f => f.Id, id, "starting force");
    public BehaviorDef Behavior(string id) => Find(Behaviors.Behaviors, b => b.Id, id, "behavior");
    public DrillDef DrillFor(string rank) => Find(Behaviors.Drills, d => d.Rank, rank, "drill for rank");
    public SecurityLevelDef SecurityLevel(string id) => Find(Core.Security.Levels, l => l.Id, id, "security level");
    public TileDef Tile(string id) => Find(Layouts.Tiles, t => t.Id, id, "tile");
    public RoomDef Room(string id) => Find(Layouts.Rooms, r => r.Id, id, "room");
    public VesselDef Vessel(string id) => Find(Layouts.Vessels, v => v.Id, id, "vessel");
    public HazardDef Hazard(string id) => Find(Layouts.Hazards, h => h.Id, id, "hazard");
    public TerrainTypeDef TerrainType(string id) => Find(Terrain.Types, t => t.Id, id, "terrain type");
    public EnvironmentDef Environment(string id) => Find(Terrain.Environments, e => e.Id, id, "environment");
    public MissionDef Mission(string id) => Find(Missions.Missions, m => m.Id, id, "mission");
    public TierDef Tier(int tier, string? opposition = null) => Find(
        opposition is null ? Missions.Tiers : Find(Missions.Oppositions ?? [], o => o.Id, opposition, "opposition").Tiers,
        t => t.Tier.ToString(), tier.ToString(), "threat tier");
    public ComplicationDef Complication(string id) => Find(Missions.Complications, x => x.Id, id, "complication");
    public ClockSizeDef ClockSize(string id) => Find(Missions.Clock.Sizes, z => z.Id, id, "clock size");

    private static T Find<T>(List<T> items, Func<T, string> key, string id, string kind) =>
        items.FirstOrDefault(x => key(x) == id) ?? throw new KeyNotFoundException($"Unknown {kind} '{id}'");

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        RespectRequiredConstructorParameters = true,
        RespectNullableAnnotations = true,
    };

    public static ContentSet LoadFile(string path) => Parse(File.ReadAllText(path));

    /// <summary>Loads content.json text and validates it; throws <see cref="ContentException"/> on any error.</summary>
    public static ContentSet Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var errors = new List<string>();

        T Section<T>(string name) where T : class
        {
            try
            {
                if (root.TryGetProperty(name, out var el)) return el.Deserialize<T>(Options)!;
                errors.Add($"{name}: section missing");
            }
            catch (JsonException e) { errors.Add($"{name}: {e.Message}"); }
            return null!;
        }

        var set = new ContentSet
        {
            Core = Section<CoreContent>("core"),
            Weapons = Section<WeaponsContent>("weapons"),
            Armour = Section<ArmourContent>("armour"),
            Gear = Section<GearContent>("gear"),
            Skills = Section<SkillsContent>("skills"),
            Troops = Section<TroopsContent>("troops"),
            Keywords = Section<KeywordsContent>("keywords"),
            Factions = Section<FactionsContent>("factions"),
            Rosters = Section<RostersContent>("rosters"),
            Behaviors = Section<BehaviorsContent>("behaviors"),
            Layouts = Section<LayoutsContent>("layouts"),
            Terrain = Section<TerrainContent>("terrain"),
            Missions = Section<MissionsContent>("missions"),
        };
        if (errors.Count == 0) errors.AddRange(ContentValidator.Validate(set));
        if (errors.Count > 0) throw new ContentException(errors);
        return set;
    }

    /// <summary>Loads content/dist/content.json, found by walking up from a start directory.</summary>
    public static ContentSet LoadDefault(string? startDir = null) => LoadFile(FindDefaultPath(startDir));

    /// <summary>Finds content/dist/content.json by walking up from a start directory.</summary>
    public static string FindDefaultPath(string? startDir = null)
    {
        var dir = new DirectoryInfo(startDir ?? AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "content", "dist", "content.json");
            if (File.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }
        throw new FileNotFoundException("content/dist/content.json not found; run tools/build_content.py");
    }
}
