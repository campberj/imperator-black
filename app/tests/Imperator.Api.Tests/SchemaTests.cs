using Imperator.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Imperator.Api.Tests;

// Model and change-tracking checks; none of these open a database connection.
public class SchemaTests
{
    private static ImperatorDb NewDb() => new(new DbContextOptionsBuilder<ImperatorDb>()
        .UseNpgsql(ApiFactory.UnreachableDatabase)
        .UseSnakeCaseNamingConvention()
        .Options);

    private static CampaignEvent AnEvent() => new()
    {
        Id = Guid.NewGuid(), CampaignId = Guid.NewGuid(), Sequence = 1, Type = "TaskingIssued", Payload = "{}",
        CreatedAt = DateTimeOffset.UtcNow, CreatedBy = Guid.NewGuid(),
    };

    [Fact]
    public void EventSequenceIsUniquePerCampaign()
    {
        using var db = NewDb();
        var events = db.Model.FindEntityType(typeof(CampaignEvent))!;
        var index = events.GetIndexes().Single(i => i.Properties.Select(p => p.Name).SequenceEqual(["CampaignId", "Sequence"]));
        Assert.True(index.IsUnique);
        Assert.Equal("jsonb", events.FindProperty(nameof(CampaignEvent.Payload))!.GetColumnType());
        Assert.Equal("campaign_events", events.GetTableName());
    }

    [Fact]
    public void MembershipRoleIsStoredByName()
    {
        using var db = NewDb();
        var role = db.Model.FindEntityType(typeof(CampaignMembership))!.FindProperty(nameof(CampaignMembership.Role))!;
        Assert.Equal("character varying(20)", role.GetColumnType());
    }

    [Fact]
    public void ModifyingAnEventIsRefused()
    {
        using var db = NewDb();
        var e = AnEvent();
        db.Attach(e);
        e.Payload = """{"edited":true}""";
        var ex = Assert.Throws<InvalidOperationException>(() => db.SaveChanges());
        Assert.Contains("append-only", ex.Message);
    }

    [Fact]
    public async Task DeletingAnEventIsRefused()
    {
        using var db = NewDb();
        db.Remove(db.Attach(AnEvent()).Entity);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync(TestContext.Current.CancellationToken));
        Assert.Contains("append-only", ex.Message);
    }
}
