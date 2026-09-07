namespace GHelper.Hardware.Hp;

public sealed record HpFanMaxExperimentBaselineCaptureData(
    string? Model,
    string? Sku,
    string? BiosVersion,
    int? ThermalPolicyVersion,
    int? FanGetCount,
    bool? FanMaxGetEnabled,
    string? FanGetLevelRaw,
    string[] ReadOnlyProbeSummary);

public static class HpFanMaxExperimentBaselineCaptureMapper
{
    public static HpFanMaxExperimentLogRecord CreateBlocked(
        HpFanMaxExperimentBaselineCaptureCommandResult command,
        params string[] additionalBlockedReasons)
    {
        ArgumentNullException.ThrowIfNull(command);

        return Create(command, null, false, "Baseline capture was not started. No WMI or hardware action was attempted.", additionalBlockedReasons);
    }

    public static HpFanMaxExperimentLogRecord CreateCaptured(
        HpFanMaxExperimentBaselineCaptureCommandResult command,
        HpFanMaxExperimentBaselineCaptureData data)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(data);

        return Create(
            command,
            data,
            true,
            "Approved read-only baseline capture completed. No SetFanMax write or other hardware write was attempted.",
            []);
    }

    private static HpFanMaxExperimentLogRecord Create(
        HpFanMaxExperimentBaselineCaptureCommandResult command,
        HpFanMaxExperimentBaselineCaptureData? data,
        bool baselineCapturePerformed,
        string baselineCaptureResult,
        IEnumerable<string> additionalBlockedReasons)
    {
        return HpFanMaxExperimentLogRecord.CreateBlocked(
            command.PayloadLengthCandidate,
            command.PayloadBytesHypothesis) with
        {
            Model = data?.Model,
            Sku = data?.Sku,
            BiosVersion = data?.BiosVersion,
            ThermalPolicyVersion = data?.ThermalPolicyVersion,
            BaselineFanGetCount = data?.FanGetCount,
            BaselineFanMaxGet = data?.FanMaxGetEnabled,
            BaselineFanGetLevelRaw = data?.FanGetLevelRaw,
            BaselineCapturePerformed = baselineCapturePerformed,
            BaselineCaptureResult = baselineCaptureResult,
            BaselineReadOnlyProbeSummary = data?.ReadOnlyProbeSummary ?? [],
            EnableResult = "Not attempted: baseline capture does not execute SetFanMax or any hardware write.",
            RestoreResult = "Not attempted: baseline capture does not execute SetFanMax or any hardware write.",
            ManualObservationNotes = "Developer-only read-only baseline capture. No fan write was attempted.",
            BlockedReasons =
            [
                .. HpFanMaxExperimentLogRecord.CreateBlocked().BlockedReasons,
                .. command.ValidationReasons,
                .. additionalBlockedReasons
            ]
        };
    }
}
