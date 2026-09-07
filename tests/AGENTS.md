# Test Scope

- Keep tests deterministic, pure, and hardware-independent.
- Never invoke WMI, `hpqBIOSInt*`, fan commands, EC access, elevated operations, or external reference code.
- Use temporary directories for file-backed tests and clean them in the test lifecycle.
- Preserve fail-closed assertions: no UI control route, no fallback/retry, raw fan data stays raw, and `DeviceValidatedInputLength` stays null.
- Prefer focused tests beside the touched behavior; run the full solution suite for shared HP safety contracts.
- Do not loosen assertions merely to accept a behavior change.
