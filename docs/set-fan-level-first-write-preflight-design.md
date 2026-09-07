# SetFanLevel First-Write Preflight Design

## Decision And Scope

The [non-executable harness](set-fan-level-first-write-harness-scaffold.md) now checks supplied offline declarations and serializes a plan. "Preflight satisfied" is not permission, recovery validation or first-write readiness; the full evidence requirements below remain unresolved.

The [endpoint/recovery audit](set-fan-level-endpoint-recovery-audit.md) defines only an offline envelope: equal arguments, integer2..99%, default55 -> raw1..54, four bytes. Zero/100 endpoints are excluded; recovery remains a multi-command, unvalidated handoff. No executable range or exact first-write value is approved; first-write remains NO-GO.

The [percentage-mapping dry-run](set-fan-level-percentage-mapping-dry-run.md) adds close-device arithmetic evidence only. Its four-byte mapped examples, including zero and100 endpoints, are not executable candidates or approved restore values. Default55 is not a validated target ceiling; all first-write gates below remain unsatisfied.

Documentation-only design. SetFanLevel is unvalidated, real writes remain unimplemented and unexecuted, and the first-write decision is **NO-GO**. This document adds no approval flag, transport, probe, or execution permission. The existing dry-run must remain permanently serialization-only; a future write proposal requires separate review and explicit authorization for implementation and execution.

## Current Evidence

The [omencore Victus deep trace](omencore-victus-16s-setfanlevel-deep-trace.md) narrows the ordinary V1 path to conditional percent-to-level scaling and a four-byte request, with no normal high-bit insertion. It does not prove accepted F.31 units/length. Recovery admission must also account for low-positive rounding to zero, raw readback replay, equal-pair maintenance and multi-command service shutdown reset. None is an approved restore or write candidate.

The [ABI and units evidence audit](set-fan-level-abi-units-evidence-audit.md) now records the six local reference revisions: two/three-, four-, and 128-byte input shapes conflict; caller RPM/100, percent scaling, and +128 cleaning conventions do not establish F.31 units. Closest Victus 16-s0xxx/F.30 evidence warns about zero-level recovery. `80-80` has uncertain firmware meaning and is not a selected target. ABI, range, mapping, and restore gates remain unsatisfied; first-write **NO-GO** is unchanged.

The [dry-run scaffold](set-fan-level-dry-run-research-scaffold.md) persisted candidate `128` as `80-80` under `%APPDATA%\VictusX\Logs\FanExperiments`. The saved record reported `NoHardwareInvocation=true`, `NoWmiInvocation=true`, `WriteExecuted=false`, `IsExecutable=false`, and `DeviceValidatedInputLength=null`. This verifies parsing, encoding, file persistence, and safe exit only. It proves no fan response, safe value, fan mapping, supported ABI, or restore behavior.

| Candidate metadata | Current hypothesis |
| --- | --- |
| Namespace / class | `root\wmi` / `hpqBIntM` |
| Method | `hpqBIOSInt0` |
| Command / command type | `0x20008` / `0x2E` |
| Payload | Two equal raw bytes: candidate `128` becomes `80-80` |
| Units and fan ordering | Unknown; not RPM, percent, or validated fan targets |
| Length | Two-byte serialization hypothesis only; not device validated |

The [Omen comparison](omenxhub-omensuperhub-fan-implementation-comparison.md) records two/three-byte reference shapes; the existing forbidden catalog entry assumes four bytes, and the [blocker summary](fan-write-blocker-summary.md) records further cross-platform differences. These conflicts must be resolved, not tried sequentially. No new reference inspection or code copying was performed for this design.

## Why Risk Exceeds SetFanMax

SetFanMax has repeated exact-device physical-response evidence and a named enable/off pair, although durable restore and physical-duration control remain unproven. SetFanLevel introduces unknown target scaling, per-fan mapping, manual-policy interaction, and potential cooling reduction. Successful SetFanMax calls confer no permission to use SetFanLevel. Existing review evidence warns of problematic V1 handoffs with zero-level writes; `00-00` must not be assumed to mean auto or restore.

`FanMaxGet` remains inconclusive and `FanGetLevel` remains raw-only. Neither can establish that a level request took effect safely or that firmware automatic control resumed.

## Required Gates Before Any First Write

All gates below are proposed requirements, not implemented permissions. Unknown, missing, contradictory, stale, or failed evidence blocks admission.

| Gate | Required proof / future check |
| --- | --- |
| Separate review and consent | Reviewed SetFanLevel-specific protocol, reviewer/date, exact build revision, one exact payload, and explicit one-session human authorization. Neither dry-run flags nor SetFanMax approvals qualify. |
| ABI and candidate | Evidence supporting input length, units, ordering of both fan targets, required policy state, and the selected candidate on this device. No percent/RPM inference from raw bytes. |
| Exact device | HP manufacturer and documented Victus 16-s0035nt / firmware model alias `Victus by HP Gaming Laptop 16-s0xxx`, SKU `7Z5Z2EA#AB8`, BIOS `F.31`, ThermalPolicyVersion `1`. Record exact reported strings and reviewed aliases; no generic HP/Victus match or first-WMI-instance shortcut. |
| WMI target identity | Independently reviewed exact instance identity and command/method contract. Ambiguous or multiple matching instances block. No enumeration or invocation in this task. |
| Elevation and power | Administrator session, AC explicitly online, sufficient reviewed battery reserve, stable power, and no pending suspend/restart/update. Unknown power/reserve blocks. |
| Thermal and physical safety | Attended laptop on a stable ventilated surface, idle/light stable workload, known functioning cooling, independently identified temperature monitoring, and reviewed numeric abort limits and maximum observation time. No arbitrary universal thresholds; unfilled limits or unavailable monitoring block. |
| Exclusive ownership | No concurrent experiment, overlapping writer, active prior pulse/hold effect, or unresolved HP/OMEN service contention. Record service state; do not disable services automatically. |
| Fresh baseline | Approved read-only baseline from the same session/device; successful responses, ThermalPolicyVersion `1`, `FanGetCount=2`, baseline `FanMaxGet=false`, and recorded raw `FanGetLevel`, temperature, power, and physical fan observations. Cached reports support planning only. |
| Restore and recovery | Independently justified firmware-auto handoff plus reviewed recovery procedure, operator availability, failure/cancellation handling, and observable return criteria. No guessed restore packet. |
| Auditability | Append-only preflight/intent record persisted before any attempted write; exact payload and all gate evidence captured. Failed logging blocks admission. |

Fresh baseline acquisition would itself need separately authorized read-only work. This task invokes no WMI, including baseline probes. Baseline `FanMaxGet=false` is a conservative exclusion check, not proof of automatic mode; lingering physical activity or unknown ownership still blocks.

## Candidate Range Strategy

The dry-run accepts `0..255` solely because a byte has that representation. The current executable candidate set is **empty**. Candidate `128` is not a recommended first target, midpoint speed, or proof of a safe `80-80` command. Neither zero nor maximum is presumed safer, and raw baseline bytes must not be echoed as writable targets.

First resolve units, ABI, fan mapping, and policy/restore semantics through evidence review. A later separately reviewed protocol could allow exactly one evidence-backed fixed candidate and payload shape for one session. It must justify adequate cooling for both fans and name the relevant evidence. No arbitrary CLI range, sweeps, ramps, clamping, trial-and-error escalation, asymmetric fan targets, or fallback variants. Any later candidate requires a new decision; this plan selects none.

## Restore And Recovery Prerequisite

Before admission, document the exact proposed auto-control restoration mechanism, its evidence, expected physical/state response, observation interval, and manual recovery steps if it fails. SetFanMax off, `SetFanLevel(0,0)`, replayed baseline bytes, restart, or a timeout are not established SetFanLevel recovery mechanisms. If recovery would require a currently forbidden command, stop and obtain a separate proof design; do not broaden this proposal implicitly.

A future separately approved runner would allow one candidate write attempt and arrange one independently approved restore attempt in `finally` once the write was attempted, including ambiguous return/exception cases. That restore is planned cleanup, not retry or fallback. No recovery payload is approved here. Exceptions, cancellation, AC loss, and readback/logging failure after a write must not bypass cleanup. `finally` cannot protect against forced process termination, OS crash, power loss, or a hung firmware call; those cases require a reviewed external operator recovery plan before admission.

A bounded foreground wait limits runner scheduling only. SetFanMax hold evidence already shows physical activity can outlast a requested wait. No indefinite hold, background reassertion, repeated writes, or claim that software timeout guarantees physical stop. On an unsafe event, stop candidate progression, follow only the pre-reviewed cleanup/recovery plan, record uncertainty, and require review before any further session.

## Manual Observations And Logs

Required future observation fields: operator and timestamps; baseline audible fan/airflow state for both fans where identifiable; response observed yes/no/unknown; ramp delay; duration; airflow change; fan asymmetry or stall; command/restore attempt times; observed return to baseline/auto and delay; temperatures and source identity; AC/battery state; workload; abnormal noise, temperature spike, freeze, crash/Windows event; unsafe-abort reason; sanitized notes. Unknown is explicit, never inferred from transport success or prefilled approval flags.

Use the existing `Logs/FanExperiments` directory with separate SetFanLevel research filenames and a session identifier. Preserve original dry-run records. Proposed future logs must append immutable preflight, intent, result, restore, and observation records with UTC timestamps, exact build/device identity, candidate bytes/length, reference/protocol revision, gate reasons, baseline/raw readbacks, attempted/succeeded/error status, and missing evidence. Do not rewrite a dry-run as an executed experiment. Record actual invocation state honestly; no-hardware markers from dry-run must never be reused as claims for a future write.

If post-write logging fails, cleanup still takes precedence and the session cannot count as successful evidence. Manual notes may be appended later with provenance; they cannot retroactively authorize the attempt. `DeviceValidatedInputLength` remains null and normal validation/control permission fields remain false.

## Outcome Criteria

| Outcome | Proposed interpretation |
| --- | --- |
| Blocked before write | Any admission gate is not established; no hardware attempt. This is the current state. |
| Research success only | All gates satisfied, one command returned success, independently observed expected response on the mapped fans, reviewed restore succeeded and return to automatic behavior was independently observed over the required interval, complete logs, and no safety issue. Still not normal-control validation. |
| Inconclusive | Command success with missing physical confirmation, raw readback change alone, `FanMaxGet=false`, unclear units/fan mapping, incomplete logs, or uncertain restore attribution. Sound/airflow alone cannot establish an exact target speed. |
| Command failure | Rejection, exception, or ambiguous completion; possible side effects remain unknown and planned cleanup is still required after an attempt. No retry. |
| Unsafe abort | Cooling loss/stall/asymmetry, abnormal sound, reviewed thermal limit crossed, AC loss, loss of monitoring, app/OS freeze/crash, or failed/unknown recovery. Unsafe status takes precedence over positive response evidence. |

FanMaxGet false cannot be the sole success or failure criterion. Inconclusive restore is a safety blocker, never successful recovery. Favorable transport results or user observations alone cannot satisfy the full gate.

## Explicitly Blocked Behaviors

- Real SetFanLevel implementation/execution in the current task and current dry-run route.
- Normal UI, slider/toggle, pulse/run button, tray/API control route, fan curves, and automatic control.
- Retries, background loops, persistent state across restarts, and concurrent experiments.
- EC/PawnIO/PwnIO/WinRing0 fallback, one-byte fallback, and alternate payload variants.
- SetFanMode, `0x37`, performance/power writes, or automatic SetFanMax preconditioning as an implicit prerequisite.
- Changing `DeviceValidatedInputLength` or claiming validated payload/normal fan control.

## Next Safe Task And Decisions

Following the [completed static audit](set-fan-level-abi-units-evidence-audit.md), prepare a documentation-only request for existing exact-device protocol/recovery evidence, including the conflicting buffer lengths and high-bit semantics. Do not request new writes or select a candidate. A pure fixture-only preflight evaluator may be considered separately, with no transport or hardware connection.

Developer-only four-byte SetFanMax Pulse/Hold remains operational under its existing explicit CLI gates. SetFanLevel dry-run proves serialization only; SetFanLevel first-write readiness remains **NO-GO**. `DeviceValidatedInputLength` remains null/unset, `FanMaxGet` inconclusive, and `FanGetLevel` raw-only. Normal/user-facing fan control remains **NO-GO** because ABI, reliable state feedback, durable recovery, repeatability, and thermal/power/failure evidence are incomplete.
