namespace Imperator.Core;

// Land terrain and table setup (rules chapter 12), from content/terrain.yaml.

/// <summary>
/// What a terrain piece does. Kind is area, linear, solid, structure or feature. Cover, Move and Sight are rules
/// text; they default to empty so ContentValidator reports a missing one as tools/validate_content.py does.
/// </summary>
public sealed record TerrainTypeDef(string Id, string Name, string Kind, string Cover = "", string Move = "", string Sight = "");

/// <summary>Height: Level inches per storey or level, and the rules for climbing, height advantage and falling.</summary>
public sealed record HeightDef(int Level, List<string> Rules);

/// <summary>Noise on land: Loud noise carries LoudRange inches.</summary>
public sealed record LandNoiseDef(int LoudRange, string Text);

/// <summary>A visibility condition, rolled once per land battle.</summary>
public sealed record ConditionDef(D6Range Roll, string Id, string Name, string Text);

/// <summary>
/// A piece the mission generator draws for a sector roll: a terrain type id, its footprint in inches, and a label.
/// The rules text is the authority; pieces are its footprint, placed near the sector's middle.
/// </summary>
public sealed record TerrainPieceDef(string Type, double W, double H, string Label);

/// <summary>One sector roll. Pieces is empty for open ground (no cover, for the density check).</summary>
public sealed record SectorRollRow(D6Range Roll, string Text, List<TerrainPieceDef>? Pieces = null);

/// <summary>A land environment: a d6 table per 12" sector, and what a sector becomes in the density check.</summary>
public sealed record EnvironmentDef(string Id, string Name, string Fill, List<SectorRollRow> Table, List<TerrainPieceDef>? FillPieces = null);

public sealed record EnvironmentRollRow(D6Range Roll, string Environment);

public sealed record BoardDef(string Models, string Size, string Sectors);

public sealed record TerrainContent(
    List<TerrainTypeDef> Types, HeightDef Height, LandNoiseDef Noise, List<ConditionDef> Conditions,
    List<EnvironmentDef> Environments, List<EnvironmentRollRow> EnvironmentRoll, List<BoardDef> Boards);
