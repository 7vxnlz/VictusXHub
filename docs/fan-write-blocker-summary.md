# Fan Write Blocker Summary

The [temperature/RPM milestone](hp-temperature-fan-rpm-telemetry.md) adds only optional NVIDIA GPU-temperature reads. CPU temperature and V1 fan RPM discovery remain unresolved; no 0x38 or other HP method is invoked and FanGetLevel stays raw-only. No fan write/control route or validation change; normal control remains NO-GO.

The [SetFanLevel preflight harness](set-fan-level-first-write-harness-scaffold.md) is internal and non-executable: supplied gates and four-byte equal-pair JSON only, no transport or CLI/UI route. Satisfied offline preflight does not authorize a write. Recovery/ABI proof remains missing, DeviceValidatedInputLength remains null, and first-write/normal control remain NO-GO.

The [SetFanLevel endpoint/recovery audit](set-fan-level-endpoint-recovery-audit.md) supports2..99% -> raw1..54 as an offline default-V1 envelope only. Close8BD4 zero-handoff failures and the reference's multi-command reset/replay fallback prevent execution approval even inside that range. No exact value selected; F.31 recovery/ABI remain unvalidated, DeviceValidatedInputLength null, first-write and normal control NO-GO.

Preferred SetFanLevel research input now uses the [percentage-mapping dry-run](set-fan-level-percentage-mapping-dry-run.md), default V1 ceiling55 with integer truncation and a special100 result, serialized as four bytes only. Legacy raw128/80-80 is deprecated research compatibility. No transport, UI, high-bit packing or fallback was added; F.31 validation, first-write readiness and normal control remain NO-GO, with DeviceValidatedInputLength null.

The [closest-Victus deep trace](omencore-victus-16s-setfanlevel-deep-trace.md) confirms source-level V1 percentage scaling without a normal high-bit flag and four-byte omencore input; ghelper-omen's128-byte buffer is caller-selected, not infrastructure padding. Low-positive rounding, raw replay, multi-command recovery and fallback remain hazards. F.30 is not F.31 validation;80-80 remains uncertain, no first-write value is selected, and DeviceValidatedInputLength stays null. SetFanLevel first-write and normal fan control remain NO-GO.

Current SetFanLevel decision: the [ABI/units evidence audit](set-fan-level-abi-units-evidence-audit.md) establishes common `0x20008/0x2E` identity but conflicting two/three-, four-, and 128-byte reference requests, mixed units, +128 cleaning ambiguity, and nearby Victus/F.30 zero-level recovery failures. `80-80` remains serialization-only with uncertain firmware meaning. No first-write value or restore packet is selected; request existing exact-device evidence next, not a new experiment. SetFanLevel first-write and normal fan control remain **NO-GO**; `DeviceValidatedInputLength` stays null. Developer-only SetFanMax pulse/hold permissions are unchanged.

The [SetFanLevel dry-run research scaffold](set-fan-level-dry-run-research-scaffold.md) adds CLI-only parsing and persisted JSON output for an unvalidated two-byte raw-value hypothesis. Timestamped `set-fan-level-research-dry-run-*.json` files under `Logs/FanExperiments` remain available when WinExe console output is invisible, with explicit no-hardware/no-WMI markers. It has no WMI/transport/UI route, rejects mixed probe/write flags before startup, and leaves SetFanLevel execution forbidden. The `0..255` bound is byte representation only. `DeviceValidatedInputLength` remains null and normal fan control remains **NO-GO**.

## 1. Current Read-Only Successes

The HP Victus read-only path is stable and report-backed:

- `SystemDesignData` succeeded and decoded `ThermalPolicyVersion=1` plus a software fan-control support declaration.
- `FanGetCount` succeeded and reports two fans with protection status clear.
- `FanMaxGet` succeeded and reports max fan disabled.
- `FanGetLevel` succeeded, but its returned values remain raw-only and must not be treated as RPM, percent, a curve point, or a writable target.

These results prove diagnostic visibility only. They do not validate any write command or payload.

## 2. Why SetFanMax Is Blocked

SetFanMax (`0x27`) remains blocked because its device-specific input length is unknown. Two exact-device four-byte attempts and one one-byte comparison returned successful calls and observed fan response, but FanMaxGet remained false after enable and restore; see the [one-byte comparison result](set-fan-max-1byte-comparison-result.md). Four byte has more repeated experimental evidence, but neither is a validated payload or reliable state readback. Matching enable/disable behavior, restore proof, and manual recovery proof remain incomplete.

`DeviceValidatedInputLength` therefore remains unset and normal SetFanMax control remains **NO-GO**. The repeated four-byte physical response permits only separately approved, command-line-only developer experiments. One byte may be used only for one separately approved comparison experiment; neither path permits normal writes or UI control.

Experiment logs may classify a successful command plus manually observed fan response and `FanMaxGet=false` as readback-inconclusive. In that narrow case, legacy `Outcome` is `Unknown` rather than a misleading plain `Fail`, and the FanMaxGet-only failure reason is omitted. This distinction does not validate a payload, recover a reliable latch readback, or permit normal fan control.

The [clean classified four-byte result](set-fan-max-4byte-clean-classified-result.md) records this mapping on the exact device. It supports a limited developer-only pulse-design discussion, not a normal control path.

The [four-byte Max Fan Pulse design](set-fan-max-4byte-max-fan-pulse-design.md) has a separately gated command-line implementation and a recorded bounded developer result, but it has no normal UI route. It does not alter normal control readiness.

The [pulse result](set-fan-max-4byte-max-fan-pulse-result.md) now records a successful bounded developer run. It makes the developer pulse operational, not normal fan control.

The [pulse history/status view design](set-fan-max-pulse-history-status-view-design.md) is implemented as read-only local-log research support only. It cannot execute a pulse or change normal-control NO-GO.

HP Diagnostic Quit now cleans up the diagnostic tray shell and exits its UI loop. This lifecycle fix does not add a pulse route, fan-control UI, or hardware action.

The [pulse history and Quit checkpoint](set-fan-max-pulse-history-quit-checkpoint.md) records the stabilized read-only dashboard history behavior and explicit HP Diagnostic process termination guarantee.

The [HP fan control research abstraction design](hp-fan-control-research-abstraction-design.md) keeps any future internal extraction limited to the developer-only pulse boundary; it must not create a generic fan-control API or a user-facing route.

The research contracts represent only `FourByteMaxFanPulse`, fail-closed gates, snapshots, outcomes, and append-only log requests. The pulse parser and runner now use the fixed four-byte contract metadata without changing flags, gates, payloads, restore behavior, logging, or UI. This does not alter normal-control **NO-GO**.

The [contract-refactor runtime verification](set-fan-max-pulse-contract-refactor-verification.md) records a successful bounded developer pulse after that wiring. FanMaxGet remained inconclusive and `DeviceValidatedInputLength` remained unset, so the result does not alter normal-control **NO-GO**.

The [OmenXHub/OmenSuperHub implementation comparison](omenxhub-omensuperhub-fan-implementation-comparison.md) finds an exact reference match for the SetFanMax WMI class, method, command, and command type, but both repositories use one-byte payloads and omit VictusX's bounded gates and `finally` restore evidence. OmenSuperHub is the narrower SetFanMax source to study; OmenXHub's PawnIO EC fallback, recurring SetFanLevel writes, SetFanMode, `0x37`, curves, and APIs must not enter VictusX. This reference evidence does not select a payload or change **NO-GO**.

The [developer Max Fan Hold command](set-fan-max-developer-hold-command.md) is a separate, CLI-only four-byte research route with an explicit approval and an inclusive `10`-to-`180`-second pre-restore wait. Its [first result](set-fan-max-developer-hold-first-result.md) had successful enable/restore and no reported adverse behavior, but FanMaxGet stayed false and the approximately two-minute observed response outlasted the ten-second wait. The wait is not validated physical fan-duration control; `DeviceValidatedInputLength` remains unset and normal-control **NO-GO** is unchanged.

The hold CLI and logs now state those limits explicitly, including that fans may remain high after restore or wait expiry. `RequestedPreRestoreWaitSeconds` and `HoldDurationSemantics` clarify the evidence while preserving the legacy `RequestedHoldSeconds` field. The [hold semantics checkpoint](set-fan-max-hold-semantics-checkpoint.md) records this as the current developer-only hold state. No gate, payload, runtime fan behavior, or readiness decision changed.

The [normal fan control go/no-go evidence matrix](normal-fan-control-go-no-go-evidence-matrix.md) is the current source-of-truth for what remains missing before user-facing fan control can even be reconsidered.

The [normal fan control proof priority plan](normal-fan-control-proof-priority-plan.md) ranks the blockers. `DeviceValidatedInputLength`, readback reliability, restore proof, thermal/power safety, and recovery behavior must be proven before normal fan control can move out of **NO-GO**.

The [DeviceValidatedInputLength and readback proof design](device-validated-input-length-readback-proof-design.md) defines fail-closed proof criteria for the top blockers. It keeps physical fan response separate from payload validation, treats FanMaxGet as inconclusive, and keeps FanGetLevel raw-only.

The HP Diagnostic dashboard now includes a read-only proof-gap analyzer over local append-only experiment logs and the cached capability report. Missing or invalid evidence stays fail-closed; it cannot invoke WMI, run a pulse, or enable fan control.

The [HP fan proof gap analyzer checkpoint](hp-fan-proof-gap-analyzer-checkpoint.md) records that this analyzer is visible in dashboard, copy, and export output, keeps `DeviceValidatedInputLength` unset, treats FanMaxGet as inconclusive and FanGetLevel as raw-only, and documents that normal window close hides to tray while explicit Quit is the full process-termination path.

The [HP Diagnostic preview readiness checkpoint](hp-diagnostic-preview-readiness-checkpoint.md) confirms that packaging/release-prep may continue only as source/readiness work; normal fan control remains **NO-GO** and release packaging remains blocked by identity, notices, signing/checksum, clean-machine validation, and NU1900 warning disposition.

Developer experiment observation arguments are log metadata only. They require the existing command-line experiment path and cannot approve a payload, bypass a one-byte or four-byte approval, or expose a user-facing control.

## 3. Why SetFanMode Is Blocked

SetFanMode (`0x1A`) changes thermal-policy state rather than a simple fan flag. References disagree between two-byte and four-byte inputs, and mode values vary by thermal-policy generation. Reference flows also show that a successful return may not mean the hardware completed the transition. VictusX has no validated mode readback, baseline mode, restore sequence, or exact-device payload contract.

## 4. Why SetFanLevel Is Blocked

The [first-write preflight design](set-fan-level-first-write-preflight-design.md) records the required exact-device, power, baseline, candidate, observation, logging, and recovery gates. All real-write admission remains NO-GO: the dry-run `128` / `80-80` result is serialization evidence only, no safe executable range is selected, and zero-level or SetFanMax-off restore must not be assumed.

SetFanLevel (`0x2E`) has the widest uncertainty: reviewed references use two-, three-, four-, and 128-byte inputs. Fan ordering, capability bits, and value scale differ across platforms. V1 references document problematic handoffs, including maximum, zero-speed, or non-responsive behavior after `SetFanLevel(0,0)`. The working `FanGetLevel` read is raw-only and cannot validate write semantics or prove a return to BIOS automatic control.

## 5. Why 0x37 Is Blocked

`0x37` is ambiguous across references. It appears as a V2/OMEN Max fan-level read path in some code and as write-like power-limit/control behavior in other command flows. This device reports thermal policy V1, and no device-specific method, direction, payload, or safe interpretation has been established. It must not be probed, prepared as a write, or used as a fallback.

## 6. Why Fan UI Control Must Wait

A control UI would imply that a command, range, state transition, restore path, and failure response are trustworthy. None is proven. Buttons, toggles, sliders, curves, persistence, retries, or background control could turn unresolved firmware behavior into repeated hardware writes. The existing HP UI must remain read-only and cached/report-backed.

## 7. Exact Missing Evidence Before Any Write

Before any fan write can even be reconsidered, all of the following must exist for one exact command on this exact model/SKU/BIOS:

1. One independently reviewed method, command, input size, payload meaning, and bounded target state, with no alternate-shape fallback.
2. A known-safe readback proving the pre-write baseline and post-write state independently of the write return code.
3. A length- and command-matched restore action followed by readback proving the original state was recovered.
4. A reviewed manual recovery procedure for failed, ambiguous, or persistent firmware state.
5. Defined AC-power, thermal-observation, abort, timeout, and cancellation conditions.
6. Explicit human approval scoped to implementation design first; execution would require separate authorization.
7. Evidence that the selected command does not depend on an unvalidated SetFanMax, SetFanMode, SetFanLevel, `0x37`, or EC side path.

Until every item is proven, all fan writes remain **NO-GO**.

## 8. Recommended Next Safe Task

The [first-write experiment runner design](set-fan-max-first-write-experiment-runner-design.md) now has a developer-only, command-line-only implementation. Its explicit four-byte-only approval flag does not select the payload, change `DeviceValidatedInputLength`, or enable normal control. The next safe task is a separately authorized controlled second four-byte confirmation design with stronger manual evidence, not a one-byte test or UI work.

Supporting decisions: [SetFanMax payload-length final audit](set-fan-max-payload-length-final-audit.md), [SetFanMode and SetFanLevel risk study](set-fan-mode-level-risk-study.md), and [missing-proof tracker](set-fan-max-missing-proof-tracker.md).

The authoritative current gap list is the [SetFanMax proof gap checklist](set-fan-max-proof-gap-checklist.md). It keeps payload length unset and all fan writes **NO-GO** until exact-device restore, thermal/power, recovery, rollback, and approval evidence is complete.

The [manual evidence capture package](set-fan-max-manual-evidence-capture.md) defines the required record format. It is documentation-only and does not permit VictusX to generate missing write evidence.

The [SetFanMax first-write decision gate](set-fan-max-first-write-decision-gate.md) consolidates the required proof into one explicit NO-GO threshold before any implementation-design task may begin.

The focused [payload-length reference decision](set-fan-max-payload-length-reference-decision.md) confirms that neither reference shape is exact-device evidence; no payload length or write candidate has been selected.

The [manual experiment logger design](set-fan-max-manual-experiment-logger-design.md) is documentation-only and cannot bypass any blocker, enable a command, or justify fan UI control.

The [first-write runner safety audit](set-fan-max-first-write-runner-safety-audit.md) verifies the implemented runner remains command-line-only, approval-gated, and unable to alter this NO-GO status.

The [one-byte comparison result](set-fan-max-1byte-comparison-result.md) shows a similar physical response to the two four-byte records, but retains NO-GO for normal control. The [payload strategy decision](set-fan-max-payload-strategy-decision.md) keeps four byte preferred for controlled confirmation and one byte comparison-only. Any future experiment still requires separate approval, append-only evidence logging, and matching restore; it is not payload validation or UI-control authorization.

That metadata must explicitly retain unreliable readback, unvalidated normal control, and prohibited user-facing control. It records evidence; it cannot change a safety decision.
