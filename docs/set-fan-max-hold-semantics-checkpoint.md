# SetFanMax Hold Semantics Checkpoint

## Purpose

The developer-only SetFanMax Hold command exists for bounded HP Victus fan research. It reuses the proven VictusX four-byte SetFanMax safety model, but it is not normal fan control and has no Settings, tray, Diagnostic, or user-facing UI route.

## CLI Gate Summary

The command is reachable only through the explicit developer command line:

```powershell
dotnet run --project app\VictusX.csproj -- --hp-victus --hp-wmi-readonly-test --hp-fan-max-hold --i-understand-this-can-affect-fans --i-approve-4-byte-max-fan-hold --max-fan-hold-seconds=10
```

The runtime also preserves the existing safety gates:

- exact HP Victus target identity: SKU `7Z5Z2EA#AB8`, BIOS `F.31`, ThermalPolicyVersion `1`
- administrator elevation
- AC power online
- successful read-only baseline capture
- `FanGetCount=2`
- baseline `FanMaxGet=false`

Missing or invalid command flags, missing approval, missing elevation, AC offline/unknown, wrong target identity, failed baseline, wrong fan count, or baseline max-fan state all fail closed before any SetFanMax write attempt.

## Duration Semantics

`--max-fan-hold-seconds` is a bounded pre-restore wait. It controls how long the foreground runner waits after the single enable attempt before performing readback and attempting the matching restore.

It is not validated physical fan-duration control. Scheduling and post-wait readback can add elapsed time before restore is attempted, and firmware behavior may keep the fan high after restore or after the requested wait expires. Physical fan duration is BIOS-dependent and remains unvalidated.

## First Hold Result

The first manual ten-second hold result is recorded in [set-fan-max-developer-hold-first-result.md](set-fan-max-developer-hold-first-result.md).

Summary:

- requested pre-restore wait: `10` seconds
- enable command: attempted and succeeded
- restore command: attempted and succeeded
- `FanMaxGet` stayed `false`
- manual observation: fan activity lasted approximately two minutes, not ten seconds
- no abnormal noise, overheating, freeze, or crash was reported

This confirms command execution and restore return success for the bounded developer route. It does not prove direct duration control, durable restore semantics, reliable readback, or normal fan-control readiness.

## Log And Output Fields

Hold logs preserve compatibility with older records while adding clearer semantics:

- `RequestedPreRestoreWaitSeconds`: descriptive field for the requested bounded pre-restore wait
- `RequestedHoldSeconds`: retained compatibility field with the same stored value
- `HoldDurationSemantics`: explains that the wait is not validated physical fan-duration control
- `PhysicalFanResponseObserved`: optional manual observation metadata
- `RestoreObserved`: optional manual observation metadata
- `UnsafeAbortObserved`: optional safety observation metadata
- `ManualObservationNotes`: sanitized operator notes

Manual observation fields affect logging and outcome classification only. They cannot bypass approvals, gates, payload restrictions, or hardware safety checks.

## Safety Boundaries

The hold path remains intentionally narrow:

- four-byte SetFanMax only: enable `01-00-00-00`, restore `00-00-00-00`
- no one-byte fallback
- no retry
- no alternate-payload fallback
- no EC path
- no PawnIO/PwnIO, WinRing0, or LibreHardwareMonitor fan-write path
- no SetFanMode
- no SetFanLevel
- no command `0x37`
- no fan curve
- no normal UI route
- no pulse/hold button
- no automatic or background execution

## Current Decisions

- Developer-only four-byte Max Fan Pulse/Hold: operational under explicit CLI gates only.
- Hold duration physical control: not validated.
- FanMaxGet: inconclusive.
- FanGetLevel: raw-only.
- DeviceValidatedInputLength: null/unset.
- Normal/user-facing fan control: **NO-GO**.

## Recommended Next Safe Fan-Research Task

Design a documentation-only observation protocol for any future separately authorized hold run. It should record requested pre-restore wait, observed ramp delay, observed high-fan duration, observed restore/return timing, thermal/power state, abnormal sounds, freezes, crashes, and operator notes. It must not run an experiment, expose a UI control, add fallback behavior, or change any readiness decision.
