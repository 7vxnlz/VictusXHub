namespace GHelper.Hardware.Hp;

public sealed record HpFanProofGapAnalysis(
    string EvidenceSources,
    string DeveloperPulseDecision,
    string DeviceValidatedInputLengthDecision,
    string FanMaxGetDecision,
    string FanGetLevelDecision,
    string RestoreVerificationDecision,
    string RepeatabilityDecision,
    string ThermalPowerSafetyDecision,
    string NormalFanControlDecision,
    int ValidExperimentLogCount,
    int InvalidExperimentLogCount);

public static class HpFanProofGapAnalyzer
{
    public const string DeveloperPulseOperational = "Operational under explicit developer gates - four-byte Max Fan Pulse only";
    public const string DeviceValidatedInputLengthUnset = "Unset / not validated - local evidence cannot approve an input length";
    public const string FanMaxGetInconclusive = "Inconclusive - false after enable cannot confirm a max-fan latch";
    public const string FanGetLevelRawOnly = "Raw-only - not RPM, percent, or a control state";
    public const string RestoreVerificationPartial = "Partial developer evidence only - durable restore behavior is not proven";
    public const string RepeatabilityPartial = "Partial bounded evidence only - normal-control session repeatability is not proven";
    public const string ThermalPowerSafetyMissing = "Missing - thermal and power-state safety proof is not sufficient";
    public const string NormalFanControlNoGo = "NO-GO - normal/user-facing fan control remains disabled";

    public static HpFanProofGapAnalysis Analyze(string experimentLogDirectory, HpDiagnosticReportLoadResult? cachedReport)
    {
        HpFanMaxPulseHistoryEntriesLoadResult logs = HpFanMaxPulseHistoryLoader.LoadAll(experimentLogDirectory);
        bool hasFanMaxGetFalseAfterEnable = logs.Entries.Any(entry =>
            entry.WriteExecuted == true &&
            entry.EnableCommandSucceeded == true &&
            entry.PostEnableFanMaxGet == false);

        return new HpFanProofGapAnalysis(
            BuildEvidenceSources(logs, cachedReport),
            DeveloperPulseOperational,
            DeviceValidatedInputLengthUnset,
            hasFanMaxGetFalseAfterEnable ? FanMaxGetInconclusive : "Inconclusive - no sufficient valid post-enable readback evidence",
            FanGetLevelRawOnly,
            RestoreVerificationPartial,
            RepeatabilityPartial,
            ThermalPowerSafetyMissing,
            NormalFanControlNoGo,
            logs.Entries.Count,
            logs.InvalidLogCount);
    }

    private static string BuildEvidenceSources(HpFanMaxPulseHistoryEntriesLoadResult logs, HpDiagnosticReportLoadResult? cachedReport)
    {
        string reportStatus = cachedReport?.Status switch
        {
            HpDiagnosticReportLoadStatus.Loaded => "cached capability report loaded locally",
            HpDiagnosticReportLoadStatus.CouldNotBeRead => "cached capability report invalid/unreadable",
            _ => "cached capability report unavailable"
        };

        return "Local experiment logs: " + logs.Entries.Count + " valid, " + logs.InvalidLogCount +
            " invalid ignored; " + reportStatus + ". No WMI or hardware action was performed.";
    }
}
