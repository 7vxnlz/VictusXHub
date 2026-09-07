# FanMaxGet Real Device Result

## What Was Tested

An elevated manual HP BIOS WMI read-only invocation of `FanMaxGet` (`CommandType 0x26`) was run on the real HP Victus device.

## Required Flags And Elevation

The successful test required `--hp-victus`, `--hp-wmi-readonly-test`, and an elevated Administrator process.

## Successful Result Summary

- `FanMaxGetInvocationAllowed`: `true`
- `FanMaxGetInvocationAttempted`: `true`
- `FanMaxGetInvocationSucceeded`: `true`
- `FanMaxGetReturnedByteCount`: `4`
- `FanMaxGetInvocationError`: empty
- `FanMaxGetDecodeSucceeded`: `true`
- `FanMaxGetDecodeErrors`: `[]`

## Decoded Max Fan State

- `MaxFanStateRaw`: `0`
- `IsMaxFanEnabled`: `false`
- `UnknownByteCount`: `3`
- `UnknownByteRange`: `1..3`
- `UnknownNonZeroByteCount`: `0`

## What This Proves

The HP WMI provider accepted the explicitly gated `FanMaxGet` read-only status probe and returned a decodable 4-byte payload reporting max fan disabled.

## What It Does Not Prove Yet

This does not prove fan speed readings, fan level readings, RPM readback, fan curves, manual fan control, `FanMaxSet`, performance mode control, firmware safety behavior, or any write-capable command support.

## Still Forbidden

Fan control, fan writes, fan speed control, `FanMaxSet` / `0x27`, `SetFanLevel`, `SetFanMode`, ambiguous `0x37`, performance mode control, EC access, BIOS writes, hardware writes, polling loops, retries, and ASUS behavior changes remain forbidden.

## Recommended Next Safe Step

Investigate `FanGetLevel` or `FanGetRpm` as read-only status only before considering any fan control work.
