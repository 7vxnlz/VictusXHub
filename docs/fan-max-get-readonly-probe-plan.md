# FanMaxGet Read-Only Probe Plan

## Why FanMaxGet Is Next

`FanMaxGet` (`CommandType 0x26`, `hpqBIOSInt4`) is the next safest fan read-only candidate after `FanGetCount`. References describe it as a max-fan status/latch query with four zero input bytes and a 4-byte response, separate from the forbidden `FanMaxSet` / `0x27` write command.

## Required Flags And Elevation

Any future real invocation must require `--hp-victus`, `--hp-wmi-readonly-test`, and an elevated Administrator process.

## Expected Safety Gates

- HP Victus mode enabled
- explicit read-only test mode enabled
- process elevated
- exact `FanMaxGet` command definition
- `SafeReadOnlyInvocation` catalog safety
- `ReadOnly` command access
- `0x26` command ID
- `hpqBIOSInt4` method and 4-byte output match
- single-shot invocation only
- no raw binary logging

## Expected Report Fields

- `FanMaxGetInvocationAllowed`
- `FanMaxGetInvocationAttempted`
- `FanMaxGetInvocationSucceeded`
- `FanMaxGetReturnedByteCount`
- `FanMaxGetInvocationError`
- `FanMaxGetDecodeSucceeded`
- `FanMaxGetDecodeErrors`
- `FanMaxGetDecoded.MaxFanStateRaw`
- `FanMaxGetDecoded.IsMaxFanEnabled`
- `FanMaxGetDecoded.UnknownByteCount`
- `FanMaxGetDecoded.UnknownByteRange`
- `FanMaxGetDecoded.UnknownNonZeroByteCount`

## Still Forbidden

Fan control, fan writes, fan speed control, `FanMaxSet` / `0x27`, `SetFanLevel`, `SetFanMode`, ambiguous `0x37`, performance mode control, EC access, BIOS writes, hardware writes, polling loops, retries, and ASUS behavior changes remain forbidden.

## Later Manual Test Step

Run this later from an elevated Administrator terminal only after explicit approval:

```powershell
dotnet run --project app\VictusX.csproj -- --hp-victus --hp-wmi-readonly-test
```

Do not run this command from Codex.
