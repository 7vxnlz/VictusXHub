# Package License Review Workflow

## Review Scope

This workflow covers packages that may be distributed with a future VictusX HP diagnostic preview ZIP or installer. It is not legal advice and does not modify license terms.

Review the final package contents, not only project files. Test-only packages need review only if test tooling or developer bundles are distributed.

## Direct Package Review Steps

1. Start from `app/VictusX.csproj` and the current resolved restore graph.
2. For each direct application dependency, record package id, version, package source, project usage, and whether it is included in the final package.
3. Check authoritative package metadata: NuGet package page, package `.nuspec`, repository link, license expression/file, notice file, and project license.
4. Record required attribution, notice text, source link, and any redistribution conditions visible from those sources.
5. Mark the package blocked if metadata is missing, contradictory, or not reviewed.

## Transitive Package Review Steps

1. Generate or inspect a clean restore graph for the release candidate.
2. Compare the resolved graph with [Dependency Notice Inventory](dependency-notice-inventory.md).
3. For each runtime transitive dependency included in the package, record the same evidence required for direct packages.
4. Exclude build/test-only transitives from the application notice only when the final package contents prove they are not distributed.
5. Re-run the review when package versions or publish mode change.

## Metadata Sources to Check

- `app/VictusX.csproj`
- `tests/VictusX.Tests/VictusX.Tests.csproj` if developer/test artifacts are distributed
- clean `project.assets.json` restore output
- NuGet package metadata and license fields
- package `.nupkg` contents, including `.nuspec`, `LICENSE`, `NOTICE`, and readme files
- upstream repository license and notice files
- final ZIP/installer file list

## Required Evidence to Record

- package id and exact version
- direct or transitive status
- runtime, test-only, or build-only distribution status
- authoritative license metadata source
- notice/attribution text required for distribution, if visible
- package file path or package hash from restore metadata
- reviewer and review date
- final package-content confirmation
- unresolved questions or blocked status

## Unknown and Blocked Handling

Unknown license, notice, or redistribution evidence keeps the preview release blocked. Do not assume permissive terms from package popularity, repository name, or prior use in upstream projects.

If direct and transitive evidence disagree, keep the package blocked until a maintainer resolves the discrepancy from authoritative sources.

## Future ZIP/Installer Notice Requirements

A future preview package should include:

- the repository license text;
- clear modified-from-G-Helper attribution;
- a reviewed third-party notices file covering runtime packages in the final package;
- icon attribution if the future icon requires it;
- source location and source revision for the packaged build.

The [Package License And Third-Party Notices Completion Plan](package-license-third-party-notices-completion-plan.md) and source-level [Third-Party Notices Draft](../THIRD-PARTY-NOTICES.md) list the release-facing files, package contents, and verification checklist that must be satisfied after this workflow is completed.

Use [Runtime Dependency License Review Evidence Checklist](runtime-dependency-license-review-evidence-checklist.md) as the per-runtime-package evidence record for this workflow.

## Fail-Closed Release Criteria

Do not publish a preview if any runtime dependency has unreviewed license or notice status, if package contents do not match the reviewed inventory, if upstream attribution is missing, or if the review record is not tied to the release candidate source and artifact.

## Recommended Next Safe Task

Review the existing source-level [Third-Party Notices Draft](../THIRD-PARTY-NOTICES.md) against [Runtime Dependency License Review Evidence Checklist](runtime-dependency-license-review-evidence-checklist.md), authoritative metadata, and final package contents; retain its pending status until the record is complete.
