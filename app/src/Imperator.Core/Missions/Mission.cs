using System.Text.Json.Serialization;
using Imperator.Core.Layouts;

namespace Imperator.Core.Missions;

// A generated mission, in the scenario format tools/generate_mission.py writes (docs/scenarios/missions/*.yaml), so
// the printed sheet and the app read the same shape. Distances are inches from the board's south-west corner (the
// player's side): x grows east, y grows north.

/// <summary>An objective marker. Tile is the deck tile or land sector it stands in; Prose is how the briefing names it.</summary>
public sealed record MissionObjective(int N, double X, double Y, string Tile, string Label, string Prose);

/// <summary>A guard model, numbered, placed openly near the first objective.</summary>
public sealed record MissionEnemy(int N, string Troop, string Element, double X, double Y, string State);

/// <summary>A hidden contact chip (?N). Where is the tile or sector it was placed in.</summary>
public sealed record MissionContact(int N, double X, double Y, string Where);

/// <summary>The enemy's way in and out. Tile is the deck tile, or "edge" on land.</summary>
public sealed record MissionEntry(double X, double Y, string Tile, string Label);

/// <summary>The mission clock: the die rolled at the end of each round, the time limit, and the check-ins.</summary>
public sealed record MissionClock(string Die, int Limit, int CheckInEvery, int FirstCheckIn);

public sealed record MissionBriefing(string Heading, string Text);

/// <summary>
/// A terrain piece on the mission's board: a deck's room cover (Room) or a land table's terrain (Terrain, Sector).
/// Type is how it is drawn: light, heavy, hill, area, ditch, water or wire.
/// </summary>
public sealed record MissionCover(string Id, double X, double Y, double W, double H, string Type, string Label, string Piece,
    string? Room = null, string? Terrain = null, string? Sector = null);

public sealed record MissionDeploy(double X, double Y, double W, double H, string Side, string Label);

public sealed record MissionSector(string Name, string Text);

/// <summary>A generated mission. Setting, Seed and ContentVersion reproduce it exactly.</summary>
/// <param name="Rolls">The roll log, in order, as the reference writes it.</param>
/// <param name="MissionType">The mission type's id; "mission" in the scenario format.</param>
/// <param name="Hvt">The high-value target's troop id (capture missions), or null.</param>
/// <param name="Links">Ship only: the deck graph (see <see cref="Deck.Links"/>).</param>
public sealed record Mission(
    string Id, string Title, string Kind, string KindLabel, string RulesVersion, string ContentVersion,
    string Setting, int Seed, [property: JsonPropertyName("mission")] string MissionType, int Tier, string Size,
    MissionClock Clock, int AlertStart, string Complication, string Situation, List<MissionBriefing> Briefing,
    DeckBoard Board, List<DeckTile> Tiles, List<DeckDoor> Doors, List<MissionCover> Cover,
    List<MissionObjective> Objectives, MissionEntry EnemyEntry, List<string> SecondExits,
    List<MissionEnemy> Enemies, List<MissionContact> Contacts, string Reinforcement, string? Hvt, List<string> Rolls,
    // Ship
    DeckEntry? Entry = null, string? Vessel = null, string? Hazard = null, List<string>? DeckNotes = null, List<DeckLink>? Links = null,
    // Land
    string? Environment = null, string? Conditions = null, List<MissionDeploy>? Deploy = null, List<MissionSector>? Sectors = null);
