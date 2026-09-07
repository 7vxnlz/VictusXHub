# SetFanMax Payload-Shape Decision Plan

## Current Conflict

`SetFanMax` has two reference-backed input shapes for the same apparent transport: command `0x20008`, command type `0x27`, `hpqBIOSInt0`, and no output.

| Shape | Enable | Restore/disable | References |
| --- | --- | --- | --- |
| One byte | `{ 0x01 }` | `{ 0x00 }` | OmenSuperHub, OmenXHub |
| Four bytes | `{ 0x01, 0x00, 0x00, 0x00 }` | `{ 0x00, 0x00, 0x00, 0x00 }` | ghelper-omen, omencore |

The common state byte is useful reference evidence. The conflicting length is not a compatibility guarantee.

## Victus Evidence So Far

This HP Victus has only proven read-only behavior:

- `ThermalPolicyVersion=1`.
- `FanCount=2`.
- `FanMaxGet` works and reported max fan disabled.
- `FanGetLevel` works, but returns raw-only values.

These facts show a usable read-only WMI path. They do not select a write payload length, establish a max-fan latch, or prove a restore route.

## Decision Criteria

### Choose One Byte Only If

All of the following independently identify this exact model/BIOS and one-byte input:

1. Reviewable device-specific evidence names this Victus model and firmware, command `0x27`, and input length `1`.
2. The evidence identifies the same zero-output WMI method and a matching one-byte restore state.
3. A human-reviewed recovery plan can restore the disabled baseline if the experiment fails.
4. The preflight model records `DeviceValidatedInputLength=1` explicitly; it must never infer it from a reference family.

### Choose Four Bytes Only If

All of the following independently identify this exact model/BIOS and four-byte input:

1. Reviewable device-specific evidence names this Victus model and firmware, command `0x27`, and input length `4`.
2. The evidence identifies the three trailing zero bytes as accepted for both enable and restore.
3. A human-reviewed recovery plan can restore the disabled baseline if the experiment fails.
4. The preflight model records `DeviceValidatedInputLength=4` explicitly; it must never pad by convention.

## Stop Conditions

Stop and retain **NO-GO** if any condition applies:

- Both shapes are claimed, or neither shape is device-specific.
- Evidence is from an OMEN family/model rather than this Victus and BIOS revision.
- The evidence does not cover both enable and restore/disable using the same length.
- `FanMaxGet` pre-read is unavailable, reports an unexpected baseline, or cannot be included in a later post-write verification plan.
- Recovery, elevation, AC power, independent thermal observation, required future flags, or explicit human approval is missing.
- A proposed solution adds `SetFanMode`, `SetFanLevel`, `0x37`, EC access, automatic retries, or UI control.

## Still Forbidden

No WMI write invocation, `SetFanMax` implementation, payload execution, `SetFanMode`, `SetFanLevel`, ambiguous `0x37`, EC access, BIOS writes, hardware writes, fan UI, background control, or change to default ASUS behavior is permitted.

## Recommended Next Safe Step

Keep `DeviceValidatedInputLength` unset and the implementation gate at **NO-GO**. Obtain independently reviewable, model-and-BIOS-specific evidence that selects exactly one shape, then update the missing-proof tracker without adding runtime write code.

Related: [model/BIOS evidence matrix](set-fan-max-model-bios-evidence-matrix.md), [reference fan write flow comparison](reference-fan-write-flow-comparison.md), [method/input validation](set-fan-max-method-input-validation.md), and [missing-proof tracker](set-fan-max-missing-proof-tracker.md). The matrix does not select a payload length; `DeviceValidatedInputLength` remains unset.
