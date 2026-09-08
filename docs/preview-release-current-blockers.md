# Preview Release Current Blockers

Updated: 2026-09-08

This is the concise current blocker snapshot for the VictusX HP Diagnostic preview. It does not authorize publishing, signing, checksum generation, dependency changes, package creation, or runtime behavior changes.

## Current Decisions

- Source-only release preparation: **Done / GO**
- Preview package publish: **Blocked / NO-GO**
- Normal/user-facing fan control: **Blocked / NO-GO**
- Developer-only four-byte SetFanMax pulse/hold: operational under explicit CLI gates only; not part of preview UI release readiness.

## Unresolved Release Blockers

| Item | Status | Required evidence / decision | Primary reference |
| --- | --- | --- | --- |
| Final distributable checksum | Blocked | After the exact unsigned distributable is frozen, record its name, version, commit, size, SHA-256 hash, reviewer/date, and clean-machine validation linkage. | [Signing/checksum evidence plan](signing-checksum-evidence-plan.md) |
| Clean-machine validation | Blocked | Validate the exact package candidate on a clean Windows machine/VM, including launch, read-only UI/no-control behavior, Quit/process termination, paths, crash handling, and reviewer/date evidence. | [Clean-machine evidence plan](clean-machine-validation-evidence-plan.md) |
| Final package contents inspection | Workflow ready; exact distributable pending | Run `tools/verify-victusx-preview-package.ps1` against the exact distributable to verify deterministic layout, identity metadata, external NvAPIWrapper, notice-file matches, forbidden artifacts, and per-file/manifest hashes. Final checksum and clean-machine evidence remain separate. | [Package contents inspection](preview-package-contents-inspection.md), [Packaging readiness audit](windows-packaging-readiness-audit.md) |

## Completed Source-Side Items

| Item | Status | Evidence |
| --- | --- | --- |
| HP inherited-shell UI visual state | Done | Manual confirmation recorded: compact inherited shell, readable footer labels/icons, aligned disabled captions, and read-only Diagnostic side panel. See [HP inherited shell UI checkpoint](hp-inherited-shell-ui-final-checkpoint.md). |
| Icon identity, provenance, and source wiring | Done pending final visual verification | The owner-approved PNG source, deterministic multi-frame ICO, hashes, and factual ChatGPT image-generation provenance are recorded. One VictusX icon supplies executable and HP tray identity; obsolete performance variants are removed and non-HP behavior is unchanged. See [Icon wiring checkpoint](victusx-icon-wiring-checkpoint.md). |
| Runtime dependency license identity review | Done | Current restore graph review covers seven direct runtime packages and one transitive package. Artifact presence remains part of final inspection. See [Runtime dependency review](runtime-dependency-license-review.md). |
| Package-library license/notice files | Done | MIT, Apache-2.0/NOTICE, LGPL-3.0/GPL-3.0, and package notice texts are assembled and publish-wired under `app/Assets/Licenses`. |
| Final package notice/attribution review | Done for current preview baseline | The restored graph, project GPL, modified-from-G-Helper attribution, eight application package entries, exact .NET 10.0.11 runtime notices, NvAPIWrapper sidecar/notices, and icon provenance are complete and internally consistent. The inspector now fails closed on the fixed notice inventory; repeat against the exact distributable. |
| NvAPIWrapper LGPL packaging architecture | Done for current preview baseline | The HP profile preserves the existing package/API behavior and excludes `NvAPIWrapper.dll` from the single-file bundle as a replaceable sidecar. The current candidate confirms the DLL is external, version-matched, replaceable, and accompanied by its notices. Repeat against the exact distributable. See [Runtime dependency review](runtime-dependency-license-review.md). |
| MMI runtime release disposition | Done | Duplicate MMI/CIM readiness probe and package reference were removed; restored assets contain no MMI package. Final artifact inspection must confirm absence. |
| `NU1900` disposition | Done for current source | Network-capable restore retrieved NuGet vulnerability data; project graphs reported no vulnerable packages; restore/build/test completed without `NU1900`. Repeat on the exact release candidate. See [NU1900 checkpoint](nu1900-warning-checkpoint.md). |
| .NET runtime servicing baseline | Done for current source | The prior 10.0.10 `win-x64` runtime-pack baseline was superseded after servicing advisories. SDK 10.0.400 restored Microsoft.NETCore.App.Runtime.win-x64 and Microsoft.WindowsDesktop.App.Runtime.win-x64 at 10.0.11; the ignored local candidate passed automated inspection. |
| Self-contained .NET runtime notice matching | Done for verified 10.0.11 baseline | Verbatim exact-pack Core/Windows Desktop license files, Core third-party notices, and packaged identity provenance are byte-matched by the inspector. The Windows Desktop pack supplies no separate notice file, so no substitute was inferred. Repeat the inspection against the final distributable. See [.NET runtime notice checklist](dotnet-runtime-notice-checklist.md). |
| HP publish profile source | Done | Profile is `Release`, `net10.0-windows`, `win-x64`, self-contained, single-file, untrimmed, native self-extracting, symbol-free, and externally packages license/notices plus the safe HP launcher. |
| Package inspection workflow | Done for source tooling | The fail-closed inspector accepts one explicit directory, rejects unexpected/forbidden files, verifies source-matching notices and launcher, confirms the external NvAPIWrapper layout, and emits deterministic SHA-256 evidence. A local ignored candidate passed the automated checks; this is not final release evidence. |
| Initial preview signing decision | Done | **Unsigned — explicitly approved by maintainer.** No approved code-signing certificate is configured and no signing was performed. Release material must warn that Windows may display an unknown or unverified publisher. |
| Localized inherited branding review | Done | Localized `Strings*.resx` display values use VictusX. Compatibility resource keys and internal identifiers remain unchanged. See [Visible branding audit](victusx-visible-branding-audit.md). |

## Notes

- Remaining unavailable HP telemetry is not a preview release blocker while fail-closed wording remains clear. Safe read-only telemetry work may continue separately.
- Normal fan control remains blocked and is not required for preview. Do not add sliders, toggles, curves, pulse/run buttons, background writes, EC/PawnIO fallback, or normal fan-control UI.

## Current Release Answer

Source-only work may continue. Preview package publishing remains blocked until the unresolved release blockers above are complete.
