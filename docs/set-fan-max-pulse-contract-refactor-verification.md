# SetFanMax Pulse Contract Refactor Verification

## Command

The manually run developer-only verification was:

```powershell
dotnet run --project app\VictusX.csproj -- --hp-victus --hp-wmi-readonly-test --hp-fan-max-pulse --i-understand-this-can-affect-fans --i-approve-4-byte-max-fan-pulse --physical-fan-response-observed=true --restore-observed=true --manual-observation-notes="Contract-refactor verification pulse; fan ramped; airflow increased; restore observed; no abnormal noise, overheating, freeze, or crash."
```

## Recorded Result

- The `FourByteMaxFanPulse` contract supplied enable `01-00-00-00` and matching restore `00-00-00-00`.
- Enable and restore were attempted and succeeded.
- Post-enable FanMaxGet was `false` with raw FanGetLevel `32-00`; post-restore FanMaxGet was `false` with raw FanGetLevel `33-00`.
- Physical response and restore were observed. No unsafe abort, abnormal noise, overheating, freeze, or crash was reported.
- `Outcome=Unknown`, `BlockedReasons=[]`, `WriteExecuted=true`, `FirstWriteGateSatisfied=false`, and `DeviceValidatedInputLength=null`.
- Classification: `CommandSucceededPhysicalResponseObservedReadbackInconclusive` with `ReadbackReliability=Inconclusive`.

## Interpretation

This manually observed run confirms that the contract refactor preserved the bounded developer-only four-byte pulse behavior. It does not make FanMaxGet a reliable latch readback, fully validate the payload ABI for normal use, set `DeviceValidatedInputLength`, or permit normal fan control.

The HP Diagnostic UI remains read-only and normal/user-facing fan control remains **NO-GO**.

## Recommended Next Safe Task

Keep the contract path covered by pure regression tests and preserve the current explicit command-line gates. Any future work must remain internal research architecture or read-only evidence review, not user-facing fan control.
