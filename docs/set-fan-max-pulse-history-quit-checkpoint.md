# SetFanMax Pulse History And HP Diagnostic Quit Checkpoint

## Pulse History Status

The HP Diagnostic dashboard now includes a read-only "Latest SetFanMax pulse/history" section. It summarizes local append-only SetFanMax experiment and pulse logs for manual review only. The section is evidence display, not a control surface.

The data source is local file storage under `%APPDATA%\VictusX\Logs\FanExperiments\`. The loader reads JSON records from that folder only, selects the latest valid SetFanMax pulse/experiment record, and never invokes WMI, starts a probe, executes a command, or refreshes hardware.

The latest pulse summary may show:

- Timestamp.
- Payload used.
- `WriteExecuted`.
- `EnableCommandSucceeded`.
- `RestoreCommandSucceeded`.
- `PhysicalFanResponseObserved`.
- `RestoreObserved`.
- `ReadbackReliability`.
- `ExperimentalOutcomeClassification`.
- Bounded manual observation notes.

Missing folders, empty folders, malformed JSON, truncated JSON, unrelated records, and missing fields fail closed. The dashboard shows no optimistic pulse status and uses "No valid pulse history available" or unknown/not validated wording instead of inferring physical response, restore behavior, payload validation, or user-facing control permission.

## No-Control Guarantees

The pulse history/status view contains:

- No pulse/run button.
- No fan slider.
- No fan toggle.
- No fan curve UI.
- No normal fan UI.
- No WMI invocation from the dashboard.
- No tray or Diagnostic action that can start a pulse.

The existing HP Diagnostic actions remain local-only: copy summary, reload cached report, open report folder, and export diagnostic report. They do not run read-only probes or write-capable commands.

## HP Diagnostic Quit Behavior

HP Diagnostic Quit now uses the diagnostic shell cleanup path. In HP mode it disposes the diagnostic tray/menu resources, hides and releases the tray icon, stops the tray retry timer, and exits the WinForms UI message loop on the UI thread.

This preserves the Diagnostic/Quit-only HP shell and avoids inherited ASUS/G-Helper lifecycle hooks that are not needed for read-only HP diagnostic mode. Explicit Quit is expected to fully terminate the VictusX process without leaving a tray-resident instance.

## Current Readiness

The developer-only four-byte Max Fan Pulse path is operational only behind explicit command-line approval gates and append-only logging. It is not exposed in the HP Diagnostic dashboard, tray menu, or normal UI.

Normal/user-facing fan control remains **NO-GO**. `DeviceValidatedInputLength` remains null/unset, FanMaxGet readback remains inconclusive, one-byte remains comparison-only, and no payload is fully validated for normal control.

## Remaining Limitations

- Pulse history depends on existing local logs; it cannot prove anything when logs are absent or malformed.
- Manual observation fields remain operator-supplied evidence and are not automatic hardware truth.
- FanMaxGet remains unreliable as the sole success criterion for this device/BIOS.
- The dashboard does not provide live telemetry, live polling, fan control, performance control, or hardware refresh.
- Preview/release readiness remains separate from developer-only hardware experiments.

## Recommended Next Safe Task

Keep the HP Diagnostic dashboard as a read-only evidence surface. The next safe task is a source-level audit of the pulse history loader, dashboard formatter, and Quit lifecycle tests, or a documentation checkpoint for how local pulse-history evidence should be reviewed before any future developer-only experiment.
