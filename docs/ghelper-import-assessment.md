# G-Helper Import Assessment

This is a small, token-safe assessment for possibly basing VictusControl directly on G-Helper and then replacing ASUS-specific behavior with HP Victus behavior. No files were copied, no source code was changed, and the G-Helper reference repository was not modified.

## Current VictusControl State

- Repository path: `D:\Projects\Workspace\VictusControl`
- Current branch: `main`
- Recent state: `v0.4.0-settings-and-logging-foundation` is committed, followed by the next milestone planning update.
- Current structure: layered .NET solution with `src/`, `tests/`, `docs/`, AI workflow files, and reference tracking docs.
- Current implementation: solution skeleton, read-only device identity/capability contracts, a Windows identity provider, JSON settings foundation, and privacy-aware local logging.
- Current git status before this assessment: clean.

## G-Helper Top-Level Structure Summary

Reference path: `D:\Projects\Workspace\references\g-helper`

Top-level items:

- `.github/`
- `app/`
- `docs/`
- `.gitignore`
- `crowdin.yml`
- `LICENSE`

The actual application source is concentrated under `app/`. The root `docs/README.md` describes G-Helper as a lightweight ASUS laptop control utility and Armoury Crate alternative.

## G-Helper Solution And Project Summary

G-Helper solution/project files:

- `app/GHelper.sln`
- `app/GHelper.csproj`

Project characteristics from `GHelper.csproj`:

- C#/.NET Windows desktop app
- `net10.0-windows`
- Windows Forms UI via `UseWindowsForms`
- x64 platform target
- `OutputType` is `WinExe`
- `StartupObject` is `GHelper.Program`
- allows unsafe blocks
- includes embedded low-level binary resources under `Pawn/`
- includes package references such as `System.Management`, `TaskScheduler`, `HidSharpCore`, `NAudio.Wasapi`, `NvAPIWrapper.Net`, and `WinForms.DataVisualization`
- contains build/publish targets that can interact with a running G-Helper process or package output

## License And Attribution Notes

G-Helper is licensed under GPL-3.0 based on the reference repository `LICENSE` file.

If VictusControl is based directly on G-Helper source, VictusControl should be treated as a GPL-3.0 derivative unless a separate licensing decision or permission is obtained. A direct import would require preserving license notices, attribution, source availability, and clear modification history.

This should be decided before any G-Helper source is imported. The current VictusControl docs and code should not be mixed with G-Helper code casually.

## VictusControl Files That Must Be Preserved

Preserve the current project identity and AI workflow layer:

- `AGENTS.md`
- `AI_CONTEXT.md`
- `SESSION_STATE.md`
- `TOKEN_STRATEGY.md`
- `CONTEXT_RECIPES.md`
- `REFERENCE_POLICY.md`
- `REFERENCE_SOURCES.md`
- `.repomixignore`
- `repomix.config.json`
- `.github/copilot-instructions.md`
- `.github/prompts/`
- `docs/`

Preserve current source and test work unless an explicit reset/replacement strategy is approved:

- `VictusControl.sln`
- `src/`
- `tests/`
- `.gitignore`

## VictusControl Files That May Conflict With A G-Helper Import

Likely conflicts or decisions:

- `VictusControl.sln` vs `app/GHelper.sln`
- current layered `src/` and `tests/` layout vs G-Helper's single `app/` layout
- `.github/` workflow/instruction files
- `docs/` project docs vs G-Helper documentation assets
- `.gitignore`
- application identity: `VictusControl` namespaces/project names vs `GHelper` assembly/project names
- UI framework direction: current WPF shell vs G-Helper WinForms app
- target framework: current .NET 8 vs G-Helper `net10.0-windows`

These conflicts do not block an import, but they mean the import should be done in a dedicated branch or separate experimental task, not mixed into ordinary milestone work.

## G-Helper Folders/Files That Look Safe To Import Later

Potential import candidates after license approval and a dedicated import plan:

- `app/GHelper.sln`
- `app/GHelper.csproj`
- `app/Program.cs`
- `app/App.config`
- `app/app.manifest`
- `app/.editorconfig`
- selected non-generated app infrastructure files after targeted review
- selected UI files only if the project chooses to adopt WinForms instead of preserving the current WPF direction

"Safe to import later" means structurally plausible, not legally or technically approved. Every imported file would still need attribution, renaming, and ASUS-removal review.

## G-Helper Folders/Files To Exclude From Import

Exclude by default:

- `.git/`
- build outputs such as `bin/`, `obj/`, logs, and generated artifacts
- `app/Pawn/` embedded binary resources
- ASUS-specific control files such as `app/AsusACPI.cs`, ASUS keyboard/mouse settings, and ASUS peripheral/control modules
- generated WinForms files such as `*.Designer.cs` and `*.resx` until the UI import strategy is explicit
- `app/Resources/` and image/icon assets unless licensing and branding are reviewed
- `app/Ally/`, `app/AnimeMatrix/`, `app/Peripherals/`, `app/USB/`, and other ASUS/ROG-specific folders unless a later targeted review says otherwise
- build/publish targets that kill running apps, package executables, or assume G-Helper naming

## Immediate Risks

- GPL-3.0 licensing may require VictusControl to adopt compatible licensing if source is imported.
- G-Helper is ASUS-specific at its core, so a direct import may carry hardware assumptions deep into startup, UI, settings, and control flows.
- Importing generated WinForms UI files could create a large, noisy diff and high future token cost.
- Current VictusControl layered architecture conflicts with G-Helper's compact single-app structure.
- G-Helper targets .NET 10 while VictusControl currently targets .NET 8.
- Embedded binaries, unsafe blocks, and low-level helper resources must not be imported casually.

## Recommended Next Tiny Step

Before copying anything, make a licensing and structure decision:

- decide whether VictusControl is willing to become GPL-3.0-compatible as a G-Helper-derived project
- decide whether the next experiment should happen in a separate branch or disposable worktree
- create a tiny import mapping document that compares only G-Helper root/app project files against the current VictusControl root

Do not import source files until that decision is explicit.
