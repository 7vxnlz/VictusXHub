# HP Reference Index

Use this index to jump to exact read-only reference files. Revisions are tracked in `REFERENCE_SOURCES.md`; verify the SHA before relying on a source. Paths below are relative to `D:\Projects\Workspace\references\`. References provide behavior evidence, not target-device validation or permission to copy code.

| Feature / symbol | Reference file and symbol | Why it matters |
| --- | --- | --- |
| Victus 16-s0xxx / 8BD4 profile | `omencore/src/OmenCoreApp/Hardware/ModelCapabilityDatabase.cs` (`8BD4`, `16-s0`) | Closest known V1 device profile; close-device only, BIOS F.30. |
| 8BD4 field failures | `omencore/docs/3.6.3-BUG-REPORTS.md` (`DC-Victus16-8BD4`) | Records unsafe zero-level handoff/stuck-fan evidence. |
| HP BIOS WMI ABI | `omencore/src/OmenCoreApp/Hardware/HpWmiBios.cs` (`SendBiosCommand`, `SetFanMax`, `SetFanLevel`, `GetFanLevel`, `GetFanRpmDirect`) | Namespace/class/method, command types, input sizes, and read paths. |
| V1 percent mapping | `omencore/src/OmenCoreApp/Hardware/WmiFanController.cs` (`MapFanPercentToWmiLevel`, `SetFanSpeed`) | Close-device mapping: below 100 uses the V1 ceiling; 100 is special. |
| Restore/reapply lifecycle | `omencore/src/OmenCoreApp/Hardware/WmiFanController.cs` (`RestoreAutoControl`, `ClearV1AutoModeFloor`); `omencore/src/OmenCoreApp/Services/FanService.cs` | Multi-command recovery, retries, timers, suspend/resume behavior that VictusX must not inherit implicitly. |
| Device detection chain | `omencore/src/OmenCoreApp/Hardware/CapabilityDetectionService.cs`; `omencore/src/OmenCoreApp/Hardware/FanControllerFactory.cs` | Maps model/profile/generation to the WMI controller. |
| Alternate HP WMI implementation | `ghelper-omen/app/Omen/HpWmiBios.cs` (`SetFanLevel`, `ExtendFanCountdown`); `ghelper-omen/app/Omen/WmiFanController.cs` | Conflicting 128-byte setter versus four-byte replay and background control. |
| G-Helper shell/layout | `g-helper/app/Settings.cs`; `g-helper/app/UI/RButton.cs` | Behavioral/layout comparison for the inherited shell; do not copy blindly. |
| BIOS-only SetFanMax pair | `OmenSuperHub/OmenHardware.cs` (`SetMaxFanSpeedOn`, `SetMaxFanSpeedOff`); `OmenSuperHub/Program.Menu.cs` | Narrow on/off behavior reference, but one-byte payload and persistent UI are not adopted. |
| SuperHub curves/reapply | `OmenSuperHub/Program.cs`; `OmenSuperHub/Program.Config.cs`; `OmenSuperHub/FanCurveProfile.cs` | Timer-driven writes and startup restore that remain blocked. |
| XHub SetFanLevel/SetFanMax | `OmenXHub/OmenHardware.cs` (`SetFanLevel`, `SetMaxFanSpeedOn`, `SetMaxFanSpeedOff`) | BIOS WMI comparison including cleaning variants. |
| PawnIO direct-EC fallback | `OmenXHub/Services/EcFanService.cs`; `OmenXHub/Services/TrayService.cs` | Explicitly forbidden EC fallback, retries, curves, and background reassertion. |
| Remote fan routes | `OmenXHub/Services/HardwareApiService.cs` | Demonstrates unsafe user/API exposure and weak success propagation; do not port. |
| LibreHardwareMonitor telemetry | `OmenSuperHub/Program.cs` (`RunHardwareMonitor`) | Sensor discovery comparison only; driver/dependency adoption requires separate review. |

## Search Workflow

1. Search VictusX first.
2. Read `REFERENCE_POLICY.md` and confirm the SHA in `REFERENCE_SOURCES.md`.
3. Open only the indexed file(s), then search the exact symbol and callers.
4. Record paths, symbols, revision, device context, and confidence in the resulting evidence document.
5. Never modify or pack a reference repository.
