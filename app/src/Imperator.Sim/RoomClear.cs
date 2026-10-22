using Imperator.Core;

namespace Imperator.Sim;

public enum EntryMethod { Dynamic, FlashFirst, FragFirst, BreachAndFlash }

public enum RoomOutcome { Cleared, Repulsed, Ongoing }

/// <summary>
/// A stack of attackers clears one 6"x6" compartment held by defenders in Light cover.
/// Models reactions and Overwatch (rules v0.2 chapter 2), grenades and breaching charges (chapter 6).
/// Simplifications: no positions inside the room; every defender can see every attacker who has entered;
/// attackers outside the door are out of sight; attackers do not react.
/// </summary>
public sealed class RoomClear
{
    private readonly Rng _rng;
    private readonly CoreContent _core;
    private readonly ContentSet _content;
    private readonly List<Soldier> _att, _def;
    private readonly HashSet<Soldier> _inside = new();
    private readonly EntryMethod _method;
    private readonly bool _defendersOnOverwatch;
    private bool _preEntryDone;
    public int MaxRounds { get; init; } = 3;
    /// <summary>Chance that another model in the compartment is within the 3" beaten zone of the first target.</summary>
    public double ZoneChance { get; init; } = 0.6;
    /// <summary>Experimental: extra wound rows for shots inside the compartment (close quarters).</summary>
    public int CloseQuartersRows { get; init; }
    public int RoundCleared { get; private set; }

    public RoomClear(Rng rng, ContentSet content, List<Soldier> attackers, List<Soldier> defenders, EntryMethod method, bool defendersOnOverwatch)
    {
        _rng = rng; _content = content; _core = content.Core;
        _att = attackers; _def = defenders; _method = method; _defendersOnOverwatch = defendersOnOverwatch;
        foreach (var d in _def)
        {
            d.Cover = Cover.Light;
            d.OverwatchReactions = defendersOnOverwatch ? 2 : 0;
        }
    }

    public RoomOutcome Run()
    {
        for (int round = 1; round <= MaxRounds; round++)
        {
            foreach (var s in _att.Concat(_def)) s.StartRound();
            bool attackersTurn = true; // the attackers choose when to go in
            while (true)
            {
                var side = attackersTurn ? _att : _def;
                var next = side.FirstOrDefault(s => s.IsActive && !s.Activated);
                if (next is not null)
                {
                    if (attackersTurn) AttackerActivation(next); else DefenderActivation(next);
                }
                var o = Outcome();
                if (o != RoomOutcome.Ongoing) { RoundCleared = round; return o; }
                if (!_att.Any(s => s.IsActive && !s.Activated) && !_def.Any(s => s.IsActive && !s.Activated)) break;
                attackersTurn = !attackersTurn;
            }
            EndOfRound();
            var o2 = Outcome();
            if (o2 != RoomOutcome.Ongoing) { RoundCleared = round; return o2; }
        }
        return RoomOutcome.Ongoing;
    }

    private RoomOutcome Outcome() =>
        !_def.Any(s => s.IsActive) ? RoomOutcome.Cleared :
        !_att.Any(s => s.IsActive) ? RoomOutcome.Repulsed : RoomOutcome.Ongoing;

    private static bool Broken(List<Soldier> side) => side.Count(s => !s.IsActive) * 2 >= side.Count;

    private bool BreakTest(Soldier s, List<Soldier> side)
    {
        if (!Broken(side)) return true;
        var (a, b) = _rng.TwoD6();
        int bonus = side.Any(f => f.IsActive && f.IsLeader && f != s) ? 1 : 0;
        if (a + b == 2 || a + b + s.Nerve + s.RollPenalty + bonus < _core.TestTarget) { s.Flee(); return false; }
        return true;
    }

    // ---------- Attackers ----------

    private void AttackerActivation(Soldier s)
    {
        s.Activated = true;
        s.OverwatchReactions = 0;
        if (!BreakTest(s, _att)) return;

        if (!_inside.Contains(s))
        {
            int actions = 2;
            if (!_preEntryDone)
            {
                _preEntryDone = true;
                actions -= PreEntry(s);
                if (!s.IsActive) return;
            }
            if (_pendingFlash && actions == 2)
            {
                _pendingFlash = false;
                ThrowInto(s, "flashbang");
                actions--;
                if (!s.IsActive) return;
            }
            if (actions == 0) return;
            EnterNow(s);
            actions--;
            if (actions > 0 && s.IsActive) AttackerShoot(s, moved: true);
            return;
        }

        if (s.Pinned)
        {
            Rally(s, _att);
            if (s.Cover != Cover.Open) AttackerShoot(s, false); else s.Cover = Cover.Light; // move to cover
            return;
        }
        if (s.Cover == Cover.Open)
        {
            s.Cover = Cover.Light; // move to cover inside the room
            AttackerShoot(s, moved: true);
        }
        else
        {
            AttackerShoot(s, false);
            if (s.IsActive) AttackerShoot(s, false);
        }
    }

    /// <summary>Returns the number of actions used before entry.</summary>
    private int PreEntry(Soldier s)
    {
        switch (_method)
        {
            case EntryMethod.FlashFirst:
                ThrowInto(s, "flashbang");
                return 1;
            case EntryMethod.FragFirst:
                ThrowInto(s, "frag");
                return 1;
            case EntryMethod.BreachAndFlash:
                // Place (Interact) and detonate: the placer is beside the door and out of sight, so no reactions.
                var near = _def.Where(d => d.IsActive).OrderBy(_ => _rng.NextDouble()).Take(1).ToList();
                foreach (var d in near) { d.AddSuppression(2); d.CannotReact = true; d.HitThisRound = true; }
                // The next attacker throws the flashbang as part of its activation.
                _pendingFlash = true;
                return 2;
            default:
                return 0;
        }
    }

    private bool _pendingFlash;

    private void EnterNow(Soldier s)
    {
        _inside.Add(s);
        s.Cover = Cover.Open;
        DefenderReactions(s); // Move trigger: interrupts during the move through the door
    }

    private void ThrowInto(Soldier thrower, string grenade)
    {
        var g = _content.Thrown(grenade);
        var (onTarget, slipped) = Grenades.Throw(_rng, _core, thrower);
        if (slipped)
        {
            var stack = _att.Where(a => a.IsActive && !_inside.Contains(a)).ToList();
            if (grenade == "frag") Grenades.Frag(_rng, _core, g, stack, confined: false);
            else Grenades.Flashbang(stack);
            return;
        }
        if (!onTarget) return; // lands short in the doorway; assume no effect inside
        var targets = _def.Where(d => d.IsActive).ToList();
        if (grenade == "frag")
        {
            Grenades.Frag(_rng, _core, g, targets, confined: true);
            foreach (var d in _def.Where(d => !d.IsActive)) CasualtyShock(d, _def);
        }
        else Grenades.Flashbang(targets);
    }

    private void AttackerShoot(Soldier s, bool moved)
    {
        var order = _def.Where(d => d.IsActive).OrderBy(d => d.Cover).ThenBy(_ => _rng.NextDouble()).ToList();
        if (order.Count == 0) return;
        int sit = moved && !s.Weapon.Has("handy") ? -1 : 0;
        var targets = Zone(order[0], order.Skip(1));
        Shooting.FireBurst(_rng, _core, s, targets, sit, CloseQuartersRows);
        foreach (var t in targets.Where(t => !t.IsActive)) CasualtyShock(t, _def);
        if (s.IsActive) DefenderReactions(s); // Shoot trigger
    }

    // ---------- Defenders ----------

    private void DefenderReactions(Soldier trigger)
    {
        foreach (var d in _def.OrderBy(_ => _rng.NextDouble()).ToList())
        {
            if (!trigger.IsActive) return;
            if (!d.IsActive || d.CannotReact || d.Pinned) continue;
            if (d.OverwatchReactions > 0)
            {
                d.OverwatchReactions--;
            }
            else if (!_defendersOnOverwatch && !d.Activated)
            {
                d.Activated = true; // unactivated reaction: gives up its activation
            }
            else continue;
            var others = _att.Where(a => a.IsActive && a != trigger && _inside.Contains(a)).OrderBy(a => a.Cover);
            var targets = Zone(trigger, others);
            Shooting.FireBurst(_rng, _core, d, targets, -1, CloseQuartersRows);
            foreach (var t in targets.Where(t => !t.IsActive)) CasualtyShock(t, _att.Where(_inside.Contains).ToList());
        }
    }

    private void DefenderActivation(Soldier d)
    {
        d.Activated = true;
        d.OverwatchReactions = 0;
        if (!BreakTest(d, _def)) return;
        var visible = _att.Where(a => a.IsActive && _inside.Contains(a)).ToList();
        if (d.Pinned)
        {
            Rally(d, _def);
            if (visible.Count > 0) DefenderShoot(d, visible);
            return;
        }
        if (visible.Count == 0)
        {
            if (d.Suppression > 0) Rally(d, _def);
            d.OverwatchReactions = 2;
            return;
        }
        DefenderShoot(d, visible);
        visible = visible.Where(a => a.IsActive).ToList();
        if (visible.Count > 0) DefenderShoot(d, visible);
    }

    private void DefenderShoot(Soldier d, List<Soldier> visible)
    {
        var order = visible.Where(a => a.IsActive).OrderBy(a => a.Cover).ThenBy(_ => _rng.NextDouble()).ToList();
        if (order.Count == 0) return;
        var targets = Zone(order[0], order.Skip(1));
        Shooting.FireBurst(_rng, _core, d, targets, 0, CloseQuartersRows);
        foreach (var t in targets.Where(t => !t.IsActive)) CasualtyShock(t, _att.Where(_inside.Contains).ToList());
    }

    /// <summary>The first target, then each other candidate that happens to be inside its beaten zone.</summary>
    private List<Soldier> Zone(Soldier first, IEnumerable<Soldier> candidates)
    {
        var list = new List<Soldier> { first };
        list.AddRange(candidates.Where(_ => _rng.NextDouble() < ZoneChance));
        return list;
    }

    // ---------- Shared ----------

    private static void Rally(Soldier s, List<Soldier> side) =>
        s.RemoveSuppression(s.IsLeader || side.Any(f => f.IsActive && f.IsLeader) ? 2 : 1);

    private static void CasualtyShock(Soldier casualty, List<Soldier> nearby)
    {
        foreach (var f in nearby.Where(f => f.IsActive && f != casualty))
            f.AddSuppression(casualty.IsLeader ? 2 : 1); // everyone in a 6" room is within 6"
    }

    private void EndOfRound()
    {
        foreach (var s in _att.Concat(_def))
        {
            s.TickDeathClock();
            s.TryRecover(_rng, _core.TestTarget);
            if (s.IsActive && !s.HitThisRound && s.Cover != Cover.Open) s.RemoveSuppression(1);
        }
    }
}
