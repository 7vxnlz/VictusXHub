# HP Fan Read-Only Command Map

## Proven Read-Only Fan Commands

| Command | Status | Notes |
| --- | --- | --- |
| `FanGetCount` / `0x10` | Proven on real HP Victus | Returned a 4-byte payload and decoded `FanCount=2` with protection status clear. |
| `FanMaxGet` / `0x26` | Proven on real HP Victus | Returned a 4-byte payload and decoded max fan disabled. |

## Prepared But Raw-Only Commands

| Command | Status | Notes |
| --- | --- | --- |
| `FanGetLevel` / `0x2D` | Proven read-only response, raw-only interpretation | Returned 128 bytes. Only raw prefix bytes are preserved; values must not be interpreted as RPM, percent, curve, or control level. |

## Deferred Commands

| Command | Reason |
| --- | --- |
| `FanGetRpm` / `0x38` | Deferred for this `ThermalPolicyVersion=1` Victus because references describe it as V2/OMEN Max-specific and warn it can return garbage or phantom RPM values on V0/V1 systems. |
| Fan RPM `0x11` path | Mentioned by reference material as a separate standard RPM path, but not yet investigated or approved for this Victus. |

## Ambiguous Commands

| Command | Risk |
| --- | --- |
| `GetFanType` / `0x2C` or `44` | Read-shaped, but command numbering and command-family usage need a focused investigation before any probe preparation. |
| `GetFanTable` / `0x2F` | Read-shaped, but likely exposes table/curve data with unclear semantics and higher over-interpretation risk. |
| `FanGetLevelV2` / `0x37` | Ambiguous because references also associate `0x37` with write-like power-limit behavior. |

## Forbidden Write/Control Commands

- `SetFanMode` / `0x1A`
- `SetFanLevel` / `0x2E`
- `SetFanMax` / `0x27`
- Any EC fallback or direct EC fan path
- Any polling loop, retry loop, heartbeat, restore behavior, or UI control for fan writes

## Real HP Victus Results So Far

- `SystemDesignData`: succeeded and decoded `ThermalPolicyVersion=1`, `DeclaresSoftwareFanControlSupport=true`, `ShippingAdapterPowerRatingWatts=280`, and `DefaultCpuPowerLimitWithGpuWatts=30`.
- `FanGetCount`: succeeded and decoded `FanCount=2`.
- `FanMaxGet`: succeeded and decoded max fan disabled.
- `FanGetLevel`: succeeded and decoded raw-only values: `Fan1RawValue=23`, `Fan2RawValue=0`, unknown tail all zero.

## Safe To Read Next

No additional command should be invoked yet. The safest next research target is `GetFanType` / `0x2C` or `44` as documentation-only investigation, because fan type/capability metadata would be less semantic than fan tables and less V2-specific than `FanGetRpm`.

## Must Not Be Implemented Yet

Do not implement fan control, fan writes, fan speed control, RPM interpretation for raw level bytes, fan curves, EC access, BIOS writes, hardware writes, polling loops, retries, or ASUS behavior changes.

## Recommended Next Concrete Task

Investigate `GetFanType` / `0x2C` or `44` as read-only documentation only. Decide whether it is safe enough to prepare later, but do not invoke it and do not add it to the app command path yet.
