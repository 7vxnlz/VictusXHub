namespace GHelper.Hardware.Hp;

public static class HpFanMaxExperimentRunLogMapper
{
    public static HpFanMaxExperimentLogRecord Create(
        HpFanMaxExperimentRunResult result,
        HpFanMaxExperimentManualObservation? manualObservation = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        HpFanMaxExperimentBaseline? baseline = result.Baseline;
        bool writeAttempted = result.EnableWrite.Attempted || result.RestoreWrite.Attempted;
        HpFanMaxExperimentLogRecord record = new()
        {
            PayloadLengthCandidate = result.Payload?.Candidate,
            PayloadBytesHypothesis = result.Payload?.EnableBytesHex,
            Model = baseline?.Model,
            Sku = baseline?.Sku,
            BiosVersion = baseline?.BiosVersion,
            ThermalPolicyVersion = baseline?.ThermalPolicyVersion,
            BaselineFanGetCount = baseline?.FanGetCount,
            BaselineFanMaxGet = baseline?.FanMaxGetEnabled,
            BaselineFanGetLevelRaw = baseline?.FanGetLevelRaw,
            BaselineCapturePerformed = result.BaselineCapturePerformed,
            BaselineCaptureResult = result.BaselineCapturePerformed ? "Approved read-only baseline capture completed." : "Baseline capture was blocked before WMI or hardware action.",
            BaselineReadOnlyProbeSummary = baseline?.ReadOnlyProbeSummary ?? [],
            EnableResult = FormatResult(result.EnableWrite),
            EnableCommandSucceeded = result.EnableWrite.Attempted ? result.EnableWrite.Succeeded : null,
            PostEnableFanMaxGet = result.PostEnableReadback?.FanMaxGetEnabled,
            FanMaxGetConfirmedEnable = result.PostEnableReadback?.FanMaxGetEnabled is bool maxFanEnabled
                ? maxFanEnabled
                : null,
            PostEnableFanGetLevelRaw = result.PostEnableReadback?.FanGetLevelRaw,
            RestoreResult = FormatResult(result.RestoreWrite),
            RestoreCommandSucceeded = result.RestoreWrite.Attempted ? result.RestoreWrite.Succeeded : null,
            PostRestoreFanMaxGet = result.PostRestoreReadback?.FanMaxGetEnabled,
            PostRestoreFanGetLevelRaw = result.PostRestoreReadback?.FanGetLevelRaw,
            PhysicalFanResponseObserved = manualObservation?.PhysicalFanResponseObserved,
            RestoreObserved = manualObservation?.RestoreObserved,
            UnsafeAbortObserved = false,
            ReadbackReliability = result.EnableWrite.Attempted && result.PostEnableReadback?.FanMaxGetEnabled == false
                ? HpFanMaxExperimentReadbackReliability.Inconclusive
                : HpFanMaxExperimentReadbackReliability.Unknown,
            ManualObservationNotes = manualObservation?.ManualObservationNotes ?? "No manual observations were supplied; physical response and restore state remain unknown. DeviceValidatedInputLength remains unset.",
            Outcome = result.Outcome,
            BlockedReasons = result.BlockedReasons,
            WriteExecuted = writeAttempted
        };

        return record.ExperimentalOutcomeClassification == HpFanMaxExperimentalOutcomeClassification.CommandSucceededPhysicalResponseObservedReadbackInconclusive
            ? record with
            {
                Outcome = HpFanMaxExperimentOutcome.Unknown,
                BlockedReasons = record.BlockedReasons
                    .Where(reason => !string.Equals(
                        reason,
                        "Enable result or post-enable FanMaxGet readback did not confirm max fan enabled.",
                        StringComparison.Ordinal))
                    .ToArray()
            }
            : record;
    }

    private static string FormatResult(HpFanMaxExperimentWriteResult result) =>
        result.Attempted
            ? result.Succeeded ? "Attempted and succeeded." : "Attempted and failed: " + (result.Error ?? "unknown error")
            : "Not attempted: " + (result.Error ?? "blocked");
}
