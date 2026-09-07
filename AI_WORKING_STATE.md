# AI Working State

Updated: 2026-09-07

## Current Truth

- Product: VictusX, a .NET 10 Windows utility currently targeting HP Victus 16-s0035nt / SKU `7Z5Z2EA#AB8` / BIOS `F.31` / Thermal Policy V1.
- HP mode uses the compact inherited shell. Unsupported controls are visible but disabled/read-only. The HP tray has explicit Open VictusX and Open Diagnostic actions; Diagnostic uses one read-only owned side panel and activates the existing instance when already open.
- Status currently includes CPU load, battery/AC/charging, device detection, NVIDIA GPU temperature when NVAPI is available, named HP battery-care setting status when the read-only Instrumented BIOS inventory is accessible, exact-SKU keyboard-backlight capability evidence, and exact-device GPU-switch capability from cached SystemDesignData. The HP tray shows a compact disabled CPU/GPU/battery/screen summary from the existing formatted telemetry and refreshes it through the existing polling cycle/menu-open refresh. HP mode provides Windows-native refresh-rate actions in the main shell and tray only when one hardware-internal panel is identified and rates match its current resolution/bit depth/orientation; both routes share one validated apply path and have no vendor write path. Keyboard state/levels, numeric charge limits, and current GPU mode remain unavailable.
- CPU package temperature and V1 fan RPM remain unavailable. `FanGetLevel` is raw-only and must never be shown as RPM or percent. `FanMaxGet` is inconclusive.
- Developer-only four-byte SetFanMax pulse/hold is operational behind explicit CLI gates. Hold seconds mean bounded pre-restore wait, not physical fan-duration control.
- SetFanLevel percentage dry-run and first-write preflight are hardware/WMI-inert. First-write readiness is NO-GO; no value is selected.
- `DeviceValidatedInputLength` is null/unset. Normal/user-facing fan control is NO-GO.
- Source-only release preparation is GO. Preview publishing remains NO-GO pending remaining notice/package compliance, signing/checksums, clean-machine validation, and final artifact inspection.
- The isolated `Microsoft.Management.Infrastructure`/`CimSession` readiness probe and package graph were removed. HP namespace/class/method readiness remains read-only through the existing `System.Management` WMI path; the `Runtime.Win` distribution blocker is resolved at source/restore-graph level pending final artifact confirmation.
- Four valid seven-frame VictusX ICO assets are present under `app/Assets`. `VictusX.ico` is the fixed executable/window/taskbar identity. HP performance mode is unavailable, controls remain disabled, and the tray uses the base icon, not saved inherited configuration. Variant routing is retained for future verified state. Non-HP behavior is unchanged. See `docs/hp-performance-mode-evidence.md`: close F.30/V1 references do not validate F.31 switching/readback/recovery. Final package hash matching and ownership/license confirmation remain pending release evidence.
- Localized `Strings*.resx` display values have been cleared of inherited G-Helper product wording. Compatibility resource keys and internal identifiers remain unchanged.
- Runtime package-library license and NOTICE texts are assembled under `app/Assets/Licenses` from recorded authoritative sources and wired as external publish content. The NvAPIWrapper LGPL source architecture is resolved by excluding `NvAPIWrapper.dll` from the single-file bundle as a replaceable sidecar; release-candidate layout verification remains pending. Self-contained .NET runtime notice matching remains open.
- The HP preview profile is source-configured for Release `win-x64`, self-contained single-file, untrimmed and symbol-free output. It packages `LICENSE`, notices, the safe `--hp-victus` launcher, and the replaceable NvAPIWrapper assembly externally.
- First preview publish source readiness remains NO-GO until the final icon exists and `THIRD-PARTY-NOTICES.md` is promoted from source-assembled status after runtime-pack, external NvAPIWrapper layout, icon, and final artifact checks.
- `docs/preview-release-current-blockers.md` is the concise current snapshot for preview release blockers; deeper evidence remains in the packaging docs.

## Verification Baseline

- Branch: `main` tracking `origin/main`.
- Last verified: `dotnet build VictusX.sln` passed with zero warnings and `dotnet test VictusX.sln --no-build` passed 405/405 after adding HP tray quick actions and single-instance Diagnostic activation. No hardware or real display-mode command was run. Prior built-output inspection found the associated executable icon and all four expected embedded VictusX icon resources.
- NU1900 is not suppressed. Current-source audit evidence is cleared: a network-capable restore retrieved official NuGet vulnerability data, both project graphs reported no vulnerable packages, and required restore/build/test completed without NU1900. Repeat on the exact release candidate.

## Context Routing

Load one domain pack from `docs/context-packs/`. Use `docs/reference-index.md` only for reference comparisons. Use historical decision documents only when the task touches that decision.
