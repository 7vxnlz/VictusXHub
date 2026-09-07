# SetFanMax Implementation Go / No-Go Gate

This gate decides whether the project may open a separately approved guarded implementation task. It never authorizes a fan write.

## 1. Current Status

**NO-GO.** SetFanMax is not implemented, write permission is hardcoded false, and `DeviceValidatedInputLength` is `null`. The dry-run report is correctly fail-closed.

## 2. Proven Prerequisites

- `SystemDesignData` read-only invocation and decode succeeded.
- `FanGetCount` reports two fans with clear protection status.
- `FanMaxGet` reports max fan disabled.
- Raw-only `FanGetLevel` readback succeeded.
- The no-write simulator, preflight model, dry-run report, and manual validation package exist.
- Normal `--hp-victus` mode leaves all explicit invocation attempts false.

These prerequisites prove read-only diagnostics and safety scaffolding only.

## 3. Missing Proof

- Exactly one device-specific SetFanMax input length, `1` or `4`.
- Matching restore/disable behavior for that same length, verified by `FanMaxGet`.
- A safe, reviewed, locally executable manual recovery path.
- Explicit human confirmation of the evidence, recovery plan, and implementation-only scope.

Recovery and restore evidence must satisfy [the SetFanMax recovery and restore proof plan](set-fan-max-recovery-restore-proof-plan.md). Until then, the gate remains **NO-GO**.

The [final payload-length audit](set-fan-max-payload-length-final-audit.md) confirms that neither the one-byte nor four-byte reference evidence validates this exact device. `DeviceValidatedInputLength` therefore remains unset.

The [SetFanMax proof gap checklist](set-fan-max-proof-gap-checklist.md) is the source of truth for the remaining implementation and UI evidence. Any unchecked item keeps this gate **NO-GO**.

Any evidence submitted for reconsideration must satisfy the [manual evidence capture package](set-fan-max-manual-evidence-capture.md). An incomplete package, or one requiring inference, keeps this gate **NO-GO**.

## 4. GO Conditions

Every condition below must be true and supported by attached evidence:

1. Exactly one input length is device-validated; neither zero nor two selections are acceptable.
2. Enable and restore descriptions use that same length.
3. Restore/disable is proven to return `FanMaxGet` to the original disabled state.
4. Manual recovery is documented, independently reviewed, locally available, and accepted by the operator.
5. Current device identity, BIOS, thermal policy, fan count, protection state, AC power, and thermal observation match the approved baseline.
6. The dry-run has no blocked reasons, while write implemented/allowed remain false before implementation.
7. The required human approval wording is recorded verbatim.
8. A separate task explicitly authorizes only the constrained implementation described below.

Meeting these conditions permits implementation review only, not execution or device testing.

## 5. NO-GO Conditions

The decision is **NO-GO** if any proof is missing, stale, ambiguous, conflicting, inferred from another model, or not independently reviewable. It is also NO-GO for an unknown/enabled `FanMaxGet` baseline, missing recovery access, unsafe temperature or power, absent flags/elevation, UI or background execution, retries, multiple writes, payload-length fallback, or any request broader than SetFanMax.

Any uncertainty resolves to NO-GO.

## 6. Required Human Approval Wording

The reviewer must record this exact statement:

> I reviewed the device-specific SetFanMax input-length evidence, matching restore proof, FanMaxGet baseline and verification plan, and manual recovery procedure for this exact Victus and BIOS. I approve a constrained implementation-design task only. This approval does not authorize a hardware write, device test, fan-control UI, retry, or background execution.

The reviewer name, date, device/BIOS identifiers, and evidence references must accompany the statement.

## 7. Required Future Runtime Flags

A future implementation must require all of these together:

- `--hp-victus`
- `--hp-wmi-readonly-test`
- `--hp-fan-write-test`
- `--set-fan-max-experiment`

Administrator elevation and a foreground interactive confirmation are also mandatory. Flags and elevation never override a failed gate. The exact future contract is defined by [the guarded implementation specification](set-fan-max-guarded-implementation-spec.md); it remains documentation-only while this gate is **NO-GO**.

## 8. First Implementation Constraints

The first separately approved implementation must be SetFanMax-only, single-command, single-attempt, foreground-only, and unavailable to normal startup or UI. It must accept only the one proven input length, have no fallback/default shape, reject every other write command, perform a fresh `FanMaxGet` pre-read, record redacted metadata, perform immediate readback, and enter mandatory restore handling on every exit path. No automatic retry, polling control loop, persistent setting, or reusable general write API is allowed.

## 9. Required Rollback Behavior

The exact restore/disable operation must be defined and reviewed before enable is possible. Restore must use the same validated length, run synchronously in the foreground, and be followed by `FanMaxGet` confirming disabled and matching the captured baseline. Failure, ambiguity, timeout, cancellation, or exception must mark restore unverified, stop all further writes, and direct the human to the approved manual recovery path. No speculative alternate payload or EC fallback is permitted.

## 10. Final Recommendation

**Remain NO-GO.** Keep SetFanMax docs/type-only and write implemented/allowed false. Do not open a guarded implementation task until the device-specific input length, matching restore behavior, manual recovery path, and exact human approval are all attached and independently reviewed.
