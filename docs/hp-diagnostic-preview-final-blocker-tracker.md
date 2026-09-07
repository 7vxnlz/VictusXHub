# HP Diagnostic Preview Final Blocker Tracker

## Purpose

This tracker consolidates the current HP Diagnostic preview release blockers and the evidence required before any package can be published. It is source-only release preparation and does not authorize publishing, signing, checksum generation, WMI probing, or fan-control UI.

## Blocker Tracker

The [HP UI/usefulness gap audit](hp-victus-ui-usefulness-gap-audit.md) separates the resolved shell visual blocker from the remaining usefulness gap. Its first status phase is implemented: the inherited shell displays read-only Windows CPU load and battery/presence/AC/charging status plus device detection, while unproven CPU/GPU temperature and fan RPM values fail closed as unavailable. Unsupported controls remain disabled, and this does not close any package evidence gate below.

| Blocker | Current status | Required evidence | Owner/action | Related docs | Release decision |
| --- | --- | --- | --- | --- | --- |
| HP inherited-shell UI visual state | Resolved | Manual confirmation that HP mode preserves the compact inherited shell, readable footer labels/icons, aligned disabled captions, and owned Diagnostic side-panel behavior | Recorded; preserve this state during future release-prep edits | [Inherited shell UI checkpoint](hp-inherited-shell-ui-final-checkpoint.md), [Source-only readiness checkpoint](hp-diagnostic-source-only-readiness-final-checkpoint.md) | GO for source-only preview prep; not a package-publish approval |
| Icon/app identity | Blocked | Original or properly licensed VictusX icon asset, provenance, attribution decision, executable/window/tray/Explorer verification, rollback evidence | Maintainer to approve asset and verify integration later | [Icon requirements](victusx-icon-asset-requirements.md), [Icon identity plan](victusx-icon-app-identity-plan.md), [Icon implementation plan](victusx-icon-app-identity-implementation-plan.md) | NO-GO until approved asset and verification exist |
| Package license/notices | Blocked | Reviewed project license packaging, G-Helper modified-project attribution, source revision, HP/OMEN wording review, final package notice contents | Maintainer to complete notice review from authoritative evidence | [Notices audit](third-party-notices-audit.md), [Notices completion plan](package-license-third-party-notices-completion-plan.md), [Package license workflow](package-license-review-workflow.md) | NO-GO until reviewed notice set is complete |
| Runtime dependency license review | Blocked | Authoritative identity, version, license source, notice requirement, bundling requirement, attribution requirement, artifact presence, reviewer/date for every runtime candidate | Maintainer to review direct and transitive runtime packages | [Dependency inventory](dependency-notice-inventory.md), [Runtime dependency checklist](runtime-dependency-license-review-evidence-checklist.md) | NO-GO until every runtime dependency is reviewed or excluded by artifact evidence |
| `THIRD-PARTY-NOTICES.md` release readiness | Draft only | Filled and reviewed dependency notices, upstream attribution, icon attribution decision, final artifact-content match | Maintainer to convert source-level draft to reviewed release evidence | [Third-Party Notices Draft](../THIRD-PARTY-NOTICES.md), [Runtime dependency checklist](runtime-dependency-license-review-evidence-checklist.md) | NO-GO while draft/pending entries remain |
| `NU1900` audit-source warning disposition | Open | Clean restore/build/test and vulnerability-list evidence without unresolved `NU1900`, or documented maintainer alternate vulnerability review tied to the release candidate | Maintainer to run and record release-prep audit checks | [NU1900 disposition plan](nu1900-audit-source-warning-disposition-plan.md) | NO-GO while vulnerability-audit confidence is incomplete |
| Signing/checksum evidence | Open | Artifact name/version, source commit SHA, signing status, certificate/thumbprint if signed, SHA-256 hash, checksum verification, reviewer/date | Maintainer to sign or approve unsigned status and generate final checksums only after artifact selection | [Signing workflow](signing-checksum-workflow.md), [Signing/checksum evidence plan](signing-checksum-evidence-plan.md) | NO-GO until evidence matches the final validated artifact |
| Clean-machine validation | Open | OS/build, machine type, artifact name/version, commit SHA, launch command, dashboard/tray observations, Quit/process result, crash/event-log review, reviewer/date | Maintainer/tester to validate final package on clean Windows machine or VM | [Clean-machine plan](clean-machine-validation-plan.md), [Clean-machine evidence plan](clean-machine-validation-evidence-plan.md) | NO-GO until final package passes clean-machine validation |
| Final package contents inspection | Blocked by missing artifact | Complete file list proving notices, license, launcher, safety notes, checksum/signature evidence, and absence of developer-only logs/flags/device captures | Maintainer to inspect the final ZIP/installer after artifact creation | [Packaging readiness audit](windows-packaging-readiness-audit.md), [Release blockers](hp-diagnostic-preview-release-blockers.md) | NO-GO until artifact contents match reviewed evidence |
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

Design read-only source validation for CPU/GPU temperature and fan RPM as scoped by the [usefulness gap audit](hp-victus-ui-usefulness-gap-audit.md). Unknown sensors must remain unavailable unless their identity and units are proven without HP BIOS-method probes, EC access, drivers, or experiments. Icon implementation still requires an original/licensed asset; license/notice and all other package evidence gates remain open. Preview package publish and normal fan control remain NO-GO.
