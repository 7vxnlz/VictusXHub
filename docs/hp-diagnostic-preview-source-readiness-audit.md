# HP Diagnostic Preview Source Readiness Audit

## Completed Source Prerequisites

- HP diagnostic mode is stable behind `--hp-victus`.
- The HP shell is Diagnostic/Quit-only and uses cached/report-backed data.
- Safe local-only actions exist for copy, reload, open report folder, and export.
- Publish profile and launcher source files exist for a future HP diagnostic preview.
- Repository tests guard the launcher/profile against `--hp-wmi-readonly-test`.
- VictusX preview metadata is aligned while inherited default behavior remains preserved.
- User safety notes, package license review workflow, icon asset requirements, signing/checksum workflow, and clean-machine validation plan exist.

## Remaining Release Blockers

- Package-license and notice review is incomplete.
- The inherited icon/app visual identity has not been replaced.
- No signing or checksum evidence exists.
- Clean-machine validation has not been run against a final package candidate.
- No final package contents list or release evidence record exists.

## Required Final Evidence Before Packaging

- Source commit and version selected for the candidate package.
- Final package contents list.
- Confirmed launcher arguments: exactly `--hp-victus`.
- Confirmed absence of `--hp-wmi-readonly-test` and future write/control flags.
- Build and test results for the candidate source.
- Reviewed package license and notice inventory.
- Approved icon/app identity verification.
- SHA-256 checksum and signing status for the final artifact.
- Clean-machine validation record.
- User-facing safety notes reviewed against the release candidate.

## Safety Status

VictusX HP diagnostic preview remains read-only. Fan control, fan writes, performance control, EC writes, BIOS writes, power writes, and hardware-control UI are not implemented.

SetFanMax remains NO-GO/design-only. This is an intentional safety boundary, not a release blocker to remove.

## Launcher and Profile Status

`tools/run-victusx-hp-diagnostic.ps1` and `app/Properties/PublishProfiles/VictusX-HP-Diagnostic-win-x64.pubxml` exist as source files only. The launcher is intended to pass only `--hp-victus`.

No preview artifact has been published, and `dotnet publish` has not been run as part of this audit.

## Metadata Status

Project metadata is aligned to VictusX preview identity and versioning. `RootNamespace` and inherited resource names remain intentionally unchanged to preserve the imported base and default ASUS/G-Helper behavior.

## Notices and License Status

The repository has a third-party notices audit, dependency notice inventory, and package license review workflow. The release blocker remains open until package licenses/notices are reviewed against authoritative metadata and matched to final package contents.

This audit does not modify license terms or make legal conclusions.

## Icon and App Identity Status

The inherited icon remains unchanged. Icon replacement is blocked until the asset requirements and acceptance checklist are satisfied with original or properly licensed VictusX assets.

## Signing and Checksum Status

The signing/checksum workflow exists, but no signatures or checksums have been generated. Future checksum/signing evidence must match the exact artifact that passes clean-machine validation.

## Clean-Machine Validation Status

The clean-machine validation plan exists, but it cannot be completed until a final package candidate exists. Release remains blocked until that validation passes.

## Current Release Decision

No VictusX HP diagnostic preview release is ready yet. The repository is source-prepared for a future preview package workflow, but the release gates remain closed.

## Recommended Next Safe Task

Create a source-only `THIRD-PARTY-NOTICES.md` draft from the dependency inventory and package license review workflow, leaving legal conclusions marked pending review.
