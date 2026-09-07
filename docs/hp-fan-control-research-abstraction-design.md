# HP Fan Control Research Abstraction Design

## Purpose

Define a narrow internal boundary for future HP fan-control research without treating it as a product control API. The boundary exists to keep explicitly approved, developer-only research operations isolated from the read-only HP Diagnostic UI.

## Current Supported Operation

The only supported research metadata remains the developer-only four-byte SetFanMax Max Fan Pulse:

- enable payload: `01-00-00-00`
- matching restore payload: `00-00-00-00`
- command: `0x20008`, command type: `0x27`
- WMI class/method: `hpqBIntM` / `hpqBIOSInt0`

It is command-line-only, separately approval-gated, exact-device-gated, elevation-gated, AC-gated, baseline-gated, single-attempt, matching-restore, and append-only logged. The bounded [Max Fan Hold command](set-fan-max-developer-hold-command.md) reuses this exact operation metadata with an independently required approval and a `10`-to-`180`-second foreground pre-restore wait. The [first result](set-fan-max-developer-hold-first-result.md) shows that physical response can outlast that wait, so duration is runner execution policy, not a physical fan-duration control or new generic fan-control operation. Neither route is exposed through Settings, the tray, or the Diagnostic dashboard.

## Explicitly Unsupported Operations

The [closest-Victus SetFanLevel deep trace](omencore-victus-16s-setfanlevel-deep-trace.md) shows why neither the reference controller nor its factory belongs in the narrow contracts: scaled-zero requests, readback replay, equal-pair maintenance, retries, multi-command recovery and backend fallback extend far beyond one bounded operation. Caller-local buffer sizes and separate cleaning protocols do not validate a target ABI. No operation or permission is added; SetFanLevel first-write remains NO-GO.

The [SetFanLevel ABI/units audit](set-fan-level-abi-units-evidence-audit.md) found incompatible reference input lengths, ambiguous scaling/high-bit semantics, and close-device zero-handoff hazards. It selects no executable value or restore packet. Keep these as evidence only, outside the executable operation contracts; `DeviceValidatedInputLength` remains null and normal control remains **NO-GO**.

The [SetFanLevel dry-run scaffold](set-fan-level-dry-run-research-scaffold.md) is a separate pure parser/JSON record, not an `IHpFanResearchOperation` or transport command. It serializes an unvalidated two-byte hypothesis only and exits before hardware startup. The operation enum, catalog permissions, pulse/hold gates, and `DeviceValidatedInputLength` are unchanged. All execution restrictions below still apply.

This abstraction must not grow into normal fan control. The following remain unsupported and blocked:

- normal or user-facing fan control, fan curves, sliders, and toggles
- SetFanMode, SetFanLevel, and `0x37`
- EC writes, performance control, and power-limit writes
- background, scheduled, automatic, retrying, or fallback fan actions

## Internal Contracts

The pure `HpFanResearchContracts` layer records these narrow roles. Its fixed four-byte pulse metadata now flows into the existing command-line pulse path without changing runner behavior:

- `IHpFanResearchOperation`: one named, bounded research operation; it has no generic speed, curve, or mode input.
- `HpFanResearchRequest`: immutable operation identity, explicitly approved payload hypothesis, and manual-observation metadata. It must never represent a UI command or persisted user preference.
- `IHpFanResearchGateEvaluator`: returns a fail-closed admission decision from command-line approval, target identity, elevation, AC, baseline, and operation-specific conditions.
- `IHpFanResearchBaselineProvider`: supplies only the approved pre/post read-only snapshots used by a research operation.
- `IHpFanResearchTransport`: an internal, operation-specific transport boundary. It must accept only the exact allowed payloads for that operation and have no mode, level, curve, or arbitrary-command API.
- `IHpFanResearchLogSink`: appends an immutable research record under `%APPDATA%\VictusX\Logs\FanExperiments\`.
- `IHpFanResearchOutcomeClassifier`: separates command return, manually observed physical response, observed restore, and readback reliability without declaring product readiness.

`HpFanResearchOperationKind` still has only `FourByteMaxFanPulse`. The associated operation descriptor keeps `DeviceValidatedInputLength` null. `IHpFanResearchOperation` and `IHpFanMaxPulseResearchOperation` describe fixed metadata only; neither exposes execution or generic fan-control methods. The pulse and hold parsers carry that same operation, while the existing runner accepts each through narrow overloads. The hold cannot select payload length and contributes only a validated bounded duration plus log metadata.

Existing `HpFanMaxExperimentRunner`, `HpFanMaxExperimentWmiTransport`, baseline provider, log writer, and outcome classifier already map to these responsibilities. Pulse and hold share the fixed bytes, no-retry/no-fallback behavior, restore path, and log location; the hold adds only its own CLI approval and bounded delay. Keep any future refactor internal and behavior-preserving; do not create a broad `IFanControlService`.

The [contract-refactor runtime verification](set-fan-max-pulse-contract-refactor-verification.md) records the same four-byte enable/restore behavior with observed response and restore. It is developer-only evidence, not normal-control validation.

The [OmenXHub/OmenSuperHub implementation comparison](omenxhub-omensuperhub-fan-implementation-comparison.md) confirms a common BIOS-only SetFanMax identity but also shows why this boundary must stay narrow. OmenSuperHub is the closer source to study for explicit Max Fan on/off semantics; OmenXHub offers useful transport-serialization evidence but also contains PawnIO direct-EC fallback, timer-driven SetFanLevel, SetFanMode, `0x37`, and broader control APIs. None of those broader paths belong in this abstraction.

The [normal fan control go/no-go evidence matrix](normal-fan-control-go-no-go-evidence-matrix.md) defines the broader evidence threshold that must be met before any user-facing fan-control design can move out of NO-GO.

## Required Safety Gates

Every future research operation must remain opt-in and fail closed:

- explicit developer command, acknowledgement, and operation-specific human approval
- exact HP Victus target identity, currently SKU `7Z5Z2EA#AB8`, BIOS `F.31`, and ThermalPolicyVersion `1`
- elevation and confirmed AC power
- successful approved read-only baseline, including `FanGetCount=2` and baseline `FanMaxGet=false` where applicable
- one declared payload hypothesis, one enable attempt, no retry, and no alternate-payload fallback
- matching restore in a `finally` path after an enable attempt
- readback and append-only logging even when the result is unknown or failed

## Logging And Diagnostic Boundary

Research logs must record the gate decision, exact payload bytes, baseline, enable/restore attempts, readbacks, manual observations, classifier result, blocked reasons, and whether a write was actually attempted. Logs are evidence, not authorization.

Hold CLI output and logs now explicitly describe a bounded pre-restore wait. `RequestedPreRestoreWaitSeconds` exposes the existing `RequestedHoldSeconds` value without breaking older records; `HoldDurationSemantics` states BIOS-dependent, unvalidated physical duration and continued normal-control NO-GO. The [hold semantics checkpoint](set-fan-max-hold-semantics-checkpoint.md) records this final clarified state. The wait is not an exact restore deadline because readback follows it. Manual observation arguments already exist and affect logging/classification only; runner timing, gates, payloads, and restore behavior are unchanged.

The HP Diagnostic dashboard may read the latest valid local log for status/history only. It must remain local-file-only, never invoke WMI, never refresh hardware, and expose no pulse/run button, fan toggle, slider, tray route, or other control route.

## Remaining Evidence Limits

`DeviceValidatedInputLength` remains unset because repeated physical response does not prove that one input shape is the durable, supported ABI for normal control. The four-byte pulse is preferred only for bounded developer research; one byte remains comparison-only.

`FanMaxGet` remains inconclusive: it stayed `false` during observed physical fan response. It cannot be the sole success criterion or a normal-control state contract. Raw `FanGetLevel` values remain observational only.

## Safe Extension Rules

A future research operation needs its own written protocol, exact-device evidence, dedicated approval flag, bounded payload set, baseline/readback plan, restore proof, abort/recovery plan, append-only logging, and pure gate/classifier tests. It must not inherit permission from the pulse path merely because it targets fans.

Before any normal fan-control UI can be considered, the project needs a separately reviewed product decision selecting a validated input length; durable restore semantics; a reliable success/state criterion independent of FanMaxGet alone; repeated thermal, AC/battery, failure, cancellation, and recovery evidence; and a user-facing safety design. Until then, normal fan control remains **NO-GO**.

## Recommended Next Implementation Step

The [SetFanLevel first-write preflight design](set-fan-level-first-write-preflight-design.md) is documentation-only and leaves the candidate set empty until ABI and recovery evidence exists. Do not connect the dry-run record to the research transport or treat SetFanMax approval as SetFanLevel authorization.

Prepare the audit's documentation-only request for existing exact-device ABI and recovery evidence; do not select a first-write value or request new experiments. Keep the dry-run record disconnected from transport. Future hold observation protocols must still distinguish pre-restore wait from physical duration; neither research path may add UI, background control, or fallback behavior.
