using RubiksCube.Core;
using Xunit;

namespace RubiksCube.Tests;

public sealed class SolverSmokeTests
{
    [Theory]
    [InlineData(1, 3)]
    [InlineData(2, 4)]
    public void SolvesScrambledCube(int seed, int length)
    {
        var rng = new Random(seed);
        var scramble = new ScrambleGenerator(rng).Generate(length);
        var state = new CubeState().Apply(scramble);
        Assert.False(state.IsSolved());

        var solver = new BeginnerSolver();
        var result = solver.Solve(state);
        Assert.True(result.AllMoves.Count > 0);

        var solved = state.Apply(result.AllMoves);
        Assert.True(solved.IsSolved(), $"Solver did not solve the cube (seed={seed}, len={length})");
    }
}