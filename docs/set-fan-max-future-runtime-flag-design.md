# SetFanMax Future Runtime Flag And Approval Design

This document designs a possible future single-experiment ceremony. No listed flag currently enables a fan write, and the implementation gate remains **NO-GO**.

## 1. Why The Read-Only Flag Is Not Enough

`--hp-wmi-readonly-test` authorizes only the existing guarded read-only probes needed for baseline and verification. It does not express consent to mutate firmware state, identify SetFanMax as the sole write, acknowledge recovery risk, or constrain an experiment to one foreground attempt. Treating it as write authorization would collapse the read/write safety boundary.

## 2. Proposed Future Flags

All four flags would be required together, in documentation only:

- `--hp-victus`: selects the HP Victus developer path.
- `--hp-wmi-readonly-test`: permits approved pre-write, post-write, and restore readbacks.
- `--hp-fan-write-test`: explicitly selects the future manual fan-write test envelope.
- `--set-fan-max-experiment`: narrows that envelope to SetFanMax only.

Missing, duplicated, conflicting, or unrecognized flags must stop before payload construction. Normal `--hp-victus` and the read-only flag alone must remain non-writing.

The current type-only policy contains older placeholder write-flag names. This proposal does not change that code. Before implementation, one separately approved task must choose a single canonical set and update policy, parser, report schema, and tests atomically while write execution remains disabled.

## 3. Required Human Approval Wording

Only after the implementation gate becomes GO may the physically present operator enter this exact foreground confirmation:

> I reviewed the approved device-specific SetFanMax input length, FanMaxGet baseline, restore proof, thermal stop limits, and manual recovery plan for this exact Victus and BIOS. I authorize one SetFanMax enable attempt followed by mandatory readback and restore. I understand that these flags do not authorize any other fan command, retry, background action, or UI control.

The report must record reviewer/operator identity in redacted form, timestamp, device/BIOS identifiers, evidence-package revision, and confirmation result. Typed flags cannot substitute for this confirmation.

## 4. Administrator Requirement

The future process must be elevated before preflight begins. Elevation must be verified and reported, but it cannot override missing evidence, failed readbacks, absent flags, or rejected human approval. No self-elevation after confirmation is allowed because it would lose the reviewed process context.

## 5. AC Power And Thermal Observation

Stable AC power, adequate battery reserve, and an independent foreground thermal observation plan are mandatory. The report must name the observation source, baseline, stop thresholds, responsible observer, and availability through restore/recovery. Loss of AC power, observation, or safe temperature immediately aborts the enable path and enters the approved restore/recovery flow if a write already occurred.

## 6. Required Before-State Report

Before any future write, record:

- Timestamp, model/SKU, BIOS, thermal policy, adapter/AC state, battery reserve, and elevation.
- Presence of all four proposed flags and successful interactive approval.
- Evidence-package revision, selected device-validated input length (`1` or `4`), restore-proof reference, and recovery-plan reference.
- Successful `SystemDesignData`, `FanGetCount`, `FanMaxGet`, and raw-only `FanGetLevel` results with errors empty.
- Immediate `FanMaxGet` baseline with `IsMaxFanEnabled=false`.
- Thermal source, baseline, thresholds, observer, and readiness.
- Dry-run blocked reasons (must be empty), while write implemented/allowed remain false until the separately approved runtime implementation exists.
- Method/command and payload-description metadata without executable raw payload bytes.

## 7. Required After-Write Report

After the single future enable attempt, record attempted/succeeded/error status, timestamp and elapsed time, selected input length, redacted payload-description digest, immediate `FanMaxGet` invocation/decode result, observed `IsMaxFanEnabled`, thermal observations, and any abort reason. Method return alone must never be reported as verified state.

## 8. Required Restore Report

Record restore attempted/succeeded/error status, use of the same validated input length, immediate and any evidence-required settling readbacks, final `FanMaxGet.IsMaxFanEnabled=false`, baseline match, final thermal state, manual recovery required/performed, recovery outcome, and completion timestamp. Restore is unverified unless the final readback is successful and disabled.

## 9. Required Abort Behavior

Before a write, any failed gate terminates with no payload construction or invocation. After a future write, cancellation, exception, unsafe temperature, lost power/observation, unexpected result, or failed readback stops all non-recovery activity and enters the one reviewed restore path. No automatic retry, alternate payload length, different write command, background continuation, or EC fallback is permitted. Failed restore invokes the documented human recovery plan and keeps the gate NO-GO for further experiments.

## 10. Why Fan UI Must Wait

A UI would bypass the deliberate command-line ceremony and imply repeatable control. One guarded experiment would not prove persistence, repetition, cancellation, lifecycle recovery, or general fan-level safety. Fan controls must remain absent until a separate body of evidence and explicit product-level safety review exists.
