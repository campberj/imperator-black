using Imperator.Core;

namespace Imperator.Core.Tests;

public class DiceTests
{
    [Fact]
    public void Parses2d8() => Assert.Equal(new DiceSpec(2, 8), DiceSpec.Parse("2d8"));

    [Fact]
    public void RejectsBadDice() => Assert.ThrowsAny<Exception>(() => DiceSpec.Parse("2x8"));

    [Theory]
    [InlineData("2d4")]
    [InlineData("2d6")]
    [InlineData("2d8")]
    [InlineData("2d10")]
    [InlineData("2d12")]
    public void PmfSumsToOne(string spec) => Assert.Equal(1.0, DiceSpec.Parse(spec).Pmf().Values.Sum(), 1e-9);

    [Theory]
    [InlineData("2d4")]
    [InlineData("2d6")]
    [InlineData("2d8")]
    [InlineData("2d10")]
    [InlineData("2d12")]
    public void PmfMeanMatchesSpec(string spec)
    {
        var dice = DiceSpec.Parse(spec);
        Assert.Equal(dice.Mean, dice.Pmf().Sum(kv => kv.Key * kv.Value), 1e-9);
    }
}
