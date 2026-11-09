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

    [Fact]
    public void DecidedEventsCarryTheCurrentSchemaVersion()
    {
        var created = SoloCampaignLifecycle.DecideCreate(Id, "Foundry Moon", "0.5.0");
        var rename = SoloCampaignLifecycle.DecideRename(SoloCampaignLifecycle.Replay([created]), "The Foundry");
        Assert.Equal(SoloCampaignLifecycle.SchemaVersions[SoloCampaignLifecycle.Created], created.SchemaVersion);
        Assert.Equal(SoloCampaignLifecycle.SchemaVersions[SoloCampaignLifecycle.Renamed], rename.SchemaVersion);
    }

    [Fact]
    public void ReplaysAStoredVersionOneEvent()
    {
        // As read back from the database: written by S1 before schema_version existed, defaulted to 1.
        var stored = new SoloCampaignEvent(1, SoloCampaignLifecycle.Created,
            """{"Id":"0a872488-2c84-4162-b93b-f8bced7de299","Name":"Foundry Moon","ContentVersion":"0.5.0"}""", 1);
        var renamed = new SoloCampaignEvent(2, SoloCampaignLifecycle.Renamed, """{"Name":"The Foundry"}""", 1);
        var state = SoloCampaignLifecycle.Replay([stored, renamed]);
        Assert.Equal(new SoloCampaignState(Id, "The Foundry", "0.5.0", 2), state);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(2)]
    [InlineData(99)]
    public void RejectsUnknownSchemaVersions(int version)
    {
        var created = SoloCampaignLifecycle.DecideCreate(Id, "Foundry Moon", "0.5.0");
        var ex = Assert.Throws<InvalidOperationException>(() =>
            SoloCampaignLifecycle.Replay([created with { SchemaVersion = version }]));
        Assert.Contains("schema version", ex.Message);

        var rename = SoloCampaignLifecycle.DecideRename(SoloCampaignLifecycle.Replay([created]), "The Foundry");
        Assert.Throws<InvalidOperationException>(() =>
            SoloCampaignLifecycle.Replay([created, rename with { SchemaVersion = version }]));
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
