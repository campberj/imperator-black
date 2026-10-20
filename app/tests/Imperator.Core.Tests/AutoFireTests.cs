using Imperator.Core;
using static Imperator.Core.Tests.TestContent;

namespace Imperator.Core.Tests;

// Automatic fire (Auto N), rules v0.5.
public class AutoFireTests
{
    [Theory]
    [InlineData("lmg", 3)]
    [InlineData("smg", 2)]
    [InlineData("rifle", 1)]
    public void RateOfFire(string weapon, int rate) => Assert.Equal(rate, Set.Ranged(weapon).Rate);

    [Fact]
    public void BurstEngagesAtMostNTargets()
    {
        var gunner = Soldier.Generic("g", 1, 1, Set.Ranged("lmg"), Set.Melee("knife"));
        var many = Enumerable.Range(0, 5).Select(_ => Fresh(1)).ToList();
        Assert.Equal(3, Shooting.FireBurst(new Rng(9), Rules, gunner, many, 0).Count);
    }

    [Fact]
    public void NonAutoBurstEngagesOneTarget()
    {
        var rifleman = Soldier.Generic("r", 1, 1, Set.Ranged("rifle"), Set.Melee("knife"));
        var many = Enumerable.Range(0, 5).Select(_ => Fresh(1)).ToList();
        Assert.Single(Shooting.FireBurst(new Rng(9), Rules, rifleman, many, 0));
    }

    [Fact]
    public void ExtraTargetModifierAndZone()
    {
        Assert.Equal(-1, Rules.AutoFire?.ExtraTargetModifier);
        Assert.Equal(3, Rules.AutoFire?.Zone);
    }

    [Fact]
    public void ExtraTargetsAreHitLessOften()
    {
        // LMG gunner Shoot 1 vs armour 3: first target at net +1 = 21/36; extra targets at net 0 = 15/36.
        var gunner = Soldier.Generic("g", 1, 1, Set.Ranged("lmg"), Set.Melee("knife"));
        var rng = new Rng(21); int firstHits = 0, thirdHits = 0, trials = 40_000;
        for (int i = 0; i < trials; i++)
        {
            var ts = Enumerable.Range(0, 3).Select(_ => Fresh(3)).ToList();
            var r = Shooting.FireBurst(rng, Rules, gunner, ts, 0);
            if (r[0].Hit) firstHits++;
            if (r[2].Hit) thirdHits++;
        }
        Assert.Equal(21.0 / 36, firstHits / (double)trials, 0.01);
        Assert.Equal(15.0 / 36, thirdHits / (double)trials, 0.01);
    }
}
