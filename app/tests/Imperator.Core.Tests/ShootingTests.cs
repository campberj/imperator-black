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
}
