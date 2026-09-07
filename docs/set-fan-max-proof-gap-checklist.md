# SetFanMax Proof Gap Checklist

This is the source-of-truth checklist for evidence required before any SetFanMax write implementation may begin. It records proof status only and never authorizes a write.

## Current Status

**NO-GO.** SetFanMax is not implemented or enabled. `DeviceValidatedInputLength` remains unset, and no code may default, infer, probe, retry, or fall back between payload lengths.

## Known Command Evidence

The reviewed references consistently support the following metadata, but not this device's complete write contract:

- Command: `0x20008`
- Command type: `0x27`
- Method: `hpqBIOSInt0`
- Enable state byte: `0x01`
- Restore/disable state byte: `0x00`

## Payload-Length Gap

| Candidate | Existing evidence | Missing exact-device proof |
| --- | --- | --- |
| 1 byte | OmenSuperHub and OmenXHub use the state byte alone; one exact-device comparison produced an observed response. | Input ABI, reliable success readback, restore semantics, thermal/power, and recovery proof remain unvalidated. |
| 4 bytes | ghelper-omen and omencore use `{ state, 0, 0, 0 }`; two exact-device experiments produced observed responses. | Input ABI, reliable success readback, restore semantics, thermal/power, and recovery proof remain unvalidated. |

Read-only success from `FanMaxGet`, `FanGetCount`, or `FanGetLevel` does not establish the write input length. Neither candidate is selected.

The [two exact-device four-byte experiments](set-fan-max-second-4byte-confirmation-result.md) and [one one-byte comparison](set-fan-max-1byte-comparison-result.md) produced observed fan responses, but post-enable FanMaxGet remained false in every case. Four byte has more repeated physical-response evidence, but neither length is selected. The input ABI, readback semantics, restore behavior, and payload decision remain open.

Cached experimental-status fields can preserve that distinction, including `SetFanMaxDeveloperExperimentAllowed=true` for four byte only, but cannot mark input ABI, readback, restore, or UI proof as complete.

## Proven Read-Only Baseline

- [x] Exact-device identity captured for SKU `7Z5Z2EA#AB8`, BIOS `F.31`, thermal policy V1.
- [x] Elevated `SystemDesignData`, `FanGetCount`, `FanMaxGet`, and `FanGetLevel` baseline probes succeeded and decoded.
- [x] Both hypothesis-labelled records captured fan count `2`, `FanMaxGet=false`, and raw FanGetLevel prefix `22-25` without a write.

See the [exact-device baseline evidence](set-fan-max-exact-device-baseline-evidence.md). These checks establish the pre-write baseline only; they do not satisfy the payload ABI, restore, thermal/power, recovery, rollback, or approval gaps below.

## Remaining Proof Gaps

- [ ] **Device input ABI:** one independently reviewable strategy selects exactly one input length for normal control on this exact model/SKU/BIOS, with no alternate-shape fallback. Two four-byte and one one-byte physical responses are not sufficient because their state readback is inconclusive.
- [ ] **Restore behavior:** the same selected length restores max fan to disabled, proven by a reviewed readback/observation contract. FanMaxGet matched the disabled baseline but did not show the observed enabled response, so it is insufficient as the sole proof.
- [ ] **Thermal observation:** a named independent source, baseline, stop thresholds, observer, and continuous availability through restore and recovery are reviewed.
- [ ] **AC, battery, and power state:** stable AC, adequate battery reserve, and defined behavior for unplug, suspend, shutdown, or power transition are evidenced for the experiment. Existing policy requirements are not device proof.
- [ ] **Failure and recovery:** exception, timeout, cancellation, ambiguous readback, lost power, and failed restore have a reviewed stop path and a locally available manual recovery procedure.
- [ ] **Rollback proof:** one complete evidence chain records baseline, bounded enable observation, restore, final `FanMaxGet=false`, baseline match, and recovery outcome.
- [ ] **Human review:** the evidence package and implementation-only approval wording are recorded for this exact device and BIOS.

## Required Before Implementation

Every checkbox above must be proven and independently reviewable. The evidence must also identify the exact method, command, command type, selected input size, bounded target, pre/post/restore readbacks, return/error metadata, timing, AC/battery state, thermal observations, aborts, and recovery result. A separate task must then explicitly authorize a constrained implementation design; proof completion alone does not authorize execution.

SetFanMode, SetFanLevel, ambiguous `0x37`, EC paths, generic write APIs, automatic retry, background operation, and payload-shape fallback cannot be used to fill any gap.

## Required Before UI Exposure

Even after a separately authorized guarded experiment, fan UI remains blocked until repeated exact-device evidence proves safe enable, restore, cancellation, shutdown, suspend/resume, power-loss handling, and recovery across reviewed runs. A product-level safety review must separately approve bounded controls, lifecycle behavior, error reporting, and removal of write access when evidence becomes stale. One successful experiment is not UI evidence.

## No-Guess Rule

No code should guess `1` or `4`, choose by repository count or model similarity, construct both shapes, retry with the other shape, or silently default a missing value. `DeviceValidatedInputLength` must remain unset and preflight must fail closed until exact-device evidence selects one length.

## Recommended Next Safe Task

Define a payload strategy decision that distinguishes bounded developer experiments from normal control. Do not select a payload for normal use or expose normal control until the remaining ABI, readback, restore, thermal/power, recovery, rollback, and approval gaps close.

The [second four-byte confirmation result](set-fan-max-second-4byte-confirmation-result.md) strengthens experimental evidence but does not close any checkbox.

Supporting detail: [payload-length final audit](set-fan-max-payload-length-final-audit.md), [recovery/restore proof plan](set-fan-max-recovery-restore-proof-plan.md), and [implementation gate](set-fan-max-implementation-go-no-go.md).

Use the [manual evidence capture package](set-fan-max-manual-evidence-capture.md) to record and review any independently obtained exact-device evidence. Completing the template does not authorize implementation or execution.

Use the [manual evidence review workflow](set-fan-max-manual-evidence-review-workflow.md) to apply the required review order and fail-closed rules before this checklist or the first-write decision gate could ever be updated.

The planned [evidence import/review design](set-fan-max-evidence-import-review-design.md) may flag incomplete or conflicting manual exports only. It cannot mark any proof as proven, select a payload length, or change NO-GO.

The focused [payload-length reference decision](set-fan-max-payload-length-reference-decision.md) confirms that generic one-byte and near-device four-byte evidence cannot resolve this exact-device gap.

The [manual experiment logger design](set-fan-max-manual-experiment-logger-design.md) specifies the evidence a future separately authorized attempt would need to capture; it cannot fill any checkbox without independent review.

The HP Diagnostic dashboard's read-only SetFanMax readiness panel is audited in [SetFanMax readiness panel audit](set-fan-max-readiness-panel-audit.md); it reports these gaps only and does not enable writes. The consolidated implementation threshold is the [first-write decision gate](set-fan-max-first-write-decision-gate.md), which remains NO-GO.
