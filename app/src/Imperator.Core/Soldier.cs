namespace Imperator.Core;

public enum Status { Active, Down, Bleeding, Critical, Dead, Captured, Fled }

public enum Cover { Open = 0, Light = 1, Heavy = 2 }

/// <summary>A model's state during a fight. Mirrors the tokens on the table.</summary>
public sealed class Soldier
{
    public required string Name { get; init; }
    public required int Q { get; init; }
    public required int Armour { get; init; }
    public required RangedWeapon Weapon { get; init; }
    public required MeleeWeapon Melee { get; init; }
    public bool IsLeader { get; init; }
    public bool IsMedic { get; init; }
    public bool VoidTrained { get; init; }

    public int Suppression { get; private set; }
    public bool Wounded { get; private set; }
    public Status Status { get; private set; } = Status.Active;
    public Cover Cover { get; set; } = Cover.Open;
    public int OverwatchReactions { get; set; }
    public bool Activated { get; set; }
    public bool CannotReact { get; set; }
    public bool HitThisRound { get; set; }
    /// <summary>Rounds left before a Bleeding or Critical model dies.</summary>
    public int DeathClock { get; private set; }
    public bool Stabilised { get; private set; }

    public bool IsActive => Status == Status.Active;
    public bool Pinned => Suppression >= 3;
    public bool CanBeTreated => Status is Status.Down or Status.Bleeding or Status.Critical && !Stabilised;

    /// <summary>Modifier applied to all of this model's rolls.</summary>
    public int RollPenalty => -Suppression - (Wounded ? 1 : 0);

    public bool CanReact =>
        IsActive && !CannotReact && (OverwatchReactions > 0 || !Activated);

    public void AddSuppression(int n)
    {
        if (!IsActive) return;
        Suppression = Math.Min(5, Suppression + n);
        if (Suppression >= 2) OverwatchReactions = 0;
    }

    public void RemoveSuppression(int n) => Suppression = Math.Max(0, Suppression - n);

    /// <summary>Applies a final wound row (after armour). Returns true if the model was taken out of the fight.</summary>
    public bool ApplyRow(int row)
    {
        if (!IsActive || row <= 0) return false;
        HitThisRound = true;
        switch (row)
        {
            case 1: AddSuppression(1); return false;
            case 2: AddSuppression(2); return false;
            case 3: AddSuppression(1); return false;
            case 4:
                if (Wounded) { Incapacitate(Status.Down); return true; }
                Wounded = true; return false;
            case 5:
                if (Wounded) { Incapacitate(Status.Down); return true; }
                Wounded = true; AddSuppression(1); return false;
            case 6: Incapacitate(Status.Down); return true;
            case 7: Incapacitate(Status.Bleeding); DeathClock = 3; return true;
            case 8: Incapacitate(Status.Critical); DeathClock = 1; return true;
            default: Incapacitate(Status.Dead); return true;
        }
    }

    public void Incapacitate(Status status)
    {
        Status = status;
        Suppression = 0;
        OverwatchReactions = 0;
    }

    public void Flee() => Status = Status.Fled;

    /// <summary>End-of-round death clocks. Returns true if the model died.</summary>
    public bool TickDeathClock()
    {
        if (Status is not (Status.Bleeding or Status.Critical) || Stabilised) return false;
        DeathClock--;
        if (DeathClock <= 0) { Status = Status.Dead; return true; }
        return false;
    }

    /// <summary>End-of-round recovery test for a plain Down model.</summary>
    public bool TryRecover(Rng rng, int target = 8)
    {
        if (Status != Status.Down || Stabilised) return false;
        var (a, b) = rng.TwoD6();
        if (a + b == 2 || a + b + Q < target) return false;
        StandUp();
        return true;
    }

    /// <summary>A Medic action on this model by a medic of quality q.</summary>
    public void Treat(Rng rng, int medicQ, bool medicKeyword)
    {
        if (!CanBeTreated) return;
        if (Status == Status.Down && medicKeyword) { StandUp(); return; }
        var (a, b) = rng.TwoD6();
        int total = a + b + medicQ + (medicKeyword ? 2 : 0);
        bool fail = a + b == 2;
        switch (Status)
        {
            case Status.Down when !fail && total >= 8: StandUp(); break;
            case Status.Bleeding when !fail && total >= 8: Status = Status.Down; DeathClock = 0; break;
            case Status.Critical when !fail && total >= 10: Stabilised = true; DeathClock = 0; break;
        }
    }

    private void StandUp()
    {
        Status = Status.Active;
        Wounded = true;
        Suppression = 3;
    }
}
