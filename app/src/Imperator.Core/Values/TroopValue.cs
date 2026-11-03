using Imperator.Core;

namespace Imperator.Core.Values;

// These records are the compiled form of content/values.yaml. All pricing parameters are content, not app rules.
public sealed record ValueBaseline(int Tv, int Move, int Wounds, int Size);
public sealed record ValueCost(double Cost, string? Per = null);
public sealed record ValueRangeBand(int UpTo, double Factor);
public sealed record ValueWeaponFormula(
    double Scale, List<int> ArmourMix, List<ValueRangeBand> Range, double AutoExtraTarget,
    Dictionary<string, double> KeywordFactors, double ExplosionPerInch, double RoundsFlexibility);
public sealed record ValuesContent(
    string Version, ValueBaseline Baseline, Dictionary<string, double> Multiplier,
    Dictionary<string, double> AttributeCosts, double MovePerInch, ValueWeaponFormula WeaponFormula,
    double MeleeScale, Dictionary<string, ValueCost> Gear, Dictionary<string, ValueCost> Grenades,
    Dictionary<string, ValueCost> Skills, double SurvivalExponent, double AutomatedFactor, int RequisitionPer);

/// <summary>The value of a troop and its default or selected loadout.</summary>
public sealed record TroopValue(string Troop, int Tv, int Requisition);

/// <summary>
/// Rules chapter 15. Exact port of tools/troop_values.py; the Python reference fixture verifies each weapon,
/// melee weapon, survival factor, troop TV and Requisition. Nothing is derived in the API or web app.
/// </summary>
public sealed class TroopValueCalculator(ContentSet content)
{
    private readonly ValuesContent _values = content.Values;
    private readonly Dictionary<string, double> _weaponBases = new();
    private readonly Dictionary<string, double> _meleeBases = new();
    private readonly Dictionary<(int Armour, int Wounds), double> _survivals = new();

    private static double Rounded(double value, int decimals) => Math.Round(value, decimals, MidpointRounding.ToEven);

    public double Multiplier(int? value)
    {
        int key = Math.Clamp(value ?? 0, 0, 5);
        return _values.Multiplier[key.ToString(System.Globalization.CultureInfo.InvariantCulture)];
    }

    private int WoundRow(int total)
    {
        foreach (var r in content.Core.WoundTable)
            if (total >= r.Min && total <= r.Max) return r.Row;
        return total < content.Core.WoundTable[0].Min ? 0 : 10;
    }

    /// <summary>Expected row from a hit averaged over the armour mix, with AP reducing effective armour.</summary>
    public double Lethality(string damage, int ap)
    {
        var pmf = DiceSpec.Parse(damage).Pmf();
        return _values.WeaponFormula.ArmourMix.Average(armour =>
        {
            int reduction = Math.Max(0, armour - ap);
            return pmf.Sum(entry => entry.Value * Math.Max(0, Math.Min(10, WoundRow(entry.Key)) - reduction));
        });
    }

    private double RangeFactor(int shortRange)
    {
        var bands = _values.WeaponFormula.Range;
        return (bands.FirstOrDefault(b => shortRange <= b.UpTo) ?? bands[^1]).Factor;
    }

    private double ShotValue(string damage, int ap, IEnumerable<string> keywords, int? auto, int? explosion, int shortRange)
    {
        var formula = _values.WeaponFormula;
        var keys = keywords.ToArray();
        double result = Lethality(damage, ap) * RangeFactor(shortRange);
        if (auto is > 0 && keys.Contains("auto")) result *= 1 + (auto.Value - 1) * formula.AutoExtraTarget;
        foreach (string key in keys)
            result *= formula.KeywordFactors.GetValueOrDefault(key, 1.0);
        if (explosion is > 0) result *= 1 + explosion.Value * formula.ExplosionPerInch;
        return result;
    }

    public double WeaponBase(string id)
    {
        if (_weaponBases.TryGetValue(id, out var cached)) return cached;
        var weapon = content.Ranged(id);
        double best = ShotValue(weapon.Damage, weapon.Ap, weapon.Keywords, weapon.Auto, weapon.Explosion, weapon.ShortRange);
        if (weapon.Rounds is { Count: > 0 })
        {
            foreach (var round in weapon.Rounds)
                best = Math.Max(best, ShotValue(round.Damage, round.Ap, weapon.Keywords.Concat(round.Keywords ?? []),
                    weapon.Auto, round.Explosion, weapon.ShortRange));
            best *= _values.WeaponFormula.RoundsFlexibility;
        }
        return _weaponBases[id] = Rounded(best * _values.WeaponFormula.Scale, 1);
    }

    public double MeleeBase(string id)
    {
        if (_meleeBases.TryGetValue(id, out var cached)) return cached;
        var weapon = content.Melee(id);
        return _meleeBases[id] = Rounded(Lethality(weapon.Damage, weapon.Ap) * _values.MeleeScale, 1);
    }

    /// <summary>Expected 2d8 AP 0 hits to Down from the Python reference, including wounds markers.</summary>
    public double HitsToDown(int armour, int wounds)
    {
        double down = 0, wounded = 0;
        foreach (var (total, probability) in DiceSpec.Parse("2d8").Pmf())
        {
            int row = Math.Min(10, WoundRow(total)) - armour;
            if (row >= 6) down += probability;
            else if (row >= 4) wounded += probability;
        }
        if (down + wounded == 0) return 50.0;
        double expected = 0;
        for (int markers = wounds; markers >= 0; markers--)
            expected = (1 + (markers == wounds ? 0 : wounded * expected)) / (down + wounded);
        return expected;
    }

    public double Survival(int armour, int wounds)
    {
        var key = (armour, wounds);
        if (_survivals.TryGetValue(key, out var cached)) return cached;
        double ratio = HitsToDown(armour, wounds) / HitsToDown(0, 1);
        return _survivals[key] = Rounded(Math.Pow(ratio, _values.SurvivalExponent), 2);
    }

    /// <summary>
    /// Calculate a troop's value. Optional loadout parameters replace the whole named choice, matching the
    /// Python reference's weapon, melee and gear overrides. Grenades and armour may also be supplied.
    /// </summary>
    public TroopValue Calculate(string troopId, string? weapon = null, string? melee = null,
        IReadOnlyList<string>? gear = null, string? armour = null, IReadOnlyList<string>? grenades = null)
    {
        var troop = content.Troop(troopId);
        var stats = troop.Stats;
        double subtotal = _values.Baseline.Tv;
        foreach (var (attribute, cost) in _values.AttributeCosts)
        {
            int? rating = Attribute(stats, attribute);
            if (rating is not null && rating != 0) subtotal += rating.Value * cost;
        }
        subtotal += (stats.Move - _values.Baseline.Move) * _values.MovePerInch;

        string? equipped = weapon ?? troop.Weapon;
        if (equipped is not null) subtotal += Rounded(WeaponBase(equipped) * Multiplier(stats.Shoot), 1);

        var chosenGear = gear ?? troop.Gear;
        string? chosenMelee = melee ?? troop.Melee;
        foreach (var item in chosenGear)
            if (content.GearById(item).Melee is { } grantedMelee) chosenMelee = grantedMelee;
        if (chosenMelee is not null) subtotal += Rounded(MeleeBase(chosenMelee) * Multiplier(stats.Fight), 1);

        double kit = 0;
        double Price(string id, IReadOnlyDictionary<string, ValueCost> prices)
        {
            var cost = prices[id];
            return cost.Cost * (cost.Per is null ? 1.0 : Multiplier(Attribute(stats, cost.Per)));
        }
        foreach (string item in chosenGear) kit += Price(item, _values.Gear);
        foreach (string item in grenades ?? troop.Grenades) kit += Price(item, _values.Grenades);

        var grantedSkills = chosenGear.SelectMany(item => content.GearById(item).GrantsSkills?.Keys.AsEnumerable() ?? Enumerable.Empty<string>()).ToHashSet();
        foreach (var (skill, level) in troop.Skills)
            if (!grantedSkills.Contains(skill))
            {
                var cost = _values.Skills[skill];
                kit += cost.Cost * level * (cost.Per is null ? 1.0 : Multiplier(Attribute(stats, cost.Per)));
            }
        subtotal += kit;
        int armourRating = content.ArmourById(armour ?? troop.Armour).Value;
        double survival = Survival(armourRating, stats.Wounds);
        double automated = troop.Skills.ContainsKey("automated") ? _values.AutomatedFactor : 1.0;
        int tv = Math.Max(1, (int)Rounded(subtotal * survival * automated, 0));
        int requisition = (int)Math.Ceiling(tv / (double)_values.RequisitionPer);
        return new TroopValue(troop.Id, tv, requisition);
    }

    private static int? Attribute(TroopStats stats, string name) => name switch
    {
        "move" => stats.Move,
        "shoot" => stats.Shoot,
        "fight" => stats.Fight,
        "physique" => stats.Physique,
        "nerve" => stats.Nerve,
        "tech" => stats.Tech,
        "wounds" => stats.Wounds,
        "size" => stats.Size,
        _ => throw new ArgumentException($"Unknown value attribute '{name}'"),
    };

    public IReadOnlyList<TroopValue> AllTroops() => content.Troops.Troops.Select(t => Calculate(t.Id)).ToList();
}
