# HP Fan Proof Gap Analyzer Checkpoint

## Analyzer Purpose

The HP fan proof gap analyzer summarizes the current SetFanMax evidence state from existing local evidence only. It is a read-only research/status surface for the HP Diagnostic dashboard, not a control feature.

Its job is to keep the remaining normal fan-control blockers visible:

- `DeviceValidatedInputLength` remains unset.
- `FanMaxGet` remains inconclusive.
- `FanGetLevel` remains raw-only.
- normal/user-facing fan control remains **NO-GO**.

## Local-Only Evidence Sources

The analyzer reads:

- append-only SetFanMax experiment JSON logs under `%APPDATA%\VictusX\Logs\FanExperiments\`
- the cached HP capability report at `%APPDATA%\VictusX\hp-capability-report.json`

It does not run probes, invoke WMI, execute commands, start experiments, or infer missing evidence from hardware.

## Dashboard, Copy, And Export Visibility

The analyzer output is shown as read-only text in:

- the HP Diagnostic dashboard
- copy summary output
- exported diagnostic Markdown

The visible status is evidence-oriented only. It does not add, expose, or route to any fan-control action.

## Fail-Closed Behavior

Missing, empty, corrupt, truncated, or invalid log/report data remains safe:

- missing pulse evidence reports no optimistic normal-control status
- invalid JSON is ignored or summarized safely
- missing fields display unknown, unvalidated, or blocked status
- stale cached reports cannot set `DeviceValidatedInputLength`
- normal fan-control readiness remains **NO-GO**

## Current Decisions

| Area | Current decision |
| --- | --- |
| Developer-only 4-byte Max Fan Pulse | Operational under explicit command-line gates only |
| Normal/user-facing fan control | **NO-GO** |
| `DeviceValidatedInputLength` | Unset / not validated |
| `FanMaxGet` | Inconclusive; it cannot confirm the max-fan latch |
| `FanGetLevel` | Raw-only; not RPM, percent, curve data, or control state |

## No-Control Guarantees

The analyzer and dashboard integration provide no fan-control surface:

- no normal fan UI
- no fan sliders
- no fan toggles
- no fan buttons
- no pulse/run button
- no automatic writes
- no background writes
- no WMI invocation from the analyzer
- no SetFanMode, SetFanLevel, `0x37`, EC, fan-curve, or performance-control route

## Close And Quit Behavior

Standard window close in HP Diagnostic mode hides the window to the tray by design. That behavior is not a failure condition.

Explicit Quit is expected to terminate the process. The prior HP Diagnostic Quit fix confirmed that the diagnostic tray resources are disposed and the UI message loop exits. The latest desktop verification after the proof-gap analyzer work could confirm HP mode startup and close-to-tray behavior, but the automation could not target the tray Quit menu; that limitation should be reported separately from the close-to-tray behavior.

## Recommended Next Safe Task

Design restore/recovery proof criteria using the analyzer's fail-closed summary as input. That next task should remain documentation/read-only unless separately authorized, and it should not run experiments, invoke WMI, or add fan-control UI.
