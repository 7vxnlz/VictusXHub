# Package License And Third-Party Notices Completion Plan

## Current License/Notice State

- Top-level `LICENSE` is present and contains the GNU General Public License version 3 text.
- `README.md` credits G-Helper as the original application base and lists HP/Omen reference repositories.
- A top-level `THIRD-PARTY-NOTICES.md` source-level draft is present; it records discovered package identities but deliberately leaves license conclusions pending.
- [Third-Party Notices Audit](third-party-notices-audit.md), [Dependency Notice Inventory](dependency-notice-inventory.md), and [Package License Review Workflow](package-license-review-workflow.md) define the current evidence and review process.
- [Runtime Dependency License Review Evidence Checklist](runtime-dependency-license-review-evidence-checklist.md) defines the exact per-runtime-package evidence required before the draft notices can become reviewed release evidence.
- The package license/notice blocker remains open until authoritative package metadata and final package contents are reviewed together.

This plan is not legal advice and does not modify license terms.

## Source Attribution Requirements

Before any HP Diagnostic preview package is distributed, release materials should include:

- the repository license text;
- a clear statement that VictusX is modified from G-Helper;
- the upstream G-Helper project URL and reviewed source commit when known;
- VictusX source location and source revision for the packaged build;
- preservation of any upstream notices that apply to reused source or assets;
- attribution for any newly added icon/app identity asset if required by its license.

## Third-Party Dependency Notice Requirements

For every runtime dependency included in the final preview package, record:

- package id and exact version;
- direct or transitive status;
- whether it is included in the distributed application payload;
- authoritative package metadata source;
- license expression or license file evidence;
- notice or attribution text, if required by visible metadata;
- package source URL and repository URL where available;
- reviewer, review date, and unresolved questions;
- confirmation that the final package contents match the reviewed dependency list.

Test-only dependencies need review only if test tooling or developer artifacts are distributed.

## Inherited G-Helper Attribution Considerations

VictusX still retains inherited G-Helper structure, including `GHelper` root namespace, startup object, resources, and portions of the imported application shell. The preview package should not present VictusX as an unrelated clean-room application.

The package notice should make the modified-project relationship clear while also stating that VictusX is independent and not endorsed by the original G-Helper project.

## HP/OMEN Trademark Wording Constraints

Use HP, OMEN, and Victus names only to identify compatibility target and research context. Do not imply:

- HP endorsement, certification, or authorization;
- OMEN Gaming Hub compatibility claims beyond tested diagnostic behavior;
- ownership of HP, OMEN, or Victus marks;
- that the preview provides HP-supported fan or performance control.

User-facing package wording should keep `VictusX` as the app name and `HP Diagnostic preview` as descriptive read-only context.

## Files To Review Before Release

- `LICENSE`
- `README.md`
- future `THIRD-PARTY-NOTICES.md` or equivalent package notice file
- `app/VictusX.csproj`
- `tests/VictusX.Tests/VictusX.Tests.csproj` if test artifacts are distributed
- clean restore `project.assets.json` for the release candidate
- final ZIP/installer file list
- icon asset provenance and attribution record
- package launcher/shortcut documentation
- user-facing safety notes

## Package Contents That Must Include Notices

A future preview ZIP or installer should include, or clearly link to:

- project license text;
- VictusX modified-from-G-Helper attribution;
- source repository and release source revision;
- reviewed third-party notices for runtime packages in the artifact;
- icon attribution or license note if applicable;
- user-facing HP Diagnostic preview safety notes;
- checksum/signing verification instructions once available.

## Acceptable Preview Package State

An acceptable preview package may ship only after:

- runtime dependency licenses/notices are reviewed from authoritative metadata;
- final package contents match the reviewed inventory;
- upstream/source attribution is included;
- inherited icon/app identity is replaced or otherwise resolved;
- release entry point uses only `--hp-victus`;
- no developer-only flags, logs, symbols, machine paths, or captured device data are included;
- clean-machine validation and checksum/signing evidence are complete.

## Remaining Blocking Items

- `THIRD-PARTY-NOTICES.md` is draft-only and has not been reviewed against authoritative runtime package metadata or final artifact contents.
- Direct and transitive runtime dependency licenses remain unreviewed from authoritative metadata.
- The runtime dependency evidence checklist is present, but its required evidence has not been filled and reviewed.
- Final package contents do not exist yet, so package notice matching cannot be completed.
- Icon/app identity attribution cannot be finalized until the future icon asset is approved.
- Signing/checksum and clean-machine validation remain open.
- NU1900 audit-source warnings need release-candidate disposition.

## Verification Checklist

- Confirm `LICENSE` is present in the package.
- Confirm modified-from-G-Helper attribution is present and readable.
- Confirm every runtime package in the final artifact has a reviewed notice/license record.
- Confirm test-only packages are excluded from application notices unless distributed.
- Confirm icon license/provenance and attribution status are recorded.
- Confirm final package contents match the notice inventory.
- Confirm no license terms were edited.
- Confirm no package contains developer-only flags or captured local logs.
- Confirm `--hp-wmi-readonly-test` is absent from release shortcuts, launchers, and user instructions.
- Confirm normal fan control remains absent and **NO-GO**.

## Recommended Next Safe Task

Review the source-level `THIRD-PARTY-NOTICES.md` draft using [Runtime Dependency License Review Evidence Checklist](runtime-dependency-license-review-evidence-checklist.md), authoritative runtime package metadata, and a final package file list. Do not publish artifacts or fill legal conclusions from local project metadata alone.
