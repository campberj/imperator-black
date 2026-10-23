using Imperator.Core;
using static Imperator.Core.Tests.TestContent;

namespace Imperator.Core.Tests;

public class ShootingTests
{
    [Fact]
    public void Plain2d6HitsAt8() =>
        Assert.Equal(15.0 / 36, 1 - Shooting.Odds(Rules, 0, new DiceSpec(1, 2), 0, 0)[Outcome.Miss], 1e-9);

    [Fact]
    public void NaturalTwoAlwaysMisses() =>
        Assert.Equal(1.0 / 36, Shooting.Odds(Rules, 20, new DiceSpec(1, 2), 0, 0)[Outcome.Miss], 1e-9);

    [Fact]
    public void NaturalDoubleSixAlwaysHits() =>
        Assert.Equal(1.0 / 36, 1 - Shooting.Odds(Rules, -20, new DiceSpec(1, 2), 0, 0)[Outcome.Miss], 1e-9);

    [Fact]
    public void OutcomesSumToOne() =>
        Assert.Equal(1.0, Shooting.Odds(Rules, 1, new DiceSpec(2, 8), 1, 0).Values.Sum(), 1e-9);

    [Fact]
    public void SampledOddsMatchExact()
    {
        var rng = new Rng(42);
        var exact = Shooting.Odds(Rules, 1, new DiceSpec(2, 10), 1, 0);
        int trials = 200_000, misses = 0, outs = 0;
        for (int i = 0; i < trials; i++)
        {
            var r = Shooting.Resolve(rng, Rules, 1, new DiceSpec(2, 10), 1, 0);
            if (!r.Hit) misses++;
            else if (r.Outcome >= Outcome.Down) outs++;
        }
        double exactOut = exact[Outcome.Down] + exact[Outcome.Bleeding] + exact[Outcome.Critical] + exact[Outcome.Dead];
        Assert.Equal(exact[Outcome.Miss], misses / (double)trials, 0.005);
        Assert.Equal(exactOut, outs / (double)trials, 0.005);
    }

    [Fact]
    public void SampledRowsMatchExpectedRow()
    {
        var rng = new Rng(7);
        double exact = Shooting.ExpectedRow(Rules, 1, new DiceSpec(2, 8), 2, 0);
        int trials = 200_000;
        double total = 0;
        for (int i = 0; i < trials; i++)
        {
            var r = Shooting.Resolve(rng, Rules, 1, new DiceSpec(2, 8), 2, 0);
            if (r.Hit) total += Math.Max(0, r.Row);
        }
        Assert.Equal(exact, total / trials, 0.02);
    }

    [Fact]
    public void ExpectedRowRisesWithApAgainstArmour() =>
        Assert.True(Shooting.ExpectedRow(Rules, 0, new DiceSpec(2, 8), 2, 1) > Shooting.ExpectedRow(Rules, 0, new DiceSpec(2, 8), 2, 0));

    [Fact]
    public void MarksmanTwoAimIgnoresLightCover()
    {
        var m1 = Soldier.FromTroop(Set, "tfb_marksman");
        var m2 = Soldier.FromTroop(Set, "rfa_marksman");
        Assert.Equal(2, Shooting.AimModifier(m1, Cover.Light));
        Assert.Equal(3, Shooting.AimModifier(m2, Cover.Light));
        Assert.Equal(2, Shooting.AimModifier(m2, Cover.Open));
        Assert.Equal(1, Shooting.AimModifier(Fresh(), Cover.Light));
    }

    [Fact]
    public void SuppressiveFireIgnoresCoverAndStopsReactions()
    {
        var rng = new Rng(7);
        var rifle = Set.Ranged("lmg");
        int n = 20000, hits = 0;
        for (int k = 0; k < n; k++)
        {
            var shooter = Soldier.Generic("gunner", 2, 0, rifle, null);
            var target = Soldier.Generic("target", 2, 0, rifle, null);
            target.Cover = Cover.Heavy;
            target.OverwatchReactions = 1;
            if (Shooting.Suppress(rng, Rules, shooter, target, 0))
            {
                hits++;
                Assert.True(target.CannotReact);
                Assert.Equal(0, target.OverwatchReactions);
                Assert.True(target.Suppression >= 1);
            }
        }
        // 2d6 + 2 vs 8 with no cover penalty: 26/36 (natural 2 misses, double 6 hits).
        Assert.Equal(26.0 / 36, hits / (double)n, 0.015);
    }
}
