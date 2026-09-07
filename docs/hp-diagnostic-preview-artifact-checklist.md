# HP Diagnostic Preview Artifact Checklist

## Purpose

`app/Properties/PublishProfiles/VictusX-HP-Diagnostic-win-x64.pubxml` is the source-side profile for a future self-contained `win-x64` HP diagnostic preview. It is not a release instruction and does not publish an artifact by itself.

`tools/run-victusx-hp-diagnostic.ps1` is the future portable-package launcher source. When placed under a package `tools` folder beside `VictusX.exe`, it starts the application with exactly:

```text
--hp-victus
```

## Required and Forbidden Arguments

- Required runtime argument: `--hp-victus`.
- Forbidden from the profile, launcher, shortcuts, and release instructions: `--hp-wmi-readonly-test` and all future write/control arguments.
- The normal diagnostic shell is local/report-backed; explicit developer probes are not part of a preview artifact.

## Before Any Preview Publish

- Review [HP Diagnostic Preview Release Blockers](hp-diagnostic-preview-release-blockers.md) as the final consolidated release gate.
- Review [HP Diagnostic Preview Source Readiness Audit](hp-diagnostic-preview-source-readiness-audit.md) and confirm every remaining blocker has final evidence.
- Review [HP Diagnostic Preview User Safety Notes](hp-diagnostic-preview-user-safety-notes.md) and include or link them from preview release instructions.
- Confirm the explicit VictusX version, product, publisher, and copyright metadata in the packaged executable; replace the inherited icon and add license and notice metadata.
- Review the icon/app identity plan and [VictusX Icon Asset Requirements](victusx-icon-asset-requirements.md); use an original or licensed multi-resolution icon, preserve shared default-mode resources, and verify executable, tray, window, and shortcut rendering in the packaged HP preview.
- Include the applicable license text, prominent G-Helper modified-project attribution, and a reviewed third-party notice inventory for resolved packages and dependencies.
- Complete [Package License Review Workflow](package-license-review-workflow.md) for runtime direct and transitive dependencies before any package is distributed.
- Review [Dependency Notice Inventory](dependency-notice-inventory.md) from a clean restore and reconcile it with final package contents before distribution.
- Confirm the notice inventory against authoritative package metadata and the final package contents; do not treat this repository audit as legal advice.
- Confirm the launcher is packaged under `tools` beside `VictusX.exe` and that its only application argument is `--hp-victus`.
- Run a clean-machine packaged smoke test: diagnostic-only shell, no elevation, no control/update surfaces, and every explicit invocation field `Attempted=false`.
- Run the clean-machine validation plan using the package launcher only; verify report/export paths, Event Viewer and Reliability Monitor, process cleanup, and portable-folder removal.
- Verify report loading and local-only copy, reload, folder-open, and export behavior for missing and corrupt reports.
- Follow [Signing and Checksum Workflow](signing-checksum-workflow.md): produce and verify a SHA-256 checksum for the final validated artifact, and add timestamped Authenticode signing when a verified publisher identity is available.
- Exclude logs, raw device captures, machine-specific paths, and symbols from distribution.

## Repository Safety Checks

`HpDiagnosticPreviewConfigurationTests` reads only the launcher's and publish profile's source text. It verifies that the launcher supplies `--hp-victus`, neither source file contains `--hp-wmi-readonly-test`, and the profile opts out of the inherited ZIP-on-publish target. The tests do not execute the launcher or invoke `dotnet publish`.

## Release Status

Release remains blocked by the consolidated [HP Diagnostic Preview Release Blockers](hp-diagnostic-preview-release-blockers.md). No fan or performance control, fan writes, or hardware-control support is included or claimed. SetFanMax remains NO-GO/design-only.
