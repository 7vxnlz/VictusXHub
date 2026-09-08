# Dependency Notice Inventory

## Scope and Evidence

This source-level inventory uses `app/VictusX.csproj`, `tests/VictusX.Tests/VictusX.Tests.csproj`, and the locally restored `project.assets.json` files under each project's ignored `obj` folder. It is not legal advice. License findings are recorded separately in [Runtime Dependency License Review](runtime-dependency-license-review.md).

No `packages.lock.json` exists. NuGet cache metadata and authoritative upstream license/notice sources were reviewed on 2026-09-06; the current ignored candidate was matched on 2026-09-08.

## Application Dependencies

These seven direct package references and one resolved transitive exactly match the current application restore graph and reviewed preview notice inventory.

| Package | Version | Resolution status | Local license/notice evidence |
| --- | --- | --- | --- |
| FftSharp | 2.2.0 | Direct | MIT; reviewed |
| HidSharpCore | 1.3.0 | Direct | Apache-2.0 plus upstream NOTICE; reviewed |
| NAudio.Wasapi | 2.3.0 | Direct | MIT; reviewed |
| NvAPIWrapper.Net | 0.8.1.101 | Direct | LGPL-3.0; reviewed, external sidecar/notices verified |
| System.Management | 10.0.10 | Direct | MIT; reviewed |
| TaskScheduler | 2.12.2 | Direct | MIT; reviewed |
| WinForms.DataVisualization | 1.10.2 | Direct | MIT; reviewed |
| NAudio.Core | 2.3.0 | Transitive | MIT; reviewed |

The former MMI direct dependency and its Runtime.Win/Runtime.Unix transitives were removed after tracing their only use to a duplicate read-only CIM readiness probe. The restored application graph and current candidate contain no MMI package or stale MMI file.

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

## Preview Package Gate

Follow [Package License Review Workflow](package-license-review-workflow.md) before using this inventory for any preview release decision.

The [Third-Party Notices](../THIRD-PARTY-NOTICES.md) file is reviewed for the current preview baseline; repeat the inspector against the exact distributable.

Use [Runtime Dependency License Review Evidence Checklist](runtime-dependency-license-review-evidence-checklist.md) to capture the authoritative per-dependency evidence that this inventory intentionally does not infer.

The reviewed package includes the applicable project license text, G-Helper modified-project attribution, and license/notice material for every application dependency above. The exact distributable must match this inventory.

The test-only list should be reviewed separately if any test or developer tooling is distributed. A future clean restore should regenerate and compare the resolved application graph before the release decision.

## Unknowns and Current Status

The current graph's license identities, visible notice obligations, external notice files, icon provenance, and current-candidate package match are complete. Release remains blocked by signing/final-checksum evidence and clean-machine validation of the exact distributable.

## Recommended Next Safe Task

Repeat the fail-closed package inspection against the exact distributable, then complete signing/final-checksum evidence and clean-machine validation. Do not publish until those release checks are complete.
