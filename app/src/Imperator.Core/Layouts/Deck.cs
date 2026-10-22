namespace Imperator.Core.Layouts;

// A generated deck, in the scenario format tools/generate_layout.py writes (docs/scenarios/decks/*.yaml),
// so the printed deck sheets and the app read the same shape. Distances are inches from the south-west
// corner of the board: x grows east, y grows north.

public sealed record DeckBoard(int Width, int Height);

/// <summary>A placed tile. Kind is passage, junction, compartment or small; Room is the room id, if any.</summary>
public sealed record DeckTile(string Id, string Name, string Size, double X, double Y, double W, double H, string Kind, string? Room);

/// <summary>A door on a tile edge. Orientation h lies along an east-west wall, v along a north-south wall.</summary>
public sealed record DeckDoor(string Id, double X, double Y, string Orientation, string State, string? Security, string Note);

public sealed record DeckEntry(double X, double Y, string Orientation, string Label);

public sealed record DeckObjective(double X, double Y, string Label);

/// <summary>A cover piece, W × H inches with its south-west corner at X, Y, in the room tile Room.</summary>
public sealed record DeckCover(string Id, double X, double Y, double W, double H, string Type, string Label, string Piece, string Room);

/// <summary>A deck built by the chapter 11 procedure. Seed and ContentVersion reproduce it exactly.</summary>
/// <param name="TileCounts">Tiles used by size: compartment, passage and junction (small rooms sit on junction tiles).</param>
public sealed record Deck(
    string Id,
    string Title,
    string RulesVersion,
    string ContentVersion,
    string Kind,
    string Vessel,
    int Seed,
    string Situation,
    DeckBoard Board,
    List<DeckTile> Tiles,
    List<DeckDoor> Doors,
    DeckEntry Entry,
    List<DeckCover> Cover,
    DeckObjective? Objective,
    string Hazard,
    int Rolls,
    List<string> Notes,
    Dictionary<string, int> TileCounts);
