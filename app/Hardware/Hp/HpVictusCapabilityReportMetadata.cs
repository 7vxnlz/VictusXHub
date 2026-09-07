namespace GHelper.Hardware.Hp;

public static class HpVictusCapabilityReportMetadata
{
    public const int SchemaVersion = 2;

    public static readonly string[] SetFanMaxSchemaV2ExperimentalStatusFields =
    [
        "SetFanMaxExperimentalPayloadCandidate",
        "SetFanMaxPhysicalResponseObserved",
        "SetFanMaxPhysicalResponseConfirmationCount",
        "SetFanMaxReadbackReliable",
        "SetFanMaxDeveloperExperimentAllowed",
        "SetFanMaxDeveloperExperimentPayload",
        "SetFanMaxNormalControlValidated",
        "SetFanMaxUserFacingControlAllowed"
    ];
}
