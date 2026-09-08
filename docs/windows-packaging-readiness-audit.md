# Windows Packaging Readiness Audit

## Current Naming and Output

- The solution, project, assembly, executable, and ZIP target use `VictusX` / `VictusX.exe`.
- The manifest identity is `VictusX.app`, x64 is the only configured platform, and the app runs `asInvoker`.
- Internal `GHelper` root namespace, startup object, resource names, and single-instance event naming remain from the imported base.
- Explicit VictusX metadata now defines product, read-only diagnostic description, contributor attribution, copyright, and coherent `0.1.0-preview.1` / `0.1.0.0` version fields.

## Branding and Metadata Risks

- The owner-approved `VictusX.Source.png` and generated multi-resolution `VictusX.ico` now provide distinct executable and HP tray identity; provenance, hashes, and wiring are recorded in the [icon checkpoint](victusx-icon-wiring-checkpoint.md).
- The former icon plan and implementation plan are retained as superseded historical records.
- The project now supplies explicit VictusX product, description, company/authors, copyright, assembly, file, and informational version metadata. A future packaged smoke test must still confirm the final executable properties match those values.
- G-Helper and ASUS strings/resources remain in the binary. HP shell isolation hides inherited control/update surfaces, but packaging must not imply that those features support HP hardware.
- GPLv3, modified-from-G-Helper attribution, seven direct plus one transitive application-package notices, exact .NET 10.0.11 runtime notices, NvAPIWrapper treatment, and icon provenance are reviewed and current-candidate matched.
- `docs/dependency-notice-inventory.md` records the exact current restore graph; test-only packages are absent from the inspected candidate.
- The completion plan, reviewed `THIRD-PARTY-NOTICES.md`, and runtime evidence checklist preserve the verified notice baseline for exact-distributable matching.
- [NU1900 Audit-Source Warning Disposition Plan](nu1900-audit-source-warning-disposition-plan.md) defines how the recurring package vulnerability audit-source warnings must be investigated before preview packaging.

## Package and Installer Status

- The HP preview profile creates a self-contained, single-file `win-x64` build with exact .NET 10.0.11 runtime notice evidence packaged externally.
- Publish currently creates an unversioned `VictusX.zip`.
- No installer/MSIX/WiX/Inno Setup project, release workflow, code signing, checksum generation, update channel, uninstall behavior, or packaged shortcut definition exists.
- [Signing and Checksum Workflow](signing-checksum-workflow.md) defines the future evidence needed before distribution; no signing or checksum has been performed.
- [Signing And Checksum Evidence Plan](signing-checksum-evidence-plan.md) defines the artifact-specific evidence that must be recorded before a preview can be released.
- Imported hardware-oriented resources and dependencies remain in the executable even though HP Diagnostic mode does not use their control paths.

## Required HP Diagnostic Runtime

A diagnostic preview must launch with exactly:

```text
--hp-victus
```

The IDE launch profile is not embedded into a published executable. Any preview shortcut or launcher must explicitly include this flag. A bare executable preserves the default G-Helper/ASUS behavior and is not a safe HP diagnostic preview entry point.

`--hp-wmi-readonly-test` must never appear in release profiles or shortcuts. It enables a separate elevated developer-only probe gate and is not required for the cached diagnostic shell.

## Safety Status

- HP mode is diagnostic-only and report-backed.
- HP mode preserves the designer-owned inherited shell with unsupported controls disabled; its only HP-specific footer addition is Diagnostic, which opens the vertically scrollable read-only pulse-history and proof-gap sections as an owned side panel, analogous to the inherited Updates panel.
- The HP inherited-shell UI visual blocker is resolved by manual confirmation; the compact shell, readable footer labels/icons, aligned disabled captions, and Diagnostic side-panel behavior are recorded in [HP Inherited Shell UI Final Checkpoint](hp-inherited-shell-ui-final-checkpoint.md).
- The [HP UI/usefulness gap audit](hp-victus-ui-usefulness-gap-audit.md) keeps the accepted visual restoration separate from capability proof. A narrow OS-only status provider now supplies CPU load and battery/presence/AC/charging state; device status uses existing snapshot/cached evidence. CPU/GPU temperature and fan RPM remain unavailable rather than guessed. This does not close package or control gates.
- Normal HP fan and performance control remain unavailable; no active user-facing write route is exposed.
- Developer-only four-byte SetFanMax Max Fan Pulse is operational under explicit CLI gates only; it is not a Diagnostic action or normal-control validation.
- Normal fan control remains NO-GO, `DeviceValidatedInputLength` stays unset, FanMaxGet stays inconclusive, and FanGetLevel stays raw-only.

## Pre-release Checklist

- Treat [HP Diagnostic Preview Release Blockers](hp-diagnostic-preview-release-blockers.md) as the consolidated source-of-truth release gate before any preview package is published.
- Use [HP Diagnostic Preview Final Blocker Tracker](hp-diagnostic-preview-final-blocker-tracker.md) as the current blocker/evidence table for source-only release prep.
- Use [HP Diagnostic Source-Only Readiness Final Checkpoint](hp-diagnostic-source-only-readiness-final-checkpoint.md) as the final source-only readiness handoff before asset, license, and artifact-specific work.
- Use [HP Inherited Shell UI Final Checkpoint](hp-inherited-shell-ui-final-checkpoint.md) as the manual visual confirmation that HP mode preserves the compact inherited shell, readable footer, aligned disabled captions, and Diagnostic side panel.
- Review [HP Diagnostic Preview Readiness Checkpoint](hp-diagnostic-preview-readiness-checkpoint.md) for the current read-only dashboard, proof-gap analyzer, close-to-tray behavior, and remaining blocker status.
- Review [HP Diagnostic Preview Source Readiness Audit](hp-diagnostic-preview-source-readiness-audit.md) before selecting any package candidate.
- Confirm the explicit product/version/publisher metadata and approved VictusX icon in the exact packaged executable.
- Choose and document framework-dependent versus self-contained runtime packaging.
- Add a dedicated HP diagnostic publish profile and versioned artifact name.
- Provide a shortcut or launcher that always supplies only `--hp-victus`.
- Confirm inherited control/update surfaces remain unreachable in the packaged HP entry path.
- Verify clean-machine startup, report/export paths, missing-runtime behavior, upgrade/uninstall behavior, and clean shutdown.
- Run build/tests and a packaged smoke test with every explicit invocation field `Attempted=false`.
- Preserve the reviewed license/third-party notices; add final checksum, signing decision/evidence, and any required distribution scans before public distribution.
- Complete the signing/checksum workflow against the same final artifact used for clean-machine validation.
- Record artifact name, version, source commit, signing status, certificate details when applicable, SHA-256 hash, reviewer, date, and final artifact validation in the signing/checksum evidence plan.
- Review the third-party notices audit, preserve applicable upstream notices, and verify direct and transitive package attribution from authoritative metadata.
- Repeat the reviewed dependency notice inventory and runtime evidence checks against the exact distributable.
- Dispose the recurring `NU1900` audit-source warnings with clean restore/build/test and vulnerability-list evidence, or retain the release block with a documented maintainer decision.
- Confirm no developer-only flags, symbols, logs, machine paths, or captured device data are shipped.
- Execute the [Clean-Machine Validation Plan](clean-machine-validation-plan.md) against the final candidate package and retain the evidence record described by [Clean-Machine Validation Evidence Plan](clean-machine-validation-evidence-plan.md).

## Change Now

Keep the explicit VictusX metadata and approved icon hashes aligned with the future artifact version. Final visual confirmation remains part of clean-machine validation.

See [Third-Party Notices Audit](third-party-notices-audit.md) for the source-attribution and package-notice distribution gate.
See [Package License And Third-Party Notices Completion Plan](package-license-third-party-notices-completion-plan.md) for the completed notice checklist and exact-distributable repeat requirements.
See the [VictusX Icon Wiring Checkpoint](victusx-icon-wiring-checkpoint.md) for the implemented HP-preview identity, provenance, and validation record.
See [Signing and Checksum Workflow](signing-checksum-workflow.md) for future release integrity evidence.
See [Signing And Checksum Evidence Plan](signing-checksum-evidence-plan.md) for the exact artifact-level evidence required before release.
See [Clean-Machine Validation Plan](clean-machine-validation-plan.md) for the future portable-preview acceptance procedure.
See [Clean-Machine Validation Evidence Plan](clean-machine-validation-evidence-plan.md) for the artifact-specific clean-machine evidence required before release.
See [HP Diagnostic Preview Release Blockers](hp-diagnostic-preview-release-blockers.md) for the consolidated release blocker checklist.
See [HP Diagnostic Preview Final Blocker Tracker](hp-diagnostic-preview-final-blocker-tracker.md) for the concise blocker/evidence decision table.
See [HP Diagnostic Source-Only Readiness Final Checkpoint](hp-diagnostic-source-only-readiness-final-checkpoint.md) for the final source-only package-readiness decision before asset, license, and artifact-specific work.
See [HP Inherited Shell UI Final Checkpoint](hp-inherited-shell-ui-final-checkpoint.md) for the restored HP inherited-shell visual checkpoint after manual confirmation.
See [HP Diagnostic Preview Source Readiness Audit](hp-diagnostic-preview-source-readiness-audit.md) for the current source-only package readiness decision.
See [HP Diagnostic Preview Readiness Checkpoint](hp-diagnostic-preview-readiness-checkpoint.md) for the stabilized HP Diagnostic preview state after proof-gap analyzer integration.
See [NU1900 Audit-Source Warning Disposition Plan](nu1900-audit-source-warning-disposition-plan.md) before treating build/test success as package vulnerability audit evidence.

## Wait Until Later

Defer automatic updates, startup registration, a production installer/store package, fan/performance UI claims, hardware-control permissions, and any control-oriented shortcut until the corresponding HP behavior is implemented and safety-validated. Do not globally rename internal namespaces or remove default G-Helper behavior as part of preview packaging.

## Recommended Next Safe Task

The next product task is a read-only source-validation design for CPU/GPU temperature and fan RPM, scoped by the [usefulness gap audit](hp-victus-ui-usefulness-gap-audit.md), with no HP BIOS-method probe, EC, driver, or experiment invocation. Packaging remains source-only: final package review, signing/checksum, and clean-machine gates stay open. Do not publish binaries or enable normal fan control.

See [HP Diagnostic Publish Profile Design](hp-diagnostic-publish-profile-design.md) for the proposed artifact name, dedicated launcher contract, deployment choice, and fail-closed pre-release checklist.
