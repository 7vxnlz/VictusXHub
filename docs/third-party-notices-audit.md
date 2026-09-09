# Third-Party Notices Audit

## Scope

This is a repository-evidence checkpoint for a future HP diagnostic preview. It is not legal advice and does not determine the license obligations of any dependency.

## G-Helper Attribution

- `docs/README.md` identifies VictusX as a G-Helper-based project and credits [G-Helper](https://github.com/seerge/g-helper) by seerge as the original application base.
- `.github/ai/SESSION_STATE.md` records the reviewed G-Helper source commit as `5c26f5ac970dab9e26347d80976ebf1eece91b1e`.
- The intentionally retained `GHelper` root namespace, startup object, resource names, and imported application structure are further evidence that a preview needs prominent upstream attribution and a clear modified-project statement.
- The repository top-level `LICENSE` is the unmodified GNU General Public License, version 3 text. No separate VictusX copyright notice, modification notice, or packaged-source notice file was found.

## Current Files and Gaps

| Evidence | Current status | Preview-package follow-up |
| --- | --- | --- |
| `LICENSE` | Present; GPLv3 text | Include the applicable license text and verify its presentation in the package. |
| README credits | Present for G-Helper and research references | Add a concise package attribution that names G-Helper, the source location/commit, and that VictusX is modified. |
| `THIRD-PARTY-NOTICES.md` | Reviewed for the current preview baseline | Preserve it and repeat inspector matching against the exact distributable. |
| Package lock | Not found | Keep a clean-restore comparison step before packaging. |
| Resolved dependency inventory | Locally restored assets were inspected | See [Dependency Notice Inventory](dependency-notice-inventory.md); repeat from a clean restore before packaging. |
| Package license evidence | Recorded and candidate-matched for the current graph | See [Runtime Dependency License Review](runtime-dependency-license-review.md). |
| Runtime dependency review evidence | Complete for current baseline | Exact .NET 10.0.11 notices and absence of MMI artifacts are inspector-verified. |

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

Use [Package License And Third-Party Notices Completion Plan](package-license-third-party-notices-completion-plan.md) and the reviewed [Third-Party Notices](../THIRD-PARTY-NOTICES.md) to preserve the current notice set and repeat exact-distributable matching.

Use [Runtime Dependency License Review Evidence Checklist](runtime-dependency-license-review-evidence-checklist.md) for the exact evidence required before the runtime dependency section can be treated as reviewed.

## Still Blocked or Unknown

The current dependency graph, license identities, notice files, package-runtime mapping, NvAPIWrapper treatment, and icon provenance are reviewed and matched to the current candidate. Signing/final-checksum evidence and clean-machine validation remain incomplete, so this audit does not authorize publishing.

## Recommended Next Safe Task

Repeat the inspector against the exact distributable, then complete signing/final-checksum evidence and clean-machine validation.
