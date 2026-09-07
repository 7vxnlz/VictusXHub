# HP WMI Invocation Implementation Comparison

Scope: compare VictusX HP WMI invocation against `ghelper-omen/app/Omen/HpWmiBios.cs` without invoking any `hpqBIOSInt*` method.

## ghelper-omen WMI shape

- Creates a `CimSession` with `CimSession.Create(null)`.
- Reads the `root\wmi` `hpqBDataIn` class and creates a reusable input `CimInstance`.
- Sets `Sign` to the shared `SECU` byte signature.
- Creates a keyed `hpqBIntM` `CimInstance` using `InstanceName = ACPI\PNP0C14\0_0`, then resolves it with `GetInstance`.
- Uses a CIM-first invocation path and only falls back to legacy `System.Management` WMI after CIM command failure.
- Uses a 5-second `CimOperationOptions.Timeout` for CIM invocations.

## ghelper-omen input parameters

- `Sign`: `SECU`.
- `Command`: BIOS command family, commonly `0x20008` for default commands.
- `CommandType`: specific command ID, for example `0x28` for system design data.
- `Size`: `0` when there is no input payload; otherwise input payload length.
- `hpqBData`: set only when an input payload exists.
- `InData`: wraps the populated `hpqBDataIn` object.

## ghelper-omen method selection

- Method name is selected from output size: `hpqBIOSInt{outDataSize}`.
- `SystemDesignData` uses command family `Default`, command type `0x28`, no input payload, and output size `128`, so it calls `hpqBIOSInt128`.

## ghelper-omen output handling

- Reads `OutData`.
- Reads `rwReturnCode`.
- If return code is `0`, returns the `Data` byte array for non-zero output sizes.
- If return code is non-zero, logs the code and treats the command as failed.
- Tracks command reliability and can temporarily disable WMI commands after repeated failures.

## ghelper-omen exception and access handling

- CIM path catches `CimException` separately and logs native/status details.
- General exceptions are throttled to avoid log spam.
- After CIM failure, it attempts the legacy `System.Management` path.
- Legacy path catches general exceptions and logs a debug failure.
- Access denied is not specially recovered beyond the normal failure/fallback behavior.

## VictusX current WMI shape

- Uses only the legacy-style `System.Management` path for real invocation.
- Opens `\\.\root\wmi` with `ManagementScope`.
- Enumerates `hpqBIntM` instances and selects the instance whose `InstanceName` contains `ACPI\PNP0C14\0_0`.
- Creates `hpqBDataIn` input data through `ManagementClass.CreateInstance()`.
- Calls `GetMethodParameters()` and then `InvokeMethod()` on the selected `ManagementObject`.
- Invocation is blocked unless both `--hp-victus` and `--hp-wmi-readonly-test` are present.
- Only `SystemDesignData` is eligible for real invocation.

## Meaningful differences from VictusX

1. ghelper-omen has a CIM-first path; VictusX currently has only the legacy `System.Management` path.
2. ghelper-omen invokes the CIM class method directly; VictusX invokes the selected legacy `ManagementObject` instance.
3. ghelper-omen keeps a resolved CIM BIOS method instance during initialization; VictusX resolves the legacy instance only at invocation time.
4. ghelper-omen applies a timeout to CIM invocation; VictusX does not currently have an invocation timeout.
5. ghelper-omen attempts heartbeat/read commands during initialization; VictusX intentionally avoids automatic real invocation unless the explicit test flag is present.
6. ghelper-omen can fall back from CIM to legacy WMI; VictusX has no CIM path to try before legacy WMI.
7. ghelper-omen returns raw output bytes to callers; VictusX records only success/failure, return code, and returned byte count.
8. ghelper-omen tracks repeated command failures and throttles logs; VictusX returns one structured result per attempt.
9. VictusX has stricter safety gates than ghelper-omen: HP mode required, explicit read-only test flag required, command allowlist required, read-only classification required, and exposed-method validation required.
10. VictusX's legacy input construction matches the ghelper-omen legacy fallback for `SystemDesignData`: `Sign=SECU`, `Command=0x20008`, `CommandType=0x28`, `Size=0`, method `hpqBIOSInt128`, and no payload.

## Differences that may explain Access denied

- Most likely: WMI provider permission/elevation. VictusX diagnostics show the process is not elevated while `root\wmi`, `hpqBIntM`, and method metadata are readable. That combination fits a provider that allows metadata reads but rejects method invocation.
- Plausible: the CIM path may behave differently from the legacy `System.Management` path on this machine or BIOS version. ghelper-omen's primary path is CIM, while VictusX's only real invocation path is legacy WMI.
- Less likely: malformed `SystemDesignData` input. VictusX's legacy parameter construction matches ghelper-omen's legacy fallback for the same command shape.
- Less likely: wrong method name. VictusX validates that `hpqBIOSInt128` is exposed before invocation.
- Less likely: missing HP WMI class or service. Current diagnostics show the namespace, class, and method metadata are readable, and HP-related services are present.

## Exact recommended next code change

Add a non-invoking CIM readiness probe for `SystemDesignData` that mirrors ghelper-omen's CIM setup without calling `InvokeMethod`:

- create a `CimSession`;
- confirm `root\wmi` can load `hpqBDataIn`;
- confirm `Sign`, `Command`, `CommandType`, `Size`, and `hpqBData` properties exist;
- confirm `hpqBIntM` can be resolved with `InstanceName = ACPI\PNP0C14\0_0`;
- confirm `hpqBIOSInt128` method metadata exists through CIM;
- write these diagnostics to `hp-capability-report.json`.

After that, if the maintainer explicitly approves another real test, try the existing `System.Management` invocation once as administrator before adding any CIM invocation path.

## Must remain forbidden

- No automatic `hpqBIOSInt*` invocation in `--hp-victus` mode.
- No invocation without `--hp-wmi-readonly-test`.
- No fan, EC, BIOS write, power, thermal, RGB, keyboard, battery, or performance-mode commands.
- No raw returned data logging by default.
- No ASUS default behavior changes.
