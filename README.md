# VictusX

A G-Helper-based control utility being adapted for HP Victus laptops.

> [!WARNING]
> **Project status: Experimental / early development.**
> VictusX is not production-ready HP hardware-control software. Current development is focused on HP Victus detection, diagnostics, and guarded read-only telemetry.

SetFanMax write work is design-only and currently **NO-GO**. Fan control, fan writes, and fan-control UI are not implemented.

## What is VictusX?

VictusX is a Windows utility project based on [G-Helper](https://github.com/seerge/g-helper), currently being adapted for HP Victus laptops.

The current work is about replacing ASUS-specific hardware-control paths with HP Victus-safe abstractions. HP detection and guarded read-only WMI diagnostics are implemented, but real HP hardware control is not ready yet.

Target development hardware:

- HP Victus Gaming Laptop 16-s0035nt / 7Z5Z2EA

## Current Experimental Status

Implemented:

- G-Helper source imported as the project base
- VictusX project shell
- HP Victus startup mode
- Safe unsupported hardware mode
- HP capability report
- HP WMI availability probe
- HP WMI invocation sandbox
- HP Victus detection
- HP WMI/CIM readiness diagnostics
- `SystemDesignData` read-only decode
- `FanGetCount` read-only decode
- `FanMaxGet` read-only decode
- `FanGetLevel` raw-only decode
- Access denied diagnostics

Not implemented yet:

- Fan control
- Fan writes
- Fan-control UI
- Performance mode control
- Battery charge limit control
- RGB / keyboard lighting control
- GPU mode control
- Production HP hardware control

Deferred or design-only:

- `FanGetRpm` and `GetFanType` are deferred pending stronger device-specific evidence.
- `SetFanMax` is NO-GO/design-only; no SetFanMax execution path exists.

## Safety model

VictusX is being developed conservatively.

- Default ASUS/G-Helper behavior is preserved.
- HP work is behind explicit developer flags.
- Normal `--hp-victus` mode is non-invoking for explicit HP WMI telemetry commands.
- Explicit HP WMI read-only tests require developer flags and an elevated Administrator process.
- Real HP WMI read-only invocation requires both:

```bash
--hp-victus
--hp-wmi-readonly-test
```

No fan writes, EC writes, BIOS setting writes, thermal writes, power writes, RGB writes, keyboard-lighting writes, battery writes, or production HP control paths are implemented.

Current HP read-only telemetry notes are consolidated in [docs/hp-readonly-telemetry-status.md](docs/hp-readonly-telemetry-status.md). These are diagnostics milestones only and do not indicate hardware control support.

The HP Diagnostic dashboard is read-only and report-backed; it does not refresh hardware or provide fan or performance control.

## Usage for current development builds

Build the solution:

```bash
dotnet build VictusX.sln
```

Run the current safe HP Victus probing mode:

```bash
dotnet run --project app\VictusX.csproj -- --hp-victus
```

For IDE profile usage, local report paths, and safety notes, see the [VictusX HP Diagnostic run guide](docs/victusx-hp-diagnostic-run-guide.md).

The future preview profile and launcher contract are documented in the [HP Diagnostic preview artifact checklist](docs/hp-diagnostic-preview-artifact-checklist.md); no preview artifact has been published.

User-facing safety notes for a future HP diagnostic preview are documented in [docs/hp-diagnostic-preview-user-safety-notes.md](docs/hp-diagnostic-preview-user-safety-notes.md).

The `--hp-wmi-readonly-test` flag is only for controlled developer testing. Do not use it unless you are intentionally testing the guarded HP WMI invocation path and understand the current limitations.

HP WMI real invocation tests are developer-only and may require running VictusX from an elevated Administrator terminal. The normal safe command remains:

```bash
dotnet run --project app\VictusX.csproj -- --hp-victus
```

### Developer-only elevated HP WMI test

The guarded HP WMI read-only invocation path is for controlled developer testing only. It may require running from an elevated Administrator terminal, and it is documented separately in [docs/elevated-hp-wmi-readonly-test-guide.md](docs/elevated-hp-wmi-readonly-test-guide.md).

Do not use the elevated test path for normal development runs. The normal recommended command remains:

```bash
dotnet run --project app\VictusX.csproj -- --hp-victus
```

## Development roadmap

- HP WMI/CIM investigation
- HP command catalog validation
- Read-only telemetry
- Safe HP hardware-controller implementation
- UI cleanup and rebranding
- Packaging and release work later

## Credits

VictusX builds on the excellent work of:

- [G-Helper](https://github.com/seerge/g-helper) by seerge, used as the original application base
- [ghelper-omen](https://github.com/ib-3/ghelper-omen), used as an HP/Omen research reference
- [OmenCore](https://github.com/theantipopau/omencore), used as an HP/Omen research reference
- [OmenSuperHub](https://github.com/breadeding/OmenSuperHub), used as an HP/Omen research reference
- [OmenXHub](https://github.com/MasonDye/OmenXHub), used as an HP/Omen research reference

VictusX is independent and is not affiliated with, authorized by, or endorsed by HP Inc., ASUS, or the original G-Helper project.

## Disclaimer

This is experimental software. Use at your own risk.

Hardware-control development can affect system behavior if implemented incorrectly. VictusX currently avoids production HP hardware-control writes while the HP Victus support layer is being investigated.
