# SetFanMax Four-Byte Max Fan Pulse Design

## Purpose And Scope

The proposed "Max Fan Pulse" is one bounded, developer-only experiment on the exact target HP Victus: SKU `7Z5Z2EA#AB8`, BIOS `F.31`, ThermalPolicyVersion `1`. It would test a single temporary max-fan enable followed by matching restore while collecting manual observations. It is not normal fan control, a persistent mode, a fan curve, or a user-facing feature.

## Payload And Flags

The only proposed payload pair is four byte: enable `01-00-00-00` and restore `00-00-00-00`. A future implementation would require all of:

- `--hp-victus`
- `--hp-wmi-readonly-test`
- `--hp-fan-max-pulse`
- `--i-understand-this-can-affect-fans`
- `--i-approve-4-byte-max-fan-pulse`
- `--physical-fan-response-observed=true|false`
- `--restore-observed=true|false`
- `--manual-observation-notes="..."`

The observation arguments are append-only log metadata. They cannot choose a payload, bypass a gate, or initiate hardware activity.

## Required Gates

- Exact HP Victus identity, SKU `7Z5Z2EA#AB8`, BIOS `F.31`, and ThermalPolicyVersion `1`.
- Elevated Administrator process and confirmed AC online state.
- Successful same-session read-only baseline capture.
- `FanGetCount=2` and baseline `FanMaxGet=false` as a safety precondition, despite the latter being an inconclusive enable readback.
- Explicit human acknowledgement and pulse-specific approval.

## Allowed Sequence

1. Capture the approved read-only baseline.
2. Make one four-byte enable attempt.
3. Wait one fixed, short interval.
4. Read FanMaxGet and raw FanGetLevel.
5. In `finally`, make one matching four-byte restore attempt.
6. Read FanMaxGet and raw FanGetLevel after restore.
7. Append one complete experiment log with manual observations.

No retry, payload fallback, or additional write is permitted.

## Classification And Boundaries

Command success with physical response and observed restore, while FanMaxGet remains `false`, is `CommandSucceededPhysicalResponseObservedReadbackInconclusive`. It is a limited developer-experiment success signal only, not normal-control validation. `DeviceValidatedInputLength` remains unset because the ABI and durable state semantics are not established.

The command must not add one-byte fallback, fan curves, sliders, background control, SetFanMode, SetFanLevel, `0x37`, EC writes, or normal UI exposure.

## Before Normal Control

Normal fan control still needs a selected and reviewed ABI, durable restore and recovery proof, a reliable success criterion beyond FanMaxGet, repeated thermal and power-transition evidence, and a separate product/UI decision. Until then normal/user-facing control remains **NO-GO**.

## Implementation Status

The separate command-line-only pulse parser and pure gate tests are implemented. It now carries the internal `FourByteMaxFanPulse` research operation contract into a pulse-specific runner overload. This preserves the existing flags, gates, four-byte pair, single attempt, matching restore, append-only logging, and absent Diagnostic UI/tray route. The [pulse result](set-fan-max-4byte-max-fan-pulse-result.md) records one successful bounded developer run with observed physical response and restore.
