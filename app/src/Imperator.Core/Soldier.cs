namespace Imperator.Core;

public enum Status { Active, Down, Bleeding, Critical, Dead, Captured, Fled }

public enum Cover { Open = 0, Light = 1, Heavy = 2 }

/// <summary>A model's state during a fight. Mirrors the tokens on the table.</summary>
public sealed class Soldier
{
    public required string Name { get; init; }

    // Attributes (rules v0.4). "Never used" attributes (Automated models) load as 0.
    public int Move { get; init; } = 6;
    public int Shoot { get; init; }
    public int Fight { get; init; }
    public int Physique { get; init; }
    public int Nerve { get; init; }
    public int Tech { get; init; }
    public int Wounds { get; init; } = 1;
    public int Size { get; init; } = 2;
    public string Rank { get; init; } = "regular";

    public required int Armour { get; init; }
    public required RangedWeapon Weapon { get; init; }
    /// <summary>Null for models with no melee weapon (Automated).</summary>
    public MeleeWeapon? Melee { get; init; }
    public IReadOnlyDictionary<string, int> Skills { get; init; } = new Dictionary<string, int>();
    public IReadOnlyCollection<string> WeaponKeywords { get; init; } = Array.Empty<string>();

    public int SkillLevel(string id) => Skills.TryGetValue(id, out var lvl) ? lvl : 0;
    public bool IsLeader => SkillLevel("leader") > 0;
    public bool IsMedic => SkillLevel("medic") > 0;
    public bool VoidTrained => SkillLevel("void_trained") > 0;
    public bool Automated => SkillLevel("automated") > 0;
    public bool Augmented => SkillLevel("augmented") > 0;
    /// <summary>Hit bonus from the Aim action (Marksman 1+ makes it +2).</summary>
    public int AimBonus => SkillLevel("marksman") >= 1 ? 2 : 1;

    public int Suppression { get; private set; }
    public int WoundedMarkers { get; private set; }
    public bool Wounded => WoundedMarkers > 0;
    public Status Status { get; private set; } = Status.Active;
    public Cover Cover { get; set; } = Cover.Open;
    public int OverwatchReactions { get; set; }
    public bool Activated { get; set; }
    public bool CannotReact { get; set; }
    public bool HitThisRound { get; set; }
    /// <summary>Rounds left before a Bleeding or Critical model dies.</summary>
    public int DeathClock { get; private set; }
    public bool Stabilised { get; private set; }
    /// <summary>Augmented: the first Suppression it gains each round is ignored.</summary>
    private bool _augmentSpent;

    public bool IsActive => Status == Status.Active;
    /// <summary>Suppression at which the model is Pinned: 3 by default; set from content by <see cref="UsePinningRule"/>.</summary>
    public int PinnedAt { get; set; } = 3;
    /// <summary>Pinned at PinnedAt Suppression, or 4 with Jedi discipline if that is higher.</summary>
    public bool Pinned => Suppression >= (SkillLevel("jedi_discipline") > 0 ? Math.Max(4, PinnedAt) : PinnedAt);

    /// <summary>Sets <see cref="PinnedAt"/> from the content's suppression rule (flat, or Nerve plus an offset).</summary>
    public void UsePinningRule(CoreContent core)
    {
        var s = core.Suppression;
        if (s is null) return;
        PinnedAt = s.PinnedByNerve is { } off ? Math.Max(Math.Max(1, s.PinnedAtLeast), Nerve + off) : s.PinnedAt;
    }
    public bool CanBeTreated => Status is Status.Down or Status.Bleeding or Status.Critical && !Stabilised;

    /// <summary>Modifier applied to all of this model's rolls.</summary>
    public int RollPenalty => -Suppression - (Wounded ? 1 : 0);

    public bool CanReact =>
        IsActive && !CannotReact && (OverwatchReactions > 0 || !Activated);

    /// <summary>Builds a soldier from a troop profile in content.</summary>
    public static Soldier FromTroop(ContentSet content, string troopId, string? weaponId = null, string? name = null)
    {
        var t = content.Troop(troopId);
        var skills = new Dictionary<string, int>(t.Skills);
        var weaponKeywords = new List<string>();
        string? melee = t.Melee;
        foreach (var g in t.Gear.Select(content.GearById))
        {
            foreach (var (s, lvl) in g.GrantsSkills ?? new()) skills[s] = Math.Max(lvl, skills.GetValueOrDefault(s));
            weaponKeywords.AddRange(g.WeaponKeywords ?? new());
        }
        var soldier = new Soldier
        {
            Name = name ?? t.Name,
            Rank = t.Rank,
            Move = t.Stats.Move,
            Shoot = t.Stats.Shoot,
            Fight = t.Stats.Fight ?? 0,
            Physique = t.Stats.Physique ?? 0,
            Nerve = t.Stats.Nerve ?? 0,
            Tech = t.Stats.Tech ?? 0,
            Wounds = t.Stats.Wounds,
            Size = t.Stats.Size,
            Armour = content.ArmourById(t.Armour).Value,
            Weapon = (weaponId ?? t.Weapon) is { } wid ? content.Ranged(wid) : RangedWeapon.None,
            Melee = melee is null ? null : content.Melee(melee),
            Skills = skills,
            WeaponKeywords = weaponKeywords,
        };
        soldier.UsePinningRule(content.Core);
        return soldier;
    }

    /// <summary>
    /// An abstract soldier with every rolled attribute set to one rating (simulation and tests).
    /// </summary>
    public static Soldier Generic(string name, int rating, int armour, RangedWeapon weapon, MeleeWeapon? melee,
        bool leader = false, bool medic = false, bool voidTrained = false, int wounds = 1)
    {
        var skills = new Dictionary<string, int>();
        if (leader) skills["leader"] = 1;
        if (medic) skills["medic"] = 1;
        if (voidTrained) skills["void_trained"] = 1;
        return new Soldier
        {
            Name = name, Shoot = rating, Fight = rating, Physique = rating, Nerve = rating, Tech = rating,
            Wounds = wounds, Armour = armour, Weapon = weapon, Melee = melee, Skills = skills,
        };
    }

    /// <summary>
    /// The cover this model has against a shooter it faces, given the terrain's cover: a Shield counts as Light cover
    /// when it has no other.
    /// </summary>
    public Cover CoverFacing(Cover terrain) => terrain == Cover.Open && SkillLevel("shield") > 0 ? Cover.Light : terrain;

    /// <summary>Start of a round: clears Activated, hit-this-round and cannot-react, and readies Augmented.</summary>
    public void StartRound()
    {
        Activated = false;
        HitThisRound = false;
        CannotReact = false;
        _augmentSpent = false;
    }

    public void AddSuppression(int n)
    {
        if (!IsActive || Automated || n <= 0) return;
        if (Augmented && !_augmentSpent)
        {
            _augmentSpent = true;
            if (--n == 0) return;
        }
        Suppression = Math.Min(5, Suppression + n);
        if (Suppression >= 2) OverwatchReactions = 0;
    }

    public void RemoveSuppression(int n) => Suppression = Math.Max(0, Suppression - n);

    /// <summary>Applies a final wound row (after armour). Returns true if the model was taken out of the fight.</summary>
    public bool ApplyRow(int row)
    {
        if (!IsActive || row <= 0) return false;
        HitThisRound = true;
        if (Automated && row >= 6) { Incapacitate(Status.Dead); return true; } // destroyed
        switch (row)
        {
            case 1: AddSuppression(1); return false;
            case 2: AddSuppression(2); return false;
            case 3: AddSuppression(1); return false;
            case 4:
            case 5:
                WoundedMarkers++;
                if (WoundedMarkers > Wounds) { Incapacitate(Status.Down); return true; }
                if (row == 5) AddSuppression(1);
                return false;
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

    /// <summary>End-of-round recovery test for a plain Down model: 2d6 + Physique (+1 if Augmented).</summary>
    public bool TryRecover(Rng rng, int target = 8)
    {
        if (Status != Status.Down || Stabilised || Automated) return false;
        var (a, b) = rng.TwoD6();
        if (a + b == 2 || a + b + Physique + (Augmented ? 1 : 0) < target) return false;
        StandUp();
        return true;
    }

    /// <summary>A Medic action on this model by a medic with the given Tech.</summary>
    public void Treat(Rng rng, int medicTech, bool medicSkill)
    {
        if (!CanBeTreated || Automated) return;
        if (Status == Status.Down && medicSkill) { StandUp(); return; }
        var (a, b) = rng.TwoD6();
        int total = a + b + medicTech + (medicSkill ? 2 : 0);
        bool fail = a + b == 2;
        switch (Status)
        {
            case Status.Down when !fail && total >= 8: StandUp(); break;
            case Status.Bleeding when !fail && total >= 8: Status = Status.Down; DeathClock = 0; break;
            case Status.Critical when !fail && total >= 10: Stabilised = true; DeathClock = 0; break;
        }
    }

    /// <summary>Gets up with 3 Suppression and as many Wounded markers as its Wounds.</summary>
    private void StandUp()
    {
        Status = Status.Active;
        WoundedMarkers = Wounds;
        Suppression = 3;
    }
}
