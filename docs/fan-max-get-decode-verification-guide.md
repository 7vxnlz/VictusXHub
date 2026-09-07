# FanMaxGet Decode Verification Guide

## Later Manual Command

Run from an elevated Administrator terminal only after explicit approval:

```powershell
dotnet run --project app\VictusX.csproj -- --hp-victus --hp-wmi-readonly-test
```

Do not run this command from Codex.

## Report Fields To Copy Back

- `FanMaxGetInvocationAllowed`
- `FanMaxGetInvocationAttempted`
- `FanMaxGetInvocationSucceeded`
- `FanMaxGetReturnedByteCount`
- `FanMaxGetInvocationError`
- `FanMaxGetDecodeSucceeded`
- `FanMaxGetDecodeErrors`
- `FanMaxGetDecoded`

## Expected Safe Result

- Invocation is attempted only with `--hp-victus`, `--hp-wmi-readonly-test`, elevation, and a `SafeReadOnlyInvocation` catalog entry.
- Returned byte count is `4` if firmware accepts the read.
- Decode succeeds or fails safely.
- Unknown bytes remain summarized only.
- Max-fan state is reported as status only, with no control behavior.

## Still Forbidden

Fan control, fan writes, fan speed control, `FanMaxSet` / `0x27`, `SetFanLevel`, `SetFanMode`, ambiguous `0x37`, performance mode control, EC access, BIOS writes, hardware writes, polling loops, retries, and ASUS behavior changes remain forbidden.
