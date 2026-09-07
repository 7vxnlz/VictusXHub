# G-Helper Import Strategy

This document decides how VictusControl should proceed if it is directly based on G-Helper. It is not an import plan and does not authorize copying source files yet.

## Licensing Decision

Using G-Helper as the direct source base means VictusControl should be treated as a G-Helper-derived project. G-Helper is GPL-3.0, so importing its code implies VictusControl must remain GPL-3.0-compatible unless explicit alternate permission is obtained from the relevant copyright holders.

Decision: if G-Helper code is imported, VictusControl should adopt a GPL-3.0-compatible licensing posture, preserve attribution, keep source available, and clearly mark the project as modified from G-Helper where appropriate.

## Files To Preserve Before Import

Preserve the VictusControl project identity and workflow layer:

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

Preserve the current `src/`, `tests/`, and `VictusControl.sln` until the import strategy explicitly decides whether G-Helper will replace them or live in a separate experiment.

## Current Skeleton Decision

Do not remove the current clean skeleton under `src/` and `tests/` as part of the first import step.

The skeleton is small, tested, and contains useful VictusControl-specific contracts. If the project later chooses a direct G-Helper base, remove or archive the skeleton only in a dedicated replacement commit after the first import experiment proves the imported base can be renamed, licensed, and built safely.

## Commit Strategy

Do not import G-Helper in one large mixed commit.

Use staged commits:

1. record license/attribution decision
2. import the smallest build metadata subset into an isolated location or branch
3. rename project identity
4. remove or disable unsafe build targets and bundled binaries
5. remove ASUS-specific code in later focused passes

Each step should be reviewable in GitHub Desktop.

## Smallest Safe First Import Step

The smallest safe first import step is not copying application code. It is a tiny import mapping task that checks only:

- current VictusControl root files
- `D:\Projects\Workspace\references\g-helper\LICENSE`
- `D:\Projects\Workspace\references\g-helper\app\GHelper.sln`
- `D:\Projects\Workspace\references\g-helper\app\GHelper.csproj`
- `D:\Projects\Workspace\references\g-helper\app\app.manifest`
- `D:\Projects\Workspace\references\g-helper\app\App.config`
- `D:\Projects\Workspace\references\g-helper\app\.editorconfig`

The output should be a short file-level import map: preserve, import later, exclude, or requires decision.

## First Import Step Out Of Scope

Keep these out of scope:

- copying G-Helper source files
- deleting VictusControl `src/` or `tests/`
- ASUS-removal planning beyond obvious file-level exclusions
- HP WMI, fan control, telemetry, EC access, BIOS writes, or hardware write logic
- importing `Pawn/`, binaries, icons, resources, generated WinForms files, or build/publish targets
- running any build, app, installer, script, or setup command

## Files To Check Before Copying Anything

Before any source import, check:

- G-Helper `LICENSE`
- G-Helper `app/GHelper.csproj`
- G-Helper `app/GHelper.sln`
- G-Helper `app/app.manifest`
- G-Helper `app/App.config`
- G-Helper `.gitignore`
- VictusControl `.gitignore`
- VictusControl AI workflow docs
- VictusControl `src/` and `tests/` status

## Next Codex Task

Recommended next task:

Create a token-safe G-Helper import map that inspects only G-Helper license, solution, project, manifest, app config, editorconfig, and immediate `app/` top-level filenames. Do not copy files. Do not delete the current skeleton. Classify each candidate as preserve, import later, exclude, or needs user decision.
