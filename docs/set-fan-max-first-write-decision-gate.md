# SetFanMax First-Write Decision Gate

## Current Status

**NO-GO for normal fan-write readiness and UI.** `DeviceValidatedInputLength` is unset. The repeated exact-device response supports a limited **developer-only experimental GO** for four byte. A separate one-time approval can now permit one bounded one-byte comparison experiment, based on generic-family reference evidence and reported external behavior, but neither route validates a payload, changes this decision for normal use, or authorizes a fan-control UI.

## Known Evidence

- The exact target is HP Victus `16-s0035nt`, SKU `7Z5Z2EA#AB8`, BIOS `F.31`, thermal policy V1, with two fans.
- Read-only `SystemDesignData`, `FanGetCount`, `FanMaxGet`, and raw-only `FanGetLevel` succeeded.
- Two elevated exact-device baseline records captured the same state: fan count `2`, `FanMaxGet=false`, and raw FanGetLevel prefix `22-25`; both kept `WriteExecuted=false` and `DeviceValidatedInputLength=null`.
- References support `hpqBIOSInt0`, command `0x20008`, type `0x27`, enable byte `0x01`, and restore/disable byte `0x00`.
- Two exact-device four-byte experiments returned success for enable and restore and each produced an observed fan ramp, but FanMaxGet remained `false`; see the [second four-byte confirmation result](set-fan-max-second-4byte-confirmation-result.md). Four byte is preferred for limited developer-only experimental evidence only.
- One exact-device one-byte comparison returned success for enable and restore with observed increased airflow, but FanMaxGet also remained `false`; see the [one-byte comparison result](set-fan-max-1byte-comparison-result.md). It is comparison evidence only.

## Required GO Evidence

Every item must be independently reviewable and traceable to the exact device and BIOS above:

1. **Payload length:** one exact-device record proves either the one-byte or four-byte shape, with no guess, fallback, retry, or alternate-shape experiment.
2. **Restore:** the selected shape has a matching restore/disable result and final `FanMaxGet=false` readback matching the baseline.
3. **Thermal and power state:** stable AC, documented battery reserve, independent thermal baseline/limits, and defined stop behavior for power transitions are evidenced through restore.
4. **Failure and recovery:** timeout, exception, cancellation, ambiguous readback, power loss, and failed restore each have a reviewed abort and locally available recovery path.
5. **Human approval:** an independent reviewer records the evidence references, exact identity, conflicts, and approval for a separately scoped implementation-design task.

## If The Gate Later Becomes GO

Any future implementation remains constrained to SetFanMax only, one bounded target action, exact validated input length, pre/post/restore `FanMaxGet` readbacks, fail-closed aborts, no retry/fallback, no background operation, and no UI exposure. SetFanMode, SetFanLevel, `0x37`, EC paths, performance control, and fan controls remain outside scope.

## Decision

The gate is currently **not satisfied**. No write code, payload selection, or hardware experiment may be added until this document is explicitly changed to **GO** with the required evidence. The [first-write experiment runner design](set-fan-max-first-write-experiment-runner-design.md) now documents the future sequence only; it contains no execution path and preserves every unresolved GO condition.

The cached HP diagnostic report and dashboard expose this decision as `SetFanMaxFirstWriteGateStatus=NO-GO`, `SetFanMaxFirstWriteGateSatisfied=false`, and a fail-closed reason. These fields are diagnostic-only and cannot authorize or execute a write.

Separate experimental fields record the repeated four-byte physical response and the narrow developer-only allowance: `SetFanMaxDeveloperExperimentAllowed=true` and `SetFanMaxDeveloperExperimentPayload=FourByte`. The separate one-byte comparison requires `--i-approve-one-time-set-fan-max-1-byte-comparison` and all existing runtime gates; it is not represented as a validated report payload. Both paths must keep `SetFanMaxReadbackReliable=false`, `SetFanMaxNormalControlValidated=false`, and `SetFanMaxUserFacingControlAllowed=false`. They are not a normal-control gate update or payload validation.

Schema-v2 report persistence includes those experimental fields so cached diagnostics can distinguish a controlled four-byte developer experiment from normal-control validation. Missing schema-v2 fields remain old-report data and must fail closed.

Older cached reports that lack these fields must also display `NO-GO` / not satisfied with an explicit old-report missing-field reason. Cached optimistic values such as `GO` or `true` are treated as invalid for authorization and remain blocked.

Test coverage for these fail-closed dashboard, report, and copy/export summary paths is tracked in [set-fan-max-gate-test-coverage-checkpoint.md](set-fan-max-gate-test-coverage-checkpoint.md).

Any manually completed export must first pass the [manual evidence review workflow](set-fan-max-manual-evidence-review-workflow.md). Only a cited update of this gate and the proof gap checklist can change the current NO-GO decision.

A future import parser may help identify missing or conflicting fields, but is not evidence approval and cannot update this gate; see the [evidence import/review design](set-fan-max-evidence-import-review-design.md).

The focused [payload-length reference decision](set-fan-max-payload-length-reference-decision.md) found no consensus or exact-device proof. It does not select a length or change this gate's NO-GO state.

The proposed [manual experiment logger design](set-fan-max-manual-experiment-logger-design.md) defines the evidence record. The runner implementation follows the same append-only format but does not alter any GO condition.

The dry-run and baseline commands remain write-disabled. The separate first-write runner maps `WriteExecuted=true` only after an actual selected SetFanMax attempt. Its four-byte-only approval flag is limited to one developer experiment scope; it cannot validate an input length, select a payload, or cross this gate for normal use.

The [runner safety audit](set-fan-max-first-write-runner-safety-audit.md) confirms the implementation remains command-line-only and current-gate blocked; it does not change this NO-GO decision.

The one-byte and four-byte results are partial experimental evidence with inconclusive readback. They do not validate a payload, update `DeviceValidatedInputLength`, or authorize normal fan control. The [payload strategy decision](set-fan-max-payload-strategy-decision.md) defines their bounded developer-experiment roles.
