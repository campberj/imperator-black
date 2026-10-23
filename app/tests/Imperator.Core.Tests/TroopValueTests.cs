using System.Text.Json;
using Imperator.Core;
using Imperator.Core.Values;

namespace Imperator.Core.Tests;

public sealed class TroopValueTests
{
    private static readonly ContentSet Content = ContentSet.LoadDefault();
    private static readonly TroopValueCalculator Values = new(Content);

    private static JsonElement Reference()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "troop-values-reference.json");
        if (!File.Exists(path))
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(path))
            {
                path = Path.Combine(dir.FullName, "app", "tests", "Imperator.Core.Tests", "Fixtures", "troop-values-reference.json");
                dir = dir.Parent;
            }
        }
        return JsonDocument.Parse(File.ReadAllText(path)).RootElement.Clone();
    }

    [Fact]
    public void EveryTroopMatchesPythonReference()
    {
        var all = Values.AllTroops().ToDictionary(t => t.Troop);
        var entries = Reference().GetProperty("troops");
        Assert.Equal(entries.EnumerateObject().Count(), all.Count);
        foreach (var p in entries.EnumerateObject())
        {
            Assert.Equal(p.Value.GetProperty("tv").GetInt32(), all[p.Name].Tv);
            Assert.Equal(p.Value.GetProperty("requisition").GetInt32(), all[p.Name].Requisition);
        }
    }

    [Fact]
    public void EveryWeaponBaseMatchesPythonReference()
    {
        foreach (var p in Reference().GetProperty("weapon_base").EnumerateObject())
            Assert.Equal(p.Value.GetDouble(), Values.WeaponBase(p.Name), 1);
        foreach (var p in Reference().GetProperty("melee_base").EnumerateObject())
            Assert.Equal(p.Value.GetDouble(), Values.MeleeBase(p.Name), 1);
    }

    [Fact]
    public void EverySurvivalFactorMatchesPythonReference()
    {
        foreach (var p in Reference().GetProperty("survival").EnumerateObject())
        {
            var key = p.Name.Split(',');
            Assert.Equal(p.Value.GetDouble(), Values.Survival(int.Parse(key[0]), int.Parse(key[1])), 2);
        }
        Assert.Equal(3, Content.Values.AttributeCosts["nerve"]);
    }
}
