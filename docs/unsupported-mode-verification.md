# Unsupported Hardware Mode Verification

Date: 2026-08-30
Scope: static inspection of `app/` after the ASUS unsupported-mode guard passes. No source files were changed.

## Remaining unguarded paths, if any

- `app/Peripherals/Keyboard/AsusKeyboard.cs` and `app/Peripherals/Mouse/AsusMouse.cs` still contain low-level ASUS HID enumeration/read/write methods. Current detection/timer entry points in `PeripheralsProvider` are guarded, but these model methods are not independently guarded.
- `app/AnimeMatrix/Communication/Platform/WindowsUsbProvider.cs` can still open HID devices when constructed. Higher-level ASUS lighting/startup paths are guarded, but this provider is not independently aware of unsupported-hardware mode.
- `app/Gpu/GPUModeControl.cs` has helper methods such as `CaptureNvBootState()` and `StandardModeFix()` with direct ASUS ACPI access. Observed startup/shutdown callers are guarded, but the methods themselves are not fully self-guarded.
- `app/AppConfig.cs` still performs ASUS model/registry heuristics, including OLED registry lookup. These are read-only but still ASUS-specific.
- `app/Display/VisualControl.cs` and `app/Helpers/DynamicLightingHelper.cs` contain Windows registry writes for generic Windows display/lighting settings. They are not ASUS ACPI/HID writes, but should remain reviewed before enabling unsupported mode broadly.

## Paths already guarded

- `app/Program.cs` guards normal startup, ASUS service startup, initial hardware actions, session events, shutdown reset paths, overlay startup, peripherals registration, and startup battery fallback WMI behind unsupported-hardware mode.
- `app/Hardware/HardwareControllerFactory.cs` selects `UnsupportedHardwareController` when requested and keeps ASUS as the default path.
- `app/Hardware/UnsupportedHardwareController.cs` returns safe unsupported/default values without WMI, EC, BIOS, HID, or ASUS calls.
- `app/HardwareControl.cs` guards ASUS sensor fallbacks, fan RPM reads, GPU app cleanup, and other direct ASUS helper paths.
- `app/Input/InputDispatcher.cs` guards ASUS ACPI hotkey, keyboard, camera, ScreenPad, status LED, and ASUS service-dependent paths.
- `app/USB/AsusHid.cs`, `app/USB/Aura.cs`, and `app/USB/XGM.cs` guard direct ASUS HID/Aura/XG Mobile reads and writes.
- `app/Fans.cs`, `app/Fan/FanSensorControl.cs`, `app/Mode/ModeControl.cs`, `app/Battery/BatteryControl.cs`, `app/Gpu/GPUModeControl.cs`, and `app/Display/ScreenControl.cs` have guards on the main fan, mode, battery, GPU, and screen hardware entry paths.
- `app/Ally/AllyControl.cs` guards ROG Ally AutoTDP, TDP, controller, binding, turbo, and reset actions at the visible action/timer entry points.
- `app/Settings.cs`, `app/Extra.cs`, `app/Handheld.cs`, `app/AsusKeyboardSettings.cs`, and `app/AsusMouseSettings.cs` have guards around the reviewed user-triggered ASUS hardware actions.
- `app/UpdatesController.cs` guards ASUS firmware/package registry probes while leaving generic update logic intact.

## ASUS-specific but safe to leave for now

- `app/AsusACPI.cs` remains the ASUS implementation and is expected to keep ASUS-specific reads/writes until a later abstraction/replacement phase.
- ASUS UI labels, forms, resources, model heuristics, and branding remain in place by design; this verification only covers unsupported-mode execution safety.
- ASUS peripheral model classes remain useful reference structure for later abstraction, but should not be enabled as default unsupported-mode behavior.
- Armoury Crate update/filter metadata is ASUS-specific, but the high-risk service start/stop and firmware-registry paths are guarded.

## Recommended next code step

Add a tiny method-level guard pass for caller-guarded helpers: `GPUModeControl.CaptureNvBootState()`, `GPUModeControl.StandardModeFix()`, the low-level ASUS peripheral HID model methods, and `WindowsUsbProvider` construction/read/write methods. Keep default ASUS behavior unchanged and continue avoiding HP implementation until the ASUS abstraction boundary is cleaner.
