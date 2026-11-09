using System.Text.Json.Nodes;
using Imperator.Core.Campaigns;

namespace Imperator.Core.Tests;

public sealed class CampaignDefinitionTests
{
    private static readonly string CampaignsPath = CampaignDefinitions.FindDefaultPath(AppContext.BaseDirectory);
    private static readonly Lazy<CampaignDefinitions> _defs = new(() => CampaignDefinitions.LoadFile(CampaignsPath, TestContent.Set));
    private static CampaignDefinitions Defs => _defs.Value;

    [Fact]
    public void FoundryMoonLoadsWithItsBranches()
    {
        var fm = Defs.Get("foundry-moon");
        Assert.True(fm.IsBespoke);
        Assert.Equal(("short_arc", "republic", "separatist", "orders", "fireteam"),
            (fm.Format, fm.Faction, fm.Opposition, fm.Command, fm.ForceSelection));
        Assert.Null(fm.Base);
        Assert.Equal("1", fm.Start);
        Assert.Equal(["1", "2A", "2B", "3"], fm.Missions!.Select(m => m.Label));
        Assert.Equal(CampaignDefinitions.Grades, fm.Results!.Select(r => r.Grade));

        var two = fm.Mission("2A").Mission!;
        Assert.Equal(new Missions.MissionForceLimit(300, 7), two.ForceLimit);
        Assert.Equal("foundry-moon", two.Arc!.Id);
        var decisive = two.Next!.Single(n => n.Grade == "Decisive");
        Assert.Equal("3", decisive.To);
        Assert.Equal(2, decisive.Effects!.RemoveContacts);
        Assert.Null(decisive.Effects.Unknown);
        Assert.StartsWith("SITREP.", decisive.Report);

        // The rest of the scenario format is kept for the mission view.
        Assert.True(two.Rest!.ContainsKey("objectives"));

        var one = fm.Mission("1").Mission!.Next!;
        Assert.Equal("player", one.Single(n => n.Grade == "Decisive").Effects!.Initiative);
        Assert.Equal("2B", one.Single(n => n.Grade == "Failure").To);
        Assert.Equal(-50, fm.Mission("2B").Mission!.Next!.Single(n => n.Grade == "Failure").Effects!.ForceTv);
        Assert.All(fm.Mission("3").Mission!.Next!, n => Assert.True(n.Ends));
    }

    [Fact]
    public void OuterReachPatrolLoadsAsProcedural()
    {
        var orp = Defs.Get("outer-reach-patrol");
        Assert.False(orp.IsBespoke);
        Assert.Equal(("arcs", "tfb", null, "black_ops"), (orp.Format, orp.Faction, orp.Opposition, orp.Command));
        Assert.Equal("poc_boarding_element", orp.StartForce);
        Assert.Equal(new CampaignResources(6, 1, 3, 0, 0), orp.Resources);
        Assert.Equal(("ship", "TFS Lantern"), (orp.Base!.Kind, orp.Base.Name));
        Assert.Equal(3, orp.Generator!.Arcs);
        Assert.Equal("campaign", orp.Generator.NodeTable);
        Assert.Equal("default", orp.Reports);
        Assert.Equal(CampaignDefinitions.Grades, orp.End!.Select(e => e.Result));
        Assert.Equal(2, orp.Attacks!.Single().When.GetProperty("arc").GetInt32());
        Assert.Null(orp.Missions);
    }

    [Fact]
    public void ServesEachDefinitionAsCompiled()
    {
        var json = Defs.Json("foundry-moon");
        Assert.Equal("Foundry Moon", json.GetProperty("name").GetString());
        Assert.Equal(4, json.GetProperty("missions").GetArrayLength());
        Assert.Throws<KeyNotFoundException>(() => Defs.Get("nowhere"));
        Assert.False(Defs.TryGet("nowhere", out _));
    }

    [Fact]
    public void CampaignContentLoadsLatitudesAndReports()
    {
        var cp = TestContent.Set.Campaign;
        Assert.Equal(["black_ops", "orders", "free_hand"], cp.Latitudes.Select(l => l.Id));
        Assert.Contains(cp.Reports, r => r.Id == "tasking" && r.Text.Contains("{force}"));
    }

    [Theory]
    [InlineData("foundry-moon", "faction", "\"nobody\"", "campaign foundry-moon faction: unknown id 'nobody'")]
    [InlineData("foundry-moon", "opposition", "\"nobody\"", "campaign foundry-moon opposition: unknown id 'nobody'")]
    [InlineData("foundry-moon", "command", "\"anarchy\"", "campaign foundry-moon command: unknown id 'anarchy'")]
    [InlineData("foundry-moon", "force_selection", "\"squad\"", "campaign foundry-moon: force_selection must be fireteam or soldier")]
    [InlineData("foundry-moon", "format", "\"saga\"", "campaign foundry-moon: format short_arc/arcs/story and source bespoke/procedural")]
    [InlineData("foundry-moon", "source", "\"found\"", "campaign foundry-moon: format short_arc/arcs/story and source bespoke/procedural")]
    [InlineData("foundry-moon", "kind", "\"mission\"", "campaign foundry-moon: kind must be campaign")]
    [InlineData("foundry-moon", "start", "\"9\"", "campaign foundry-moon: start must be a mission label")]
    [InlineData("outer-reach-patrol", "base.kind", "\"castle\"", "campaign outer-reach-patrol base kind: unknown id 'castle'")]
    [InlineData("outer-reach-patrol", "base.facilities", "{\"moat\": 1}", "campaign outer-reach-patrol base facility: unknown id 'moat'")]
    [InlineData("outer-reach-patrol", "base.facilities", "{\"medical\": 4}", "campaign outer-reach-patrol: facility medical level 0-3")]
    [InlineData("outer-reach-patrol", "start_force", "\"nobody\"", "campaign outer-reach-patrol start force: unknown id 'nobody'")]
    [InlineData("foundry-moon", "missions[1].mission.next[0].to", "\"4\"", "campaign foundry-moon mission 2A Decisive: to must be a mission label or end")]
    [InlineData("foundry-moon", "missions[1].mission.next[0].effects", "{\"morale\": 1}", "campaign foundry-moon mission 2A Decisive: unknown effect 'morale'")]
    [InlineData("foundry-moon", "missions[0].mission.next[2].grade", "\"Marginal\"", "campaign foundry-moon mission 1: next must cover Decisive, Marginal and Failure")]
    public void BadDefinitionFailsLoudly(string campaign, string path, string json, string expected)
    {
        var e = Assert.Throws<ContentException>(() => LoadMutated(root => Set(Campaign(root, campaign), path, json)));
        Assert.Contains(expected, e.Errors);
    }

    [Fact]
    public void MissingMissionFileFailsLoudly()
    {
        var e = Assert.Throws<ContentException>(() => LoadMutated(root =>
            Campaign(root, "foundry-moon")["missions"]![2]!.AsObject().Remove("mission")));
        Assert.Contains("campaign foundry-moon: mission file 2b-bring-them-home.yaml missing", e.Errors);
    }

    [Fact]
    public void DuplicateIdFailsLoudly()
    {
        var e = Assert.Throws<ContentException>(() => LoadMutated(root =>
            root["campaigns"]!.AsArray().Add(Campaign(root, "foundry-moon").DeepClone())));
        Assert.Contains("campaign foundry-moon: duplicate id", e.Errors);
    }

    [Fact]
    public void MissingRequiredFieldNamesTheCampaign()
    {
        var e = Assert.Throws<ContentException>(() => LoadMutated(root => Campaign(root, "outer-reach-patrol").AsObject().Remove("name")));
        Assert.Contains(e.Errors, x => x.StartsWith("campaign outer-reach-patrol: "));
    }

    private static CampaignDefinitions LoadMutated(Action<JsonNode> mutate)
    {
        var root = JsonNode.Parse(File.ReadAllText(CampaignsPath))!;
        mutate(root);
        return CampaignDefinitions.Parse(root.ToJsonString(), TestContent.Set);
    }

    private static JsonNode Campaign(JsonNode root, string id) =>
        root["campaigns"]!.AsArray().Single(c => c!["id"]!.GetValue<string>() == id)!;

    // Path syntax: dotted keys with [n] indexes; the last key is set (added if absent).
    private static void Set(JsonNode node, string path, string json)
    {
        var parts = System.Text.RegularExpressions.Regex.Matches(path, @"(\w+)|\[(\d+)\]").ToList();
        foreach (var m in parts[..^1])
            node = (m.Groups[1].Success ? node[m.Groups[1].Value] : node[int.Parse(m.Groups[2].Value)])!;
        var last = parts[^1];
        if (last.Groups[1].Success) node[last.Groups[1].Value] = JsonNode.Parse(json);
        else node[int.Parse(last.Groups[2].Value)] = JsonNode.Parse(json);
    }
}
