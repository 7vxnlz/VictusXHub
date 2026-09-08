# HP Diagnostic Preview Release Blockers

## Current Release Status

The VictusX HP diagnostic preview is not release-ready. HP diagnostic mode is stable and read-only, but distribution remains blocked until notice, identity, signing, checksum, and clean-machine validation evidence is complete.

No binaries or release artifacts should be published from the current state.

See [HP Diagnostic Preview Source Readiness Audit](hp-diagnostic-preview-source-readiness-audit.md) for the final source-only readiness summary.

See [HP Diagnostic Preview Readiness Checkpoint](hp-diagnostic-preview-readiness-checkpoint.md) for the post-proof-gap-analyzer preview state, including read-only fan proof status, close-to-tray behavior, and the remaining packaging blockers.

See [HP Diagnostic Preview Final Blocker Tracker](hp-diagnostic-preview-final-blocker-tracker.md) for the consolidated current blocker table and required evidence.

See [HP Diagnostic Source-Only Readiness Final Checkpoint](hp-diagnostic-source-only-readiness-final-checkpoint.md) for the final source-only readiness decision before asset, license, and artifact-specific work.

## Completed Prerequisites

- HP diagnostic mode launches with `--hp-victus` and preserves the inherited compact G-Helper-style shell with unsupported HP controls disabled/read-only.
- HP read-only diagnostic dashboard is report-backed and local-file-only.
- HP inherited-shell UI visual confirmation is recorded: footer labels/icons are readable, disabled captions are aligned, and Diagnostic opens as an owned read-only side panel.
- User-facing safety notes exist for the future preview entry path.
- Final source-only readiness audit exists for the current preview package state.
- HP capability report schema metadata persists.
- Publish profile and launcher source files exist for a future HP diagnostic preview.
- Safety tests guard the launcher/profile from including `--hp-wmi-readonly-test`.
- Project metadata is aligned to VictusX preview identity while keeping inherited default behavior intact.
- Dependency notice inventory, icon/app identity plan, and clean-machine validation plan exist.
- HP fan proof-gap analyzer and preview readiness checkpoints exist after dashboard stabilization.

## Blocking Items

Treat [HP Diagnostic Preview Final Blocker Tracker](hp-diagnostic-preview-final-blocker-tracker.md) as the concise source-of-truth table for current release blockers.

- Preserve the resolved inherited-shell UI state during future release-prep changes.
- Preserve the completed package-license/notice review and repeat it against the exact distributable.
- Confirm the approved VictusX icon visually on the exact clean-machine candidate.
- Define and verify signing and checksum output for the final package.
- Run the clean-machine validation plan against a final candidate package.
- Dispose recurring `NU1900` package vulnerability audit-source warnings with documented evidence.
- Confirm the final package contains no logs, captured device data, symbols, machine paths, or developer-only flags.
- Confirm the release entry point always launches with only `--hp-victus`.
- Include or link [HP Diagnostic Preview User Safety Notes](hp-diagnostic-preview-user-safety-notes.md) with the preview package.

## Package-License/Notice Review Blocker

The direct and transitive package notices, project GPL, G-Helper attribution, exact .NET 10.0.11 runtime evidence, NvAPIWrapper treatment, and icon provenance are reviewed and matched to the current candidate. Preserve this evidence and repeat the inspector against the exact distributable.

The [Package License And Third-Party Notices Completion Plan](package-license-third-party-notices-completion-plan.md), [Runtime Dependency License Review Evidence Checklist](runtime-dependency-license-review-evidence-checklist.md), and reviewed [Third-Party Notices](../THIRD-PARTY-NOTICES.md) define and record the completed current-baseline notice review.

The future ZIP/installer must include applicable license text, upstream G-Helper modified-project attribution, and reviewed third-party notices.

## Icon/App Identity Blocker

The owner-approved `VictusX.Source.png` and generated `VictusX.ico` now supply executable and HP tray identity. Source provenance, hashes, frames, and wiring are complete in the [VictusX Icon Wiring Checkpoint](victusx-icon-wiring-checkpoint.md); Explorer, shortcut, taskbar, tray, and window appearance still require final clean-machine confirmation.

The former identity implementation plan is retained as a superseded historical record.

Default ASUS/G-Helper behavior and shared resources must not be broken while preparing HP diagnostic preview identity.

## Signing/Checksum Blocker

A preview package needs at least a verified SHA-256 checksum. Authenticode signing should be added when a verified publisher identity is available. Follow [Signing and Checksum Workflow](signing-checksum-workflow.md); the checksum/signing process must be repeatable and must apply to the final package actually tested.

Use [Signing And Checksum Evidence Plan](signing-checksum-evidence-plan.md) to record artifact name, version, source commit SHA, signing status, certificate/thumbprint when signed, SHA-256 hash, reviewer/date, and confirmation that the evidence matches the same artifact used for clean-machine validation.

## NU1900 Audit-Source Warning Blocker

Follow [NU1900 Audit-Source Warning Disposition Plan](nu1900-audit-source-warning-disposition-plan.md). Build and test passing while `NU1900` appears is not enough to claim package vulnerability audit completion. The release remains blocked until clean restore/build/test and vulnerability-list evidence is recorded, or a maintainer documents an alternate vulnerability-review disposition for the release candidate.

## Clean-Machine Validation Blocker

Use [Clean-Machine Validation Plan](clean-machine-validation-plan.md). This blocker remains open until a final package is tested on a clean Windows machine or VM using the launcher only, with no prior app data required and no developer-only flags.

Validation must confirm startup, report path behavior, export behavior, Diagnostic/Quit-only shell, clean shutdown, no remaining process, and no crash records.

Use [Clean-Machine Validation Evidence Plan](clean-machine-validation-evidence-plan.md) to record OS/build, machine type, artifact name/version, commit SHA, launch command, dashboard/tray observations, process termination, crash/event-log review, reviewer, and date for the exact artifact under release consideration.

## Safety Blockers That Must Remain Closed

- Fan control must remain unimplemented.
- Fan writes must remain unimplemented.
- SetFanMax must remain NO-GO/design-only.
- Performance control must remain unimplemented.
- Power-limit writes, EC writes, BIOS writes, and hardware-control paths must remain absent.
- `--hp-wmi-readonly-test` must not appear in release launchers, shortcuts, publish profiles, or user-facing release instructions.

These are intentional safety conditions, not release tasks to unblock.

## Required Final Pre-Release Evidence

- Final package contents list.
- Confirmed launcher arguments: exactly `--hp-victus`.
- Confirmed absence of `--hp-wmi-readonly-test` and future write/control flags.
- Build and test results from the release candidate source.
- Dependency and notice review result following the package license review workflow.
- `NU1900` disposition evidence from restore/build/test and vulnerability-list checks.
- Icon/app identity verification result against the asset acceptance checklist.
- SHA-256 checksum and signing status following the signing/checksum workflow and evidence plan.
- Clean-machine validation record following the clean-machine evidence plan.
- Report showing explicit read-only invocations remain `Attempted=false` during normal HP diagnostic mode.
- Manual confirmation that no fan/performance/write UI is present.
- Final user-facing safety notes reviewed for the exact release candidate.

## Recommended Next Safe Task

Preserve the approved icon hashes and verify the exact candidate visually during clean-machine validation. Do not publish artifacts until the remaining release gates are complete.
