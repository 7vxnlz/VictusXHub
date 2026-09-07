# SetFanMax Manual Evidence Review Workflow

This workflow reviews a manually completed HP Diagnostic Markdown export. It is documentation-only and remains **NO-GO** until every required proof is independently reviewed for the exact target device. It cannot approve code writes, select a payload length, invoke WMI, or authorize an experiment by itself.

## Required Export Content

Review the diagnostic summary, SetFanMax readiness/gate state, and the blank manual evidence template. The export template must reserve fields for identity, SystemDesignData, baseline readbacks, timestamps/conditions, payload-length evidence, enable/restore readbacks, recovery notes, reviewer metadata, and human approval. Reject the record if the export is missing its read-only disclaimer, `NO-GO` status, or the warning not to guess `1` versus `4` bytes.

## Review Sequence

1. **Identity:** match model, SKU, BIOS, thermal policy version, and SystemDesignData summary to Victus `16-s0035nt` / `7Z5Z2EA#AB8` / BIOS `F.31`. Any conflict or missing field stops review.
2. **Baseline telemetry:** review timestamped FanGetCount, disabled FanMaxGet baseline, raw-only FanGetLevel values, AC/battery state, and any established non-write temperature source. Missing, stale, or ambiguous data stops review.
3. **External observations:** review recorded fan-noise and thermal observations alongside timestamps and conditions. They support, but never replace, decoded readbacks.
4. **Payload length:** require one sanitized exact-device evidence record for either input size `1` or `4`, including method, command/type, state-byte meaning, and matching enable/restore readbacks. Do not infer, retry, or compare candidates experimentally.
5. **Restore and recovery:** require length-matched restore/disable proof, final `FanMaxGet=false` baseline match, and reviewed failure/recovery notes. Missing or failed restore keeps NO-GO.
6. **Reviewer checkpoint:** an independent reviewer records name, date, exact identity, evidence references, conflicts, and the implementation-only approval wording from the [first-write decision gate](set-fan-max-first-write-decision-gate.md).

## Fail-Closed Rules

Retain **NO-GO** for incomplete, conflicting, stale, untraceable, or non-exact-device evidence; unknown input length; missing baseline, thermal/power, restore, recovery, or reviewer record; any alternate-shape retry; or evidence involving EC or other fan write commands. Review completion is not write authorization.

## Future Import Support

A future parser may summarize a manually completed export only as untrusted evidence. Its fail-closed requirements and limits are defined in the [evidence import/review design](set-fan-max-evidence-import-review-design.md): parser output cannot select a payload length, approve a write, or change this workflow's NO-GO status.

Future parser tests must use manual Markdown fixtures only and preserve these limits; see the [evidence import parser test plan](set-fan-max-evidence-import-parser-test-plan.md).

The future [manual experiment logger design](set-fan-max-manual-experiment-logger-design.md) describes required capture fields and stop conditions only. Its logs remain untrusted until reviewed through this workflow.

## If Evidence Ever Meets The Gate

Before any separately authorized implementation-design task, update the [first-write decision gate](set-fan-max-first-write-decision-gate.md) to `GO` with cited exact-device evidence and update the [proof gap checklist](set-fan-max-proof-gap-checklist.md) with each proven item. Until those documents change together, no write code may be added.

## Recommended Next Safe Task

Review an existing sanitized exact-device evidence record against this workflow. If none exists, preserve the read-only diagnostic state and **NO-GO** decision.
