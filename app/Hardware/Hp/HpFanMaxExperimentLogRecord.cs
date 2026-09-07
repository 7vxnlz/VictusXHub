namespace GHelper.Hardware.Hp;

public enum HpFanMaxExperimentPayloadLengthCandidate
{
    OneByteHypothesis = 1,
    FourByteHypothesis = 4
}

public enum HpFanMaxExperimentOutcome
{
    Unknown,
    Pass,
    Fail
}

public enum HpFanMaxExperimentReadbackReliability
{
    Unknown,
    Inconclusive,
    Reliable
}

public enum HpFanMaxExperimentalOutcomeClassification
{
    Unknown,
    BlockedBeforeWrite,
    CommandSucceededPhysicalResponseObservedReadbackInconclusive,
    CommandSucceededNoPhysicalConfirmation,
    CommandFailed,
    RestoreFailed,
    UnsafeAbort
}

public sealed record HpFanMaxExperimentLogRecord
{
    public const string GateStatus = "NO-GO";

    public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;
    public string? Model { get; init; }
    public string? Sku { get; init; }
    public string? BiosVersion { get; init; }
    public int? ThermalPolicyVersion { get; init; }
    public HpFanMaxExperimentPayloadLengthCandidate? PayloadLengthCandidate { get; init; }
    public string? PayloadBytesHypothesis { get; init; }
    public string? DeveloperOnlyOperation { get; init; }
    public int? RequestedHoldSeconds { get; init; }
    // Keep the original stored field so older logs and readers retain the same value.
    public int? RequestedPreRestoreWaitSeconds => RequestedHoldSeconds;
    public string? HoldDurationSemantics => DeveloperOnlyOperation == HpFanMaxHoldCommand.DeveloperOnlyOperationName
        ? HpFanMaxHoldCommand.DurationSemantics
        : null;
    public string Command { get; init; } = "0x20008";
    public string CommandType { get; init; } = "0x27";
    public string WmiClass { get; init; } = "hpqBIntM";
    public string WmiMethod { get; init; } = "hpqBIOSInt0";
    public int? BaselineFanGetCount { get; init; }
    public bool? BaselineFanMaxGet { get; init; }
    public string? BaselineFanGetLevelRaw { get; init; }
    public bool BaselineCapturePerformed { get; init; }
    public string BaselineCaptureResult { get; init; } = "Not captured.";
    public string[] BaselineReadOnlyProbeSummary { get; init; } = [];
    public string? EnableResult { get; init; }
    public bool? EnableCommandSucceeded { get; init; }
    public bool? PostEnableFanMaxGet { get; init; }
    public bool? FanMaxGetConfirmedEnable { get; init; }
    public string? PostEnableFanGetLevelRaw { get; init; }
    public string? RestoreResult { get; init; }
    public bool? RestoreCommandSucceeded { get; init; }
    public bool? PostRestoreFanMaxGet { get; init; }
    public string? PostRestoreFanGetLevelRaw { get; init; }
    public bool? PhysicalFanResponseObserved { get; init; }
    public bool? RestoreObserved { get; init; }
    public bool UnsafeAbortObserved { get; init; }
    public HpFanMaxExperimentReadbackReliability ReadbackReliability { get; init; } = HpFanMaxExperimentReadbackReliability.Unknown;
    public string? ManualObservationNotes { get; init; }
    public HpFanMaxExperimentOutcome Outcome { get; init; } = HpFanMaxExperimentOutcome.Unknown;
    public string[] BlockedReasons { get; init; } = [];

    public bool WriteExecuted { get; internal init; }
    public bool FirstWriteGateSatisfied => false;
    public int? DeviceValidatedInputLength => null;
    public HpFanMaxExperimentalOutcomeClassification ExperimentalOutcomeClassification =>
        HpFanMaxExperimentOutcomeClassifier.Classify(this);

    public static HpFanMaxExperimentLogRecord CreateBlocked(
        HpFanMaxExperimentPayloadLengthCandidate? payloadLengthCandidate = null,
        string? payloadBytesHypothesis = null)
    {
        return new HpFanMaxExperimentLogRecord
        {
            PayloadLengthCandidate = payloadLengthCandidate,
            PayloadBytesHypothesis = payloadBytesHypothesis,
            BlockedReasons =
            [
                "SetFanMax first-write gate is NO-GO.",
                "DeviceValidatedInputLength is unset.",
                "No SetFanMax write was attempted for this record."
            ]
        };
    }
}
