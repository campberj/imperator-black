using Imperator.Core;

namespace Imperator.Sim;

public enum FightWinner { A, B, Draw }

/// <summary>
/// Abstract open-ground firefight, in the spirit of the A War of Worlds prototype simulator.
/// No board: cover is a state, range is sampled per shot, and both sides use the same simple policy.
/// Reactions are not modelled here (see RoomClear for those).
/// </summary>
public sealed class Firefight
{
    private readonly Rng _rng;
    private readonly CoreContent _core;
    private readonly List<Soldier> _a, _b;
    public int Rounds { get; init; } = 8;
    public double MinRange { get; init; } = 6;
    public double MaxRange { get; init; } = 24;
    public double AdvanceChance { get; init; } = 0.2;
    /// <summary>Chance that another enemy model is within the 3" beaten zone of the first target.</summary>
    public double ZoneChance { get; init; } = 0.35;

    public Firefight(Rng rng, CoreContent core, List<Soldier> a, List<Soldier> b)
    {
        _rng = rng; _core = core; _a = a; _b = b;
        foreach (var s in a.Concat(b)) s.UsePinningRule(core);
    }

    /// <summary>Activations taken, and how many of them began Pinned (for the pinning comparison).</summary>
    public int Activations { get; private set; }
    public int PinnedActivations { get; private set; }
    private readonly Dictionary<Soldier, (int Pinned, int Acts)> _bySoldier = new();
    /// <summary>Per model: activations begun Pinned, and activations taken.</summary>
    public IEnumerable<(Soldier Soldier, int Pinned, int Acts)> PinnedBySoldier => _bySoldier.Select(kv => (kv.Key, kv.Value.Pinned, kv.Value.Acts));

    public static void ScatterCover(Rng rng, IEnumerable<Soldier> side, double open = 0.2, double heavy = 0.3)
    {
        foreach (var s in side)
        {
            double r = rng.NextDouble();
            s.Cover = r < open ? Cover.Open : r < open + heavy ? Cover.Heavy : Cover.Light;
        }
    }

    public FightWinner Run()
    {
        for (int round = 1; round <= Rounds; round++)
        {
            foreach (var s in _a.Concat(_b)) s.StartRound();
            bool aTurn = Initiative(_a) >= Initiative(_b);
            while (true)
            {
                var side = aTurn ? _a : _b;
                var foe = aTurn ? _b : _a;
                var next = PickActivator(side);
                if (next is not null) Activate(next, side, foe);
                if (Done(out var w)) return w;
                if (PickActivator(_a) is null && PickActivator(_b) is null) break;
                aTurn = !aTurn;
            }
            EndOfRound(_a.Concat(_b));
            if (Done(out var w2)) return w2;
        }
        int ca = _a.Count(s => s.IsActive), cb = _b.Count(s => s.IsActive);
        return ca > cb ? FightWinner.A : cb > ca ? FightWinner.B : FightWinner.Draw;
    }

    private int Initiative(List<Soldier> side)
    {
        var (x, y) = _rng.TwoD6();
        return x + y + side.Where(s => s.IsActive && s.IsLeader).Select(s => s.Nerve).DefaultIfEmpty(0).Max();
    }

    private bool Done(out FightWinner w)
    {
        bool aOut = !_a.Any(s => s.IsActive), bOut = !_b.Any(s => s.IsActive);
        w = aOut && bOut ? FightWinner.Draw : aOut ? FightWinner.B : FightWinner.A;
        return aOut || bOut;
    }

    // Prototype activation priority: pinned first, then models in the open, then the rest.
    private static Soldier? PickActivator(List<Soldier> side) =>
        side.Where(s => s.IsActive && !s.Activated)
            .OrderByDescending(s => s.Pinned).ThenBy(s => s.Cover).FirstOrDefault();

    private Soldier? Target(List<Soldier> foe)
    {
        var live = foe.Where(s => s.IsActive).ToList();
        if (live.Count == 0) return null;
        var best = live.Min(s => s.Cover);
        return _rng.Pick(live.Where(s => s.Cover == best).ToList());
    }

    private int RangeMod(Soldier shooter) =>
        shooter.Weapon.RangeModifier(MinRange + _rng.NextDouble() * (MaxRange - MinRange));

    private void Shoot(Soldier s, List<Soldier> side, List<Soldier> foe, int situational)
    {
        var t = Target(foe);
        if (t is null) return;
        var targets = Zone(t, foe);
        Shooting.FireBurst(_rng, _core, s, targets, situational + RangeMod(s));
        foreach (var x in targets.Where(x => !x.IsActive)) CasualtyShock(x, foe);
    }

    private List<Soldier> Zone(Soldier first, List<Soldier> foe)
    {
        var list = new List<Soldier> { first };
        list.AddRange(foe.Where(f => f.IsActive && f != first && _rng.NextDouble() < ZoneChance).OrderBy(f => f.Cover));
        return list;
    }

    private void CasualtyShock(Soldier casualty, List<Soldier> side)
    {
        foreach (var f in side.Where(f => f.IsActive && f != casualty))
            if (_rng.NextDouble() < 0.5) f.AddSuppression(casualty.IsLeader ? 2 : 1); // ~half are within 6"
    }

    private bool Broken(List<Soldier> side) =>
        side.Count(s => !s.IsActive) * 2 >= side.Count;

    private void Activate(Soldier s, List<Soldier> side, List<Soldier> foe)
    {
        s.Activated = true;
        Activations++;
        if (s.Pinned) PinnedActivations++;
        var (p, n) = _bySoldier.GetValueOrDefault(s);
        _bySoldier[s] = (p + (s.Pinned ? 1 : 0), n + 1);
        if (Broken(side))
        {
            var (x, y) = _rng.TwoD6();
            int bonus = side.Any(f => f.IsActive && f.IsLeader && f != s) ? 1 : 0;
            if (x + y == 2 || x + y + s.Nerve + s.RollPenalty + bonus < _core.TestTarget) { s.Flee(); return; }
        }

        bool leaderNear = side.Any(f => f.IsActive && f.IsLeader);
        if (s.Pinned)
        {
            s.RemoveSuppression(s.IsLeader || leaderNear ? 2 : 1);
            if (s.Pinned && s.Cover == Cover.Open) MoveToCover(s);
            else if (s.Pinned) s.RemoveSuppression(s.IsLeader || leaderNear ? 2 : 1);
            else if (s.Cover != Cover.Open) Shoot(s, side, foe, 0);
            return;
        }

        var patient = s.IsMedic ? side.FirstOrDefault(f => f.CanBeTreated && f.Status != Status.Down) ?? side.FirstOrDefault(f => f.CanBeTreated) : null;
        if (patient is not null)
        {
            s.Cover = RandomCover(0.4);
            patient.Treat(_rng, s.Tech, s.IsMedic);
            return;
        }

        if (s.IsLeader)
        {
            var shaken = side.Where(f => f.IsActive && f != s && f.Suppression >= 2).OrderByDescending(f => f.Suppression).FirstOrDefault();
            if (shaken is not null)
            {
                shaken.RemoveSuppression(2);
                Shoot(s, side, foe, 0);
                return;
            }
        }

        int moved = s.Weapon.Has("handy") ? 0 : -1;
        if (s.Weapon.Has("cumbersome")) moved -= 1;
        if (s.Cover == Cover.Open)
        {
            MoveToCover(s);
            Shoot(s, side, foe, moved);
        }
        else if (_rng.NextDouble() < AdvanceChance)
        {
            s.Cover = RandomCover(0.4);
            Shoot(s, side, foe, moved);
        }
        else if (s.Weapon.Has("auto") && foe.Count(f => f.IsActive) >= 2)
        {
            var t = Target(foe);
            if (t is not null) Shooting.SuppressBurst(_rng, _core, s, Zone(t, foe), RangeMod(s));
            Shoot(s, side, foe, 0);
        }
        else
        {
            Shoot(s, side, foe, s.AimBonus); // Aim, then Shoot
        }
    }

    private void MoveToCover(Soldier s) => s.Cover = _rng.NextDouble() < 0.8 ? (_rng.NextDouble() < 0.6 ? Cover.Light : Cover.Heavy) : Cover.Open;

    private Cover RandomCover(double open)
    {
        double r = _rng.NextDouble();
        return r < open ? Cover.Open : r < open + (1 - open) * 0.67 ? Cover.Light : Cover.Heavy;
    }

    private void EndOfRound(IEnumerable<Soldier> all)
    {
        foreach (var s in all)
        {
            s.TickDeathClock();
            s.TryRecover(_rng, _core.TestTarget);
            if (s.IsActive && !s.HitThisRound && s.Cover != Cover.Open) s.RemoveSuppression(1);
        }
    }
}
