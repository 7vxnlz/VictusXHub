# Next Fan Read-Only Probe After FanMaxGet

## Candidate Comparison

| Candidate | Known shape | Selection note |
| --- | --- | --- |
| `FanGetLevel` / `0x2D` | Default command, 4-byte zero input, 128-byte output | Best next candidate for this ThermalPolicy V1 Victus, but decoded values must stay raw until units are proven. |
| `FanGetRpm` / `0x38` | Default command, 4-byte input, 128-byte output; references parse first words as RPM | Useful if supported, but appears more V2/OMEN Max specific than `FanGetLevel`. |
| `GetFanType` / `0x2C` or `44` | Default command, 4-byte zero input, 128-byte output | Read-shaped, but command numbering is ambiguous across reference command families. |
| `GetFanTable` / `0x2F` | Default command, 4-byte zero input, 128-byte output | Larger table/curve payload with unclear semantics; not a minimal status probe. |

## Selected Next Probe

Select `FanGetLevel` / `0x2D` as the next HP fan read-only status probe to prepare.

## Why Selected

`FanGetLevel` is the next smallest conceptual step after proving fan count and max-fan latch status: it should provide per-fan status bytes for the two known fans without entering write/control command space. It also better matches the known ThermalPolicy V1 device than the likely V2-specific RPM path.

## Why Others Are Deferred

- `FanGetRpm` / `0x38`: defer until there is stronger evidence this Victus firmware supports the V2/OMEN Max RPM readback path.
- `GetFanType` / `0x2C` or `44`: defer because command `44` appears ambiguous in reference material.
- `GetFanTable` / `0x2F`: defer because fan table or curve payloads are larger and easier to misinterpret.

## Expected Output Shape

Expected input is four zero bytes. Expected output is 128 bytes, with early bytes likely carrying per-fan raw level/status values. For this two-fan device, only clearly supported per-fan raw values should be surfaced; the rest must remain summarized as unknown tail data.

## Risks And Ambiguities

The returned values may represent level, estimated RPM, firmware readback, or another model-specific unit. A decoder must not label values as speed, percent, RPM, target, control level, or fan curve data unless later evidence proves that meaning.

## Required Gates Before Future Invocation

- `--hp-victus`
- `--hp-wmi-readonly-test`
- elevated Administrator process
- `SafeReadOnlyInvocation` catalog entry
- exact command ID `0x2D`
- expected method and 128-byte output shape
- single-shot manual invocation only
- sanitized report fields only, with no full raw binary log

## Still Forbidden

Fan control, fan writes, fan speed control, `SetFanLevel`, `SetFanMode`, `FanMaxSet` / `0x27`, ambiguous `0x37`, performance mode control, EC access, BIOS writes, hardware writes, polling loops, retries, and ASUS behavior changes remain forbidden.

## Recommended Next Code Step

Prepare `FanGetLevel` as a gated read-only catalog candidate with a pure decoder scaffold and synthetic tests that expose only raw per-fan values plus summarized unknown tail data. Do not invoke it from Codex.
