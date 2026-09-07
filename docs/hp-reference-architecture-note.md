# HP Reference Architecture Note

## ghelper-omen

1. Main architecture style: G-Helper-shaped WinForms app with HP/Omen hardware code grouped under `app/Omen/`.
2. HP hardware logic lives in `app/Omen/`, especially `HpWmiBios.cs`, `IHpWmiBios.cs`, `CapabilityDetectionService.cs`, `WmiBiosMonitor.cs`, and fan/lighting services.
3. UI and hardware are partially separated through Omen services/interfaces, but the app remains close to the original G-Helper structure.
4. Safe/clean to learn from: yes, especially for HP WMI command shape and capability checks; avoid copying directly.
5. Copy conceptually: keep HP hardware access behind narrow interfaces and capability detection.
6. Avoid: importing WMI write paths, EC/PawnIO paths, heartbeat/reapply loops, or fan-control behavior before explicit safety gates.

## omencore

1. Main architecture style: larger multi-project app with clearer service, hardware, UI, tests, and cross-platform boundaries.
2. HP hardware logic lives mainly in `src/OmenCoreApp/Hardware/` and hardware-facing services under `src/OmenCoreApp/Services/`.
3. UI is more separated through services/view models, especially in the Avalonia project.
4. Safe/clean to learn from: yes for architecture and safety states; less direct for VictusX because it is not a G-Helper fork.
5. Copy conceptually: explicit capability levels, diagnostics, conflict checks, and tested service boundaries.
6. Avoid: broad migration of its service graph, Linux-specific implementation details, driver/install tooling, telemetry loops, and EC write paths.

## OmenSuperHub

1. Main architecture style: compact WinForms-style app with hardware commands close to UI/menu/config code.
2. HP hardware logic lives mostly in top-level `OmenHardware.cs` plus `App/OmenLighting.cs`.
3. UI and hardware separation is limited; many calls are static and command-oriented.
4. Safe/clean to learn from: useful for raw command cross-checking, not as an architecture model.
5. Copy conceptually: simple command catalog cross-reference for known HP BIOS WMI command IDs.
6. Avoid: static global hardware control style, proprietary SDK assumptions, bundled binaries, and direct UI-to-hardware writes.

## OmenXHub

1. Main architecture style: WPF/service-heavy Omen utility with richer folders for services, pages, views, models, and raw hardware commands.
2. HP hardware logic lives in top-level `OmenHardware.cs`, `App/OmenLighting.cs`, and service files such as `HardwareService.cs`, `FanService.cs`, and `HardwareApiService.cs`.
3. UI and hardware are separated more than OmenSuperHub through services/views, but raw static command paths still exist.
4. Safe/clean to learn from: good for command cross-checking and service ideas; too broad to follow directly.
5. Copy conceptually: keep raw command metadata separate from higher-level services and UI.
6. Avoid: native lighting DLL paths, EC/driver paths, broad service import, and direct command execution without Victus-specific safety gates.

## Current recommendation

- Best architecture reference: `omencore`, because it has the clearest separation of hardware, services, UI, diagnostics, and tests.
- Best HP command reference: `ghelper-omen\app\Omen\HpWmiBios.cs`, because it is closest to the current VictusX/G-Helper codebase and already names the HP WMI command surface.
