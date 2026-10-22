using System.Security.Cryptography;
using System.Text;
using Imperator.Core;

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

    public static ContentFile Load(string path)
    {
        var json = File.ReadAllBytes(path);
        var set = ContentSet.Parse(Encoding.UTF8.GetString(json));
        var version = set.Core.Version;
        var hash = Convert.ToHexStringLower(SHA256.HashData(json))[..12];
        return new ContentFile { Path = path, Version = version, Json = json, ETag = $"\"{version}-{hash}\"", Set = set };
    }
}
