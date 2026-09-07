# SetFanMode and SetFanLevel Risk Separation Study

## Scope

This study covers two write commands only: SetFanMode (`0x1A`) and SetFanLevel (`0x2E`). It does not authorize either command, select a payload, or change the SetFanMax **NO-GO** decision.

## 1. SetFanMode Reference Evidence

The reviewed references send command `0x20008`, type `0x1A`, and output size `0`. The intended data starts with `FF` followed by a firmware mode value.

- ghelper-omen and omencore use four bytes: `FF`, mode, `00`, `00`.
- OmenSuperHub and OmenXHub use two bytes: `FF`, mode.

The mode mapping itself depends on thermal-policy generation. The hub projects map V1 UI states to values such as `0x30`, `0x31`, and `0x50`; omencore additionally treats a mode change as a transition that may need verification and later fan-level handoff work. This is policy control, not a harmless status selection.

## 2. SetFanLevel Reference Evidence

The reviewed references send command `0x20008`, type `0x2E`, and output size `0`, but disagree materially on input shape and interpretation.

- ghelper-omen's current implementation creates a 128-byte buffer with the first two bytes set, despite its nearby format comment describing four bytes.
- omencore uses four bytes: fan 1, fan 2, `00`, `00`.
- OmenSuperHub and OmenXHub use two bytes for two fans or three bytes for a third fan; OmenXHub can fall back to direct EC writes when its WMI level write fails.

The same numeric values are not portable: references describe V1 as a krpm-style range and V2 as percentage-like, while hub code also applies Clean Creek capability bits. VictusX has no device-validated SetFanLevel size, scale, fan ordering, or capability-bit contract.

## 3. Method/Input Shape Summary

| Command | Reference shapes found | Consequence |
| --- | --- | --- |
| SetFanMode `0x1A` | `FF-mode` or `FF-mode-00-00` | Input length and mode meanings are not device-validated. |
| SetFanLevel `0x2E` | two, three, four, and 128-byte buffers | Length, fan count, scale, and optional flags vary across code paths. |

Both commands use `hpqBIOSInt0` in their corresponding write helpers, but that shared output method does not make their input ABI or effects equivalent.

## 4. Prior-State Dependencies

SetFanMode changes the thermal-policy state that governs later fan behavior. omencore documents transitions from Performance to Default that may leave the hardware state unchanged unless followed by a V1-specific fan-level hint.

SetFanLevel is therefore stateful: references send it after mode changes, after max-fan release, during timer keepalive, or as a manual-floor clear. The same bytes can mean a transition hint, a manual target, a floor, or a dangerous zero-duty command depending on firmware and current mode.

## 5. Restore And Readback Requirements

Any future experiment would need a device-validated restore sequence, not merely a successful return code:

- SetFanMode needs an approved baseline mode and a readback that proves the intended mode took effect.
- SetFanLevel needs baseline raw values, validated semantics, a post-write observation, and a proven handoff back to BIOS automatic control.
- Both need a successful final `FanMaxGet` baseline match where applicable, thermal observation, AC power, human approval, and a manual recovery path.

VictusX intentionally has none of these write-specific readbacks or restore proofs.

## 6. Interaction With FanGetLevel

VictusX's `FanGetLevel` result is explicitly raw-only: current values must not be interpreted as RPM, percent, fan curve position, or a writable target. It can show returned bytes, but cannot prove that a SetFanLevel payload was accepted semantically, that a SetFanMode transition completed, or that automatic control was restored.

## 7. Why They Are Riskier Than SetFanMax Now

SetFanMax has one binary target and an existing read-only `FanMaxGet` baseline. Its payload length is still unresolved, so it remains **NO-GO**.

SetFanMode and SetFanLevel add more unknowns than SetFanMax:

- more than one observed input length;
- mode/scale meanings that differ by thermal-policy generation;
- interactions with max-fan state and manual fan floors;
- reference flows with repeated writes, timing dependencies, and optional EC fallback;
- documented V1 reports of fans remaining at maximum or becoming non-responsive after `SetFanLevel(0,0)`.

They are not safer alternatives and must not be used to bypass the SetFanMax gate.

## 8. Candidate Decision

Neither SetFanMode nor SetFanLevel should be considered before SetFanMax is resolved. Neither is a current candidate for implementation, validation, fallback, restore, UI, or hardware experiment.

## 9. Still Forbidden

SetFanMax, SetFanMode, SetFanLevel, `0x37`, fan control, fan-write payloads, EC access, BIOS writes, hardware writes, fan-control UI, automatic retries, polling control loops, and WMI invocation remain forbidden.

## 10. Recommended Next Safe Step

Keep all fan-write code absent. Treat the raw-only read-only telemetry as diagnostic evidence only, and require a separately approved, device-specific evidence package before any future study can move either SetFanMode or SetFanLevel beyond documentation.

## References Reviewed

- ghelper-omen `app/Omen/HpWmiBios.cs`
- omencore `src/OmenCoreApp/Hardware/HpWmiBios.cs`
- omencore `src/OmenCoreApp/Hardware/WmiFanController.cs`
- OmenSuperHub `OmenHardware.cs`
- OmenXHub `OmenHardware.cs`
