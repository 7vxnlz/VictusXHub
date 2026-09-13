using GHelper.Hardware.Hp;
using Xunit;

namespace VictusXHub.Tests.Hardware.Hp;

public sealed class HpKeyboardBacklightReadOnlyStatusTests
{
    [Theory]
    [InlineData(0x00, false, "Off")]
    [InlineData(0xE4, true, "On")]
    public void ExactTarget_WithProvenRawShape_DecodesBinaryCurrentState(byte byte0, bool expectedIsOn, string expectedState)
    {
        HpKeyboardBacklightStatus status = Resolve(CreateCapturedResult(byte0));

        Assert.Equal(HpKeyboardBacklightAvailability.SupportedStateAvailable, status.Availability);
        Assert.Equal(expectedIsOn, status.IsOn);
        Assert.Equal("Supported", status.SupportText);
        Assert.Equal(expectedState, status.CurrentStateText);
        Assert.Equal("Keyboard backlight: " + expectedState + " (Current startup KeyboardStatus).", status.EvidenceText);
    }

    [Theory]
    [InlineData(0x64)]
    [InlineData(0x01)]
    public void ExactTarget_WithUnrecognizedByte_FailsClosed(byte byte0)
    {
        HpKeyboardBacklightStatus status = Resolve(CreateCapturedResult(byte0));

        Assert.Equal(HpKeyboardBacklightAvailability.SupportedStateUnavailable, status.Availability);
        Assert.Null(status.IsOn);
        Assert.Equal("Unavailable", status.CurrentStateText);
    }

    [Fact]
    public void ExactTarget_WithNonZeroTail_FailsClosed()
    {
        byte[] bytes = new byte[128];
        bytes[0] = 0xE4;
        bytes[1] = 0x01;

        HpKeyboardBacklightStatus status = Resolve(CreateCapturedResult(bytes));

        Assert.Equal(HpKeyboardBacklightAvailability.SupportedStateUnavailable, status.Availability);
        Assert.Null(status.IsOn);
    }

    [Fact]
    public void AccessDenied_RetainsTransportReasonWithoutClaimingMalformedData()
    {
        var result = HpKeyboardStatusReadOnlyProbeResult.FromInvocation(
            HpWmiInvocationResult.Failed(GetStatusCommand(), "ManagementException: Access denied"));

        HpKeyboardBacklightStatus status = Resolve(result);

        Assert.Null(status.IsOn);
        Assert.Equal("Supported", status.SupportText);
        Assert.Contains("Current startup KeyboardStatus", status.EvidenceText);
        Assert.Contains("TransportUnavailable", status.EvidenceText);
        Assert.Contains("Access denied", status.EvidenceText);
        Assert.DoesNotContain("unrecognized raw", status.EvidenceText);
    }

    [Fact]
    public void ExactTarget_WithWrongLength_FailsClosed()
    {
        HpKeyboardStatusReadOnlyProbeResult malformed = HpKeyboardStatusReadOnlyProbeResult.FromInvocation(
            HpWmiInvocationResult.SuccessfulInvocation(GetStatusCommand(), new byte[127], 0));

        HpKeyboardBacklightStatus status = Resolve(malformed);

        Assert.Equal(HpKeyboardBacklightAvailability.SupportedStateUnavailable, status.Availability);
        Assert.Null(status.IsOn);
        Assert.Contains("InvalidOutputLength", status.EvidenceText);
        Assert.Contains("127", status.EvidenceText);
    }

    [Fact]
    public void ExactTarget_WithoutTheExpectedRawReturnCode_FailsClosed()
    {
        HpKeyboardStatusReadOnlyProbeResult result = HpKeyboardStatusReadOnlyProbeResult.FromInvocation(
            HpWmiInvocationResult.SuccessfulInvocation(GetStatusCommand(), new byte[128], null));

        HpKeyboardBacklightStatus status = Resolve(result);

        Assert.Equal(HpKeyboardBacklightAvailability.SupportedStateUnavailable, status.Availability);
        Assert.Null(status.IsOn);
    }

    [Fact]
    public void MismatchedExactDevice_IsUnavailableEvenWhenRawDataMatches()
    {
        HpKeyboardStatusReadOnlyProbeGateResult wrongGate = HpKeyboardStatusReadOnlyProbeGate.Evaluate(CreateDevice(sku: "OTHER#AB8"));
        HpKeyboardBacklightStatus status = HpKeyboardBacklightStatus.Resolve(wrongGate, CreateCapturedResult(0xE4));

        Assert.Equal(HpKeyboardBacklightAvailability.Unavailable, status.Availability);
        Assert.Null(status.IsOn);
    }

    [Fact]
    public void ReadOnlyStatusModel_HasNoWriteCapability()
    {
        string[] methodNames = typeof(HpKeyboardBacklightStatus).GetMethods()
            .Select(method => method.Name)
            .ToArray();

        Assert.DoesNotContain(methodNames, name => name.StartsWith("Set", StringComparison.Ordinal));
        Assert.DoesNotContain(methodNames, name => name.Contains("Write", StringComparison.Ordinal));
        Assert.DoesNotContain(methodNames, name => name.Contains("Invoke", StringComparison.Ordinal));
    }

    private static HpKeyboardBacklightStatus Resolve(HpKeyboardStatusReadOnlyProbeResult result) =>
        HpKeyboardBacklightStatus.Resolve(HpKeyboardStatusReadOnlyProbeGate.Evaluate(CreateDevice()), result);

    private static HpKeyboardStatusReadOnlyProbeResult CreateCapturedResult(byte byte0)
    {
        byte[] bytes = new byte[128];
        bytes[0] = byte0;
        return CreateCapturedResult(bytes);
    }

    private static HpKeyboardStatusReadOnlyProbeResult CreateCapturedResult(byte[] bytes) =>
        HpKeyboardStatusReadOnlyProbeResult.FromInvocation(
            HpWmiInvocationResult.SuccessfulInvocation(GetStatusCommand(), bytes, 0));

    private static HpKeyboardStatusReadOnlyProbeDevice CreateDevice(
        string sku = "7Z5Z2EA#AB8",
        string bios = "F.31",
        string board = "8BD4")
    {
        var identity = new HpRyzenTemperatureProbeIdentity(
            HpRyzenTemperatureProbeIdentitySource.Wmi,
            "HP",
            "Victus by HP Gaming Laptop 16-s0035nt",
            sku,
            bios,
            "test");
        var ryzen = new HpRyzenTemperatureProbeDevice(identity, "AMD Ryzen 5 7640HS", 0x19, 0x74, 0x1);
        return new HpKeyboardStatusReadOnlyProbeDevice(ryzen, board);
    }

    private static HpBiosWmiCommandDefinition GetStatusCommand() => Assert.Single(
        HpBiosWmiCommandCatalog.Definitions,
        definition => definition.Name == "KeyboardStatus");
}
