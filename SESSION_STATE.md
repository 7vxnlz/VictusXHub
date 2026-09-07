# Session State

Hard rule: keep this file short. It is a handoff note, not a diary.

## Current State

- HP read-only refresh-rate status: HP mode now displays the current Windows internal-panel refresh rate when `ScreenNative.FindLaptopScreen(rememberInternalDisplay: false)` and `GetRefreshRate()` return a valid value; missing/ambiguous/invalid values show Unavailable. No display switching, new controls, dependencies, fan code, WMI methods, config cache write, or hardware writes were added.
- Refresh-rate verification: build passed with 4 existing NU1900 warnings; 354/354 tests passed.
- CPU-only source follow-up: existing thermal-zone/ASUS/Qualcomm and ambiguous HP 0x23 readings do not prove F.31 CPU-package identity. CPU stays Unavailable; added cached/freshness rejection tests and documented candidates in `docs/hp-temperature-fan-rpm-telemetry.md`. No runtime code, dependency, driver or hardware access added.
- CPU follow-up verification: build passed with 4 existing NU1900 warnings; 347/347 tests passed. No runtime launch or hardware probe performed.

- Temperature/RPM milestone: [source/decision notes](docs/hp-temperature-fan-rpm-telemetry.md). Optional NVIDIA GPU temperature via existing NVAPI getter, bounded single-worker polling and 5-second freshness; CPU temperature and V1 fan RPM remain unavailable. No HP methods, writes, new dependencies or UI controls. Build passed (4 NU1900 warnings); 341/341 tests passed. Native runtime availability not verified; DeviceValidatedInputLength unchanged/null, fan control NO-GO.

- Added [non-executable SetFanLevel preflight harness](docs/set-fan-level-first-write-harness-scaffold.md): offline gate evaluation and JSON only; no CLI/UI/transport wiring. Satisfied declarations do not authorize writes. DeviceValidatedInputLength null; first-write and normal control NO-GO.
- Harness verification: solution build passed (4 existing NU1900 warnings); full suite 332/332 passed. No fan CLI, hardware invocation, publication or commit.

- Date: 2026-09-05
- Latest safety note: HP Diagnostic report loading hardened for missing/corrupt reports.
- Latest path note: HP Diagnostic paths and status messages standardized.
- Latest reference note: Reference fan write flow comparison added; no write implementation.
- Latest SetFanMax note: Payload-shape decision plan added; implementation remains NO-GO.
- Latest approval note: DeviceValidatedInputLength manual approval design added; SetFanMax remains NO-GO.
- Latest evidence note: HP diagnostic shell regression audit added; shell remains diagnostic-only.
- Latest launch note: HP diagnostic launch profile and run guide added.
- Latest packaging note: Windows packaging readiness audit added for HP diagnostic mode.
- Latest publish design note: HP diagnostic publish profile design added; no binaries published.
- Latest preview source note: HP diagnostic publish profile source files prepared; no artifacts published.
- Latest preview safety note: HP diagnostic preview profile/launcher safety checks added.
- Latest metadata note: HP diagnostic preview metadata cleaned; no artifacts published.
- Latest notices note: third-party notices audit added; release remains blocked.
- Latest dependency note: dependency notice inventory added; release remains blocked.
- Latest identity note: VictusX icon/app identity replacement plan added; no assets changed.
- Latest clean-machine note: clean-machine validation plan added; release remains blocked.
- Latest release blocker note: HP diagnostic preview release blocker checklist consolidated; release remains blocked.
- Latest icon requirements note: VictusX icon asset requirements added; no assets changed.
- Latest package license note: package license review workflow added; release remains blocked.
- Latest signing note: signing/checksum workflow added; no artifacts signed or checksummed.
- Latest user safety note: HP diagnostic preview user-facing safety notes added; no artifacts released.
- Latest source readiness note: HP diagnostic preview source readiness audit added; release remains blocked.
- Latest SetFanMax proof note: final proof gap checklist added; fan write implementation remains NO-GO.
- Latest SetFanMax evidence note: manual evidence capture package added; payload length remains unset and writes remain NO-GO.
- Latest HP dashboard note: read-only SetFanMax evidence readiness is shown from cached/report-backed state.
- Latest audit note: SetFanMax readiness panel audited as diagnostic-only; no write path added.
- Latest first-write gate note: SetFanMax first-write decision gate added; current state remains NO-GO.
- Latest first-write gate dashboard note: SetFanMax first-write gate status is report-backed and fail-closed in HP diagnostics.
- Latest fallback note: old/missing SetFanMax first-write gate report fields fail closed with explicit old-report wording.
- Latest test coverage note: SetFanMax gate fail-closed test coverage checkpoint added.
- Latest export note: SetFanMax manual evidence capture template added to diagnostic Markdown exports; no write path added.
- Latest review note: SetFanMax manual evidence review workflow added; fan writes remain NO-GO.
- Latest alignment note: SetFanMax evidence export template aligned with manual review workflow.
- Latest import design note: SetFanMax manual evidence import/review design added; parser output must remain fail-closed and cannot authorize writes.
- Latest parser test note: SetFanMax evidence import parser test plan added; future tests remain fixture-only and fail-closed.
- Latest payload reference note: SetFanMax payload-length reference decision added; OmenXHub/OmenSuperHub one-byte evidence and omencore/ghelper-omen four-byte evidence remain insufficient for F.31.
- Latest experiment logger note: SetFanMax manual experiment logger design added; writes remain disabled and NO-GO.
- Latest experiment scaffold note: SetFanMax local experiment log scaffold added; it serializes blocked records only and has no WMI or write path.
- Latest dry-run note: SetFanMax developer-only dry-run command logs a blocked record only, then exits before normal startup; no WMI or write path exists.
- Latest dry-run verification note: SetFanMax dry-run runtime verification wrote blocked one-byte and four-byte hypothesis logs; both kept WriteExecuted=false and DeviceValidatedInputLength unset.
- Latest baseline capture note: SetFanMax developer-only baseline capture is gated to elevation and the approved read-only probes; no write path was added.
- Latest baseline verification note: runtime baseline capture was not run from Codex because the current process was not elevated; no WMI method was invoked.
- Latest exact-device baseline note: manual elevated one-byte and four-byte hypothesis captures matched on F.31/V1, fan count 2, max fan disabled, and raw level `22-25`; no write occurred and payload length remains unset.
- Latest first-write runner note: developer-only first-write runner design added; it remains documentation-only with implementation and execution NO-GO.
- Latest first-write runner implementation note: developer-only SetFanMax runner added behind required flags, exact identity, elevation, AC, baseline, and separate false approval gates; it was not run and remains NO-GO.
- Latest first-write runner audit note: source audit hardened transport-exception restore handling; runner remains command-line-only, unexecuted, and NO-GO.
- Latest four-byte approval note: one explicit developer-only four-byte experiment approval flag added; normal fan-write readiness and UI remain NO-GO, payload length remains unvalidated.
- Latest four-byte result note: first exact-device four-byte experiment had successful calls and observed fan ramp, but FanMaxGet stayed false; treat as partial evidence, keep payload length unset and normal control NO-GO.
- Latest second confirmation note: controlled second four-byte confirmation protocol added; it requires improved manual observations and keeps one-byte, payload validation, and normal control blocked.
- Latest second confirmation approval note: the second four-byte confirmation now requires a separate explicit command-line approval in addition to the original four-byte approval; it remains unexecuted and NO-GO for normal use.
- Latest second four-byte result note: a second exact-device four-byte experiment repeated the observed fan response with no abnormal noise, overheating, or crash; FanMaxGet remained false, so four byte is preferred experimental evidence only and normal control remains NO-GO.
- Latest experimental-status note: HP diagnostic report/dashboard/copy/export distinguish two observed four-byte physical responses from unvalidated normal control; all control permission fields remain fail-closed false.
- Latest outcome-classification note: experiment logs separately classify command success, optional physical response, restore status, and inconclusive FanMaxGet readback; normal control remains NO-GO.
- Latest observation note: developer-only SetFanMax experiment logs accept bounded manual physical-response, restore, and note metadata; observations cannot bypass gates or trigger hardware.
- Latest classified-result note: observed four-byte command success, physical response, and restore map to readback-inconclusive with legacy Outcome Unknown; normal control remains NO-GO.
- Latest clean classified-result note: fresh four-byte run recorded Outcome Unknown with no blocked reasons and readback-inconclusive classification; normal control remains NO-GO.
- Latest pulse-design note: four-byte Max Fan Pulse is documented as a separately approved developer-only design; no implementation or normal control path was added.
- Latest pulse implementation note: four-byte Max Fan Pulse command added behind its own flags and existing runner gates; it was not run and normal control remains NO-GO.
- Latest pulse result note: four-byte Max Fan Pulse completed with observed response and restore; it is operational only for the gated developer path, while normal control remains NO-GO.
- Latest pulse history note: read-only local-log pulse history/status dashboard design added; no control route or hardware action was added.
- Latest pulse history implementation note: HP Diagnostic dashboard now summarizes the latest valid local pulse/experiment log; it remains local-file-only and has no pulse or fan-control action.
- Latest HP shell quit note: HP Diagnostic Quit now disposes the diagnostic tray resources and exits the UI message loop; pulse history remains local-file-only.
- Latest pulse history/Quit checkpoint note: final checkpoint added for read-only pulse history and HP Diagnostic full-process Quit behavior; normal fan control remains NO-GO.
- Latest HP fan research abstraction note: internal architecture design added around the gated developer-only four-byte pulse path; no normal fan-control API or UI route is authorized.
- Latest HP fan research contract note: pure, unwired contracts now represent only the four-byte developer pulse and fail-closed research evidence; no runtime or UI behavior changed.
- Latest HP fan research refactor note: the command-line-only four-byte pulse now consumes the narrow research contract through a behavior-preserving runner overload; no UI route or normal-control permission changed.
- Latest HP fan research runtime note: manual post-refactor four-byte pulse verification preserved observed response and restore with inconclusive FanMaxGet; normal control remains NO-GO and DeviceValidatedInputLength remains unset.
- Latest normal fan-control note: go/no-go evidence matrix added; developer-only four-byte pulse remains operational, but user-facing fan control remains NO-GO.
- Latest normal fan-control proof note: blocker priority plan added; next safe task is read-only proof design for DeviceValidatedInputLength and readback reliability.
- Latest input-length/readback note: read-only proof design added; DeviceValidatedInputLength stays null, FanMaxGet inconclusive, FanGetLevel raw-only.
- Latest proof-gap analyzer note: HP Diagnostic now summarizes local log/report proof gaps read-only; normal fan control remains NO-GO.
- Latest proof-gap checkpoint note: HP fan proof gap analyzer checkpoint added; close-to-tray versus explicit Quit behavior documented.
- Latest preview readiness note: HP Diagnostic preview readiness checkpoint added after proof-gap analyzer stabilization; packaging prep may continue source-only while normal fan control remains NO-GO.
- Latest icon implementation note: VictusX icon/app identity implementation plan added; no assets or executable metadata changed.
- Latest notices completion note: package license and third-party notices completion plan added; no license terms or artifacts changed.
- Latest third-party notices draft note: source-level THIRD-PARTY-NOTICES.md added with discovered dependency entries marked pending; final runtime package review and release remain blocked.
- Latest runtime license checklist note: runtime dependency license review evidence checklist added; notices remain draft and release remains blocked.
- Latest NU1900 note: audit-source warning disposition plan added; build/test pass does not close package vulnerability audit readiness.
- Latest signing evidence note: signing/checksum evidence plan added; no artifacts, signatures, or final checksums were created.
- Latest clean-machine evidence note: clean-machine validation evidence plan added; no artifact exists and preview release remains blocked.
- Latest final blocker tracker note: HP Diagnostic preview final blocker tracker added; source-only prep may continue, package publish remains NO-GO.
- Latest source-only final checkpoint note: HP Diagnostic source-only readiness final checkpoint added; package publish and normal fan control remain NO-GO.
- Latest HP diagnostic layout note: dashboard content now scrolls within working-area-bounded HP mode; all diagnostic sections and safety boundaries remain read-only.
- Latest HP diagnostic shell note: HP mode now keeps the designer-owned inherited shell directly; safety disabling and one Diagnostic footer action are the only HP-specific main-window overrides, while Diagnostic remains a scrollable owned side panel like Updates.
- Latest HP caption note: disabled no-image shell tiles now keep the inherited centered caption alignment; image-before-text buttons keep the separate reserved icon lane.
- Latest HP inherited-shell checkpoint note: final manual-visual checkpoint records the restored compact inherited shell, readable footer labels/icons, aligned disabled captions, and read-only Diagnostic side panel.
- Latest preview tracker note: HP inherited-shell UI visual blocker is marked resolved; package publish and normal fan control remain NO-GO.
- Latest usefulness audit: `docs/hp-victus-ui-usefulness-gap-audit.md` identifies the G-Helper-style usefulness gap despite accepted shell visuals; next safe implementation is live read-only telemetry/status. Normal fan control remains NO-GO; no product code changed.
- Latest live telemetry note: HP mode now shows OS-only CPU load and battery/presence/AC/charging status plus snapshot-backed device detection; CPU/GPU temperature and fan RPM fail closed as unavailable, and Diagnostic shows source/freshness text. No control route or BIOS-method invocation was added.
- Latest Omen fan comparison note: OmenXHub and OmenSuperHub match the SetFanMax WMI identity but use one-byte payloads and unsafe broader control flows. OmenSuperHub is the narrower behavior reference; no payload decision, product code, or fan-control permission changed.
- Latest developer hold note: a CLI-only four-byte Max Fan Hold command now reuses the exact-device SetFanMax runner for a bounded `10`-to-`180`-second foreground wait, one enable attempt, and `finally` restore. It was not run; normal fan control remains NO-GO and `DeviceValidatedInputLength` remains unset.
- Latest developer hold result note: the first manually run ten-second hold had successful enable/restore, inconclusive FanMaxGet, and an approximately two-minute observed response with no reported adverse behavior. The requested value is a bounded pre-restore wait, not validated physical fan-duration control.
- Latest hold semantics checkpoint note: `docs/set-fan-max-hold-semantics-checkpoint.md` records the clarified hold state, including explicit CLI gates, requested pre-restore wait semantics, compatibility fields, manual observation fields, and unchanged NO-GO normal fan control.
- Branch: `main` tracking `origin/main`
- Current objective: keep SetFanLevel research serialization-only and retain separately gated four-byte SetFanMax pulse/hold; normal fan control and input-length validation remain NO-GO.
- Last completed work: added preferred `--fan-percent-candidate=0..100` SetFanLevel dry-run mapping, documented in `docs/set-fan-level-percentage-mapping-dry-run.md`. Default V1 arithmetic is integer p*55/100 below100, with100 ->100; JSON contains paired mapped bytes plus two zeros, source/confidence and endpoint hazards. Schema2 percentage records coexist with deprecated schema1 raw128/80-80 compatibility. No high bit, hardware/UI route, first-write value or F.31 validation; DeviceValidatedInputLength stays null and normal control NO-GO.
- Latest percentage dry-run checkpoint: `docs/set-fan-level-percentage-dry-run-checkpoint.md` concisely records mapping examples, four-byte serialization, legacy raw128 status, and endpoint/recovery research gaps. No product code or safety decision changed.
- Latest icon wiring checkpoint: `docs/victusx-icon-wiring-checkpoint.md` defines `app/Assets/VictusX.ico` as the future reviewed shared executable/tray source and records remaining inherited icon paths. The default launch profile now says VictusX; no asset or runtime icon behavior changed.
- Latest visible-branding audit: `docs/victusx-visible-branding-audit.md` records safe VictusX title, tray, and base/Turkish display-text cleanup while preserving namespaces, resource keys, action/config identifiers, updater references, and unreviewed localizations.
- Latest NU1900 checkpoint: `docs/nu1900-warning-checkpoint.md` confirms warnings are non-fatal for local build/test but remain a preview release blocker until clean audit evidence or an explicit maintainer alternate disposition is recorded; no audit setting changed.
- Files touched recently: dry-run command/model, focused tests, percentage-mapping design, scaffold, deep trace, preflight, blocker summary, and this state file. Prior uncommitted evidence work preserved.
- Latest endpoint/recovery audit: `docs/set-fan-level-endpoint-recovery-audit.md` records an offline-only default-V1 envelope of equal2..99% -> raw1..54, excluding zero and100 endpoints. Multi-command handoff, raw replay, ignored failures and close8BD4 stopped-fan reports leave execution NO-GO. No exact first-write value or product code change; DeviceValidatedInputLength remains null and normal fan control NO-GO. Next research: existing F.31 minimum-stable-level and durable auto-recovery evidence.
- Known build/test status: 2026-09-06 build passed with 0 errors and 4 recurring NU1900 warnings; tests passed 294/294, none skipped. Static research and documentation only; no fan CLI, experiment, WMI invocation, publish or release artifact.
- Open questions: one-byte versus four-byte ABI equivalence, reliable FanMaxGet/FanGetLevel interpretation, durable restore behavior, and the relationship between pre-restore wait and physical fan-response duration remain unproven. Reference implementations do not close these gaps.
- Next likely task: prepare the deep trace's documentation-only request for existing exact-target ABI/units/high-bit and recovery evidence, including the rationale for caller-selected128 versus4 input bytes and the8BD4 zero-handoff hazard. No new write/probe request, numeric candidate, or connection from dry-run to transport is approved.
- Reference commits reviewed: OmenXHub `ca84cb011d1d3e5850445d19e45fbea06e83a8fd`; OmenSuperHub `a6ab6988c446ee5421466097fdf60c0d521e5c81`; omencore `b39b44978902606aa708cc0d78bcfd87e95fd88b`; ghelper-omen `1694844d2725e79a2b2065a0a1494fa1d143e3f4`; G-Helper `5c26f5ac970dab9e26347d80976ebf1eece91b1e`; ECC `5eddf1a3ffd311423be2d4ba7d26f7209c91b033`
- Notes: Stable state remains documentation/read-only-safe. Elevated manual `SystemDesignData` read-only HP WMI invocation previously succeeded and returned 128 bytes. SystemDesignData synthetic sample validation notes added. SystemDesignData decoder unit tests added. SystemDesignData report decode wiring added. SystemDesignData decode verification guide added. Real HP Victus SystemDesignData decode succeeded and reports software fan control support. HP fan read-only command investigation added. FanGetCount read-only probe preparation added. FanGetCount explicit read-only report path prepared. HP mode shutdown after FanGetCount wiring now marshals exit through WinForms UI. Elevated FanGetCount read-only HP WMI invocation succeeded and reports 2 fans. Next HP fan read-only probe candidate selected. FanMaxGet read-only probe preparation added. FanMaxGet explicit read-only report path prepared. Elevated FanMaxGet read-only HP WMI invocation succeeded and reports max fan disabled. Next HP fan read-only probe after FanMaxGet selected. FanGetLevel read-only probe preparation added. HP mode now opens the settings window and ignores background exit requests after UI teardown. HP mode startup/shutdown stabilized. FanGetLevel explicit read-only report path prepared. Elevated FanGetLevel read-only HP WMI invocation succeeded and returned raw fan level bytes. HP read-only telemetry status consolidated. FanGetRpm read-only investigation added. Next safe HP path selected after FanGetRpm deferral. HP fan read-only command map created. GetFanType read-only investigation added. HP fan write safety design added; no fan writes implemented. HP fan write preflight checklist added; no write implementation. First HP fan write experiment design added; no write implementation. SetFanMax future write safety scaffolding added; no write implementation. SetFanMax payload/preflight design added; no write implementation. SetFanMax write preflight readiness audit added; no write implementation. SetFanMax method/input validation documented; no write implementation. SetFanMax device-specific validation plan and stricter input-length safety model added; no write implementation. SetFanMax device validation simulator and decision matrix added; no write implementation. SetFanMax no-write dry-run report path added; writes remain impossible. SetFanMax manual validation package added; no write implementation. SetFanMax implementation go/no-go gate added; current state remains NoGo. SetFanMax recovery/restore proof plan added; no write implementation. README and project status checkpoint refreshed. HP read-only telemetry UI plan added; no control UI implemented. HP read-only telemetry UI implementation spec added; no UI implemented. HP mode visible title rebranded to VictusX. HP read-only diagnostic moved to Diagnostic tab in HP mode. HP Diagnostic tab enriched with cached/report-only data and non-hardware actions; no control UI implemented. HP Diagnostic tab polished and verified as read-only. HP Diagnostic export added using cached read-only data only. HP Diagnostic tab dashboard layout improved; still read-only. HP Diagnostic dashboard formatting extracted into testable read-only helpers. HP Diagnostic health summary added using cached read-only data only. omencore Victus 16-s0xxx SetFanMax evidence deep dive added; payload length remains unset. HP Diagnostic dashboard completion checkpoint added. VictusX HP mode branding/packaging audit added. HP mode shell isolated to read-only Diagnostic UI and local-only actions. HP shell isolation crash fixed; diagnostic-only HP mode preserved. Do not invoke WMI, run `--hp-wmi-readonly-test`, implement hardware control, or add write-capable hardware paths without explicit user direction.

## Update Template

- Date:
- Branch:
- Current objective:
- Last completed work:
- Files touched recently:
- Known build/test status:
- Open questions:
- Next likely files:
- Reference commits reviewed:
- Notes: Second ASUS isolation pass completed. HP reference command inventory created. Read-only HP capability probe added.















