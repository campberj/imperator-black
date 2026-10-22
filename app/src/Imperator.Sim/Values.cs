using System.Text;
using Imperator.Core;

namespace Imperator.Sim;

/// <summary>One troop profile's measures against the reference trooper. Win rates count a draw as half a win.</summary>
public sealed record TroopValue(
    TroopDef Troop, Soldier Model, double? Duel12, double? Duel24, double CloseCombat,
    double? RowsA0, double? RowsA2, double? RowsA3)
{
    /// <summary>The mean of the win rates the profile has (a model with no ranged weapon has only close combat).</summary>
    public double Score => new[] { Duel12, Duel24, CloseCombat }.OfType<double>().Average();
}

/// <summary>
/// Troop value harness (decision 6.12, roadmap Phase 7 item 6): every troop profile, with its default loadout, against a
/// reference trooper in three duels, plus the expected wound rows of one Shoot action. Data for calibrating points;
/// the points formula is a design decision that comes after.
/// </summary>
public static class Values
{
    public const string ReferenceTroop = "tfb_vat_assaulter";

    /// <summary>A duel with no result after this many rounds is a draw.</summary>
    public const int RoundLimit = 30;

    private static readonly string[] Modelled = ["automated", "marksman", "augmented", "shield", "jedi_discipline", "twin_blades", "leader"];

    /// <summary>Skills and gear that change the math but are not modelled here, with the reason.</summary>
    private static readonly Dictionary<string, string> NotModelled = new()
    {
        ["laser_detector"] = "Laser detector (reactions are not simulated)",
        ["deflect"] = "Deflect (its holders have no ranged weapon, so they fight no ranged duel)",
        ["self_destruct"] = "Self-destruct (blasts are not simulated)",
        ["grenade_launcher"] = "grenade launcher (Throws are not simulated)",
        ["mk19"] = "Mk19 grenade launcher (Throws are not simulated)",
        ["coop_engagement"] = "cooperative engagement package (helps friends, not the carrier)",
        ["flux_warhead"] = "flux warhead (blasts are not simulated)",
    };

    public static TroopValue Measure(ContentSet content, string troopId, int n, int seed)
    {
        var troop = content.Troop(troopId);
        var model = Soldier.FromTroop(content, troopId);
        bool ranged = !model.Weapon.Has("no_ranged");
        double? Rows(int armour) => ranged ? ExpectedRows(content.Core, model, armour) : null;
        return new TroopValue(troop, model,
            ranged ? WinRate(n, Rng.FromKey($"values-duel12-{troopId}-{seed}"), rng => Duel(content, rng, troopId, 12, Cover.Open)) : null,
            ranged ? WinRate(n, Rng.FromKey($"values-duel24-{troopId}-{seed}"), rng => Duel(content, rng, troopId, 24, Cover.Light)) : null,
            WinRate(n, Rng.FromKey($"values-cc-{troopId}-{seed}"), rng => Melee(content, rng, troopId)),
            Rows(0), Rows(2), Rows(3));
    }

    private static double WinRate(int n, Rng rng, Func<Rng, double> run)
    {
        double total = 0;
        for (int i = 0; i < n; i++) total += run(rng);
        return total / n;
    }

    /// <summary>
    /// A ranged duel: both models at the given range and cover. Each round, initiative decides who activates first; each
    /// activation is Aim then Shoot (a model with Suppression Rallies in place of Aim; a Pinned model in the open may not
    /// Shoot, so it Rallies again unless the first Rally unpinned it). The first to put the other Down or worse wins.
    /// Returns 1 if the troop wins, 0 if the reference does, 0.5 for a draw.
    /// </summary>
    public static double Duel(ContentSet content, Rng rng, string troopId, double inches, Cover cover)
    {
        var a = Soldier.FromTroop(content, troopId);
        var b = Soldier.FromTroop(content, ReferenceTroop);
        a.Cover = a.CoverFacing(cover);
        b.Cover = b.CoverFacing(cover);
        for (int round = 1; round <= RoundLimit; round++)
        {
            a.StartRound();
            b.StartRound();
            var (first, second) = AFirst(rng, a, b) ? (a, b) : (b, a);
            Activate(rng, content.Core, first, second, inches);
            if (!second.IsActive) return second == b ? 1 : 0;
            Activate(rng, content.Core, second, first, inches);
            if (!first.IsActive) return first == b ? 1 : 0;
            foreach (var s in new[] { a, b })
                if (!s.HitThisRound && s.Cover != Cover.Open) s.RemoveSuppression(1);
        }
        return 0.5;
    }

    /// <summary>Initiative: 2d6 plus Nerve for a Leader, rerolling ties.</summary>
    private static bool AFirst(Rng rng, Soldier a, Soldier b)
    {
        while (true)
        {
            int ia = Initiative(rng, a), ib = Initiative(rng, b);
            if (ia != ib) return ia > ib;
        }
    }

    private static int Initiative(Rng rng, Soldier s)
    {
        var (x, y) = rng.TwoD6();
        return x + y + (s.IsLeader ? s.Nerve : 0);
    }

    private static void Activate(Rng rng, CoreContent core, Soldier s, Soldier target, double inches)
    {
        int situational = s.Weapon.RangeModifier(inches);
        if (s.Suppression > 0)
        {
            Rally(s);
            if (s.Pinned && s.Cover == Cover.Open) { Rally(s); return; }
        }
        else situational += Shooting.AimModifier(s, target.Cover);
        Shooting.Fire(rng, core, s, target, situational);
    }

    private static void Rally(Soldier s) => s.RemoveSuppression(s.IsLeader ? 2 : 1);

    /// <summary>Close combat to its end under the default break rule; a coin decides who charges.</summary>
    public static double Melee(ContentSet content, Rng rng, string troopId)
    {
        var a = Soldier.FromTroop(content, troopId);
        var b = Soldier.FromTroop(content, ReferenceTroop);
        bool aCharges = rng.D(2) == 1;
        var r = aCharges
            ? CloseCombat.Fight(rng, content.Core, a, b, charging: true)
            : CloseCombat.Fight(rng, content.Core, b, a, charging: true);
        return r.End switch
        {
            CombatEnd.AttackerWon => aCharges ? 1 : 0,
            CombatEnd.DefenderWon => aCharges ? 0 : 1,
            _ => 0.5,
        };
    }

    /// <summary>
    /// Expected total wound rows from one aimed Shoot at 12" against fresh targets in the open with the given Armour.
    /// An Auto N weapon engages N targets, −1 after the first.
    /// </summary>
    public static double ExpectedRows(CoreContent core, Soldier s, int armour)
    {
        int extra = core.AutoFire?.ExtraTargetModifier ?? -1;
        int mod = Shooting.Modifier(s, Cover.Open, s.Weapon.RangeModifier(12) + Shooting.AimModifier(s, Cover.Open));
        return Enumerable.Range(0, s.Weapon.Rate)
            .Sum(i => Shooting.ExpectedRow(core, mod + (i > 0 ? extra : 0), s.Weapon.DamageDice, armour, s.Weapon.Ap));
    }

    public static string Report(ContentSet content, int n, int seed)
    {
        var core = content.Core;
        var reference = content.Troop(ReferenceTroop);
        string version = string.Join('.', core.Version.Split('.').Take(2));
        var md = new StringBuilder();
        md.AppendLine($"# Troop values — content v{core.Version}");
        md.AppendLine();
        md.AppendLine($"Generated by `app/src/Imperator.Sim --values`. Monte Carlo runs: {n:N0} per measure per profile, seed {seed}. Regenerate with `dotnet run -c Release --project app/src/Imperator.Sim -- --values docs/analysis/troop-values-v{version}.md`.");
        md.AppendLine();
        md.AppendLine($"Every troop profile in `content/troops.yaml`, with its default loadout (armour, weapon, melee weapon, gear), against the reference trooper, the {reference.Name} (`{ReferenceTroop}`). These are data for calibrating troop values (decision 6.12); the points formula comes after.");
        md.AppendLine();
        md.AppendLine("- **Duel 12\"**: both models in the open at 12\". Each round, initiative (2d6, plus Nerve for a Leader) decides who activates first. Each activation is Aim then Shoot; a model with Suppression Rallies in place of Aim, and a Pinned model in the open (which may not Shoot) Rallies again unless the first Rally unpinned it. The first to put the other Down or worse wins. A Shield counts as Light cover.");
        md.AppendLine("- **Duel 24\" cover**: the same at 24\", both models in Light cover; end-of-round recovery applies.");
        md.AppendLine("- **Close combat**: the exchange rules of chapter 5, a coin deciding who charges, both using the default break rule (break only after losing while Wounded or at 3+ Suppression; it ends only if both break).");
        md.AppendLine($"- **Score**: the mean of the three win rates. A draw (both broke away, or no result after {RoundLimit} rounds) counts as half a win. The reference scores 50% against itself.");
        md.AppendLine("- **Rows A0 / A2 / A3**: expected total wound rows from one aimed Shoot at 12\" against fresh targets in the open with Armour 0, 2 and 3 (a miss or no effect counts 0, rows cap at 10). An *Auto N* weapon engages N targets, −1 after the first. *Suppressing* adds Suppression, not rows, so it does not show here.");
        md.AppendLine("- No grenades, Moves or reactions. † Automated. \\* has a Proposed skill. N/A: no ranged weapon (its score is close combat alone).");
        md.AppendLine();

        var proposed = content.Skills.Skills.Where(s => s.Status == "proposed").Select(s => s.Id).ToHashSet();
        var values = content.Troops.Troops.Select(t => Measure(content, t.Id, n, seed)).ToList();
        foreach (var faction in content.Factions.Factions)
        {
            var rows = values.Where(v => v.Troop.Faction == faction.Id).OrderByDescending(v => v.Score).ToList();
            if (rows.Count == 0) continue;
            md.AppendLine($"## {faction.Name}");
            md.AppendLine();
            md.AppendLine("| Troop | Weapon | Score | Duel 12\" | Duel 24\" cover | Close combat | Rows A0 | Rows A2 | Rows A3 | Move | Shoot | Fight | Wounds | Armour |");
            md.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|---|---|---|");
            foreach (var v in rows)
            {
                var m = v.Model;
                string marks = (m.Automated ? " †" : "") + (m.Skills.Keys.Any(proposed.Contains) ? " \\*" : "");
                var st = v.Troop.Stats;
                md.AppendLine($"| {v.Troop.Name}{marks} | {(m.Weapon.Has("no_ranged") ? "—" : m.Weapon.Name)} | {Pct(v.Score)} | {Pct(v.Duel12)} | {Pct(v.Duel24)} | {Pct(v.CloseCombat)} | {Num(v.RowsA0)} | {Num(v.RowsA2)} | {Num(v.RowsA3)} | {st.Move}\" | {st.Shoot} | {Stat(st.Fight)} | {st.Wounds} | {m.Armour} |");
            }
            md.AppendLine();

            var ids = rows.SelectMany(v => v.Model.Skills.Keys.Concat(v.Troop.Gear)).ToHashSet();
            var missing = NotModelled.Where(kv => ids.Contains(kv.Key)).Select(kv => kv.Value).ToList();
            if (missing.Count > 0)
            {
                md.AppendLine($"Not modelled: {string.Join("; ", missing)}.");
                md.AppendLine();
            }
        }

        md.AppendLine("Modelled skills: " + string.Join(", ", Modelled.Select(id => content.Skill(id).Name)) +
                      ". Augmented's +1 to Physique tests does not arise (a Down model has lost the duel). Skills not listed under a table have no effect on these measures.");
        return md.ToString();
    }

    private static string Pct(double? p) => p is { } v ? $"{v * 100:0}%" : "N/A";
    private static string Num(double? x) => x is { } v ? $"{v:0.00}" : "N/A";
    private static string Stat(int? x) => x is { } v ? $"{v}" : "—";
}
