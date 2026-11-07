namespace RubiksCube.Core;

/// <summary>
/// Contract for a cube solver. Implementations should produce
/// a step-by-step solution with explanations.
/// </summary>
public interface ISolver
{
    /// <summary>
    /// Solve the given cube state, returning a structured solution.
    /// </summary>
    SolverResult Solve(CubeState state);

    /// <summary>
    /// Get the next recommended move for the given state, or null if solved.
    /// Used for hints.
    /// </summary>
    CubeMove? GetHint(CubeState state);
}
