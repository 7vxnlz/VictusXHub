# HP Diagnostic Preview Final Blocker Tracker

## Purpose

This tracker consolidates the current HP Diagnostic preview release blockers and the evidence required before any package can be published. It is source-only release preparation and does not authorize publishing, signing, checksum generation, WMI probing, or fan-control UI.

## Blocker Tracker

The [HP UI/usefulness gap audit](hp-victus-ui-usefulness-gap-audit.md) separates the resolved shell visual blocker from the remaining usefulness gap. Its first status phase is implemented: the inherited shell displays read-only Windows CPU load and battery/presence/AC/charging status plus device detection, while unproven CPU/GPU temperature and fan RPM values fail closed as unavailable. Unsupported controls remain disabled, and this does not close any package evidence gate below.

| Blocker | Current status | Required evidence | Owner/action | Related docs | Release decision |
| --- | --- | --- | --- | --- | --- |
| HP inherited-shell UI visual state | Resolved | Manual confirmation that HP mode preserves the compact inherited shell, readable footer labels/icons, aligned disabled captions, and owned Diagnostic side-panel behavior | Recorded; preserve this state during future release-prep edits | [Inherited shell UI checkpoint](hp-inherited-shell-ui-final-checkpoint.md), [Source-only readiness checkpoint](hp-diagnostic-source-only-readiness-final-checkpoint.md) | GO for source-only preview prep; not a package-publish approval |
| Icon/app identity | Source complete; candidate visual check pending | Owner-approved source provenance, recorded hashes, multi-frame ICO, and executable/HP tray wiring are complete; exact-candidate Explorer/window/taskbar/tray appearance remains | Complete with clean-machine validation | [Icon wiring checkpoint](victusx-icon-wiring-checkpoint.md), [Icon requirements](victusx-icon-asset-requirements.md) | Source GO; final visual evidence remains under clean-machine NO-GO |
| Package license/notices | Resolved for current preview baseline | Project GPL, G-Helper modified-project attribution, eight-package notice inventory, exact .NET 10.0.11 runtime notices, NvAPIWrapper treatment, and icon provenance were matched to the current candidate | Repeat inspector against exact distributable | [Third-Party Notices](../THIRD-PARTY-NOTICES.md), [Package license workflow](package-license-review-workflow.md) | Notice review GO; publishing remains separately blocked |
| Runtime dependency license review | Resolved | Authoritative identity, version, license source, notice/attribution requirement, and packaged evidence recorded for every runtime candidate | Repeat only if dependencies change | [Dependency inventory](dependency-notice-inventory.md), [Runtime dependency checklist](runtime-dependency-license-review-evidence-checklist.md) | GO for current dependency graph |
| `THIRD-PARTY-NOTICES.md` release readiness | Reviewed for current preview baseline | Exact-distributable byte match remains automated by the inspector | Preserve with exact distributable | [Third-Party Notices](../THIRD-PARTY-NOTICES.md), [Runtime dependency checklist](runtime-dependency-license-review-evidence-checklist.md) | Notice content GO; publishing remains separately blocked |
| `NU1900` audit-source warning disposition | Resolved for current graph | Network-capable restore/audit completed without vulnerable-package findings or suppressed warnings | Repeat if dependency inputs change | [NU1900 checkpoint](nu1900-warning-checkpoint.md) | GO for current graph |
| Signing decision | Resolved for initial preview | **Unsigned — explicitly approved by maintainer**; no approved certificate is configured and no signing was performed | Preserve the unsigned warning in release material | [Signing workflow](signing-checksum-workflow.md), [Signing/checksum evidence plan](signing-checksum-evidence-plan.md) | GO for the signing decision only |
| Final distributable checksum | Open | Artifact name/version, source commit SHA, size, SHA-256 hash, checksum verification, reviewer/date | Generate only after the exact distributable is frozen | [Signing workflow](signing-checksum-workflow.md), [Signing/checksum evidence plan](signing-checksum-evidence-plan.md) | NO-GO until evidence matches the clean-machine-validated artifact |
| Clean-machine validation | Open | OS/build, machine type, artifact name/version, commit SHA, launch command, dashboard/tray observations, Quit/process result, crash/event-log review, reviewer/date | Maintainer/tester to validate final package on clean Windows machine or VM | [Clean-machine plan](clean-machine-validation-plan.md), [Clean-machine evidence plan](clean-machine-validation-evidence-plan.md) | NO-GO until final package passes clean-machine validation |
| Final package contents inspection | Workflow verified; exact distributable pending | Current candidate passes layout, notice, runtime, identity, forbidden-artifact, and checksum-manifest checks | Repeat against exact distributable selected for signing/checksum and clean-machine validation | [Package inspection](preview-package-contents-inspection.md), [Release blockers](preview-release-current-blockers.md) | Current candidate GO; final artifact linkage pending |
| Normal fan control blocked state | Intentionally blocked | Continued absence of normal fan UI, fan sliders/toggles, pulse button, automatic/background writes, performance control, and unsupported command expansion | Maintainer to preserve safety boundary during release prep | [Fan write blocker summary](fan-write-blocker-summary.md), [Payload strategy](set-fan-max-payload-strategy-decision.md) | Must remain NO-GO for preview |

## Final Current Decision

- Source-only release-prep can continue.
- Preview package publish: NO-GO.
- Normal/user-facing fan control: NO-GO.
- Developer-only 4-byte Max Fan Pulse: operational under explicit command-line gates only.
- HP inherited-shell UI visual blocker: resolved by manual confirmation.
- Diagnostic side-panel behavior: resolved and accepted.

HP mode preserves the designer-owned inherited shell while disabling unsupported performance, GPU, display, keyboard, battery, and fan interactions. Diagnostic is the only HP-specific footer addition and opens a vertically scrollable owned side panel using the inherited Updates-style secondary-window pattern without adding a control surface.

The restored inherited shell, readable footer labels/icons, aligned disabled captions, and Diagnostic side-panel behavior are recorded in [HP Inherited Shell UI Final Checkpoint](hp-inherited-shell-ui-final-checkpoint.md).

See [HP Diagnostic Source-Only Readiness Final Checkpoint](hp-diagnostic-source-only-readiness-final-checkpoint.md) for the final source-only readiness state before asset, license, and artifact-specific work.

## Recommended Next Safe Task

Design read-only source validation for CPU/GPU temperature and fan RPM as scoped by the [usefulness gap audit](hp-victus-ui-usefulness-gap-audit.md). Unknown sensors must remain unavailable unless their identity and units are proven without HP BIOS-method probes, EC access, drivers, or experiments. Icon source integration and the unsigned initial-preview decision are complete; final distributable checksum and clean-machine evidence gates remain open. Preview package publish and normal fan control remain NO-GO.
