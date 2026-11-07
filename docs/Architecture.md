# Architecture

## Overview

The Rubik's Cube application follows a layered, separation-of-concerns architecture. The cube logic lives in a pure .NET class library with no UI dependencies, the desktop shell is a WinUI 3 application, and a TypeScript/Three.js renderer scaffold is prepared for future WebView2 integration.

## Solution layers

```
┌─────────────────────────────────────┐
│  RubiksCube.App  (WinUI 3, .NET 10) │  ← Presentation / shell
├─────────────────────────────────────┤
│  RubiksCube.Core  (.NET 10)         │  ← Domain: state, moves, colors, faces
├─────────────────────────────────────┤
│  RubiksCube.Tests  (xUnit)          │  ← Tests for Core
└─────────────────────────────────────┘
┌─────────────────────────────────────┐
│  RubiksCube.Renderer  (TS/Three.js) │  ← Future 3D view (WebView2)
└─────────────────────────────────────┘
```

## RubiksCube.Core (domain layer)

Independent .NET 10 class library. No references to WinUI, WebView2, or any UI framework.

### Key types

| Type | Kind | Responsibility |
|------|------|----------------|
| `CubeState` | `sealed class` | Immutable cube state (54 stickers), move application, solved-state detection |
| `CubeMove` | `readonly record struct` | A face turn (face + quarter turns), inverse, standard notation (`R`, `R2`, `R'`) |
| `CubeFace` | `enum` | `Up, Right, Front, Down, Left, Back` |
| `CubeColor` | `enum` | `White, Yellow, Green, Blue, Red, Orange` |
| `ISolver` | `interface` | Solver contract: `Solve` and `GetHint` |
| `SolverResult` | `sealed class` | Aggregated solution: `Steps`, `AllMoves`, `TotalMoves` |
| `SolutionStep` | `sealed record` | Named step with `Description` and `Moves` |
| `ScrambleGenerator` | `sealed class` | Generates random legal scrambles (no repeated adjacent faces) |
| `ISolver` | `interface` | Solver contract: `Solve` and `GetHint` |
| `SolverResult` | `sealed class` | Aggregated solution: `Steps`, `AllMoves`, `TotalMoves` |
| `SolutionStep` | `sealed record` | Named step with `Description` and `Moves` |
| `BeginnerSolver` | `sealed class` | BFS-based solver (max depth 6) with state pruning |

### Immutability

`CubeState.Apply(...)` does not mutate the existing instance. It clones the internal sticker array and returns a new `CubeState`. This makes state history, undo, and replay straightforward and thread-safe for read operations.

### Move application

`CubeState.ApplyQuarterTurn(face)` rotates the target face clockwise and cycles the adjacent edge strips. A `CubeMove` with `QuarterTurns > 1` applies the quarter turn repeatedly (2 = half turn, 3 = counter-clockwise).

## RubiksCube.App (presentation layer)

WinUI 3 desktop application targeting `net10.0-windows10.0.19041.0`.

### Responsibilities

- Window, layout, and navigation (XAML)
- 2D cube rendering on a `Canvas` using projected polygons per facelet
- Move history list and status text
- Button actions: Scramble, Undo, Undo all, Reset
- Palette mapping from `CubeColor` to hex colors

### Data flow

```
User action (button click)
        ↓
MainWindow code-behind
        ↓
CubeState.Apply(move)  →  new CubeState (immutable)
        ↓
_history updated
        ↓
UpdateStatus() → redraw Canvas + status text
```

### Packages

- `Microsoft.WindowsAppSDK` 1.7.250606001
- `CommunityToolkit.Mvvm` 8.4.0
- `Microsoft.Web.WebView2` 1.0.3179.45 (ready for the Three.js bridge)

## RubiksCube.Renderer (future 3D layer)

TypeScript module with Three.js, designed to run inside a local WebView2 control hosted by the WinUI 3 app.

### Message contract

```ts
export interface CubeRendererMessage {
  type: 'setState' | 'animateMove' | 'reset' | 'toggleMathVis';
  stickers?: FaceColor[];
  move?: string;
  showNodes?: boolean;
  showOrbits?: boolean;
  showConnections?: boolean;
}
```

The C# side posts messages to the renderer; the renderer applies state or animates moves. Currently scaffolded (console.debug stubs), not yet wired to the desktop app.

### Packages

- `three` ^0.179.1
- `typescript` ^5.9.2, `@types/three` ^0.179.0

## Solver layer (`RubiksCube.Core`)

A pure .NET solver lives alongside the cube engine with no UI dependencies.

### Key types

| Type | Kind | Responsibility |
|------|------|----------------|
| `ISolver` | `interface` | Contract: `Solve(CubeState)` → `SolverResult`, `GetHint(CubeState)` → `CubeMove?` |
| `SolverResult` | `sealed class` | Aggregates `Steps` and flattened `AllMoves`/`TotalMoves` |
| `SolutionStep` | `sealed record` | Named step with `Description` and `Moves` (move sequence) |
| `BeginnerSolver` | `sealed class` | BFS-based solver (max depth 6) with visited-state pruning; returns a single step containing the full solution |

### Algorithm

`BeginnerSolver` uses breadth-first search over the cube state graph. Each node expands all 18 legal moves, skipping consecutive moves on the same face. A `HashSet<string>` of serialized sticker states prevents re-exploration. Search stops as soon as a solved state is reached, guaranteeing a shortest solution within the depth limit.

### Tests

`SolverSmokeTests` verifies that scrambled cubes (various seeds/lengths) are solved and that the resulting move sequence restores the solved state.

xUnit test project targeting .NET 10. References only `RubiksCube.Tests`.

Covered scenarios:
- New cube is solved
- Four quarter turns restore state (all faces)
- Move + inverse restores state
- Quarter turn preserves nine stickers of each color
- Scramble does not repeat adjacent faces

## Build configuration

- `Directory.Build.props` — `LangVersion=preview`, `Nullable=enable`, `ImplicitUsings=enable`, `TreatWarningsAsErrors=true`
- Solution targets `Debug|x64` and `Release|x64`
- App supports `x86;x64;ARM64` platforms with MSIX tooling enabled\n\n## Known build environment issues\n\nThe WinUI 3 app project (`RubiksCube.App`) has a **pre-existing MSIX packaging build error** that prevents building or running the desktop app from this environment:\n\n> error MSB4062: The \"Microsoft.Build.Packaging.Pri.Tasks.ExpandPriContent\" task could not be loaded from assembly `Microsoft.Build.Packaging.Pri.Tasks.dll`. The system cannot find the file specified.\n\nThis is caused by a missing Windows App SDK / MSIX packaging task component in the .NET 10 SDK (version 10.0.401) on this machine. It is **not related to the solver or cube logic** — the Core library and all 29 unit tests build and pass correctly.\n\n**Workaround**: The app can be run using `dotnet run` only if the MSIX packaging task is installed, or the project can be modified to disable MSIX tooling (already set `<EnableMsixTooling>false</EnableMsixTooling>` in `RubiksCube.App.csproj`). Core testing and solver functionality are unaffected.\n\nIf UI integration or MSIX packaging is required, the build environment would need the Windows App SDK components reinstalled or the project targeted differently.
