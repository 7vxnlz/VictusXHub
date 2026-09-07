# HP Diagnostic Preview Readiness Checkpoint

## Current HP Diagnostic Feature Set

HP Diagnostic mode launches with `--hp-victus` as a read-only diagnostic shell. It provides the cached/report-backed Diagnostic dashboard, local copy/export actions, cached report reload, report-folder opening, read-only SetFanMax readiness status, read-only pulse history/status, and read-only fan proof-gap analysis.

No normal fan control surface is implemented.

## Read-Only Dashboard Status

The dashboard remains a status and evidence view. It uses cached diagnostic data and local files, and normal `--hp-victus` mode does not run explicit HP WMI probes.

The dashboard continues to state that fan control is not implemented, SetFanMax normal control is not validated, and user-facing fan control is not allowed.

## Pulse History/Status Status

The SetFanMax pulse history view reads local append-only JSON logs from `%APPDATA%\VictusX\Logs\FanExperiments\`. It displays the latest valid developer-only experiment/pulse evidence as read-only status.

It has no pulse button, fan slider, fan toggle, retry, fallback, automatic write, background write, or tray route.

## Proof Gap Analyzer Status

The HP fan proof gap analyzer summarizes local FanExperiments logs and the cached HP capability report. It appears in the dashboard, copy summary, and exported diagnostic report.

Missing, stale, corrupt, truncated, or invalid evidence fails closed. The analyzer cannot invoke WMI, execute a pulse, run an experiment, select a payload length, or change a safety decision.

## Close-To-Tray And Quit Behavior

Standard window close hides the HP Diagnostic window to the tray by design. This is expected behavior and should not be treated as a shutdown failure.

Explicit tray Quit is the full process-termination path. A prior Quit fix confirmed diagnostic tray cleanup and UI-loop exit. The latest desktop automation could verify startup and close-to-tray behavior, but it could not target the tray Quit menu.

## Fan-Control Safety Decisions

| Area | Current decision |
| --- | --- |
| Developer-only 4-byte Max Fan Pulse | Operational under explicit command-line gates |
| Normal/user-facing fan control | **NO-GO** |
| `DeviceValidatedInputLength` | Unset / not validated |
| `FanMaxGet` | Inconclusive |
| `FanGetLevel` | Raw-only |

## Explicit Blocked Fan-Control Items

- no normal fan control UI
- no fan sliders, toggles, or buttons
- no pulse/run button
- no fan curves
- no automatic or background fan writes
- no generic fan-control API
- no SetFanMode, SetFanLevel, `0x37`, EC write, performance write, retry, or fallback path
- no `DeviceValidatedInputLength` update
- no normal-control payload validation

## Current Verification Status

Current source verification is build/test clean with the known NU1900 audit-source warnings caused by unavailable NuGet vulnerability metadata. Tests are passing at 199/199.

Normal HP Diagnostic launch with `--hp-victus` opens the `VictusX Read-only Diagnostic` window. Normal close hides to tray by design. Verification must not use `--hp-wmi-readonly-test`, `--hp-fan-write-experiment`, or `--hp-fan-max-pulse`.

## Remaining Preview Blockers

- icon/app identity remains blocked until inherited visual identity is replaced with approved VictusX assets
- package license/notice review remains incomplete
- signing/checksum evidence is not complete for a final package
- clean-machine validation has not been run against a final candidate package
- NU1900 audit-source warnings need disposition before release candidate acceptance
- no release package or shortcut may include `--hp-wmi-readonly-test`
- no normal fan control UI may be added for the diagnostic preview

## Preview Readiness Decision

The source is ready to continue packaging/release-prep documentation and source-only hardening for an HP Diagnostic preview. It is not ready to publish a preview package until the release blockers are closed.

Fan control remains blocked: developer-only pulse evidence does not authorize normal or user-facing fan control.

## Recommended Next Safe Task

Prepare a source-only release-candidate validation checklist that ties together package contents, icon identity, notice review, checksum/signing evidence, clean-machine validation, and NU1900 warning disposition without publishing artifacts.
