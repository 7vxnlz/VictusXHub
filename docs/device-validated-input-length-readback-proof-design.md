# DeviceValidatedInputLength And Readback Proof Design

## Current Known State

- `DeviceValidatedInputLength` is `null` and must remain unset.
- `FanMaxGet` is inconclusive: it stayed `false` during observed SetFanMax physical fan response.
- `FanGetLevel` is ambiguous: raw values changed, but they are not decoded as RPM, percent, fan speed, or control state.
- The four-byte SetFanMax Max Fan Pulse is operational only as a developer-only bounded pulse behind explicit gates.
- Normal/user-facing fan control remains **NO-GO**.

## What DeviceValidatedInputLength Would Mean

`DeviceValidatedInputLength` would mean VictusX has exact-device evidence that one SetFanMax input shape is the intended, durable payload length for this HP Victus `16-s0035nt` / `7Z5Z2EA#AB8` / BIOS `F.31` / ThermalPolicyVersion `1` target.

It would not mean that normal fan control is safe by itself. A validated input length would only settle the ABI shape for one command; readback, restore, thermal behavior, recovery, and UI safety would still need separate proof.

## Why Physical Pulse Response Is Not Enough

Physical fan ramp confirms that the command can affect the fan. It does not prove:

- whether firmware interpreted every payload byte as intended
- whether one-byte and four-byte payloads are ABI-equivalent
- whether restore is durable beyond a short pulse
- whether repeated or background writes are safe
- whether the same behavior survives sleep/resume, battery operation, crashes, or HP/OMEN service conflicts

Four byte remains the preferred developer experiment payload because it has repeated exact-device records and closer Victus/V1 reference context. That preference does not validate it for normal control.

## Readback Reliability Requirements

Normal fan control needs a trustworthy way to know whether the requested fan state occurred and whether it returned to automatic/safe behavior.

`FanMaxGet=false` after enable means FanMaxGet cannot currently be trusted as latch confirmation. It may still be useful as one raw diagnostic signal, but it cannot be the sole success criterion.

`FanGetLevel` raw values must remain raw-only until their byte meaning is decoded and validated against external observations. They must not be displayed or used as RPM, percent, a speed target, or fan-curve input.

## Read-Only Evidence Sources

This proof design may use only already available read-only or local evidence:

- existing append-only SetFanMax experiment logs under `%APPDATA%\VictusX\Logs\FanExperiments\`
- HP capability report data
- cached HP Diagnostic report data
- reference-source comparison documents already recorded in the repository
- manually supplied observation notes attached to existing logs

This task does not permit new WMI invocation, experiment execution, pulse execution, command expansion, or hardware probing.

## Payload Input Length Confidence Criteria

Acceptable future proof:

- exact target identity matches HP Victus `16-s0035nt` / `7Z5Z2EA#AB8` / BIOS `F.31` / ThermalPolicyVersion `1`
- selected payload length has repeated, reviewed, append-only records
- enable and matching restore bytes are recorded
- evidence explains why the alternate payload is not selected for normal control
- review explicitly states the selected length is scoped to SetFanMax on this exact device

Unacceptable proof:

- generic OMEN/HP source evidence alone
- successful command return alone
- physical fan response alone
- "both lengths seem similar" as ABI validation
- updating `DeviceValidatedInputLength` from a dashboard, cached report, or parser output without human review

Fail-closed rule: if evidence is missing, conflicting, ambiguous, generic, or not exact-device, keep `DeviceValidatedInputLength=null`.

## FanMaxGet Reliability Criteria

Acceptable future proof:

- a documented explanation of why FanMaxGet stays `false` while the fan physically responds, or a replacement state criterion that does not rely on FanMaxGet alone
- repeated logs showing how FanMaxGet behaves before enable, after enable, after restore, and after cooldown
- review showing whether FanMaxGet is useful only as baseline safety, restore hint, or not useful for state confirmation

Unacceptable proof:

- treating `FanMaxGet=false` as proof that enable failed when physical response and restore were observed
- treating `FanMaxGet=false` as proof that restore succeeded
- using FanMaxGet as the only UI state contract

Fail-closed rule: until a reliable interpretation exists, FanMaxGet remains inconclusive and cannot drive normal UI.

## FanGetLevel Interpretation Criteria

Acceptable future proof:

- repeatable raw values correlated with external fan behavior, timing, and restore observations
- documented byte meaning, fan ordering, scale, and limits if discoverable
- clear statement of what FanGetLevel can and cannot prove

Unacceptable proof:

- assuming raw bytes are RPM, percent, duty, curve points, or target speeds
- using raw values as slider positions
- treating a value change as normal-control validation

Fail-closed rule: keep FanGetLevel raw-only and non-actionable until decoded.

## Restore Verification Criteria

Acceptable future proof:

- command result, post-restore readback, manual restore observation, and follow-up timing are all recorded
- restore remains safe across repeated sessions, process exit, cancellation, and recovery cases
- recovery guidance exists for failed or ambiguous restore

Unacceptable proof:

- assuming restore is durable from one successful return
- relying on FanMaxGet alone
- relying on the fan eventually stopping without timing and context

Fail-closed rule: without durable restore proof, no normal fan control.

## Normal Fan-Control Readiness Criteria

Normal fan control cannot be considered until all of these are true:

- `DeviceValidatedInputLength` is reviewed and set by an explicit evidence decision
- readback reliability has a documented success/state criterion independent of FanMaxGet alone
- FanGetLevel remains either decoded safely or explicitly excluded from control decisions
- restore, thermal, power-state, sleep/resume, crash/recovery, and service-conflict proof exists
- unsupported commands remain isolated and blocked
- a separate user-facing safety design is reviewed

Fail-closed rule: one missing criterion keeps normal/user-facing fan control **NO-GO**.

## Recommended Next Safe Implementation Task

The read-only HP fan proof-gap analyzer now aggregates valid local experiment logs and the already cached capability report for dashboard, copy, and export text. It ignores missing or invalid evidence, preserves `DeviceValidatedInputLength` as unset, treats FanMaxGet as inconclusive and FanGetLevel as raw-only, and cannot invoke WMI, run experiments, expose controls, add pulse buttons, or update a safety decision.

The [HP fan proof gap analyzer checkpoint](hp-fan-proof-gap-analyzer-checkpoint.md) records the current analyzer behavior and clarifies that normal window close hides to tray while explicit Quit is the full process-termination path.
