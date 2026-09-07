# Next Milestone Plan

This plan defines the next safe milestone after `v0.4.0-settings-and-logging-foundation`. It is a planning document only and does not add implementation code, HP WMI calls, fan control, telemetry loops, EC access, BIOS writes, UI features, or hardware write logic.

## Recommended Milestone

Milestone: `v0.5.0-static-capability-profile-loader`

Create a conservative local capability profile loader that maps known identity information to static, read-only capability decisions from bundled JSON/config resources. The loader should merge with the existing `DeviceIdentity` and `DeviceCapabilityProfile` contracts, but it must not perform HP WMI probing or assume that hardware control is supported.

## Why This Comes Next

`v0.4.0` added settings and privacy-aware logging. That gives the project enough foundation to introduce explainable capability decisions without hiding failures or leaking noisy machine data.

This milestone comes before the other candidates because:

- `v0.5.0-static-capability-profile-loader` improves safety and decision-making using local conservative data only.
- `v0.5.0-read-only-system-snapshot` will be useful soon, but it increases the amount of collected machine data before the app has a capability decision pipeline.
- `v0.5.0-app-startup-composition` should wait until there is something useful to compose beyond identity, settings, logging, and static capabilities.
- `v0.5.0-reference-command-inventory` should wait until the product has a clear internal capability vocabulary to compare against references.

The static loader is the best next step because it lets VictusControl represent known, unknown, unsupported, unavailable, and intentionally deferred capabilities before any live hardware probing exists.

## Files And Projects To Create Or Modify

Expected implementation scope:

- `src/VictusControl.Application/Capabilities/` for profile-loading contracts or orchestration if needed
- `src/VictusControl.Infrastructure/Capabilities/` for a JSON-backed static capability profile loader
- `src/VictusControl.Infrastructure/Capabilities/Profiles/` or an equivalent embedded/local resource location for conservative JSON profile data
- `src/VictusControl.Infrastructure/VictusControl.Infrastructure.csproj` only for embedded resources or necessary built-in configuration
- `tests/VictusControl.Application.Tests/` for merge/orchestration behavior if added
- `tests/VictusControl.Infrastructure.Tests/` for JSON parsing, matching, fallback, and conservative defaults
- `SESSION_STATE.md` after implementation verification
- `AI_CONTEXT.md` only if the architecture description changes meaningfully

The first profile may target the known HP Victus 16 identity only as an identity match, not as proof that fan control, thermal mode control, keyboard backlight control, telemetry, EC access, or BIOS writes are supported.

## Out Of Scope

The milestone must not include:

- HP WMI namespace enumeration, class probing, method calls, or command IDs
- fan control, fan curves, thermal profiles, keyboard lighting, EC access, BIOS writes, or telemetry loops
- runtime hardware probing beyond using the existing identity contract as input
- service/process conflict detection unless represented as static `Unknown` or `Unavailable` guidance
- UI settings screens, tray behavior, startup registration, update checks, packaging, or installers
- vendor DLLs, HP binaries, drivers, native interop, or copied reference source
- automatic log upload, analytics, crash reporting, or network activity
- reference repository modifications

## Tests To Add

Add focused tests with local JSON fixtures or in-memory streams:

- exact HP Victus identity can load a matching static profile
- unknown identity returns a safe unknown profile
- missing profile file or empty profile list returns safe unknown capabilities
- invalid/corrupt profile JSON fails safely and logs a sanitized warning if logging is wired
- profile matching uses manufacturer, family, model, SKU, and product name conservatively
- duplicate capability entries resolve deterministically
- no static profile marks fan control, thermal mode, telemetry, EC access, or BIOS writes as supported by default
- bundled profile data contains only approved capability kinds and statuses

Do not add tests that require HP hardware, administrator rights, WMI probing, fan state, BIOS state, EC access, network access, or application execution.

## Verification Commands

Run the standard verification after implementation:

```text
dotnet restore VictusControl.sln
dotnet build VictusControl.sln
dotnet test VictusControl.sln --no-build
```

Use built-in .NET JSON APIs. Do not add third-party configuration packages unless a specific limitation is documented first.

## Reference Repositories

Relevant references:

- `ib-3/ghelper-omen`: optional targeted reference for capability vocabulary only, if VictusControl terms are insufficient.
- `theantipopau/omencore`: optional targeted reference for conservative capability-status concepts and safety tests.

References not needed:

- `seerge/g-helper`: not needed unless generic app configuration questions arise.
- `breadeding/OmenSuperHub`: not needed for static profile loading.
- `MasonDye/OmenXHub`: not needed for static profile loading.
- `affaan-m/ECC`: not needed for product implementation.

Use VictusControl contracts first. Inspect reference repositories only for a specific capability-vocabulary question, record commit SHAs if used, and do not copy source or data.

## Risks

- Treating static profile presence as proof of hardware-control support.
- Overfitting profile matching to one exact model string.
- Adding too much schema before real discovery data exists.
- Mixing static decisions with live probing responsibilities.
- Accidentally storing private identifiers, serial numbers, or user-specific data in profile resources.
- Creating a profile loader that is hard to override when runtime probing later produces better evidence.

## Rollback Plan

Rollback should be simple:

- remove the static profile loader and bundled profile resources
- remove capability-loading contracts or orchestration added only for this milestone
- remove matching tests
- revert any project file resource changes
- keep `v0.4.0` settings/logging, `v0.3.0` identity provider, and `v0.2.0` contracts intact

No hardware state should be changed by this milestone, so no device restore action should be required.

## Recommended Model

Use `GPT-5.5 Medium`.

Reason: the work spans Application, Infrastructure, resources, and tests, but it remains read-only and deterministic. Escalate to `GPT-5.5 High` only if schema design or merge semantics become contentious.

## Recommended Commit Message

```text
feat: add static capability profile loader
```

## Preparing For Later HP WMI Capability Probing

This milestone prepares for later HP WMI capability probing without adding HP WMI by establishing:

- a deterministic capability-profile schema
- conservative matching between runtime identity and known profile data
- explicit reasons for unknown, unsupported, unavailable, or deferred capabilities
- a merge point where future live read-only probes can add evidence without enabling writes automatically
- tests proving that known Victus identity still does not imply fan, thermal, telemetry, EC, or BIOS support

Later HP WMI capability probing should extend this pipeline by adding read-only evidence. It must not replace the conservative defaults, and it must not introduce fan control, thermal writes, EC access, or BIOS writes in the same milestone.

## Immediately After This Milestone

After `v0.5.0-static-capability-profile-loader`, update `SESSION_STATE.md` with the loader, resource data, tests, and verification result.

The next planning decision should choose between:

- `v0.6.0-read-only-system-snapshot`
- `v0.6.0-app-startup-composition`
- `v0.6.0-reference-command-inventory`

Do not move to HP WMI writes, fan control, telemetry loops, EC access, BIOS writes, or UI controls until the read-only discovery pipeline can produce explainable capability evidence from both identity and conservative local profiles.
