# SetFanMax Exact-Device Baseline Evidence

## Status

This document records manually captured, elevated, read-only baseline evidence for the target HP Victus. No SetFanMax write was executed. `DeviceValidatedInputLength` remains unset, neither payload length is validated, and first-write readiness remains **NO-GO**.

## Device Identity

- Model: `Victus by HP Gaming Laptop 16-s0xxx`
- SKU: `7Z5Z2EA#AB8`
- BIOS: `F.31`
- Thermal policy version: `1`

## Baseline Results

| Field | Four-byte hypothesis | One-byte hypothesis |
| --- | --- | --- |
| Candidate | `FourByteHypothesis` | `OneByteHypothesis` |
| Hypothesis bytes | `01-00-00-00` | `01` |
| Command metadata | `0x20008` / type `0x27` | `0x20008` / type `0x27` |
| WMI metadata | `hpqBIntM` / `hpqBIOSInt0` | `hpqBIntM` / `hpqBIOSInt0` |
| Fan count | `2` | `2` |
| Max fan enabled | `false` | `false` |
| FanGetLevel raw prefix | `22-25` | `22-25` |
| Baseline captured | `true` | `true` |
| Write executed | `false` | `false` |
| First-write gate satisfied | `false` | `false` |
| Device-validated input length | `null` | `null` |

The command, payload, class, and method values above are experiment-hypothesis metadata only. Neither SetFanMax payload was submitted.

## Read-Only Probe Results

| Probe | Attempted | Succeeded | Decoded | Bytes |
| --- | --- | --- | --- | --- |
| `SystemDesignData` | Yes | Yes | Yes | 128 |
| `FanGetCount` | Yes | Yes | Yes | 4 |
| `FanMaxGet` | Yes | Yes | Yes | 4 |
| `FanGetLevel` | Yes | Yes | Yes | 128 |

The captured baseline is two fans, max-fan mode disabled, and raw FanGetLevel prefix `22-25`. Raw values are not RPM, percentages, curve points, or control levels.

## What This Proves

- The exact SKU and BIOS can complete all four approved read-only probes in the elevated baseline path.
- The device identity and thermal policy decode match the intended F.31/V1 target.
- Both hypothesis-labelled records start from the same read-only fan baseline.
- The logging path preserves `WriteExecuted=false`, `FirstWriteGateSatisfied=false`, and `DeviceValidatedInputLength=null`.

## What This Does Not Prove

- It does not show whether SetFanMax accepts a one-byte or four-byte input.
- It does not prove enable behavior, return codes, latch behavior, restore/disable behavior, thermal impact, power-state safety, failure recovery, or human approval.
- It does not permit SetFanMax implementation, execution, normal fan control UI, retries, or payload fallback.

## Readiness Decision

This evidence is not enough to implement normal fan control UI and is not enough to implement or run a first-write runner. It is enough to refine a **documentation-only**, developer-only first-write runner design around the proven baseline and explicit remaining blockers. That design must select no payload length, contain no write path, and preserve **NO-GO** until the [first-write decision gate](set-fan-max-first-write-decision-gate.md) is changed with independently reviewed write/restore, thermal/power, recovery, and approval evidence.

The exact state sequence and fail-closed boundaries are defined in the [first-write experiment runner design](set-fan-max-first-write-experiment-runner-design.md). It is not an implementation or execution authorization.

## Recommended Next Step

Keep the [first-write experiment runner design](set-fan-max-first-write-experiment-runner-design.md) documentation-only and obtain independent review of the remaining write, restore, thermal/power, recovery, rollback, and approval gaps.
