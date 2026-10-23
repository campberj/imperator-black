using Imperator.Core;
using static Imperator.Core.Tests.TestContent;

namespace Imperator.Core.Tests;

public class ContentTests
{
    [Fact]
    public void ContentVersion() => Assert.Equal("0.5.0", Rules.Version);

    [Fact]
    public void HitTargetIs8() => Assert.Equal(8, Rules.HitTarget);

    [Fact]
    public void TenWoundRows() => Assert.Equal(10, Rules.WoundTable.Count);

    [Fact]
    public void WoundBandsContiguousFrom2()
    {
        Assert.Equal(2, Rules.WoundTable[0].Min);
        Assert.All(Rules.WoundTable.Zip(Rules.WoundTable.Skip(1)), p => Assert.Equal(p.First.Max + 1, p.Second.Min));
    }

    [Fact]
    public void RifleIs2d8() => Assert.Equal(new DiceSpec(2, 8), Set.Ranged("rifle").DamageDice);

    [Fact]
    public void OverwatchGivesOneReaction() => Assert.Equal(1, Rules.OverwatchReactions);

    [Fact]
    public void AtLauncherHasAHighExplosiveRound()
    {
        var w = Set.Ranged("at_launcher");
        Assert.Equal("Anti-armour", w.Round);
        var he = Assert.Single(w.Rounds!);
        Assert.Equal(new DiceSpec(1, 8, 10), he.DamageDice);
        Assert.Equal(2, he.Explosion);
        Assert.Equal(0, he.Ap);
    }

    [Fact]
    public void ShotgunPenalties()
    {
        var shotgun = Set.Ranged("shotgun");
        Assert.Equal(-2, shotgun.PenaltyBeyondShort);
        Assert.Equal(-3, shotgun.PenaltyBeyondDouble);
    }

    [Theory]
    [InlineData(30, 0)]
    [InlineData(31, -1)]
    [InlineData(61, -2)]
    public void RifleDefaultPenalties(int range, int modifier) => Assert.Equal(modifier, Set.Ranged("rifle").RangeModifier(range));
}
