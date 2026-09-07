# HP WMI SystemDesignData Access Denied Note

## 1. What was attempted

`SystemDesignData` was attempted once as the first controlled read-only HP BIOS WMI invocation in `--hp-victus` mode.

## 2. Result

The invocation failed with `ManagementException: Access denied`.

## 3. Why this is acceptable/safe

The attempt was limited to the single approved read-only candidate, did not invoke fan, power, thermal, keyboard, RGB, battery, EC, or BIOS-write commands, and returned a structured failure instead of crashing the app.

## 4. Current fix

Real HP BIOS WMI invocation now requires both flags:

- `--hp-victus`
- `--hp-wmi-readonly-test`

`--hp-victus` alone now runs identity, WMI availability, and dry-run validation only.

## 5. Meaning for future HP WMI work

Future HP WMI work must treat method availability as separate from invocation permission. The legacy `System.Management` path can read metadata, and the CIM path can also read class and method metadata. However, CIM instance metadata also returned `Access denied`, which points toward elevation, WMI provider permissions, or HP service/driver gating rather than a missing WMI surface.

## 6. Next investigation options

- Administrator elevation.
- WMI namespace permissions.
- HP service/driver dependency.
- Alternate HP InstrumentedBIOS path.

## 7. What remains forbidden

Fan control, performance mode control, battery control, keyboard lighting control, RGB/lightbar control, EC access, BIOS writes, power writes, thermal writes, vendor DLLs, proprietary HP binaries, background loops, and any HP WMI command other than explicitly approved read-only tests.
