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
| Icon attribution | Blocked | The final icon asset's ownership, license, provenance, and required attribution must be reviewed and reflected in release notices. | [Third-Party Notices](../THIRD-PARTY-NOTICES.md), [Icon asset requirements](victusx-icon-asset-requirements.md) |
| `THIRD-PARTY-NOTICES.md` release readiness | Blocked | Promote from source-assembled status only after runtime-pack notice matching, external NvAPIWrapper layout verification, icon attribution, and final artifact matching are complete. | [Third-Party Notices](../THIRD-PARTY-NOTICES.md), [Runtime dependency review](runtime-dependency-license-review.md) |
| Signing/checksum evidence | Blocked | Record artifact name, version, commit, signing status, signer/certificate if applicable, SHA-256 hash, reviewer/date, and validation linkage. | [Signing/checksum evidence plan](signing-checksum-evidence-plan.md) |
| Clean-machine validation | Blocked | Validate the exact package candidate on a clean Windows machine/VM, including launch, read-only UI/no-control behavior, Quit/process termination, paths, crash handling, and reviewer/date evidence. | [Clean-machine evidence plan](clean-machine-validation-evidence-plan.md) |
| Final package contents inspection | Workflow ready; final candidate pending | Run `tools/verify-victusx-preview-package.ps1` against the exact candidate to verify deterministic layout, identity metadata, external NvAPIWrapper, notice-file matches, forbidden artifacts, and per-file/manifest hashes. Manual runtime-notice, signing, attribution, and clean-machine evidence remains separate. | [Package contents inspection](preview-package-contents-inspection.md), [Packaging readiness audit](windows-packaging-readiness-audit.md) |

## Completed Source-Side Items

| Item | Status | Evidence |
| --- | --- | --- |
| HP inherited-shell UI visual state | Done | Manual confirmation recorded: compact inherited shell, readable footer labels/icons, aligned disabled captions, and read-only Diagnostic side panel. See [HP inherited shell UI checkpoint](hp-inherited-shell-ui-final-checkpoint.md). |
| Icon assets and source wiring | Done pending package verification | Four valid multi-frame ICO assets are wired for fixed application identity and HP Silent/Balanced/Turbo tray routing; unknown HP mode uses the base icon and non-HP behavior is unchanged. See [Icon wiring checkpoint](victusx-icon-wiring-checkpoint.md). |
| Runtime dependency license identity review | Done | Current restore graph review covers seven direct runtime packages and one transitive package. Artifact presence remains part of final inspection. See [Runtime dependency review](runtime-dependency-license-review.md). |
| Package-library license/notice files | Done | MIT, Apache-2.0/NOTICE, LGPL-3.0/GPL-3.0, and package notice texts are assembled and publish-wired under `app/Assets/Licenses`. |
| NvAPIWrapper LGPL packaging architecture | Done pending artifact verification | The HP profile preserves the existing package/API behavior and excludes `NvAPIWrapper.dll` from the single-file bundle as a replaceable sidecar. Final inspection must confirm the DLL is external, version-matched, replaceable, and accompanied by its notices. See [Runtime dependency review](runtime-dependency-license-review.md). |
| MMI runtime release disposition | Done | Duplicate MMI/CIM readiness probe and package reference were removed; restored assets contain no MMI package. Final artifact inspection must confirm absence. |
| `NU1900` disposition | Done for current source | Network-capable restore retrieved NuGet vulnerability data; project graphs reported no vulnerable packages; restore/build/test completed without `NU1900`. Repeat on the exact release candidate. See [NU1900 checkpoint](nu1900-warning-checkpoint.md). |
| .NET runtime servicing baseline | Done for current source | The prior 10.0.10 `win-x64` runtime-pack baseline was superseded after servicing advisories. SDK 10.0.400 restored Microsoft.NETCore.App.Runtime.win-x64 and Microsoft.WindowsDesktop.App.Runtime.win-x64 at 10.0.11; the ignored local candidate passed automated inspection. |
| Self-contained .NET runtime notice matching | Done for verified 10.0.11 baseline | Verbatim exact-pack Core/Windows Desktop license files, Core third-party notices, and packaged identity provenance are byte-matched by the inspector. The Windows Desktop pack supplies no separate notice file, so no substitute was inferred. Repeat the inspection against the final distributable. See [.NET runtime notice checklist](dotnet-runtime-notice-checklist.md). |
| HP publish profile source | Done | Profile is `Release`, `net10.0-windows`, `win-x64`, self-contained, single-file, untrimmed, native self-extracting, symbol-free, and externally packages license/notices plus the safe HP launcher. |
| Package inspection workflow | Done for source tooling | The fail-closed inspector accepts one explicit directory, rejects unexpected/forbidden files, verifies source-matching notices and launcher, confirms the external NvAPIWrapper layout, and emits deterministic SHA-256 evidence. A local ignored candidate passed the automated checks; this is not final release evidence. |
| Localized inherited branding review | Done | Localized `Strings*.resx` display values use VictusX. Compatibility resource keys and internal identifiers remain unchanged. See [Visible branding audit](victusx-visible-branding-audit.md). |

## Notes

- Remaining unavailable HP telemetry is not a preview release blocker while fail-closed wording remains clear. Safe read-only telemetry work may continue separately.
- Normal fan control remains blocked and is not required for preview. Do not add sliders, toggles, curves, pulse/run buttons, background writes, EC/PawnIO fallback, or normal fan-control UI.

## Current Release Answer

Source-only work may continue. Preview package publishing remains blocked until the unresolved release blockers above are complete.
