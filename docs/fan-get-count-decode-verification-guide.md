# FanGetCount Decode Verification Guide

## Later Manual Command

Run from an elevated Administrator terminal only after explicit approval:

```powershell
dotnet run --project app\VictusX.csproj -- --hp-victus --hp-wmi-readonly-test
```

Do not run this command from Codex.

## Report Fields To Copy Back

- `FanGetCountInvocationAllowed`
- `FanGetCountInvocationAttempted`
- `FanGetCountInvocationSucceeded`
- `FanGetCountReturnedByteCount`
- `FanGetCountInvocationError`
- `FanGetCountDecodeSucceeded`
- `FanGetCountDecodeErrors`
- `FanGetCountDecoded`

## Expected Safe Result

- Invocation is attempted only with `--hp-victus`, `--hp-wmi-readonly-test`, elevation, and a `SafeReadOnlyInvocation` catalog entry.
- Returned byte count is `4` if firmware accepts the read.
- Decode succeeds or fails safely.
- Unknown bytes remain summarized only.

## Still Forbidden

- Fan speed control, fan writes, `SetFanMode`, `SetFanLevel`, `SetFanMax`, ambiguous `0x37`, polling, retries, heartbeat, EC access, BIOS writes, hardware writes, performance mode control, battery control, and ASUS behavior changes.
