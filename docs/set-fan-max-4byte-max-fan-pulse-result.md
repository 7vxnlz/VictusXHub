# SetFanMax Four-Byte Max Fan Pulse Result

## Command

The manually run developer-only pulse was:

```powershell
dotnet run --project app\VictusX.csproj -- --hp-victus --hp-wmi-readonly-test --hp-fan-max-pulse --i-understand-this-can-affect-fans --i-approve-4-byte-max-fan-pulse --physical-fan-response-observed=true --restore-observed=true --manual-observation-notes="Fan ramped for about 2 minutes; airflow increased; restore observed; no abnormal noise, overheating, freeze, or crash."
```

## Recorded Result

- Four-byte enable `01-00-00-00` and matching restore `00-00-00-00` were both attempted and succeeded.
- Post-enable FanMaxGet was `false` with raw FanGetLevel `34-00`.
- Post-restore FanMaxGet was `false` with raw FanGetLevel `34-00`.
- Physical response and restore were observed; no unsafe abort was observed.
- `Outcome=Unknown`, `BlockedReasons=[]`, `WriteExecuted=true`, `FirstWriteGateSatisfied=false`, and `DeviceValidatedInputLength=null`.
- Classification: `CommandSucceededPhysicalResponseObservedReadbackInconclusive`.

## Interpretation

The separately gated four-byte Max Fan Pulse is operational for this exact-device developer experiment: command success, physical fan response, and restore observation were recorded. FanMaxGet remains inconclusive and cannot certify the enabled latch by itself.

This validates only the bounded developer pulse path. It does not fully validate the payload ABI for normal control, set `DeviceValidatedInputLength`, or permit normal/user-facing fan control. One byte remains comparison-only. The Diagnostic UI remains read-only and normal fan control remains **NO-GO**.

## Recommended Next Safe Step

The [pulse history/status view design](set-fan-max-pulse-history-status-view-design.md) is now implemented as a read-only local-log summary in the Diagnostic dashboard. It has no pulse execution or user-facing control route.

The [HP fan control research abstraction design](hp-fan-control-research-abstraction-design.md) defines how the developer-only pulse path may later be organized internally without becoming a normal fan-control API.

The [contract-refactor runtime verification](set-fan-max-pulse-contract-refactor-verification.md) confirms that the same gated four-byte pulse behavior remained operational after the internal contract wiring.
