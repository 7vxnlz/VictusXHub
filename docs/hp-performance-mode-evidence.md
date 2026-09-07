# HP Performance Mode Evidence

## Decision

Real Silent/Balanced/Turbo switching on Victus 16-s0035nt / 7Z5Z2EA#AB8 / F.31 / V1 remains **NO-GO**. Static reference mappings are not exact-device write, readback, or recovery validation. No write was implemented or executed.

## Closest Evidence

Reference paths below are relative to `D:\Projects\Workspace\references\` and were inspected read-only.

| Reference / symbol | Finding | Confidence |
| --- | --- | --- |
| `omencore/src/OmenCoreApp/Hardware/ModelCapabilityDatabase.cs`, 8BD4 profile | Victus 16-s0xxx, V1, F.30; `UserVerified=false`, V1 zero-floor clearing disabled; notes describe stuck/non-reactive fan behavior. SKU keyboard evidence does not validate performance switching. | Close-device evidence, not F.31 validation |
| `omencore/src/OmenCoreApp/Hardware/HpWmiBios.cs`, `SetFanMode`, `FanMode`, `SendBiosCommand` | `root\wmi`, `hpqBIntM`, `hpqBIOSInt0`, command `0x20008`, type `0x1A`, four bytes `[FF, mode, 00, 00]`. Default `30`, Performance `31`, Cool `50` (hex). | Implementation inference |
| `omencore/src/OmenCoreApp/Hardware/WmiFanController.cs`, `SetPerformanceMode` | Quiet aliases select Cool `50`; default selects `30`; performance selects `31`. Can release MaxFan and starts countdown maintenance for non-default modes. | Implementation inference |
| Same file, `ConfirmFanModeReadback` | Accepts command result when EC is unavailable or strict readback is disabled. Not an authoritative WMI current-mode getter. | Implementation inference |
| `OmenSuperHub/OmenHardware.cs`, `SetFanMode` | Two-byte `[FF, mode]`; Balance `30`, Performance `31`, Cool `50`, but Quiet falls to default `30`. | Generic HP implementation |
| `OmenXHub/OmenHardware.cs`, `SetFanMode`, `SetBalanceMode` | Generic Balance `30`, but dedicated Balance sends `32`; two-byte buffers. | Conflicting implementation inference |
| `ghelper-omen/app/Omen/HpWmiBios.cs`, `SetFanMode`; `app/Omen/WmiFanController.cs` | Four-byte `30/31/50` corroboration; fan hints, MaxFan release and timed maintenance are separate risks. | Generic HP implementation |

Inspected revisions: omencore `b39b44978902606aa708cc0d78bcfd87e95fd88b`; OmenSuperHub `a6ab6988c446ee5421466097fdf60c0d521e5c81`; OmenXHub `ca84cb011d1d3e5850445d19e45fbea06e83a8fd`; ghelper-omen `1694844d2725e79a2b2065a0a1494fa1d143e3f4`.

## Blockers

- Silent is an application alias, not a proven F.31 firmware mode; references disagree on its mapping.
- Two-byte versus four-byte acceptance is unvalidated on this target. Balanced also has a conflicting dedicated reference value.
- Reliable current-mode readback has not been established. Saved `performance_mode` configuration is not hardware state.
- Closest controller transition/recovery paths include fan-level hints, MaxFan release and periodic reapplication. These must not be inherited into a one-shot, no-fallback implementation.
- `0x37` power setters are separate from the isolated mode setter; no evidence justifies adding them here.

## Safe State Wiring

`HpPerformanceModeStatus` stores reference-only aliases and permanently reports unavailable current mode and no switch capability. It has no transport, payload builder, delegate or executor. HP mode clears inherited tile activation, preserves disabled controls, and exposes the blocker through tooltip/accessibility text and Diagnostic summary. Tray refresh uses the unknown state and base VictusX icon, never saved ASUS configuration. Asset routing remains available for future verified states; application identity and non-HP behavior are unchanged.

No successful mode-change refresh is claimed: no real transition exists. Tests cover reference aliases, unknown/invalid state, neutral tray routing and disabled UI wiring.

Next safe task: obtain authoritative exact-F.31 supported-mode, input-length, current-state and one-shot recovery evidence independent of fan writes/reapply loops before designing an executable transport. No mode value is authorized for execution. Fan control remains NO-GO; `DeviceValidatedInputLength` remains null.
