# Clean-Machine Validation Plan

## Purpose

This plan defines acceptance checks for a future `VictusX-HP-Diagnostic-*-win-x64-portable.zip`. It does not authorize publishing, hardware control, or developer-only WMI probes.

Use [Clean-Machine Validation Evidence Plan](clean-machine-validation-evidence-plan.md) to record artifact-specific validation evidence before this plan can be treated as completed release validation.

## Target Environment

- A clean Windows 10 or Windows 11 x64 virtual machine or separate standard-user profile.
- No prior VictusX install, extracted preview folder, launch shortcut, or `%APPDATA%\VictusX` data unless the run is explicitly testing upgrade or stale-report behavior.
- The future preview is expected to be self-contained; record the Windows edition/build and whether Windows Defender or other endpoint protection reports a warning.
- Test with normal user privileges and network unavailable or available; neither state should be required for the cached diagnostic shell.

## Portable Launch

1. Obtain the future approved portable ZIP from the maintainer and verify its recorded checksum/signature before extracting it to a writable user folder.
2. Confirm the package launcher is under `tools` beside `VictusX.exe` and start the launcher, not a bare executable.
3. Confirm the launcher supplies exactly:

```text
--hp-victus
```

`--hp-wmi-readonly-test` and all write/control arguments are forbidden from the package, shortcut, launcher, and test procedure.

## Expected Application Behavior

- The application opens visibly as `VictusX Read-only Diagnostic` without an elevation prompt.
- The shell exposes Diagnostic and Quit only, with local-only Copy summary, Reload cached report, Open report folder, and Export diagnostic report actions.
- No fan, performance, GPU, battery, lighting, ASUS control, updater, or control-like UI is visible or enabled.
- The dashboard clearly says that fan control is not implemented and SetFanMax is NO-GO/design-only.
- All explicit probe fields in `%APPDATA%\VictusX\hp-capability-report.json` remain `Attempted=false`; no `hpqBIOSInt*` method is invoked.

## Report and Export Checks

- Startup creates or safely updates `%APPDATA%\VictusX\hp-capability-report.json` with the expected schema metadata and normal HP diagnostic mode/source.
- Missing, empty, corrupt, and older-schema reports render a safe unavailable/read-error state without a crash.
- Reload reads only the local report; Copy summary contains no raw binary data; Open report folder opens only the VictusX app-data folder.
- Export creates a sanitized Markdown report under `%APPDATA%\VictusX\Logs\Reports\` and includes the read-only/no-control disclaimer.
- Exported content must not contain raw binary dumps, developer-only arguments, machine-specific diagnostic captures beyond the intended summary, or unsupported control claims.

## Shutdown and Stability Checks

- Close the window and use Quit in separate runs; both paths must exit without a Windows exception dialog.
- Confirm no `VictusX.exe` process remains in Task Manager after each exit.
- Review Windows Reliability Monitor and Event Viewer Application entries for the run window. A .NET unhandled exception, native access violation, crash report, or unexpected WMI/control activity blocks release.
- Confirm no startup registration, scheduled task, service, updater action, or installer residue was created by the portable preview.

## Package Contents Checklist

- Versioned executable and `tools` launcher are present; the launcher uses only `--hp-victus`.
- Explicit VictusX metadata and approved icon render in Explorer, shortcut, taskbar, tray, and the window.
- Applicable license text, G-Helper modified-project attribution, and reviewed third-party notices are present.
- Checksum/signature records, source revision, and run instructions match the artifact.
- No symbols, build logs, raw device captures, development settings, test flags, updater channel data, or unexpected binaries are included.

## Rollback and Removal

- Close the app first, then delete the extracted portable-preview folder and any manually created shortcut.
- Verify no process, startup registration, task, service, or installer entry remains.
- Preserve report/export files only when needed for support; otherwise remove `%APPDATA%\VictusX` deliberately after confirming it contains no needed user data.
- Record the artifact version, results, and any failure before retrying on a fresh environment.

## Release Blocking Criteria

Release remains blocked if any required item is absent, any explicit invocation is attempted, any control/write surface appears, report/export behavior is not local-only, the app crashes or leaves a process, package contents or notices cannot be verified, or signing/checksum review is incomplete. Passing this plan also does not authorize fan or performance control.

Clean-machine validation also remains blocked until the final artifact name, version, source commit, launch command, observations, process termination result, crash/event-log review, reviewer, and date are recorded as described in [Clean-Machine Validation Evidence Plan](clean-machine-validation-evidence-plan.md).

## Recommended Next Safe Task

Create a release-candidate evidence record template that ties this validation plan to package contents, notices, `NU1900` disposition, and signing/checksum evidence. Do not package or publish until the notice, icon, signing, and clean-machine blockers are all closed by a maintainer.
