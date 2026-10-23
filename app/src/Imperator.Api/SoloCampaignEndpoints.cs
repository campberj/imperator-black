using Imperator.Api.Data;
using Imperator.Core;
using Imperator.Core.Campaigns;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Imperator.Api;

/// <summary>
/// S1 single-user development API. Deliberately not mapped outside Development until authentication
/// and campaign membership checks are implemented in the later multiplayer phase.
/// </summary>
public static class SoloCampaignEndpoints
{
    public sealed record CreateCampaignRequest(string? Name);
    public sealed record RenameCampaignRequest(string? Name);
    public sealed record CampaignSummary(Guid Id, string Name, long Version, string ContentVersion, DateTimeOffset CreatedAt);
    public sealed record CampaignEventResponse(long Sequence, string Type, string Payload, DateTimeOffset CreatedAt);

    public static void MapSoloCampaigns(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            throw new InvalidOperationException("Unauthenticated solo campaign endpoints are development-only.");

        var routes = app.MapGroup("/campaigns");
        routes.MapPost("/", Create);
        routes.MapGet("/", List);
        routes.MapGet("/{id:guid}", Load);
        routes.MapGet("/{id:guid}/events", History);
        routes.MapPatch("/{id:guid}/name", Rename);
    }

    private static Guid LocalPlayerId(IConfiguration config)
    {
        if (Guid.TryParse(config["SoloCampaigns:LocalPlayerId"], out var id) && id != Guid.Empty)
            return id;
        throw new InvalidOperationException("Development SoloCampaigns:LocalPlayerId must be a non-empty GUID.");
    }

    private static async Task<Player> EnsureLocalPlayer(ImperatorDb db, IConfiguration config, CancellationToken ct)
    {
        var id = LocalPlayerId(config);
        var player = await db.Players.FindAsync([id], ct);
        if (player is not null) return player;
        player = new Player
        {
            Id = id,
            DisplayName = config["SoloCampaigns:LocalPlayerName"] ?? "Local Commander",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Players.Add(player);
        return player;
    }

    private static SoloCampaignState Project(IEnumerable<CampaignEvent> events) =>
        SoloCampaignLifecycle.Replay(events.Select(e => new SoloCampaignEvent(e.Sequence, e.Type, e.Payload)));

    private static async Task<List<CampaignEvent>> Events(ImperatorDb db, Guid campaignId, CancellationToken ct) =>
        await db.CampaignEvents.AsNoTracking()
            .Where(e => e.CampaignId == campaignId)
            .OrderBy(e => e.Sequence).ToListAsync(ct);

    private static async Task<Campaign?> Owned(ImperatorDb db, IConfiguration config, Guid id, CancellationToken ct)
    {
        var playerId = LocalPlayerId(config);
        return await db.Campaigns
            .FirstOrDefaultAsync(c => c.Id == id && c.Members.Any(m => m.PlayerId == playerId), ct);
    }

    private static IResult BadName(string? name)
    {
        try { SoloCampaignLifecycle.ValidName(name); return Results.Ok(); }
        catch (ArgumentException e) { return Results.BadRequest(new { error = e.Message }); }
    }

    private static async Task<IResult> Create(CreateCampaignRequest request, ImperatorDb db,
        IConfiguration config, ContentFile content, CancellationToken ct)
    {
        try { SoloCampaignLifecycle.ValidName(request.Name); }
        catch (ArgumentException e) { return Results.BadRequest(new { error = e.Message }); }

        var now = DateTimeOffset.UtcNow;
        var player = await EnsureLocalPlayer(db, config, ct);
        var id = Guid.NewGuid();
        var change = SoloCampaignLifecycle.DecideCreate(id, request.Name!, content.Version);
        var campaign = new Campaign { Id = id, Name = SoloCampaignLifecycle.ValidName(request.Name), CreatedAt = now };
        db.Campaigns.Add(campaign);
        db.CampaignMemberships.Add(new CampaignMembership
        {
            CampaignId = id, PlayerId = player.Id, Role = CampaignRole.Commander, JoinedAt = now
        });
        db.CampaignEvents.Add(new CampaignEvent
        {
            Id = Guid.NewGuid(), CampaignId = id, Sequence = change.Sequence,
            Type = change.Type, Payload = change.Payload, CreatedAt = now, CreatedBy = player.Id
        });
        // EF inserts the player, campaign, membership and first event atomically in one SaveChanges.
        await db.SaveChangesAsync(ct);
        return Results.Created($"/campaigns/{id}", SoloCampaignLifecycle.Replay([change]));
    }

    private static async Task<IResult> List(ImperatorDb db, IConfiguration config, CancellationToken ct)
    {
        var player = LocalPlayerId(config);
        var campaigns = await db.Campaigns.AsNoTracking()
            .Where(c => c.Members.Any(m => m.PlayerId == player))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

        if (campaigns.Count == 0) return Results.Ok(Array.Empty<CampaignSummary>());
        var ids = campaigns.Select(c => c.Id).ToArray();
        var events = await db.CampaignEvents.AsNoTracking()
            .Where(e => ids.Contains(e.CampaignId))
            .OrderBy(e => e.Sequence).ToListAsync(ct);
        var eventGroups = events.ToLookup(e => e.CampaignId);

        return Results.Ok(campaigns.Select(c =>
        {
            var state = Project(eventGroups[c.Id]);
            return new CampaignSummary(c.Id, state.Name, state.Version, state.ContentVersion, c.CreatedAt);
        }).ToArray());
    }

    private static async Task<IResult> Load(Guid id, ImperatorDb db, IConfiguration config, CancellationToken ct)
    {
        if (await Owned(db, config, id, ct) is null) return Results.NotFound();
        return Results.Ok(Project(await Events(db, id, ct)));
    }

    private static async Task<IResult> History(Guid id, ImperatorDb db, IConfiguration config, CancellationToken ct)
    {
        if (await Owned(db, config, id, ct) is null) return Results.NotFound();
        var events = await Events(db, id, ct);
        return Results.Ok(events.Select(e => new CampaignEventResponse(e.Sequence, e.Type, e.Payload, e.CreatedAt)));
    }

    private static async Task<IResult> Rename(Guid id, RenameCampaignRequest request,
        ImperatorDb db, IConfiguration config, CancellationToken ct)
    {
        try { SoloCampaignLifecycle.ValidName(request.Name); }
        catch (ArgumentException e) { return Results.BadRequest(new { error = e.Message }); }
        var campaign = await Owned(db, config, id, ct);
        if (campaign is null) return Results.NotFound();
        var state = Project(await Events(db, id, ct));
        SoloCampaignEvent change;
        try { change = SoloCampaignLifecycle.DecideRename(state, request.Name!); }
        catch (ArgumentException e) { return Results.BadRequest(new { error = e.Message }); }

        // The campaign row is an index/read hint; event replay is the source of truth.
        campaign.Name = SoloCampaignLifecycle.ValidName(request.Name);
        db.CampaignEvents.Add(new CampaignEvent
        {
            Id = Guid.NewGuid(), CampaignId = id, Sequence = change.Sequence,
            Type = change.Type, Payload = change.Payload, CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = LocalPlayerId(config)
        });
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // The existing (campaign_id, sequence) unique index rejects two writers of the same event.
            return Results.Conflict(new { error = "Campaign changed; reload and retry." });
        }
        return Results.Ok(SoloCampaignLifecycle.Apply(state, change));
    }
}
