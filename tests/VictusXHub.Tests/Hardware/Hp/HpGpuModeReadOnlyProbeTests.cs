using GHelper.Hardware.Hp;
using Xunit;

namespace VictusXHub.Tests.Hardware.Hp;

public sealed class HpGpuModeReadOnlyProbeTests
{
    [Fact]
    public void Catalog_PinsTheOemGpuBiosSelectionReadContract()
    {
        HpBiosWmiCommandDefinition command = GetCommand();

        Assert.Equal(HpBiosWmiCommandFamily.Gpu, command.Family);
        Assert.Equal(0x01u, command.BiosCommand);
        Assert.Equal(0x52u, command.CommandId);
        Assert.Equal("hpqBIOSInt4", command.MethodName);
        Assert.Equal(0, command.ExpectedInputSize);
        Assert.Equal(4, command.ExpectedOutputSize);
        Assert.Equal(HpBiosWmiCommandAccess.ReadOnly, command.Access);
        Assert.Equal(HpBiosWmiCommandSafety.SafeReadOnlyInvocation, command.Safety);
        Assert.DoesNotContain(HpBiosWmiCommandCatalog.Definitions, definition =>
            definition.Name == "GpuBiosSelection" && definition.BiosCommand == 0x02);
    }

    [Fact]
    public void InvocationClient_PreservesZeroByteInputAndPinsTheGpuGetterAllowlist()
    {
        string client = ReadRepositoryFile("app", "Hardware", "Hp", "HpWmiInvocationClient.cs");

        Assert.Contains("definition.BiosCommand == 0x01", client, StringComparison.Ordinal);
        Assert.Contains("definition.CommandId == 0x52", client, StringComparison.Ordinal);
        Assert.Contains("definition.ExpectedInputSize == 0", client, StringComparison.Ordinal);
        Assert.Contains("string.Equals(definition.MethodName, \"hpqBIOSInt4\"", client, StringComparison.Ordinal);
        Assert.Contains("if (definition.ExpectedInputSize > 0)", client, StringComparison.Ordinal);
    }

    [Fact]
    public void ExactDeviceGate_AcceptsTheValidatedTarget()
    {
        Assert.True(HpGpuModeReadOnlyProbeGate.Evaluate(CreateDevice()).IsAccepted);
    }

    [Theory]
    [InlineData("OTHER#AB8", "F.31", "8BD4")]
    [InlineData("7Z5Z2EA#AB8", "F.30", "8BD4")]
    [InlineData("7Z5Z2EA#AB8", "F.31", "8BD5")]
    public void ExactDeviceGate_RejectsMismatchedIdentity(string sku, string bios, string board)
    {
        HpGpuModeReadOnlyProbeGateResult result = HpGpuModeReadOnlyProbeGate.Evaluate(CreateDevice(sku, bios, board));

        Assert.False(result.IsAccepted);
        Assert.Equal(HpGpuModeReadOnlyProbeAvailability.DeviceMismatch, result.Availability);
    }

    [Theory]
    [InlineData(0x00, 0x00, "Hybrid")]
    [InlineData(0x01, 0x01, "Discrete")]
    [InlineData(0x02, 0x02, "Optimus")]
    [InlineData(0x03, 0x03, "Uma")]
    [InlineData(0x81, 0x01, "Discrete")]
    public void Decoder_UsesTheOemMaskAndKnownMappings(byte raw, byte masked, string expected)
    {
        HpGpuModeReadOnlyProbeResult result = CreateCapturedResult(raw);

        Assert.True(result.IsCaptured);
        Assert.Equal(raw, result.RawByte0);
        Assert.Equal(masked, result.MaskedByte0);
        Assert.Equal(expected, result.SelectedBiosGraphicsConfiguration.ToString());
    }

    [Fact]
    public void Decoder_LeavesUnknownValueUnknown()
    {
        HpGpuModeReadOnlyProbeResult result = CreateCapturedResult(0x04);

        Assert.True(result.IsCaptured);
        Assert.Equal(HpSelectedBiosGraphicsConfiguration.Unknown, result.SelectedBiosGraphicsConfiguration);
        Assert.Equal("UMA", CreateCapturedResult(0x03).SelectedBiosGraphicsConfigurationText);
    }

    [Fact]
    public void Result_RejectsMalformedLengthAndMissingReturnCode()
    {
        HpGpuModeReadOnlyProbeResult malformed = HpGpuModeReadOnlyProbeResult.FromInvocation(
            HpWmiInvocationResult.SuccessfulInvocation(GetCommand(), new byte[3], 0));
        HpGpuModeReadOnlyProbeResult missingCode = HpGpuModeReadOnlyProbeResult.FromInvocation(
            HpWmiInvocationResult.SuccessfulInvocation(GetCommand(), new byte[4], null));

        Assert.Equal(HpGpuModeReadOnlyProbeAvailability.InvalidOutputLength, malformed.Availability);
        Assert.False(malformed.IsCaptured);
        Assert.Equal(3, malformed.RawData?.Length);
        Assert.Equal(HpSelectedBiosGraphicsConfiguration.Unknown, malformed.SelectedBiosGraphicsConfiguration);
        Assert.Equal(HpGpuModeReadOnlyProbeAvailability.InvalidReturnCode, missingCode.Availability);
        Assert.False(missingCode.IsCaptured);
        Assert.Equal(4, missingCode.RawData?.Length);
        Assert.Null(missingCode.MaskedByte0);
    }

    [Fact]
    public void Result_PreservesTransportFailureWithoutDecoding()
    {
        HpGpuModeReadOnlyProbeResult result = HpGpuModeReadOnlyProbeResult.FromInvocation(
            HpWmiInvocationResult.Failed(GetCommand(), "ManagementException: access denied"));

        Assert.Equal(HpGpuModeReadOnlyProbeAvailability.TransportUnavailable, result.Availability);
        Assert.True(result.Invoked);
        Assert.Null(result.RawData);
        Assert.Equal(HpSelectedBiosGraphicsConfiguration.Unknown, result.SelectedBiosGraphicsConfiguration);
        Assert.Contains("access denied", result.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void EvidenceWriter_EmitsDeterministicRawEvidenceAndSemanticBoundary()
    {
        HpGpuModeReadOnlyProbeResult result = CreateCapturedResult(0x81);
        string evidence = HpGpuModeReadOnlyProbeEvidenceWriter.Format(new(
            new DateTimeOffset(2026, 9, 14, 0, 0, 0, TimeSpan.Zero),
            HpGpuModeReadOnlyProbeGateResult.Accepted,
            result));

        Assert.Contains("TimestampUtc: 2026-09-14T00:00:00.0000000+00:00", evidence, StringComparison.Ordinal);
        Assert.Contains("IdentityGate: accepted", evidence, StringComparison.Ordinal);
        Assert.Contains("InvocationAttempted: true", evidence, StringComparison.Ordinal);
        Assert.Contains("RawReturnCode: 0x00000000", evidence, StringComparison.Ordinal);
        Assert.Contains("ReturnedDataLength: 4", evidence, StringComparison.Ordinal);
        Assert.Contains("RawByte0: 0x81", evidence, StringComparison.Ordinal);
        Assert.Contains("MaskedByte0: 0x01", evidence, StringComparison.Ordinal);
        Assert.Contains("SelectedBiosGraphicsConfiguration: Discrete", evidence, StringComparison.Ordinal);
        Assert.Contains("RawDataHex: 81000000", evidence, StringComparison.Ordinal);
        Assert.Contains("SemanticBoundary: BIOS-selected configuration; applied/current topology not yet proven", evidence, StringComparison.Ordinal);
    }

    [Fact]
    public void DeveloperRoute_IsExplicitSingleInvocationAndAbsentFromProductionStartup()
    {
        string program = ReadRepositoryFile("app", "Program.cs");
        string command = ReadRepositoryFile("app", "Hardware", "Hp", "HpGpuModeReadOnlyProbeCommand.cs");
        string capabilityProbe = ReadRepositoryFile("app", "Hardware", "Hp", "HpVictusCapabilityProbe.cs");

        Assert.Contains("HpGpuModeReadOnlyProbeCommand.TryRun(args)", program, StringComparison.Ordinal);
        Assert.True(program.IndexOf("HpGpuModeReadOnlyProbeCommand.TryRun(args)", StringComparison.Ordinal) <
                    program.IndexOf("MainCore(args)", StringComparison.Ordinal));
        Assert.Contains("--hp-victus --hp-gpu-mode-readonly-probe", command, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(command, ".TryInvoke("));
        Assert.DoesNotContain("GpuBiosSelection", capabilityProbe, StringComparison.Ordinal);
        Assert.DoesNotContain("BiosCommand == 0x02", command, StringComparison.Ordinal);
        Assert.DoesNotContain("SetGpu", command, StringComparison.OrdinalIgnoreCase);
    }

    private static HpGpuModeReadOnlyProbeDevice CreateDevice(
        string sku = "7Z5Z2EA#AB8",
        string bios = "F.31",
        string board = "8BD4")
    {
        var identity = new HpRyzenTemperatureProbeIdentity(
            HpRyzenTemperatureProbeIdentitySource.Wmi,
            "HP",
            "Victus by HP Gaming Laptop 16-s0xxx",
            sku,
            bios,
            "test");
        var ryzen = new HpRyzenTemperatureProbeDevice(identity, "AMD Ryzen 5 7640HS", 0x19, 0x74, 0x1);
        return new HpGpuModeReadOnlyProbeDevice(ryzen, board);
    }

    private static HpBiosWmiCommandDefinition GetCommand() => Assert.Single(
        HpBiosWmiCommandCatalog.Definitions,
        definition => definition.Name == "GpuBiosSelection");

    private static HpGpuModeReadOnlyProbeResult CreateCapturedResult(byte rawByte0)
    {
        byte[] bytes = [rawByte0, 0, 0, 0];
        return HpGpuModeReadOnlyProbeResult.FromInvocation(
            HpWmiInvocationResult.SuccessfulInvocation(GetCommand(), bytes, 0));
    }

    private static string ReadRepositoryFile(params string[] segments) =>
        File.ReadAllText(Path.Combine([FindRepositoryRoot(), .. segments]));

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "VictusXHub.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("VictusXHub repository root was not found.");
    }

    private static int CountOccurrences(string text, string value)
    {
        int count = 0;
        int offset = 0;
        while ((offset = text.IndexOf(value, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += value.Length;
        }

        return count;
    }
}
