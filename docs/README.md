# VictusXHub - Lightweight control utility for HP Victus laptops

[![Experimental](https://img.shields.io/badge/status-experimental-orange)](https://github.com/7vxnlz/VictusXHub)
[![Windows](https://img.shields.io/badge/platform-Windows-0078D4)](https://www.microsoft.com/windows)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)](https://dotnet.microsoft.com/)
[![License: GPL-3.0](https://img.shields.io/badge/license-GPL--3.0-blue)](../LICENSE)

VictusXHub is an open-source Windows utility for HP Victus laptops, originally based on the excellent [G-Helper](https://github.com/seerge/g-helper) project. It aims to provide lightweight hardware monitoring and carefully validated control without requiring OMEN Gaming Hub. The project is experimental and currently focuses on safe, read-only diagnostics for a single validated Victus device.

> [!WARNING]
> VictusXHub is under active development and is currently validated primarily on the HP Victus 16-s0035nt / `7Z5Z2EA`. Hardware-control features that are not independently validated remain disabled.

## Current status

| Feature | Status |
| --- | --- |
| HP Victus detection | Working |
| Read-only Diagnostic dashboard | Working |
| CPU temperature | Working when PawnIO access is available |
| GPU temperature | Working when NVAPI is available |
| Display refresh rate | Working |
| SystemDesignData / Thermal Policy | Working |
| Fan count | Working |
| Fan RPM | Unavailable |
| Performance Mode | Unavailable |
| GPU Switching | Unavailable |
| Keyboard Lighting state | Unavailable |
| Battery Care | Unavailable |
| Fan Control | Blocked / not validated |

Normal HP startup may collect the approved read-only SystemDesignData and FanGetCount discovery data. Opening Diagnostic does not issue additional HP WMI requests. FanGetLevel remains raw-only and is never presented as RPM or percent.

## Supported hardware

Current physical validation is centered on:

- HP Victus Gaming Laptop 16-s0035nt
- SKU `7Z5Z2EA` / `7Z5Z2EA#AB8`
- BIOS `F.31`; board `8BD4` evidence where noted

Broader HP Victus support is a future goal, not a current compatibility claim.

## Build and run

There is no stable public release yet. For a development build:

```powershell
dotnet build VictusXHub.sln
dotnet run --project app\VictusXHub.csproj -- --hp-victus
```

## Documentation

- [HP Diagnostic run guide](victusx-hp-diagnostic-run-guide.md)
- [Read-only telemetry status](hp-readonly-telemetry-status.md)
- [HP safety notes](hp-diagnostic-preview-user-safety-notes.md)
- [Third-party notices](../THIRD-PARTY-NOTICES.md) and [GPL-3.0 license](../LICENSE)

## Credits

VictusXHub is based on and inspired by [G-Helper](https://github.com/seerge/g-helper) by seerge. The project also uses targeted HP/Omen research references recorded in its development documentation. [PawnIO](https://github.com/namazso/PawnIO) is used only for the validated read-only CPU-temperature path.

VictusXHub is not affiliated with or endorsed by HP, ASUS, or G-Helper.

## Disclaimer

This is experimental software. Hardware features remain unavailable until they are independently validated for the target device.
