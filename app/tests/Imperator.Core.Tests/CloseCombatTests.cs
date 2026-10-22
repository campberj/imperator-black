using Imperator.Core;
using static Imperator.Core.Tests.TestContent;

namespace Imperator.Core.Tests;

public class CloseCombatTests
{
    [Fact]
    public void AlwaysResolvesWithLoserOut()
    {
        var rng = new Rng(3);
        int ended = 0; bool consistent = true;
        for (int i = 0; i < 20_000; i++)
        {
            var a = Fresh(1); var d = Fresh(1);
            var r = CloseCombat.Fight(rng, Rules, a, d, charging: true);
            if (r.End != CombatEnd.ExchangeLimit) ended++;
            if (r.End == CombatEnd.AttackerWon && d.IsActive) consistent = false;
            if (r.End == CombatEnd.DefenderWon && a.IsActive) consistent = false;
        }
        Assert.Equal(20_000, ended);
        Assert.True(consistent, "winner's opponent is always out");
    }

    [Fact]
    public void ArmourStalematesEndByOverpowering()
    {
        // Unarmed vs heavy plate cannot wound, but losing still adds Suppression, so it ends by overpowering.
        var rng = new Rng(5);
        int limit = 0;
        for (int i = 0; i < 5_000; i++)
        {
            var d = Soldier.Generic("h", 1, 3, Set.Ranged("rifle"), Set.Melee("unarmed"));
            var a = Soldier.Generic("u", 1, 3, Set.Ranged("rifle"), Set.Melee("unarmed"));
            if (CloseCombat.Fight(rng, Rules, a, d, false, CloseCombat.NeverBreak, CloseCombat.NeverBreak).End == CombatEnd.ExchangeLimit) limit++;
        }
        Assert.Equal(0, limit);
    }

    [Fact]
    public void NoMeleeWeaponNeverWins()
    {
        var rng = new Rng(11); int turretWins = 0;
        for (int i = 0; i < 2_000; i++)
        {
            var a = Fresh(0); var t = Soldier.FromTroop(Set, "fs_turret");
            if (CloseCombat.Fight(rng, Rules, a, t, true).End == CombatEnd.DefenderWon) turretWins++;
        }
        Assert.Equal(0, turretWins);
    }

    [Fact]
    public void TwinBladesAddOneToExchanges()
    {
        var ahsoka = Soldier.FromTroop(Set, "rep_ahsoka");
        var guardian = Soldier.FromTroop(Set, "rep_jedi_guardian");
        Assert.Equal(ahsoka.Fight, guardian.Fight);
        Assert.Equal(CloseCombat.Modifier(guardian, false, 0, false) + 1, CloseCombat.Modifier(ahsoka, false, 0, false));
    }
}
