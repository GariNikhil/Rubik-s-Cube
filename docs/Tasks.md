# Tasks and implementation phases

## Phase 1 — Project foundation (complete)

- [x] Create WinUI 3 Windows application
- [x] Add .NET 10 configuration
- [x] Add MVVM Toolkit
- [x] Create application shell
- [x] Add dark theme
- [x] Confirm build and launch

## Phase 2 — Cube engine (complete)

- [x] Implement cube data model (`CubeState`)
- [x] Implement all basic moves (`CubeMove`, quarter/half/double turns)
- [x] Implement inverse moves (`CubeMove.Inverse`)
- [x] Implement scramble generation (`ScrambleGenerator`)
- [x] Add unit tests (`CubeStateTests`)

## Phase 3 — Native interface (complete)

- [x] Add title and layout (WinUI 3 shell)
- [x] Add buttons (Scramble, Undo, Undo all, Reset)
- [x] Add move history display
- [x] Add cube state controls (2D facelet view)
- [x] Add status text
- [x] Add settings panel (theme toggle Light/Dark, persisted via LocalSettings)
- [x] Add keyboard shortcuts (Ctrl+R Scramble, Ctrl+Z Undo, Ctrl+Shift+Z Undo all, Ctrl+, Settings)

## Phase 4 — 3D renderer

- [x] Create local TypeScript renderer project
- [x] Add Three.js dependency
- [x] Define message contract (`CubeRendererMessage`)
- [x] Add `CubeRendererBridge` class
- [x] Build 3D cube (Three.js scene, camera, geometry)
- [x] Add face colors
- [x] Add camera control (OrbitControls)
- [x] Add layer rotation animations
- [x] Connect C# state to TypeScript renderer through WebView2
- [x] Add XAML compile workaround note (XamlCompiler.exe silent failure — use `UseXamlCompilerExecutable=false`)

## Phase 5 — Mathematical visualization (complete)

- [x] Add colored node groups
- [x] Add orbit paths
- [x] Add mathematical connection lines
- [x] Animate the relationships
- [x] Connect colors to the cube state
- [x] Add `toggleMathVis` message type with showNodes/showOrbits/showConnections flags
- [x] Rebuild `dist/cube-renderer.js` (TypeScript compiles cleanly, `npm run build` succeeds)
- [x] Update message contract in Architecture.md

## Phase 6 — Solver and education (in progress)

- [x] Add solver framework (`ISolver`, `SolutionStep`, `SolverResult`)
- [x] Add beginner solver (`BeginnerSolver`) using BFS with state pruning
- [x] Add step explanations (`SolutionStep.Name`/`Description`)
- [x] Add hints (`GetHint` returns first recommended move)
- [x] Add solver smoke tests (`SolverSmokeTests`)
- [ ] Add algorithm display (UI)
- [ ] Animate solution sequences (UI)
- [ ] Wire solver into UI (Solve/Hint buttons)

## Phase 7 — Testing and packaging (partial)

- [x] Run core unit tests
- [ ] Test resize and high-DPI display
- [ ] Test keyboard and mouse interaction
- [ ] Test WebView2 missing-runtime behavior
- [ ] Build MSIX package
- [ ] Test installation and uninstall
- [ ] Create release build

## How to use this file

- Check off items as they are completed.
- Add new tasks under the appropriate phase.
- Move items between phases if priorities change.
- Keep this file in sync with `plan.md` and the actual codebase.
