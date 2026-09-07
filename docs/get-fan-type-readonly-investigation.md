# GetFanType Read-Only Investigation

## Reference Evidence

Reviewed references:

- `ghelper-omen` commit `1694844d2725e79a2b2065a0a1494fa1d143e3f4`
- `omencore` commit `b39b44978902606aa708cc0d78bcfd87e95fd88b`
- `OmenSuperHub` commit `a6ab6988c446ee5421466097fdf60c0d521e5c81`
- `OmenXHub` commit `ca84cb011d1d3e5850445d19e45fbea06e83a8fd`

`OmenSuperHub` and `OmenXHub` both implement `GetFanType` as a default HP BIOS WMI command using command group `0x20008`, command type `44`, four zero input bytes, and a 128-byte output. They parse the first four bytes as packed 4-bit fan type values and bytes 8-9 as fan capability bits.

`ghelper-omen` and `omencore` do not provide a matching high-confidence `GetFanType` HP BIOS WMI implementation in the reviewed HP WMI files.

## Read-Only Assessment

The `command=0x20008`, `commandType=44`, four-zero-input, 128-byte-output form appears read-only because it is used as a get/info query and does not construct a write payload.

## Expected Method And Output Shape

If later prepared, the expected shape appears to be:

- BIOS command group: default / `0x20008`
- Command type: `44` decimal / `0x2C`
- Input: four zero bytes
- Method: `hpqBIOSInt128`
- Output: 128 bytes
- Known candidate payload: first four bytes as packed fan type nibbles; bytes 8-9 as capability bits

Unknown bytes must remain summarized only.

## V1/V2 Specificity

The references do not clearly mark this as V1-only or V2-only. It may be a broader default-command information query, but that is not proven for this ThermalPolicy V1 Victus.

## Risks And Ambiguities

- `44` / `0x2C` is ambiguous unless paired with the default command group `0x20008`.
- References also use command type `44` with other command groups for legacy clean-state reads and writes.
- `0x2C` appears as an EC fan speed percentage write register in OmenCore, which must not be confused with HP BIOS WMI command type `0x2C`.
- Capability bits may relate to clean/reverse-fan behavior and must not become write/control behavior.
- No real captured `GetFanType` result exists for this Victus.

## Decision

Defer `GetFanType` preparation for now. It is a plausible read-only information command, but it should not be promoted to a safe invocation candidate until the command group, command type, output shape, and raw-only reporting fields are specified in a separate probe plan.

## Still Forbidden

Do not invoke WMI from Codex, run `--hp-wmi-readonly-test` from Codex, prepare `GetFanType` for invocation, implement fan control, implement fan writes, add fan speed control, infer clean/reverse-fan control from capability bits, add EC access, add BIOS writes, add hardware writes, add polling loops, add retries, or change ASUS behavior.

## Recommended Next Task

Create a short `GetFanType` read-only probe plan that keeps it documentation-only or catalog-only until reviewed. The plan should require exact default command group `0x20008`, command type `44`, `hpqBIOSInt128`, four zero input bytes, raw-only decoded fan type/capability summaries, and no control behavior.
