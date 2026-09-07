# SetFanMax First-Write Runner Safety Audit

## Scope And Result

Source-level review found the developer-only runner is command-line-only and unreachable from the Diagnostic UI, tray menu, and normal startup unless `--hp-fan-write-experiment` is present. Normal fan write readiness remains **NO-GO**. The documented second four-byte confirmation requires both `--i-approve-one-time-set-fan-max-4-byte-experiment` and `--i-approve-second-set-fan-max-4-byte-confirmation`; both are rejected for the one-byte hypothesis and neither validates a payload.

## Verified Gates

- Requires `--hp-victus`, `--hp-wmi-readonly-test`, `--hp-fan-write-experiment`, one `--set-fan-max-payload-length=1` or `=4`, and `--i-understand-this-can-affect-fans`. The documented four-byte confirmation additionally requires both explicit four-byte approvals, including `--i-approve-second-set-fan-max-4-byte-confirmation`.
- Rejects missing, invalid, duplicate, dry-run, and baseline-capture combinations.
- Requires Administrator elevation and confirmed local AC power; offline or unknown power blocks before baseline capture.
- Requires HP Victus identity, SKU `7Z5Z2EA#AB8`, BIOS `F.31`, thermal policy V1, successful decoded baseline probes, fan count `2`, `FanMaxGet=false`, and nonempty raw FanGetLevel data.
- Captures the approved read-only baseline before the write boundary. It permits one selected hypothesis only, with no retry or alternate-payload fallback.

## Write And Recovery Scope

The runtime transport fixes the only possible command to `0x20008`, type `0x27`, class `hpqBIntM`, method `hpqBIOSInt0`, and the exact paired one-byte or four-byte enable/restore buffers. It has no SetFanMode, SetFanLevel, `0x37`, EC, fan-curve, or performance-control route.

After an enable attempt, the runner waits once, captures FanMaxGet/raw FanGetLevel, then attempts the matching restore in `finally` and captures the same readback again. The audit hardened the exceptional path: a managed transport exception is now treated as an uncertain attempted enable, so matching restore is still attempted once. Append-only `CreateNew` JSON logging preserves baseline, attempts, readbacks, outcome, and blocked/failure reasons.

## Current Boundary

The runner was not executed for this audit, and no WMI method was invoked. The payload length remains unselected, `DeviceValidatedInputLength` remains null, and normal fan control remains absent. The narrow approval flag does not change those facts or authorize a control UI.

After this source audit, one manually reviewed four-byte experiment was performed separately. Its successful WMI returns and observed fan ramp are recorded as partial evidence in the [first four-byte result](set-fan-max-first-4byte-experiment-result.md); FanMaxGet did not confirm enabled state, so this audit's NO-GO conclusion for normal use remains unchanged.

## Recommended Next Safe Task

Review any future exact-device evidence against the first-write decision gate; retain the runner unexecuted while the gate is **NO-GO**.
