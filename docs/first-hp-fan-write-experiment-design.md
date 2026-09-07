# First HP Fan Write Experiment Design

This is a safety design only. It does not authorize code, WMI invocation, or a hardware write.

`HpFanWriteSafetyPolicy`, `HpFanWritePreflightResult`, and `HpFanMaxWriteExperimentPlan` now preserve these requirements as type-only, default-blocked scaffolding. `HpFanMaxPayloadDescription` and its pure preflight evaluator describe the reference-shaped fields without creating a payload or invocation path.

## Candidate Comparison

| Command | Future first-experiment suitability | Reason |
| --- | --- | --- |
| `SetFanMax` / `0x27` | Conditional candidate | A binary max-fan latch has a reference-shaped request and an already-proven companion readback, `FanMaxGet` / `0x26`. |
| `SetFanMode` / `0x1A` | Defer | It changes a coupled thermal/performance policy. This V1 Victus has no proven readback for the selected mode. |
| `SetFanLevel` / `0x2E` | Defer | Fan order, units, floor, ceiling, timeout, and return-to-auto behavior are not proven. References also require periodic refresh on some devices. |
| `0x37` | Block | It is a V2 fan-level command in one reference context and a write-like power-limit command in another. It is unsafe to classify for this V1 device. |

The conditional future candidate is `SetFanMax` only: enable max fan once, confirm it, then restore the captured latch state. It is not a safe general-control feature and remains forbidden until a separate implementation approval.

## Required Readback Before Write

Immediately before any future run, the elevated foreground process must capture successful, matching read-only results for:

- `SystemDesignData`: expected device identity, `ThermalPolicyVersion=1`, and software fan support.
- `FanGetCount`: `FanCount=2` and protection status clear.
- `FanMaxGet`: the current latch state to restore exactly.
- `FanGetLevel`: raw bytes only, with no interpretation.

Abort when any result is missing, stale, has an unexpected length or decode error, reports protection, or differs from the approved device/BIOS fingerprint.

## Required Write Constraints

The future experiment must use one fixed, reviewed request shape only: the reference documents `SetFanMax` as a four-byte buffer with a first-byte boolean and all remaining bytes zero. The approved experiment must independently verify the required method and input size for this exact device before use.

- One `enable` attempt only; no configurable values, loops, retries, polling, or background work.
- All unused bytes must be zero; no caller-supplied trailing data.
- No concurrent HP tooling or vendor fan utility may be active.
- The laptop must be on AC power, locally supervised, and independently monitored for temperature and audible fan response.

## Required Readback And Restore

After the single write attempt, read `FanMaxGet` once and require an explicit enabled result. If it does not confirm, abort and report the final state as unknown; do not retry.

Before the experiment is allowed, the matching clear-latch request must be separately established as the only restore behavior. Restore must run synchronously in the same foreground process, restore the exact pre-write `FanMaxGet` state, then be verified by `FanMaxGet` and the approved read-only baseline. Do not infer an auto mode, issue `SetFanMode`, use EC access, or run automatic cleanup after process failure.

## Abort Conditions

Abort before writing for a failed runtime gate, missing elevation, unconfirmed human approval, a non-clear protection result, unsuitable power or thermal conditions, unknown baseline state, an unproven restore request, or any second requested write.

Abort after the attempt for a BIOS error, exception, failed readback, unexpected state, cancellation, UI shutdown, unsafe thermal observation, or failed restore confirmation. Record the final hardware state as known restored, known not restored, or unknown without issuing fallback writes.

## Required Future Runtime Flags

All of these must be present simultaneously, with an interactive foreground confirmation that repeats the fixed command and expected latch transition:

- `--hp-victus`
- `--hp-fan-write-experiment`
- `--hp-wmi-write-manual-test`
- `--hp-fan-write-acknowledge-risk`

An elevated Administrator process and a dedicated write-only allowlist are also mandatory. `SafeReadOnlyInvocation` must never authorize this path.

## Why UI Fan Control Must Wait

This design describes one reversible-looking latch experiment, not durable manual fan control. It does not establish safe manual levels, fan ordering, units, timing behavior, watchdog behavior, a reliable return-to-auto sequence, or failure recovery. No UI, saved setting, service, shortcut, or automation may expose fan control.

## Recommended Next Concrete Step

Perform a documentation-only review that establishes the exact `SetFanMax` clear-latch request and a model-specific, readback-verified restore procedure. Then require an explicit implementation approval before adding any write-capable code.

Reference evidence was reviewed from `ghelper-omen` `1694844d2725e79a2b2065a0a1494fa1d143e3f4`; its command shapes are research evidence, not implementation authority.
