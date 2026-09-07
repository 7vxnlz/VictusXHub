# Remaining ASUS Risk List

Date: 2026-08-30
Scope: post-guard sweep for `--unsupported-hardware` mode in the imported VictusX/G-Helper codebase.

This list tracks ASUS-specific code that still exists after the unsupported-hardware isolation passes. The goal is safe isolation, not ASUS removal.

## Startup/background

| Path | Risk | Status | Next action |
|---|---|---|---|
| `app/Program.cs` | ASUS ACPI/service startup, battery limit startup, power-source charger-mode read | Mostly already guarded; `GetPowerSource()` now returns a safe battery fallback in unsupported mode | Leave unchanged until controller selection is abstracted further |
| `app/Input/InputDispatcher.cs` | ASUS ACPI keyboard event listener, hotkeys, LEDs, camera, ScreenPad | Already guarded at startup and method boundaries; raw event handler now exits in unsupported mode | Leave unchanged unless testing finds a missed hotkey path |
| `app/Input/MKeyControl.cs` | ASUS M-key HID writes | Safe to guard now; `ApplyAll()` and `Reset()` now exit in unsupported mode | Later replace with hardware-independent hotkey abstraction |
| `app/Ally/AllyControl.cs` | Ally AutoTDP timer and controller HID writes | AutoTDP/PPT writes already guarded; Ally controller HID helpers remain ASUS-specific but normally unreachable outside Ally mode | Leave controller HID behavior for a dedicated Ally removal pass |

## UI-triggered

| Path | Risk | Status | Next action |
|---|---|---|---|
| `app/Settings.cs` | Armoury, GPU, fan, Matrix, Aura, XGM, Ally button handlers | Already guarded on direct high-risk handlers | Leave UI in place until a later UI pruning/rebrand pass |
| `app/Extra.cs` | ACPI testing, boot sound, status LED, NumberPad, ASUS services, cores/VRAM/APU controls | Already guarded at constructor edge and direct handlers | Later split ASUS-only advanced controls from generic settings |
| `app/Handheld.cs` | Ally controller configuration | User-triggered Ally hardware actions now no-op in unsupported mode | Leave UI intact until dedicated Ally UI isolation |
| `app/AsusKeyboardSettings.cs`, `app/AsusMouseSettings.cs` | ASUS peripheral writes | Direct write handlers already guarded | Leave class names and screens unchanged until rebrand/removal pass |

## Sensor/telemetry

| Path | Risk | Status | Next action |
|---|---|---|---|
| `app/HardwareControl.cs` | ACPI sensor fallback reads for CPU/GPU temps, fan RPM, battery discharge | Main read loops and direct helper fallbacks now skip ASUS ACPI in unsupported mode | Later add cleaner hardware-neutral sensor abstraction |
| `app/Fan/FanSensorControl.cs` | Fan calibration and RPM polling | Startup/calibration callers guarded; internal calibration still ASUS-specific | Leave unchanged until fan control abstraction replaces it |
| `app/Pawn/` | Low-level CPU/SMU access | Still present from G-Helper; not ASUS-only but hardware-sensitive | Leave unchanged; inventory before any HP-specific use |

## Lighting/peripherals

| Path | Risk | Status | Next action |
|---|---|---|---|
| `app/USB/Aura.cs` | ASUS HID and ACPI lighting writes | Shared writer methods already guarded | Later extract generic lighting capability interface |
| `app/USB/XGM.cs` | ASUS XG Mobile HID discovery/writes | Safe guards added to public XGM methods | Later remove or isolate behind external-GPU capability |
| `app/USB/AsusHid.cs` | Raw ASUS HID discovery and writes | Public read/write helpers now return safe defaults in unsupported mode | Still central dependency; isolate behind capability interfaces later |
| `app/USB/AsusLampArray.cs` | ASUS lighting device access | Still ASUS-specific; reached from Aura paths, now largely guarded by Aura entry points | Leave unchanged until Aura abstraction pass |
| `app/Peripherals/` | ASUS mouse/keyboard HID models and detection | Detection and settings handlers already guarded; model classes remain ASUS-specific | Leave unchanged until peripheral abstraction/removal pass |
| `app/AnimeMatrix/`, `app/Matrix.cs`, `app/Slash.cs`, `app/Input/NumberPad.cs` | ASUS lighting panels and NumberPad control | UI entry/handler paths guarded; low-level device classes remain ASUS-specific | Leave unchanged for dedicated lighting/peripheral removal pass |

## Fan/power/mode

| Path | Risk | Status | Next action |
|---|---|---|---|
| `app/Mode/ModeControl.cs` | ASUS performance mode, fan curves, power limits, Ryzen/UV helpers | Major public and helper paths already guarded | Leave unchanged until hardware interfaces replace direct ACPI calls |
| `app/Mode/Modes.cs` | ASUS/Vivobook mode read and mode constants | Guarded where it reads ACPI; constants remain | Leave unchanged |
| `app/Fans.cs` | Fan curves, GPU power, hysteresis, power sliders, calibration | Guarded at form/helper boundaries | Leave unchanged until fan UI is rewritten or hidden for HP |
| `app/Battery/BatteryControl.cs` | ASUS battery limit ACPI and registry writes | Already guarded | Leave unchanged until battery capability abstraction exists |

## Display/GPU

| Path | Risk | Status | Next action |
|---|---|---|---|
| `app/Gpu/GPUModeControl.cs` | ASUS dGPU Eco/MUX/XGM control | Major entry points already guarded | Leave unchanged until GPU mode abstraction exists |
| `app/Display/ScreenControl.cs` | ASUS screen overdrive, miniLED, FHD/HDR ACPI state | Main public paths already guarded | Leave visualization-only reads for later cleanup |
| `app/Display/VisualControl.cs` | ASUS GameVisual/Splendid/ICC behavior | Main public paths already guarded | Leave unchanged until display feature policy exists |
| `app/Display/AmdDisplay.cs` | AMD display optimization registry checks | Not ASUS ACPI; not guarded | Leave unchanged for now |

## ASUS service/update/package logic

| Path | Risk | Status | Next action |
|---|---|---|---|
| `app/Helpers/AsusService.cs` | ASUS service detection/start/stop | Startup and UI callers guarded; service helper remains ASUS-specific | Later isolate behind service-management abstraction |
| `app/UpdatesController.cs` | ASUS package/update metadata and ASUS registry reads | ASUS-specific firmware/package registry probes now skipped in unsupported mode | Leave generic update inventory behavior unchanged until rebrand phase |
| `app/AppConfig.cs` | ASUS model-family detection and ASUS registry read for OLED care | Still ASUS-specific by design during fork transition | Leave unchanged until device identity abstraction replaces model heuristics |

## Highest-risk remaining unguarded areas

1. `app/USB/AsusHid.cs` and dependent peripheral classes remain the central raw ASUS HID layer. They should not be edited piecemeal; isolate behind capability interfaces later.
2. `app/HardwareControl.cs` still contains direct helper-level ACPI sensor fallbacks. Main loops are guarded, but a future pass should add safer helper defaults.
3. `app/Handheld.cs` and deeper `app/Ally/AllyControl.cs` controller HID methods remain ROG Ally-specific. They are not the next HP Victus priority.
4. `app/UpdatesController.cs` still assumes ASUS/G-Helper update/package metadata and needs a later rebrand/update strategy pass.

## Safest next tiny fix

Next safest step is a focused abstraction pass around `AsusHid`, peripherals, and lighting. Do not remove ASUS code yet.