using Imperator.Core.Campaigns;

namespace Imperator.Core.Tests;

public sealed class SoloCampaignLifecycleTests
{
    private static readonly Guid Id = Guid.Parse("0a872488-2c84-4162-b93b-f8bced7de299");

    [Fact]
    public void CreationAndRenameReplayDeterministically()
    {
        var created = SoloCampaignLifecycle.DecideCreate(Id, "  Foundry Moon  ", "0.5.0");
        var start = SoloCampaignLifecycle.Apply(null, created);
        Assert.Equal("Foundry Moon", start.Name);
        Assert.Equal("0.5.0", start.ContentVersion);
        Assert.Equal(1, start.Version);

        var rename = SoloCampaignLifecycle.DecideRename(start, "The Foundry");
        var expected = SoloCampaignLifecycle.Apply(start, rename);
        var rebuilt = SoloCampaignLifecycle.Replay([created, rename]);
        Assert.Equal(expected, rebuilt);
        Assert.Equal(2, rebuilt.Version);
        Assert.Equal(Id, rebuilt.Id);
        Assert.Equal("The Foundry", rebuilt.Name);
    }

    [Fact]
    public void RejectsUnknownOrBrokenEventHistory()
    {
        var create = SoloCampaignLifecycle.DecideCreate(Id, "Foundry Moon", "0.5.0");
        Assert.Throws<InvalidOperationException>(() => SoloCampaignLifecycle.Replay([]));
        Assert.Throws<InvalidOperationException>(() => SoloCampaignLifecycle.Replay([create with { Sequence = 2 }]));
        Assert.Throws<InvalidOperationException>(() => SoloCampaignLifecycle.Replay([create, create]));
        Assert.Throws<InvalidOperationException>(() => SoloCampaignLifecycle.Replay([create, new(2, "BattleWon", "{}")]));
        Assert.Throws<InvalidOperationException>(() => SoloCampaignLifecycle.Replay([create with { Payload = "{" }]));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void EmptyNamesAreNotValid(string name)
    {
        Assert.Throws<ArgumentException>(() => SoloCampaignLifecycle.DecideCreate(Id, name, "0.5.0"));
    }

    [Fact]
    public void TooLongOrDuplicateNamesAreRejected()
    {
        Assert.Throws<ArgumentException>(() => SoloCampaignLifecycle.DecideCreate(Id, new string('x', 201), "0.5.0"));
        var created = SoloCampaignLifecycle.DecideCreate(Id, "Foundry Moon", "0.5.0");
        var state = SoloCampaignLifecycle.Replay([created]);
        Assert.Throws<ArgumentException>(() => SoloCampaignLifecycle.DecideRename(state, " Foundry Moon "));
    }
}
