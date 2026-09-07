# HP Victus UI And Usefulness Gap Audit

## Scope And Evidence

CPU follow-up: [source review](hp-temperature-fan-rpm-telemetry.md) confirms existing Windows thermal-zone counters, inherited ASUS/Qualcomm paths and conflicting HP 0x23 selector labels do not establish F.31 CPU-package temperature. CPU remains Unavailable; no provider, dependency, hardware access or layout change was added. Optional GPU temperature remains separate.

Latest [temperature/RPM milestone](hp-temperature-fan-rpm-telemetry.md): optional read-only NVIDIA GPU temperature now uses the existing NVAPI dependency with bounded single-worker polling and independent freshness. CPU temperature and V1 fan RPM remain unavailable for lack of a verified safe source; 0x38 and FanGetLevel-to-RPM conversion are not adopted. HP mode also shows the current Windows refresh rate for the internal panel when `ScreenNative` can identify it without persisting display cache changes and `EnumDisplaySettingsEx` returns a valid current frequency. Layout and fan-control NO-GO are unchanged. This supersedes the GPU-unavailable implementation note below when a valid NVIDIA sensor is present.

Audit date: 2026-09-04. This is a source-level audit and implementation roadmap only. No product code, control, telemetry polling, hardware behavior, or safety permission is changed. No app, probe, or experiment was run for this audit.

Implementation update (2026-09-04): the first read-only telemetry phase is now implemented without changing the audit's control decisions. The compact HP shell samples CPU load through Windows `GetSystemTimes` and battery percentage/presence/AC/charging state through Windows `GetSystemPowerStatus`. Device detection comes from the existing startup capability snapshot, with cached diagnostic identity as a labeled fallback. CPU temperature, GPU temperature, and fan RPM remain `Unavailable` because no verified safe source exists; FanGetLevel is not used or interpreted as RPM. Polling runs on the UI thread only while the shell is visible, resets delta state when hidden, and marks samples stale after five seconds. Diagnostic shows the source and freshness summary as read-only text.

The [inherited-shell checkpoint](hp-inherited-shell-ui-final-checkpoint.md) records the user's manual confirmation that HP mode looks good. Its compact geometry, aligned disabled captions, readable footer, and owned Diagnostic side panel are accepted. The remaining gap is usefulness: the familiar shell suggests live information and supported actions, but most inherited sections are deliberately unavailable in HP mode.

The comparison baseline is the inherited G-Helper behavior already in this repository and the user's feedback. No sibling reference repository was read or copied; this is not a claim about the latest upstream feature set or HP support in another project. Existing attribution and licensing requirements remain intact. Target evidence is scoped to Victus 16-s0035nt / model 16-s0xxx / SKU 7Z5Z2EA#AB8 / BIOS F.31 / ThermalPolicyVersion 1, not all HP laptops.

### Selected Source Evidence

Search expanded from the shell to the existing telemetry, battery, display, and HP controller paths because those determine whether an inherited feature is real, a placeholder, or unsafe to reuse. Selected excerpts were sufficient; no directory-wide source dump was needed.

| Source | Finding used in this audit |
| --- | --- |
| [Settings.cs](../app/Settings.cs): constructor, `ConfigureHpReadOnlyShell`, `ConfigureHpReadOnlySection`, `RefreshSensors`, `InitAura` | Inherited sections are retained; interactive controls are disabled; HP sensor refresh returns immediately; Aura population is skipped for unsupported hardware. Thank You and Updates are explicitly disabled. |
| [Settings.cs](../app/Settings.cs): Diagnostic construction, reload, summary, export, and Quit handlers | Diagnostic is an owned, bounded, scrollable panel. It loads report/history/proof data locally and offers copy, cached reload, folder, and export actions. Close hides; explicit HP Quit uses the dedicated shutdown path. |
| [Settings.Designer.cs](../app/Settings.Designer.cs) | Refresh-rate labels, keyboard items, and battery slider values exist as inherited defaults. Their presence is not HP capability evidence. |
| [Program.cs](../app/Program.cs): HP startup and unsupported-hardware branches | HP mode selects unsupported-hardware safety behavior and bypasses inherited GPU setup, Aura, screen initialization, automatic modes, and vendor lifecycle hooks. Developer experiment commands are separate CLI paths. |
| [IHardwareController.cs](../app/Hardware/Abstractions/IHardwareController.cs), [HpVictusHardwareController.cs](../app/Hardware/HpVictusHardwareController.cs) | The shared interface is ASUS-shaped and includes writes. The HP implementation is a disconnected stub: unsupported returns, empty responses, or no-ops; fan reads return -1 and battery discharge returns null. This is not an HP telemetry implementation. |
| [HpVictusCapabilityProbe.cs](../app/Hardware/Hp/HpVictusCapabilityProbe.cs) | Startup identity/readiness discovery is distinct from explicitly gated BIOS-method probes. Existing captured fan evidence must not be treated as continuous telemetry. |
| [HardwareControl.cs](../app/HardwareControl.cs) | Inherited sensor loops return early for unsupported hardware. Individual candidates include OS CPU-time sampling and native battery status, but temperature/power/GPU paths also contain vendor assumptions and driver-related fallbacks. |
| [ScreenControl.cs](../app/Display/ScreenControl.cs), [BatteryControl.cs](../app/Battery/BatteryControl.cs) | Display and battery writes are blocked for unsupported hardware. The inherited display orchestration mixes OS refresh changes with ASUS overdrive/miniled operations; battery-limit writes use ASUS identifiers. |
| [RButton.cs](../app/UI/RButton.cs), [HP preview tests](../tests/VictusX.Tests/Hardware/Hp/HpDiagnosticPreviewConfigurationTests.cs) | Disabled painting and shell/side-panel boundaries have focused regression checks. Many checks inspect source structure; they do not prove live telemetry accuracy or hardware support. |

Decision evidence also comes from the [proof analyzer checkpoint](hp-fan-proof-gap-analyzer-checkpoint.md), [fan evidence matrix](normal-fan-control-go-no-go-evidence-matrix.md), [proof priority plan](normal-fan-control-proof-priority-plan.md), [input-length/readback proof design](device-validated-input-length-readback-proof-design.md), and [fan write blockers](fan-write-blocker-summary.md). Historical experiment results and visual acceptance are recorded evidence, not new verification in this task.

## Decision Table

Safety labels describe the proposed next action, not permission to enable an entire inherited section. `Safe read-only now` means an existing local/OS source or presentation boundary is suitable for scoped future work; it does not mean the feature is already wired. Unknown readings stay unknown.

| Area | Current behavior | Desired useful behavior | Safety level | Current decision | Required evidence | Recommended next action |
| --- | --- | --- | --- | --- | --- | --- |
| Main shell and clarity | Accepted compact shell with live CPU load, battery/AC, device status, and explicit unavailable sensor values | Concise real status and explicit unavailability | Safe read-only now | Implemented read-only; preserve layout | Runtime visual confirmation where desktop automation can target the window | Keep controls disabled and details in Diagnostic |
| CPU load | Live Windows `GetSystemTimes` delta with warm-up, stale, reset, and error handling | Live load with freshness | Safe read-only now | Implemented read-only | Runtime comparison remains useful; source/units are explicit | Preserve null rather than synthetic zero |
| CPU temperature/power | No verified live HP source; shell shows temperature unavailable | Identified CPU temperature; watts only when supported | Read-only possible with implementation | Availability conditional | Sensor identity, units, freshness, source initialization review | Keep unavailable until a source is proven |
| Fan count/history | Cached probe/log evidence; target count 2 | Clearly dated fan status | Safe read-only now | Historical evidence only | Matching device and capture provenance | Keep raw evidence in Diagnostic |
| Fan RPM | No validated HP RPM source; shell explicitly shows unavailable | Genuine per-fan RPM or unavailable | Needs proof | No numeric RPM inferred | Read-only tachometer source, units, fan mapping | Keep unavailable until source proven |
| FanMaxGet / FanGetLevel | Inconclusive latch / raw bytes | Honest diagnostic interpretation | Safe read-only now | No state/control inference | Future independent correlation | Preserve inconclusive/raw labels |
| GPU temperature/status | GPU provider setup bypassed in HP; shell shows temperature unavailable | Per-adapter temperature and observed activity | Read-only possible with implementation | Query-only candidate | Adapter identity, initialization side effects, sleeping-GPU behavior | Review a narrow getter-only source |
| Performance mode | Disabled inherited mode tiles | Known current HP policy, if detectable | Needs proof | Writes blocked | Exact-device policy identity and read-only source | Separate detection from control design |
| GPU mode | Disabled Eco/Standard/Ultimate/Optimized | Real topology/status, not assumed mode | Unknown | No equivalent HP mode proven | MUX/hybrid support and state semantics | Read-only capability research |
| Display refresh detection | Current mode and matching rates from Windows | Current and supported OS display modes | Safe read-only now | Implemented | Unique internal-panel identification and matching geometry | Keep fail-closed filtering |
| Display refresh switching | HP-only selector for Windows-reported matching rates | Explicit OS-only change | Safe non-HP control | Implemented | Windows test/apply result and refreshed current-mode confirmation | Keep isolated from vendor controls |
| Keyboard lighting | Disabled unpopulated/unselected inherited dropdown | Known backlight capability and status | Unknown | RGB and writes unproven | Exact keyboard identity/protocol/status evidence | Show unknown; research detection only |
| Battery status | Live Windows `GetSystemPowerStatus` percentage/presence/AC/charging display | Charge, charging/discharging, AC, age | Safe read-only now | Implemented read-only | Broader runtime cases and battery-age source remain separate | Preserve unknown/no-battery semantics; no limit callbacks |
| Battery charge limit | Disabled inherited slider/default 100% | Genuine conservation capability/status | Needs proof | HP charge-limit writes blocked | Exact HP firmware support, units, range, restore | Label unavailable; research read-only capability |
| Footer | Accepted rendering; Thank You/Updates disabled; Diagnostic/Quit work | Clear supported actions | Safe read-only now | Keep accepted footer | Localized unavailable reasons; future URL/update provenance | Clarify status without enabling updater |
| Diagnostic | Cached reports, history, proof gaps, copy/export | Detailed provenance and troubleshooting | Safe read-only now | Preserve read-only side panel | Missing/corrupt/stale evidence tests | Keep research details off main shell |
| Normal fan control | No active HP fan controls | Only future independently validated control | Blocked | NO-GO | Full device/ABI/readback/restore/safety decision | No sliders, toggles, curves, or pulse UI |

## 1. Main Shell Usability

The shell now satisfies visual familiarity. Performance, GPU, display, keyboard, battery, and footer stay in their inherited positions. Reopening the layout refactor would not solve the missing functionality.

The useful G-Helper-style experience would answer, at a glance, what the machine is doing and which actions genuinely work. HP mode currently cannot reliably answer CPU/GPU temperature, fan RPM, current vendor policy, or charge-limit state from the main shell. `SettingsForm_VisibleChanged` disables the inherited sensor timer in HP mode and `RefreshSensors` returns before reading sensors. Empty labels and inherited defaults are therefore expected, not evidence of zero activity or an applied setting.

The first improvement is read-only status with source and age, plus concise unavailable states. Keep detailed reports in Diagnostic. Safe presentation work does not authorize an inherited callback, a performance/power write, or a fan control.

## 2. CPU Telemetry

- **Temperature:** HP has no live temperature provider wired into the shell. The inherited `GetCPUTemp` first uses ASUS ACPI outside unsupported mode, then a fixed `Thermal Zone Information` counter; another branch uses a model-specific WMI thermal-zone query. None establishes exact-target CPU package temperature. Do not relabel a chassis/ACPI thermal zone as CPU temperature or present a missing value as 0 C.
- **Load:** `GetCPUUsage` samples `GetSystemTimes` deltas. This is the strongest existing CPU read-only candidate. Its first sample and long-pause behavior return null; future polling must respect/reset its sampling baseline rather than showing a synthetic zero or repeatedly missing the interval.
- **Power:** Existing paths include performance counters, AMD GPU/APU helpers, and a PawnIO Intel MSR fallback. They are not an approved HP watts source. Do not load a driver/MSR path or reuse an entire overlay loop to obtain a number. A future read-only power value needs an identified CPU package source and units; battery discharge and GPU power are not substitutes.

Live temperature display is safe only when the selected source's read path and initialization are reviewed and its sensor identity is trustworthy. Otherwise show unavailable, or a correctly named thermal-zone reading in Diagnostic. No new HP BIOS probe, driver installation, EC/MSR access, or tuning operation is part of the first telemetry task.

## 3. Fan Telemetry

Recorded target `FanGetCount=2` is useful historical capability evidence, not a live fan-running count. Diagnostic already distinguishes decoded count, raw level values, and experimental status. The HP controller's `GetFan` returns -1; it is not an RPM implementation. The inherited overlay multiplies ASUS fan readings by 100, a scaling rule that must not migrate to HP raw bytes.

`FanGetLevel` remains raw-only: it is not RPM, percent, a speed target, or a control state. `FanMaxGet` stayed false during observed physical fan response, so it cannot confirm a max-fan latch or prove that fans stopped. A recorded false value must retain the inconclusive warning.

Safe display today is a dated count/history summary and unknown live RPM. A numeric RPM display requires an independently reviewed read-only tachometer source with units and fan-to-sensor mapping. No such HP source was established by this audit; do not invoke even a nominally read-only HP command to fill the gap. A failed/missing sample must not imply 0 RPM or a stopped fan.

RPM observation is measurement. Fan control requests a state change and needs validated command, bounds, restore, thermal, and recovery semantics. Even accurate RPM would not authorize controls.

## 4. GPU Telemetry

The inherited shell shows temperature/fan information through `HardwareControl`; HP skips GPU provider creation and sensor refresh. `GetGPUTemp` references a GPU getter but also contains ASUS ACPI and AMD APU fallback assumptions. `RecreateGpuControl` constructs NVIDIA/AMD control implementations, so it cannot be enabled wholesale merely because a temperature getter exists.

A future query-only path may report OS adapter identity, current display attachment, and temperature/activity from an already available, reviewed driver interface. Review provider construction/disposal as well as getters. Avoid waking a sleeping dGPU solely to poll it; a unavailable/sleeping status is acceptable, and a read failure alone cannot prove sleep or power-off.

Distinguish iGPU/dGPU inventory, observed activity, and physical MUX routing. Enumerating two adapters does not prove the current HP GPU mode. No GPU switching, power limits, clock/voltage changes, process killing, device disable/enable, ASUS ACPI, or performance writes belong in telemetry.

## 5. Performance Mode

Silent/Balanced/Turbo and inherited Fans/Power affordances are disabled. Their labels and any inherited highlight are not evidence of the current HP thermal/performance policy. `IHardwareController` contains ASUS-oriented performance methods; the HP stub rejects or ignores them.

A useful HP implementation would first identify and explain the actual current policy, source, power-state constraints, and unsupported cases. Current HP policy detection is unproven. An OS power-mode reading could eventually be shown under its own OS label; it must not be mapped to HP Silent/Balanced/Turbo or ThermalPolicyVersion without proof.

Performance/power writes remain blocked. This includes reusing `SetPerformanceMode`, power-limit tuning, `SetFanMode`, `SetFanLevel`, `0x37`, or EC calls as shortcuts. Detection does not establish a setter, valid values, or restore behavior.

## 6. GPU Mode

Eco, Standard, Ultimate, and Optimized are inherited options. Their handlers address ASUS GPU mode identifiers or automatic GPU-mode logic; HP controls stay disabled. The source reviewed here establishes no safe HP equivalent, MUX support, or persisted mode semantics for the target SKU.

Read-only adapter/status detection may proceed through reviewed sources. Vendor GPU mode switching remains blocked pending exact-device capability, reboot/transition requirements, reliable readback, recovery, and HP/OMEN service-conflict evidence. Never make an inherited tile functional just because its caption sounds applicable.

## 7. Display Refresh Rate

The inherited designer includes `60Hz` and `120Hz + OD` presets and an automatic mode. HP skips `ScreenControl.InitScreen`, so these are not detected target display capabilities. Overdrive, HDR/miniled, and other inherited options likewise must not imply HP support.

OS current-mode and supported-rate enumeration use the existing DisplayConfig/`EnumDisplaySettingsEx` boundary with strict unique hardware-internal identification. HP mode shows the current rate and enables one compact selector only when Windows reports modes matching the current resolution, bit depth, and orientation; duplicates are removed. Missing or ambiguous identity and invalid modes fail closed to `Unavailable`.

The HP selector bypasses inherited `SetScreen`, ASUS overdrive, MiniLED, automatic AC/battery switching, and vendor hardware paths. It changes only `DM_DISPLAYFREQUENCY`, first uses Windows `CDS_TEST`, applies only on explicit selection, and requires a fresh current-mode confirmation. A rejection or unconfirmed result retains the prior state and shows a concise failure.

## 8. Keyboard Lighting

`InitAura` returns for unsupported hardware before it binds the keyboard mode list. The designer contains inherited Static/Breathe/Rainbow/Strobe items, but these are disabled defaults; an unselected/empty-looking dropdown is not proof of missing hardware or RGB support. Color and Extra controls do not establish a HP capability.

Backlight presence, zones, brightness steps, RGB support, current state, and the exact keyboard protocol remain unknown. Existing OS/device inventory or already cached capability evidence could inform a read-only detection design, but generic HID presence is not a backlight state and a color swatch is not a measured color.

Use an unavailable/unknown status first. Any future lighting write requires exact-device protocol, payload/range, ownership, persistence, and restore evidence. Do not reuse Aura/TUF methods, guess HID output reports, or issue a write to discover capability.

## 9. Battery Status And Charge Limit

The visible charge-limit section is inherited and disabled. The designer slider defaults to 100 and has a 100% action; the HP battery controller returns no discharge value and inherited limit setters return early in unsupported mode. This is neither a real HP conservation setting nor a implemented simulator. A displayed default/configured limit is not proof of firmware state or current battery charge.

Basic battery percentage, presence, AC online, and charging/discharging are useful OS read-only candidates. `HardwareControl` already declares `GetSystemPowerStatus` and a native system-battery-state query. Review failure sentinels: the current percentage helper falls back to zero on failure, so its return value alone cannot support an honest unavailable state.

Do not reuse all of `ReadBatteryState`/`FormatBatteryCharge`: formatting can call a battery-limit helper, and health discovery uses a WMI query. Those routes are currently guarded, but a future telemetry design should have no dependency on limit-setting behavior. Capacity/health/rate need separate source/unit review; observed charge percentage is not charge-limit readback.

HP battery conservation/BIOS support for this exact SKU/BIOS requires research. Distinguish firmware-managed health policies from a user-set percentage threshold. No charge-limit write may be enabled from an inherited slider, ASUS identifier, UI screenshot, or another HP model's behavior. Required future evidence includes genuine capability/current-state detection, accepted range, power transitions, persistence, and recovery.

## 10. Footer Actions

- **Thank You / Donations:** visually accepted but explicitly disabled in HP; the inherited donation controller is not initialized there. Preserve upstream attribution. Any future working credits/donation link needs a clearly identified destination and ownership review, not activation of the inherited route by assumption.
- **Updates:** visually accepted but disabled, and its handler returns in HP. Diagnostic reuses the owned-panel interaction pattern; that does not make Updates functional. A future read-only version/source-link design needs a VictusX channel and provenance; inherited BIOS/driver/updater behavior must not be exposed as HP support.
- **Diagnostic:** useful now. It toggles an owned secondary panel; Main/closing the panel returns to the shell. Preserve compact layout, scrolling, dark styling, and the local-only action boundary.
- **Quit:** uses the dedicated HP shutdown path. Standard close intentionally hides to tray. Preserve this distinction and retain process-termination checks for future lifecycle changes.

No further geometry polish is required by the accepted visual checkpoint. Remaining work is truthful availability wording, localization/accessibility verification, and future route-specific evidence. This audit does not re-run desktop verification.

## 11. Diagnostic Side Panel

Diagnostic already provides substantial research/troubleshooting value: report metadata, device identity, WMI/CIM readiness, decoded/raw fan evidence, SetFanMax safety decisions, local pulse history, and proof-gap analysis. Copy summary, reload cached report, open diagnostic folder, and export use existing local data. Keep all these features.

The data sources include the in-memory capability snapshot, cached `%APPDATA%\VictusX\hp-capability-report.json`, and local `%APPDATA%\VictusX\Logs\FanExperiments\` evidence. Missing/corrupt evidence fails closed. Reload/schema refresh is a local report operation, not a fresh sensor capture. Capture provenance and last observation time must stay separate from report formatting time.

Only concise, identified current readings belong on the future main shell. Raw bytes, method availability, protocol evidence, blocked reasons, and research history belong in Diagnostic. Do not turn a pulse history record into a run action or reuse the CLI runner from the panel.

Normal startup already performs identity/readiness discovery through the capability probe, including read-only management queries; that is distinct from footer/panel actions and from `hpqBIOSInt*` invocations. This audit executes neither. Future telemetry must not call the capability probe or add a HP BIOS probe as a polling shortcut.

## 12. Unavailable Controls Clarity

The safety strategy works, but dim tiles, an empty-looking dropdown, and static values can look broken or suggest that elevation will unlock support. Preserve accepted geometry and readable disabled painting. Do not make controls visually enabled to imply availability.

As part of future status work, use short section-level wording such as `Unavailable in HP mode`, `Current mode unknown`, `Charge limit not supported by this app`, or `RPM unavailable`. Use `Not detected` only after a detection attempt; distinguish that from `Not implemented`, `Cached`, `Stale`, and `Unknown`.

Attach reasons to the containing section or accessible description rather than depending only on hover over a disabled WinForms control. Any badge is status text, not a command. Avoid selected-looking defaults implying applied policy. Preserve localized footer captions and screen-reader names. Keep detailed safety explanations in Diagnostic so the main shell stays compact.

## 13. Live Read-Only Telemetry Roadmap

Use a narrow, source-reviewed snapshot/presentation path, not the write-capable `IHardwareController` contract and not a re-enabled `HardwareControl.ReadSensors`/overlay loop. Existing code may supply small reviewed OS queries; being named `Get` or `Read` is insufficient if initialization, formatting, or fallback can tune hardware.

Each field should carry value or null, units, source/sensor identity, capture time, and availability reason. Keep live readings separate from cached research evidence. Never substitute zero, a designer value, a stale experiment record, or a configured target for a measured value.

OS battery/AC status and CPU load are now wired through a narrow source that contains only the two Windows status APIs. CPU/GPU temperature and fan RPM remain unavailable until a non-writing source is identified and verified for those meanings. This implementation does not expand probing privileges to satisfy a numeric display requirement. No exact-target temperature or RPM provider has been validated.

Future polling should be bounded, non-overlapping, cancellable, and independent of automatic control. Pause or reduce work when hidden, reset stale/delta state after resume, marshal updates to the UI thread, and dispose on Quit. Choose sampling intervals compatible with the actual provider; do not silently reuse the CPU helper's long-pause reset with an incompatible poll interval. Avoid GPU wakeups, new privileged services/drivers, EC/MSR access, HP WMI write methods, and explicit HP read/write probes.

Future acceptance evidence: pure missing/error/stale/unit/source tests, no-write call-boundary tests, fixture fan values staying raw-only, preserved null `DeviceValidatedInputLength`, and source-reviewed non-admin runtime comparison when separately authorized. Validate CPU/GPU sensor identities against an independent read-only source; validate battery unknown/presence cases and sampling load. Test doubles alone cannot certify real sensor accuracy.

## 14. Safe Control Roadmap

| Category | Scope |
| --- | --- |
| Safe to implement now as read-only, when requested | Local evidence presentation and explicit availability/freshness. OS CPU load and battery/AC status are now implemented; temperature/fan status remains unavailable. |
| Safe to design, not implement yet | OS-only reversible refresh switching; query-only keyboard/conservation detection; clearly attributed credits and a read-only VictusX update-status panel. |
| Requires proof | Temperature sensor identity, GPU provider initialization/sleep effects, RPM units/mapping, current HP performance/GPU mode, lighting/charge-limit support, and every prospective non-fan write/restore path. |
| Blocked / NO-GO | Normal fan UI, sliders/toggles/curves, pulse/run buttons, generic fan-control APIs, automatic/background writes, performance/power writes, SetFanMode, SetFanLevel, 0x37, EC writes, and unsupported inherited routes. |

An OS-level action may eventually be lower risk than a firmware operation, but it still requires its own reviewed boundary and authorization. No section becomes supported by removing `IsUnsupportedHardwareMode` guards or changing a single enabled flag.

## 15. Fan-Control Boundary

Developer-only 4-byte Max Fan Pulse is operational under explicit CLI gates only. This does not mean normal fan control is ready. Its command success and manually observed response/restore establish bounded experimental evidence, not durable ABI equivalence, RPM semantics, or firmware ownership.

`DeviceValidatedInputLength` remains null/unset. FanMaxGet remains inconclusive; FanGetLevel remains raw-only. One-byte evidence remains comparison-only and must not become a fallback or default. No new experiment is authorized here.

No fan slider, fan toggle, fan curve, or pulse/run button should be added. Normal/user-facing fan control remains **NO-GO** until separate reviewed input-length, readback, durable restore, repeatability, thermal/power, sleep/resume, crash/recovery, and HP/OMEN conflict evidence allows reconsideration. Accurate live telemetry would help research, but cannot grant control permission.

## 16. Prioritized Implementation Order

| Phase | Work | Exit evidence and boundary |
| --- | --- | --- |
| 1. Clear UI states and labels | Within the next telemetry task, label unavailable/cached/unknown fields using existing shell primitives | No geometry rewrite; no default interpreted as measured/applied; disabled controls stay disabled |
| 2. Live read-only CPU/GPU/fan/battery status | Start with proven OS load/battery sources; add temperature/RPM only where source review succeeds | Freshness, units, error/null behavior, responsiveness and disposal; no probing or write fallback |
| 3. Display/keyboard/battery capability detection | Current refresh rate is read-only status; identify supported display modes and research backlight/conservation capabilities | Exact-device source evidence; unknown stays unknown; no capability-discovery writes |
| 4. Proven non-fan controls only | Separately design/review one reversible operation at a time, with OS display switching a candidate | Explicit authorization, correct device/mode, bounded inputs, rollback/recovery and non-admin tests; no blanket enablement or performance/power writes |
| 5. Fan research continuation | Review existing proof gaps and design missing restore/readback/safety evidence | Separate approval for any future CLI execution; no experiment in this roadmap task and no research action in UI |
| 6. Normal fan control only if future evidence allows | Reassess all normal-control gates before even considering product controls | Documented decision plus exact-device safety/restore evidence; currently NO-GO, with no promised delivery date |

Phases are a dependency order, not approval to execute them. Package preparation remains a separate track with icon, notices, audit, signing/checksum, clean-machine, and artifact-content gates. A useful telemetry implementation would not close those release blockers.

## 17. Biggest Risks

- Inherited ASUS assumptions leaking through shared interfaces, constants, model flags, GPU constructors, or automatic lifecycle hooks.
- Familiar controls that appear selected or meaningful but do nothing; inherited 100%/refresh/keyboard defaults mistaken for detected state.
- Accidentally invoking writes by removing unsupported-mode guards or reusing a sensor formatter that calls control helpers.
- User confusion from disabled sections, unclear empty fields, and advice to elevate despite absent HP support.
- Presenting thermal-zone readings as CPU package temperature, raw fan bytes as RPM, stale cache as live telemetry, or unavailable sensors as zero.
- Treating repeated physical pulse response as full payload, restore, or normal fan-control validation.
- Sensor polling causing resource leaks, repeated failures, unnecessary dGPU wakeups, or tray/Quit regressions.
- Publishing before identity, attribution/license review, NU1900 disposition, signing/checksum, clean-machine validation, and final package inspection are complete.

## 18. Concrete Next Safe Implementation Task

Design a read-only source-validation task for CPU/GPU temperature and fan RPM without invoking HP BIOS methods, EC access, drivers, or experiments. The design must identify sensor identity, units, initialization side effects, sleep behavior, freshness, and independent comparison evidence before any additional live value is implemented.

Until that proof exists, keep the current truthful `Unavailable` values. Do not install drivers, invoke BIOS methods, enable inherited controls, or interpret raw fan bytes to fill missing readings. OS CPU load and battery/AC status are already implemented through their narrow read-only provider.

## Verification And Limits

`dotnet build VictusX.sln` passed on 2026-09-04 with 0 errors and 4 recurring NU1900 audit-source warnings. `dotnet test VictusX.sln` passed 220/220 tests with none skipped. New pure tests cover missing/invalid/stale CPU and power samples, source independence, formatter fail-closed behavior, cached identity labeling, and the no-temperature/no-RPM boundary.

The normal `--hp-victus` process launched and remained responsive. Runtime logs confirmed that explicit HP BIOS-method probes were skipped because the required developer flag was absent. The desktop automation helper could not enumerate the untitled borderless VictusX window, so this run did not produce independent screenshot verification; the accepted shell appearance remains based on the prior recorded manual confirmation. No artifact was published or created, and nothing was committed.

## Final Decisions

- Source-only release-prep: GO
- Preview package publish: NO-GO
- Developer-only 4-byte Max Fan Pulse: operational under explicit CLI gates only
- Normal/user-facing fan control: NO-GO
- Next safe implementation: read-only CPU/GPU temperature and fan-RPM source validation design
