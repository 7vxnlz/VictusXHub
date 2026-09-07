# Second SetFanMax Four-Byte Confirmation Result

## Command

The separately approved developer-only command was:

```powershell
dotnet run --project app\VictusX.csproj -- --hp-victus --hp-wmi-readonly-test --hp-fan-write-experiment --set-fan-max-payload-length=4 --i-understand-this-can-affect-fans --i-approve-one-time-set-fan-max-4-byte-experiment --i-approve-second-set-fan-max-4-byte-confirmation
```

Target: Victus by HP Gaming Laptop `16-s0xxx`, SKU `7Z5Z2EA#AB8`, BIOS `F.31`, ThermalPolicyVersion `1`.

## Recorded Result

- Enable `01-00-00-00`: attempted and succeeded.
- Post-enable FanMaxGet: `false`.
- Post-enable raw FanGetLevel: `31-00`.
- Restore `00-00-00-00`: attempted and succeeded.
- Post-restore FanMaxGet: `false`.
- Post-restore raw FanGetLevel: `33-00`.
- Runner outcome: `Fail`, solely because FanMaxGet did not confirm an enabled state.
- `WriteExecuted=true`; `FirstWriteGateSatisfied=false`; `DeviceValidatedInputLength=null`.

The operator heard the fan ramp again, observed it run for about two minutes, and later stop. No abnormal noise, overheating, or crash was observed.

## Comparison And Interpretation

Like the [first four-byte result](set-fan-max-first-4byte-experiment-result.md), enable and restore returned success, an observable fan response followed, and FanMaxGet remained `false`. The repeated physical response makes the four-byte form the **preferred experimental payload only** for this exact device.

FanMaxGet is not a sufficient sole success criterion for this device/BIOS: it did not reflect either observed response. Its semantics are unresolved, rather than disproven. The raw FanGetLevel transitions (`33-00`, then `31-00`/`33-00`) are recorded as raw bytes only; they do not establish RPM, percent, a control level, or a max-fan latch.

Future logs preserve this combination as `CommandSucceededPhysicalResponseObservedReadbackInconclusive` when the physical response is manually recorded. For that classification, the legacy `Outcome` is normalized to `Unknown` and the FanMaxGet-only failure reason is removed; the classification remains experimental interpretation, not payload or normal-control validation. See the [classified four-byte result](set-fan-max-4byte-classified-experiment-result.md).

The [clean classified four-byte result](set-fan-max-4byte-clean-classified-result.md) records this corrected mapping with explicitly observed physical response and restore.

## Safety And Readiness

The two observations provide repeatable, limited developer-only physical-response evidence for the four-byte hypothesis. They do not fully validate its ABI, semantics, restore timing, failure recovery, or normal control behavior. `DeviceValidatedInputLength` remains unset; one byte remains blocked and must not be tested as a fallback.

Schema-v2 cached diagnostic reports identify this exact evidence as `SetFanMaxExperimentalPayloadCandidate=FourByte`, physical response observed twice, and readback not reliable. They also record `SetFanMaxDeveloperExperimentAllowed=true` and `SetFanMaxDeveloperExperimentPayload=FourByte`: this is a limited GO for further controlled, command-line-only developer experiments on this exact device. It neither validates the payload ABI nor changes `DeviceValidatedInputLength`. Normal-control validation and user-facing control remain explicitly `false`. Schema-v1 or missing cached reports must continue to render the physical-response and developer-experiment state as unknown/blocked.

Normal fan control UI remains **NO-GO**. User-facing control still requires a reliable state/readback contract, independently reviewed restore and recovery proof, bounded thermal/power evidence, repeated safety observations, and a separate product-level lifecycle/UI review.

## Recommended Next Safe Step

The [one-byte comparison result](set-fan-max-1byte-comparison-result.md) now records one similar exact-device physical response. The [payload strategy decision](set-fan-max-payload-strategy-decision.md) keeps four byte preferred for controlled confirmation, one byte comparison-only, and normal control blocked.
