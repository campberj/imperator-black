using System.Text.Json;
using System.Text.Json.Serialization;

namespace Imperator.Core;

// Game content, compiled from content/*.yaml by tools/build_content.py into content/dist/content.json.

public sealed record RankDef(string Id, string Name, string Typical, string Text);

public sealed record AttributeDef(string Id, string Name, string Short, string Text);

public sealed record WoundRowDef(int Row, int Min, int Max, string Id, string Text);

public sealed record HitModifierDef(string Id, int Value, string Text);

public sealed record CoreContent(
    string Version,
    int HitTarget,
    int TestTarget,
    int NaturalMiss,
    List<RankDef> Ranks,
    List<AttributeDef> Attributes,
    List<HitModifierDef> HitModifiers,
    List<WoundRowDef> WoundTable);

public sealed record RangedWeapon(string Id, string Name, string Damage, int ShortRange, int Ap, List<string> Keywords, List<int>? RangePenalty)
{
    public DiceSpec DamageDice => DiceSpec.Parse(Damage);
    public bool Has(string keyword) => Keywords.Contains(keyword);
    public int PenaltyBeyondShort => RangePenalty is { Count: > 0 } p ? p[0] : -1;
    public int PenaltyBeyondDouble => RangePenalty is { Count: > 1 } p ? p[1] : -2;

    /// <summary>Range modifier at a given distance in inches.</summary>
    public int RangeModifier(double inches) =>
        inches > 2 * ShortRange ? PenaltyBeyondDouble : inches > ShortRange ? PenaltyBeyondShort : 0;
}

public sealed record MeleeWeapon(string Id, string Name, string Damage, int Ap)
{
    public DiceSpec DamageDice => DiceSpec.Parse(Damage);
}

public sealed record ThrownWeapon(string Id, string Name, int ThrowRange, int BlastRadius, string? Damage, int Ap, string? Effect, List<string> Keywords)
{
    public DiceSpec? DamageDice => Damage is null ? null : DiceSpec.Parse(Damage);
}

public sealed record WeaponsContent(List<RangedWeapon> Ranged, List<MeleeWeapon> Melee, List<ThrownWeapon> Thrown);

public sealed record ArmourDef(string Id, string Name, int Value, List<string> Keywords);

public sealed record ArmourContent(List<ArmourDef> Armour);

public sealed record GearDef(string Id, string Name, string Text, Dictionary<string, int>? GrantsSkills,
    List<string>? WeaponKeywords, string? Melee, string Status);

public sealed record GearContent(List<GearDef> Gear);

public sealed record SkillDef(string Id, string Name, string Status, List<string> Levels);

public sealed record SkillsContent(List<SkillDef> Skills);

/// <summary>Attributes; null means "never used" (Automated models only).</summary>
public sealed record TroopStats(int Move, int Shoot, int? Fight, int? Physique, int? Nerve, int? Tech, int Wounds, int Size);

public sealed record TroopDef(
    string Id, string Faction, string Name, string Role, string Rank, TroopStats Stats,
    string Armour, List<string>? ArmourOptions,
    string Weapon, List<string>? WeaponOptions, string? Melee,
    List<string> Grenades, List<string> Gear, List<string>? GearOptions,
    Dictionary<string, int> Skills, string? Miniatures);

public sealed record TroopsContent(List<TroopDef> Troops);

public sealed class ContentSet
{
    public required CoreContent Core { get; init; }
    public required WeaponsContent Weapons { get; init; }
    public required ArmourContent Armour { get; init; }
    public required GearContent Gear { get; init; }
    public required SkillsContent Skills { get; init; }
    public required TroopsContent Troops { get; init; }

    public RangedWeapon Ranged(string id) => Weapons.Ranged.Single(w => w.Id == id);
    public MeleeWeapon Melee(string id) => Weapons.Melee.Single(w => w.Id == id);
    public ThrownWeapon Thrown(string id) => Weapons.Thrown.Single(w => w.Id == id);
    public ArmourDef ArmourById(string id) => Armour.Armour.Single(a => a.Id == id);
    public GearDef GearById(string id) => Gear.Gear.Single(g => g.Id == id);
    public TroopDef Troop(string id) => Troops.Troops.Single(t => t.Id == id);

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    public static ContentSet LoadFile(string path)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var root = doc.RootElement;
        return new ContentSet
        {
            Core = root.GetProperty("core").Deserialize<CoreContent>(Options)!,
            Weapons = root.GetProperty("weapons").Deserialize<WeaponsContent>(Options)!,
            Armour = root.GetProperty("armour").Deserialize<ArmourContent>(Options)!,
            Gear = root.GetProperty("gear").Deserialize<GearContent>(Options)!,
            Skills = root.GetProperty("skills").Deserialize<SkillsContent>(Options)!,
            Troops = root.GetProperty("troops").Deserialize<TroopsContent>(Options)!,
        };
    }

    /// <summary>Finds content/dist/content.json by walking up from a start directory.</summary>
    public static ContentSet LoadDefault(string? startDir = null)
    {
        var dir = new DirectoryInfo(startDir ?? AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "content", "dist", "content.json");
            if (File.Exists(candidate)) return LoadFile(candidate);
            dir = dir.Parent;
        }
        throw new FileNotFoundException("content/dist/content.json not found; run tools/build_content.py");
    }
}
