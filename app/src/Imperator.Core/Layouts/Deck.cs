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

/// <summary>A connection between two tiles, by tile id. Door is the door's id, or null for a plain opening (a spine joint).</summary>
public sealed record DeckLink(string A, string B, string? Door);

/// <summary>A cover piece, W × H inches with its south-west corner at X, Y, in the room tile Room.</summary>
public sealed record DeckCover(string Id, double X, double Y, double W, double H, string Type, string Label, string Piece, string Room);

/// <summary>A deck built by the chapter 11 procedure. Seed and ContentVersion reproduce it exactly.</summary>
/// <param name="TileCounts">Tiles used by size: compartment, passage and junction (small rooms sit on junction tiles).</param>
/// <param name="Links">The deck graph: every connection between tiles, doors and spine openings alike.</param>
/// <param name="Spine">The spine's tile ids, from the docking passage to the far end.</param>
/// <param name="Heart">The heart tile's id, or null if no heart fitted.</param>
/// <param name="HazardId">The hazard's id (Hazard is its text), or null for none.</param>
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
    Dictionary<string, int> TileCounts,
    List<DeckLink> Links,
    List<string> Spine,
    string? Heart,
    string? HazardId)
{
    /// <summary>
    /// Route distance in tile steps from one tile to every tile, over <see cref="Links"/> (breadth first).
    /// Tiles that cannot be reached get 99, as in the reference (tools/generate_mission.py).
    /// </summary>
    public Dictionary<string, int> RouteFrom(string tileId)
    {
        var adj = Tiles.ToDictionary(t => t.Id, _ => new List<string>());
        foreach (var l in Links)
        {
            adj[l.A].Add(l.B);
            adj[l.B].Add(l.A);
        }
        var dist = new Dictionary<string, int> { [tileId] = 0 };
        var frontier = new List<string> { tileId };
        while (frontier.Count > 0)
        {
            var next = new List<string>();
            foreach (var a in frontier)
                foreach (var b in adj[a])
                    if (dist.TryAdd(b, dist[a] + 1)) next.Add(b);
            frontier = next;
        }
        return Tiles.ToDictionary(t => t.Id, t => dist.GetValueOrDefault(t.Id, 99));
    }
}
