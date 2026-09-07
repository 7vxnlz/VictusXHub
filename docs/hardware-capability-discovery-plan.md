# Hardware Capability Discovery Plan

This plan defines how VictusControl will discover supported hardware capabilities on the HP Victus Gaming Laptop 16-s0035nt / 7Z5Z2EA before any hardware control implementation exists. It is a planning document only: no HP WMI commands, EC writes, fan writes, BIOS writes, telemetry polling, or vendor binaries are introduced by this plan.

## Purpose

VictusControl needs a safe, repeatable way to identify what the target machine exposes at runtime. The first hardware-facing work should answer what the laptop reports and what control surfaces appear to exist, not attempt to control fans, thermal modes, lighting, or BIOS settings.

The discovery layer should produce a capability profile that later application and hardware modules can use to decide which features are available, unsupported, unknown, or intentionally disabled.

## Why Discovery Comes First

Fan control, HP WMI writes, EC access, and BIOS changes are safety-sensitive. Implementing writes before discovery risks assuming that Victus and OMEN machines expose identical interfaces, that a model string is enough to prove support, or that an unavailable command can fail harmlessly.

Discovery comes first because it creates evidence before behavior. It lets the project separate identity, capability probing, logging, and unsupported-device handling from later control code.

## Information To Collect

Collect only read-only machine and environment facts at first:

- system manufacturer, model, SKU, family, product name, and BIOS version
- Windows version, architecture, power platform role, and AC/DC power state
- battery presence, battery health fields available through Windows, and AC adapter state
- exposed WMI namespaces and classes relevant to HP system management
- available ACPI thermal zones and reported passive/critical trip data if exposed read-only
- visible plug-and-play device identifiers for HP system devices, embedded controller-adjacent devices, keyboard devices, and thermal devices
- running HP and OMEN service/process names that may own hardware control
- installed HP-related software packages where discoverable through safe Windows APIs
- sensor-provider availability, but without opening low-level write paths
- current process privileges and whether elevated permissions would be required for future operations

Do not collect secrets, user documents, serial numbers in logs intended for sharing, or high-cardinality machine data unless explicitly needed for diagnostics.

## Safe Read-Only Discovery Steps

1. Detect host identity through Windows APIs and WMI/CIM read operations.
2. Enumerate relevant WMI namespaces and class names without invoking mutating methods.
3. Query selected read-only WMI/CIM properties for system, BIOS, battery, thermal, and device inventory.
4. Enumerate HP/OMEN service and process presence without stopping, starting, or configuring anything.
5. Detect whether OMEN Gaming Hub or HP services appear active and record possible ownership conflicts.
6. Produce an in-memory capability profile with confidence levels and reasons.
7. Persist only sanitized discovery results if the user enables diagnostics export later.
8. Log unsupported, unknown, and unavailable capabilities as normal outcomes, not failures.

## Unsafe Or Deferred Actions

Do not perform these during discovery:

- EC writes or direct embedded controller access
- fan speed writes, fan curve writes, or thermal profile writes
- BIOS setting writes or WMI method calls that change state
- vendor DLL loading for hardware control
- HP proprietary binary redistribution
- driver installation, service installation, service restarts, or elevated repair actions
- disabling OMEN Gaming Hub, HP services, Windows services, or scheduled tasks
- assuming OMEN command behavior applies to Victus hardware without probing and verification

## Required Windows Data Sources

Use standard Windows read-only sources before any HP-specific probing:

- `Win32_ComputerSystem` for manufacturer, model, system type, and family-like fields where available
- `Win32_ComputerSystemProduct` for name, vendor, version, and SKU-like identifiers
- `Win32_BIOS` for BIOS vendor, version, release date, and SMBIOS version
- `Win32_OperatingSystem` for Windows version and architecture
- `Win32_Battery` and related battery classes for battery presence and safe status fields
- `Win32_PnPEntity` or SetupAPI-style enumeration for device IDs and HP-related devices
- `Win32_Service` for HP/OMEN service presence and status
- `Win32_Process` only for coarse process presence when conflict detection requires it
- ACPI thermal zone classes where exposed read-only by Windows
- Windows power status APIs for AC/DC state

## Possible HP-Specific Data Sources

HP-specific sources should be treated as optional and probed conservatively:

- HP WMI namespaces and classes exposed on the machine
- HP BIOS instrumentation namespaces, when present
- OMEN or HP system management classes, when present
- HP service presence as a signal that another owner may be controlling hardware
- HP software package presence as context, not proof of capability

A class or namespace being present is not enough to enable writes. It only increases confidence that a later, separately approved implementation can inspect documented or reverse-engineered behavior more closely.

## Runtime Detection

At runtime, VictusControl should detect:

- whether the machine is an HP system
- whether model, SKU, or product name matches the intended Victus target family
- whether required read-only WMI/CIM classes are available
- whether HP-specific namespaces/classes are present
- whether each planned feature is supported, unsupported, unknown, unavailable, or disabled due to conflict risk
- whether OMEN Gaming Hub or HP services appear to be active control owners
- whether the app is running with enough privileges for read-only discovery
- whether future write-capable features would require elevation or explicit user consent

Runtime probing should be preferred over hardcoded model assumptions. Model/SKU matching can raise confidence, but it must not bypass capability checks.

## Capability Profile Contents

A capability profile should store compact, explainable facts:

- profile schema version
- detected manufacturer, model, SKU, product name, and BIOS version
- operating system and architecture summary
- discovery timestamp
- per-capability status: supported, unsupported, unknown, unavailable, or disabled
- confidence level for each capability
- reason strings for each decision
- source of evidence, such as Windows WMI, HP WMI namespace presence, or service detection
- conflict indicators for OMEN Gaming Hub and HP services
- safety restrictions that affected decisions
- sanitized diagnostics suitable for issue reports

The profile should avoid raw dumps. It should be small enough to paste into a GitHub issue after review.

## Unknown Or Unsupported Hardware

Unknown hardware must default to observation-only behavior. VictusControl should continue to start, show that the device is unsupported or not fully identified, and provide sanitized diagnostics instructions where appropriate.

Unsupported or unknown capabilities should not be hidden behind silent failures. The app should explain which evidence was missing and should avoid offering controls that cannot be proven safe.

## Avoiding Conflicts With OMEN Gaming Hub And HP Services

VictusControl must not fight existing HP control software. During discovery, it may detect whether OMEN Gaming Hub or HP services are installed or running, but it must not stop, disable, reconfigure, or replace them.

If another service appears to own fan, thermal, lighting, or performance behavior, the capability profile should mark affected future write features as conflict-risk until a later design defines coexistence rules. Later control milestones should include restore-to-auto behavior and clear user warnings before any write-capable feature is enabled.

## Logging Requirements

Discovery logging should be structured, minimal, and privacy-aware:

- log discovery start and completion
- log each data source as available, unavailable, or failed read-only
- log per-capability status and reason
- log exceptions with type and safe message, not large raw dumps
- avoid serial numbers, usernames, full paths, and unrelated process details in shareable logs
- treat missing WMI classes as expected outcomes
- keep logs useful for GitHub issue triage without exposing sensitive machine data

## Safety Rules

- Start read-only.
- No EC writes.
- No fan writes.
- No BIOS setting writes.
- No vendor DLLs.
- No proprietary HP binaries.
- No driver installation or service manipulation.
- No assumption that OMEN and Victus behave identically.
- Prefer runtime capability probing over hardcoded model assumptions.
- Keep unsupported hardware on safe defaults.
- Require separate implementation approval before adding any write-capable hardware path.

## First Implementation Milestone After This Plan

Recommended next milestone: `v0.2.0-device-identity-and-capability-contracts`.

Scope:

- domain types for device identity and capability status
- hardware abstraction contracts for read-only capability discovery
- application service boundary for requesting a capability profile
- fake/test implementations only
- unit tests for unknown hardware, supported target identity, unsupported identity, and conflict-risk states

Out of scope for that milestone:

- HP WMI method calls
- concrete fan control
- telemetry polling
- BIOS writes
- EC access
- vendor binary loading
- UI controls beyond any minimal compile-only shell already present

## Verification Checklist

Before any future hardware-control work begins, verify that:

- discovery contracts can represent supported, unsupported, unknown, unavailable, and conflict-risk outcomes
- unknown hardware defaults to observation-only behavior
- target model/SKU detection does not bypass capability probing
- OMEN Gaming Hub and HP service presence can be represented as conflict signals
- no write-capable hardware API exists in the discovery milestone
- tests cover the target identity and unsupported-device paths
- logs are structured and sanitized
- no reference code, vendor binaries, HP DLLs, drivers, or proprietary assets were copied
- `dotnet build` and relevant tests pass after implementation milestones
