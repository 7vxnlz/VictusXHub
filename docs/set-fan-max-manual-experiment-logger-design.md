# SetFanMax Manual Experiment Logger Design

## Status and Purpose

This is an append-only audit logger for one future, separately authorized developer experiment. The runner is command-line-only and current approval wiring remains blocked, so writes are not enabled, `DeviceValidatedInputLength` is unset, and the current decision is **NO-GO**. The logger records evidence around a bounded enable/restore attempt only if a later separately reviewed gate permits it; it cannot grant permission itself.

## Candidate Payloads and Order

| Candidate | Enable | Restore |
| --- | --- | --- |
| 1 byte | `{ 0x01 }` | `{ 0x00 }` |
| 4 bytes | `{ 0x01, 0x00, 0x00, 0x00 }` | `{ 0x00, 0x00, 0x00, 0x00 }` |

If every gate is later approved, the proposed first research candidate is four bytes because omencore has the nearest `8BD4` / V1 cohort. One byte remains an alternate research candidate because OmenXHub and OmenSuperHub agree on it. Neither is validated, selected, or allowed today; no fallback, second shape, or retry is permitted in one experiment.

## Future Runtime Gates

The future implementation would require all of the following, with no UI route:

- `--hp-victus`
- `--hp-wmi-readonly-test`
- `--hp-fan-write-experiment`
- `--set-fan-max-payload-length=1` or `=4`
- `--i-understand-this-can-affect-fans`
- an elevated Administrator process, stable AC power, documented battery reserve, and explicit human approval for the exact device and BIOS.

The proposed log folder is `%APPDATA%\VictusX\Logs\FanExperiments\`. A future implementation must create a new timestamped, append-only text or Markdown record before any action and leave it locally available after failure.

## Current Developer-Only Dry Run

The write-disabled command `--hp-victus --hp-fan-write-experiment-dry-run --set-fan-max-payload-length=1` or `=4` now creates one local blocked JSON record and exits before normal application startup. The length is recorded only as a hypothesis. Missing `--hp-victus`, missing/invalid/duplicate length values, or `--hp-wmi-readonly-test` are rejected in that record. It does not invoke WMI, collect live readbacks, execute SetFanMax, or alter `DeviceValidatedInputLength`.

Runtime verification produced blocked records for both payload hypotheses. The four-byte record stored `PayloadLengthCandidate=FourByteHypothesis` with `PayloadBytesHypothesis=01-00-00-00`; the one-byte record stored `PayloadLengthCandidate=OneByteHypothesis` with `PayloadBytesHypothesis=01`. Both records kept `WriteExecuted=false`, `DeviceValidatedInputLength=null`, `FirstWriteGateSatisfied=false`, `Outcome=Unknown`, and the standard NO-GO blocked reasons. Enable and restore results both state that WMI and hardware were not attempted.

## Developer-Only Read-Only Baseline Capture

The separate baseline command is `--hp-victus --hp-wmi-readonly-test --hp-fan-write-experiment-baseline --set-fan-max-payload-length=1` or `=4`. It requires an elevated Administrator process and exits after writing one append-only experiment record. It reuses only the existing approved read-only probes: `SystemDesignData`, `FanGetCount`, `FanMaxGet`, and `FanGetLevel`.

The record captures decoded identity and baseline facts where available: model, SKU, BIOS, thermal policy version, fan count, max-fan state, and the known raw FanGetLevel prefix. It summarizes each probe's attempted/success/decode/byte-count state without logging full binary output. The payload length remains a hypothesis only; the record always preserves `WriteExecuted=false`, `DeviceValidatedInputLength=null`, `FirstWriteGateSatisfied=false`, and **NO-GO**. It does not invoke SetFanMax or any other write-capable command, and it does not start the normal UI.

Manual elevated runtime verification produced exact-device baseline records for both hypothesis labels. Each record identified SKU `7Z5Z2EA#AB8`, BIOS `F.31`, thermal policy V1, fan count `2`, `FanMaxGet=false`, and raw FanGetLevel prefix `22-25`; all four approved read-only probes succeeded. Both records kept `WriteExecuted=false`, `FirstWriteGateSatisfied=false`, and `DeviceValidatedInputLength=null`. See the [exact-device baseline evidence](set-fan-max-exact-device-baseline-evidence.md).

## Required Evidence Record

The baseline record must include timestamp; model; SKU; BIOS; thermal policy; selected candidate length; proposed payload bytes; command `0x20008`; type `0x27`; WMI class/method; FanGetCount; FanMaxGet; raw FanGetLevel; AC/battery state; temperature source/baseline; and operator approval.

The post-enable record must include the write return/error metadata, elapsed time, FanMaxGet, raw FanGetLevel, temperature trend, fan-noise observation, UI responsiveness, and any Windows crash/event-log observation. The restore record must include the restore return/error metadata, elapsed time, FanMaxGet, raw FanGetLevel, baseline comparison, recovery notes, process-cleanup result, and final `pass`, `fail`, or `unknown` result.

## Timing and Stop Rules

Proposed design windows are: establish a stable baseline for at least 60 seconds; allow no more than 10 seconds between enable and the immediate restore attempt; then observe the restored state for at least 60 seconds. These are future safety-review defaults, not permission to execute today.

Abort before writing for any missing gate, unknown baseline, unstable AC/battery state, unavailable readback, absent temperature observation, active thermal warning, or missing recovery route. After a future enable attempt, immediately stop on a timeout, exception, loss of UI responsiveness, unexpected temperature rise, unexpected fan behavior, failed/ambiguous readback, suspend/shutdown/power transition, crash evidence, or inability to restore. Do not retry, try the alternate length, start a timer, or continue background control.

## Required Manual Observations

The operator must record fan noise, temperature trend, UI responsiveness, Windows crash/event-log findings, and whether the process exits cleanly. A successful WMI return alone is never a pass; final `FanMaxGet=false` and documented recovery are required after restore.

The developer-only runner can persist explicitly supplied manual response metadata with `--physical-fan-response-observed=true|false`, `--restore-observed=true|false`, and `--manual-observation-notes="..."`. It does not infer a physical response when these arguments are absent. Notes are sanitized and capped at 512 characters; malformed or repeated observation arguments fail the experiment request closed before its write boundary.

## Fail-Closed Outcome

Missing, conflicting, ambiguous, or untraceable logs are `unknown` and preserve **NO-GO**. The record cannot update `DeviceValidatedInputLength`, unlock a normal fan control UI, select an alternate payload, or approve further writes. Only independently reviewed exact-device evidence can support a later update to the [first-write decision gate](set-fan-max-first-write-decision-gate.md) and [proof gap checklist](set-fan-max-proof-gap-checklist.md).

## Required Evidence Before Updating Input Length

One reviewed record for this exact model/SKU/BIOS must identify exactly one length, payload and method; show pre-enable, post-enable, and post-restore readbacks; confirm restore to the captured disabled baseline; include AC/battery and thermal observations; document failure/recovery handling; and contain independent human approval. Even then, it would support only a separately authorized design update, not normal control UI.

## Current Runner Boundary

The [first-write experiment runner](set-fan-max-first-write-experiment-runner-design.md) now implements the future sequence behind all required flags, elevation, AC, exact identity, baseline, and separate approval gates. It selects no payload and does not weaken the first-write gate: current application wiring fails closed before any write transport is reached.
