# HP Hardware Scope

- Treat all HP WMI, firmware, fan, power, and thermal behavior as safety-sensitive.
- Keep read-only providers separate from write-capable transports. Missing, stale, ambiguous, or invalid telemetry fails closed to Unavailable/Unknown.
- Never label `FanGetLevel` as RPM, percent, PWM, or control state. `FanMaxGet` is inconclusive on the target.
- Keep `DeviceValidatedInputLength` null. Close-device or reference behavior is not exact-device validation.
- Normal fan control, SetFanLevel execution, SetFanMode, command type `0x37`, EC writes, PawnIO/PwnIO/WinRing0 fallback, retries, payload fallback, curves, and background reassertion remain blocked.
- Existing SetFanMax pulse/hold behavior may change only when explicitly requested: exact target gates, four-byte payload, one attempt, bounded wait, `finally` restore, append-only logging, no UI route.
- Tests must use pure models/fakes and must not invoke `hpqBIOSInt*` or hardware.
- Start with `docs/context-packs/fan-research.md` or `docs/context-packs/telemetry.md`, not the whole directory.
