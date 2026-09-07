# HP Read-Only Telemetry UI Plan

## Scope

Show report-backed HP diagnostics only. The panel must not invoke WMI, poll hardware, expose control actions, or imply production hardware-control support.

## Suggested Sections

### Device Identity

Show the detected manufacturer, model, product identifier, and whether HP Victus developer mode is active.

### WMI/CIM Readiness

Show readiness and elevation status from the existing capability report. Label this **Diagnostic readiness**, not hardware-control availability.

### SystemDesignData Facts

Show the read-only decode status, shipping adapter rating, thermal policy version, and **Software fan control declared by firmware: Yes/No**. Add: **Capability declaration only; fan control is not implemented.**

### Fan Read-Only Facts

Safely show:

- **Detected fans:** `FanCount`
- **Maximum fan mode:** Enabled/Disabled from `IsMaxFanEnabled`
- **Fan 1 raw level byte:** `Fan1RawValue`
- **Fan 2 raw level byte:** `Fan2RawValue`
- **Raw level data:** **Diagnostic firmware values; not RPM or percent.**

Do not interpret raw levels as RPM, percentage, fan speed, curve position, control level, or thermal behavior. Unknown bytes may be shown only as count/range summaries.

### Safety / NO-GO Status

Show **Fan control: Not implemented**, **Fan writes: Not implemented**, and **SetFanMax experiment: NO-GO (design only)**. When live read-only probes have not been explicitly run, show **No live telemetry invocation performed** rather than presenting defaults as device readings.

## Hidden Or Disabled

Do not add fan buttons, toggles, sliders, presets, curves, target speeds, Apply/Restore actions, or write-test controls. Hide unsupported or ambiguous commands including `SetFanMax`, `SetFanLevel`, `SetFanMode`, `0x37`, `FanGetRpm`, and `GetFanType`.

## Why Controls Must Wait

Read-only success proves report-safe diagnostics, not safe write payloads, restore behavior, thermal bounds, or recovery. A control UI would falsely imply those guarantees and could expose an unsafe path before the SetFanMax gate moves from NO-GO.

## Recommended Next Implementation Step

Build a report-backed, read-only view model and static diagnostics panel using persisted `hp-capability-report.json` data. Keep it disconnected from WMI invocation and omit every control affordance.
