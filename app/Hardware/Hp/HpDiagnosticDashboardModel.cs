namespace GHelper.Hardware.Hp;

public enum HpDiagnosticDashboardStatus
{
    Normal,
    Ready,
    Warning,
    Blocked
}

public sealed record HpDiagnosticDashboardRow(
    string Label,
    string Value,
    HpDiagnosticDashboardStatus Status);

public sealed record HpDiagnosticDashboardSection(
    string Title,
    IReadOnlyList<HpDiagnosticDashboardRow> Rows);

public sealed record HpDiagnosticUserSummaryInput
{
    public string? Model { get; init; }
    public string? Sku { get; init; }
    public string? BiosVersion { get; init; }
    public string? HpVictusDetection { get; init; }
    public string? FanCount { get; init; }
    public string? ThermalPolicy { get; init; }
    public string? CpuLoad { get; init; }
    public string? GpuTemperature { get; init; }
    public string? BatteryPower { get; init; }
    public string? RefreshRate { get; init; }
    public string? CpuTemperature { get; init; }
    public string? FanRpm { get; init; }
    public string? GpuSwitchingCapability { get; init; }
    public string? KeyboardBacklightCapability { get; init; }
    public string? BatteryCareCapability { get; init; }
    public string? FanControlStatus { get; init; }
}

public sealed record HpDiagnosticDashboardHealthSummary(
    string DeviceStatus,
    string WmiStatus,
    string ReadOnlyTelemetryStatus,
    string FanReadOnlyStatus,
    string FanControlStatus);

public sealed record HpDiagnosticDashboardInput
{
    public string? ReportSchemaVersion { get; init; }
    public string? ReportGeneratedBy { get; init; }
    public string? ReportMode { get; init; }
    public string? ReportSource { get; init; }
    public string? ReportGeneratedAt { get; init; }
    public bool? IsHpVictusDetected { get; init; }
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }
    public string? Sku { get; init; }
    public string? BiosVersion { get; init; }
    public string? RootWmiReadiness { get; init; }
    public string? HpqBIntMReadiness { get; init; }
    public string? HpqBDataInReadiness { get; init; }
    public string? SystemDesignDataDecodeStatus { get; init; }
    public string? ThermalPolicyVersion { get; init; }
    public string? SoftwareFanControlSupport { get; init; }
    public string? FanCount { get; init; }
    public string? MaxFanState { get; init; }
    public string? Fan1RawLevel { get; init; }
    public string? Fan2RawLevel { get; init; }
    public string? SetFanMaxPulseHistoryStatus { get; init; }
    public string? SetFanMaxPulseTimestamp { get; init; }
    public string? SetFanMaxPulsePayload { get; init; }
    public string? SetFanMaxPulseWriteExecuted { get; init; }
    public string? SetFanMaxPulseEnableCommandSucceeded { get; init; }
    public string? SetFanMaxPulseRestoreCommandSucceeded { get; init; }
    public string? SetFanMaxPulsePhysicalFanResponseObserved { get; init; }
    public string? SetFanMaxPulseRestoreObserved { get; init; }
    public string? SetFanMaxPulseReadbackReliability { get; init; }
    public string? SetFanMaxPulseOutcomeClassification { get; init; }
    public string? SetFanMaxPulseNotesSummary { get; init; }
    public string? FanProofGapEvidenceSources { get; init; }
    public string? FanProofGapDeveloperPulseDecision { get; init; }
    public string? FanProofGapDeviceValidatedInputLengthDecision { get; init; }
    public string? FanProofGapFanMaxGetDecision { get; init; }
    public string? FanProofGapFanGetLevelDecision { get; init; }
    public string? FanProofGapRestoreVerificationDecision { get; init; }
    public string? FanProofGapRepeatabilityDecision { get; init; }
    public string? FanProofGapThermalPowerSafetyDecision { get; init; }
    public string? FanProofGapNormalFanControlDecision { get; init; }
    public string? SetFanMaxWriteImplemented { get; init; }
    public string? SetFanMaxWriteAllowed { get; init; }
    public string? SetFanMaxFirstWriteGateStatus { get; init; }
    public string? SetFanMaxFirstWriteGateSatisfied { get; init; }
    public string? SetFanMaxFirstWriteGateReason { get; init; }
    public string? SetFanMaxExperimentalPayloadCandidate { get; init; }
    public string? SetFanMaxPhysicalResponseObserved { get; init; }
    public string? SetFanMaxPhysicalResponseConfirmationCount { get; init; }
    public string? SetFanMaxReadbackReliable { get; init; }
    public string? SetFanMaxDeveloperExperimentAllowed { get; init; }
    public string? SetFanMaxDeveloperExperimentPayload { get; init; }
    public string? SetFanMaxNormalControlValidated { get; init; }
    public string? SetFanMaxUserFacingControlAllowed { get; init; }
    public string? SetFanMaxDeviceValidatedInputLength { get; init; }
    public string? SetFanMaxBlockedReason { get; init; }
    public string? SetFanMaxNextRequiredProof { get; init; }
}
