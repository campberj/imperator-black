namespace Imperator.Core;

/// <summary>A dice expression such as "2d8".</summary>
public readonly record struct DiceSpec(int Count, int Sides)
{
    public static DiceSpec Parse(string s)
    {
        var parts = s.Trim().ToLowerInvariant().Split('d');
        if (parts.Length != 2 || !int.TryParse(parts[0], out var n) || !int.TryParse(parts[1], out var d) || n < 1 || d < 2)
            throw new FormatException($"Bad dice expression '{s}'");
        return new DiceSpec(n, d);
    }

    public int Min => Count;
    public int Max => Count * Sides;
    public double Mean => Count * (Sides + 1) / 2.0;

    /// <summary>Exact probability of each total.</summary>
    public IReadOnlyDictionary<int, double> Pmf()
    {
        var dist = new Dictionary<int, double> { [0] = 1.0 };
        for (int i = 0; i < Count; i++)
        {
            var next = new Dictionary<int, double>();
            foreach (var (total, p) in dist)
                for (int face = 1; face <= Sides; face++)
                    next[total + face] = next.GetValueOrDefault(total + face) + p / Sides;
            dist = next;
        }
        return dist;
    }

    public override string ToString() => $"{Count}d{Sides}";
}

/// <summary>Seeded random source. Every simulated or generated result goes through one of these.</summary>
public sealed class Rng
{
    private readonly Random _random;
    public int Seed { get; }

    public Rng(int seed)
    {
        Seed = seed;
        _random = new Random(seed);
    }

    public int D(int sides) => _random.Next(1, sides + 1);

    public int Roll(DiceSpec spec)
    {
        int total = 0;
        for (int i = 0; i < spec.Count; i++) total += D(spec.Sides);
        return total;
    }

    /// <summary>Rolls 2d6 and returns both dice, for natural-2 and double-6 checks.</summary>
    public (int A, int B) TwoD6() => (D(6), D(6));

    public double NextDouble() => _random.NextDouble();

    public T Pick<T>(IReadOnlyList<T> items) => items[_random.Next(items.Count)];

    public void Shuffle<T>(IList<T> items)
    {
        for (int i = items.Count - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
    }
}
