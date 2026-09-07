# ASUS Dependency Inventory

Scope: imported G-Helper source currently in VictusControl. This inventory identifies ASUS-specific dependencies only; it does not remove, redesign, or implement replacements.

## Summary

The imported app is tightly centered on ASUS ACPI/WMI/HID behavior. The highest-coupling dependencies are `AsusACPI`, `Program.acpi`, `AppConfig` model detection, `ModeControl`, `GPUModeControl`, `HardwareControl`, `Aura`, and the WinForms settings/fan UI that calls them directly.

## Subsystems

| # | Subsystem | Folder | Main classes/files | Dependency type | Coupling | Can it be abstracted? | Suggested future HP replacement |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Application startup hardware hub | `.` | `Program.cs` | Hardware, Device detection, Service, UI | High | Yes, but only after introducing startup composition boundaries | HP-safe app startup that selects an HP/Victus hardware provider instead of global `Program.acpi` |
| 2 | ASUS ACPI/WMI command layer | `.` | `AsusACPI.cs`, `AsusFan`, `AsusMode`, `AsusGPU` | Hardware, Native API, ASUS WMI | High | Yes; this should become a backend behind hardware-control interfaces | HP Victus hardware backend using verified read/write mechanisms only after capability discovery |
| 3 | ASUS model and capability detection | `.` | `AppConfig.cs` | Device detection, Registry, Hardware | High | Yes; detection should be separated from settings | HP/Victus identity and capability detection based on read-only Windows data and explicit profiles |
| 4 | Global hardware telemetry/control coordinator | `.` | `HardwareControl.cs` | Hardware, Native API, ASUS ACPI, WMI | High | Yes, but it mixes sensors, battery, fan, CPU/GPU power, and overlay feed | HP-safe telemetry aggregator plus separate capability-gated control services |
| 5 | ASUS services and Armoury Crate integration | `Helpers/`, `.`, `Display/` | `Helpers/AsusService.cs`, `Extra.cs`, `Settings.cs`, `UpdatesController.cs`, `Helpers/ProcessHelper.cs`, `Display/VisualControl.cs` | Service, Registry, UI | High | Yes | HP/OMEN service coexistence policy that detects conflicts without disabling services by default |
| 6 | Fan curve and fan sensor control | `Fan/`, `.` | `Fan/FanSensorControl.cs`, `Fans.cs`, `Fans.Designer.cs`, `Fans.resx` | Hardware, UI | High | Yes | HP Victus fan capability model and backend only after safe fan command validation |
| 7 | Performance modes and power limits | `Mode/`, `.` | `Mode/ModeControl.cs`, `Mode/Modes.cs`, `Mode/PowerNative.cs`, `Fans.cs` | Hardware, Native API, Registry | High | Yes | HP thermal/performance profile service with conservative supported-mode mapping |
| 8 | GPU mode, MUX, Eco/Ultimate, XG Mobile | `Gpu/`, `USB/`, `.` | `Gpu/GPUModeControl.cs`, `Gpu/IGpuControl.cs`, `Gpu/NVidia/*`, `Gpu/AMD/*`, `USB/XGM.cs`, `Settings.cs` | Hardware, ASUS-specific, Native API | High | Partially; vendor GPU APIs may remain generic, ASUS MUX/XG logic must be isolated | HP-safe GPU information/control boundary; no HP MUX behavior assumed until verified |
| 9 | Battery charge/discharge control | `Battery/`, `.`, `Mode/` | `Battery/BatteryControl.cs`, `Program.cs`, `AsusACPI.cs`, `Mode/PowerNative.cs` | Hardware, Native API | High | Yes | HP battery charge-limit backend only if a safe supported API is discovered |
| 10 | Display and ASUS visual controls | `Display/` | `ScreenControl.cs`, `VisualControl.cs`, `ScreenBrightness.cs`, `ScreenCCD.cs`, `DisplayNative.cs`, `AmdDisplay.cs`, `ColorProfileHelper.cs` | Hardware, Registry, Native API, WMI | Medium | Yes | Split generic Windows display features from HP/Victus-specific display features |
| 11 | Keyboard hotkeys, status LEDs, and input dispatch | `Input/`, `.` | `Input/InputDispatcher.cs`, `Input/MKeyControl.cs`, `Input/NumberPad.cs`, `Input/KeyboardHook.cs`, `Input/KeyboardListener.cs`, `AsusKeyboardSettings.cs` | Hardware, UI, ASUS-specific | High | Yes | HP keyboard backlight/hotkey provider after read-only key/event discovery |
| 12 | Aura, ASUS HID, and RGB/lamp array | `USB/`, `Helpers/`, `UI/` | `USB/Aura.cs`, `USB/AsusHid.cs`, `USB/AsusLampArray.cs`, `Helpers/DynamicLightingHelper.cs`, `UI/RColorPicker.cs` | Hardware, Native API, ASUS HID, UI | High | Yes | HP keyboard lighting provider, separate from generic color UI and Windows Dynamic Lighting |
| 13 | Anime Matrix and Slash lighting/display | `AnimeMatrix/`, `.`, `USB/` | `AnimeMatrix/*`, `Matrix.cs`, `Slash.cs`, `USB/Aura.cs` | Hardware, UI, ASUS-specific | Medium | Yes; likely removable or optional for Victus | No HP replacement unless target Victus has equivalent hardware |
| 14 | ASUS external/peripheral devices | `Peripherals/`, `USB/`, `.` | `Peripherals/PeripheralsProvider.cs`, `Peripherals/Keyboard/AsusKeyboard.cs`, `Peripherals/Mouse/AsusMouse.cs`, model classes, `AsusMouseSettings.cs`, `AsusKeyboardSettings.cs` | Hardware, ASUS HID, UI | Medium | Yes | Optional HP/peripheral-neutral device layer; likely out of scope for first Victus port |
| 15 | ROG Ally handheld behavior | `Ally/`, `.` | `Ally/AllyControl.cs`, `Handheld.cs`, `Handheld.Designer.cs`, `Handheld.resx` | Hardware, UI, ASUS-specific | Medium | Yes; likely feature-gated or removed later | No HP replacement for Victus laptop unless a generic controller layer is needed |
| 16 | Low-level CPU/driver/PawnIO access | `Pawn/`, `.`, `Mode/` | `Pawn/IntelMsr.cs`, `Pawn/RyzenSmu.cs`, `Pawn/PawnIOWrapper.cs`, `Pawn/CpuInfo.cs`, `HardwareControl.cs`, `ModeControl.cs`, `Fans.cs` | Hardware, Native API, Driver/EC-adjacent | High | Yes, with strict safety boundary | No default HP replacement; defer all driver/MSR/SMU access unless explicitly justified |
| 17 | ASUS-specific registry usage | cross-cutting | `AppConfig.cs`, `Display/VisualControl.cs`, `Display/ScreenControl.cs`, `Battery/BatteryControl.cs`, `Input/*`, `Helpers/Keystone.cs`, `Helpers/DynamicLightingHelper.cs`, `Peripherals/*`, `UpdatesController.cs` | Registry, Device detection, Service | Medium | Yes | HP-safe registry reader/writer policy; prefer read-only until exact keys are verified |
| 18 | ASUS-specific UI and forms | `.`, `UI/`, `Properties/`, `Resources/` | `Settings.cs`, `Extra.cs`, `Fans.cs`, `Matrix.cs`, `Slash.cs`, `Updates.cs`, designer/resx files, icons/resources | UI | High | Yes; UI must be decoupled from direct hardware calls | Victus UI backed by capability-aware view models/services, with unsupported controls hidden or disabled |
| 19 | Update and ASUS package filtering | `AutoUpdate/`, `.` | `AutoUpdate/AutoUpdateControl.cs`, `UpdatesController.cs`, `Updates.cs` | Service, Registry, UI | Medium | Yes | VictusControl updater plus optional HP/Windows update awareness; no ASUS package assumptions |
| 20 | Generic native/Windows API wrappers with ASUS call sites | `.`, `Display/`, `Helpers/`, `Overlay/`, `Gpu/` | `NativeMethods.cs`, `DisplayNative.cs`, `ScreenNative.cs`, `Helpers/OSDBase.cs`, `Helpers/RestrictedProcessHelper.cs`, `Gpu/AMD/AmdAdl2.cs`, `Gpu/NVidia/*`, `Overlay/EtwFpsMonitor.cs` | Native API, Hardware-adjacent | Low to Medium | Partially; generic wrappers can remain, ASUS-specific callers must be isolated | Keep generic Windows/GPU wrappers only where useful; route HP-specific usage through abstractions |

## Marker Coverage

- `Program.acpi`, `AsusACPI`, and `AsusFan` appear across startup, settings UI, fan UI, fan sensors, GPU mode, hardware telemetry, battery, display, input, mode, Ally, Aura, and XG Mobile paths.
- ASUS WMI usage is concentrated in `AsusACPI.cs` through `AsusAtkWmi_WMNB` and `AsusAtkWmiEvent`, plus read-only Windows WMI usage in display/hardware helpers.
- ASUS HID usage is concentrated in `USB/AsusHid.cs`, `USB/Aura.cs`, `USB/AsusLampArray.cs`, `USB/XGM.cs`, and ASUS peripheral classes.
- ASUS service/Armoury integration is concentrated in `Helpers/AsusService.cs`, with UI exposure in `Settings.cs` and `Extra.cs`.
- ASUS-specific UI is spread through forms, designer files, resources, and localized strings.

## Replacement Difficulty Ranking

Easiest to hardest:

1. Update/package filtering (`AutoUpdate/`, `UpdatesController.cs`)
2. Generic native/Windows wrappers with ASUS call sites
3. ASUS-specific registry usage policy
4. Anime Matrix and Slash feature area
5. ROG Ally handheld feature area
6. ASUS external/peripheral devices
7. Display and visual controls
8. ASUS services and Armoury Crate integration
9. Battery charge/discharge control
10. Keyboard hotkeys, status LEDs, and input dispatch
11. Aura, ASUS HID, and RGB/lamp array
12. GPU mode, MUX, Eco/Ultimate, XG Mobile
13. Fan curve and fan sensor control
14. Performance modes and power limits
15. ASUS-specific UI and forms
16. Application startup hardware hub
17. Global hardware telemetry/control coordinator
18. ASUS model and capability detection
19. Low-level CPU/driver/PawnIO access
20. ASUS ACPI/WMI command layer

## Highest-Risk Area

`AsusACPI.cs` is the highest-risk subsystem because it centralizes write-capable ACPI/WMI commands, fan curves, performance modes, GPU switching, keyboard power/RGB behavior, and capability probing through ASUS-specific device IDs.

## Lowest-Risk Area

`AutoUpdate/AutoUpdateControl.cs` and `UpdatesController.cs` are the lowest-risk ASUS-specific subsystem because most ASUS coupling appears to be package filtering, update presentation, and registry/package metadata rather than direct hardware writes.
