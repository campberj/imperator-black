namespace Imperator.Core;

public enum CombatEnd { AttackerWon, DefenderWon, BothBrokeAway, ExchangeLimit }

public sealed record CombatResult(CombatEnd End, int Exchanges);

/// <summary>Decides whether a model continues or breaks away after an exchange.</summary>
public delegate bool BreakPolicy(Soldier self, bool lostLastExchange);

public static class CloseCombat
{
    /// <summary>Default solo rule: break if it lost the last exchange and is Wounded or has 3+ Suppression.</summary>
    public static readonly BreakPolicy DefaultBreak = (s, lost) => lost && (s.Wounded || s.Suppression >= 3);

    public static readonly BreakPolicy NeverBreak = (_, _) => false;

    public static int Modifier(Soldier s, bool charging, int supporters, bool zeroG) =>
        s.Q + s.RollPenalty + (charging ? 1 : 0) + Math.Min(2, supporters) + (zeroG && !s.VoidTrained ? -1 : 0);

    /// <summary>
    /// Fights close combat to its end: one side incapacitated, or both break away.
    /// A safety limit stops runaway loops in simulation; at the table there is no limit.
    /// </summary>
    public static CombatResult Fight(
        Rng rng, CoreContent core, Soldier attacker, Soldier defender, bool charging,
        BreakPolicy? attackerPolicy = null, BreakPolicy? defenderPolicy = null,
        int attackerSupport = 0, int defenderSupport = 0, bool zeroG = false, int exchangeLimit = 50,
        int decisiveMargin = 0)
    {
        attackerPolicy ??= DefaultBreak;
        defenderPolicy ??= DefaultBreak;
        for (int n = 1; n <= exchangeLimit; n++)
        {
            var (a1, a2) = rng.TwoD6();
            var (d1, d2) = rng.TwoD6();
            int at = a1 + a2 + Modifier(attacker, charging && n == 1, attackerSupport, zeroG);
            int dt = d1 + d2 + Modifier(defender, false, defenderSupport, zeroG);

            bool attackerLost, defenderLost;
            if (at == dt)
            {
                attacker.AddSuppression(1);
                defender.AddSuppression(1);
                attackerLost = defenderLost = true; // neither won
            }
            else
            {
                bool attackerWins = at > dt;
                var (winner, loser) = attackerWins ? (attacker, defender) : (defender, attacker);
                bool crit = attackerWins ? a1 == 6 && a2 == 6 : d1 == 6 && d2 == 6;
                attackerLost = !attackerWins;
                defenderLost = attackerWins;

                if (loser.Suppression >= 5)
                {
                    loser.Incapacitate(Status.Down); // overpowered (or captured)
                }
                else
                {
                    int margin = Math.Abs(at - dt);
                    int bonus = decisiveMargin > 0 && margin >= decisiveMargin ? 1 : 0; // experimental: decisive win
                    int row = Math.Min(10, Wounds.FinalRow(core, rng.Roll(winner.Melee.DamageDice), crit, loser.Armour, winner.Melee.Ap) + bonus);
                    if (row <= 0) loser.AddSuppression(1);
                    else loser.ApplyRow(row);
                }
            }

            if (!defender.IsActive) return new CombatResult(CombatEnd.AttackerWon, n);
            if (!attacker.IsActive) return new CombatResult(CombatEnd.DefenderWon, n);

            // The loser declares first, but the combat ends only if both break.
            if (defenderPolicy(defender, defenderLost) && attackerPolicy(attacker, attackerLost))
                return new CombatResult(CombatEnd.BothBrokeAway, n);
        }
        return new CombatResult(CombatEnd.ExchangeLimit, exchangeLimit);
    }
}

public static class Grenades
{
    /// <summary>Throw roll: true if the grenade lands on the point.</summary>
    public static (bool OnTarget, bool Slipped) Throw(Rng rng, CoreContent core, Soldier thrower, int situational = 0)
    {
        var (a, b) = rng.TwoD6();
        if (a + b == 2) return (false, true);
        return (a + b + thrower.Q + thrower.RollPenalty + situational >= core.HitTarget, false);
    }

    /// <summary>Frag blast on a set of models: one damage roll, read per model.</summary>
    public static void Frag(Rng rng, CoreContent core, ThrownWeapon frag, IEnumerable<Soldier> targets, bool confined)
    {
        int dmg = rng.Roll(frag.DamageDice!.Value);
        foreach (var t in targets.Where(t => t.IsActive))
        {
            int row = Wounds.FinalRow(core, dmg, false, t.Armour, frag.Ap, t.Cover == Cover.Heavy ? 1 : 0) + (confined ? 1 : 0);
            t.HitThisRound = true;
            t.ApplyRow(Math.Min(row, 10));
        }
    }

    public static void Flashbang(IEnumerable<Soldier> targets)
    {
        foreach (var t in targets.Where(t => t.IsActive))
        {
            t.AddSuppression(2);
            t.OverwatchReactions = 0;
            t.CannotReact = true;
            t.HitThisRound = true;
        }
    }
}
