# SetFanMax First-Write Experiment Runner Design

## Status

The developer-only runner is implemented. Normal fan write readiness remains **NO-GO**, but a narrowly scoped command-line approval can satisfy the runner's human-approval gate for one reviewed four-byte experiment only. It neither validates the payload length nor changes `DeviceValidatedInputLength`, which remains unset. There is no UI or tray route.

## Proven Baseline

The exact target is Victus by HP Gaming Laptop `16-s0xxx`, SKU `7Z5Z2EA#AB8`, BIOS `F.31`, thermal policy V1. Elevated read-only baselines report `FanGetCount=2`, `FanMaxGet=false`, and raw FanGetLevel prefix `22-25`; `SystemDesignData`, `FanGetCount`, `FanMaxGet`, and `FanGetLevel` all succeeded. See [exact-device baseline evidence](set-fan-max-exact-device-baseline-evidence.md).

One manually reviewed four-byte attempt returned success for enable and restore and produced an observed fan ramp, but FanMaxGet stayed `false` and the runner correctly returned `Fail`. See the [first four-byte result](set-fan-max-first-4byte-experiment-result.md). It is not payload validation and establishes that future confirmation criteria cannot rely on FanMaxGet alone.

## Candidate Payloads

| Hypothesis | Enable | Matching restore/off |
| --- | --- | --- |
| Four byte | `01-00-00-00` | `00-00-00-00` |
| One byte | `01` | `00` |

Exactly one hypothesis may be supplied for one future run. The runner must never infer a default, try the other length, retry, or fall back. Supplying a hypothesis does not validate it.

## Future Flags And Gates

The future runner would require all of these flags:

- `--hp-victus`
- `--hp-wmi-readonly-test`
- `--hp-fan-write-experiment`
- `--set-fan-max-payload-length=1` or `=4`
- `--i-understand-this-can-affect-fans`
- `--i-approve-one-time-set-fan-max-4-byte-experiment` when, and only when, the selected hypothesis is `=4`
- `--i-approve-second-set-fan-max-4-byte-confirmation` for the separately reviewed second four-byte confirmation only

Optional observation metadata is accepted only with this developer experiment path: `--physical-fan-response-observed=true|false`, `--restore-observed=true|false`, and `--manual-observation-notes="..."`. Boolean values are strict and notes are sanitized and capped at 512 characters. These arguments annotate the append-only log after the runner result; they never bypass an approval, choose a payload, or initiate hardware activity.

The original four-byte approval and the separate second-confirmation approval are both required for the documented confirmation. Either is rejected for `=1`; neither creates a fallback, selects a payload, or changes `DeviceValidatedInputLength`. The runner still requires exact model/SKU/BIOS match, an elevated Administrator process, confirmed AC power, and a same-session successful baseline. AC is checked through the local Windows power-line status API and unknown/offline power fails closed. That baseline must include all approved read-only probes, `FanGetCount=2`, `FanMaxGet=false`, and recorded raw FanGetLevel values. Any mismatch, unavailable reading, or missing approval blocks the run before a write.

## Future Run Sequence

1. Open an append-only experiment record and capture the exact identity, flags, power state, manual operator confirmation, baseline probes, and chosen hypothesis.
2. Recheck every gate immediately before the write boundary.
3. Execute one SetFanMax enable payload for the selected hypothesis only.
4. Wait one short, controlled observation interval, never exceeding ten seconds and ending immediately on a stop condition.
5. Read `FanMaxGet` and raw `FanGetLevel`; record return/error data and manual fan, thermal, UI, and event-log observations.
6. Execute the matching restore/off payload exactly once using the same hypothesis length.
7. Read `FanMaxGet` and raw `FanGetLevel` again; require a final `FanMaxGet=false` baseline match.
8. Append and seal the final experiment record with timings, observations, stop/recovery details, and a pass/fail/unknown outcome.

The future implementation must not use SetFanMode, SetFanLevel, `0x37`, EC access, background control, UI controls, a second payload shape, or automatic retries.

## Stop And Restore Rules

Abort before enable for invalid or duplicate flags, missing elevation/AC/approval, identity mismatch, baseline failure, non-false max-fan baseline, unavailable thermal observation, power transition, protection fault, or missing recovery route. After enable, stop immediately for timeout, exception, ambiguous readback, unexpected fan behavior, unsafe thermal trend, UI loss, crash/event-log evidence, suspend/shutdown/unplug, or any failed gate.

Restore is mandatory after an enable attempt. It uses the matching off payload exactly once, followed by immediate `FanMaxGet` and raw FanGetLevel readback. A failed or ambiguous restore is `unknown` or `fail`, retains NO-GO, records manual recovery steps, and prohibits retries or alternate-payload attempts.

## Required Log Fields

The append-only record must contain timestamp; operator and approval reference; model, SKU, BIOS, thermal policy; flags and elevation; AC/battery and thermal observation source; hypothesis length and bytes; command/type/class/method metadata; baseline, post-enable, and post-restore probe results; invocation return/error metadata; timings; manual fan/thermal/UI/event-log observations; stop reason; recovery actions; `WriteExecuted`; and final outcome. It must summarize decoded values only and never dump raw binary buffers.

## Outcome Rules

- `Pass`: every gate held, one enable attempt was observed within reviewed limits, matching restore completed, final `FanMaxGet=false` matched the baseline, and no stop condition occurred.
- `Fail`: an explicit error, safety threshold breach, failed restore, or unrecoverable state occurred.
- `Unknown`: default for missing, conflicting, incomplete, or ambiguous evidence, including any uncertain restore/readback.

A pass is evidence for independent review only. It does not select a payload, update `DeviceValidatedInputLength`, change the gate to GO, or enable normal fan control UI.

## Evidence Needed Before Input-Length Update

Only an independently reviewed exact-device record may support a later decision. It must show one length and payload, the pre-enable baseline, bounded enable result, post-enable readback, matching restore, final disabled readback, AC/battery and thermal observations, failure/recovery handling, and human approval. The first-write decision gate and proof gap checklist must then be updated together in a separately authorized design task.

## Current Implementation Boundary

The command-line runner has no UI or tray route, never retries, and never falls back between payload lengths. It creates a blocked append-only record when a command or runtime gate fails. The original and second-confirmation approvals are both required for the documented second four-byte confirmation; they cannot satisfy a one-byte request or alter normal fan-write readiness.

The [runner safety audit](set-fan-max-first-write-runner-safety-audit.md) verified these boundaries and hardened the exception path so a managed enable-transport failure still reaches the matching one-time restore attempt.

## Recommended Next Step

Plan a separately authorized controlled second four-byte confirmation only, with timestamped manual fan, thermal, AC/battery, UI, and event-log observations plus readback criteria beyond FanMaxGet. Do not test one byte, update `DeviceValidatedInputLength`, or expose normal control while the result remains partial.
