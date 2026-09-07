namespace GHelper.Hardware.Hp;

public sealed record HpFanMaxWritePreflightRequest
{
    public string RequestedCommandName { get; init; } = string.Empty;
    public uint? RequestedCommandId { get; init; }
    public IReadOnlyList<string> PresentFlags { get; init; } = [];
    public bool IsAdministrator { get; init; } = false;
    public bool HasInteractiveHumanConfirmation { get; init; } = false;
    public bool HasApprovedDeviceBaseline { get; init; } = false;
    public bool HasHealthyReadOnlyBaseline { get; init; } = false;
    public bool HasStableAcPower { get; init; } = false;
    public bool HasIndependentThermalObservation { get; init; } = false;
    public bool HasSuccessfulPreReadFanMaxGet { get; init; } = false;
    public bool? CurrentMaxFanEnabled { get; init; }
    public bool HasPostWriteReadbackPlan { get; init; } = false;
    public bool HasRestoreReadbackPlan { get; init; } = false;
    public bool IsSingleWriteAttempt { get; init; } = false;
    public HpFanMaxWriteExperimentPlan Plan { get; init; } = new();
}
