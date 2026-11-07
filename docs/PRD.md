# Product Requirements Document (PRD)

## Product overview

**Rubik's Cube** is a Windows desktop application for visualizing, scrambling, and learning Rubik's Cube moves. It provides an accurate cube engine, a desktop shell for interaction, and a path toward a 3D animated view and solver/education features.

## Target audience

- Rubik's Cube enthusiasts and learners
- Anyone wanting a simple desktop cube simulator on Windows

## Goals (in scope)

1. **Accurate cube engine** — correct state transitions for all six faces, quarter/half/double turns, inverses, solved-state detection
2. **Scrambling** — random legal scrambles with no repeated adjacent faces
3. **Session history** — undo last move, undo all moves, reset to solved
4. **Desktop shell** — WinUI 3 app with buttons, move history, and status
5. **2D cube visualization** — projected facelet view reflecting current state
6. **Unit-tested core** — xUnit coverage for state, moves, inverses, and scrambles

## Non-goals (out of scope for now)

- General-purpose cube-solving algorithm (Kociemba, CFOP, etc.)
- Persistent save/load of scrambles or solutions
- 3D animated cube view (renderer scaffold exists but is not connected)
- User accounts, cloud sync, or online features

## User stories

| ID | Story | Acceptance criteria |
|----|-------|---------------------|
| US-01 | As a user, I can start the app and see a solved cube | Application launches; cube displays in solved state; status reads "Solved state" |
| US-02 | As a user, I can scramble the cube | Clicking Scramble applies 20 random moves; cube facelets update; history length increases |
| US-03 | As a user, I can undo the last move | Clicking Undo reverses the most recent move; cube updates; history shortens by one |
| US-04 | As a user, I can undo all moves | Clicking Undo all reverses the full session; cube returns to solved; history clears |
| US-05 | As a user, I can reset the cube | Clicking Reset returns to solved state and clears history |
| US-06 | As a developer, I can run unit tests | `dotnet test` passes; core behaviors are covered |

## Functional requirements

| Req | Description |
|-----|-------------|
| FR-01 | The cube engine must correctly rotate any face 90°, 180°, or 270° |
| FR-02 | Applying a move and its inverse must restore the prior state |
| FR-03 | Four quarter turns of the same face must restore the solved state |
| FR-04 | A quarter turn must preserve exactly nine stickers of each color |
| FR-05 | Scrambles must not repeat the same face on consecutive moves |
| FR-06 | The UI must display the current move count and last action status |
| FR-07 | The solution must build on .NET 10 with WinUI 3 on Windows 10/11 |

## Success metrics

- Application builds and launches without errors
- All xUnit tests pass
- Scramble/Undo/Reset behave correctly per user stories
- TypeScript renderer type-checks cleanly (`npm run check`)

## Release plan

See `docs/Tasks.md` for implementation phases and `plan.md` for the technology stack recommendation. First release targets an x64 MSIX package for Windows.
