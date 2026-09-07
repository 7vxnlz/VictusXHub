# Third-Party Notices Audit

## Scope

This is a repository-evidence checkpoint for a future HP diagnostic preview. It is not legal advice and does not determine the license obligations of any dependency.

## G-Helper Attribution

- `README.md` identifies VictusX as a G-Helper-based project and credits [G-Helper](https://github.com/seerge/g-helper) by seerge as the original application base.
- `SESSION_STATE.md` records the reviewed G-Helper source commit as `5c26f5ac970dab9e26347d80976ebf1eece91b1e`.
- The intentionally retained `GHelper` root namespace, startup object, resource names, and imported application structure are further evidence that a preview needs prominent upstream attribution and a clear modified-project statement.
- The repository top-level `LICENSE` is the unmodified GNU General Public License, version 3 text. No separate VictusX copyright notice, modification notice, or packaged-source notice file was found.

## Current Files and Gaps

| Evidence | Current status | Preview-package follow-up |
| --- | --- | --- |
| `LICENSE` | Present; GPLv3 text | Include the applicable license text and verify its presentation in the package. |
| README credits | Present for G-Helper and research references | Add a concise package attribution that names G-Helper, the source location/commit, and that VictusX is modified. |
| `THIRD-PARTY-NOTICES.md` | Draft records reviewed license identities; release packaging remains pending | Assemble required license/notices and compare against final package contents before distribution. |
| Package lock | Not found | Keep a clean-restore comparison step before packaging. |
| Resolved dependency inventory | Locally restored assets were inspected | See [Dependency Notice Inventory](dependency-notice-inventory.md); repeat from a clean restore before packaging. |
| Package license evidence | Recorded for the current graph | See [Runtime Dependency License Review](runtime-dependency-license-review.md); required release files and artifact matching remain open. |
| Runtime dependency review evidence | Source-level review complete | MMI runtime blocker resolved by removing the duplicate probe/package graph; final artifact must confirm absence. |

## NuGet Considerations

`app/VictusX.csproj` directly references FftSharp, HidSharpCore, NAudio.Wasapi, NvAPIWrapper.Net, System.Management, TaskScheduler, and WinForms.DataVisualization. [Dependency Notice Inventory](dependency-notice-inventory.md) records their locally resolved versions and discovered transitives. [Runtime Dependency License Review](runtime-dependency-license-review.md) records the authoritative source-level license evidence and visible notice requirements.

## Future Preview Distribution Gate

Before a ZIP or installer is created, the release review should include:

- the applicable license text and a clear VictusX modified-from-G-Helper attribution;
- a reviewed `THIRD-PARTY-NOTICES.md` (or equivalent package document) covering direct and resolved transitive dependencies;
- a completed package license review record following [Package License Review Workflow](package-license-review-workflow.md);
- a source location and source revision matching the preview artifact;
- confirmation that no upstream notices were removed and that the package contents match the reviewed notice inventory;
- review of the final artifact by a maintainer qualified to make the distribution decision.

Use [Package License And Third-Party Notices Completion Plan](package-license-third-party-notices-completion-plan.md) and the source-level [Third-Party Notices Draft](../THIRD-PARTY-NOTICES.md) as the checklist and starting record for turning the current audit and inventory into a reviewed package notice set.

Use [Runtime Dependency License Review Evidence Checklist](runtime-dependency-license-review-evidence-checklist.md) for the exact evidence required before the runtime dependency section can be treated as reviewed.

## Still Blocked or Unknown

The current dependency graph and license identities are reviewed. Required license/notice-file assembly, package-runtime contents, and final source-distribution presentation remain unresolved. The inherited icon, signing/checksum plan, and clean-machine packaged smoke test are also incomplete. Release remains blocked; this audit does not authorize a publish.

## Recommended Next Safe Task

Assemble the required license/notice files and compare them against a final package file list, explicitly confirming that no stale MMI runtime asset is included. Keep the notices marked draft until that work is complete.
