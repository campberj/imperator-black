using System.Security.Cryptography;
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

    public static ContentFile Load(string path)
    {
        var json = File.ReadAllBytes(path);
        var version = ContentSet.LoadFile(path).Core.Version;
        var hash = Convert.ToHexStringLower(SHA256.HashData(json))[..12];
        return new ContentFile { Path = path, Version = version, Json = json, ETag = $"\"{version}-{hash}\"" };
    }
}
