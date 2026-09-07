# G-Helper Entry Dependency Map

Scope: one-level dependency map for the imported application entry layer only. No recursive source inspection was performed.

## `ghelper-import/Program.cs`

| Direct dependency | Category |
| --- | --- |
| `SettingsForm`, `Extra`, form methods/properties | UI |
| `ToastForm` | UI |
| `HardwareOverlay` | UI / Hardware |
| `ModeControl`, `Modes` | Hardware |
| `GPUModeControl` | Hardware |
| `AllyControl` | ASUS-specific / Hardware |
| `ClamshellModeControl` | Generic infrastructure / Hardware-adjacent |
| `AsusACPI`, `AsusHid`, `AsusService` | ASUS-specific / Hardware |
| `HardwareControl` | Hardware |
| `BatteryControl` | Hardware |
| `InputDispatcher`, `NumberPad` | Hardware / Input |
| `ScreenControl`, `VisualControl`, `DynamicLightingHelper`, `ColorProfileHelper` | Hardware / Utility |
| `PeripheralsProvider`, `XGM`, `Aura` | ASUS-specific / Hardware |
| `AppConfig`, `Logger`, `ProcessHelper`, `Startup`, `NativeMethods`, `PowerSettingGuid` | Utility / Generic infrastructure |
| `Properties.Resources`, `Properties.Strings` | UI / Resources |

Next dependency to import: `Settings.cs` with its designer/resource files, because `Program.cs` constructs `SettingsForm` immediately and the tray/UI startup path depends on it.

## `ghelper-import/AppConfig.cs`

| Direct dependency | Category |
| --- | --- |
| `Logger`, `ProcessHelper` | Utility / Generic infrastructure |
| `Modes` | Hardware |
| `AsusFan`, `AsusACPI` | ASUS-specific / Hardware |
| `System.Management.ManagementObjectSearcher` | Generic infrastructure |
| `Microsoft.Win32.Registry` | Generic infrastructure / ASUS-adjacent detection |

Next dependency to import: none before UI startup mapping; this file is already present, but its `Modes`, `AsusFan`, and `AsusACPI` dependencies should remain unresolved until hardware boundaries are reviewed.

## `ghelper-import/NativeMethods.cs`

| Direct dependency | Category |
| --- | --- |
| `Logger` | Utility |
| Win32 P/Invoke APIs | Generic infrastructure |
| WinForms `Form` | UI / Generic infrastructure |

Next dependency to import: none; immediate local dependency `Logger` is already present.

## `ghelper-import/Helpers/Startup.cs`

| Direct dependency | Category |
| --- | --- |
| `Logger`, `ProcessHelper` | Utility / Generic infrastructure |
| `Microsoft.Win32.TaskScheduler` | Generic infrastructure |
| WinForms `Application`, `MessageBox` | UI / Generic infrastructure |

Next dependency to import: none; immediate local dependencies are already present.

## `ghelper-import/Helpers/Logger.cs`

| Direct dependency | Category |
| --- | --- |
| `ProcessHelper` | Utility / Generic infrastructure |

Next dependency to import: none; immediate local dependency is already present.

## `ghelper-import/Helpers/ProcessHelper.cs`

| Direct dependency | Category |
| --- | --- |
| `Logger` | Utility |
| `Properties.Strings` | UI / Resources |
| WinForms `Application`, `MessageBox` | UI / Generic infrastructure |
| Windows process/security APIs | Generic infrastructure |
| `KillSmartDisplayControl` target string | ASUS-specific utility |

Next dependency to import: none before `SettingsForm`; `Properties.Strings` base resources are already present, and ASUS service/process behavior should not be expanded yet.
