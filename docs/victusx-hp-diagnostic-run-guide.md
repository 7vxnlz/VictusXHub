# VictusX HP Diagnostic Run Guide

VictusX HP Diagnostic mode is a read-only development shell. It writes the HP capability report and exposes cached diagnostic actions without enabling fan or performance control.

## Safe Command-Line Run

From the repository root:

```powershell
dotnet run --project app\VictusX.csproj -- --hp-victus
```

The window should open as `VictusX Read-only Diagnostic`. Normal HP Diagnostic mode does not attempt the explicit HP BIOS WMI probes.

## Visual Studio and Rider

Select the `VictusX HP Diagnostic` launch profile, then run or debug the project. The profile supplies only `--hp-victus`; the existing `GHelper` profile remains unchanged for the default application path.

## Local Files

- Capability report: `%APPDATA%\VictusX\hp-capability-report.json`
- Sanitized exports: `%APPDATA%\VictusX\Logs\Reports\hp-diagnostic-YYYYMMDD-HHMMSS.md`

Copy summary, reload cached report, open diagnostic folder, and export diagnostic report are local-file-only actions.

## Safety

The explicit HP WMI test path is developer-only, requires separate safety gates and elevation, and is intentionally absent from launch profiles. Do not add or use `--hp-wmi-readonly-test` for normal development runs.

Fan control, fan writes, performance control, and production HP hardware control are not implemented. SetFanMax remains NO-GO/design-only.
