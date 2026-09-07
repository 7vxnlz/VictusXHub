# FanGetLevel Decode Verification Guide

## Later Manual Command

Run from an elevated Administrator terminal only after explicit approval:

```powershell
dotnet run --project app\VictusX.csproj -- --hp-victus --hp-wmi-readonly-test
```

Do not run this command from Codex.

## Report Fields To Copy Back

- `FanGetLevelInvocationAllowed`
- `FanGetLevelInvocationAttempted`
- `FanGetLevelInvocationSucceeded`
- `FanGetLevelReturnedByteCount`
- `FanGetLevelInvocationError`
- `FanGetLevelDecodeSucceeded`
- `FanGetLevelDecodeErrors`
- `FanGetLevelDecoded`

## Expected Safe Result

- Invocation is attempted only with `--hp-victus`, `--hp-wmi-readonly-test`, elevation, and a `SafeReadOnlyInvocation` catalog entry.
- Returned byte count is expected to be `128` if firmware accepts the read.
- Decode succeeds or fails safely.
- Only the known raw prefix is retained; the remaining bytes are summarized.
- Raw values are not RPM, percent, a fan curve, or a control level.

## Still Forbidden

Fan control, fan writes, fan speed control, `SetFanMode` / `0x1A`, `SetFanLevel` / `0x2E`, `SetFanMax` / `0x27`, ambiguous `0x37`, performance mode control, battery control, EC access, BIOS writes, hardware writes, polling loops, retries, and ASUS behavior changes remain forbidden.
