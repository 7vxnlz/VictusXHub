# FanGetRpm Read-Only Investigation

## Reference Evidence

Reviewed references:

- `ghelper-omen` commit `1694844d2725e79a2b2065a0a1494fa1d143e3f4`
- `omencore` commit `b39b44978902606aa708cc0d78bcfd87e95fd88b`
- `OmenXHub` commit `ca84cb011d1d3e5850445d19e45fbea06e83a8fd`

`ghelper-omen` defines `FanGetRpm` as command `0x38` and calls it `GetFanRPM direct` for OMEN Max 2025+ systems. Its `GetFanRpmDirect` path sends the default BIOS command with four zero input bytes and expects a 128-byte response, then parses the first four bytes as two 16-bit RPM values with endian and range checks.

`omencore` also names `0x38` as `FanGetRpm`, but explicitly gates it to ThermalPolicy V2/OMEN Max-style systems and warns that V0/V1 systems may return garbage or phantom RPM-shaped values.

`OmenXHub` does not strengthen the `0x38` case for this Victus. Its standard RPM comment points to a separate `0x11` path with 4-byte output and says it is semantically different from the Victus-style `0x2D` level path.

## V1/V2 Specificity

`FanGetRpm` / `0x38` appears V2-specific. The real HP Victus `SystemDesignData` result decoded `ThermalPolicyVersion=1`, so this device does not currently match the reference-backed V2 condition.

## Expected Method And Output Size

If used on a matching V2 system, the expected shape appears to be:

- BIOS command group: default
- Command type: `0x38`
- Input: four zero bytes
- Method: `hpqBIOSInt128`
- Output: 128 bytes
- Known candidate payload: first four bytes only, potentially two 16-bit fan RPM values

## Read-Only Assessment

The command appears read-only in the references because it is used as a get/status path and has no write payload semantics. That is not enough to make it appropriate for this V1 Victus.

## Risks And Ambiguities

- References warn `0x38` can produce garbage or phantom values on V0/V1 systems.
- Plausible numeric output could be misleading if interpreted as RPM on the wrong thermal policy.
- The current Victus has real `FanGetLevel` raw bytes, but those bytes are not proven RPM, percent, fan curve, or control level.
- No real captured `0x38` result exists for this device.

## Decision

Defer `FanGetRpm` / `0x38` preparation for this Victus. It should not be added as a safe read-only invocation candidate now.

## Recommended Alternative Next Step

Either pause HP fan WMI reads and continue consolidating read-only telemetry evidence, or separately investigate the `0x11` standard RPM read-only path as a new candidate before any code preparation. Do not treat `0x11` as approved without its own reference review and safety plan.

## Still Forbidden

Do not invoke WMI from Codex, run `--hp-wmi-readonly-test` from Codex, implement fan control, implement fan writes, add fan speed control, interpret unknown values as RPM/percent/curve/control level, add EC access, add BIOS writes, add hardware writes, add polling loops, add retries, or change ASUS behavior.
