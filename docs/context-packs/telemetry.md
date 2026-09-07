# Telemetry Context Pack

Use for HP read-only CPU/GPU/fan/battery/display status. Add a source-specific file only after locating its symbol.

## Files

- `AGENTS.md`
- `AI_WORKING_STATE.md`
- `app/Hardware/Hp/AGENTS.md`
- `app/Hardware/Hp/HpReadOnlyTelemetry.cs`
- `app/Hardware/Hp/HpReadOnlyTelemetryFormatter.cs`
- `app/Hardware/Hp/HpWindowsTelemetrySource.cs`
- `app/Hardware/Hp/HpGpuTemperaturePoller.cs`
- `app/Hardware/Hp/HpNvidiaTemperatureSource.cs`
- `tests/VictusX.Tests/Hardware/Hp/HpReadOnlyTelemetryTests.cs`
- `docs/hp-temperature-fan-rpm-telemetry.md`
- `docs/hp-victus-ui-usefulness-gap-audit.md`

Do not include fan write runners. `FanGetLevel` stays raw-only and V1 RPM stays unavailable unless separately proven.
