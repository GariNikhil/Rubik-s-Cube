# Design

## Design goals

- **Native Windows feel** — WinUI 3 with Fluent styling, high-DPI support, keyboard/mouse/touch input
- **Clean domain separation** — cube logic is pure C# with no UI dependencies
- **Extensible rendering** — 2D view today, Three.js 3D view via WebView2 tomorrow
- **Accessible and packagable** — MSIX distribution, Start menu integration, clean uninstall

## UI framework

| Concern | Technology |
|---------|------------|
| Shell / layout | WinUI 3 + XAML |
| Styling | Fluent design (WinUI defaults) |
| 2D cube view | WinUI `Canvas` + projected polygons |
| 3D cube view (future) | Three.js inside WebView2 |
| 2D diagrams (future) | Win2D where useful |

## 2D cube view

`MainWindow` draws three visible faces (Up, Left, Right) on a `Canvas` using isometric-style projection. Each facelet is a filled polygon with a black stroke.

### Color palette

| CubeColor | Hex | Usage |
|-----------|-----|-------|
| White | `#FFFFFF` | Up face |
| Yellow | `#F3D21B` | Down face |
| Green | `#18D64B` | Front face |
| Blue | `#3987FF` | Back face |
| Red | `#C72E4A` | Right face |
| Orange | `#FF8A1E` | Left face |

The same palette is used in `DrawPatternMap()` to show the six color swatches in a radial arrangement.

## 3D rendering (future)

The `RubiksCube.Renderer` project defines a message-based bridge between C# and TypeScript:

- `setState` — receive 54 sticker colors and redraw the 3D cube
- `animateMove` — receive standard notation (`R`, `U'`, `F2`, …) and animate the layer turn
- `reset` — return the cube to solved
- `toggleMathVis` — toggle visibility of math visualization layers (node groups, orbit paths, connection lines)

## Mathematical visualization (Phase 5)

The renderer now includes a mathematical visualization layer for educational purposes:

- **Node groups** — 54 colored spheres (one per sticker), grouped by face color. Reflect current cube state via `setState`. Visible by default.
- **Orbit paths** — 6 thin ring meshes positioned outside the cube showing the rotation path for each face. Hidden by default (toggleable).
- **Connection lines** — 3×3 grid lines on each face connecting adjacent stickers, showing the spatial relationships between stickers on each face. Hidden by default (toggleable).
- **Orbit highlighting** — During a move animation, the relevant orbit path is highlighted with a yellow ring. The highlight is cleared after the animation completes.

Visibility of each layer can be toggled independently via the `toggleMathVis` message with `showNodes`, `showOrbits`, and `showConnections` boolean fields.

C# → TypeScript communication happens through WebView2 web messaging. The renderer is intentionally decoupled so it can be developed and type-checked independently (`npm run check`).

## Interaction model

| User action | Result |
|-------------|--------|
| Scramble | Generates 20 random moves, applies them, appends to history |
| Undo | Pops last move, applies its inverse, redraws |
| Undo all | Reverses the full history in reverse order, clears history |
| Reset | New solved `CubeState`, clears history |

Status text reflects the current state ("Solved state", "Scrambled with N moves", "Move undone", "State updated").

## Accessibility

- Keyboard navigation through WinUI controls
- High-DPI aware (WinUI 3 handles scaling)
- Clear status text for screen readers
- MSIX packaging for clean installation and uninstallation
