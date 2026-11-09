using System.Text.Json;
using System.Text.Json.Serialization;
using Imperator.Core.Missions;

namespace Imperator.Core.Campaigns;

// Campaign definitions (decision 7.16, docs/design/campaign-format.md): one file per campaign, compiled by
// tools/build_content.py into content/dist/campaigns.json with each bespoke mission's file embedded.
// The definition only chooses among rules in content/ and supplies story text; nothing here is a rule.

/// <summary>A campaign definition: bespoke (authored missions and branches) or procedural (generated arcs).</summary>
public sealed record CampaignDefinition(
    string Id, string Kind, string Format, string Source, string Name, string Faction, string? Opposition,
    string RulesVersion, string ForceSelection, string Command, string Summary,
    CampaignBase? Base = null, List<CampaignAttack>? Attacks = null, string? Path = null,
    // Bespoke
    string? Start = null, List<CampaignMissionEntry>? Missions = null, List<CampaignResult>? Results = null,
    int? RosterRows = null,
    // Procedural
    string? StartForce = null, CampaignResources? Resources = null, CampaignGenerator? Generator = null,
    string? Reports = null, List<CampaignEnd>? End = null)
{
    public bool IsBespoke => Source == "bespoke";

    public CampaignMissionEntry Mission(string label) =>
        (Missions ?? []).FirstOrDefault(m => m.Label == label)
        ?? throw new KeyNotFoundException($"Campaign {Id} has no mission '{label}'");
}

/// <summary>The campaign's base (chapter 14 <i>The base</i>): kind, name and facility levels.</summary>
public sealed record CampaignBase(string Kind, string Name, Dictionary<string, int>? Facilities = null);

/// <summary>A base attack beyond the threat track; <see cref="When"/> is the authored trigger, kept as written.</summary>
public sealed record CampaignAttack(JsonElement When, string Text);

/// <summary>One bespoke mission: its label, its file, the printed board and force lines, and the file itself.</summary>
public sealed record CampaignMissionEntry(string Label, string File, string Board, string Force,
    CampaignMission? Mission = null);

/// <summary>
/// A bespoke mission file (a chapter 13 mission with an <c>arc</c> block), typed where the campaign
/// engine reads it. Every other field of the scenario format is kept, unread, in <see cref="Rest"/>.
/// </summary>
public sealed record CampaignMission(string Id, string Title, string Setting, int Tier,
    string? Opposition = null, MissionArc? Arc = null, MissionForceLimit? ForceLimit = null,
    List<CampaignNext>? Next = null)
{
    [JsonExtensionData] public Dictionary<string, JsonElement>? Rest { get; init; }
}

/// <summary>A branch out of a mission by grade: where it goes, what it carries, and its report and sheet text.</summary>
public sealed record CampaignNext(string Grade, string To, string Report, string Text, CampaignEffects? Effects = null)
{
    public bool Ends => To == CampaignDefinitions.End;
}

/// <summary>
/// Effects a branch carries into the next mission or the campaign (campaign-format.md, <i>Effects</i>).
/// Any key not listed lands in <see cref="Unknown"/>, which the checks reject.
/// </summary>
public sealed record CampaignEffects(
    string? Initiative = null, int? AlertStart = null, int? ForceTv = null, int? RemoveContacts = null,
    int? Requisition = null, int? Intel = null, int? Standing = null, int? Evidence = null, int? Threat = null,
    string? Flag = null)
{
    [JsonExtensionData] public Dictionary<string, JsonElement>? Unknown { get; init; }
}

public sealed record CampaignResult(string Grade, string Text);
public sealed record CampaignResources(int Requisition, int Intel, int Standing, int Evidence, int Threat);
public sealed record CampaignGenerator(int Arcs, string NodeTable, List<string>? Environments = null,
    List<string>? Vessels = null);
public sealed record CampaignEnd(string Result, string When, string Text);

/// <summary>Every campaign definition in campaigns.json, validated against a <see cref="ContentSet"/>.</summary>
public sealed class CampaignDefinitions
{
    public const string End = "end";
    public static readonly IReadOnlyList<string> Grades = ["Decisive", "Marginal", "Failure"];
    public static readonly IReadOnlySet<string> Effects = new HashSet<string>
    {
        "initiative", "alert_start", "force_tv", "remove_contacts",
        "requisition", "intel", "standing", "evidence", "threat", "flag",
    };

    private readonly Dictionary<string, JsonElement> _json;

    private CampaignDefinitions(List<CampaignDefinition> all, Dictionary<string, JsonElement> json)
    {
        All = all;
        _json = json;
    }

    public IReadOnlyList<CampaignDefinition> All { get; }

    public CampaignDefinition Get(string id) =>
        All.FirstOrDefault(d => d.Id == id) ?? throw new KeyNotFoundException($"Unknown campaign definition '{id}'");

    public bool TryGet(string id, out CampaignDefinition definition)
    {
        definition = All.FirstOrDefault(d => d.Id == id)!;
        return definition is not null;
    }

    /// <summary>The definition exactly as compiled (snake_case, every mission field), for serving whole.</summary>
    public JsonElement Json(string id) =>
        _json.TryGetValue(id, out var el) ? el : throw new KeyNotFoundException($"Unknown campaign definition '{id}'");

    public static CampaignDefinitions LoadFile(string path, ContentSet content) => Parse(File.ReadAllText(path), content);

    public static CampaignDefinitions LoadDefault(ContentSet content, string? startDir = null) =>
        LoadFile(FindDefaultPath(startDir), content);

    /// <summary>Loads campaigns.json text and checks it; throws <see cref="ContentException"/> on any error.</summary>
    public static CampaignDefinitions Parse(string json, ContentSet content)
    {
        using var doc = JsonDocument.Parse(json);
        var errors = new List<string>();
        var all = new List<CampaignDefinition>();
        var raw = new Dictionary<string, JsonElement>();
        if (!doc.RootElement.TryGetProperty("campaigns", out var list) || list.ValueKind != JsonValueKind.Array)
            throw new ContentException(["campaigns.json: needs a 'campaigns' list"]);

        int n = 0;
        foreach (var el in list.EnumerateArray())
        {
            n++;
            var where = el.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String
                ? $"campaign {id.GetString()}" : $"campaign #{n}";
            try
            {
                var d = el.Deserialize<CampaignDefinition>(ContentSet.Options)!;
                all.Add(d);
                if (!raw.TryAdd(d.Id, el.Clone())) errors.Add($"{where}: duplicate id");
            }
            catch (JsonException e) { errors.Add($"{where}: {e.Message}"); }
        }
        if (errors.Count == 0) errors.AddRange(Validate(all, content));
        if (errors.Count > 0) throw new ContentException(errors);
        return new CampaignDefinitions(all, raw);
    }

    /// <summary>The checks in tools/validate_content.py, section <i>Campaign definitions</i>.</summary>
    public static IReadOnlyList<string> Validate(IEnumerable<CampaignDefinition> definitions, ContentSet c)
    {
        var errors = new List<string>();
        var factions = c.Factions.Factions.Select(f => f.Id).ToHashSet();
        var oppositions = (c.Missions.Oppositions ?? []).Select(o => o.Id).ToHashSet();
        var latitudes = c.Campaign.Latitudes.Select(l => l.Id).ToHashSet();
        var startForces = c.Rosters.StartingForces.Select(f => f.Id).ToHashSet();
        var baseKinds = c.Campaign.Bases.Kinds.Select(k => k.Id).ToHashSet();
        var facilities = c.Campaign.Bases.Facilities.Select(f => f.Id).ToHashSet();

        void Need(string? value, IReadOnlySet<string> pool, string where)
        {
            if (value is null || !pool.Contains(value)) errors.Add($"{where}: unknown id '{value}'");
        }

        foreach (var d in definitions)
        {
            var w = $"campaign {d.Id}";
            if (d.Kind != "campaign") errors.Add($"{w}: kind must be campaign");
            Need(d.Faction, factions, $"{w} faction");
            if (d.Opposition is not null) Need(d.Opposition, oppositions, $"{w} opposition");
            Need(d.Command, latitudes, $"{w} command");
            if (d.ForceSelection is not ("fireteam" or "soldier"))
                errors.Add($"{w}: force_selection must be fireteam or soldier");
            if (d.Format is not ("short_arc" or "arcs" or "story") || d.Source is not ("bespoke" or "procedural"))
                errors.Add($"{w}: format short_arc/arcs/story and source bespoke/procedural");
            if (d.Base is not null)
            {
                Need(d.Base.Kind, baseKinds, $"{w} base kind");
                foreach (var (facility, level) in d.Base.Facilities ?? [])
                {
                    Need(facility, facilities, $"{w} base facility");
                    if (level is < 0 or > 3) errors.Add($"{w}: facility {facility} level 0-3");
                }
            }
            if (d.Source == "procedural")
            {
                Need(d.StartForce, startForces, $"{w} start force");
                continue;
            }

            var missions = d.Missions ?? [];
            var labels = missions.Select(m => m.Label).ToHashSet();
            if (d.Start is null || !labels.Contains(d.Start)) errors.Add($"{w}: start must be a mission label");
            foreach (var m in missions)
            {
                if (m.Mission is null)
                {
                    errors.Add($"{w}: mission file {m.File} missing");
                    continue;
                }
                var next = m.Mission.Next ?? [];
                if (!next.Select(x => x.Grade).Order(StringComparer.Ordinal).SequenceEqual(Grades.Order(StringComparer.Ordinal)))
                    errors.Add($"{w} mission {m.Label}: next must cover Decisive, Marginal and Failure");
                foreach (var x in next)
                {
                    if (x.To != End && !labels.Contains(x.To))
                        errors.Add($"{w} mission {m.Label} {x.Grade}: to must be a mission label or end");
                    foreach (var key in x.Effects?.Unknown?.Keys ?? Enumerable.Empty<string>())
                        errors.Add($"{w} mission {m.Label} {x.Grade}: unknown effect '{key}'");
                }
            }
        }
        return errors;
    }

    /// <summary>Finds content/dist/campaigns.json by walking up from a start directory.</summary>
    public static string FindDefaultPath(string? startDir = null)
    {
        var dir = new DirectoryInfo(startDir ?? AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = System.IO.Path.Combine(dir.FullName, "content", "dist", "campaigns.json");
            if (File.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }
        throw new FileNotFoundException("content/dist/campaigns.json not found; run tools/build_content.py");
    }
}
