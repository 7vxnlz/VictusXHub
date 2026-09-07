# First SetFanMax Four-Byte Experiment Result

## Command

The manually reviewed developer command was:

```powershell
dotnet run --project app\VictusX.csproj -- --hp-victus --hp-wmi-readonly-test --hp-fan-write-experiment --set-fan-max-payload-length=4 --i-understand-this-can-affect-fans --i-approve-one-time-set-fan-max-4-byte-experiment
```

## Exact-Device Result

Target: Victus by HP Gaming Laptop `16-s0xxx`, SKU `7Z5Z2EA#AB8`, BIOS `F.31`, ThermalPolicyVersion `1`.

- Enable payload `01-00-00-00`: attempted and reported success.
- Post-enable FanMaxGet: `false`.
- Post-enable raw FanGetLevel: `33-00`.
- Restore payload `00-00-00-00`: attempted and reported success.
- Post-restore FanMaxGet: `false`.
- Post-restore raw FanGetLevel: `33-00`.
- Runner outcome: `Fail`, because the required enabled FanMaxGet confirmation was absent.

The operator observed an audible/physical fan ramp after enable, followed later by the fan stopping. No normal fan-control UI was involved.

## Interpretation

The four-byte payload is the first exact-device write candidate with an observable fan response and successful WMI return values. This is partial success evidence only. FanMaxGet remained `false` before and after the observed response, so it cannot currently serve as the sole success readback for this device/BIOS. The raw FanGetLevel change from the prior `22-25` baseline to `33-00` is recorded but remains raw-only; it must not be interpreted as RPM, percentage, or a confirmed latch state.

The fan later stopping is consistent with the matching restore, firmware policy, or another unobserved state change. The available record cannot distinguish those explanations. Restore behavior, timing, thermal effect, and a reliable post-enable state signal remain unproven.

## Decision

- Four-byte is the only exact-device candidate with observed behavior and may be prioritized for a controlled confirmation experiment.
- Four-byte is **not** selected or fully validated.
- `DeviceValidatedInputLength` remains unset.
- Normal fan control UI and normal fan writes remain **NO-GO**.
- Do not test one-byte next: introducing a second payload shape would confound unresolved four-byte readback and restore evidence.

## Follow-Up

The [second four-byte confirmation result](set-fan-max-second-4byte-confirmation-result.md) repeated the observed physical response while FanMaxGet again stayed `false`. Together, the records make four byte the preferred experimental candidate only; they do not validate payload length or normal fan control.
