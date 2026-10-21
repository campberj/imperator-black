using Imperator.Core;
using static Imperator.Core.Tests.TestContent;

namespace Imperator.Core.Tests;

public class TroopTests
{
    public static TheoryData<string> TroopIds() => new(Set.Troops.Troops.Select(t => t.Id));

    [Theory]
    [MemberData(nameof(TroopIds))]
    public void TroopProfileAndWeaponOptionsLoad(string id)
    {
        var t = Set.Troops.Troops.Single(t => t.Id == id);
        var s = Soldier.FromTroop(Set, id);
        foreach (var w in t.WeaponOptions ?? new()) Soldier.FromTroop(Set, id, w);
        Assert.Equal(t.Stats.Move, s.Move);
        Assert.Equal(t.Stats.Shoot, s.Shoot);
    }

    [Fact]
    public void AutomatedIgnoresSuppressionAndDiesAtRow6()
    {
        var turret = Soldier.FromTroop(Set, "fs_turret");
        turret.AddSuppression(3);
        Assert.Equal(0, turret.Suppression);
        turret.ApplyRow(6);
        Assert.Equal(Status.Dead, turret.Status);
        Assert.Null(turret.Melee);
    }

    [Fact]
    public void JediHasNoRangedWeaponButALightsaber()
    {
        Assert.Null(Set.Troop("rep_ahsoka").Weapon);
        var ahsoka = Soldier.FromTroop(Set, "rep_ahsoka");
        Assert.Same(RangedWeapon.None, ahsoka.Weapon);
        Assert.Equal("lightsaber", ahsoka.Melee!.Id);
        Assert.Equal(2, ahsoka.Melee.Ap);
    }

    [Fact]
    public void TraumaKitIsAnOptionNotDefault() => Assert.False(Soldier.FromTroop(Set, "tfb_pathfinder").IsMedic);

    [Fact]
    public void SilencerAndLowLightOptics()
    {
        var liq = Soldier.FromTroop(Set, "fs_liquidator");
        Assert.Contains("silenced", liq.WeaponKeywords);
        Assert.Equal(1, liq.SkillLevel("low_light"));
    }

    [Fact]
    public void MarksmanAimsAtPlus2() => Assert.Equal(2, Soldier.FromTroop(Set, "tfb_marksman").AimBonus);

    [Fact]
    public void LeaderSkillLoads() => Assert.True(Soldier.FromTroop(Set, "tfb_vat_leader").IsLeader);
}
