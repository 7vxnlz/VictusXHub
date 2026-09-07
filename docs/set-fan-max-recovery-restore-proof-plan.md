# SetFanMax Recovery And Restore Proof Plan

This plan defines evidence required before SetFanMax may be reconsidered for guarded implementation design. It does not authorize or implement a write.

## 1. Why Proof Is Required

SetFanMax appears to change a max-fan latch. A successful method return would not prove the latch changed, remained bounded, or can be cleared. Recovery and restore must therefore be device-specific, observable through `FanMaxGet`, and proven before write-capable project code is considered.

## 2. Expected Restore Target

The captured baseline and mandatory restore target are **max fan disabled**: `FanMaxGet` must decode successfully with `IsMaxFanEnabled=false`. An unknown, ambiguous, or enabled final state is not restored.

## 3. Required Pre-Write Readback

Immediately before any future experiment, `FanMaxGet` must:

- Be invoked through the approved read-only path.
- Succeed with the expected byte count and no invocation/decode errors.
- Report `IsMaxFanEnabled=false`.
- Be recorded with timestamp, model/SKU, BIOS, thermal policy, selected input length, AC-power state, and independent temperature observation.

Failure or an enabled/unknown baseline stops the experiment.

## 4. Required Post-Write Readback

After a separately approved future enable attempt, an immediate `FanMaxGet` must record invocation/decode success and the observed latch state. It must not infer success from fan noise, raw fan levels, method return alone, or temperature. An unexpected result stops further experimentation and enters the approved restore procedure.

## 5. Required Restore Readback

The restore/disable action must use the same device-validated input length as enable. Immediately afterward, `FanMaxGet` must succeed, decode without errors, report `IsMaxFanEnabled=false`, and match the captured baseline. A second confirmation after the documented settling interval is required if device-specific evidence establishes such an interval. No guessed delay, alternate payload shape, or silent fallback is allowed.

## 6. Manual Recovery If Restore Fails

Recovery steps are containment until `FanMaxGet` proves the disabled state:

1. **App exit:** close the foreground experiment and stop all further writes. Exit does not prove the firmware latch reset.
2. **Rerun restore command:** only if a future implementation exists and the reviewed recovery procedure explicitly authorizes one human-initiated restore action with the proven payload shape. No automatic retry or alternate payload is allowed. Follow with `FanMaxGet`.
3. **Reboot:** perform a controlled reboot if restore remains unverified. Reboot is not proof by itself; run the approved read-only `FanMaxGet` after startup.
4. **BIOS/firmware reset path:** use only an HP-documented recovery/reset procedure verified for this exact model and BIOS. Record the source and steps before the experiment. Never improvise firmware settings, EC actions, power sequencing, or a generic reset assumption. Confirm the final state with `FanMaxGet` when the system is stable.

If no approved recovery option produces a successful disabled readback, stop, preserve the redacted evidence, and prohibit further writes.

## 7. Abort Conditions

Abort for missing/conflicting input length, missing restore proof, failed or ambiguous readback, max fan already enabled, changed device/BIOS/thermal baseline, protection fault, unstable AC power, unavailable temperature observation, unsafe temperature, exception, cancellation, UI shutdown, absent human operator, missing recovery access, unexpected method result, or any request for another control command. Any uncertainty is an abort.

## 8. Evidence Required Before GO Reconsideration

Attach all of the following for independent human review:

- Exact model/SKU, BIOS version, thermal policy, date, operator, and reviewer.
- One selected device-specific input length (`1` or `4`) and evidence rejecting the other as the active shape.
- Redacted method/command and enable/restore payload-description metadata for that length.
- Timestamped successful `FanMaxGet` results before enable, after enable, after restore, and after any recovery step.
- Proof that restore returns the latch to disabled and matches baseline.
- The locally available manual recovery procedure, its authoritative source, and the outcome of any required recovery validation.
- Temperature/power observations, errors, aborts, and the exact required human approval statement.

Evidence from another model, an unverified anecdote, method return alone, or fan sound is insufficient. The project must not generate missing write evidence until a separate authorization explicitly permits such an experiment.

## 9. Why Fan UI Must Wait

A UI implies repeatable, recoverable control across cancellation, shutdown, and user error. Input shape, restore behavior, persistence, recovery, and safe repetition are not proven. UI fan control must remain absent even if one manual experiment is eventually approved.

## 10. Still Forbidden

SetFanMax execution, payload execution, automatic restore/retry, fan UI/control, `SetFanMode`, `SetFanLevel`, ambiguous `0x37`, EC access, BIOS writes, hardware writes, fan-speed automation, background operation, and changes to default ASUS behavior remain forbidden. The implementation gate remains **NO-GO**.
