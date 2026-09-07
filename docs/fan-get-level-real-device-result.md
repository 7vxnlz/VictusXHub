# FanGetLevel Real Device Result

## What Was Tested

An elevated manual HP BIOS WMI read-only invocation of `FanGetLevel` (`CommandType 0x2D`) was run on the real HP Victus device.

## Required Flags And Elevation

The successful test required `--hp-victus`, `--hp-wmi-readonly-test`, and an elevated Administrator process.

## Successful Result Summary

- `FanGetLevelInvocationAllowed`: `true`
- `FanGetLevelInvocationAttempted`: `true`
- `FanGetLevelInvocationSucceeded`: `true`
- `FanGetLevelReturnedByteCount`: `128`
- `FanGetLevelInvocationError`: empty
- `FanGetLevelDecodeSucceeded`: `true`
- `FanGetLevelDecodeErrors`: `[]`

## Raw Decoded Values

- `Fan1RawValue`: `23`
- `Fan2RawValue`: `0`
- `RawValueBytes`: `FwA=`
- `KnownPrefixHex`: `17-00`
- `UnknownByteCount`: `126`
- `UnknownByteRange`: `2..127`
- `UnknownNonZeroByteCount`: `0`

## Why Values Must Remain Raw-Only

The decoded bytes are only confirmed as read-only returned values. They must not be interpreted as RPM, percent, fan curve points, firmware control levels, or safe write targets without separate reference evidence and additional read-only validation.

## What This Proves

The HP WMI provider accepted the explicitly gated `FanGetLevel` read-only probe and returned a decodable 128-byte payload with two preserved raw prefix bytes.

## What It Does Not Prove Yet

This does not prove fan RPM readings, percent readings, fan curve layout, manual fan control, firmware control semantics, safe write behavior, or any write-capable command support.

## Still Forbidden

Fan control, fan writes, fan speed control, interpretation of raw fan level bytes as RPM/percent/curve/control level, performance mode control, EC access, BIOS writes, hardware writes, polling loops, retries, and ASUS behavior changes remain forbidden.

## Recommended Next Safe Step

Investigate `FanGetRpm` as read-only status only, or pause fan reads and create a consolidated HP read-only telemetry status doc.
