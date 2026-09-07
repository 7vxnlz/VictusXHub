# HP Read-Only Telemetry UI Implementation Spec

## Current Status

This is an implementation specification only. No UI, WMI invocation, hardware control, fan control, or fan write behavior is implemented by this document.

## Data Source

The panel should read from the persisted HP capability report at `%APPDATA%\VictusX\hp-capability-report.json` and map existing `HpVictusCapabilitySnapshot` fields into a read-only view model.

Do not invoke WMI from the UI. Do not refresh live hardware state directly from the panel. Treat missing report fields as unknown rather than false device facts.

## Proposed UI Location

Add the panel as an HP developer diagnostics section in the existing settings/main window that is shown by `--hp-victus`.

The section should be visually separate from ASUS/G-Helper controls and should be labeled as read-only HP diagnostics. Default ASUS behavior and existing control surfaces must remain unchanged.

## Read-Only Fields To Display

- HP Victus developer mode active.
- Device manufacturer, model, and product identifier when present.
- WMI/CIM readiness and elevation status from the report.
- `SystemDesignDataInvocationSucceeded`
- `SystemDesignDataDecodeSucceeded`
- `ShippingAdapterPowerRatingWatts`
- `ThermalPolicyVersion`
- `DeclaresSoftwareFanControlSupport`
- `FanGetCountInvocationSucceeded`
- `FanCount`
- `FanMaxGetInvocationSucceeded`
- `IsMaxFanEnabled`
- `FanGetLevelInvocationSucceeded`
- `Fan1RawValue`
- `Fan2RawValue`
- Unknown byte summaries as count/range/non-zero count only.

## FanGetLevel Raw-Only Labels

Use labels such as:

- `Fan 1 raw level byte`
- `Fan 2 raw level byte`
- `Raw firmware level values`

Do not label these values as RPM, percent, speed, curve position, target level, control level, thermal state, or user-adjustable fan setting.

## Disabled Or Hidden Controls

Do not add or expose:

- Fan sliders.
- Fan toggles.
- Fan curve editors.
- Target RPM controls.
- Percent speed controls.
- Apply, Restore, Auto, Boost, Max Fan, or Manual Fan buttons.
- SetFanMax experiment controls.
- SetFanLevel, SetFanMode, or `0x37` controls.
- FanGetRpm or GetFanType UI fields until those commands are separately proven safe and report-backed.

## NO-GO Safety Badge

Show a clear status label:

- `Fan control: not implemented`
- `Fan writes: not implemented`
- `SetFanMax: NO-GO / design only`

If the report includes SetFanMax dry-run fields, show `SetFanMaxWriteImplemented=false`, `SetFanMaxWriteAllowed=false`, and the blocked reasons in a read-only safety area.

## Refresh Behavior

The first implementation should load the existing report on panel open and support a manual report reload action only.

Reload must reread the JSON file from disk. It must not invoke WMI, start probes, request elevation, or run `--hp-wmi-readonly-test`.

## Error And Empty States

If the report is missing, show `No HP capability report found`.

If read-only probes were not explicitly run, show `No explicit read-only telemetry invocation recorded`.

If decode fields are missing or failed, show decode status and errors only. Do not infer replacement values.

If the app is not in `--hp-victus` mode, hide the HP developer diagnostics panel or show it as unavailable.

## Test Strategy

- Unit test the report-to-view-model mapping with synthetic JSON or synthetic snapshot objects.
- Verify missing/null report fields render as unknown.
- Verify FanGetLevel values remain raw-only in labels and view-model property names.
- Verify SetFanMax safety status renders as NO-GO when write fields are false or missing.
- Verify the UI/view model has no dependency on WMI invocation classes.
- Verify normal `--hp-victus` still writes the report without invoking read-only test commands.

## Exact Next Safe Coding Step

Create a report-backed HP telemetry view model that accepts an existing `HpVictusCapabilitySnapshot` or report DTO and exposes display-only fields for the planned panel. Add tests for mapping, empty states, raw-only fan level labels, and NO-GO safety text. Do not connect the view model to WMI or add any control widgets.
