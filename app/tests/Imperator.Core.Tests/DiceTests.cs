using Imperator.Core;

namespace Imperator.Core.Tests;

public class DiceTests
{
    [Fact]
    public void Parses2d8() => Assert.Equal(new DiceSpec(2, 8), DiceSpec.Parse("2d8"));

    [Fact]
    public void RejectsBadDice() => Assert.ThrowsAny<Exception>(() => DiceSpec.Parse("2x8"));

    [Fact]
    public void ParsesHalfStep() => Assert.Equal(new DiceSpec(1, 8, 10), DiceSpec.Parse("d8+d10"));

    [Fact]
    public void HalfStepSitsBetweenThePairs()
    {
        var half = DiceSpec.Parse("d8+d10");
        Assert.Equal(10.0, half.Mean, 1e-9);
        Assert.Equal(2, half.Min);
        Assert.Equal(18, half.Max);
        Assert.Equal("d8+d10", half.ToString());
        Assert.True(DiceSpec.Parse("2d8").Mean < half.Mean && half.Mean < DiceSpec.Parse("2d10").Mean);
    }

    [Theory]
    [InlineData("2d4")]
    [InlineData("2d6")]
    [InlineData("2d8")]
    [InlineData("2d10")]
    [InlineData("2d12")]
    [InlineData("d6+d8")]
    [InlineData("d10+d12")]
    public void PmfSumsToOne(string spec) => Assert.Equal(1.0, DiceSpec.Parse(spec).Pmf().Values.Sum(), 1e-9);

    [Theory]
    [InlineData("2d4")]
    [InlineData("2d6")]
    [InlineData("2d8")]
    [InlineData("2d10")]
    [InlineData("2d12")]
    [InlineData("d8+d10")]
    public void PmfMeanMatchesSpec(string spec)
    {
        var dice = DiceSpec.Parse(spec);
        Assert.Equal(dice.Mean, dice.Pmf().Sum(kv => kv.Key * kv.Value), 1e-9);
    }
}
