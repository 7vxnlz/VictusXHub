# AI Working State

Updated: 2026-09-08

## Canonical Current Truth

- Product/device: VictusX is a .NET 10 Windows utility targeting the exact validated HP Victus 16-s0035nt, SKU `7Z5Z2EA#AB8`, BIOS `F.31`, Thermal Policy V1. Never generalize HP support across models, BIOS versions, or policy generations.
- HP mode uses the compact inherited shell. Unsupported controls stay visible but disabled/read-only. Diagnostic is a read-only owned side panel with grouped Device, Live status, and Capabilities summaries; Advanced retains source/blocker and fan-research evidence, while copy/export includes both layers. Repeated activation reuses the existing instance.
- Verified read-only status includes CPU load, battery/AC/charging, device detection, NVIDIA temperature when NVAPI is available, named HP battery-care support and enabled/disabled state when the recognized inventory value is explicit, exact-SKU keyboard-backlight capability evidence, and exact-device GPU-switch capability from cached SystemDesignData. Main-shell and tray refresh-rate actions use one Windows-native validated path and appear only for one unambiguous internal panel with matching mode geometry; there is no vendor write path.
- Unavailable: CPU package temperature, V1 fan RPM, keyboard state/levels, numeric charge limits, and current GPU mode. `FanGetLevel` is raw-only and is never RPM or percent; `FanMaxGet` is inconclusive. HP performance mode is unavailable and controls remain disabled.
- Normal/user-facing fan control is NO-GO. `DeviceValidatedInputLength` remains null. Developer SetFanMax pulse/hold remains CLI-only, explicitly gated, four-byte, no-retry, and restore-protected; hold seconds are only the bounded pre-restore wait. SetFanLevel remains non-executable research: dry-run/preflight is hardware/WMI-inert, first-write readiness is NO-GO, and no value is selected.
- Release status: source-only preparation and the fail-closed preview-directory inspection workflow are GO. The previous .NET 10.0.10 runtime-pack baseline was superseded after servicing advisories; the active `win-x64` restore baseline is .NET Core and Windows Desktop runtime packs 10.0.11 via SDK 10.0.400. The final package notice/attribution review is complete for the current preview baseline: project GPL, G-Helper attribution, eight application-package entries, exact runtime notices, NvAPIWrapper sidecar/notices, and icon provenance are internally consistent and inspector-verified. The maintainer explicitly approved the initial preview as unsigned because no approved code-signing certificate is configured; release material must warn that Windows may show an unknown or unverified publisher. Preview publishing remains NO-GO pending the final distributable checksum and clean-machine validation. See `docs/preview-release-current-blockers.md` and the `packaging` context pack.
- Non-HP behavior is unchanged.

## Verification Baseline

- Branch: `main` tracking `origin/main`.
- Last verified: with SDK 10.0.400 and the 10.0.11 `win-x64` .NET Core/Windows Desktop runtime-pack baseline, `dotnet build VictusX.sln --no-restore` passed with zero warnings; `dotnet test VictusX.sln --no-build` passed 439/439. No application, hardware, vendor-write, or real display-mode command was run.
- NU1900 is not suppressed. The last network-capable audit found no vulnerable packages; repeat against the exact release candidate.

## Context Routing

Use one route from `docs/context-packs/` only when it helps the task: `fan-research`, `telemetry`, `ui`, `packaging`, or `identity`. Search before reading selected implementation/tests. Historical evidence is demand-loaded. Reference comparisons require `REFERENCE_POLICY.md` and `docs/reference-index.md`.
