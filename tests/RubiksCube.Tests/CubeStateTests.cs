using RubiksCube.Core;
using Xunit;

namespace RubiksCube.Tests;

public sealed class CubeStateTests
{
    [Fact]
    public void NewCubeIsSolved() => Assert.True(new CubeState().IsSolved());

    [Theory]
    [InlineData(CubeFace.Up)]
    [InlineData(CubeFace.Right)]
    [InlineData(CubeFace.Front)]
    [InlineData(CubeFace.Down)]
    [InlineData(CubeFace.Left)]
    [InlineData(CubeFace.Back)]
    public void FourQuarterTurnsRestoreState(CubeFace face)
    {
        var state = new CubeState().Apply(new CubeMove(face, 4));
        Assert.True(state.IsSolved());
    }

    [Fact]
    public void MoveAndInverseRestoreState()
    {
        var move = new CubeMove(CubeFace.Front);
        var state = new CubeState().Apply(move).Apply(move.Inverse());
        Assert.True(state.IsSolved());
    }

    [Theory]
    [InlineData(CubeFace.Up)]
    [InlineData(CubeFace.Right)]
    [InlineData(CubeFace.Front)]
    [InlineData(CubeFace.Down)]
    [InlineData(CubeFace.Left)]
    [InlineData(CubeFace.Back)]
    public void QuarterTurnPreservesNineStickersOfEachColor(CubeFace face)
    {
        var state = new CubeState().Apply(new CubeMove(face));
        foreach (var color in Enum.GetValues<CubeColor>())
            Assert.Equal(9, state.Stickers.Count(sticker => sticker == color));
    }


    [Fact]
    public void ScrambleDoesNotRepeatAdjacentFaces()
    {
        var moves = new ScrambleGenerator(new Random(42)).Generate(40);
        Assert.DoesNotContain(moves.Zip(moves.Skip(1)), pair => pair.First.Face == pair.Second.Face);
    }

    [Fact]
    public void RMoveCyclesAdjacentStripsCorrectly()
    {
        // Solved: Up=White, Right=Red, Front=Green, Down=Yellow, Left=Orange, Back=Blue
        var state = new CubeState().Apply(new CubeMove(CubeFace.Right));

        // Up right column <- Front right column (Green)
        Assert.Equal(CubeColor.Green, state[CubeFace.Up, 0, 2]);
        Assert.Equal(CubeColor.Green, state[CubeFace.Up, 1, 2]);
        Assert.Equal(CubeColor.Green, state[CubeFace.Up, 2, 2]);

        // Front right column <- Down right column (Yellow)
        Assert.Equal(CubeColor.Yellow, state[CubeFace.Front, 0, 2]);
        Assert.Equal(CubeColor.Yellow, state[CubeFace.Front, 1, 2]);
        Assert.Equal(CubeColor.Yellow, state[CubeFace.Front, 2, 2]);

        // Down right column <- Back left column (Blue)
        Assert.Equal(CubeColor.Blue, state[CubeFace.Down, 0, 2]);
        Assert.Equal(CubeColor.Blue, state[CubeFace.Down, 1, 2]);
        Assert.Equal(CubeColor.Blue, state[CubeFace.Down, 2, 2]);

        // Back left column <- Up right column (White, reversed)
        Assert.Equal(CubeColor.White, state[CubeFace.Back, 2, 0]);
        Assert.Equal(CubeColor.White, state[CubeFace.Back, 1, 0]);
        Assert.Equal(CubeColor.White, state[CubeFace.Back, 0, 0]);
    }

    [Fact]
    public void UMoveCyclesAdjacentStripsCorrectly()
    {
        var state = new CubeState().Apply(new CubeMove(CubeFace.Up));

        // Back top row <- Left top row (Orange, reversed)
        Assert.Equal(CubeColor.Orange, state[CubeFace.Back, 0, 2]);
        Assert.Equal(CubeColor.Orange, state[CubeFace.Back, 0, 1]);
        Assert.Equal(CubeColor.Orange, state[CubeFace.Back, 0, 0]);

        // Right top row <- Back top row (Blue)
        Assert.Equal(CubeColor.Blue, state[CubeFace.Right, 0, 0]);
        Assert.Equal(CubeColor.Blue, state[CubeFace.Right, 0, 1]);
        Assert.Equal(CubeColor.Blue, state[CubeFace.Right, 0, 2]);

        // Front top row <- Right top row (Red)
        Assert.Equal(CubeColor.Red, state[CubeFace.Front, 0, 0]);
        Assert.Equal(CubeColor.Red, state[CubeFace.Front, 0, 1]);
        Assert.Equal(CubeColor.Red, state[CubeFace.Front, 0, 2]);

        // Left top row <- Front top row (Green)
        Assert.Equal(CubeColor.Green, state[CubeFace.Left, 0, 0]);
        Assert.Equal(CubeColor.Green, state[CubeFace.Left, 0, 1]);
        Assert.Equal(CubeColor.Green, state[CubeFace.Left, 0, 2]);
    }

    [Fact]
    public void FMoveCyclesAdjacentStripsCorrectly()
    {
        var state = new CubeState().Apply(new CubeMove(CubeFace.Front));

        // Up bottom row <- Left right column (Orange, reversed)
        Assert.Equal(CubeColor.Orange, state[CubeFace.Up, 2, 0]);
        Assert.Equal(CubeColor.Orange, state[CubeFace.Up, 2, 1]);
        Assert.Equal(CubeColor.Orange, state[CubeFace.Up, 2, 2]);

        // Right left column <- Up bottom row (White)
        Assert.Equal(CubeColor.White, state[CubeFace.Right, 0, 0]);
        Assert.Equal(CubeColor.White, state[CubeFace.Right, 1, 0]);
        Assert.Equal(CubeColor.White, state[CubeFace.Right, 2, 0]);

        // Down top row <- Right left column (Red, reversed)
        Assert.Equal(CubeColor.Red, state[CubeFace.Down, 0, 2]);
        Assert.Equal(CubeColor.Red, state[CubeFace.Down, 0, 1]);
        Assert.Equal(CubeColor.Red, state[CubeFace.Down, 0, 0]);

        // Left right column <- Down top row (Yellow)
        Assert.Equal(CubeColor.Yellow, state[CubeFace.Left, 2, 2]);
        Assert.Equal(CubeColor.Yellow, state[CubeFace.Left, 1, 2]);
        Assert.Equal(CubeColor.Yellow, state[CubeFace.Left, 0, 2]);
    }

    [Theory]
    [InlineData(CubeFace.Up, "U")]
    [InlineData(CubeFace.Right, "R")]
    [InlineData(CubeFace.Front, "F")]
    [InlineData(CubeFace.Down, "D")]
    [InlineData(CubeFace.Left, "L")]
    [InlineData(CubeFace.Back, "B")]
    public void MoveToStringProducesStandardNotation(CubeFace face, string expected)
    {
        Assert.Equal(expected, new CubeMove(face).ToString());
    }

    [Theory]
    [InlineData(CubeFace.Up, 1)]
    [InlineData(CubeFace.Right, 2)]
    [InlineData(CubeFace.Front, 3)]
    public void InverseOfInverseIsOriginalMove(CubeFace face, int turns)
    {
        var move = new CubeMove(face, turns);
        Assert.Equal(move.NormalizedTurns, move.Inverse().Inverse().NormalizedTurns);
        Assert.Equal(face, move.Inverse().Inverse().Face);
    }
}