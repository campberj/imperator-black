using Imperator.Core;
using static Imperator.Core.Tests.TestContent;

namespace Imperator.Core.Tests;

public class WoundTests
{
    [Theory]
    [InlineData(2, 1)]
    [InlineData(4, 2)]
    [InlineData(11, 6)]
    [InlineData(16, 9)]
    [InlineData(24, 10)]
    public void TotalMapsToRow(int total, int row) => Assert.Equal(row, Wounds.RowFor(Rules, total));

    [Theory]
    [InlineData(12, false, 2, 0, 5)]  // armour steps down
    [InlineData(12, false, 2, 1, 6)]  // AP reduces armour
    [InlineData(12, false, 0, 1, 7)]  // AP cannot raise the row
    [InlineData(12, true, 0, 0, 8)]   // critical adds a row
    [InlineData(24, true, 0, 0, 10)]  // capped at row 10 after armour
    [InlineData(24, true, 1, 0, 10)]  // critical at the top vs armour 1 is still row 10
    public void FinalRow(int total, bool critical, int armour, int ap, int row) =>
        Assert.Equal(row, Wounds.FinalRow(Rules, total, critical, armour, ap));
}
