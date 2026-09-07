# Third-Party Notices

## Draft Status

Status: **Source-level dependency evidence and package-library license bundle assembled; final release review pending**.

This notice is prepared for a future VictusX HP Diagnostic preview. It does not authorize publishing. License identities for the current package dependency graph have been reviewed, and the applicable package-library texts are assembled under `Assets/Licenses` for external publication. NvAPIWrapper is source-configured as a replaceable external library; verification of that layout against the release candidate remains pending. The self-contained .NET runtime notice set, icon attribution, and final artifact-content match also remain pending.

## VictusX Project Notice

Status: **Present; pending final package review**.

VictusX is a modified project based on G-Helper. The repository includes the GNU General Public License version 3 text in `LICENSE`. A future package must include the applicable project license text, the source location and revision used for the package, and this notice after review against the final package contents.

## G-Helper Attribution

Status: **Present in draft; pending final package review**.

VictusX uses [G-Helper](https://github.com/seerge/g-helper) by seerge as its original application base. The project retains inherited application structure, including the `GHelper` root namespace and resource naming. VictusX is a modified project and is not affiliated with, authorized by, or endorsed by G-Helper.

The reviewed upstream source reference recorded by this repository is commit `5c26f5ac970dab9e26347d80976ebf1eece91b1e`. This attribution must remain in future package notices, but it does not replace review of applicable upstream license and notice requirements before distribution.

## Runtime Dependency Notice Review

The following dependencies are recorded by `app/VictusX.csproj` and the local resolved dependency inventory. License evidence is recorded in the [Runtime Dependency License Review](docs/runtime-dependency-license-review.md). Inclusion in a future artifact is not yet known.

| Package | Version | Relationship | Requirement | Assembled material |
| --- | --- | --- | --- | --- |
| FftSharp | 2.2.0 | Direct | MIT attribution and license text | `FftSharp-LICENSE.txt` |
| HidSharpCore | 1.3.0 | Direct | Apache-2.0 license and upstream NOTICE | `HidSharpCore-LICENSE.txt`, `HidSharpCore-NOTICE.txt` |
| NAudio.Wasapi | 2.3.0 | Direct | MIT attribution and license text | `NAudio-LICENSE.txt` |
| NvAPIWrapper.Net | 0.8.1.101 | Direct | Prominent attribution, LGPLv3 and incorporated GPLv3 texts; distributed as an independently replaceable external library | `NvAPIWrapper-README.txt`, `NvAPIWrapper-LGPL-3.0.txt`, `GPL-3.0.txt`; release-candidate layout verification pending |
| System.Management | 10.0.10 | Direct | MIT license and package third-party notices | `System.Management-LICENSE.txt`, `System.Management-THIRD-PARTY-NOTICES.txt` |
| TaskScheduler | 2.12.2 | Direct | MIT attribution and license text | `TaskScheduler-LICENSE.txt` |
| WinForms.DataVisualization | 1.10.2 | Direct | MIT attribution and license text | `WinForms.DataVisualization-LICENSE.txt` |
| NAudio.Core | 2.3.0 | Transitive | MIT attribution and license text | Covered by `NAudio-LICENSE.txt` |

No runtime package is treated as attribution-only: the applicable license terms are assembled for each one. HidSharpCore additionally requires its NOTICE. NvAPIWrapper additionally requires prominent identification and LGPL combined-work relinking/replacement compliance. The HP preview profile now excludes `NvAPIWrapper.dll` from the single-file bundle so it can be distributed and replaced independently beside `VictusX.exe`; final artifact inspection must verify that source-side intent before release.

`app/Assets/Licenses/LICENSE-SOURCES.md` records the authoritative source revision and SHA-256 for each assembled file. The project publishes this directory externally rather than hiding it inside the single-file executable.

The former Microsoft.Management.Infrastructure package family is not part of the current project or restored dependency graph after its duplicate readiness probe was removed. Final artifact inspection must confirm that no stale MMI files are distributed. Before distribution, compare this list and the assembled files with the final ZIP or installer contents. A self-contained package also requires the exact .NET runtime license/notice set selected by the release candidate. See [Dependency Notice Inventory](docs/dependency-notice-inventory.md) and [Package License Review Workflow](docs/package-license-review-workflow.md).

The [Runtime Dependency License Review Evidence Checklist](docs/runtime-dependency-license-review-evidence-checklist.md) now tracks the remaining release-candidate and artifact checks; the source-level findings are in the completed [Runtime Dependency License Review](docs/runtime-dependency-license-review.md).

## Test-Only Dependency Notice Review (Separate)

Status: **Pending only if distributed**.

The following packages are recorded only in the test project or its resolved test tooling graph. They are not assumed to be part of an HP Diagnostic preview package. Review them separately only if test tooling, a developer bundle, or another artifact containing them is distributed.

| Package | Version | Status |
| --- | --- | --- |
| Microsoft.NET.Test.Sdk | 18.0.1 | Direct test dependency; **Pending review if distributed** |
| xunit | 2.9.3 | Direct test dependency; **Pending review if distributed** |
| xunit.runner.visualstudio | 3.1.5 | Direct test dependency; **Pending review if distributed** |
| Microsoft.CodeCoverage | 18.0.1 | Resolved test transitive; **Pending review if distributed** |
| Microsoft.TestPlatform.ObjectModel | 18.0.1 | Resolved test transitive; **Pending review if distributed** |
| Microsoft.TestPlatform.TestHost | 18.0.1 | Resolved test transitive; **Pending review if distributed** |
| Newtonsoft.Json | 13.0.3 | Resolved test transitive; **Pending review if distributed** |
| xunit.abstractions | 2.0.3 | Resolved test transitive; **Pending review if distributed** |
| xunit.analyzers | 1.18.0 | Resolved test transitive; **Pending review if distributed** |
| xunit.assert | 2.9.3 | Resolved test transitive; **Pending review if distributed** |
| xunit.core | 2.9.3 | Resolved test transitive; **Pending review if distributed** |
| xunit.extensibility.core | 2.9.3 | Resolved test transitive; **Pending review if distributed** |
| xunit.extensibility.execution | 2.9.3 | Resolved test transitive; **Pending review if distributed** |

## Icon Attribution (Pending)

Status: **Final assets present; release provenance confirmation pending**.

The project-supplied final `VictusX.ico`, `VictusX.Silent.ico`, `VictusX.Balanced.ico`, and `VictusX.Turbo.ico` assets passed Windows ICO container and frame validation and are wired for fixed application identity plus HP mode-aware tray identity. No external artwork source or third-party attribution requirement was supplied. Final release review must retain the recorded asset hashes and confirm the project owner's ownership/license statement and any required attribution against the package candidate. See [VictusX Icon Wiring Checkpoint](docs/victusx-icon-wiring-checkpoint.md).

## Trademark Notice

HP, OMEN, and Victus names are used only to identify compatibility targets and research context. VictusX is not affiliated with, authorized by, endorsed by, or certified by HP Inc. No statement in this draft implies an HP or OMEN endorsement.

## Preview Release Blocker

Status: **Blocked for release use**.

The package-library license and notice files are assembled, but this document is not completed release evidence. The NvAPIWrapper source architecture now uses a replaceable external DLL, with release-candidate verification still required. A preview package remains blocked until the self-contained runtime notice set and final artifact contents are matched, icon attribution is completed, and signing/checksum and clean-machine evidence exist. Normal/user-facing fan control also remains NO-GO.
