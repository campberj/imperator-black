using Imperator.Core;
using Imperator.Sim;

namespace Imperator.Sim.Tests;

public class ValuesTests
{
    private static readonly Lazy<ContentSet> _set = new(() => ContentSet.LoadDefault(AppContext.BaseDirectory));
    private static ContentSet Set => _set.Value;

    [Fact]
    public void ReferenceScoresHalfAgainstItself()
    {
        var v = Values.Measure(Set, Values.ReferenceTroop, 20_000, 1);
        Assert.Equal(0.5, v.Score, 0.02);
        Assert.Equal(0.5, v.Duel12!.Value, 0.02);
        Assert.Equal(0.5, v.Duel24!.Value, 0.02);
        Assert.Equal(0.5, v.CloseCombat, 0.02);
    }

    [Fact]
    public void NoRangedWeaponScoresOnCloseCombatAlone()
    {
        var v = Values.Measure(Set, "rep_jedi_guardian", 500, 1);
        Assert.Null(v.Duel12);
        Assert.Null(v.Duel24);
        Assert.Null(v.RowsA0);
        Assert.Equal(v.CloseCombat, v.Score);
    }

    [Fact]
    public void AutoWeaponsCountEveryTargetInTheBurst()
    {
        var gunner = Soldier.FromTroop(Set, "tfb_vat_gunner");
        var assaulter = Soldier.FromTroop(Set, Values.ReferenceTroop);
        Assert.True(Values.ExpectedRows(Set.Core, gunner, 0) > 2 * Values.ExpectedRows(Set.Core, assaulter, 0));
    }

    [Fact]
    public void ReportHasARowForEveryTroop()
    {
        var rows = Values.Report(Set, 50, 1).Split('\n')
            .Count(l => l.StartsWith("| ") && !l.StartsWith("| Troop |"));
        Assert.Equal(Set.Troops.Troops.Count, rows);
    }
}
