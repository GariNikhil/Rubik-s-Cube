# Recommended technology plan

## 1. Primary programming language: **C#**

I recommend **C#** as the main programming language.

Reasons:

- Best fit for a native Windows desktop application
- Strong support for Windows UI, packaging, input, files, settings, and accessibility
- Good performance for cube calculations and solving algorithms
- Easier long-term maintenance than building the whole application with JavaScript
- .NET 10 is currently a supported Long-Term Support release through November 2028. ([learn.microsoft.com](https://learn.microsoft.com/dotnet/core/releases-and-support?utm_source=openai))

## 2. Windows UI framework: **WinUI 3**

I recommend:

```text
C# + .NET 10 + WinUI 3 + Windows App SDK
```

Microsoft recommends WinUI 3 and the Windows App SDK for new native Windows desktop applications. WinUI supports XAML, modern Windows controls, Fluent styling, high-DPI displays, keyboard, mouse, touch, and Windows packaging. ([learn.microsoft.com](https://learn.microsoft.com/windows/apps/desktop?utm_source=openai))

The application will target:

- Windows 10 version 1809 or later
- Windows 11
- x64 initially
- ARM64 support can be added later if needed

WinUI 3 is also suitable for distributing the finished application as an MSIX package. ([learn.microsoft.com](https://learn.microsoft.com/en-us/windows/apps/winui?utm_source=openai))

---

# Recommended rendering approach

The reference design contains:

- A colorful 3D Rubik’s Cube
- Mathematical node diagrams
- Orbit lines
- Animated visual connections
- Color-based educational visualizations

For this reason, I recommend a **hybrid rendering architecture**.

## Main application

Use:

```text
C# + WinUI 3 + XAML
```

For:

- Window and navigation
- Buttons and controls
- Settings
- Menus
- Move history
- Algorithm explanations
- File storage
- Application state
- Accessibility
- Packaging

## 3D cube and mathematical visualization

Use:

```text
HTML + CSS + TypeScript + Three.js
```

inside a local **WebView2** control.

WinUI 3 supports WebView2, which embeds the Microsoft Edge/Chromium rendering engine inside a native desktop application. It can load local HTML, execute JavaScript, and communicate between C# and the web renderer. ([learn.microsoft.com](https://learn.microsoft.com/en-us/windows/apps/develop/ui/controls/webview2?utm_source=openai))

This gives us better support for:

- Smooth 3D cube rotation
- Facelet animations
- Camera controls
- Lighting and shadows
- Orbit diagrams
- Glow effects
- Particle effects
- Responsive visual layouts
- CSS-based neon styling

This approach technically uses two languages:

```text
C#       → Windows app and application logic
TypeScript → 3D rendering and visual effects
```

C# remains the primary language. TypeScript is limited to the rendering component.

## Alternative: all-C# rendering

A completely native alternative would use:

```text
C# + WinUI 3 + Win2D
```

Win2D provides GPU-accelerated 2D drawing and integrates with WinUI. It would work well for mathematical graphs, lines, nodes, and diagrams. ([learn.microsoft.com](https://learn.microsoft.com/en-us/windows/apps/develop/win2d/in-a-core-app?utm_source=openai))

However, it would require more manual work to create a convincing interactive 3D cube. Therefore:

- **Hybrid WebView2 + Three.js:** recommended
- **Win2D-only:** possible fallback if we decide the cube should be a 2D perspective illustration instead of a fully interactive 3D model

---

# Framework and plugin list

## Required Windows components

### 1. .NET 10 SDK

Used for:

- C# compilation
- Project management
- Testing
- Publishing

### 2. Windows App SDK

Used for:

- WinUI 3
- Window management
- Windows application lifecycle
- Native Windows APIs
- MSIX integration

### 3. WinUI 3

Used for the native Windows interface.

### 4. XAML

Used to define the interface layout and styling.

### 5. WebView2

Used to host the local 3D visualization. The WinUI 3 Windows App SDK integration includes WebView2 support, but the target computer must have the WebView2 Runtime available. ([learn.microsoft.com](https://learn.microsoft.com/en-us/windows/apps/develop/ui/controls/webview2?utm_source=openai))

The application should check for WebView2 at startup and show a clear installation message if it is missing.

## Recommended NuGet packages

### `CommunityToolkit.Mvvm`

Used for:

- MVVM architecture
- Observable properties
- Commands
- View models
- Data binding
- Validation helpers

The MVVM Toolkit is maintained as part of the .NET Community Toolkit and is designed to work with WinUI and other .NET UI frameworks. ([learn.microsoft.com](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/?utm_source=openai))

### `Microsoft.Graphics.Win2D`

Optional, but recommended for:

- Mathematical orbit lines
- Node diagrams
- Graph connections
- Glow effects
- 2D overlays
- Custom educational diagrams

### `Microsoft.WindowsAppSDK`

Required for the WinUI 3 application.

### Testing packages

We can use:

```text
xUnit
Microsoft.NET.Test.Sdk
```

for testing:

- Cube state transitions
- Move rotations
- Scramble generation
- Solving logic
- Color mapping
- Serialization

## Recommended npm packages

Only the renderer should use npm packages:

```text
three
typescript
vite
```

Possible additions later:

```text
@types/three
lil-gui
```

`lil-gui` would be useful during development for testing camera angles, lighting, and animation settings, but it should not necessarily be included in the final interface.

---

# Proposed application architecture

```text
RubiksCube.sln
│
├── RubiksCube.App
│   ├── App.xaml
│   ├── MainWindow.xaml
│   ├── Pages
│   ├── Controls
│   ├── ViewModels
│   └── Services
│
├── RubiksCube.Core
│   ├── CubeState
│   ├── CubeFace
│   ├── CubeMove
│   ├── MoveParser
│   ├── ScrambleGenerator
│   ├── Solver
│   └── ColorMapping
│
├── RubiksCube.Renderer
│   ├── index.html
│   ├── src
│   │   ├── cube-renderer.ts
│   │   ├── orbit-visualization.ts
│   │   ├── renderer-messages.ts
│   │   └── theme.ts
│   └── package.json
│
├── RubiksCube.Tests
│   ├── CubeStateTests.cs
│   ├── MoveTests.cs
│   ├── ScrambleTests.cs
│   └── SolverTests.cs
│
└── Assets
    ├── Fonts
    ├── Icons
    └── ReferenceImages
```

## Important design rule

The cube logic must not depend on the visual renderer.

The core logic should be able to run independently:

```text
CubeState → Move → New CubeState
```

Then the renderer receives the updated cube state and animates it.

This separation makes it possible to:

- Test the solving logic without opening the UI
- Replace the 3D renderer later
- Add an AI solver later
- Export and import cube positions
- Create a command-line test tool
- Avoid visual code affecting mathematical correctness

---

# Core cube model

The cube engine should support:

## Faces

```text
Up
Down
Left
Right
Front
Back
```

## Standard colors

```text
White
Yellow
Green
Blue
Red
Orange
```

## Standard moves

```text
U, U', U2
D, D', D2
L, L', L2
R, R', R2
F, F', F2
B, B', B2
```

## Future support

- Wide moves
- Slice moves
- Cube rotations
- Beginner solution
- CFOP solver
- Layer-by-layer explanation
- Algorithm notation display
- Undo/redo
- Move timing

---

# User interface plan

## Main window

The visual design should be inspired by the uploaded images but not copy the Instagram interface.

### Overall style

- Dark charcoal or deep teal background
- Neon green educational headings
- Bright cube colors
- Soft glowing borders
- Rounded panels
- Subtle animated orbit lines
- High contrast text
- Minimal but polished controls

## Proposed layout

```text
┌──────────────────────────────────────────────────────────┐
│ Math Thinking for Smart Rubik's Cube Solving             │
│ Learn the patterns behind every move                     │
├───────────────────────────────┬──────────────────────────┤
│                               │                          │
│ Mathematical visualization    │ Interactive 3D cube      │
│ Nodes, orbit paths, colors    │ Rotate / zoom / animate  │
│                               │                          │
├───────────────────────────────┴──────────────────────────┤
│ Scramble | Solve | Reset | Undo | Redo                   │
├──────────────────────────────────────────────────────────┤
│ Move history                                               │
├──────────────────────────────────────────────────────────┤
│ Current algorithm / educational explanation               │
└──────────────────────────────────────────────────────────┘
```

## Main screens

### 1. Home / Learn

- Title
- Animated mathematical background
- Cube preview
- Start button
- Learn how the cube works

### 2. Cube Playground

- Interactive cube
- Scramble button
- Move buttons
- Keyboard controls
- Undo/redo
- Reset cube

### 3. Solver

- Enter or edit cube state
- Solve button
- Step-by-step move list
- Animated solution
- Explanation of each step

### 4. Math Visualization

- Color groups
- Node relationships
- Orbit paths
- Permutation visualization
- Facelet movement diagram

### 5. Settings

- Theme
- Animation speed
- Reduce motion
- Cube size
- Camera angle
- Sound effects
- Show notation
- Show educational labels

---

# Development tools

## Required

- Windows 10 or Windows 11
- .NET 10 SDK
- Windows App SDK
- Node.js LTS
- Git
- VS Code

## Recommended optional tool

### Visual Studio

Visual Studio is recommended if we need:

- Advanced WinUI/XAML debugging
- Visual Studio designer features
- Native Windows debugging
- MSIX packaging tools
- Easier NuGet management

Microsoft’s current WinUI setup supports both a command-line workflow and Visual Studio. Microsoft documents the Windows App Development CLI as suitable for terminal-based workflows, including agent-assisted development. ([learn.microsoft.com](https://learn.microsoft.com/windows/apps/winui/winui3/create-your-first-winui3-app?pivots=winui3-packaged-csharp&utm_source=openai))

Because you are working in VS Code, we can initially use:

```text
VS Code
PowerShell
dotnet CLI
winapp CLI
npm
```

and install Visual Studio later only if the project needs its additional debugging or packaging support.

---

# Packaging and distribution

The application should use:

```text
MSIX
```

for the first Windows release.

Benefits:

- Clean installation
- Start menu integration
- Uninstallation support
- Versioned packages
- Safer application permissions
- Possible Microsoft Store distribution later

For the first release, we can create a self-contained Windows installer so the user does not need to manually install the .NET runtime.

The WebView2 deployment strategy should use the Evergreen Runtime where possible, because it updates automatically and is already present on most current Windows installations. The application should still detect missing WebView2 on unsupported or clean systems. ([learn.microsoft.com](https://learn.microsoft.com/en-us/windows/apps/develop/ui/controls/webview2?utm_source=openai))

---

# Implementation phases

## Phase 1 — Project foundation

- Create WinUI 3 Windows application
- Add .NET 10 configuration
- Add MVVM Toolkit
- Create application shell
- Add dark theme
- Confirm build and launch

## Phase 2 — Cube engine

- Implement cube data model
- Implement all basic moves
- Implement inverse moves
- Implement double moves
- Implement scramble generation
- Add unit tests

## Phase 3 — Native interface

- Add title and layout
- Add buttons
- Add move history
- Add cube state controls
- Add settings panel
- Add keyboard shortcuts

## Phase 4 — 3D renderer

- Create local TypeScript renderer
- Add Three.js
- Build 3D cube
- Add face colors
- Add camera control
- Add layer rotation animations
- Connect C# state to TypeScript renderer through WebView2

## Phase 5 — Mathematical visualization (complete)

- Add colored node groups
- Add orbit paths
- Add mathematical connection lines
- Animate the relationships
- Connect colors to the cube state
- Add `toggleMathVis` message type with showNodes/showOrbits/showConnections flags
- Rebuild `dist/cube-renderer.js` (TypeScript compiles cleanly, `npm run build` succeeds)
- Update message contract in Architecture.md
- Update Design.md with math-vis documentation
- Update Memory.md with math-vis memory profile

## Phase 6 — Solver and education

- Add solver framework
- Add beginner step explanations
- Add algorithm display
- Animate solution sequences
- Add hints

## Phase 7 — Testing and packaging

- Run core unit tests
- Test resize and high-DPI display
- Test keyboard and mouse interaction
- Test WebView2 missing-runtime behavior
- Build MSIX package
- Test installation and uninstall
- Create release build

---

# Final recommendation

The best stack for this project is:

```text
Primary language:
C#

Framework:
WinUI 3

Platform:
Windows App SDK

Runtime:
.NET 10

UI:
XAML

Architecture:
MVVM

Native graphics:
Win2D where useful

3D rendering:
Three.js through local WebView2

Testing:
xUnit

Package manager:
NuGet + npm

Packaging:
MSIX

Development:
VS Code + PowerShell + dotnet CLI
Optional Visual Studio for advanced WinUI debugging
```

This gives us a genuine Windows application with a modern native shell while still allowing the colorful 3D and mathematical visualizations shown in the reference images.

The workspace currently contains only the two reference images and no existing application project, so the implementation can start as a clean project. No files have been modified yet. When you are satisfied with this technical plan, **toggle to Act mode** and I can create the project foundation.