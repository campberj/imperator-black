using System.Text;
using Imperator.Core;

namespace Imperator.Sim;

/// <summary>Runs a compact set of metrics against several content variants side by side.</summary>
public static class Compare
{
    public static string Run(IReadOnlyList<(string Label, ContentSet Content)> variants, int n, int seed)
    {
        var rows = new List<(string Metric, string[] Values)>();
        var metrics = new List<(string Name, Func<ContentSet, Rng, string> Fn)>
        {
            ("Rifle vs Flak, one hit: out", (c, _) => OutOnHit(c, "rifle", 1)),
            ("Carbine vs Flak, one hit: out", (c, _) => OutOnHit(c, "carbine", 1)),
            ("Carbine vs Void plate (2), one hit: out", (c, _) => OutOnHit(c, "carbine", 2)),
            ("Shotgun vs Flak, one hit: out", (c, _) => OutOnHit(c, "shotgun", 1)),
            ("Sidearm vs no armour, one hit: out", (c, _) => OutOnHit(c, "sidearm", 0)),
            ("Room clear, dynamic entry: cleared by R3", (c, r) => Room(c, r, n, EntryMethod.Dynamic).Cleared),
            ("Room clear, dynamic entry: attackers out / dead", (c, r) => Room(c, r, n, EntryMethod.Dynamic).AttCas),
            ("Room clear, flashbang first: cleared by R3", (c, r) => Room(c, r, n, EntryMethod.FlashFirst).Cleared),
            ("Room clear, flashbang first: attackers out / dead", (c, r) => Room(c, r, n, EntryMethod.FlashFirst).AttCas),
            ("Room clear, breach + flash: cleared by R3", (c, r) => Room(c, r, n, EntryMethod.BreachAndFlash).Cleared),
            ("4 Q1 vs 4 Q1 rifles: draws", (c, r) => Fight(c, r, n, 4, 1, 4, 1).Draw),
            ("4 Q2 vs 4 Q1 rifles: Q2 side wins", (c, r) => Fight(c, r, n, 4, 2, 4, 1).A),
            ("4 Q1 vs 5 Q1 rifles: 4-side wins", (c, r) => Fight(c, r, n, 4, 1, 5, 1).A),
            ("Firefight 4v4 Q1: models dead per game", (c, r) => Fight(c, r, n, 4, 1, 4, 1).Dead),
            ("Pillar 1: 4 Q2 in cover vs 6 Q0 in the open, Q2 wins", (c, r) => Pillar1(c, r, n)),
            ("Close combat Q1 vs Q1: avg exchanges", (c, r) => Melee(c, r, n).Exchanges),
            ("Close combat Q1 vs Q1: loser dead", (c, r) => Melee(c, r, n).Dead),
        };
        foreach (var (name, fn) in metrics)
            rows.Add((name, variants.Select(v => fn(v.Content, new Rng(seed))).ToArray()));

        var md = new StringBuilder();
        md.AppendLine("| Metric | " + string.Join(" | ", variants.Select(v => v.Label)) + " |");
        md.AppendLine("|---|" + string.Concat(variants.Select(_ => "---|")));
        foreach (var (m, vals) in rows) md.AppendLine($"| {m} | {string.Join(" | ", vals)} |");
        return md.ToString();
    }

    private static string P(double p) => p < 0.0005 ? "—" : $"{p * 100:0}%";

    private static Soldier Make(ContentSet c, int q, string armour, string weapon, string melee = "knife", bool leader = false, bool vt = false) => new()
    {
        Name = weapon, Q = q, Armour = c.ArmourById(armour).Value, Weapon = c.Ranged(weapon), Melee = c.Melee(melee), IsLeader = leader, VoidTrained = vt,
    };

    private static string OutOnHit(ContentSet c, string weapon, int armour)
    {
        var w = c.Ranged(weapon);
        var o = Shooting.Odds(c.Core, 0, w.DamageDice, armour, w.Ap);
        double hit = 1 - o[Outcome.Miss];
        double outP = o[Outcome.Down] + o[Outcome.Bleeding] + o[Outcome.Critical] + o[Outcome.Dead];
        return P(outP / hit);
    }

    private static (string Cleared, string AttCas) Room(ContentSet c, Rng rng, int n, EntryMethod m)
    {
        int cleared = 0; double attOut = 0, attDead = 0;
        for (int i = 0; i < n; i++)
        {
            var att = Enumerable.Range(0, 4).Select(k => Make(c, 2, "void_plate", "carbine", leader: k == 3, vt: true)).ToList();
            var def = new List<Soldier> { Make(c, 1, "flak", "carbine"), Make(c, 1, "flak", "carbine"), Make(c, 1, "flak", "shotgun") };
            if (new RoomClear(rng, c, att, def, m, true).Run() == RoomOutcome.Cleared) cleared++;
            attOut += att.Count(s => !s.IsActive);
            attDead += att.Count(s => s.Status == Status.Dead);
        }
        return (P(cleared / (double)n), $"{attOut / n:0.00} / {attDead / n:0.00}");
    }

    private static (string A, string Draw, string Dead) Fight(ContentSet c, Rng rng, int n, int na, int qa, int nb, int qb)
    {
        int wa = 0, dr = 0; double dead = 0;
        for (int i = 0; i < n; i++)
        {
            var a = Enumerable.Range(0, na).Select(k => Make(c, qa, "flak", "rifle", leader: k == 0)).ToList();
            var b = Enumerable.Range(0, nb).Select(k => Make(c, qb, "flak", "rifle", leader: k == 0)).ToList();
            Firefight.ScatterCover(rng, a); Firefight.ScatterCover(rng, b);
            var w = new Firefight(rng, c.Core, a, b).Run();
            if (w == FightWinner.A) wa++; else if (w == FightWinner.Draw) dr++;
            dead += a.Concat(b).Count(s => s.Status == Status.Dead);
        }
        return (P(wa / (double)n), P(dr / (double)n), $"{dead / n:0.00}");
    }

    private static string Pillar1(ContentSet c, Rng rng, int n)
    {
        int wa = 0;
        for (int i = 0; i < n; i++)
        {
            var a = Enumerable.Range(0, 4).Select(k => Make(c, 2, "flak", "rifle", leader: k == 0)).ToList();
            var b = Enumerable.Range(0, 6).Select(k => Make(c, 0, "flak", "rifle", leader: k == 0)).ToList();
            Firefight.ScatterCover(rng, a, 0.0, 0.5); Firefight.ScatterCover(rng, b, 1.0, 0.0);
            if (new Firefight(rng, c.Core, a, b).Run() == FightWinner.A) wa++;
        }
        return P(wa / (double)n);
    }

    private static (string Exchanges, string Dead) Melee(ContentSet c, Rng rng, int n)
    {
        int ex = 0, dead = 0, decided = 0;
        for (int i = 0; i < n; i++)
        {
            var a = Make(c, 1, "flak", "carbine"); var d = Make(c, 1, "flak", "carbine");
            var r = CloseCombat.Fight(rng, c.Core, a, d, true);
            ex += r.Exchanges;
            if (r.End is CombatEnd.AttackerWon or CombatEnd.DefenderWon) decided++;
            if (a.Status == Status.Dead || d.Status == Status.Dead) dead++;
        }
        return ($"{ex / (double)n:0.0}", P(dead / (double)Math.Max(1, decided)));
    }
}
