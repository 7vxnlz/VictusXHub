# Second SetFanMax Four-Byte Confirmation Experiment

## Purpose And Boundary

The first exact-device four-byte attempt returned success for enable and restore and produced an observed fan ramp, but FanMaxGet stayed `false`. A second, separately authorized four-byte-only confirmation is justified to determine whether that behavior repeats under measured conditions. It is not payload validation, does not update `DeviceValidatedInputLength`, and does not enable normal fan control.

One byte remains blocked. A second payload shape would confound the unresolved four-byte readback, restore, and observation contract.

## Exact Future Command

Use only this command after separate authorization:

```powershell
dotnet run --project app\VictusX.csproj -- --hp-victus --hp-wmi-readonly-test --hp-fan-write-experiment --set-fan-max-payload-length=4 --i-understand-this-can-affect-fans --i-approve-one-time-set-fan-max-4-byte-experiment --i-approve-second-set-fan-max-4-byte-confirmation
```

Both four-byte approvals are required: the original one-time approval scopes the initial four-byte experiment path, while `--i-approve-second-set-fan-max-4-byte-confirmation` scopes this confirmation only. The second flag cannot approve `=1`, validate a payload length, change `DeviceValidatedInputLength`, add a retry, or expose a UI feature.

Do not add flags, retry, substitute `=1`, or use the command as a UI feature.

## Required Pre-Test Conditions

- AC connected and reported online; battery state recorded.
- Laptop on a stable, ventilated surface; vents unobstructed.
- No game, benchmark, update, render, or other heavy workload; record notable background activity.
- Elevated process and exact target identity: HP Victus, SKU `7Z5Z2EA#AB8`, BIOS `F.31`, ThermalPolicyVersion `1`.
- Successful same-session SystemDesignData, FanGetCount, FanMaxGet, and raw FanGetLevel baseline.
- Baseline FanGetCount=`2`, FanMaxGet=`false`, and raw FanGetLevel recorded.
- Recovery route, stop authority, and an independent observer or written observation method ready before enable.

## Required Manual Observation Record

Record baseline, enable, and restore timestamps plus these fields:

- Fan audible ramp: yes/no; approximate delay and duration.
- Airflow increased: yes/no/unknown.
- Fan stopped after restore: yes/no/unknown; approximate delay.
- Any abnormal sound: description or none.
- Temperature: source, baseline, peak/trend, and any spike.
- AC/battery state at baseline, enable, and restore.
- UI/app responsiveness and any freeze.
- Windows crash, event, suspend, unplug, or process-cleanup observation.
- FanMaxGet and raw FanGetLevel after enable and after restore, without interpreting raw values as RPM or percent.

## Classification

- **Strong success:** one four-byte enable and matching restore return success; the same documented fan/airflow response repeats; fan stops after restore; final disabled baseline is observed; thermal/power/UI checks remain normal; no abort occurs. FanMaxGet may remain false, but the discrepancy must be explicitly recorded and independently reviewed.
- **Partial success:** enable/restore return success and an observed fan response occurs, but FanMaxGet stays false or observation/thermal/restore evidence is incomplete. This preserves NO-GO.
- **Fail:** a gate, WMI return, matching restore, or required observation fails; no expected fan response is observed; or final state is uncertain.
- **Unsafe/abort:** abnormal noise, rapid temperature rise, AC/power transition, UI loss, Windows crash/event, unexpected persistent fan behavior, or any unavailable restore path. Stop immediately; do not retry or switch payloads.

If FanMaxGet is false again while physical behavior repeats, treat it as an unresolved telemetry conflict, not as proof of either failure or a max-fan latch. Record it as partial success and retain the exact WMI/readback/manual timeline for review.

## Evidence Needed For A Limited Developer-Only GO Review

Only after two consistent four-byte records may a separate review consider a limited developer-only GO. The records must show exact identity, flags, AC/battery state, successful baseline, one enable and matching restore each, repeatable fan/airflow timing, safe thermal/UI observations, final disabled behavior, recovery evidence, and an explained or independently corroborated FanMaxGet discrepancy. This is still not normal UI evidence and does not automatically validate the input length.

## Recommended Next Implementation Step

The confirmation was completed and recorded in the [second four-byte result](set-fan-max-second-4byte-confirmation-result.md). Do not change hardware behavior now. The only safe follow-up is report/log-only experimental status metadata that distinguishes an observed physical response from validated normal control.
