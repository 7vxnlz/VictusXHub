# HP Diagnostic Preview User Safety Notes

## What the Preview Does

The future VictusX HP diagnostic preview is intended to open a read-only diagnostic window for supported HP Victus testing. It shows cached/report-backed status such as device detection, WMI/CIM readiness, read-only telemetry milestones, fan count, max-fan status, raw fan level bytes, and SetFanMax NO-GO/design-only state.

It may let users copy, reload, open, or export the local diagnostic report. These actions use local files only.

## What the Preview Does Not Do

- It does not control fans.
- It does not write fan settings.
- It does not control performance modes.
- It does not change power limits.
- It does not write EC, BIOS, RGB, keyboard lighting, battery, GPU, or thermal settings.
- It does not claim production HP hardware control support.

## Diagnostic-Only Mode

Preview users should launch through the packaged launcher or shortcut that supplies only:

```text
--hp-victus
```

Normal HP diagnostic mode is local/report-backed and should not run explicit HP WMI test probes.

## Developer-Only Flag Warning

`--hp-wmi-readonly-test` is for controlled developer-only elevated testing. It must not be used by preview users and must not appear in preview launchers, shortcuts, publish profiles, or release instructions.

## Report and Export Locations

- Cached diagnostic report: `%APPDATA%\VictusX\hp-capability-report.json`
- Sanitized exported summaries: `%APPDATA%\VictusX\Logs\Reports`

Exported summaries must not include raw binary dumps or imply hardware-control support.

## What Users Should Report Back

- Whether the window opens as `VictusX Read-only Diagnostic`.
- Whether the Diagnostic/Quit-only shell appears.
- Whether the report path exists.
- Whether export succeeds.
- Any startup, shutdown, or Windows error dialog.
- The visible Diagnostic dashboard summary, without raw binary dumps or personal/machine-specific paths unless requested for debugging.

## Release-Blocked Status

No HP diagnostic preview artifact has been released from the current state. Release remains blocked by package-license/notice review, icon replacement, signing/checksum evidence, and clean-machine validation.
