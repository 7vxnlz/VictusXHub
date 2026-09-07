# HP Read-Only Telemetry Status

## Current Successful Read-Only Commands

- `SystemDesignData`: succeeded and decoded a 128-byte capability payload.
- `FanGetCount`: succeeded and decoded `FanCount=2`.
- `FanMaxGet`: succeeded and decoded max fan disabled.
- `FanGetLevel`: succeeded and decoded raw-only fan level bytes.

## What Each Command Proves

- `SystemDesignData` proves the guarded HP WMI read-only path can return model capability data and that this device declares software fan control support.
- `FanGetCount` proves the guarded fan status path can return a decodable fan-count payload.
- `FanMaxGet` proves the guarded fan status path can read the current max-fan latch/state.
- `FanGetLevel` proves the guarded fan status path can return a 128-byte raw fan-level payload.

## Current Decoded HP Victus Facts

- Shipping adapter rating: `280 W`.
- Thermal policy version: `1`.
- Platform feature flags: `1`.
- Declares software fan control support: `true`.
- Declares extreme mode support: `false`.
- PL4 default value: `190`.
- GPU mode switch raw value: `6`.
- Default CPU power limit with GPU: `30 W`.
- Fan count: `2`.
- Fan protection status: clear.
- Max fan state: disabled.
- Fan raw level prefix: `Fan1RawValue=23`, `Fan2RawValue=0`, `KnownPrefixHex=17-00`.

## What Remains Unknown

- Actual fan RPM values.
- Whether `FanGetLevel` raw values map to percent, firmware levels, curves, or anything else.
- Fan curve/table layout.
- Safe write payloads, bounds, restore behavior, retries, or firmware side effects.
- Whether any write-capable fan, thermal, power, or BIOS command is safe.

## What Remains Forbidden

Fan control, fan writes, fan speed control, interpreting raw fan level bytes as RPM/percent/curve/control level, performance mode control, EC access, BIOS writes, hardware writes, polling loops, retries, and ASUS behavior changes remain forbidden.

## Risk Status Before FanGetRpm

Risk remains moderate even for additional reads. The existing commands were successful only under explicit HP developer flags, elevated manual execution, command catalog allow-listing, and report-safe decoding. `FanGetRpm` must be treated as read-only status only and must use the same gates before any future invocation.

## Recommended Next Step

Investigate `FanGetRpm` as read-only status only. Do not implement fan control yet.
