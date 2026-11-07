using System.Linq;

namespace RubiksCube.Core;

public sealed class BeginnerSolver : ISolver
{
    private static readonly CubeMove[] AllMoves = EnumerateMoves().ToArray();
    private static IEnumerable<CubeMove> EnumerateMoves()
    {
        foreach (CubeFace face in Enum.GetValues<CubeFace>()) { yield return new CubeMove(face, 1); yield return new CubeMove(face, 2); yield return new CubeMove(face, 3); }
    }

    public SolverResult Solve(CubeState state)
    {
        if (state.IsSolved()) return new SolverResult(Array.Empty<SolutionStep>());
        var solution = BFS(state, 6);
        if (solution == null || solution.Count == 0) return new SolverResult(Array.Empty<SolutionStep>());
        var steps = new List<SolutionStep> { new SolutionStep("Solve Cube", "Apply these moves to solve the cube.", solution) };
        return new SolverResult(steps);
    }

    public CubeMove? GetHint(CubeState state) { var r = Solve(state); return r.AllMoves.Count > 0 ? r.AllMoves[0] : null; }

    private List<CubeMove>? BFS(CubeState start, int maxDepth)
    {
        var queue = new Queue<(CubeState state, List<CubeMove> path)>();
        queue.Enqueue((start, new List<CubeMove>()));
        var visited = new HashSet<string> { StateKey(start) };

        while (queue.Count > 0)
        {
            var (state, path) = queue.Dequeue();
            if (path.Count >= maxDepth) continue;

            foreach (var move in AllMoves)
            {
                if (path.Count > 0 && move.Face == path[path.Count - 1].Face) continue;
                var next = state.Apply(move);
                var key = StateKey(next);
                if (visited.Contains(key)) continue;
                visited.Add(key);
                var newPath = new List<CubeMove>(path) { move };
                if (next.IsSolved()) return newPath;
                queue.Enqueue((next, newPath));
            }
        }
        return null;
    }

    private string StateKey(CubeState state) => string.Join(",", state.Stickers.Select(c => (int)c));
}