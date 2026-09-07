# FanGetCount Read-Only Probe Plan

## Why This Is First

`FanGetCount` (`CommandType 0x10`, `hpqBIOSInt4`) is the smallest fan-related read-only candidate found in the HP references. It returns a short status-shaped buffer and is used as a basic fan availability/count query, not as a fan control command.

`SystemDesignData` reporting `DeclaresSoftwareFanControlSupport=true` makes this worth testing, but it does not make fan control safe.

## Required Flags And Elevation

- `--hp-victus`
- `--hp-wmi-readonly-test`
- Elevated Administrator process
- Explicit future wiring for `FanGetCount` only

## Expected Safety Gates

- HP Victus mode must be enabled.
- The explicit read-only test flag must be present.
- The process must be elevated.
- `hpqBIntM` and `hpqBDataIn` must be available.
- `hpqBIOSInt4` must be exposed.
- The command must match the catalog entry exactly: read-only, `SafeReadOnlyInvocation`, 4-byte input, 4-byte output.
- Invocation must be single-shot, with no polling, retries, heartbeat, writes, restore behavior, UI controls, or EC fallback.
- Logs and reports must avoid full raw binary output.

## Expected Report Fields

- `FanGetCountInvocationAllowed`
- `FanGetCountInvocationAttempted`
- `FanGetCountInvocationSucceeded`
- `FanGetCountReturnedByteCount`
- `FanGetCountDecodeSucceeded`
- `FanGetCountDecodeErrors`
- `FanGetCountDecoded.FanCount`
- `FanGetCountDecoded.ProtectionStatusRaw`
- `FanGetCountDecoded.OverCurrentProtectionTripped`
- `FanGetCountDecoded.OverTemperatureProtectionTripped`
- `FanGetCountDecoded.UnknownByteCount`
- `FanGetCountDecoded.UnknownByteRange`
- `FanGetCountDecoded.UnknownNonZeroByteCount`

## Still Forbidden

- `SetFanMode` / `0x1A`
- `SetFanLevel` / `0x2E`
- `SetFanMax` / `0x27`
- Ambiguous `0x37`
- Fan speed control, fan writes, polling, performance mode control, battery control, EC access, BIOS writes, hardware writes, and ASUS behavior changes

## Later Manual Test Step

After explicit `FanGetCount` invocation routing is added and reviewed, run this from an elevated Administrator terminal:

```powershell
dotnet run --project app\VictusX.csproj -- --hp-victus --hp-wmi-readonly-test
```

Do not run that command from Codex.
