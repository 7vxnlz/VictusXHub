# VictusX ASUS Removal Map

Scope: preparation map only. No ASUS code is deleted here, no HP code is proposed or implemented, and runtime behavior must remain unchanged until a later explicit refactor.

## Must Stub First

- `app/AsusACPI.cs` - central ASUS ACPI/WMI command surface; callers need a safe no-op/unsupported seam before removals.
- `app/HardwareControl.cs` - global telemetry/control coordinator reads fans, temps, battery, CPU/GPU power, and ACPI data.
- `app/Mode/ModeControl.cs` - applies performance modes, power limits, undervolt, and shutdown/sleep reset behavior.
- `app/Mode/Modes.cs` - shared mode constants/state are used by app startup, UI, fan config, and power behavior.
- `app/Fan/FanSensorControl.cs` - formats and monitors ASUS fan readings used by UI/status labels.
- `app/Battery/BatteryControl.cs` - charge-limit/discharge behavior depends on ASUS ACPI and Windows battery APIs.
- `app/Gpu/GPUModeControl.cs` - controls ASUS Eco/Standard/Ultimate, MUX, and XG Mobile paths.
- `app/Display/ScreenControl.cs` - mixes generic display control with ASUS ACPI screen features.
- `app/USB/Aura.cs` - keyboard/RGB behavior crosses ASUS HID and ASUS ACPI paths.
- `app/Input/InputDispatcher.cs` - routes hotkeys/status LEDs/backlight behavior into ASUS ACPI/HID services.

## Can Remove Early

- `app/Ally/` - ROG Ally-specific handheld behavior; not relevant to HP Victus laptop once UI/startup references are safely gated.
- `app/AnimeMatrix/` - ASUS Anime Matrix/Slash device logic; likely no HP Victus equivalent.
- `app/Slash.cs`, `app/Slash.Designer.cs`, `app/Slash.resx` if present later - ASUS Slash UI; likely removable after UI references are isolated.
- `app/Matrix.cs`, `app/Matrix.Designer.cs`, `app/Matrix.resx` - ASUS Anime Matrix UI; likely removable after UI references are isolated.
- `app/USB/XGM.cs` - ASUS XG Mobile-specific behavior; removable after GPU UI references are isolated.
- `app/Peripherals/Keyboard/Models/` - ASUS keyboard model catalog; removable after peripheral provider is isolated.
- `app/Peripherals/Mouse/Models/` - ASUS mouse model catalog; removable after peripheral provider is isolated.

## Must Keep Temporarily

- `app/Program.cs` - startup currently wires global ASUS services/controllers and UI; changing it would change runtime behavior.
- `app/AppConfig.cs` - stores settings and contains ASUS model/capability detection used throughout the app.
- `app/Settings.cs`, `app/Fans.cs`, `app/Extra.cs` - UI code directly references ASUS services, ACPI, fans, modes, GPU, Aura, and debug tools.
- `app/Gpu/IGpuControl.cs` - small existing GPU abstraction used by NVIDIA/AMD implementations.
- `app/Gpu/NVidia/` - partly vendor-generic GPU support; keep until ASUS MUX logic is separated from generic GPU behavior.
- `app/Gpu/AMD/` - partly vendor-generic GPU support; keep until ASUS-specific power/display assumptions are separated.
- `app/Display/DisplayNative.cs`, `app/Display/ScreenNative.cs`, `app/Display/ScreenBrightness.cs`, `app/Display/ScreenCCD.cs` - generic Windows display wrappers may still be useful.
- `app/Input/KeyboardHook.cs`, `app/Input/KeyboardListener.cs` - generic input hooks may still be useful after ASUS hotkey routing is isolated.
- `app/UI/` - custom WinForms controls are mostly generic and should not be removed during ASUS hardware isolation.

## Needs HP Replacement Later

- `app/AsusACPI.cs` - replace with HP/Victus hardware backend behind the existing abstraction seam.
- `app/Hardware/Abstractions/IHardwareController.cs` - evolve into vendor-neutral contracts that do not expose ASUS-specific names or enums.
- `app/Mode/` - replace ASUS performance-mode and power-limit mapping with HP-supported capability-driven modes.
- `app/Fan/` plus `app/Fans.cs` - replace ASUS fan read/write assumptions with HP Victus fan capability behavior.
- `app/Battery/` - replace ASUS battery limit/discharge reads with HP-supported battery behavior if available.
- `app/Gpu/GPUModeControl.cs` - replace ASUS Eco/MUX/XG decisions with HP-safe GPU capability decisions.
- `app/Display/VisualControl.cs` and ASUS branches in `app/Display/ScreenControl.cs` - replace ASUS GameVisual/Splendid behavior with HP-safe display features or disable them.
- `app/USB/Aura.cs`, `app/USB/AsusHid.cs`, `app/USB/AsusLampArray.cs` - replace ASUS keyboard/RGB transport with HP-supported keyboard lighting, if verified.
- `app/Input/InputDispatcher.cs`, `app/Input/MKeyControl.cs`, `app/Input/NumberPad.cs` - replace ASUS hotkey/status LED mappings with HP-safe input handling.
- `app/Peripherals/PeripheralsProvider.cs`, `app/Peripherals/Keyboard/AsusKeyboard.cs`, `app/Peripherals/Mouse/AsusMouse.cs` - replace or remove ASUS peripheral support based on Victus scope.
- `app/Pawn/` - replace only if a safe, necessary, non-proprietary HP-compatible low-level capability is explicitly justified.

## Unknown / Inspect Later

- `app/Display/AmdDisplay.cs` - may be generic AMD display behavior or ASUS-specific in practice; inspect when display work starts.
- `app/Helpers/DynamicLightingHelper.cs` - may be partly useful for Windows Dynamic Lighting, but current Aura coupling must be checked.
- `app/Helpers/DeviceHelper.cs` - likely generic device enumeration; inspect before deciding to keep or abstract.
- `app/Helpers/Keystone.cs` - ASUS Keystone-specific; inspect when removing ASUS extras.
- `app/Overlay/` - hardware overlay may be reusable, but it consumes `HardwareControl` outputs.
- `app/Peripherals/IPeripheral.cs` - may be reusable as a generic shape, but current implementations are ASUS-heavy.
- `app/Mode/PowerNative.cs` - Windows power API wrapper; inspect before replacing mode behavior.
- `app/Pawn/CpuInfo.cs` - may contain generic CPU detection, but lives beside driver/MSR/SMU code.

## Answers

1. Safest first ASUS-removal code step: introduce an `UnsupportedHardwareController`/stub implementation behind the existing abstraction seam and allow startup composition to choose it without changing current ASUS behavior.
2. Central risk file: `app/AsusACPI.cs`.
3. Folder likely removable earliest: `app/Ally/`, after UI/startup references are safely gated.
4. Folder that must not be touched yet: `app/Pawn/`, because it contains low-level driver/MSR/SMU access and is referenced by power/fan/mode flows.
5. Next Codex task: create a build-safe plan for inserting a no-op hardware controller seam without removing ASUS code or changing default runtime behavior.
