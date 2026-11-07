# Memory and performance

## CubeState memory profile

A `CubeState` holds 54 `CubeColor` values in a `CubeColor[]` array.

- `CubeColor` is an `enum` backed by `int` (4 bytes)
- Sticker array: 54 × 4 bytes ≈ 216 bytes + array overhead
- `Clone()` copies the array on every `Apply(...)` call

For session-level move history (tens to hundreds of moves), this is negligible. Each state is small and short-lived, making the garbage collector the appropriate reclamation mechanism.

## Move history

`MainWindow` keeps a `List<CubeMove>` for the current session. `CubeMove` is a 16-byte readonly record struct (enum + int). A 100-move session uses ~1.6 KB for history. Undo operations mutate this list in place (no new state allocations for the list itself).

## Renderer memory

- The 2D WinUI view creates `Polygon` and `SolidColorBrush` objects per facelet on each redraw. Old visuals are cleared with `CubeCanvas.Children.Clear()`, allowing WinUI/XAML to dispose them.
- The TypeScript renderer (future) will hold Three.js geometries, materials, and scene graph objects. These should be disposed (`geometry.dispose()`, `material.dispose()`) when the WebView2 control is unloaded to avoid GPU memory leaks.

## Mathematical visualization memory (Phase 5)

The math-vis layer adds the following Three.js objects to the scene:

- **Nodes**: 54 small `SphereGeometry` meshes (radius 0.05) grouped by color into 6 `THREE.Group` instances.
- **Orbit paths**: 6 `RingGeometry` meshes (inner 1.40, outer 1.48) — one per face.
- **Connection lines**: 6 groups of `Line` segments (4 lines per face × 2 directions = 12 lines per face, 72 total) using `BufferGeometry`.
- **Orbit highlight**: 1 `RingGeometry` mesh (inner 1.40, outer 1.52) shown only during move animations.

All geometries and materials are disposed in `dispose()` to avoid GPU memory leaks. The `connectionLines` group and node groups are traversed recursively to dispose nested objects. The orbit highlight is disposed and recreated on each move animation.

| Area | Consideration |
|------|---------------|
| Move application | O(stickers) clone per move — trivial for 54 stickers |
| History undo | Re-applying N inverse moves is O(N × stickers) — fine for session lengths |
| Redraw | Full canvas clear + redraw per status update — acceptable for 54 facelets |
| 3D animation | Future WebView2 layer rotations should use `requestAnimationFrame` and tweening |

## When to revisit

- If move history grows to thousands of entries (e.g. solver output), consider a command pattern with pooled states or a persistent data structure.
- If the 3D renderer is added, profile WebView2 memory and dispose Three.js resources on navigation away.
