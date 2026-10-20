namespace Imperator.Core;

public readonly record struct ShotResult(bool Hit, bool Critical, int Row)
{
    public Outcome Outcome => !Hit ? Outcome.Miss : Wounds.Classify(Row);
}

public static class Shooting
{
    public static int CoverModifier(Cover cover) => cover switch { Cover.Light => -1, Cover.Heavy => -2, _ => 0 };

    /// <summary>
    /// Total modifier for a shot: shooter Q and penalties, target cover, and situational modifiers
    /// (aim, moved, reaction, range...) passed in as <paramref name="situational"/>.
    /// </summary>
    public static int Modifier(Soldier shooter, Cover targetCover, int situational) =>
        shooter.Q + shooter.RollPenalty + CoverModifier(targetCover) + situational;

    public static bool IsHit(CoreContent core, int a, int b, int modifier) =>
        a + b != core.NaturalMiss && (a == 6 && b == 6 || a + b + modifier >= core.HitTarget);

    public static ShotResult Resolve(Rng rng, CoreContent core, int modifier, DiceSpec damage, int armour, int ap, int rowBonus = 0)
    {
        var (a, b) = rng.TwoD6();
        int dmg = rng.Roll(damage); // thrown together with the hit dice
        if (!IsHit(core, a, b, modifier)) return new ShotResult(false, false, 0);
        bool crit = a == 6 && b == 6;
        return new ShotResult(true, crit, Math.Min(10, Wounds.FinalRow(core, dmg, crit, armour, ap) + rowBonus));
    }

    /// <summary>Resolves a shot between two soldiers and applies the result to the target.</summary>
    public static ShotResult Fire(Rng rng, CoreContent core, Soldier shooter, Soldier target, int situational, int rowBonus = 0)
    {
        var w = shooter.Weapon;
        var r = Resolve(rng, core, Modifier(shooter, target.Cover, situational), w.DamageDice, target.Armour, w.Ap, rowBonus);
        if (r.Hit)
        {
            target.HitThisRound = true;
            target.ApplyRow(r.Row);
            if (w.Has("suppressing")) target.AddSuppression(1);
        }
        return r;
    }

    /// <summary>Suppressive Fire: hit roll only; 1 Suppression on a hit, 2 if it beats the target by 3+.</summary>
    public static bool Suppress(Rng rng, CoreContent core, Soldier shooter, Soldier target, int situational)
    {
        var (a, b) = rng.TwoD6();
        int mod = Modifier(shooter, target.Cover, situational);
        if (!IsHit(core, a, b, mod)) return false;
        target.HitThisRound = true;
        target.AddSuppression(a + b + mod >= core.HitTarget + 3 ? 2 : 1);
        return true;
    }

    /// <summary>Exact probability of each outcome for one shot against a fresh target.</summary>
    public static Dictionary<Outcome, double> Odds(CoreContent core, int modifier, DiceSpec damage, int armour, int ap)
    {
        var result = Enum.GetValues<Outcome>().ToDictionary(o => o, _ => 0.0);
        var pmf = damage.Pmf();
        for (int a = 1; a <= 6; a++)
        for (int b = 1; b <= 6; b++)
        {
            const double p2d6 = 1.0 / 36;
            if (!IsHit(core, a, b, modifier)) { result[Outcome.Miss] += p2d6; continue; }
            bool crit = a == 6 && b == 6;
            foreach (var (dmg, p) in pmf)
                result[Wounds.Classify(Wounds.FinalRow(core, dmg, crit, armour, ap))] += p2d6 * p;
        }
        return result;
    }
}
