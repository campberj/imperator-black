namespace Imperator.Core;

public enum Outcome
{
    Miss,
    NoEffect,     // hit, but stepped down to row 0 or below
    Suppression,  // rows 1-3
    Wounded,      // rows 4-5 (on a fresh target)
    Down,         // row 6
    Bleeding,     // row 7
    Critical,     // row 8
    Dead,         // rows 9-10
}

public static class Wounds
{
    /// <summary>Row on the wound table for a damage total, before armour.</summary>
    public static int RowFor(CoreContent core, int damageTotal)
    {
        foreach (var r in core.WoundTable)
            if (damageTotal >= r.Min && damageTotal <= r.Max) return r.Row;
        return damageTotal < core.WoundTable[0].Min ? 0 : core.WoundTable[^1].Row;
    }

    /// <summary>Final row after critical bonus and armour step-down. 0 or less means no effect.</summary>
    public static int FinalRow(CoreContent core, int damageTotal, bool critical, int armour, int ap, int extraStepDown = 0)
    {
        int row = RowFor(core, damageTotal) + (critical ? 1 : 0) - Math.Max(0, armour - ap) - extraStepDown;
        return Math.Min(row, 10);
    }

    public static Outcome Classify(int row) => row switch
    {
        <= 0 => Outcome.NoEffect,
        <= 3 => Outcome.Suppression,
        <= 5 => Outcome.Wounded,
        6 => Outcome.Down,
        7 => Outcome.Bleeding,
        8 => Outcome.Critical,
        _ => Outcome.Dead,
    };
}
