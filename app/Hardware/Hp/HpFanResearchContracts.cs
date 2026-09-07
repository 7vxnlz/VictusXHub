namespace GHelper.Hardware.Hp;

public enum HpFanResearchOperationKind
{
    FourByteMaxFanPulse
}

public enum HpFanResearchOperationStatus
{
    Blocked,
    DeveloperOnlyResearch
}

public sealed record HpFanResearchOperationDescriptor(
    HpFanResearchOperationKind Kind,
    HpFanResearchOperationStatus Status,
    int? DeviceValidatedInputLength)
{
    public static HpFanResearchOperationDescriptor FourByteMaxFanPulse { get; } = new(
        HpFanResearchOperationKind.FourByteMaxFanPulse,
        HpFanResearchOperationStatus.DeveloperOnlyResearch,
        null);
}

public sealed record HpFanResearchGateResult(
    bool IsAllowed,
    IReadOnlyList<string> Reasons)
{
    public static HpFanResearchGateResult Blocked(params string[] reasons) => new(false, reasons);
}

public sealed record HpFanResearchBaselineSnapshot(
    bool CaptureSucceeded,
    int? FanGetCount,
    bool? FanMaxGetEnabled,
    string? FanGetLevelRaw);

public sealed record HpFanResearchCommandResult(
    bool Attempted,
    bool Succeeded,
    string? Detail);

public sealed record HpFanResearchRestoreResult(
    bool Attempted,
    bool Succeeded,
    string? Detail);

public sealed record HpFanResearchOutcome(
    HpFanMaxExperimentalOutcomeClassification Classification,
    HpFanMaxExperimentReadbackReliability ReadbackReliability);

public sealed record HpFanResearchAppendOnlyLogRequest(
    HpFanResearchOperationDescriptor Operation,
    HpFanResearchGateResult Gate,
    HpFanResearchBaselineSnapshot? Baseline,
    HpFanResearchCommandResult Enable,
    HpFanResearchRestoreResult Restore,
    HpFanResearchOutcome Outcome);

public sealed record HpFanResearchAppendOnlyLogResult(
    bool Appended,
    string? Path,
    string? Error);

public interface IHpFanResearchOperation
{
    HpFanResearchOperationDescriptor Descriptor { get; }
}

public interface IHpFanMaxPulseResearchOperation : IHpFanResearchOperation
{
    ReadOnlyMemory<byte> EnablePayload { get; }
    ReadOnlyMemory<byte> RestorePayload { get; }
    string EnablePayloadHex { get; }
    string RestorePayloadHex { get; }
}

public sealed class FourByteMaxFanPulseResearchOperation : IHpFanMaxPulseResearchOperation
{
    private static readonly byte[] EnablePayloadBytes = [0x01, 0x00, 0x00, 0x00];
    private static readonly byte[] RestorePayloadBytes = [0x00, 0x00, 0x00, 0x00];

    public HpFanResearchOperationDescriptor Descriptor => HpFanResearchOperationDescriptor.FourByteMaxFanPulse;
    public ReadOnlyMemory<byte> EnablePayload => EnablePayloadBytes;
    public ReadOnlyMemory<byte> RestorePayload => RestorePayloadBytes;
    public string EnablePayloadHex => "01-00-00-00";
    public string RestorePayloadHex => "00-00-00-00";
}
