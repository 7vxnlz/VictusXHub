# G-Helper File-Level Import Map

This map classifies G-Helper files and folders for a possible future direct import into VictusControl. It uses only file names, folder names, project metadata, license metadata, and prior import assessment notes. No G-Helper files were copied.

Reference source: `D:\Projects\Workspace\references\g-helper` at `5c26f5ac970dab9e26347d80976ebf1eece91b1e`.

## Import Candidate

- `app/.editorconfig` - small formatting metadata; safe to compare before import.
- `app\App.config` - small app-level configuration metadata; inspect before deciding.
- `app\app.manifest` - likely needed for Windows desktop app behavior; inspect and rebrand before use.
- `app\GHelper.sln` - useful as a source-base solution reference if importing into an isolated branch/location.
- `app\GHelper.csproj` - useful as the main import metadata source, after unsafe targets/resources are removed or disabled.

## Exclude From Import

- `.git/` - reference repository history must not be copied into VictusControl.
- G-Helper build outputs, logs, `bin/`, and `obj/` if present - generated artifacts.
- `app\Pawn/` - embedded low-level binary resources; do not import casually.
- `app\favicon.ico` - branding asset; not appropriate for VictusControl without explicit asset/licensing review.
- `app\Resources/` - resource and asset folder; likely noisy and branding-sensitive.
- generated WinForms files such as `*.Designer.cs` and `*.resx` - large/noisy until UI import strategy is explicit.
- `GHelper.csproj` build/publish targets `KillRunningGHelper` and `ZipSingleExe` - assume G-Helper process names and perform build-time side effects.

## Import Later After Review

- `app\Program.cs` - startup shape may be useful, but requires source review and rebrand.
- `app\AppConfig.cs` - settings shape may be useful, but likely intertwined with ASUS options.
- `app\Settings.cs` - UI/settings behavior may be useful only after choosing WinForms strategy.
- `app\Helpers/` - likely utility code; inspect individual files later.
- `app\UI/` - possible UI helper layer; inspect later if keeping WinForms.
- `app\Battery/` - possible app feature reference; inspect later for vendor assumptions.
- `app\Display/` - possible app feature reference; inspect later for hardware/API assumptions.
- `app\Input/` - possible hotkey/input reference; inspect later.
- `app\AutoUpdate/` and `app\UpdatesController.cs` - update flow reference only after release strategy exists.

## Conflict With Existing VictusControl File

- `app\GHelper.sln` conflicts conceptually with `VictusControl.sln`.
- `app\GHelper.csproj` conflicts conceptually with current `src\VictusControl.App\VictusControl.App.csproj`.
- `.github/` conflicts with VictusControl AI workflow and future repository automation.
- `docs/` conflicts with VictusControl docs and G-Helper-specific README/assets.
- `.gitignore` overlaps with VictusControl `.gitignore`; merge manually later if needed.
- `app\.editorconfig` may conflict with current formatting defaults if added at root.

## Requires Rename/Rebrand

- `app\GHelper.sln` - project/solution identity.
- `app\GHelper.csproj` - assembly name, startup object, icon, target names, and project name.
- `app\Program.cs` - namespace/startup identity if imported later.
- `app\app.manifest` - application identity and requested behavior must be reviewed.
- `app\favicon.ico` - G-Helper branding; exclude unless replaced.
- any namespace, assembly, resource, mutex, event, or process name containing `GHelper`, `G-Helper`, `Asus`, `ROG`, or `Armoury`.

## Requires License/Attribution Preservation

- `LICENSE` - GPL-3.0 license text must be preserved if G-Helper code is imported.
- `docs\README.md` - project attribution/reference text may be useful when drafting notices, but do not copy marketing/docs wholesale.
- any imported G-Helper source file - requires GPL-3.0-compatible licensing posture and clear modification history.
- `app\GHelper.csproj` package/resource metadata - preserve attribution and review third-party package licenses before use.

## Likely ASUS-Specific Candidate, Inspect Later

- `app\AsusACPI.cs` - ASUS hardware access.
- `app\AsusKeyboardSettings.cs` - ASUS keyboard behavior.
- `app\AsusMouseSettings.cs` - ASUS peripheral behavior.
- `app\Ally/` - ASUS ROG Ally-specific.
- `app\AnimeMatrix/` - ASUS lighting feature.
- `app\Peripherals/` - likely ASUS accessory-specific.
- `app\USB/` - likely ASUS device/peripheral-specific.
- `app\Fan/`, `app\Fans.cs`, `app\Mode/`, `app\HardwareControl.cs` - likely important but probably ASUS-entangled; inspect later, do not import first.
- `app\Gpu/`, `app\Overlay/`, `app\Handheld.cs`, `app\Matrix.cs`, `app\Slash.cs`, `app\Extra.cs` - possible useful behavior mixed with ASUS-specific assumptions.

## Unknown, Do Not Touch Yet

- `crowdin.yml` - localization workflow; irrelevant until localization strategy exists.
- `.github/` - inspect later only if CI/release workflow is being rebuilt.
- `app\Properties/` - generated settings/resources/localization; do not import until UI/resource strategy is explicit.
- top-level `docs/` media and localized docs - avoid until attribution and documentation strategy are settled.
- any G-Helper file not listed here - leave untouched until a focused import task names it.

## Existing VictusControl `src/` And `tests/`

Keep the current `src/` and `tests/` skeleton for now. It is small, tested, VictusControl-specific, and useful as a safety net. Do not delete or replace it in the first import commit.

If the project chooses a direct G-Helper base, evaluate removal or archival of the skeleton only in a dedicated branch and commit after the imported base is understood.

## AI Workflow Files To Preserve

Preserve:

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
- existing `docs/` planning and reference-tracking documents

## GPL/Attribution Files Likely Required

- `LICENSE` - required if G-Helper code is imported.
- a future attribution notice or section in VictusControl docs - should state that the imported base derives from G-Helper and identify the source commit.
- relevant third-party license notices from `app\GHelper.csproj` package references - review before source import.

## Smallest Safe First Import Commit

Smallest safe first import commit: add only licensing/attribution scaffolding and an isolated copy of the G-Helper build metadata after explicit user approval.

Candidate contents after approval:

- GPL-3.0 license decision/notice updates
- source commit attribution note
- isolated imported metadata files only, with unsafe build targets disabled before any build is attempted

Do not include application source in that first commit.

## First Import Commit Out Of Scope

Keep these out of scope:

- copying G-Helper application source files
- deleting VictusControl `src/` or `tests/`
- importing `Pawn/`, binaries, generated UI files, resources, or icons
- ASUS-removal planning beyond obvious exclusions
- HP WMI, fan control, telemetry, EC access, BIOS writes, or hardware write logic
- running builds, installers, scripts, setup commands, or application code

## Next Codex Task

Recommended next task:

Ask the user to confirm the GPL-3.0-compatible direction. If confirmed, create a tiny license/attribution preparation document or commit plan before copying any G-Helper files.
