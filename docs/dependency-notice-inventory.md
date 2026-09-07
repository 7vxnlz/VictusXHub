# Dependency Notice Inventory

## Scope and Evidence

This source-level inventory uses `app/VictusX.csproj`, `tests/VictusX.Tests/VictusX.Tests.csproj`, and the locally restored `project.assets.json` files under each project's ignored `obj` folder. It is not legal advice. License findings are recorded separately in [Runtime Dependency License Review](runtime-dependency-license-review.md).

No `packages.lock.json` exists. NuGet cache metadata and authoritative upstream license/notice sources were reviewed on 2026-09-06; final artifact contents remain unavailable.

## Application Dependencies

These seven direct package references and one resolved transitive are candidates for the future application preview payload. Source-level license identities are reviewed; release packaging actions remain open.

| Package | Version | Resolution status | Local license/notice evidence |
| --- | --- | --- | --- |
| FftSharp | 2.2.0 | Direct | MIT; reviewed |
| HidSharpCore | 1.3.0 | Direct | Apache-2.0 plus upstream NOTICE; reviewed |
| NAudio.Wasapi | 2.3.0 | Direct | MIT; reviewed |
| NvAPIWrapper.Net | 0.8.1.101 | Direct | LGPL-3.0; reviewed, packaging compliance pending |
| System.Management | 10.0.10 | Direct | MIT; reviewed |
| TaskScheduler | 2.12.2 | Direct | MIT; reviewed |
| WinForms.DataVisualization | 1.10.2 | Direct | MIT; reviewed |
| NAudio.Core | 2.3.0 | Transitive | MIT; reviewed |

The former MMI direct dependency and its Runtime.Win/Runtime.Unix transitives were removed after tracing their only use to a duplicate read-only CIM readiness probe. The restored application graph contains no MMI package; final artifact inspection must confirm no stale files are distributed.

## Test-Only Dependencies

The test project's three direct packages resolve ten additional test tooling packages locally. They are not expected to be part of an application preview payload, but need separate review if test tooling or a developer bundle is ever distributed.

| Package | Version | Resolution status |
| --- | --- | --- |
| Microsoft.NET.Test.Sdk | 18.0.1 | Direct |
| xunit | 2.9.3 | Direct |
| xunit.runner.visualstudio | 3.1.5 | Direct |
| Microsoft.CodeCoverage | 18.0.1 | Transitive |
| Microsoft.TestPlatform.ObjectModel | 18.0.1 | Transitive |
| Microsoft.TestPlatform.TestHost | 18.0.1 | Transitive |
| Newtonsoft.Json | 13.0.3 | Transitive |
| xunit.abstractions | 2.0.3 | Transitive |
| xunit.analyzers | 1.18.0 | Transitive |
| xunit.assert | 2.9.3 | Transitive |
| xunit.core | 2.9.3 | Transitive |
| xunit.extensibility.core | 2.9.3 | Transitive |
| xunit.extensibility.execution | 2.9.3 | Transitive |

## Future Preview Package Gate

Follow [Package License Review Workflow](package-license-review-workflow.md) before using this inventory for any preview release decision.

Follow [Package License And Third-Party Notices Completion Plan](package-license-third-party-notices-completion-plan.md) when converting this inventory and the source-level [Third-Party Notices Draft](../THIRD-PARTY-NOTICES.md) into a reviewed package notice file for a future HP Diagnostic preview.

Use [Runtime Dependency License Review Evidence Checklist](runtime-dependency-license-review-evidence-checklist.md) to capture the authoritative per-dependency evidence that this inventory intentionally does not infer.

Before a ZIP or installer is created, a maintainer should review authoritative package metadata for every application dependency above, record the applicable license and notice text in a versioned inventory, and confirm the final package contents against that inventory. The package should also include the applicable project license text and the G-Helper modified-project attribution described in [Third-Party Notices Audit](third-party-notices-audit.md).

The test-only list should be reviewed separately if any test or developer tooling is distributed. A future clean restore should regenerate and compare the resolved application graph before the release decision.

## Unknowns and Current Status

The current graph's license identities and visible notice obligations are established in [Runtime Dependency License Review](runtime-dependency-license-review.md). Release remains blocked by required notice-file assembly, final package inspection, the inherited icon, signing/checksums, and clean-machine packaged validation.

## Recommended Next Safe Task

Assemble the required dependency license/notice files and perform final package-content confirmation, including confirmation that no stale MMI asset is present. Do not publish until those release checks are complete.
