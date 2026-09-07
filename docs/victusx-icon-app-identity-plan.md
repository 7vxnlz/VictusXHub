# VictusX Icon and App Identity Plan

## Current Status

- `app/VictusX.csproj` embeds `app/favicon.ico` as the executable icon. Packaging audit work identifies it as the inherited blue G icon.
- The project and manifest identity are already `VictusX` / `VictusX.app`; product, read-only description, contributor attribution, and preview version metadata are explicit.
- In `--hp-victus` mode, the tray tooltip is `VictusX` and the visible diagnostic title is `VictusX Read-only Diagnostic`.
- The tray icon still uses `Properties.Resources.standard`, an inherited shared icon resource. No clearly named, reviewed VictusX icon asset was found.

## Current Identity References

| Surface | Current reference | Future HP diagnostic treatment |
| --- | --- | --- |
| Executable / Explorer | `ApplicationIcon` -> `app/favicon.ico` | Use an approved VictusX preview icon through the HP preview publish configuration. |
| Tray | `Program.cs` -> `Properties.Resources.standard` | Add an `--hp-victus`-only VictusX tray icon selection. |
| Windows forms | Executable-associated icon and `UI/IconHelper.cs` | Verify large and small window icons after the new executable and tray resources exist. |
| Shared resources | `standard.ico`, `ultimate.ico`, `dark-*.ico`, `light-*.ico` | Keep unchanged until a separately approved default-mode branding effort. |
| Text metadata | project properties and `app.manifest` | Keep the existing VictusX metadata aligned with the preview artifact version. |

## New Icon Requirements

See [VictusX Icon Asset Requirements](victusx-icon-asset-requirements.md) for the detailed ownership, format, accessibility, and acceptance checklist before any asset is created or replaced.

- Original or explicitly licensed for VictusX distribution, with documented provenance and terms.
- No G-Helper blue G mark, ASUS/ROG/Armoury imagery, HP, OMEN, Victus, or Windows trademarks unless permission is established.
- Legible at Windows tray and Explorer sizes; supply a multi-resolution `.ico` suitable for 16, 20, 24, 32, 40, 48, and 256 pixel use where the source design supports those sizes.
- Neutral diagnostic identity: it must not imply fan, performance, or hardware-control support.
- Match the existing read-only wording in metadata and UI; do not present the preview as HP-endorsed.

## Future Integration Plan

See [VictusX Icon and App Identity Implementation Plan](victusx-icon-app-identity-implementation-plan.md) for the implementation sequence, source integration points, verification steps, and rollback plan.

1. Approve the icon source, license/provenance record, and required resolutions before adding any asset.
2. Add the new `.ico` and an HP-specific resource entry without replacing the shared inherited resources.
3. Override `ApplicationIcon` only in the HP diagnostic preview publish configuration so the default build remains visually unchanged.
4. Add a small `--hp-victus` conditional for the tray icon; preserve the current resource selection outside HP mode.
5. Verify Explorer, taskbar, tray, large/small form icons, window title, shortcut icon, and high-DPI rendering from a packaged test build.
6. Update packaging notices if the icon has attribution or license requirements, then re-run the clean-machine preview checklist.

## What Must Remain Untouched for Now

- `RootNamespace`, startup object, shared G-Helper resource names, and inherited default-mode icon selection.
- Default ASUS/G-Helper window, tray, update, and control behavior outside `--hp-victus`.
- Any logo or brand asset without a documented right to use it.

## Preview Release Status

The inherited executable and tray identity remain a release blocker. Replacement must satisfy the asset requirements checklist before integration. Replacement is not sufficient by itself: dependency notice review, signing/checksums, and clean-machine packaged validation must also pass. No image, icon, or binary asset changed as part of this plan.

## Recommended Next Safe Task

Review and approve an original, license-documented multi-resolution VictusX diagnostic icon, then perform a separate asset-only integration task using the implementation plan.
