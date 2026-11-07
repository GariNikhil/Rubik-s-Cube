namespace RubiksCube.Core;

public readonly record struct CubeMove(CubeFace Face, int QuarterTurns = 1)
{
    public int NormalizedTurns => ((QuarterTurns % 4) + 4) % 4;

    public CubeMove Inverse() => new(Face, 4 - NormalizedTurns);

    public override string ToString()
    {
        var suffix = NormalizedTurns switch
        {
            2 => "2",
            3 => "'",
            _ => string.Empty
        };
        return $"{Face switch { CubeFace.Up => "U", CubeFace.Right => "R", CubeFace.Front => "F", CubeFace.Down => "D", CubeFace.Left => "L", _ => "B" }}{suffix}";
    }
}