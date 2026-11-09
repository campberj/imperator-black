using System.Security.Cryptography;
using System.Text;
using Imperator.Core;
using Imperator.Core.Campaigns;

namespace Imperator.Api;

/// <summary>The compiled content.json served to clients, validated with <see cref="ContentSet"/> at startup.</summary>
public sealed class ContentFile
{
    public required string Path { get; init; }
    public required string Version { get; init; }
    public required byte[] Json { get; init; }
    /// <summary>Strong ETag from the content version and a hash of the bytes.</summary>
    public required string ETag { get; init; }
    /// <summary>The same bytes, loaded and validated, for endpoints that run game logic (decks).</summary>
    public required ContentSet Set { get; init; }
    /// <summary>The campaign definitions (campaigns.json), checked against <see cref="Set"/>.</summary>
    public required CampaignDefinitions Campaigns { get; init; }

    /// <param name="path">content.json.</param>
    /// <param name="campaignsPath">campaigns.json; by default the file beside content.json.</param>
    public static ContentFile Load(string path, string? campaignsPath = null)
    {
        var json = File.ReadAllBytes(path);
        var set = ContentSet.Parse(Encoding.UTF8.GetString(json));
        var version = set.Core.Version;
        var hash = Convert.ToHexStringLower(SHA256.HashData(json))[..12];
        campaignsPath ??= System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path)!, "campaigns.json");
        var campaigns = CampaignDefinitions.LoadFile(campaignsPath, set);
        return new ContentFile
        {
            Path = path, Version = version, Json = json, ETag = $"\"{version}-{hash}\"", Set = set, Campaigns = campaigns,
        };
    }
}
