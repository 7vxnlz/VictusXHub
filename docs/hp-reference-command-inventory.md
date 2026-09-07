# HP Reference Command Inventory

Scope: targeted HP/Omen/Victus command inventory for future VictusX implementation. This is research only; no source code was copied and no reference repository was modified.

## 1. HP WMI / BIOS interface candidates

| Reference repo | File path | Short purpose | Risk | Read-only or write-capable | Use |
|---|---|---|---|---|---|
| ghelper-omen | `app/Omen/HpWmiBios.cs` | Main HP `root\wmi` / `hpqBIntM` BIOS command wrapper with command IDs, heartbeat, failure tracking, fan, GPU, keyboard, battery, and display methods. | High | Write-capable | Research only first; future implementation guidance after a read-only probe layer exists. |
| ghelper-omen | `app/Omen/IHpWmiBios.cs` | Narrow interface around HP WMI BIOS operations. | Medium | Mixed | Good future interface inspiration; do not copy directly while VictusX still has ASUS-shaped abstractions. |
| ghelper-omen | `app/Omen/WmiBiosMonitor.cs` | Read-oriented hardware monitor using HP WMI BIOS, ACPI thermal zones, NVAPI, and Windows counters. | Medium | Mostly read-only | Useful later for read-only telemetry design; avoid monitor loops for now. |
| OmenSuperHub | `OmenHardware.cs` | Raw HP BIOS WMI command map using HP OMEN SDK model helpers and `SendOmenBiosWmi`. | High | Write-capable | Research only; useful for command ID cross-checking. |
| OmenXHub | `OmenHardware.cs` | Refined raw HP BIOS WMI command map with cached WMI scope and notes about Victus_S fan-level path. | High | Write-capable | Most useful raw command reference after ghelper-omen. Research only. |

## 2. Fan control candidates

| Reference repo | File path | Short purpose | Risk | Read-only or write-capable | Use |
|---|---|---|---|---|---|
| ghelper-omen | `app/Omen/WmiFanController.cs` | Higher-level WMI fan controller with preset handling, command verification, keepalive/reapply behavior, and ineffective-command detection. | High | Write-capable | Research only until VictusX has capability checks, consent gates, and readback verification. |
| ghelper-omen | `app/Omen/FanController.cs` | Fan orchestration across WMI/EC/service backends. | High | Write-capable | Future architecture guidance only. |
| ghelper-omen | `app/Omen/EcFanControllerWrapper.cs` | EC fan fallback wrapper. | High | Write-capable | Avoid for initial HP work; EC writes are deferred. |
| ghelper-omen | `app/Omen/Services/FanService.cs` | Service-level fan workflow and presets. | High | Write-capable | Useful later for behavior ideas; not for first implementation. |
| OmenSuperHub | `OmenHardware.cs` | Fan count, fan level, fan table, fan type, fan max, and fan mode WMI commands. | High | Write-capable | Command cross-reference only. |
| OmenXHub | `OmenHardware.cs` | Fan command references including Victus_S `GetFanLevel` notes, direct RPM command, fan type/capability probing, max fan, and EC fallback warning. | High | Write-capable | Strongest raw fan command reference; research only. |
| OmenCore | `src/OmenCore.Linux/Commands/FanCommand.cs` | Linux CLI fan behavior, safety messaging, and fallback rules. | Medium | Write-capable on Linux | Use only for safety UX concepts, not Windows command implementation. |
| OmenCore | `src/OmenCore.Avalonia/Services/FanCurveService.cs` | UI/service fan curve abstraction and interpolation. | Medium | Write-capable through service | Later design inspiration only; do not add fan curves yet. |

## 3. Performance / thermal mode candidates

| Reference repo | File path | Short purpose | Risk | Read-only or write-capable | Use |
|---|---|---|---|---|---|
| ghelper-omen | `app/Omen/HpWmiBios.cs` | Thermal policy versions, fan/performance modes, CPU power limits, GPU power, GPU mode, TCC, and battery care operations. | High | Write-capable | Research only; split read-only capability checks from writes. |
| ghelper-omen | `app/Omen/GpuPowerController.cs` | GPU power control flow. | High | Write-capable | Future guidance only after capability/readback checks. |
| ghelper-omen | `app/Omen/MmioPowerLimitProvider.cs` | MMIO/power-limit path. | High | Write-capable | Avoid initially; requires low-level driver-like access. |
| ghelper-omen | `app/Omen/RyzenControl.cs`, `app/Omen/RyzenSmu.cs` | Ryzen/SMU tuning helpers. | High | Write-capable | Avoid for Victus first pass. |
| OmenSuperHub | `OmenHardware.cs` | Performance mode, CPU power limits, GPU power state, load-line, ICCMax, and graphics switching WMI commands. | High | Write-capable | Cross-check command areas only. |
| OmenXHub | `OmenHardware.cs` | Same command family with additional comments and cached WMI behavior. | High | Write-capable | Research only. |
| OmenCore | `src/OmenCore.Linux/Commands/PerformanceCommand.cs` | Capability-aware Linux performance command UX, readback warning, and hold behavior. | Medium | Write-capable on Linux | Good safety/verification guidance, not Windows implementation source. |

## 4. Keyboard / backlight candidates

| Reference repo | File path | Short purpose | Risk | Read-only or write-capable | Use |
|---|---|---|---|---|---|
| ghelper-omen | `app/Omen/HpWmiBios.cs` | Keyboard type, backlight, brightness, color table, animation, idle mode, and light bar WMI commands. | High | Write-capable | Research only until read-only keyboard capability probing exists. |
| ghelper-omen | `app/Omen/Lighting/WmiLightingBackend.cs` | 4-zone WMI lighting backend. | High | Write-capable | Future implementation guidance only. |
| ghelper-omen | `app/Omen/OmenHidLightingBackend.cs` | HID lighting backend wrapper. | High | Write-capable | Research only; HID writes need separate safety gates. |
| ghelper-omen | `app/Omen/OmenLightingService.cs`, `app/Omen/OmenHidLightingService.cs` | Higher-level lighting services and HID lighting behavior. | High | Write-capable | Future guidance; too broad for first HP command implementation. |
| OmenSuperHub | `App/OmenLighting.cs` | HP SDK / WMI lighting methods, keyboard type classification, per-key support, and light bar command areas. | High | Write-capable | Research only; depends on proprietary HP SDK concepts. |
| OmenXHub | `App/OmenLighting.cs` | Lighting command model similar to SuperHub. | High | Write-capable | Research only. |
| OmenXHub | `Services/OmenLightingNative.cs` | Native `OmenLightingSDK.dll` P/Invoke wrapper. | High | Write-capable | Avoid; proprietary/native DLL dependency is out of scope. |
| OmenCore | `src/OmenCore.Linux/Hardware/LinuxKeyboardController.cs` | Linux HP keyboard capability/write paths and fallback strategy. | Medium | Write-capable on Linux | Safety/design reference only. |

## 5. Capability / device detection candidates

| Reference repo | File path | Short purpose | Risk | Read-only or write-capable | Use |
|---|---|---|---|---|---|
| ghelper-omen | `app/Omen/CapabilityDetectionService.cs` | Runtime capability detection across fans, thermals, lighting, GPU, drivers, and conflicts. | Low | Mostly read-only | Strong candidate for future read-only Victus capability discovery design. |
| ghelper-omen | `app/Omen/DeviceCapabilities.cs` | Capability model with fan, thermal, GPU, lighting, driver, and model-family fields. | Low | Read-only model | Useful model inspiration. |
| ghelper-omen | `app/Omen/ModelCapabilityDatabase.cs` | Conservative model capability database with product IDs, fan zones, lighting, GPU, and power features. | Low | Read-only data | Useful for static profile ideas; verify against Victus 16-s0035nt before trusting. |
| OmenSuperHub | `OmenHardware.cs` | HP/OMEN product validation and system design data interpretation. | Medium | Mixed | Research only; uses HP SDK model helpers. |
| OmenXHub | `OmenHardware.cs` | Cached BIOS/system data, fan type capability, thermal policy, CPU/GPU detection. | Medium | Mixed | Research only. |
| OmenCore | `src/OmenCore.Linux/Hardware/LinuxCapabilityClassifier.cs` | Explicit capability classes: full-control, profile-only, telemetry-only, unsupported-control. | Low | Read-only classifier | Excellent concept reference for VictusX safety states. |
| OmenCore | `src/OmenCore.Avalonia/Services/IHardwareService.cs` | UI-facing capability/status contract. | Low | Interface only | Useful abstraction inspiration, not code to copy. |

## 6. Safety checks or conflict checks

| Reference repo | File path | Short purpose | Risk | Read-only or write-capable | Use |
|---|---|---|---|---|---|
| ghelper-omen | `app/Omen/Services/ConflictDetectionService.cs` | Detects OMEN Gaming Hub, FanControl, XTU, RTSS, HWiNFO, Afterburner, and related conflicts. | Low | Read-only process/shared-memory checks | Good future safety prerequisite before any HP writes. |
| ghelper-omen | `app/Omen/DriverInitializationHelper.cs` | Driver/backend initialization checks. | Medium | Mixed | Use for safety sequencing ideas only. |
| ghelper-omen | `app/Omen/BackendStatus.cs` | Backend state reporting. | Low | Read-only model | Useful for diagnostics. |
| OmenCore | `src/OmenCore.Linux/Hardware/LinuxCapabilityClassifier.cs` | Treats risky boards as degraded/profile-only until write/readback proves control. | Low | Read-only classifier | Excellent safety policy inspiration. |
| OmenXHub | `Services/EcFanService.cs` | EC fan fallback. | High | Write-capable | Avoid initially; useful only to understand what not to enable without explicit approval. |

## 7. Files that look useful later

- ghelper-omen: `app/Omen/HpWmiBios.cs`, `IHpWmiBios.cs`, `CapabilityDetectionService.cs`, `DeviceCapabilities.cs`, `ModelCapabilityDatabase.cs`, `WmiBiosMonitor.cs`, `WmiFanController.cs`, `Services/ConflictDetectionService.cs`, `Lighting/WmiLightingBackend.cs`.
- OmenXHub: `OmenHardware.cs`, `App/OmenLighting.cs`, `Services/FanService.cs`, `Services/HardwareService.cs`, `Services/HardwareApiService.cs`.
- OmenSuperHub: `OmenHardware.cs`, `App/OmenLighting.cs`, `FanCurveProfile.cs`.
- OmenCore: `LinuxCapabilityClassifier.cs`, `IHardwareService.cs`, `FanCurveService.cs`, Linux command files for safety UX wording and capability-state behavior.

## 8. Files to avoid

- Proprietary/vendor binaries and SDK DLLs under OmenSuperHub/OmenXHub `Resources/`.
- Driver folders, `.sys`, `.inf`, setup scripts, installers, and build scripts.
- EC/MMIO/MSR/PawnIO paths until there is an explicit hardware-safety milestone.
- Full UI/page imports from OmenSuperHub/OmenXHub; VictusX already has the G-Helper shell.
- Any code copied directly without license review and attribution planning.

## Most useful reference repo

`ghelper-omen` is the most useful near-term reference because it already separates several HP/Omen concepts into services/interfaces and includes capability detection, conflict checks, and WMI BIOS wrappers.

## Recommended next step

Create a small read-only HP/Victus capability probing milestone for VictusX: detect HP/Victus identity, check whether `root\wmi`/`hpqBIntM` exists, collect BIOS/system-design availability if safe, and report capabilities as unsupported/unknown by default. Do not add fan, thermal, lighting, EC, BIOS, or WMI write commands yet.
