using Imperator.Core;

// Dependency-free test runner: exits non-zero if any check fails.
// Run: dotnet run --project app/tests/Imperator.Core.Tests
var content = ContentSet.LoadDefault(Directory.GetCurrentDirectory());
var core = content.Core;
int failures = 0, passed = 0;

void Check(string name, bool ok, string detail = "")
{
    if (ok) { passed++; return; }
    failures++;
    Console.WriteLine($"FAIL: {name} {detail}");
}

void Near(string name, double actual, double expected, double tol = 1e-9) =>
    Check(name, Math.Abs(actual - expected) <= tol, $"(expected {expected:0.#####}, got {actual:0.#####})");

// --- Dice ---
Check("parse 2d8", DiceSpec.Parse("2d8") == new DiceSpec(2, 8));
Check("bad dice rejected", Throws(() => DiceSpec.Parse("2x8")));
foreach (var spec in new[] { "2d4", "2d6", "2d8", "2d10", "2d12" })
{
    var pmf = DiceSpec.Parse(spec).Pmf();
    Near($"{spec} pmf sums to 1", pmf.Values.Sum(), 1.0);
    Near($"{spec} mean", pmf.Sum(kv => kv.Key * kv.Value), DiceSpec.Parse(spec).Mean);
}

// --- Content ---
Check("content version", core.Version == "0.2.0", core.Version);
Check("hit target 8", core.HitTarget == 8);
Check("ten wound rows", core.WoundTable.Count == 10);
Check("rifle is 2d8", content.Ranged("rifle").DamageDice == new DiceSpec(2, 8));
Check("shotgun penalties -2/-3", content.Ranged("shotgun").PenaltyBeyondShort == -2 && content.Ranged("shotgun").PenaltyBeyondDouble == -3);
Check("rifle default penalties", content.Ranged("rifle").RangeModifier(31) == -1 && content.Ranged("rifle").RangeModifier(61) == -2 && content.Ranged("rifle").RangeModifier(30) == 0);

// --- Wound table ---
Check("total 2 is row 1", Wounds.RowFor(core, 2) == 1);
Check("total 13 is row 6", Wounds.RowFor(core, 13) == 6);
Check("total 24 is row 10", Wounds.RowFor(core, 24) == 10);
Check("armour steps down", Wounds.FinalRow(core, 12, false, 2, 0) == 4);
Check("AP reduces armour", Wounds.FinalRow(core, 12, false, 2, 1) == 5);
Check("AP cannot raise row", Wounds.FinalRow(core, 12, false, 0, 1) == 6);
Check("critical adds a row", Wounds.FinalRow(core, 12, true, 0, 0) == 7);
Check("row capped at 10 after armour", Wounds.FinalRow(core, 24, true, 0, 0) == 10);
Check("crit at top vs armour 1 still row 10", Wounds.FinalRow(core, 24, true, 1, 0) == 10);

// --- Hit odds ---
var plain = Shooting.Odds(core, 0, new DiceSpec(1, 2), 0, 0);
Near("P(2d6 >= 8) = 15/36", 1 - plain[Outcome.Miss], 15.0 / 36);
var huge = Shooting.Odds(core, 20, new DiceSpec(1, 2), 0, 0);
Near("natural 2 always misses", huge[Outcome.Miss], 1.0 / 36);
var hopeless = Shooting.Odds(core, -20, new DiceSpec(1, 2), 0, 0);
Near("natural double 6 always hits", 1 - hopeless[Outcome.Miss], 1.0 / 36);
Near("outcomes sum to 1", Shooting.Odds(core, 1, new DiceSpec(2, 8), 1, 0).Values.Sum(), 1.0);

// Exact vs sampled agreement
{
    var rng = new Rng(42);
    var exact = Shooting.Odds(core, 1, new DiceSpec(2, 10), 1, 0);
    int trials = 200_000, misses = 0, outs = 0;
    for (int i = 0; i < trials; i++)
    {
        var r = Shooting.Resolve(rng, core, 1, new DiceSpec(2, 10), 1, 0);
        if (!r.Hit) misses++;
        else if (r.Outcome >= Outcome.Down) outs++;
    }
    double exactOut = exact[Outcome.Down] + exact[Outcome.Bleeding] + exact[Outcome.Critical] + exact[Outcome.Dead];
    Near("sampled miss rate matches exact", misses / (double)trials, exact[Outcome.Miss], 0.005);
    Near("sampled out rate matches exact", outs / (double)trials, exactOut, 0.005);
}

// --- Soldier state ---
Soldier Fresh(int armour = 0) => new() { Name = "t", Q = 1, Armour = armour, Weapon = content.Ranged("rifle"), Melee = content.Melee("knife") };
{
    var s = Fresh(); s.ApplyRow(2);
    Check("row 2 adds 2 Suppression", s.Suppression == 2);
    s.ApplyRow(2); s.ApplyRow(2);
    Check("Suppression capped at 5", s.Suppression == 5);
    Check("pinned at 3+", s.Pinned);
}
{
    var s = Fresh(); s.ApplyRow(4);
    Check("row 4 wounds", s.Wounded && s.IsActive);
    s.ApplyRow(4);
    Check("second wound puts Down", s.Status == Status.Down);
}
{
    var s = Fresh(); s.OverwatchReactions = 2; s.AddSuppression(2);
    Check("2 Suppression cancels Overwatch", s.OverwatchReactions == 0);
}
{
    var s = Fresh(); s.ApplyRow(7);
    Check("row 7 bleeding", s.Status == Status.Bleeding);
    s.TickDeathClock(); s.TickDeathClock();
    Check("bleeding alive after 2 rounds", s.Status == Status.Bleeding);
    s.TickDeathClock();
    Check("bleeding dies after 3 rounds", s.Status == Status.Dead);
}
{
    var s = Fresh(); s.ApplyRow(8); s.TickDeathClock();
    Check("critical dies after 1 round", s.Status == Status.Dead);
}
{
    var s = Fresh(); s.ApplyRow(6);
    s.Treat(new Rng(1), 1, medicKeyword: true);
    Check("Medic keyword auto-recovers Down", s.IsActive && s.Wounded && s.Suppression == 3);
}
{
    int recovered = 0, trials = 100_000;
    var rng = new Rng(7);
    for (int i = 0; i < trials; i++) { var s = Fresh(); s.ApplyRow(6); if (s.TryRecover(rng)) recovered++; }
    Near("Q1 Down recovery = P(2d6 >= 7, not 2) = 21/36", recovered / (double)trials, 21.0 / 36, 0.005);
}

// --- Close combat ---
{
    var rng = new Rng(3);
    int ended = 0; bool consistent = true;
    for (int i = 0; i < 20_000; i++)
    {
        var a = Fresh(1); var d = Fresh(1);
        var r = CloseCombat.Fight(rng, core, a, d, charging: true);
        if (r.End != CombatEnd.ExchangeLimit) ended++;
        if (r.End == CombatEnd.AttackerWon && d.IsActive) consistent = false;
        if (r.End == CombatEnd.DefenderWon && a.IsActive) consistent = false;
    }
    Check("close combat always resolves", ended == 20_000, $"({ended})");
    Check("winner's opponent is always out", consistent);
}
{
    // Unarmed vs heavy plate cannot wound, but losing still adds Suppression, so it ends by overpowering.
    var rng = new Rng(5);
    int limit = 0;
    for (int i = 0; i < 5_000; i++)
    {
        var d = new Soldier { Name = "h", Q = 1, Armour = 3, Weapon = content.Ranged("rifle"), Melee = content.Melee("unarmed") };
        var a2 = new Soldier { Name = "u", Q = 1, Armour = 3, Weapon = content.Ranged("rifle"), Melee = content.Melee("unarmed") };
        if (CloseCombat.Fight(rng, core, a2, d, false, CloseCombat.NeverBreak, CloseCombat.NeverBreak).End == CombatEnd.ExchangeLimit) limit++;
    }
    Check("armour stalemates end by overpowering", limit == 0, $"({limit} hit the limit)");
}

Console.WriteLine(failures == 0 ? $"All {passed} checks passed." : $"{failures} failed, {passed} passed.");
return failures == 0 ? 0 : 1;

static bool Throws(Action a)
{
    try { a(); return false; } catch { return true; }
}
