namespace GHelper.Hardware.Hp;

public enum HpFanMaxTargetState
{
    EnableMaxFan,
    RestoreDisableMaxFan
}

public sealed record HpFanMaxWriteExperimentPlan
{
    public const string CommandName = "SetFanMax";
    public const uint CommandId = 0x27;

    public HpFanMaxTargetState? TargetState { get; init; }
    public HpFanMaxTargetState? RestoreTargetState { get; init; }
    public HpFanMaxValidatedInputLength? DeviceValidatedInputLength { get; init; }
    public HpFanMaxPayloadDescription? EnablePayloadDescription { get; init; }
    public HpFanMaxPayloadDescription? RestorePayloadDescription { get; init; }
    public bool RequiresReadbackBeforeWrite => true;
    public bool RequiresReadbackAfterWrite => true;
    public bool RequiresVerifiedRestore => true;
    public bool IsWriteExecutionAllowed => false;
}
