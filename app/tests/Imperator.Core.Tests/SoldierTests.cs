using Imperator.Core;
using static Imperator.Core.Tests.TestContent;

namespace Imperator.Core.Tests;

public class SoldierTests
{
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
}
