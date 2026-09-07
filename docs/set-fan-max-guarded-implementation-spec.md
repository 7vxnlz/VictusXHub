# Guarded SetFanMax Implementation Specification

This is a specification for a possible future manual experiment implementation. It does not authorize implementation, device testing, or a hardware write.

## 1. Current Status

**NO-GO.** SetFanMax is not implemented, `DeviceValidatedInputLength` is `null`, the dry-run report fails closed, and required device/recovery evidence remains missing or deferred.

## 2. Required Document Review

Before implementation may start, reviewers must approve the current revisions of:

- [Implementation go/no-go gate](set-fan-max-implementation-go-no-go.md)
- [Missing-proof tracker](set-fan-max-missing-proof-tracker.md)
- [Manual validation package](set-fan-max-manual-validation-package.md)
- [Recovery/restore proof plan](set-fan-max-recovery-restore-proof-plan.md)
- [Future runtime flag design](set-fan-max-future-runtime-flag-design.md)
- [Method/input validation](set-fan-max-method-input-validation.md)
- [Payload/preflight plan](set-fan-max-payload-preflight-plan.md)
- [Dry-run report guide](set-fan-max-dry-run-report-guide.md)

Document presence is not approval; evidence references, reviewer identity, date, model/SKU, and BIOS must be attached.

## 3. Evidence Required Before Implementation

Every missing-proof tracker item must be `Proven`. In particular:

- Exactly one device-specific input length (`1` or `4`) with no fallback.
- Length-matched restore/disable behavior proven by `FanMaxGet=false` and baseline match.
- A reviewed, locally executable manual recovery path for this model/BIOS.
- Exact implementation-only human approval plus the separately designed runtime operator confirmation.
- A named independent thermal observation source, observer, baseline, stop thresholds, and availability through recovery.

Conflicting, stale, inferred, or another-model evidence is a failed gate.

## 4. Required Future Runtime Flags

The future process must require all four proposed flags together:

- `--hp-victus`
- `--hp-wmi-readonly-test`
- `--hp-fan-write-test`
- `--set-fan-max-experiment`

These flags are not implemented today. Before runtime work, the older type-only placeholder names must be reconciled atomically with parser, policy, report, and tests while execution remains disabled. Flags cannot replace GO evidence, elevation, or interactive approval.

## 5. Required Pre-Write Readbacks

In the same elevated foreground process, immediately before payload construction:

1. `SystemDesignData` must succeed/decode and match approved device and thermal-policy evidence.
2. `FanGetCount` must succeed/decode with two fans and clear protection status.
3. `FanMaxGet` must succeed/decode with `IsMaxFanEnabled=false`; this is the captured restore target.
4. `FanGetLevel` must succeed/decode and remain raw-only.
5. AC/battery and independent thermal observations must match approved limits.

Any failed, stale, ambiguous, unexpected, or changed result stops before a write.

## 6. Required Write Constraints

- SetFanMax / `0x27` only; no generic write-command surface.
- Exactly one target/enable write attempt, followed by exactly one mandatory restore/disable write path.
- Only the proven input length and reviewed state-byte description; no default, fallback, alternate shape, or executable payload logging.
- No fan curve, percentage, RPM target, fan-speed loop, persistence, background work, or UI access.
- No `SetFanLevel`, `SetFanMode`, ambiguous `0x37`, EC fallback, automatic retry, or second target write.
- Payload construction must occur only after every gate and pre-read succeeds.

The mandatory restore is recovery, not a second target experiment.

## 7. Required Post-Write Readback

Immediately after the single target write attempt, `FanMaxGet` must be invoked and decoded. Record the observed latch state, but never infer success from method return, fan sound, raw fan level, or temperature. Any error, timeout, ambiguous state, or unexpected disabled/enabled result enters the mandatory restore path and prevents another target attempt.

## 8. Required Restore And Verification

Restore/disable must use the same validated input length and reviewed state-byte shape. It must run synchronously in the foreground after every target-write attempt, including cancellation or post-read failure. An immediate `FanMaxGet` must succeed/decode with `IsMaxFanEnabled=false` and match baseline; perform a second read only when the approved evidence defines a settling interval. Failure marks restore unverified, prohibits further writes, and hands control to the documented manual recovery procedure.

## 9. Required Report Fields

The future redacted report must include:

- **Gate/before state:** `SetFanMaxGateDecision`, `SetFanMaxEvidenceRevision`, `SetFanMaxDeviceValidatedInputLength`, `SetFanMaxFlagsSatisfied`, `SetFanMaxHumanApprovalConfirmed`, `ProcessElevated`, `StableAcPower`, `ThermalObservationReady`, device/BIOS/thermal identifiers, and all pre-read invocation/decode/state fields.
- **Target write:** `SetFanMaxTargetWriteAttempted`, `SetFanMaxTargetWriteSucceeded`, `SetFanMaxTargetWriteError`, timestamp/elapsed time, method/command metadata, selected input length, and non-reversible payload-description digest; never raw payload bytes.
- **Post-write readback:** `FanMaxGetAfterWriteInvocationSucceeded`, `FanMaxGetAfterWriteDecodeSucceeded`, `FanMaxGetAfterWriteIsMaxFanEnabled`, errors, and thermal observation.
- **Restore:** `SetFanMaxRestoreAttempted`, `SetFanMaxRestoreSucceeded`, `SetFanMaxRestoreError`, `FanMaxGetAfterRestoreInvocationSucceeded`, `FanMaxGetAfterRestoreDecodeSucceeded`, `FanMaxGetAfterRestoreIsMaxFanEnabled`, `SetFanMaxFinalStateMatchesBaseline`, and completion timestamp.
- **Abort/recovery:** `SetFanMaxAbortPhase`, `SetFanMaxAbortReasons`, `SetFanMaxManualRecoveryRequired`, `SetFanMaxManualRecoveryPerformed`, recovery result, and final known latch/thermal state.

Unknown values must remain null/unknown, not optimistic defaults.

## 10. Required Abort Behavior

Before a write, any failed gate exits with no payload construction or invocation. After a target write attempt, any exception, cancellation, unsafe temperature, power/observer loss, unexpected result, or readback failure suppresses all non-recovery work and enters the single reviewed restore path. Restore failure stops all writes and requires human recovery. Never retry automatically, switch payload length, invoke another command, continue in background, or claim success without final `FanMaxGet=false`.

## 11. UI Fan Control Must Wait

This specification covers one physically supervised experiment, not repeatable product behavior. UI control requires separate evidence for repetition, lifecycle transitions, cancellation, persistence, conflict handling, long-running thermal safety, and reliable recovery. No fan control UI may be designed from one experiment result.

## 12. Final Recommendation

**Do not implement yet.** Keep the gate `NO-GO`, write implemented/allowed false, and all write paths absent until every tracker item is proven and a new user task explicitly authorizes the narrowly scoped implementation.
