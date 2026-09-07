# HP Telemetry Roadmap

## Proven Read-Only Telemetry

- `SystemDesignData`: 128-byte capability data decoded successfully.
- `FanGetCount`: reports two fans.
- `FanMaxGet`: reports max fan disabled.
- `FanGetLevel`: returns raw-only fan-level bytes; they are not RPM, percent, a curve, or a control level.

## Report-Backed Diagnostic UI

HP mode has a read-only Diagnostic dashboard backed by the existing capability snapshot and cached `hp-capability-report.json`. It can show device and WMI/CIM readiness, decoded capability facts, fan count, max-fan state, raw fan-level values, and the SetFanMax NO-GO state. Its copy, reload, folder, and export actions are local-file-only and do not invoke WMI.

## Deferred Probes

| Probe | Status | Why deferred |
| --- | --- | --- |
| `FanGetRpm` | Deferred | Reference evidence suggests V2/OMEN Max-specific or unreliable behavior on this ThermalPolicyVersion 1 Victus. |
| `GetFanType` | Deferred | Method and sensor/type semantics are not sufficiently model-specific. |
| `GpuGetPower` | Documentation-only | It is read-shaped, but its four bytes are not proof of active performance mode, applied wattage, or safe GPU policy semantics on this device. |
| Temperature `0x23` | Deferred | References disagree on selector meaning; CPU/GPU labels conflict with ambient, PCH, VR, and board-sensor mappings. |

No deferred probe is prepared for invocation, catalogued for use, decoded in VictusX, or shown as live telemetry.

## Forbidden Control And Write Areas

Fan writes and control, performance-mode or power-limit writes, GPU power writes, EC writes, BIOS write-capable paths, `SetFanMax`, `SetFanMode`, `SetFanLevel`, and `0x37` remain forbidden. SetFanMax is design-only and remains NO-GO.

## Safe UI Improvements

The app can safely improve the existing Diagnostic dashboard with clearer cached-data presentation, report schema/version messaging, local export readability, accessibility, and missing-data guidance. It must not add polling, refresh hardware, control buttons, sliders, toggles, or inferred sensor units.

## Evidence Required Before A New Probe

A future read-only probe needs exact reference evidence for command type, method/input/output shape, model/firmware applicability, safe decoder boundaries, and user-facing semantics. Unknown bytes must remain summarized. Before preparation, the explicit `--hp-victus`, `--hp-wmi-readonly-test`, elevation, and SafeReadOnlyInvocation gates must be shown to apply; normal HP mode must remain non-invoking.

## Evidence Required Before Any Write

Any write needs the separate SetFanMax NO-GO package resolved: device-validated input length, proven restore/disable behavior, FanMaxGet pre/post/restore readbacks, manual recovery evidence, AC/thermal observation plan, explicit human approval, and guarded runtime design. Reference resemblance alone is insufficient.

## Recommended Next Safe Implementation Task

Improve the read-only Diagnostic dashboard's cached-report usability, such as report schema/version and missing-data guidance, with no WMI invocation or hardware-control surface.
