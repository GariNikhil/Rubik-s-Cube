namespace RubiksCube.Core;

/// <summary>
/// A single step in a solution, containing a human-readable name,
/// an explanation of what is happening, and the moves to execute.
/// </summary>
public sealed record SolutionStep(
    string Name,
    string Description,
    IReadOnlyList<CubeMove> Moves);
