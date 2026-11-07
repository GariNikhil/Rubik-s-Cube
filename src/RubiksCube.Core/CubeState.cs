namespace RubiksCube.Core;

public sealed class CubeState
{
    private readonly CubeColor[] _stickers;

    private static readonly CubeColor[] SolvedColors = new[]
    {
        CubeColor.White,  // Up
        CubeColor.Red,    // Right
        CubeColor.Green,  // Front
        CubeColor.Yellow, // Down
        CubeColor.Orange, // Left
        CubeColor.Blue,   // Back
    };

    public CubeState()
    {
        _stickers = Enumerable.Range(0, 6)
            .SelectMany(faceIndex => Enumerable.Repeat(SolvedColors[faceIndex], 9))
            .ToArray();
    }

    private CubeState(CubeColor[] stickers) => _stickers = stickers;

    public CubeColor this[CubeFace face, int row, int column]
    {
        get => _stickers[FaceOffset(face) + row * 3 + column];
        private set => _stickers[FaceOffset(face) + row * 3 + column] = value;
    }

    public IReadOnlyList<CubeColor> Stickers => Array.AsReadOnly(_stickers);

    public CubeState Clone() => new((CubeColor[])_stickers.Clone());

    public CubeState Apply(CubeMove move)
    {
        if (!Enum.IsDefined(move.Face))
            throw new ArgumentOutOfRangeException(nameof(move), "Move face is not a valid cube face.");

        var result = Clone();
        for (var turn = 0; turn < move.NormalizedTurns; turn++)
            result = result.ApplyQuarterTurn(move.Face);
        return result;
    }

    public CubeState Apply(IEnumerable<CubeMove> moves)
    {
        var result = this;
        foreach (var move in moves)
            result = result.Apply(move);
        return result;
    }

    public bool IsSolved()
    {
        for (var face = 0; face < 6; face++)
        {
            var faceColor = _stickers[face * 9];
            for (var i = 1; i < 9; i++)
            {
                if (_stickers[face * 9 + i] != faceColor)
                    return false;
            }
        }
        return true;
    }

    private CubeState ApplyQuarterTurn(CubeFace face)
    {
        var next = Clone();
        RotateFaceClockwise(next, face);

        var strips = AdjacentStrips(face);
        for (var index = 0; index < strips.Length; index++)
        {
            var source = strips[(index + strips.Length - 1) % strips.Length];
            for (var cell = 0; cell < 3; cell++)
                next[strips[index][cell].Face, strips[index][cell].Row, strips[index][cell].Column] = this[source[cell].Face, source[cell].Row, source[cell].Column];
        }

        return next;
    }

    private static void RotateFaceClockwise(CubeState state, CubeFace face)
    {
        var original = Enumerable.Range(0, 9).Select(i => state[face, i / 3, i % 3]).ToArray();
        for (var row = 0; row < 3; row++)
            for (var column = 0; column < 3; column++)
                state[face, row, column] = original[(2 - column) * 3 + row];
    }

    // Each cycle lists destination strips in clockwise order as seen from outside the cube.
    // Reversed sequences encode the change in orientation between adjacent face views.
    private static (CubeFace Face, int Row, int Column)[][] AdjacentStrips(CubeFace face) => face switch
    {
        CubeFace.Up => [[(CubeFace.Back, 0, 2), (CubeFace.Back, 0, 1), (CubeFace.Back, 0, 0)], [(CubeFace.Right, 0, 0), (CubeFace.Right, 0, 1), (CubeFace.Right, 0, 2)], [(CubeFace.Front, 0, 0), (CubeFace.Front, 0, 1), (CubeFace.Front, 0, 2)], [(CubeFace.Left, 0, 0), (CubeFace.Left, 0, 1), (CubeFace.Left, 0, 2)]],
        CubeFace.Down => [[(CubeFace.Front, 2, 0), (CubeFace.Front, 2, 1), (CubeFace.Front, 2, 2)], [(CubeFace.Right, 2, 0), (CubeFace.Right, 2, 1), (CubeFace.Right, 2, 2)], [(CubeFace.Back, 2, 2), (CubeFace.Back, 2, 1), (CubeFace.Back, 2, 0)], [(CubeFace.Left, 2, 0), (CubeFace.Left, 2, 1), (CubeFace.Left, 2, 2)]],
        CubeFace.Front => [[(CubeFace.Up, 2, 0), (CubeFace.Up, 2, 1), (CubeFace.Up, 2, 2)], [(CubeFace.Right, 0, 0), (CubeFace.Right, 1, 0), (CubeFace.Right, 2, 0)], [(CubeFace.Down, 0, 2), (CubeFace.Down, 0, 1), (CubeFace.Down, 0, 0)], [(CubeFace.Left, 2, 2), (CubeFace.Left, 1, 2), (CubeFace.Left, 0, 2)]],
        CubeFace.Back => [[(CubeFace.Up, 0, 0), (CubeFace.Up, 0, 1), (CubeFace.Up, 0, 2)], [(CubeFace.Left, 0, 0), (CubeFace.Left, 1, 0), (CubeFace.Left, 2, 0)], [(CubeFace.Down, 2, 2), (CubeFace.Down, 2, 1), (CubeFace.Down, 2, 0)], [(CubeFace.Right, 2, 2), (CubeFace.Right, 1, 2), (CubeFace.Right, 0, 2)]],
        CubeFace.Right => [[(CubeFace.Up, 0, 2), (CubeFace.Up, 1, 2), (CubeFace.Up, 2, 2)], [(CubeFace.Back, 2, 0), (CubeFace.Back, 1, 0), (CubeFace.Back, 0, 0)], [(CubeFace.Down, 0, 2), (CubeFace.Down, 1, 2), (CubeFace.Down, 2, 2)], [(CubeFace.Front, 0, 2), (CubeFace.Front, 1, 2), (CubeFace.Front, 2, 2)]],
        CubeFace.Left => [[(CubeFace.Up, 0, 0), (CubeFace.Up, 1, 0), (CubeFace.Up, 2, 0)], [(CubeFace.Front, 0, 0), (CubeFace.Front, 1, 0), (CubeFace.Front, 2, 0)], [(CubeFace.Down, 2, 2), (CubeFace.Down, 1, 2), (CubeFace.Down, 0, 2)], [(CubeFace.Back, 2, 2), (CubeFace.Back, 1, 2), (CubeFace.Back, 0, 2)]],
        _ => throw new ArgumentOutOfRangeException(nameof(face))
    };

    private static int FaceOffset(CubeFace face) => (int)face * 9;
}