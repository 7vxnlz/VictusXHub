# Clean-Machine Validation Evidence Plan

## Current Blocker State

Clean-machine validation remains open for the future VictusX HP Diagnostic preview. No preview package exists, no final artifact has been selected, and no clean Windows machine or VM validation has been completed against a release candidate.

This plan does not authorize publishing, artifact creation, signing, checksum generation, WMI probing, or release readiness.

See [HP Diagnostic Preview Final Blocker Tracker](hp-diagnostic-preview-final-blocker-tracker.md) for the consolidated release-blocker status that clean-machine validation feeds into.

## Expected Validation Environments

Use at least one clean Windows 10 or Windows 11 x64 machine or VM that has no development-only assumptions:

- no source checkout required;
- no Visual Studio, SDK, or test runner required for preview UI validation;
- no prior VictusX app data unless the run is explicitly testing upgrade or stale-data behavior;
- no local FanExperiments logs unless the run is explicitly testing local history migration;
- non-admin normal launch for preview UI validation;
- optional admin launch only for separately documented developer commands, not for preview UI acceptance.

The preview UI validation path must use the packaged launcher or shortcut that supplies only `--hp-victus`.

## Prerequisites To Validate

Before a clean-machine run can count as release evidence, confirm:

- .NET runtime versus self-contained packaging decision is documented;
- app icon and app identity blocker is resolved or explicitly still blocking;
- reviewed `THIRD-PARTY-NOTICES.md`, project license text, and G-Helper attribution are included when required;
- signing/checksum decision and evidence plan are ready for the same artifact;
- package contents list exists and excludes developer-only logs, captured device evidence, symbols, machine paths, and write-experiment outputs;
- `NU1900` audit-source warning disposition is recorded or remains an explicit release blocker.

## Validation Checklist

Record pass/fail/blocked evidence for each item:

- app launches with `--hp-victus`;
- read-only Diagnostic dashboard appears;
- pulse history/status fails closed when no local FanExperiments logs exist;
- proof gap analyzer fails closed when no local evidence exists;
- no normal fan control UI appears;
- no pulse/run button appears;
- no fan sliders, toggles, curves, or performance controls appear;
- no WMI write path is invoked;
- normal HP Diagnostic mode does not require `--hp-wmi-readonly-test`;
- standard window close hides to tray if that remains the designed behavior;
- explicit Quit terminates the `VictusX` process;
- logs/config/report/export paths are created under the expected user profile locations safely;
- missing HP WMI classes on non-target machines are handled without crash or write attempts;
- no Windows crash dialog, .NET unhandled exception, native access violation, Reliability Monitor failure, or Event Viewer Application error is produced by the validation run.

## Evidence Required

For each clean-machine validation run, record:

- OS edition, version, build, and architecture;
- machine type: physical HP Victus target, non-target HP machine, non-HP machine, or VM;
- user privilege level used for preview UI validation;
- package/artifact name;
- package/artifact version;
- source/build commit SHA;
- artifact checksum/signing evidence reference;
- launch command or shortcut target;
- observed app title and shell behavior;
- screenshots or textual observations for dashboard, tray/close/Quit, and absence of fan controls;
- report/export/log/config path observations;
- process termination result after explicit Quit;
- crash/event-log review result;
- reviewer and review date;
- unresolved issues and final result: passed, failed, or blocked.

## Fail-Closed Rules

- No final artifact means clean-machine validation cannot be completed.
- Validation against a source checkout or debug run does not replace package validation.
- Missing screenshot/textual observation evidence keeps the validation blocked.
- Any visible normal fan control UI, pulse/run button, fan slider, fan toggle, fan curve, or performance-control surface blocks release.
- Any WMI write invocation or developer-only experiment command in preview UI validation blocks release.
- Any package requiring developer-only flags for normal preview launch blocks release.
- Crash, process residue after explicit Quit, unsafe path creation, or missing-HP-class failure blocks release.
- Validation against an artifact that does not match signing/checksum evidence blocks release.

## What Remains Blocking

- No release-candidate artifact exists.
- No clean-machine validation evidence exists.
- Icon/app identity remains pending.
- Runtime dependency license/notice review remains pending.
- `THIRD-PARTY-NOTICES.md` remains draft/source-level.
- `NU1900` disposition remains open.
- Signing/checksum evidence remains open.
- Normal/user-facing fan control remains NO-GO.

## Recommended Next Safe Task

Create a release-candidate evidence record template that links [HP Diagnostic Preview Final Blocker Tracker](hp-diagnostic-preview-final-blocker-tracker.md), package contents, notices, `NU1900` disposition, signing/checksum evidence, and clean-machine validation without creating or publishing artifacts.
