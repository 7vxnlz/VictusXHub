# Next Fan Read-Only Probe Selection

## References Reviewed

- `ghelper-omen` `1694844d2725e79a2b2065a0a1494fa1d143e3f4`: `app/Omen/HpWmiBios.cs`
- `omencore` `b39b44978902606aa708cc0d78bcfd87e95fd88b`: `src/OmenCoreApp/Hardware/HpWmiBios.cs`, `WmiFanController.cs`
- `OmenSuperHub` `a6ab6988c446ee5421466097fdf60c0d521e5c81`: `OmenHardware.cs`
- `OmenXHub` `ca84cb011d1d3e5850445d19e45fbea06e83a8fd`: `OmenHardware.cs`

## Candidate Comparison

| Candidate | Shape seen in references | Selection note |
| --- | --- | --- |
| `FanGetRpm` / `0x38` | Default command, 4-byte input, 128-byte output; parsed from first 4 bytes as two RPM words | Read-only-looking, but references treat it as V2/OMEN Max specific. This Victus decoded `ThermalPolicyVersion=1`, so it is not the safest next probe. |
| `FanGetLevel` / `0x2D` | Default command, 4-byte zero input, 128-byte output; first bytes used as fan levels | Useful for V1/Victus status, but units may be level, estimated RPM, or commanded/readback value depending on model. |
| `FanMaxGet` / `0x26` | Default command, 4-byte zero input, 4-byte output; first byte interpreted as max-fan latch state | Safest next probe: smallest output, simple status field, and separate from the forbidden `0x27` write. |
| `GetFanType` / `0x2C` or `44` | Default command, 4-byte zero input, 128-byte output; nibbles interpreted as fan types | Ambiguous because command type `44` is also used in other command families around legacy cleaning state read/write behavior. |
| `GetFanTable` / `0x2F` | Default command, 4-byte zero input, 128-byte output | Ambiguous table/curve payload, not a minimal status probe. |

## Safest Next Probe

Select `FanMaxGet` / `0x26` as the next read-only fan status probe to prepare.

## Why It Is Safer

It has the smallest known response shape after `FanGetCount`, reports only a latch/status bit, and avoids the V2-only uncertainty of `FanGetRpm`, the unit ambiguity of `FanGetLevel`, and the larger structured payloads of fan type/table reads.

## Expected Output Shape

Expected input is four zero bytes. Expected output is 4 bytes from `hpqBIOSInt4`; references interpret byte `0` as whether max-fan mode is currently active. Unknown bytes should remain summarized only.

## Risks And Ambiguities

`FanMaxGet` is adjacent to the forbidden `FanMaxSet` / `0x27` control path, so the future implementation must keep names, command IDs, report fields, and tests clearly separated. It does not prove fan speed, fan level, RPM readback, fan curves, or control safety.

## Required Gates Before Future Invocation

- `--hp-victus`
- `--hp-wmi-readonly-test`
- elevated Administrator process
- `SafeReadOnlyInvocation` catalog entry
- exact command ID `0x26`
- method/output-size match for `hpqBIOSInt4`
- single-shot invocation only
- sanitized report fields only, with no raw binary log

## Still Forbidden

Fan control, fan writes, fan speed control, `FanMaxSet` / `0x27`, `SetFanLevel`, `SetFanMode`, `FanGetLevelV2` / `0x37`, performance mode control, EC access, BIOS writes, hardware writes, polling loops, retries, and ASUS behavior changes remain forbidden.

## Recommended Next Code Step

Prepare a pure `FanMaxGet` report decoder and synthetic tests, then promote only `FanMaxGet` / `0x26` from `ReadIntent` to a gated `SafeReadOnlyInvocation` candidate in a separate reviewed change. Do not invoke it from Codex.
