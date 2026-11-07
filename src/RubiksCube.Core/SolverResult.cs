namespace RubiksCube.Core;

/// <summary>
/// The result of solving a cube: a sequence of named steps,
/// each containing an explanation and the moves to execute.
/// </summary>
public sealed class SolverResult
{
    public IReadOnlyList<SolutionStep> Steps { get; }
    public IReadOnlyList<CubeMove> AllMoves { get; }
    public int TotalMoves => AllMoves.Count;

    public SolverResult(IReadOnlyList<SolutionStep> steps)
    {
        Steps = steps;
        AllMoves = steps.SelectMany(s => s.Moves).ToList();
    }
}
