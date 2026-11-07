# Rubik's Cube

A Windows desktop Rubik's Cube visualization and learning application built with **C#**, **.NET 10**, **WinUI 3**, and **TypeScript/Three.js**.

## Features

- **Cube engine** — independent domain model for cube state, moves, and scrambles (`RubiksCube.Core`)
- **Desktop shell** — WinUI 3 app with a 2D cube view, move history, and status display (`RubiksCube.App`)
- **Scramble generator** — random legal scrambles with no repeated adjacent faces (`ScrambleGenerator`)
- **Undo / Redo support** — invert individual moves or undo the entire session (`CubeMove.Inverse`)
- **Solved-state detection** — `CubeState.IsSolved()` validates the cube is restored
- **3D renderer scaffold** — TypeScript + Three.js bridge ready for WebView2 integration (`RubiksCube.Renderer`)
- **Unit tests** — xUnit test suite covering state, moves, inverses, and scrambles (`RubiksCube.Tests`)

> The current **Undo all** action reverses moves recorded during this app session. It is a session history, not a general-purpose cube-solving algorithm.

## Solution structure

```
rubik's cube/
├── README.md
├── plan.md
├── RubiksCube.sln
├── Directory.Build.props
├── .gitignore
├── docs/
│   ├── Architecture.md
│   ├── Design.md
│   ├── Memory.md
│   ├── PRD.md
│   ├── Rules.md
│   └── Tasks.md
├── @reference image/
│   ├── ref 1.jpg
│   └── ref 2.jpg
├── src/
│   ├── RubiksCube.Core/          # Cube state, moves, colors, faces, scramble generator
│   ├── RubiksCube.App/           # WinUI 3 desktop shell + 2D cube view
│   └── RubiksCube.Renderer/      # TypeScript + Three.js renderer scaffold (WebView2)
└── tests/
    └── RubiksCube.Tests/         # xUnit tests for Core
```

### Projects

| Project | Path | Purpose |
|---------|------|---------|
| `RubiksCube.Core` | `src/RubiksCube.Core` | Cube state, `CubeMove`, `CubeFace`, `CubeColor`, scramble generation |
| `RubiksCube.App` | `src/RubiksCube.App` | WinUI 3 shell, 2D cube rendering, move history, undo/reset |
| `RubiksCube.Renderer` | `src/RubiksCube.Renderer` | TypeScript/Three.js renderer bridge for future WebView2 3D view |
| `RubiksCube.Tests` | `tests/RubiksCube.Tests` | xUnit unit tests for the Core library |

## Core domain model

### CubeState
Represents the 54 stickers of a Rubik's Cube (6 faces × 9 stickers).

- `CubeState()` — creates a solved cube
- `Apply(CubeMove)` / `Apply(IEnumerable<CubeMove>)` — applies one or more moves, returns a new state (immutable)
- `Clone()` — deep copy
- `IsSolved()` — `true` when every face is a single solid color
- `this[CubeFace, row, column]` — sticker accessor

### CubeMove
A readonly record struct describing a face turn.

```csharp
public readonly record struct CubeMove(CubeFace Face, int QuarterTurns = 1)
```

- `NormalizedTurns` — turns normalized to 0–3
- `Inverse()` — returns the move that undoes this one
- `ToString()` — standard notation (e.g. `R`, `R2`, `R'`, `U`, `F'`)

### CubeFace / CubeColor

```csharp
public enum CubeFace { Up, Right, Front, Down, Left, Back }
public enum CubeColor { White, Yellow, Green, Blue, Red, Orange }
```

### ScrambleGenerator
Generates random scramble sequences with no repeated adjacent faces.

```csharp
var scramble = new ScrambleGenerator().Generate(20); // IReadOnlyList<CubeMove>
```

## Prerequisites

- **.NET 10 SDK**
- **Windows App SDK / WinUI 3 tooling** (Windows 10 1809+ or Windows 11)
- **Node.js LTS** and npm (for the TypeScript renderer)

## Build, run, and test

Restore and build the solution:

```powershell
dotnet restore .\RubiksCube.sln
dotnet build .\RubiksCube.sln --configuration Debug
```

Run the unit tests:

```powershell
dotnet test .\RubiksCube.sln --configuration Debug
```

Type-check the renderer:

```powershell
cd src\RubiksCube.Renderer
npm install
npm run check
```

## Usage

Launch `RubiksCube.App` and use the buttons in the shell:

| Action | Behavior |
|--------|----------|
| Scramble | Applies a random 20-move scramble and records it in history |
| Undo | Reverses the most recent move |
| Undo all | Reverses every move in the current session |
| Reset | Returns the cube to the solved state and clears history |

Move history and the current status (e.g. "Solved state") are shown in the UI.

## Development roadmap

The project is implemented in phases (see `plan.md` and `docs/Tasks.md`):

1. **Phase 1** — Project foundation (WinUI 3 shell, .NET 10, MVVM Toolkit)
2. **Phase 2** — Cube engine (data model, moves, inverses, double moves, scrambles, tests)
3. **Phase 3** — Native interface (layout, buttons, move history, settings, keyboard shortcuts)
4. **Phase 4** — 3D renderer (TypeScript + Three.js, WebView2 bridge)
5. **Phase 5** — Mathematical visualization (node groups, orbit paths, connection lines)
6. **Phase 6** — Solver and education (solver framework, algorithm display, hints)
7. **Phase 7** — Testing and packaging (resizing, high-DPI, WebView2 detection, MSIX)

## Documentation

| Doc | Description |
|-----|-------------|
| `docs/Architecture.md` | System architecture, components, and data flow |
| `docs/Design.md` | UI/UX design, rendering approach, color palette |
| `docs/Memory.md` | Memory and performance considerations |
| `docs/PRD.md` | Product requirements and user stories |
| `docs/Rules.md` | Coding conventions and project rules |
| `docs/Tasks.md` | Implementation phases and task tracking |
| `plan.md` | Recommended technology stack and rendering plan |

## Contributing

1. Follow the conventions in `docs/Rules.md`.
2. Add or update unit tests in `RubiksCube.Tests` for Core changes.
3. Type-check the renderer with `npm run check` before committing TypeScript changes.
4. Keep `plan.md` and `docs/Tasks.md` in sync with completed work.

## License

This project is provided as-is for learning and visualization purposes.