# CLINE MASTER INSTRUCTION — OFFLINE RUBIK'S CUBE LEARNING STUDIO

## Your role

Act as a senior software architect and a patient programming mentor. Help me develop my existing Rubik's Cube application step by step.

I am a beginner, so explain important decisions in plain language. Prefer small, testable changes over large rewrites.

## CRITICAL: PRESERVE EXISTING WORK

Before modifying anything:

1. Inspect the existing repository, Git status, source code, tests, project files, and documentation.
2. Identify which features are implemented, partially implemented, or only documented.
3. Do not overwrite or delete working code just because the architecture is changing.
4. Do not replace the current architecture without explaining the trade-offs and obtaining my approval.
5. Do not claim a feature works until you have verified it.
6. Keep a record of important changes and the files affected.
7. If the working tree contains uncommitted changes, preserve them.
8. Create a documentation and implementation plan before making significant code changes.

## Product vision

Build an offline-first Rubik's Cube solving and mathematical learning studio.

The application must support:

- A virtual Rubik's Cube playground.
- Beginner-friendly learning through advanced speedcubing methods.
- Interactive 3D cube manipulation and animation.
- Mathematical visualizations of cube pieces, permutations, cycles, and relationships.
- A Sudoku-inspired color and constraint visualization based on my reference image.
- Manual entry of the colors on a physical Rubik's Cube.
- Cube-state validation and locally generated solutions.
- Step-by-step solutions, move notation, hints, and educational explanations.
- Cross-platform desktop support and a browser version, with offline desktop use as the primary requirement.

The two supplied reference images are part of the product requirements:

- Reference 1: colored node groups, overlapping orbit paths, and mathematical relationships.
- Reference 2: dense connections, numbered groups, colored cells, and a Sudoku-style grid.

Use these as visual and conceptual references. Do not simply reproduce them as static images.

## Offline-first requirements

The application must work without an internet connection after installation and initial setup.

Core functionality must not depend on:

- External APIs.
- Cloud-hosted AI.
- Online accounts.
- Remote solver services.
- Remote fonts, images, scripts, stylesheets, or other runtime assets.
- A live internet connection for ordinary cube operations.

The following must work offline:

- Cube manipulation.
- Scrambling and resetting.
- Undo and redo.
- Physical cube color entry.
- Cube-state validation.
- Solution generation.
- Beginner and speedcubing algorithms.
- Mathematical visualizations.
- Grid visualization.
- Learning content.
- Settings and locally stored preferences.

Package required libraries and assets locally. Document any setup-time dependency downloads and how to verify offline operation.

A browser version may need a one-time initial load and installation of cached application assets. Clearly distinguish browser offline support from the desktop application's offline guarantees.

Do not make the core application dependent on an AI model. Any future local AI integration must be optional.

## Core product modes

### 1. Cube Playground

Provide:

- An interactive 3D 3×3 Rubik's Cube.
- Camera rotation and zoom.
- Face-turn controls and keyboard shortcuts.
- Standard notation: U, D, L, R, F, B, including inverse and half turns.
- Scramble, reset, undo, and redo.
- Move history and move counters.
- Animation speed controls.
- A solved-state indicator.

The logical cube state must remain correct during animations and user interactions.

### 2. Learn to Solve

Organize learning from beginner to advanced:

- Cube notation and piece identification.
- Beginner layer-by-layer solving.
- Cross and first-layer corners.
- Middle-layer edges.
- Last-layer orientation and permutation.
- CFOP: Cross, F2L, OLL, PLL.
- Optional exploration of Roux and ZZ.
- Algorithm library and practice.
- Step-by-step solution playback.
- Hints and explanations.
- Optional timer and move-count statistics.

Do not attempt to implement every advanced method at once. Build the beginner workflow first, then extend it through tested milestones.

### 3. Physical Cube Input

Allow users to enter the colors of their real cube manually.

Required workflow:

1. Show an editable representation of the six faces.
2. Allow the user to select a sticker and assign a color.
3. Provide a clear face-selection and orientation convention.
4. Show a legend and the expected nine stickers per face.
5. Provide a way to correct mistakes.
6. Validate color counts and cube-state legality.
7. Identify invalid or incomplete input and explain the problem.
8. Allow the user to request a solution only when the state is valid.
9. Display the solution as move notation and animated steps.
10. Allow the user to replay the moves on the virtual cube while following the same moves on the physical cube.

The face-center colors define the face-color mapping for a standard fixed-center 3×3 cube. Do not assume that six sets of nine correctly counted colors automatically form a legal cube state.

Validate corner and edge piece identities, orientation constraints, permutation parity, and other required invariants.

If input is invalid, explain what can be checked without falsely claiming to identify the exact human input mistake.

Camera-based scanning is a possible future feature, not a prerequisite.

### 4. Math Lab

Build a real, interactive mathematical visualization inspired by reference image 1.

Include:

- Color-coded groups.
- Nodes representing identifiable cube pieces or positions.
- Paths showing piece movement.
- Permutation cycles and relationships.
- Animated connections after a move.
- Selection and highlighting of pieces.
- A legend and explanatory labels.
- Synchronization with the actual cube state.

Connections must represent documented mathematical relationships, not merely decorative lines.

### 5. Sudoku-Inspired Grid Lab

Build an experimental visual learning mode inspired by reference image 2.

The goal is to represent selected cube-state information through colors, numbered cells, groups, and constraint relationships.

Before implementing a solver based on this representation:

1. Define the exact mapping between cube pieces or state variables and grid cells.
2. Define what each number, color, group, and connection represents.
3. Specify the constraints mathematically.
4. Prove or test that the mapping preserves the relevant information.
5. Test the model against solved, scrambled, and invalid states.
6. Determine whether the grid model can independently derive a valid cube solution or is primarily an educational visualization.
7. If it produces solutions, convert them to legal cube moves and verify the final cube state.
8. Compare the grid-based approach with a conventional Rubik's Cube solver.

IMPORTANT: A standard Sudoku grid does not directly encode or solve a Rubik's Cube. Do not invent a false mathematical equivalence or present an unvalidated grid as a working solver.

Keep this feature modular and label it experimental until its correctness is established.

### 6. Compare and Explain

Allow users to explore:

- The conventional cube-solving sequence.
- The corresponding cube-piece movements.
- The mathematical node and orbit visualization.
- The Sudoku-inspired grid representation.
- Differences between a proven solving algorithm and an explanatory visualization.

Keep all views synchronized to the same authoritative cube state.

## Technical architecture

The current project is documented as:

- `RubiksCube.Core`: C# cube state, moves, and scramble logic.
- `RubiksCube.App`: C# and WinUI 3 desktop interface.
- `RubiksCube.Renderer`: TypeScript, Three.js, and WebView2 renderer.
- `RubiksCube.Tests`: xUnit tests.
- Existing Markdown documentation and implementation roadmap.

Inspect the actual repository before deciding how to evolve it.

The target is cross-platform desktop and web, with offline desktop use as the priority.

Evaluate the following options:

A. Keep the existing C# cube engine and expose it to a shared cross-platform UI through a suitable local integration.

B. Port the cube domain logic to TypeScript and share it between the web and desktop application.

C. Retain the current Windows application and introduce a separate cross-platform frontend, sharing tested algorithms where practical.

Recommend the least risky option based on the actual state of the repository. Explain migration cost, offline behavior, testing, packaging, and maintenance before requesting approval for a major migration.

Potential technologies include React, TypeScript, Vite, Three.js, Vitest, Playwright, and Tauri. Treat these as candidates to evaluate, not automatic instructions to replace every existing component.

The cube engine must remain independent of rendering and UI code. It must be deterministic, testable, and the authoritative source of cube state.

All required runtime assets must be available locally.

## Documentation to update

Inspect the existing files and update the relevant ones consistently:

- `README.md`
- `plan.md`
- `docs/PRD.md`
- `docs/Architecture.md`
- `docs/Design.md`
- `docs/Memory.md`
- `docs/Rules.md`
- `docs/Tasks.md`

Add a root-level `CLINE.md` containing persistent implementation guidance if appropriate for the repository.

Documentation must clearly distinguish:

- Implemented and verified.
- Implemented but not yet verified.
- Planned.
- Experimental.
- Deferred.

Keep filenames and links consistent with the actual repository. Do not invent project files or mark tasks complete without evidence.

## Implementation roadmap

Phase 1: Inspect the repository and update documentation.

Phase 2: Confirm the cross-platform architecture and offline strategy.

Phase 3: Verify and strengthen the cube engine and its automated tests.

Phase 4: Build or stabilize the interactive 3D playground.

Phase 5: Implement physical cube input and legality validation.

Phase 6: Integrate a local solver and step-by-step solution playback.

Phase 7: Build beginner learning and the algorithm library.

Phase 8: Build Math Lab and synchronize it with cube state.

Phase 9: Design, validate, and implement Sudoku-inspired Grid Lab.

Phase 10: Compare solving approaches and improve educational explanations.

Phase 11: Add offline verification, browser support, desktop packaging, accessibility, and release tests.

Update `docs/Tasks.md` as work progresses.

## Coding and testing rules

- Preserve existing coding conventions unless a documented decision changes them.
- Write tests for mathematical behavior before or alongside implementation.
- Do not use placeholders for features described as complete.
- Avoid introducing unnecessary dependencies.
- Pin or document dependency versions.
- Never require network access for core functionality.
- Run available build, test, and type-check commands after relevant changes.
- Report commands that could not be run and why.
- Do not silently ignore warnings or failed tests.
- Do not make unrelated refactors during a focused task.

## How to work with me

I am a beginner and use VS Code with Cline.

For each milestone:

1. Explain the goal in simple terms.
2. Inspect relevant files before editing.
3. List the intended changes.
4. Make a small, coherent set of edits.
5. Run the appropriate tests.
6. Explain what changed and how to verify it.
7. Update the documentation and task checklist.
8. Stop at architectural decisions that need my approval.

## First task

Do not start by rewriting the entire application.

First inspect the repository, report the current implementation status, identify risks to existing work, and update the documentation to describe this product vision and its requirements.

Then propose the smallest safe next implementation milestone and wait for approval before undertaking a major architecture migration.