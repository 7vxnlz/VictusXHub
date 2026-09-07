# SetFanMax Manual Evidence Capture Package

This package defines the evidence record required before a SetFanMax implementation may be considered. It is a documentation and review template only: it does not authorize an experiment, select a payload length, invoke WMI, or describe an executable write procedure.

## Status And Scope

SetFanMax remains **NO-GO**. `DeviceValidatedInputLength` is unset, neither payload candidate is selected, and VictusX must not generate missing write evidence. Evidence must come from an independently authorized, sanitized, reviewable source and be traceable to the exact device and firmware.

## Device Identity Record

Record every field; any missing or conflicting identity stops review.

| Required field | Evidence to capture |
| --- | --- |
| Model | Exact Windows/firmware-reported model |
| SKU | Full SKU, including regional suffix |
| BIOS version | Version and evidence timestamp |
| Thermal policy version | Decoded `ThermalPolicyVersion` |
| SystemDesignData summary | Decode status, returned byte count, supported decoded fields, and summarized unknown tail; no raw binary dump |

For VictusX's current device, the expected identity is Victus `16-s0035nt`, SKU `7Z5Z2EA#AB8`, BIOS `F.31`, and thermal policy V1. Similar models or BIOS revisions are supporting context, not device validation.

## Baseline Evidence Record

Capture timestamped, decoded results and any errors before relying on an external record:

- `FanGetCount`: invocation/decode status, returned byte count, fan count, and protection status.
- `FanMaxGet`: invocation/decode status and an unambiguous disabled baseline.
- `FanGetLevel`: invocation/decode status and raw values only; do not label them RPM, percent, curve points, or control targets.
- AC/battery state: AC connected and stable, adapter identity/rating if available, battery charge/reserve, and power-plan/state context.
- Temperatures: source, sensor labels, timestamps, baseline values, and sampling method, but only from established non-write sources.

Missing, stale, ambiguous, or mismatched baseline data keeps the package incomplete.

### Developer-Only Baseline Record

When separately authorized and run from an elevated Administrator process, the developer-only baseline command can collect the four existing approved read-only probes into `%APPDATA%\VictusX\Logs\FanExperiments\`. It requires `--hp-victus`, `--hp-wmi-readonly-test`, `--hp-fan-write-experiment-baseline`, and exactly one unvalidated `--set-fan-max-payload-length=1` or `=4` hypothesis. It captures baseline evidence only, writes no fan state, and exits before the normal UI. Its output remains **NO-GO** and cannot select a payload length or update `DeviceValidatedInputLength`.

Manual elevated captures now provide matching exact-device read-only baselines for both hypothesis labels: fan count `2`, `FanMaxGet=false`, and raw FanGetLevel prefix `22-25`, with all four approved probes succeeding. The [exact-device baseline evidence](set-fan-max-exact-device-baseline-evidence.md) records the sanitized results. This completes only the read-only baseline portion; it does not validate a payload length or supply write, restore, thermal/power, recovery, rollback, or approval proof.

## External Observation Record

External observations support readbacks but never replace them:

- Audible fan behavior: observer, timestamp, qualitative change, duration, and environment; fan sound alone cannot prove latch state or speed.
- Thermal behavior: independent source, baseline, observed range, stop threshold, elapsed time, and any sensor loss or anomaly.
- Recovery/restore behavior: observed application/process state, reboot or approved recovery action if relevant, and final independent `FanMaxGet=false` confirmation.

Record discrepancies between method return, readback, audible behavior, and thermal behavior without resolving them by assumption.

## Payload-Length Evidence Record

Exactly one candidate must eventually have exact-device evidence; this package does not choose either:

| Candidate | Evidence required |
| --- | --- |
| 1 byte | A sanitized record for this exact model/SKU/BIOS showing `hpqBIOSInt0`, command `0x20008`, type `0x27`, input size `1`, state-byte meaning, and matching enable/restore readbacks without alternate-length retry. |
| 4 bytes | The same exact-device record showing input size `4`, the state byte plus three zero tail bytes, and matching enable/restore readbacks without alternate-length retry. |

Repository prevalence, trailing-zero intuition, a nearby model, BIOS `F.30` evidence, successful read-only commands, or trying both shapes cannot select the length. No code should default, infer, probe, retry, or fall back between `1` and `4`.

## Stop Conditions

Stop review and retain **NO-GO** for any missing/conflicting identity, absent exact input size, alternate-shape retry, unknown or enabled baseline, readback/decode failure, protection fault, unstable AC, inadequate battery reserve, unavailable thermal observation, unsafe temperature, suspend/shutdown/power transition, exception, timeout, cancellation, missing operator, missing recovery access, ambiguous result, or evidence involving another write command or EC path.

An observed stop condition must be recorded; it must not be cleared by inference or an unreviewed retry.

## Rollback And Restore Proof

The evidence package must show one complete, length-matched chain:

1. Successful `FanMaxGet=false` baseline.
2. Independently observed bounded state change, with immediate `FanMaxGet` readback.
3. Restore/disable evidence using the same validated input length.
4. Successful final `FanMaxGet=false` readback matching baseline.
5. Thermal and power state remaining within reviewed limits through restore.
6. Recovery actions and outcome if any result was failed or ambiguous.

A method return, fan noise, app exit, or reboot is not rollback proof by itself. Failed or unverified restore blocks all further consideration.

## Human Approval Checkpoint

After every required record is complete, an independent reviewer must record their name, date, exact device/BIOS identity, evidence references, conflicts reviewed, and the implementation-only approval wording from the [implementation gate](set-fan-max-implementation-go-no-go.md). Approval permits consideration of a separately scoped implementation task only; it does not authorize a hardware write, runtime test, UI control, retry, or background operation.

## Completion Decision

The package is complete only when every required field is traceable, internally consistent, independently reviewable, and accepted without inference. Until then, SetFanMax remains **NO-GO**, `DeviceValidatedInputLength` remains unset, and all fan write/control code and UI remain forbidden.

Source-of-truth gaps are tracked in the [SetFanMax proof gap checklist](set-fan-max-proof-gap-checklist.md). The next safe task is a documentation-only first-write runner specification that uses the proven baseline but leaves payload selection and all write execution absent.

The [SetFanMax readiness panel audit](set-fan-max-readiness-panel-audit.md) confirms the HP Diagnostic dashboard displays this evidence state as read-only status only. The [first-write decision gate](set-fan-max-first-write-decision-gate.md) defines the fail-closed threshold before a separate implementation-design task could be considered.

## Export Template

HP Diagnostic Markdown exports include a blank manual evidence capture template for the identity, SystemDesignData, baseline, observation, payload-length evidence, readback, recovery, reviewer, and approval fields above. It is export-only text: it remains **NO-GO**, does not select a payload length, and cannot invoke WMI or perform a fan write.

Use the [manual evidence review workflow](set-fan-max-manual-evidence-review-workflow.md) to review a completed export. Completing its review sequence still cannot approve code writes by itself. The template is aligned to the workflow so missing manual evidence is visible instead of implied.
