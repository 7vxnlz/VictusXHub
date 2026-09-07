# HP Diagnostic Source-Only Readiness Final Checkpoint

## Current Decisions

- Source-only release-prep decision: GO.
- Preview package publish decision: NO-GO.
- Normal/user-facing fan control decision: NO-GO.
- Developer-only 4-byte Max Fan Pulse decision: operational under explicit command-line gates only.

This checkpoint does not authorize publishing, artifact creation, signing, checksum generation, WMI probing, fan-control UI, or normal fan writes.

## Current HP Diagnostic Feature Set

- `--hp-victus` keeps the designer-owned VictusX/G-Helper shell directly, with inherited performance, GPU, display, keyboard, and battery sections visible but disabled in HP mode.
- Thank You, Updates, Diagnostic, and Quit remain in the inherited footer layout; Diagnostic is the only HP-specific addition and opens a report-backed, local-file-only owned side panel using the same secondary-window pattern as Updates.
- [HP Inherited Shell UI Final Checkpoint](hp-inherited-shell-ui-final-checkpoint.md) records the manual visual confirmation that the compact inherited shell, footer labels/icons, disabled shell captions, and Diagnostic side panel now render correctly.
- The [HP UI/usefulness gap audit](hp-victus-ui-usefulness-gap-audit.md) now records the first live read-only status implementation: Windows CPU load and battery/presence/AC/charging status appear in the inherited shell, device detection is snapshot/cached-report backed, and Diagnostic shows source/freshness details. CPU/GPU temperature and fan RPM remain explicitly unavailable because their sensor identities are unproven.
- Safe local actions remain available only inside Diagnostic: copy summary, reload cached report, open report folder, and export diagnostic report.
- SetFanMax readiness, first-write gate, experimental status, pulse history/status, and proof gap analyzer are visible as read-only diagnostic evidence.
- The main shell preserves inherited panel ownership, order, tile geometry, and auto-size behavior; only the Diagnostic side panel scrolls when its read-only evidence content is long.
- Standard window close hides to tray by design; explicit Quit is expected to terminate the process.

## Read-Only Safety Boundaries

- No normal fan control UI.
- No fan sliders, toggles, curves, or speed controls.
- No pulse/run button in the dashboard or tray.
- No automatic or background fan writes.
- No `--hp-wmi-readonly-test` requirement for normal HP Diagnostic preview launch.
- No normal performance control.
- `DeviceValidatedInputLength` remains unset.
- FanMaxGet remains inconclusive and FanGetLevel remains raw-only.

## Remaining Release Blockers

- Icon/app identity remains blocked pending an original or properly licensed VictusX icon asset and integration verification.
- Package license/notices remain blocked pending authoritative review and package-content matching.
- Runtime dependency license review remains blocked pending per-package evidence.
- `THIRD-PARTY-NOTICES.md` remains draft/source-level only.
- `NU1900` audit-source warning disposition remains open.
- Signing/checksum evidence remains open.
- Clean-machine validation remains open.
- Final package contents inspection remains open because no artifact exists.
- Normal/user-facing fan control must remain blocked for the preview.

## Governing Documents

- Overall tracker: [HP Diagnostic Preview Final Blocker Tracker](hp-diagnostic-preview-final-blocker-tracker.md).
- Release blockers: [HP Diagnostic Preview Release Blockers](hp-diagnostic-preview-release-blockers.md).
- Packaging readiness: [Windows Packaging Readiness Audit](windows-packaging-readiness-audit.md).
- Icon/app identity: [VictusX Icon and App Identity Implementation Plan](victusx-icon-app-identity-implementation-plan.md).
- Notices: [Package License And Third-Party Notices Completion Plan](package-license-third-party-notices-completion-plan.md) and [Third-Party Notices Draft](../THIRD-PARTY-NOTICES.md).
- Runtime dependency review: [Runtime Dependency License Review Evidence Checklist](runtime-dependency-license-review-evidence-checklist.md).
- `NU1900`: [NU1900 Audit-Source Warning Disposition Plan](nu1900-audit-source-warning-disposition-plan.md).
- Signing/checksum: [Signing And Checksum Evidence Plan](signing-checksum-evidence-plan.md).
- Clean-machine validation: [Clean-Machine Validation Evidence Plan](clean-machine-validation-evidence-plan.md).
- Fan-control safety: [Fan Write Blocker Summary](fan-write-blocker-summary.md).
- HP inherited-shell UI: [HP Inherited Shell UI Final Checkpoint](hp-inherited-shell-ui-final-checkpoint.md).

## What Can Continue Safely

- Source-only release-prep documentation.
- Icon asset review and future implementation planning without replacing assets until approved.
- Runtime dependency license evidence collection.
- `THIRD-PARTY-NOTICES.md` review against authoritative metadata.
- `NU1900` disposition in a network-connected release-prep environment.
- Signing/checksum evidence template work.
- Clean-machine validation template work.
- Final package contents checklist design without creating artifacts.

## What Must Not Happen Yet

- Do not publish binaries or create release artifacts.
- Do not sign artifacts or generate final checksums.
- Do not claim preview package release readiness.
- Do not add normal fan control UI, sliders, toggles, curves, or a pulse button.
- Do not run another experiment or developer WMI probe as part of release-prep documentation.
- Do not change `DeviceValidatedInputLength`.
- Do not include developer-only flags in preview launchers, shortcuts, release notes, or user instructions.

## Recommended Next Safe Task

Design read-only source validation for CPU/GPU temperature and fan RPM as scoped by the [usefulness gap audit](hp-victus-ui-usefulness-gap-audit.md). Keep those values unavailable unless sensor identity, units, freshness, initialization effects, and independent comparison evidence are established without HP BIOS-method probes, EC access, drivers, or experiments. Release evidence work remains source-only and all package blockers stay open.
