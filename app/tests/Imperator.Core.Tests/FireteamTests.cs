using Imperator.Core;

namespace Imperator.Core.Tests;

public class FireteamTests
{
    private static readonly ContentSet Content = ContentSet.LoadDefault();

    [Fact]
    public void EveryStandardFireteamFitsItsType()
    {
        foreach (var team in Content.Rosters.Teams)
            Assert.Empty(Fireteams.Check(Content, team.Type,
                team.Slots.Select(s => new FireteamMember(s.Troop, s.Count))));
        foreach (var element in Content.Factions.Elements)
            Assert.Empty(Fireteams.Check(Content, element.Type,
                element.Composition.Select(p => new FireteamMember(p.Troop, p.Count))));
    }

    [Fact]
    public void RejectsOverfilledFireteamsAndExcessCharacters()
    {
        Assert.Contains(Fireteams.Check(Content, "line",
            [new FireteamMember("rep_clone_trooper", 7)]),
            f => f.Contains("more than 6 models"));
        var character = Content.Troops.Troops.First(t => t.Class == "character");
        Assert.Contains("type character is one character acting alone",
            Fireteams.Check(Content, "character", [new FireteamMember(character.Id, 2)]));
    }

    [Fact]
    public void PinningComesFromCoreAndAutomatedNeverHasIt()
    {
        Assert.Equal("Pinned at 4 Suppression", Fireteams.PinningText(Content, "rep_clone_trooper"));
        Assert.Equal("Never Pinned", Fireteams.PinningText(Content, "rep_obi_wan"));
        var automated = Content.Troops.Troops.First(t => t.Skills.ContainsKey("automated"));
        Assert.Null(Fireteams.PinningText(Content, automated.Id));
    }
}
