# HP Diagnostic Shell Regression Audit

## Result

`--hp-victus` starts a visible `VictusX Read-only Diagnostic` window, writes the capability report, and uses a Diagnostic/Quit-only tray menu. The report-backed Diagnostic panel remains available with copy, reload, open-folder, and export actions; these operate on local cached data and files only.

## Isolation Boundaries

- HP mode skips inherited power, display, session, clamshell, updater, peripheral-refresh, tray-sensor, and sensor-timer hooks.
- Its window procedure bypasses inherited ASUS power-broadcast handling while retaining the taskbar tray-icon restoration message.
- Inherited fan, performance, GPU, battery, lighting, ASUS, and update surfaces are hidden. No controls or write paths are enabled.
- Standard G-Helper behavior remains on the non-HP branch; the new guards are conditional on `--hp-victus`.

## Report and Shutdown

- Startup writes `hp-capability-report.json`; explicit `SystemDesignData`, `FanGetCount`, `FanMaxGet`, and `FanGetLevel` attempts remain false without `--hp-wmi-readonly-test`.
- Quit calls WinForms application shutdown. Tray cleanup is guarded against the one-shot retry timer, and HP mode does not unregister lifecycle hooks it never registered.

## Current Limitations

The dashboard is cached/read-only, so values can be unavailable or stale. Fan and performance control remain absent; SetFanMax remains NO-GO/design-only.

## Recommended Next Safe Task

Add focused regression tests for HP-mode guard decisions and cached-report fallbacks. Do not add live hardware actions or controls.
