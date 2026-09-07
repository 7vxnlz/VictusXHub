# VictusX Icon Asset Requirements

## Ownership and Licensing

- The future VictusX icon must be original work or explicitly licensed for redistribution with the HP diagnostic preview.
- The source, author, license terms, and attribution requirements must be recorded before the asset is added.
- If generated with a tool or commissioned, keep the prompt/brief, license, and acceptance approval with the release records.
- Do not modify repository license terms as part of icon replacement.

## Required Sizes and Formats

- Provide a multi-resolution Windows `.ico` for executable, Explorer, taskbar, and shortcut display.
- Required sizes: 16x16, 20x20, 24x24, 32x32, 40x40, 48x48, and 256x256 where the design supports them cleanly.
- Keep a reviewed source master outside the compiled `.ico`, preferably vector or high-resolution raster, so future icons can be regenerated consistently.
- Verify high-DPI scaling and dark/light Windows taskbar visibility.

## Visual Identity Constraints

- The icon should communicate VictusX as a read-only HP diagnostic preview, not hardware control.
- It should be distinct from inherited G-Helper identity and from vendor/product marks.
- Avoid visual language that implies fan speed control, performance tuning, RGB control, BIOS changes, or HP endorsement.
- Keep the shape readable at tray size without depending on small text.

## Prohibited Source Material

- Do not copy the G-Helper blue G mark or inherited upstream icon shapes.
- Do not copy HP, OMEN, Victus, ASUS, ROG, Armoury Crate, or Windows trademarks unless explicit permission is documented.
- Do not derive from screenshots, package icons, firmware assets, or vendor marketing images.

## Accessibility and Readability

- Must be recognizable at 16x16 and 24x24 tray sizes.
- Must retain contrast on light and dark taskbars.
- Must avoid thin strokes that disappear under scaling.
- Must not rely on color alone to distinguish status.
- Must be visually calm enough for a diagnostic utility and not look like a warning/error badge by default.

## Acceptance Checklist Before Asset Replacement

- Ownership/licensing evidence recorded.
- Attribution requirements, if any, added to notice/release checklist.
- Multi-resolution `.ico` inspected at all required sizes.
- Explorer, taskbar, tray, shortcut, and window icon behavior verified from a package candidate.
- HP diagnostic mode uses the VictusX icon without changing default ASUS/G-Helper mode behavior.
- No `--hp-wmi-readonly-test`, fan write, performance write, EC, or BIOS write behavior is added.
- Release blocker checklist updated after verification.

## Files and Resources Likely Affected Later

- `app/favicon.ico` for executable identity.
- `app/VictusX.csproj` `ApplicationIcon` metadata.
- `app/Properties/Resources.resx` and generated designer if an HP-mode tray resource is added.
- `app/Resources/*.ico` only for a reviewed HP-mode-specific resource; shared inherited resources should remain unchanged unless a separate default-mode branding task approves it.
- `app/Program.cs` for an `--hp-victus`-only tray icon selection.
- Future shortcut/launcher/package metadata for the preview artifact.

See [VictusX Icon and App Identity Implementation Plan](victusx-icon-app-identity-implementation-plan.md) for the concrete source integration and verification sequence after an approved icon asset exists.

## Release Blocker Status

Icon/app identity remains a preview release blocker. No icon, image, or binary asset has been created, generated, replaced, or modified by this requirements document.

## Recommended Next Safe Task

Create or approve an original VictusX diagnostic icon asset with documented license/provenance and size coverage. Do not integrate it until the asset satisfies this checklist.
