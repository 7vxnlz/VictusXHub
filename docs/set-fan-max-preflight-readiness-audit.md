# SetFanMax Preflight Readiness Audit

## Current Proven Prerequisites

- Read-only `SystemDesignData` succeeded and reports `ThermalPolicyVersion=1` with software fan-control support.
- `FanGetCount` reports two fans with clear protection status.
- `FanMaxGet` succeeds and reports max fan disabled.
- `FanGetLevel` is available as raw-only data.

## Current Preflight Model

The pure model allows only `SetFanMax` / `0x27`, an initial enable target, and a restore/disable target. Its four-field description is `1,0,0,0` for enable and `0,0,0,0` for restore/disable. It has no WMI reference, payload buffer, allowlist entry, runtime consumer, or execution path.

The audit hardened two defects: callers can no longer supply a permissive policy, and restore/disable cannot be approved as the initial write.

## Required Gates

All explicit write flags, elevation, an interactive human confirmation, an approved device/firmware baseline, a healthy read-only baseline, stable AC power, independent thermal observation, and a single-write limit must be present. The normal application path remains write-free.

## Readback, Restore, And Verification

Immediately before a future attempt, `FanMaxGet` must succeed and report disabled. The plan must require enable as the initial target, post-write `FanMaxGet` verification, restore/disable, and a second `FanMaxGet` verification after restore. No retry, inferred auto mode, EC fallback, or background cleanup is allowed.

## Failure And Abort Conditions

Any missing gate, unknown or enabled pre-read state, non-`SetFanMax` command, missing post-read/restore-read plan, non-single-write request, exception, unexpected readback, cancellation, UI shutdown, or failed restore verification blocks or aborts the experiment.

## Remaining Risks And Readiness

The code is not ready for guarded write implementation. The model cannot prove the device-specific WMI method/input contract, latch persistence behavior, actual restore behavior, human supervision, thermal safety, or manual recovery. A theoretical preflight approval remains inert because execution is permanently disabled and disconnected from runtime.

## Exact Next Step

Perform a human-reviewed, documentation-only validation of the device-specific `SetFanMax` restore behavior and manual recovery procedure. Require explicit implementation approval only after that evidence exists; do not add a write path yet.
