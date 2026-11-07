namespace RubiksCube.Core;

public sealed class ScrambleGenerator
{
    private static readonly CubeFace[] Faces = Enum.GetValues<CubeFace>();
    private readonly Random _random;

    public ScrambleGenerator(Random? random = null) => _random = random ?? Random.Shared;

    public IReadOnlyList<CubeMove> Generate(int length = 20)
    {
        if (length < 1) throw new ArgumentOutOfRangeException(nameof(length));
        var moves = new List<CubeMove>(length);
        CubeFace? previous = null;
        while (moves.Count < length)
        {
            var face = Faces[_random.Next(Faces.Length)];
            if (face == previous) continue;
            moves.Add(new CubeMove(face, _random.Next(1, 4)));
            previous = face;
        }
        return moves;
    }
}