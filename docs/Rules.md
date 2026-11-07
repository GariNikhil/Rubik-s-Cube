# Rules and conventions

## General

- Follow the existing code style in the repository.
- Keep the domain layer (`RubiksCube.Core`) free of UI dependencies.
- Prefer immutability: `CubeState.Apply(...)` returns a new instance.
- Use `readonly record struct` for value-like domain types (`CubeMove`).
- Enable `Nullable`, `ImplicitUsings`, and `TreatWarningsAsErrors` (set in `Directory.Build.props`).
- Use `LangVersion=preview` to allow newer C# features.

## C# naming

| Item | Convention | Example |
|------|------------|---------|
| Classes | PascalCase | `CubeState`, `ScrambleGenerator` |
| Structs / records | PascalCase | `CubeMove`, `CubeFace` |
| Methods | PascalCase | `Apply`, `IsSolved`, `Generate` |
| Properties | PascalCase | `Stickers`, `NormalizedTurns` |
| Parameters / locals | camelCase | `move`, `face`, `state` |
| Private fields | camelCase with underscore | `_stickers`, `_history` |

## File organization

- One public type per file when practical (`CubeState.cs`, `CubeMove.cs`, `CubeFace.cs`, `CubeColor.cs`, `ScrambleGenerator.cs`).
- Tests mirror source: `CubeStateTests.cs` tests `CubeState.cs`.
- Namespaces match folder/project: `RubiksCube.Core`, `RubiksCube.App`, `RubiksCube.Tests`.

## TypeScript / renderer

- Use ES modules (`"type": "module"` in `package.json`).
- Type-check before committing: `npm run check` from `src/RubiksCube.Renderer`.
- Keep the renderer message contract stable (`setState`, `animateMove`, `reset`).

## Testing

- Use xUnit (`[Fact]`, `[Theory]`, `[InlineData]`).
- Tests must run with `dotnet test`.
- Cover: new cube solved, move+inverse restore, four quarter turns restore, sticker count preservation, scramble adjacency rule.
- Treat warnings as errors — fix compiler warnings before committing.

## Commits

- Keep commits focused on a single concern.
- Update `docs/Tasks.md` and `plan.md` when a phase is completed.
- Do not commit `bin/`, `obj/`, `node_modules/`, or `.vs/` (covered by `.gitignore`).

## Pull requests

- Include a summary of changes and which user stories or tasks they address.
- Ensure `dotnet build`, `dotnet test`, and `npm run check` all pass.
- Update documentation if behavior or architecture changes.
