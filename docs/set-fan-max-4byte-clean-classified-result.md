# SetFanMax Four-Byte Clean Classified Result

## Command

The manually run, separately approved developer experiment used:

```powershell
dotnet run --project app\VictusX.csproj -- --hp-victus --hp-wmi-readonly-test --hp-fan-write-experiment --set-fan-max-payload-length=4 --i-understand-this-can-affect-fans --i-approve-one-time-set-fan-max-4-byte-experiment --i-approve-second-set-fan-max-4-byte-confirmation --physical-fan-response-observed=true --restore-observed=true --manual-observation-notes="Fan ramped for about 2 minutes; airflow increased; restore observed; no abnormal noise, overheating, freeze, or crash."
```

## Recorded Result

- Four-byte enable `01-00-00-00` and matching restore `00-00-00-00` both returned success.
- Post-enable FanMaxGet was `false`; post-enable raw FanGetLevel was `33-00`.
- Post-restore FanMaxGet was `false`; post-restore raw FanGetLevel was `32-00`.
- Physical fan response and restore were both explicitly observed; no unsafe abort was observed.
- `ReadbackReliability=Inconclusive`, `WriteExecuted=true`, `FirstWriteGateSatisfied=false`, and `DeviceValidatedInputLength=null`.
- `Outcome=Unknown`, `BlockedReasons=[]`, and `ExperimentalOutcomeClassification=CommandSucceededPhysicalResponseObservedReadbackInconclusive`.

## Interpretation

`Unknown` with no blocked reasons is correct here: command success, manual physical response, and observed restore are recorded, but FanMaxGet cannot independently establish the enabled state on this BIOS. The classifier, not legacy Outcome, provides the precise experimental interpretation.

This repeated response keeps four byte as the preferred developer-experiment payload only. It does not validate the payload ABI for normal use, set `DeviceValidatedInputLength`, or permit normal/user-facing fan control. FanMaxGet remains inconclusive and the normal UI remains **NO-GO**.

The [Max Fan Pulse result](set-fan-max-4byte-max-fan-pulse-result.md) provides a separate operational record for the same bounded developer-only four-byte path.

## Recommended Next Safe Step

The [four-byte Max Fan Pulse design](set-fan-max-4byte-max-fan-pulse-design.md) now defines a separately approved, command-line-only proposal with matching restore and manual observation logging. It remains design-only and separate from normal fan control.
