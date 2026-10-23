using Imperator.Core;
using Imperator.Core.Forces;

namespace Imperator.Core.Tests;

public class ForceCheckTests
{
    private static readonly ContentSet Content = ContentSet.LoadDefault();
    private readonly ForceCheck _check = new(Content);

    private static ForceTeam Clone(int leader = 1, int troopers = 3, int heavy = 1) => new(
        "clone-1", "Clone trooper squad", "line", "clone_squad",
        [new("rep_clone_sergeant", leader), new("rep_clone_trooper", troopers), new("rep_clone_heavy", heavy)]);

    private static ForcePlan Plan(List<ForceTeam>? teams = null, List<ForceCharacter>? characters = null,
        int? tvLimit = 350, int? modelLimit = 8, string setting = "fireteam", List<ForceSoldier>? soldiers = null) =>
        new("Gym 2", "republic", setting, "custom", null, tvLimit, modelLimit,
            teams ?? [], characters ?? [], soldiers ?? []);

    private void Has(string code, ForcePlan plan) =>
        Assert.Contains(_check.Check(plan).Findings, f => f.Code == code);

    [Fact]
    public void GymTwoCloneSquadAndAhsokaAtThreeHundredFifty()
    {
        var plan = Plan([Clone()], [new ForceCharacter("rep_ahsoka", "clone-1")]);
        var result = _check.Check(plan);
        Assert.Equal(50 + 3 * 41 + 48 + 84, result.Tv);
        Assert.Equal(6, result.Models);
        Assert.Empty(result.Findings);
        Has("over_tv", plan with { TvLimit = 300 });
    }

    [Fact]
    public void ChecksTvModelsAndWrongFaction()
    {
        Has("over_tv", Plan([Clone()], tvLimit: 1));
        Has("over_models", Plan([Clone()], modelLimit: 4));
        Has("wrong_faction", Plan([new ForceTeam("wrong", "Wrong", "line", null,
            [new ForceSlot("sep_b1", 3)])]));
    }

    [Fact]
    public void HalfSlotsRoundUpFiveToThreeAndFourToTwo()
    {
        Has("half_slots", Plan([Clone(1, 1, 0)]));
        Assert.DoesNotContain(_check.Check(Plan([Clone(1, 2, 0)])).Findings, f => f.Code == "half_slots");
        var arc = Content.Team("arc_team");
        var reduced = new ForceTeam("arc", arc.Name, arc.Type!, arc.Id,
            [new ForceSlot("rep_arc_leader", 1), new ForceSlot("rep_arc_trooper", 1)]);
        Assert.DoesNotContain(_check.Check(Plan([reduced])).Findings, f => f.Code == "half_slots");
        Has("half_slots", Plan([reduced with { Slots = [new("rep_arc_leader", 1)] }]));
    }

    [Fact]
    public void LeaderAndClassLimitsAreWarnings()
    {
        Has("no_leader", Plan([Clone(0, 3, 1)]));
        Has("type_limits", Plan([Clone(1, 3, 3)]));
        Has("two_characters", Plan([Clone()], [new("rep_ahsoka", "clone-1"), new("rep_obi_wan", "clone-1")]));
        var leaderCharacter = Plan([Clone(0, 3, 1)], [new("rep_ahsoka", "clone-1")]);
        Assert.DoesNotContain(_check.Check(leaderCharacter).Findings, f => f.Code == "no_leader");
    }

    [Fact]
    public void LoneCharacterIsAllowed()
    {
        var result = _check.Check(Plan(characters: [new("rep_ahsoka")]));
        Assert.Empty(result.Findings);
        Assert.Equal(1, result.Fireteams);
    }

    [Fact]
    public void BySoldierGroupSevenIsWarning()
    {
        var soldiers = Enumerable.Range(0, 7).Select(_ => new ForceSoldier("rep_clone_trooper", 2)).ToList();
        Has("group_size", Plan(setting: "soldier", soldiers: soldiers, tvLimit: null, modelLimit: null));
    }
}
