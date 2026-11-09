using System.Text.Json;

namespace Imperator.Core.Campaigns;

/// <summary>
/// The minimum persistent solo-campaign state. It deliberately contains no unapproved
/// Chapter 14 gameplay mechanics; S2 adds approved gameplay events and projections.
/// </summary>
public sealed record SoloCampaignState(Guid Id, string Name, string ContentVersion, long Version);

/// <summary>
/// Database-independent event envelope. Sequence starts at one and is gap-free per campaign.
/// <see cref="SchemaVersion"/> is the version of this event type's payload: every type starts at 1
/// and bumps it when its payload changes, adding an upgrade step to <see cref="SoloCampaignLifecycle"/>.
/// </summary>
public sealed record SoloCampaignEvent(long Sequence, string Type, string Payload, int SchemaVersion = 1);

public static class SoloCampaignLifecycle
{
    public const string Created = "CampaignCreated";
    public const string Renamed = "CampaignRenamed";

    /// <summary>The current payload schema version of each event type.</summary>
    public static readonly IReadOnlyDictionary<string, int> SchemaVersions = new Dictionary<string, int>
    {
        [Created] = 1,
        [Renamed] = 1,
    };

    /// <summary>
    /// Upgrade steps, keyed by (type, from version): each turns a payload at that version into the next.
    /// Empty while every type is at version 1.
    /// </summary>
    private static readonly IReadOnlyDictionary<(string Type, int From), Func<string, string>> Upgrades =
        new Dictionary<(string Type, int From), Func<string, string>>();

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
        return new(1, Created, JsonSerializer.Serialize(new CreatedData(id, ValidName(name), contentVersion)),
            SchemaVersions[Created]);
    }

    public static SoloCampaignEvent DecideRename(SoloCampaignState state, string name)
    {
        var validated = ValidName(name);
        if (validated == state.Name)
            throw new ArgumentException("The campaign already has that name.", nameof(name));
        return new(state.Version + 1, Renamed, JsonSerializer.Serialize(new RenamedData(validated)),
            SchemaVersions[Renamed]);
    }

    public static SoloCampaignState Apply(SoloCampaignState? state, SoloCampaignEvent e)
    {
        if (e.Sequence != (state?.Version ?? 0) + 1)
            throw new InvalidOperationException($"Invalid campaign event sequence {e.Sequence}.");
        e = Upgrade(e);

        try
        {
            return e.Type switch
            {
                Created when state is null => Create(JsonSerializer.Deserialize<CreatedData>(e.Payload)
                    ?? throw new JsonException("Missing creation payload.")),
                Renamed when state is not null => state with
                {
                    Name = ValidName(JsonSerializer.Deserialize<RenamedData>(e.Payload)?.Name),
                    Version = e.Sequence
                },
                _ => throw new InvalidOperationException($"Unsupported lifecycle event '{e.Type}'.")
            };
        }
        catch (JsonException error)
        {
            throw new InvalidOperationException($"Invalid payload for campaign event '{e.Type}'.", error);
        }
    }

    /// <summary>
    /// Bring an event's payload up to its type's current schema version. Rejects unknown types and
    /// versions that are newer than this build knows, or older with no upgrade step.
    /// </summary>
    public static SoloCampaignEvent Upgrade(SoloCampaignEvent e)
    {
        if (!SchemaVersions.TryGetValue(e.Type, out var current))
            throw new InvalidOperationException($"Unsupported lifecycle event '{e.Type}'.");
        if (e.SchemaVersion < 1 || e.SchemaVersion > current)
            throw new InvalidOperationException(
                $"Unknown schema version {e.SchemaVersion} for campaign event '{e.Type}' (current {current}).");
        while (e.SchemaVersion < current)
        {
            if (!Upgrades.TryGetValue((e.Type, e.SchemaVersion), out var step))
                throw new InvalidOperationException(
                    $"No upgrade for campaign event '{e.Type}' from schema version {e.SchemaVersion}.");
            e = e with { Payload = step(e.Payload), SchemaVersion = e.SchemaVersion + 1 };
        }
        return e;
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
