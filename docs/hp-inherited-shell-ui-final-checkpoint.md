# HP Inherited Shell UI Final Checkpoint

## Purpose

This checkpoint records the restored HP Victus mode UI state after manual visual confirmation. It is a source-only UI readiness note and does not authorize publishing, WMI probing, fan-control UI, or hardware writes.

## Manual Visual Confirmation

The HP mode shell has been manually confirmed to look normal and good after the inherited-shell restoration and caption-rendering fixes. The main window now preserves the compact G-Helper/Omen-style layout instead of opening as a custom diagnostic report surface.

Confirmed UI state:

- The inherited compact shell is restored.
- Footer labels and icons are readable.
- Disabled HP shell captions are aligned cleanly.
- Diagnostic opens from the footer as an owned side panel.
- Diagnostic content remains read-only and scrollable.

## Restored Inherited Shell

`--hp-victus` keeps the designer-owned inherited shell layout with the familiar performance, GPU, display, keyboard, and battery sections visible for orientation. Unsupported HP controls remain unavailable/read-only so the visual structure is preserved without exposing unsupported control behavior.

The HP-specific main-window additions are intentionally narrow:

- Unsupported inherited controls are disabled/read-only.
- Diagnostic is available as a footer action.
- Detailed HP diagnostic evidence is shown in the owned Diagnostic side panel.

## Diagnostic Footer And Side Panel

Diagnostic is a footer action alongside Thank You, Updates, and Quit. It follows the Updates-style side-panel behavior and does not replace the main shell.

The Diagnostic side panel contains read-only report-backed and local-file-backed details, including device identity, readiness state, SetFanMax evidence, pulse history, and proof-gap analysis. Long diagnostic content remains scrollable inside the panel.

## No-Control Guarantees

- No fan slider.
- No fan toggle.
- No pulse/run button.
- No normal fan control UI.
- No WMI invocation from the HP UI.
- No background fan writes.
- No `SetFanMode`, `SetFanLevel`, `0x37`, EC write, fan curve, or performance write path is exposed.

## Current Decisions

- Source-only release-prep: GO.
- Preview package publish: NO-GO.
- Normal/user-facing fan control: NO-GO.
- Developer-only 4-byte Max Fan Pulse: operational under explicit command-line gates only.
- `DeviceValidatedInputLength`: remains unset.

## Recommended Next Safe Task

Continue source-only preview preparation by designing the dedicated HP diagnostic publish profile and preview artifact contract, without publishing binaries or creating release artifacts.
