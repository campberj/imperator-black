using System.Text.Json;

namespace Imperator.Core.Campaigns;

/// <summary>
/// The minimum persistent solo-campaign state. It deliberately contains no unapproved
/// Chapter 14 gameplay mechanics; S2 adds approved gameplay events and projections.
/// </summary>
public sealed record SoloCampaignState(Guid Id, string Name, string ContentVersion, long Version);

/// <summary>
/// Database-independent event envelope. Sequence starts at one and is gap-free per campaign.
/// </summary>
public sealed record SoloCampaignEvent(long Sequence, string Type, string Payload);

public static class SoloCampaignLifecycle
{
    public const string Created = "CampaignCreated";
    public const string Renamed = "CampaignRenamed";

    private sealed record CreatedData(Guid Id, string Name, string ContentVersion);
    private sealed record RenamedData(string Name);

    /// <summary>Validate a user-provided campaign label, not a game rule.</summary>
    public static string ValidName(string? proposed)
    {
        var name = proposed?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
            throw new ArgumentException("Campaign name must contain 1–200 characters.", nameof(proposed));
        return name;
    }

    public static SoloCampaignEvent DecideCreate(Guid id, string name, string contentVersion)
    {
        if (id == Guid.Empty) throw new ArgumentException("Campaign ID is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(contentVersion))
            throw new ArgumentException("Content version is required.", nameof(contentVersion));
        return new(1, Created, JsonSerializer.Serialize(new CreatedData(id, ValidName(name), contentVersion)));
    }

    public static SoloCampaignEvent DecideRename(SoloCampaignState state, string name)
    {
        var validated = ValidName(name);
        if (validated == state.Name)
            throw new ArgumentException("The campaign already has that name.", nameof(name));
        return new(state.Version + 1, Renamed, JsonSerializer.Serialize(new RenamedData(validated)));
    }

    public static SoloCampaignState Apply(SoloCampaignState? state, SoloCampaignEvent e)
    {
        if (e.Sequence != (state?.Version ?? 0) + 1)
            throw new InvalidOperationException($"Invalid campaign event sequence {e.Sequence}.");

        try
        {
            return e.Type switch
            {
                Created when state is null => Create(JsonSerializer.Deserialize<CreatedData>(e.Payload)
                    ?? throw new JsonException("Missing creation payload.")),
                Renamed when state is not null => state with
                {
                    Name = ValidName(JsonSerializer.Deserialize<RenamedData>(e.Payload)?.Name)
                },
                _ => throw new InvalidOperationException($"Unsupported lifecycle event '{e.Type}'.")
            };
        }
        catch (JsonException error)
        {
            throw new InvalidOperationException($"Invalid payload for campaign event '{e.Type}'.", error);
        }
    }

    private static SoloCampaignState Create(CreatedData data) =>
        new(data.Id == Guid.Empty ? throw new InvalidOperationException("Missing campaign ID.") : data.Id,
            ValidName(data.Name),
            string.IsNullOrWhiteSpace(data.ContentVersion)
                ? throw new InvalidOperationException("Missing content version.") : data.ContentVersion, 1);

    public static SoloCampaignState Replay(IEnumerable<SoloCampaignEvent> events)
    {
        SoloCampaignState? state = null;
        foreach (var e in events)
            state = Apply(state, e);
        return state ?? throw new InvalidOperationException("Campaign requires a creation event.");
    }
}
