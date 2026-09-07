# Elevated HP WMI Read-Only Test Guide

## Purpose

This test checks whether the single approved `SystemDesignData` HP BIOS WMI read-only command can execute when VictusX is running in explicit HP developer-test mode.

## Preconditions

- Use only the HP Victus target machine.
- Build succeeds with `dotnet build VictusX.sln`.
- Run from an elevated Administrator terminal.
- Understand that this is developer-only diagnostics, not production HP control.
- Do not interact with fan, power, battery, RGB, keyboard lighting, GPU, thermal, EC, or BIOS-write features.

## Command

Run from the repository root in an elevated Administrator terminal:

```bash
dotnet run --project app\VictusX.csproj -- --hp-victus --hp-wmi-readonly-test
```

## Expected safe behavior

- VictusX starts in HP Victus mode.
- Only the `SystemDesignData` read-only invocation path is eligible to run.
- No fan, performance mode, battery, RGB, keyboard lighting, GPU, EC, BIOS setting, power, or thermal write commands should run.
- Any failure should be captured as a structured report result instead of crashing the app.

## Report fields to check

Check:

- `SystemDesignDataInvocationAllowed`
- `SystemDesignDataInvocationAttempted`
- `SystemDesignDataInvocationSucceeded`
- `SystemDesignDataReturnedByteCount`
- `SystemDesignDataInvocationError`
- `ProcessElevated`
- `HpWmiInvocationBlockedReason`
- `HpWmiRecommendedNextStep`

## Report path

The report is written to:

```text
%APPDATA%\VictusX\hp-capability-report.json
```

## Success meaning

Success means the HP WMI provider accepted the single read-only `SystemDesignData` test under the required flags and elevation. It does not mean fan control, performance mode control, battery control, RGB control, keyboard lighting control, GPU control, or production HP hardware control is supported.

## Access denied meaning

`Access denied` means the WMI surface exists but the provider refused execution. Likely causes include elevation, WMI provider permissions, HP service/driver gating, or a different required HP interface path.

## Abort conditions

Abort immediately if:

- the command is not being run from an elevated Administrator terminal;
- any flag other than `--hp-victus --hp-wmi-readonly-test` is needed to make it work;
- the app attempts fan, power, thermal, battery, RGB, keyboard lighting, GPU, EC, or BIOS-write behavior;
- the app crashes before writing a report;
- unexpected raw hardware identifiers or sensitive data appear in logs or reports.

## What remains forbidden

- Fan control
- Performance mode control
- Battery charge limit control
- RGB / keyboard lighting control
- GPU mode control
- EC access
- BIOS setting writes
- Power or thermal writes
- Vendor DLLs or proprietary HP binaries
- Any HP WMI command other than the explicitly approved `SystemDesignData` read-only test
