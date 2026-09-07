# FanGetCount Real Device Result

## What Was Tested

An elevated manual HP BIOS WMI read-only invocation of `FanGetCount` was run on the real HP Victus device.

## Required Flags And Elevation

The successful test required `--hp-victus`, `--hp-wmi-readonly-test`, and an elevated Administrator process.

## Successful Result Summary

- `FanGetCountInvocationAllowed`: `true`
- `FanGetCountInvocationAttempted`: `true`
- `FanGetCountInvocationSucceeded`: `true`
- `FanGetCountReturnedByteCount`: `4`
- `FanGetCountInvocationError`: empty
- `FanGetCountDecodeSucceeded`: `true`
- `FanGetCountDecodeErrors`: `[]`

## Decoded Fan Count

`FanCount`: `2`

## Protection Status Result

- `ProtectionStatusRaw`: `0`
- `OverCurrentProtectionTripped`: `false`
- `OverTemperatureProtectionTripped`: `false`

## What This Proves

The HP WMI provider accepted the explicitly gated `FanGetCount` read-only probe and returned a decodable 4-byte status payload reporting two fans.

## What It Does Not Prove Yet

This does not prove fan speed readings, fan level readings, manual fan control, performance mode control, firmware safety behavior, or any write-capable command support.

## Unknown Tail

- `UnknownByteCount`: `2`
- `UnknownByteRange`: `2..3`
- `UnknownNonZeroByteCount`: `0`

The unknown tail is safe and boring here because it is present only as summarized byte counts/range, and all unknown bytes were zero.

## Still Forbidden

Fan control, fan writes, fan speed control, performance mode control, EC access, BIOS writes, hardware writes, polling loops, retries, and ASUS behavior changes remain forbidden.

## Recommended Next Safe Step

Investigate `FanGetRpm` or `FanGetLevel` as read-only status only before considering any fan control work.
