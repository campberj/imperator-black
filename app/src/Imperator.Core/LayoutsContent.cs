namespace Imperator.Core;

// Ship and station deck layouts (rules chapter 11), from content/layouts.yaml. Tables only for now;
// tools/generate_layout.py is the reference generator until it is ported here.

/// <summary>A floor tile. Cells are its footprint on the 3" grid; Owned is how many the tile inventory has.</summary>
public sealed record TileDef(string Id, string Name, string Size, List<int> Cells, int Owned, string? Text = null);

/// <summary>A row of a shared d6 layout table (spine turn, branches, loop door, door state, hazard roll).</summary>
public sealed record LayoutRollRow(D6Range Roll, string Result, string Text);

public sealed record VesselRollRow(D6Range Roll, string Vessel);

public sealed record RoomRollRow(D6Range Roll, string Room);

/// <summary>A cover piece to place in a room, W × H in inches. Type is light or heavy.</summary>
public sealed record CoverPieceDef(string Type, string Label, double W, double H, string Piece);

/// <summary>A room type. Size is compartment (6"x6") or small (3"x3"); Security overrides the vessel's level.</summary>
public sealed record RoomDef(string Id, string Name, string Size, List<CoverPieceDef> Cover,
    bool Unique = false, string? Security = null, string? Text = null);

/// <summary>
/// A vessel type. Board is in inches; Spine counts junction-and-passage pairs after the docking passage;
/// Compartments is [min, max], counting the Heart, the room at the far end of the spine.
/// </summary>
public sealed record VesselDef(string Id, string Name, List<int> Board, string Entry, int Spine, List<int> Compartments,
    string Heart, string HeartSecurity, string Security, List<RoomRollRow> Rooms, List<string> Hazards, string Text,
    int JunctionBonus = 0);

public sealed record HazardDef(string Id, string Name, string Text);

public sealed record LayoutsContent(
    List<TileDef> Tiles, List<VesselRollRow> VesselRoll,
    List<LayoutRollRow> SpineTurn, List<LayoutRollRow> PassageBranch, List<LayoutRollRow> JunctionBranch,
    List<LayoutRollRow> LoopDoor, List<LayoutRollRow> DoorState, List<LayoutRollRow> HazardRoll,
    List<RoomDef> Rooms, List<RoomRollRow> SmallRooms, List<VesselDef> Vessels, List<HazardDef> Hazards);
