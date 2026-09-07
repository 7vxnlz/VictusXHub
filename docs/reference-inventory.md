# Reference Inventory

This is a token-safe index of the local read-only reference repositories. It is not an architecture decision record and does not authorize copying source code into VictusControl.

Inventory method: shallow top-level listings, solution/project discovery, README excerpts, and targeted filename/content searches for hardware, fan, WMI, EC, BIOS, telemetry, settings, and configuration terms. No builds, installers, scripts, setup commands, or application code were run.

## seerge/g-helper

- Local path: `D:\Projects\Workspace\references\g-helper`
- Current commit SHA: `5c26f5ac970dab9e26347d80976ebf1eece91b1e`
- Primary language/runtime: C# on .NET, currently `net10.0-windows`
- UI framework: Windows Forms
- Solution/project files: `app/GHelper.sln`, `app/GHelper.csproj`
- Relevance: Medium

Short summary: Lightweight Asus laptop control utility and useful reference for small Windows desktop app shape, startup flow, settings, tray behavior, update flow, and hardware-control ergonomics. It is Asus-specific, so hardware logic is mostly not reusable for VictusControl.

Top-level structure summary: root contains `.github`, `app`, `docs`, `LICENSE`, and localization/site docs. Main source appears concentrated under `app`.

Key candidate files/folders:

- Startup/app lifecycle: `app/Program.cs`, `app/Helpers/Startup.cs`
- Settings/config: `app/AppConfig.cs`, `app/Settings.cs`, `app/Properties/Settings.Designer.cs`
- Hardware abstraction/control shape: `app/HardwareControl.cs`, `app/AsusACPI.cs`
- Fan UI/control references: `app/Fans.cs`, `app/Fan/FanSensorControl.cs`
- Power/thermal mode ideas: `app/Mode/ModeControl.cs`, `app/Mode/Modes.cs`, `app/Mode/PowerNative.cs`
- Telemetry/overlay: `app/Overlay/HardwareOverlay.cs`, `app/Pawn/CpuInfo.cs`
- Updates: `app/UpdatesController.cs`, `app/AutoUpdate/AutoUpdateControl.cs`
- Later README/doc reference: `docs/README.md`

Avoid:

- Asus-specific folders and files such as `app/AsusACPI.cs`, `app/USB`, `app/AnimeMatrix`, `app/Ally`, and Asus peripheral models except as structural examples
- Driver/binary payloads such as `app/Pawn/*.bin`
- Generated WinForms files such as `*.Designer.cs` and `*.resx`
- Website/media assets under `docs`

Recommended next inspection question: How does a compact WinForms utility organize startup, settings, tray commands, and update checks without pulling in Asus hardware assumptions?

## ib-3/ghelper-omen

- Local path: `D:\Projects\Workspace\references\ghelper-omen`
- Current commit SHA: `1694844d2725e79a2b2065a0a1494fa1d143e3f4`
- Primary language/runtime: C# on .NET `net8.0-windows10.0.19041.0`
- UI framework: Windows Forms
- Solution/project files: `app/GHelper.sln`, `app/GHelper.csproj`
- Relevance: High

Short summary: HP OMEN and compatible Victus adaptation of G-Helper. Most useful as a targeted reference for HP WMI BIOS direction, capability detection, fan concepts, telemetry bridge ideas, and safety guardrails. Treat it as reference material, not a code source.

Top-level structure summary: root contains `.github`, `app`, `docs`, release/patch files, `LICENSE`, and test notes. Source is mainly under `app`, with HP-specific code concentrated under `app/Omen`.

Key candidate files/folders:

- Startup/app lifecycle: `app/Program.cs`, `app/StartupChecks.cs`
- HP WMI/BIOS: `app/Omen/HpWmiBios.cs`, `app/Omen/IHpWmiBios.cs`, `app/Omen/WmiBiosMonitor.cs`
- Fan control: `app/Omen/WmiFanController.cs`, `app/Omen/FanController.cs`, `app/Omen/IFanController.cs`, `app/Omen/EcFanControllerWrapper.cs`, `app/Omen/Services/FanService.cs`
- EC/hardware access: `app/Omen/EcAccessFactory.cs`, `app/Omen/IHardwareProvider.cs`, `app/Omen/HardwareMonitorBridge.cs`, `app/Omen/LibreHardwareMonitorImpl.cs`
- Capability/device detection: `app/Omen/CapabilityDetectionService.cs`, `app/Omen/ModelCapabilityDatabase.cs`
- Thermal/telemetry models: `app/Omen/ThermalSensorProvider.cs`, `app/Omen/AcpiThermalZoneTemperatureReader.cs`, `app/Omen/Models/ThermalSample.cs`, `app/Omen/Models/FanTelemetry.cs`, `app/Omen/Models/MonitoringSample.cs`
- Settings/adaptive tuning: `app/AppConfig.cs`, `app/Omen/Adaptive/PowerModelStore.cs`
- General G-Helper inherited references: `app/HardwareControl.cs`, `app/Fans.cs`, `app/Mode/ModeControl.cs`

Avoid:

- Any wholesale G-Helper-derived source import
- Driver and binary payloads under `app/drivers`
- Asus-specific leftovers under `app/USB`, `app/AnimeMatrix`, `app/Ally`, and peripheral models
- Generated WinForms files and resources
- Patch/diff artifacts in the repository root except as historical notes

Recommended next inspection question: Which HP WMI BIOS commands and safety checks are described by the HP-specific abstractions, and which can be reimplemented cleanly for Victus 16 only?

## theantipopau/omencore

- Local path: `D:\Projects\Workspace\references\omencore`
- Current commit SHA: `b39b44978902606aa708cc0d78bcfd87e95fd88b`
- Primary language/runtime: C#/.NET 8 with some tooling scripts and website assets
- UI framework: Avalonia for current/experimental GUI projects
- Solution/project files: `OmenCore.sln`, `src/OmenCore.Avalonia/OmenCore.Avalonia.csproj`, `src/OmenCore.HardwareWorker/OmenCore.HardwareWorker.csproj`, `src/OmenCoreApp/OmenCoreApp.csproj`, test projects under `src/OmenCoreApp.Tests`, and probe tools under `tools`
- Relevance: High

Short summary: Large HP OMEN/Victus control center with richer subsystem separation, tests, diagnostics, hardware worker concepts, WMI/EC/PawnIO/Linux backend notes, and strong safety/testing references. It is useful for design questions but too large to use as default context.

Top-level structure summary: root contains `src`, `tools`, `config`, `docs`, `drivers`, `installer`, `packaging`, `qa`, `website`, and build/release scripts. It has many docs and tests, so future inspections should be very targeted.

Key candidate files/folders:

- Project shape: `OmenCore.sln`, `src/OmenCore.Avalonia`, `src/OmenCore.HardwareWorker`, `src/OmenCoreApp`, `src/OmenCoreApp.Tests`
- Hardware worker: `src/OmenCore.HardwareWorker/OmenCore.HardwareWorker.csproj`
- WMI/lighting probes: `tools/LightingProbe/Wmi.cs`, `tools/LightingProbe/Program.cs`
- Fan and hardware tests: `src/OmenCoreApp.Tests/Hardware/*Fan*`, `src/OmenCoreApp.Tests/Hardware/*Wmi*`, `src/OmenCoreApp.Tests/Services/*Fan*`, `src/OmenCoreApp.Tests/Services/TelemetryServiceTests.cs`
- UI references: `src/OmenCore.Avalonia/Views/MainWindow.axaml`, `src/OmenCore.Avalonia/Views/FanControlView.axaml`, `src/OmenCore.Avalonia/Views/SettingsView.axaml`, `src/OmenCore.Avalonia/Views/SystemControlView.axaml`
- Config docs/data: `config/default_config.json`, `docs/3.8.0-CORE-CONTROLS-NEXT-STEPS.md`, `docs/8D87-OMEN-MAX-16-SUPPORT-PLAN.md`, `docs/8D87-VERIFICATION-CHECKLIST.md`, `docs/8D87-EVIDENCE.md`
- Packaging/release references: `packaging`, `qa`, `INSTALL.md`

Avoid:

- `installer` contents, especially executable installers and driver payloads
- `drivers/PawnIO` except README-level context
- `src/OmenCoreApp/runtimes/**/native` and vendor samples
- `website` assets, screenshots, generated reports, and release scripts unless release planning specifically asks
- Broad docs scans; use exact questions and filenames

Recommended next inspection question: Which tests define safe fan, WMI, telemetry, and startup-recovery behavior that VictusControl should independently mirror?

## breadeding/OmenSuperHub

- Local path: `D:\Projects\Workspace\references\OmenSuperHub`
- Current commit SHA: `a6ab6988c446ee5421466097fdf60c0d521e5c81`
- Primary language/runtime: C# on .NET Framework 4.8
- UI framework: WPF with some WinForms interoperability/signals from project references
- Solution/project files: `OmenSuperHub.sln`, `OmenSuperHub.csproj`, bundled `LibreHardwareMonitor/LibreHardwareMonitor.sln`
- Relevance: Medium

Short summary: Offline HP OMEN/HyperX control console with fan control, power management, Omen key customization, and monitoring. Useful for alternative HP control concepts, but it bundles many binaries and a full LibreHardwareMonitor tree, so it should remain a cautious reference.

Top-level structure summary: root contains WPF/app files, `App`, `LibreHardwareMonitor`, `Properties`, `Resources`, `Program*.cs`, `OmenHardware.cs`, fan curve files, and project metadata.

Key candidate files/folders:

- Startup/menu/config: `Program.cs`, `Program.Config.cs`, `Program.Menu.cs`, `Program.OmenKey.cs`
- Hardware access: `OmenHardware.cs`, `App/OmenLighting.cs`, `App/GpuAppManager.cs`
- Fan control: `FanCurveForm.cs`, `FanCurveProfile.cs`
- Telemetry: `LibreHardwareMonitor/LibreHardwareMonitorLib/Hardware`, especially `Hardware/Sensor.cs`, `Hardware/Computer.cs`, `Hardware/Motherboard`, `Hardware/Cpu`, `Hardware/Gpu`
- EC/low-level access references: `LibreHardwareMonitor/LibreHardwareMonitorLib/PawnIo`, `LibreHardwareMonitor/LibreHardwareMonitorLib/Resources/PawnIo`
- Settings/config: `Program.Config.cs`, `app.config`
- Logging: `App/Logger.cs`

Avoid:

- `Resources` vendor DLLs, HP DLLs, certificates, drivers, `.sys`, `.inf`, `.cat`, images, and icons
- Bundled `LibreHardwareMonitor` source except for narrow telemetry concept questions
- Generated resource/designer files
- Running or installing anything related to PawnIO or bundled resources

Recommended next inspection question: Which high-level fan-curve and tray/config behaviors are useful as UX references without depending on bundled drivers or HP DLLs?

## MasonDye/OmenXHub

- Local path: `D:\Projects\Workspace\references\OmenXHub`
- Current commit SHA: `ca84cb011d1d3e5850445d19e45fbea06e83a8fd`
- Primary language/runtime: C# on .NET Framework `net481`, plus small Node/package metadata and native driver project
- UI framework: WPF with WPF-UI package references and some Windows Forms usage
- Solution/project files: `OmenSuperHub.sln`, `OmenSuperHub.csproj`, `driver/OmenXHubDrv.vcxproj`, `LibreHardwareMonitor-pawnio-squashed/LibreHardwareMonitor.sln`
- Relevance: Medium

Short summary: WPF-based offline OMEN/Victus control center derived from OmenSuperHub. It has many service-oriented files, rich UI/page structure, fan and EC service names, and useful UX/service-boundary ideas. It also includes native driver and bundled monitoring code, so it is not a source import candidate.

Top-level structure summary: root contains `App`, `Controls`, `Models`, `Pages`, `Services`, `Themes`, `Views`, `Windows`, `driver`, `LibreHardwareMonitor-pawnio-squashed`, `Preview`, assets, and project metadata.

Key candidate files/folders:

- Startup/app shell: `App.xaml`, `App.xaml.cs`, `Views/MainWindow.xaml`, `Views/MainWindow.xaml.cs`
- Service shape: `Services/HardwareService.cs`, `Services/HardwareApiService.cs`, `Services/FanService.cs`, `Services/EcFanService.cs`, `Services/ConfigService.cs`, `Services/TrayService.cs`
- Hardware access: `OmenHardware.cs`, `Services/OmenXHubDriver.cs`, `Services/NativeDefs.cs`, `Pages/NativeMethods.cs`
- Fan UI/control: `Pages/FanPage.xaml`, `Pages/FanPage.xaml.cs`, `Controls/FanIcon.cs`
- Telemetry: `Services/HWiNFOService.cs`, `Services/HWiNFOReaderService.cs`, `Services/PresentMonFpsMonitor.cs`, `LibreHardwareMonitor-pawnio-squashed/LibreHardwareMonitorLib/Hardware`
- Lighting: `App/OmenLighting.cs`, `Services/OmenLightingNative.cs`, `Services/LightingSceneService.cs`, `Services/LightingTemperatureService.cs`, `docs/lighting-reverse-findings.md`
- Settings/config: `Services/ConfigService.cs`, `Pages/SettingsPage.xaml`, `Pages/SettingsPage.xaml.cs`
- Architecture/UX notes: `ARCHITECTURE.md`, `FRONTEND_REVIEW.md`, `README.en.md`

Avoid:

- `driver` native project and any driver build/install path
- `LibreHardwareMonitor-pawnio-squashed` except targeted telemetry questions
- `Preview`, `assets`, `Resources`, images, icons, and bundled binary/resource payloads
- Network boost, macro, cleanup, and automation features unless explicitly scoped later
- Native/vendor interop copied directly into VictusControl

Recommended next inspection question: Which service boundaries from the WPF app are useful for a clean VictusControl app shell, and which features should be deliberately excluded from the first version?

## affaan-m/ECC

- Local path: `D:\Projects\Workspace\references\ECC`
- Current commit SHA: `5eddf1a3ffd311423be2d4ba7d26f7209c91b033`
- Primary language/runtime: Node.js/TypeScript-oriented tooling with Python components
- UI framework: none relevant to VictusControl hardware utility planning
- Solution/project files: `package.json`, `pyproject.toml`; no C# solution/project files found in the token-safe scan
- Relevance: Low

Short summary: AI assistant harness/workflow repository, not a Windows hardware utility. It is relevant only for AI workflow ideas, context budgeting, agent prompts, and review discipline. It should not guide VictusControl application architecture or hardware implementation.

Top-level structure summary: large tooling repo with `agents`, `commands`, `config`, `contexts`, `docs`, `hooks`, `mcp-configs`, `plugins`, `rules`, `schemas`, `scripts`, `skills`, `src`, `tests`, and workflow-related metadata.

Key candidate files/folders:

- AI workflow examples: `AGENTS.md`, `CLAUDE.md`, `WORKING-CONTEXT.md`, `COMMANDS-QUICK-REF.md`
- Context/rules systems: `contexts`, `rules`, `commands`, `schemas`, `mcp-configs`
- Agent examples: `agents/csharp-reviewer.md`, `agents/code-reviewer.md`, `agents/security-reviewer.md`, `agents/build-error-resolver.md`
- Workflow docs: `workflows/README.md`, `the-shortform-guide.md`, `the-longform-guide.md`, `the-security-guide.md`
- Package metadata: `package.json`, `pyproject.toml`

Avoid:

- Treating ECC as a hardware reference
- Importing its agent framework, scripts, plugins, marketplace content, or install logic into VictusControl
- Running `install.*`, npm scripts, dashboards, hooks, or any orchestration commands
- Large multilingual docs/assets scans

Recommended next inspection question: Are there any compact AI workflow conventions worth adapting into VictusControl docs, or is the current workflow layer already sufficient?
