# SetFanMax Manual Validation Package

This package is a final review checklist before any future guarded SetFanMax implementation. It does not authorize or execute a write.

## 1. Proven Read-Only Baseline

- `SystemDesignData` succeeded and reports software fan-control support on this Victus.
- `FanGetCount` succeeded with `FanCount=2` and clear protection status.
- `FanMaxGet` succeeded and reports max fan disabled.
- `FanGetLevel` succeeded; its values remain raw-only.
- Normal `--hp-victus` mode writes the capability report while all explicit read-only invocations remain `Attempted=false`.

## 2. Current No-Write Report

The report exposes `SetFanMaxWriteImplemented=false`, `SetFanMaxWriteAllowed=false`, `SetFanMaxDryRunEvaluated=true`, `SetFanMaxDeviceValidatedInputLength=null`, `SetFanMaxDryRunBlockedReasons`, and `SetFanMaxNextRequiredProof`. These fields describe evidence only and cannot enable execution.

## 3. Missing Proof

All of the following remain required:

- Device-specific proof selecting exactly one input length: `1` or `4`.
- Proof that restore/disable uses the same selected length and reliably returns `FanMaxGet` to disabled.
- A reviewed, locally executable manual recovery path if restore cannot be confirmed.
- Human confirmation of the reference review, selected length, recovery plan, and single-test risk.

Reference behavior from another HP model is not device proof.

## 4. Why Writes Remain Impossible

No SetFanMax executor, write payload builder, write-command allowlist entry, WMI write call, runtime registration, or fan-control UI exists. The decision simulator can only permit a later design step. The dry-run report hardcodes write implemented and write allowed to `false`, even when simulated evidence is complete.

## 5. Future Runtime Flags

Any separately approved future write implementation must require all four flags together:

- `--hp-victus`
- `--hp-fan-write-experiment`
- `--hp-wmi-write-manual-test`
- `--hp-fan-write-acknowledge-risk`

It must also require Administrator elevation and an interactive foreground confirmation. Flags and elevation alone must never authorize a write.

## 6. Required Report Before Write

Before any future write attempt, a redacted report must record:

- Timestamp, exact model/SKU, BIOS version, thermal policy version, AC-power state, and independent thermal observation status.
- Successful current `SystemDesignData`, `FanGetCount`, `FanMaxGet`, and raw-only `FanGetLevel` readbacks with decode errors empty.
- `FanMaxGet` immediately before the write, with max fan confirmed disabled.
- `SetFanMaxWriteImplemented`, `SetFanMaxWriteAllowed`, `SetFanMaxDryRunEvaluated`, `SetFanMaxDeviceValidatedInputLength`, `SetFanMaxDryRunBlockedReasons`, and `SetFanMaxNextRequiredProof`.
- Presence of every required flag, elevation, interactive confirmation, approved recovery plan, and single-attempt limit.
- Selected method/command metadata, selected input length, state-byte description, zero-tail count, and a non-reversible payload-description digest; never the executable raw payload.

No write may start unless blocked reasons are empty and a separate implementation review explicitly authorizes the experiment.

## 7. Required Report After Restore

A future implementation must record these proposed fields after its mandatory restore phase:

- `SetFanMaxRestoreAttempted`
- `SetFanMaxRestoreSucceeded`
- `SetFanMaxRestoreError`
- `FanMaxGetAfterRestoreInvocationSucceeded`
- `FanMaxGetAfterRestoreDecodeSucceeded`
- `FanMaxGetAfterRestoreIsMaxFanEnabled` (must be `false`)
- `SetFanMaxFinalStateMatchesBaseline`
- `SetFanMaxManualRecoveryRequired`
- Final temperature-observation status and completion timestamp

Missing, failed, ambiguous, or enabled final readback means restore is unverified and manual recovery is required.

## 8. Stop / Proceed Checklist

- [ ] Exactly one device input length is proven as `1` or `4`; if neither or both are claimed, **stop**.
- [ ] Restore/disable proof matches that exact length; otherwise **stop**.
- [ ] A fresh `FanMaxGet` readback succeeds and reports disabled; otherwise **stop**.
- [ ] Device identity, BIOS, thermal policy, fan count, protection status, AC power, and temperatures match the approved baseline; otherwise **stop**.
- [ ] Manual recovery is documented, reviewed, locally available, and accepted by the present operator; otherwise **stop**.
- [ ] All future flags, Administrator elevation, interactive confirmation, single-attempt limit, post-read, and restore-read plans are present; otherwise **stop**.
- [ ] Dry-run blocked reasons are empty, while write implemented/allowed remain false before implementation approval; otherwise **stop**.
- [ ] A separate human review explicitly approves a narrowly scoped implementation task; only then **proceed to implementation design**, not execution.

Any exception, cancellation, UI shutdown, unsafe temperature, unexpected return, failed readback, or failed restore ends the experiment and triggers the approved recovery procedure.

## 9. Still Forbidden

SetFanMax execution, `SetFanMode`, `SetFanLevel`, ambiguous `0x37`, automatic retries, background writes, fan-speed UI/control, EC access, BIOS writes, hardware writes, performance control, battery control, and any change to default ASUS behavior remain forbidden.

## 10. Next Step After Review

Only after this package is reviewed and every missing device-specific proof is attached, open a separate explicitly approved task to design a single-command, foreground-only SetFanMax implementation with mandatory pre-read, immediate post-read, synchronous restore, final readback, and fail-closed reporting. Do not implement it as part of this package.
