namespace GHelper.Hardware.Hp;

public enum HpFanWriteAbortReason
{
    BlockedByDefault,
    CommandNotSetFanMax,
    MissingRequiredRuntimeFlag,
    AdministratorRequired,
    InteractiveHumanConfirmationRequired,
    ApprovedDeviceBaselineRequired,
    HealthyReadOnlyBaselineRequired,
    StableAcPowerRequired,
    IndependentThermalObservationRequired,
    PreWriteReadbackRequired,
    CurrentMaxFanStateUnknown,
    BaselineMaxFanMustBeDisabled,
    DeviceValidatedInputLengthRequired,
    DeviceValidatedInputLengthInvalid,
    EnablePayloadDescriptionRequired,
    RestorePayloadDescriptionRequired,
    WriteTargetStateRequired,
    InitialWriteMustEnableMaxFan,
    PostWriteReadbackRequired,
    RestorePlanRequired,
    SingleWriteAttemptRequired,
    UnexpectedExecutionRequest
}

public sealed record HpFanWritePreflightResult
{
    public bool IsAllowed { get; init; } = false;
    public IReadOnlyList<HpFanWriteAbortReason> AbortReasons { get; init; } =
    [HpFanWriteAbortReason.BlockedByDefault];
}
