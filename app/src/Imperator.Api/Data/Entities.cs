namespace Imperator.Api.Data;

// Schema per ADR-001 §4–5. Multiplayer-shaped from day one; authentication comes later.

public sealed class Player
{
    public Guid Id { get; set; }
    public required string DisplayName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public List<CampaignMembership> Memberships { get; } = [];
}

public sealed class Campaign
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public List<CampaignMembership> Members { get; } = [];
    public List<CampaignEvent> Events { get; } = [];
}

public enum CampaignRole { Commander, Operator }

public sealed class CampaignMembership
{
    public Guid CampaignId { get; set; }
    public Guid PlayerId { get; set; }
    public CampaignRole Role { get; set; }
    public DateTimeOffset JoinedAt { get; set; }

    public Campaign Campaign { get; set; } = null!;
    public Player Player { get; set; } = null!;
}

/// <summary>
/// One entry in a campaign's append-only event log. Current state is projected from the events in
/// <see cref="Sequence"/> order; rows are never updated or deleted.
/// </summary>
public sealed class CampaignEvent
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    /// <summary>Position in the campaign's log, starting at 1 and unique per campaign.</summary>
    public long Sequence { get; set; }
    /// <summary>Event type name, e.g. TaskingIssued or AfterActionReported.</summary>
    public required string Type { get; set; }
    /// <summary>Event body as JSON (stored as jsonb).</summary>
    public required string Payload { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }

    public Campaign Campaign { get; set; } = null!;
    public Player Creator { get; set; } = null!;
}
