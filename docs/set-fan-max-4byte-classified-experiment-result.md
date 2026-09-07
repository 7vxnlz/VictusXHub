# SetFanMax Four-Byte Classified Experiment Result

## Recorded Result

The bounded developer-only four-byte experiment recorded `EnableCommandSucceeded=true`, `RestoreCommandSucceeded=true`, `PhysicalFanResponseObserved=true`, `RestoreObserved=true`, and `WriteExecuted=true`. The selected hypothesis was `01-00-00-00` enable with `00-00-00-00` restore for command `0x20008`, type `0x27`, through `hpqBIntM.hpqBIOSInt0`.

Post-enable and post-restore `FanMaxGet` were both `false`; raw FanGetLevel values were `33-00`. The operator recorded a roughly two-minute fan ramp, increased airflow, and no abnormal noise, overheating, freeze, or crash. `UnsafeAbortObserved=false`, `ReadbackReliability=Inconclusive`, and `DeviceValidatedInputLength=null`.

## Interpretation

This is `CommandSucceededPhysicalResponseObservedReadbackInconclusive`: command success, observed physical response, and observed restore are positive bounded-experiment evidence, while FanMaxGet cannot independently confirm the latch on this BIOS. The legacy `Outcome` is therefore normalized to `Unknown`, rather than a misleading plain `Fail`, and the FanMaxGet-only failure reason is removed from the log.

This does not validate the ABI, select an input length for normal control, or make FanMaxGet reliable. Four byte remains the preferred developer-experiment hypothesis only. Normal/user-facing fan control remains **NO-GO**, `DeviceValidatedInputLength` remains unset, and no UI control is permitted.

The later [clean classified result](set-fan-max-4byte-clean-classified-result.md) confirms the corrected `Outcome=Unknown` and empty blocked-reason behavior with the same readback-inconclusive classification.

## Recommended Next Safe Step

Define a separate, reviewed success and durable-restore criterion that does not rely solely on FanMaxGet before considering any normal-control decision.
