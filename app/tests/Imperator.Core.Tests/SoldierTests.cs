using Imperator.Core;
using static Imperator.Core.Tests.TestContent;

namespace Imperator.Core.Tests;

public class SoldierTests
{
    [Fact]
    public void PinnedAtThreeByDefaultAndByNerveWhenContentSaysSo()
    {
        var clone = Soldier.FromTroop(Set, "rep_clone_trooper");   // Nerve 2
        Assert.Equal(3, clone.PinnedAt);
        var byNerve = Rules with { Suppression = (Rules.Suppression ?? new SuppressionDef()) with { PinnedByNerve = 1 } };
        clone.UsePinningRule(byNerve);
        Assert.Equal(3, clone.PinnedAt);                            // Nerve 2 + 1
        var vat = Soldier.FromTroop(Set, "tfb_vat_assaulter");     // Nerve 1
        vat.UsePinningRule(byNerve);
        Assert.Equal(2, vat.PinnedAt);
        vat.UsePinningRule(byNerve with { Suppression = byNerve.Suppression! with { PinnedAtLeast = 3 } });
        Assert.Equal(3, vat.PinnedAt);
        vat.UsePinningRule(Rules with { Suppression = new SuppressionDef(PinnedByNerve: 0, PinnedNerveFactor: 2) });
        Assert.Equal(2, vat.PinnedAt);                              // double Nerve 1
    }

    [Fact]
    public void Row2AddsSuppressionCappedAt5()
    {
        var s = Fresh(); s.ApplyRow(2);
        Assert.Equal(2, s.Suppression);
        s.ApplyRow(2); s.ApplyRow(2);
        Assert.Equal(5, s.Suppression);
        Assert.True(s.Pinned, "pinned at 3+");
    }

    [Fact]
    public void SecondWoundPutsDown()
    {
        var s = Fresh(); s.ApplyRow(4);
        Assert.True(s.Wounded && s.IsActive, "row 4 wounds");
        s.ApplyRow(4);
        Assert.Equal(Status.Down, s.Status);
    }

    [Fact]
    public void TwoSuppressionCancelsOverwatch()
    {
        var s = Fresh(); s.OverwatchReactions = 2; s.AddSuppression(2);
        Assert.Equal(0, s.OverwatchReactions);
    }

    [Fact]
    public void BleedingDiesAfterThreeRounds()
    {
        var s = Fresh(); s.ApplyRow(7);
        Assert.Equal(Status.Bleeding, s.Status);
        s.TickDeathClock(); s.TickDeathClock();
        Assert.Equal(Status.Bleeding, s.Status);
        s.TickDeathClock();
        Assert.Equal(Status.Dead, s.Status);
    }

    [Fact]
    public void CriticalDiesAfterOneRound()
    {
        var s = Fresh(); s.ApplyRow(8); s.TickDeathClock();
        Assert.Equal(Status.Dead, s.Status);
    }

    [Fact]
    public void MedicSkillAutoRecoversDown()
    {
        var s = Fresh(); s.ApplyRow(6);
        s.Treat(new Rng(1), 1, medicSkill: true);
        Assert.True(s.IsActive);
        Assert.Equal(s.Wounds, s.WoundedMarkers);
        Assert.Equal(3, s.Suppression);
    }

    [Fact]
    public void Q1DownRecoveryRate()
    {
        // P(2d6 >= 7, not a natural 2) = 21/36
        int recovered = 0, trials = 100_000;
        var rng = new Rng(7);
        for (int i = 0; i < trials; i++) { var s = Fresh(); s.ApplyRow(6); if (s.TryRecover(rng)) recovered++; }
        Assert.Equal(21.0 / 36, recovered / (double)trials, 0.005);
    }

    [Fact]
    public void Wounds2CarriesTwoMarkers()
    {
        var s = Fresh(wounds: 2); s.ApplyRow(4); s.ApplyRow(5);
        Assert.True(s.IsActive);
        Assert.Equal(2, s.WoundedMarkers);
        s.ApplyRow(4);
        Assert.Equal(Status.Down, s.Status);
    }

    [Fact]
    public void Row6IsDownRegardlessOfWounds()
    {
        var s = Fresh(wounds: 2); s.ApplyRow(6);
        Assert.Equal(Status.Down, s.Status);
    }

    [Fact]
    public void AugmentedIgnoresFirstSuppressionEachRound()
    {
        var s = Soldier.FromTroop(Set, "rfa_assaulter");
        s.AddSuppression(2);
        Assert.Equal(1, s.Suppression);
        s.AddSuppression(1);
        Assert.Equal(2, s.Suppression);
        s.StartRound();
        s.AddSuppression(1);
        Assert.Equal(2, s.Suppression);
    }

    [Fact]
    public void JediDisciplinePinsAtFour()
    {
        var s = Soldier.FromTroop(Set, "rep_jedi_guardian");
        s.AddSuppression(3);
        Assert.False(s.Pinned);
        s.AddSuppression(1);
        Assert.True(s.Pinned);
    }

    [Fact]
    public void ShieldIsLightCoverOnlyWithoutOther()
    {
        var s = Soldier.FromTroop(Set, "ct_borz_bratva");
        Assert.Equal(Cover.Light, s.CoverFacing(Cover.Open));
        Assert.Equal(Cover.Heavy, s.CoverFacing(Cover.Heavy));
        Assert.Equal(Cover.Open, Fresh().CoverFacing(Cover.Open));
    }
}
