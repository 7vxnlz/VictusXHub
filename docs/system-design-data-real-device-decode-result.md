# SystemDesignData Real Device Decode Result

## What Was Tested

An elevated manual `SystemDesignData` read-only invocation was run on the real HP Victus device, then the returned bytes were decoded into the report-safe fields.

## Successful Decode Summary

- `SystemDesignDataInvocationSucceeded`: `true`
- `SystemDesignDataReturnedByteCount`: `128`
- `SystemDesignDataDecodeSucceeded`: `true`
- `SystemDesignDataDecodeErrors`: `[]`
- `IsValid`: `true`

## Important Decoded Fields

- `ShippingAdapterPowerRatingWatts`: `280`
- `ThermalPolicyVersion`: `1`
- `PlatformFeatureFlags`: `1`
- `DeclaresSoftwareFanControlSupport`: `true`
- `DeclaresExtremeModeSupport`: `false`
- `Pl4DefaultValue`: `190`
- `GpuModeSwitchRaw`: `6`
- `DefaultCpuPowerLimitWithGpuWatts`: `30`
- `UnknownByteCount`: `116`
- `UnknownByteRange`: `12..127`
- `UnknownNonZeroByteCount`: `0`

## Software Fan Control Flag

`DeclaresSoftwareFanControlSupport=true` means the decoded read-only `SystemDesignData` flags claim software fan control support is present on this device.

## What It Does Not Mean Yet

It does not prove safe fan-control commands, fan curves, write payloads, restore behavior, bounds, or UI behavior. It is only a read-only capability clue.

## Unknown Tail

The unknown tail is safe and boring here because all 116 unknown bytes were zero. The decoder keeps the tail summarized by range and non-zero count only.

## Still Forbidden

- Do not invoke WMI from Codex.
- Do not run `--hp-wmi-readonly-test` from Codex.
- Do not add fan, performance, battery, RGB, keyboard lighting, GPU, EC, BIOS write, or hardware write behavior.

## Recommended Next Safe Step

Investigate read-only fan capability/status commands before any fan control work.
