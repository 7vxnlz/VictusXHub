using GHelper.Hardware.Hp;
using Xunit;

namespace VictusXHub.Tests.Hardware.Hp;

public sealed class HpKeyboardStatusReadOnlyProbeTests
{
    [Fact]
    public void Catalog_EncodesOnlyTheProvenKeyboardStatusRawCaptureContract()
    {
        HpBiosWmiCommandDefinition command = Assert.Single(
            HpBiosWmiCommandCatalog.Definitions,
            definition => definition.Name == "KeyboardStatus");

        Assert.Equal(HpBiosWmiCommandFamily.Keyboard, command.Family);
        Assert.Equal(0x20009u, command.BiosCommand);
        Assert.Equal(0x04u, command.CommandId);
        Assert.Equal("hpqBIOSInt128", command.MethodName);
        Assert.Equal(1, command.ExpectedInputSize);
        Assert.Equal(128, command.ExpectedOutputSize);
        Assert.Equal(HpBiosWmiCommandAccess.ReadOnly, command.Access);
        Assert.Equal(HpBiosWmiCommandSafety.SafeReadOnlyInvocation, command.Safety);
        Assert.DoesNotContain(HpBiosWmiCommandCatalog.Definitions, definition =>
            definition.Name == "KeyboardStatus" && (definition.CommandId == 0x05 || definition.MethodName == "hpqBIOSInt4"));
    }

    [Fact]
    public void InvocationClient_PreservesTheFixedSecuSerializationAndZeroPayload()
    {
        string client = ReadRepositoryFile("app", "Hardware", "Hp", "HpWmiInvocationClient.cs");

        Assert.Contains("private static readonly byte[] BiosSign = [0x53, 0x45, 0x43, 0x55];", client, StringComparison.Ordinal);
        Assert.Contains("inputData[\"Command\"] = definition.BiosCommand;", client, StringComparison.Ordinal);
        Assert.Contains("inputData[\"CommandType\"] = definition.CommandId;", client, StringComparison.Ordinal);
        Assert.Contains("inputData[\"Size\"] = (uint)definition.ExpectedInputSize;", client, StringComparison.Ordinal);
        Assert.Contains("inputData[BiosDataInputFieldName] = new byte[definition.ExpectedInputSize];", client, StringComparison.Ordinal);
    }

    [Fact]
    public void ExactDeviceGate_AcceptsOnlyTheValidatedSkuBiosBoardAndRyzenIdentity()
    {
        HpKeyboardStatusReadOnlyProbeGateResult result = HpKeyboardStatusReadOnlyProbeGate.Evaluate(CreateDevice());

        Assert.True(result.IsAccepted);
    }

    [Theory]
    [InlineData("OTHER#AB8", "F.31", "8BD4")]
    [InlineData("7Z5Z2EA#AB8", "F.30", "8BD4")]
    [InlineData("7Z5Z2EA#AB8", "F.31", "8BD5")]
    public void ExactDeviceGate_RejectsMismatchedIdentityEvidence(string sku, string bios, string board)
    {
        HpKeyboardStatusReadOnlyProbeDevice device = CreateDevice(sku, bios, board);

        HpKeyboardStatusReadOnlyProbeGateResult result = HpKeyboardStatusReadOnlyProbeGate.Evaluate(device);

        Assert.False(result.IsAccepted);
        Assert.Equal(HpKeyboardStatusReadOnlyProbeAvailability.DeviceMismatch, result.Availability);
    }

    [Fact]
    public void InvocationResult_RequiresExactly128RawBytes()
    {
        var valid = HpWmiInvocationResult.SuccessfulInvocation(GetStatusCommand(), new byte[128], 0);
        var malformed = HpWmiInvocationResult.SuccessfulInvocation(GetStatusCommand(), new byte[127], 0);

        Assert.True(HpKeyboardStatusReadOnlyProbeResult.FromInvocation(valid).IsCaptured);
        HpKeyboardStatusReadOnlyProbeResult malformedResult = HpKeyboardStatusReadOnlyProbeResult.FromInvocation(malformed);
        Assert.False(malformedResult.IsCaptured);
        Assert.Equal(HpKeyboardStatusReadOnlyProbeAvailability.InvalidOutputLength, malformedResult.Availability);
        Assert.Equal(127, malformedResult.ReturnedDataLength);
    }

    [Theory]
    [InlineData(0xE4)]
    [InlineData(0x64)]
    public void RawFormatter_PreservesCandidateByteWithoutStateInterpretation(byte firstByte)
    {
        byte[] bytes = new byte[128];
        bytes[0] = firstByte;
        HpKeyboardStatusReadOnlyProbeResult result = HpKeyboardStatusReadOnlyProbeResult.FromInvocation(
            HpWmiInvocationResult.SuccessfulInvocation(GetStatusCommand(), bytes, 0));

        string formatted = string.Join("\n", HpKeyboardStatusReadOnlyProbeFormatter.Format(result));

        Assert.Contains("Data[0] (raw hex): 0x" + firstByte.ToString("X2"), formatted, StringComparison.Ordinal);
        Assert.DoesNotContain("On", formatted, StringComparison.Ordinal);
        Assert.DoesNotContain("Off", formatted, StringComparison.Ordinal);
        Assert.DoesNotContain("brightness", formatted, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ProbeRoute_IsExplicitAndCannotRunFromNormalStartupOrDiagnosticPaths()
    {
        string program = ReadRepositoryFile("app", "Program.cs");
        string command = ReadRepositoryFile("app", "Hardware", "Hp", "HpKeyboardStatusReadOnlyProbeCommand.cs");
        string capabilityProbe = ReadRepositoryFile("app", "Hardware", "Hp", "HpVictusCapabilityProbe.cs");

        Assert.Contains("HpKeyboardStatusReadOnlyProbeCommand.TryRun(args)", program, StringComparison.Ordinal);
        Assert.True(program.IndexOf("HpKeyboardStatusReadOnlyProbeCommand.TryRun(args)", StringComparison.Ordinal) <
                    program.IndexOf("MainCore(args)", StringComparison.Ordinal));
        Assert.Contains("args.Length == 2", command, StringComparison.Ordinal);
        Assert.Contains("--hp-victus --hp-keyboard-status-readonly-probe", command, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(command, ".TryInvoke("));
        Assert.DoesNotContain("hpqBIOSInt4", command, StringComparison.Ordinal);
        Assert.DoesNotContain("0x05", command, StringComparison.Ordinal);
        Assert.DoesNotContain("KeyboardStatus", capabilityProbe, StringComparison.Ordinal);
    }

    private static HpKeyboardStatusReadOnlyProbeDevice CreateDevice(
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
        return new HpKeyboardStatusReadOnlyProbeDevice(ryzen, board);
    }

    private static HpBiosWmiCommandDefinition GetStatusCommand() => Assert.Single(
        HpBiosWmiCommandCatalog.Definitions,
        definition => definition.Name == "KeyboardStatus");

    private static string ReadRepositoryFile(params string[] segments)
    {
        string root = FindRepositoryRoot();
        return File.ReadAllText(Path.Combine([root, .. segments]));
    }

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
