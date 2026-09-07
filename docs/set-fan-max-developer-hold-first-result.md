# First Developer SetFanMax Hold Result

## Command Run

```powershell
dotnet run --project app\VictusX.csproj -- --hp-victus --hp-wmi-readonly-test --hp-fan-max-hold --i-understand-this-can-affect-fans --i-approve-4-byte-max-fan-hold --max-fan-hold-seconds=10
```

This was a manually authorized, developer-only exact-device run. No additional runtime action was taken while recording this result.

## JSON Result Summary

- Enable: attempted and succeeded; `EnableCommandSucceeded=true`.
- Post-enable readback: `FanMaxGet=false`, `FanMaxGetConfirmedEnable=false`, raw FanGetLevel `3A-3D`.
- Restore: attempted and succeeded; `RestoreCommandSucceeded=true`.
- Post-restore readback: `FanMaxGet=false`, raw FanGetLevel `39-3D`.
- `WriteExecuted=true`, `FirstWriteGateSatisfied=false`, and `DeviceValidatedInputLength=null`.
- `ReadbackReliability=Inconclusive`, `Outcome=Fail`, and `ExperimentalOutcomeClassification=CommandSucceededNoPhysicalConfirmation`.

No observation arguments were supplied. The log therefore has `PhysicalFanResponseObserved=null`, `RestoreObserved=null`, and the default note that physical response and restore state remain unknown. Raw FanGetLevel values are observational only; they are not decoded as RPM, percent, or latch state.

## Manual Observation

The fan did not stay high for only about ten seconds. It remained high for approximately two minutes, then eventually returned/stopped after the restore/timeout period. No abnormal noise, overheating, freeze, or crash was observed.

This manual observation is evidence outside the original JSON record. It must not be retroactively treated as a logged physical-response or restore observation.

## Hold Duration Semantics

`--max-fan-hold-seconds=10` requests a ten-second pre-restore wait after the single enable attempt. The runner then performs readback before attempting the matching restore, so scheduling and readback time can extend the elapsed time before restore. This is neither an exact restore deadline nor proven physical fan-duration control.

The approximately two-minute physical response can outlast the requested wait, which is consistent with BIOS-latched or firmware-timed behavior. The result does not prove the latch model, the exact reason for the duration, or that restore directly determines when the physical fan stops.

Subsequent source changes clarify CLI/error output and add `RequestedPreRestoreWaitSeconds` plus `HoldDurationSemantics` to serialized hold records, retaining `RequestedHoldSeconds` for compatibility. The final clarified state is checkpointed in [set-fan-max-hold-semantics-checkpoint.md](set-fan-max-hold-semantics-checkpoint.md). Those changes do not alter this historical JSON result, add missing observations, or change fan behavior or readiness.

## Safety Interpretation

The enable and matching restore commands both returned success, and no adverse behavior was reported. That is useful bounded developer evidence, but it does not establish durable restore semantics, thermal safety across sessions, a reliable state readback, or normal-control behavior.

`Outcome=Fail` is expected under the existing mapping because FanMaxGet stayed false after enable, producing the logged unconfirmed-enable reason. `CommandSucceededNoPhysicalConfirmation` is also expected because no manual physical-response or restore flags were provided. A manually observed response does not alter those fields unless it is supplied on a future separately approved run.

## Decision

- Developer-only four-byte Max Fan Pulse/Hold: operational only under explicit CLI gates.
- Hold duration as direct physical-duration control: not validated.
- FanMaxGet: inconclusive.
- DeviceValidatedInputLength: null/unset.
- Normal/user-facing fan control: **NO-GO**.

## Recommended Next Safe Task

Design a documentation-only operator observation protocol for any future separately authorized hold run. It should record requested pre-restore wait, approximate fan-ramp and cessation times, manual physical-response/restore flags, and safety observations without changing the command, running an experiment, or exposing any UI control.
