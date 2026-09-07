namespace GHelper.Hardware.Hp;

public enum HpFanMaxDeviceValidationStopReason
{
    NoDeviceValidatedInputLength,
    ConflictingDeviceValidatedInputLengths,
    SelectedLengthLacksReferenceEvidence,
    RestoreDisableEvidenceRequired,
    FanMaxGetReadbackUnavailable,
    BaselineFanMaxGetReadbackRequired,
    BaselineMaxFanStateMustBeDisabled,
    EnableVerificationReadbackPlanRequired,
    RestoreVerificationReadbackPlanRequired,
    HumanReferenceReviewRequired,
    HumanInputLengthConfirmationRequired,
    HumanRecoveryPlanConfirmationRequired
}

public sealed record HpFanMaxDeviceValidationEvidence
{
    public bool HasObservedOneByteReferenceEvidence { get; init; }
    public bool HasObservedFourByteReferenceEvidence { get; init; }
    public bool IsOneByteShapeDeviceValidated { get; init; }
    public bool IsFourByteShapeDeviceValidated { get; init; }
    public bool HasOneByteRestoreDisableEvidence { get; init; }
    public bool HasFourByteRestoreDisableEvidence { get; init; }
    public bool IsFanMaxGetReadbackAvailable { get; init; }
    public bool? BaselineMaxFanEnabled { get; init; }
    public bool HasEnableVerificationReadbackPlan { get; init; }
    public bool HasRestoreVerificationReadbackPlan { get; init; }
    public bool HasHumanReviewedReferenceEvidence { get; init; }
    public bool HasHumanConfirmedSelectedInputLength { get; init; }
    public bool HasHumanApprovedRecoveryPlan { get; init; }
}

public sealed record HpFanMaxDeviceValidationDecision
{
    public bool CanProceedToNextDesignStep { get; init; }
    public HpFanMaxValidatedInputLength? SelectedInputLength { get; init; }
    public IReadOnlyList<HpFanMaxDeviceValidationStopReason> StopReasons { get; init; } =
        [HpFanMaxDeviceValidationStopReason.NoDeviceValidatedInputLength];
    public bool IsWriteExecutionAllowed => false;
}
