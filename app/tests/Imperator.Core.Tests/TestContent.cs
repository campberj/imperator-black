using Imperator.Core;

namespace Imperator.Core.Tests;

// Content is read-only, so every test shares one loaded set.
internal static class TestContent
{
    private static readonly Lazy<ContentSet> _set = new(() => ContentSet.LoadDefault(AppContext.BaseDirectory));

    public static ContentSet Set => _set.Value;
    public static CoreContent Rules => Set.Core;

    public static Soldier Fresh(int armour = 0, int wounds = 1) =>
        Soldier.Generic("t", 1, armour, Set.Ranged("rifle"), Set.Melee("knife"), wounds: wounds);
}
