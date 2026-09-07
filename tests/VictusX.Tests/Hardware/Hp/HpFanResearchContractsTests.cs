using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpFanResearchContractsTests
{
    [Fact]
    public void FourBytePulse_IsTheOnlyResearchOperationKind()
    {
        Assert.Equal([HpFanResearchOperationKind.FourByteMaxFanPulse], Enum.GetValues<HpFanResearchOperationKind>());

        HpFanResearchOperationDescriptor operation = HpFanResearchOperationDescriptor.FourByteMaxFanPulse;
        Assert.Equal(HpFanResearchOperationStatus.DeveloperOnlyResearch, operation.Status);
        Assert.Null(operation.DeviceValidatedInputLength);
    }

    [Fact]
    public void FourBytePulseContract_ContainsOnlyTheFixedPulsePair()
    {
        IHpFanMaxPulseResearchOperation operation = new FourByteMaxFanPulseResearchOperation();

        Assert.Equal("01-00-00-00", operation.EnablePayloadHex);
        Assert.Equal("00-00-00-00", operation.RestorePayloadHex);
        Assert.Equal(new byte[] { 0x01, 0x00, 0x00, 0x00 }, operation.EnablePayload.ToArray());
        Assert.Equal(new byte[] { 0x00, 0x00, 0x00, 0x00 }, operation.RestorePayload.ToArray());
        Assert.Null(operation.Descriptor.DeviceValidatedInputLength);
    }

    [Fact]
    public void BlockedGate_IsFailClosedAndCanBeLoggedWithoutAuthorization()
    {
        HpFanResearchGateResult gate = HpFanResearchGateResult.Blocked("Missing explicit developer approval.");
        var request = new HpFanResearchAppendOnlyLogRequest(
            HpFanResearchOperationDescriptor.FourByteMaxFanPulse,
            gate,
            null,
            new HpFanResearchCommandResult(false, false, "Blocked before enable."),
            new HpFanResearchRestoreResult(false, false, "Restore was not needed."),
            new HpFanResearchOutcome(
                HpFanMaxExperimentalOutcomeClassification.BlockedBeforeWrite,
                HpFanMaxExperimentReadbackReliability.Unknown));

        Assert.False(request.Gate.IsAllowed);
        Assert.False(request.Enable.Attempted);
        Assert.Null(request.Operation.DeviceValidatedInputLength);
    }

    [Fact]
    public void Contracts_DoNotExposeGenericFanControlOrUiMembers()
    {
        Type[] types =
        [
            typeof(IHpFanResearchOperation),
            typeof(IHpFanMaxPulseResearchOperation),
            typeof(FourByteMaxFanPulseResearchOperation),
            typeof(HpFanResearchAppendOnlyLogRequest)
        ];
        string[] forbiddenNames =
        [
            "SetFanSpeed",
            "SetFanCurve",
            "SetPerformanceMode",
            "SetFanMode",
            "SetFanLevel",
            "Slider",
            "Toggle",
            "Button"
        ];

        IEnumerable<string> memberNames = types.SelectMany(type =>
            type.GetMembers().Select(member => member.Name));

        Assert.DoesNotContain(memberNames, name =>
            forbiddenNames.Any(forbidden => name.Contains(forbidden, StringComparison.OrdinalIgnoreCase)));
    }
}
