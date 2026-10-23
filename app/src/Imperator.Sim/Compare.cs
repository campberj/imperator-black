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
            ("Real VAT team vs asset security on Overwatch, flash: cleared by R3", (c, r) => RealRoom(c, r, n, true).Cleared),
            ("Real VAT team vs asset security on Overwatch, flash: VAT out / dead", (c, r) => RealRoom(c, r, n, true).Cas),
            ("Real VAT team vs asset security on Overwatch, flash: defenders out", (c, r) => RealRoom(c, r, n, true).DefOut),
            ("Real VAT team vs unready asset security, flash: cleared by R3", (c, r) => RealRoom(c, r, n, false).Cleared),
            ("Patrol Contact (both sides have LMG + SMG): fireteam / patrol wins", (c, r) => Patrol(c, r, n)),
            ("3 rifles + LMG vs 4 rifles (all Q1): LMG side wins", (c, r) => GunTeam(c, r, n)),
            ("Pinning: 4v4 Q1 (Nerve 1): activations spent Pinned", (c, r) => Fight(c, r, n, 4, 1, 4, 1).Pinned),
            ("Pinning: 4v4 Q2 (Nerve 2): activations spent Pinned", (c, r) => Fight(c, r, n, 4, 2, 4, 2).Pinned),
            ("Pinning: 4 Q2 vs 4 Q1: Q2 side wins", (c, r) => Fight(c, r, n, 4, 2, 4, 1).A),
            ("Real: clone squad vs UN reserve fireteam: clones win / draw", (c, r) => Real(c, r, n, CloneSquad, UnFireteam).Result),
            ("Real: clone squad vs UN reserve fireteam: clone / UN activations Pinned", (c, r) => Real(c, r, n, CloneSquad, UnFireteam).Pinned),
            ("Real: VAT fireteam vs asset security + captain: VAT win / draw", (c, r) => Real(c, r, n, VatTeam, AssetSecurity).Result),
            ("Real: VAT fireteam vs asset security + captain: VAT / security activations Pinned", (c, r) => Real(c, r, n, VatTeam, AssetSecurity).Pinned),
            ("Real: VAT fireteam vs UN reserve fireteam: VAT win / draw", (c, r) => Real(c, r, n, VatTeam, UnFireteam).Result),
            ("Real: generic squad (Nerve 1) vs clone squad (Nerve 2): generic win / draw", (c, r) => Real(c, r, n, GenericSquad, CloneSquad).Result),
            ("Real: Harlow veterans (Nerve 3) vs clone squad: Harlow win / draw", (c, r) => Real(c, r, n, HarlowTeam, CloneSquad).Result),
            ("Real: Ahsoka + clone squad vs UN reserve fireteam: Republic win", (c, r) => Real(c, r, n, AhsokaSquad, UnFireteam).Result),
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

    private static Soldier Make(ContentSet c, int q, string armour, string weapon, string melee = "knife", bool leader = false, bool vt = false) =>
        Soldier.Generic(weapon, q, c.ArmourById(armour).Value, c.Ranged(weapon), c.Melee(melee), leader, voidTrained: vt);

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

    private static (string A, string Draw, string Dead, string Pinned) Fight(ContentSet c, Rng rng, int n, int na, int qa, int nb, int qb)
    {
        int wa = 0, dr = 0; double dead = 0; long acts = 0, pinned = 0;
        for (int i = 0; i < n; i++)
        {
            var a = Enumerable.Range(0, na).Select(k => Make(c, qa, "flak", "rifle", leader: k == 0)).ToList();
            var b = Enumerable.Range(0, nb).Select(k => Make(c, qb, "flak", "rifle", leader: k == 0)).ToList();
            Firefight.ScatterCover(rng, a); Firefight.ScatterCover(rng, b);
            var f = new Firefight(rng, c.Core, a, b);
            var w = f.Run();
            if (w == FightWinner.A) wa++; else if (w == FightWinner.Draw) dr++;
            dead += a.Concat(b).Count(s => s.Status == Status.Dead);
            acts += f.Activations; pinned += f.PinnedActivations;
        }
        return (P(wa / (double)n), P(dr / (double)n), $"{dead / n:0.00}", P(pinned / (double)Math.Max(1, acts)));
    }

    // Real teams, by troop id (the first is the side's leader where it has one).
    private static readonly string[] CloneSquad = ["rep_clone_sergeant", "rep_clone_trooper", "rep_clone_trooper", "rep_clone_trooper", "rep_clone_heavy"];
    private static readonly string[] AhsokaSquad = [.. CloneSquad, "rep_ahsoka"];
    private static readonly string[] UnFireteam = ["un3_rifleman", "un3_rifleman", "un3_rifleman", "un3_at_specialist"];
    private static readonly string[] VatTeam = ["tfb_vat_leader", "tfb_vat_assaulter", "tfb_vat_assaulter", "tfb_vat_gunner", "tfb_data_specialist"];
    private static readonly string[] AssetSecurity = ["fs_recovery_captain", "fs_security", "fs_security", "fs_security_gunner"];
    private static readonly string[] GenericSquad = ["gen_officer", "gen_trooper", "gen_trooper", "gen_trooper", "gen_gunner"];
    private static readonly string[] HarlowTeam = ["ct_harlow_leader", "ct_harlow_veteran", "ct_harlow_veteran", "ct_harlow_veteran"];

    /// <summary>A firefight between two real teams: A's win / draw rate, and each side's share of activations begun Pinned.</summary>
    private static (string Result, string Pinned) Real(ContentSet c, Rng rng, int n, string[] ta, string[] tb)
    {
        int wa = 0, dr = 0; long actA = 0, pinA = 0, actB = 0, pinB = 0;
        for (int i = 0; i < n; i++)
        {
            var a = ta.Select(id => Soldier.FromTroop(c, id)).ToList();
            var b = tb.Select(id => Soldier.FromTroop(c, id)).ToList();
            Firefight.ScatterCover(rng, a); Firefight.ScatterCover(rng, b);
            var f = new Firefight(rng, c.Core, a, b);
            var w = f.Run();
            if (w == FightWinner.A) wa++; else if (w == FightWinner.Draw) dr++;
            foreach (var (s, pin, act) in f.PinnedBySoldier)
                if (a.Contains(s)) { actA += act; pinA += pin; } else { actB += act; pinB += pin; }
        }
        return ($"{P(wa / (double)n)} / {P(dr / (double)n)}", $"{P(pinA / (double)Math.Max(1, actA))} / {P(pinB / (double)Math.Max(1, actB))}");
    }

    private static (string Cleared, string Cas, string DefOut) RealRoom(ContentSet c, Rng rng, int n, bool overwatch)
    {
        int cleared = 0; double attOut = 0, attDead = 0, defOut = 0;
        for (int i = 0; i < n; i++)
        {
            var att = new List<Soldier>
            {
                Soldier.FromTroop(c, "tfb_vat_assaulter"), Soldier.FromTroop(c, "tfb_vat_assaulter"), Soldier.FromTroop(c, "tfb_vat_leader"),
                Soldier.FromTroop(c, "tfb_vat_gunner"), Soldier.FromTroop(c, "tfb_data_specialist"),
            };
            var def = new List<Soldier> { Soldier.FromTroop(c, "fs_security"), Soldier.FromTroop(c, "fs_security", "shotgun"), Soldier.FromTroop(c, "fs_security_gunner") };
            if (new RoomClear(rng, c, att, def, EntryMethod.FlashFirst, overwatch).Run() == RoomOutcome.Cleared) cleared++;
            attOut += att.Count(s => !s.IsActive); attDead += att.Count(s => s.Status == Status.Dead); defOut += def.Count(s => !s.IsActive);
        }
        return (P(cleared / (double)n), $"{attOut / n:0.00} / {attDead / n:0.00}", $"{defOut / n:0.00}");
    }

    private static string Patrol(ContentSet c, Rng rng, int n)
    {
        int wa = 0, wb = 0;
        for (int i = 0; i < n; i++)
        {
            var a = new List<Soldier>
            {
                Soldier.Generic("TL", 2, c.ArmourById("composite").Value, c.Ranged("rifle"), c.Melee("knife"), leader: true),
                Soldier.Generic("Gun", 1, 1, c.Ranged("lmg"), c.Melee("knife")),
                Soldier.Generic("Med", 1, 1, c.Ranged("rifle"), c.Melee("knife"), medic: true),
                Soldier.Generic("Mk", 1, 1, c.Ranged("dmr"), c.Melee("knife")),
            };
            var b = new List<Soldier>
            {
                Soldier.Generic("Ldr", 1, 1, c.Ranged("rifle"), c.Melee("knife"), leader: true), Soldier.Generic("Gun", 1, 1, c.Ranged("lmg"), c.Melee("knife")),
                Soldier.Generic("R1", 1, 1, c.Ranged("rifle"), c.Melee("knife")), Soldier.Generic("R2", 1, 1, c.Ranged("rifle"), c.Melee("knife")),
                Soldier.Generic("Tpr", 1, 1, c.Ranged("smg"), c.Melee("knife")),
            };
            Firefight.ScatterCover(rng, a); Firefight.ScatterCover(rng, b);
            var w = new Firefight(rng, c.Core, a, b).Run();
            if (w == FightWinner.A) wa++; else if (w == FightWinner.B) wb++;
        }
        return $"{P(wa / (double)n)} / {P(wb / (double)n)}";
    }

    private static string GunTeam(ContentSet c, Rng rng, int n)
    {
        int wa = 0;
        for (int i = 0; i < n; i++)
        {
            var a = Enumerable.Range(0, 3).Select(k => Make(c, 1, "flak", "rifle", leader: k == 0)).Append(Make(c, 1, "flak", "lmg")).ToList();
            var b = Enumerable.Range(0, 4).Select(k => Make(c, 1, "flak", "rifle", leader: k == 0)).ToList();
            Firefight.ScatterCover(rng, a); Firefight.ScatterCover(rng, b);
            if (new Firefight(rng, c.Core, a, b).Run() == FightWinner.A) wa++;
        }
        return P(wa / (double)n);
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
