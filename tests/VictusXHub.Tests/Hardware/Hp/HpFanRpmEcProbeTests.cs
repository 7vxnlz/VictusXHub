using GHelper.Hardware.Hp;
using System.Reflection;
using Xunit;

namespace VictusXHub.Tests.Hardware.Hp;

public sealed class HpFanRpmEcProbeTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 9, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Exact8Bd4Target_IsAcceptedBeforePawnIoCanOpen()
    {
        HpFanRpmEcProbeGateResult result = HpFanRpmEcProbeGate.Evaluate(ExactTarget());

        Assert.True(result.IsAccepted);
        Assert.True(HpFanRpmEcProbeGate.MayOpenPawnIo(result));
    }

    [Fact]
    public void WrongSku_IsRejectedBeforePawnIoCanOpen()
    {
        HpFanRpmEcProbeDevice device = ExactTarget() with
        {
            RyzenDevice = ExactTarget().RyzenDevice with { Identity = ExactIdentity() with { Sku = "7Z5Z3EA#AB8" } }
        };

        HpFanRpmEcProbeGateResult result = HpFanRpmEcProbeGate.Evaluate(device);

        Assert.False(result.IsAccepted);
        Assert.False(HpFanRpmEcProbeGate.MayOpenPawnIo(result));
    }

    [Fact]
    public void WrongBoard_IsRejectedBeforePawnIoCanOpen()
    {
        HpFanRpmEcProbeGateResult result = HpFanRpmEcProbeGate.Evaluate(ExactTarget() with { BoardProduct = "8BD5" });

        Assert.Equal(HpFanRpmEcProbeAvailability.BoardMismatch, result.Availability);
        Assert.False(HpFanRpmEcProbeGate.MayOpenPawnIo(result));
    }

    [Fact]
    public void UnavailableBoard_DoesNotBroadenTheExactValidatedIdentityGate()
    {
        HpFanRpmEcProbeGateResult result = HpFanRpmEcProbeGate.Evaluate(ExactTarget() with { BoardProduct = string.Empty });

        Assert.True(result.IsAccepted);
    }

    [Fact]
    public void FixedProtocol_ContainsOnlyTheAuditedReadConstants()
    {
        Assert.Equal((byte)0x11, ReadConstant<byte>("Fan1Register"));
        Assert.Equal((byte)0x14, ReadConstant<byte>("Fan2Register"));
        Assert.Equal((byte)0x80, ReadConstant<byte>("AcpiEcReadCommand"));
        Assert.Equal((ushort)0x66, ReadConstant<ushort>("CommandPort"));
        Assert.Equal((ushort)0x62, ReadConstant<ushort>("DataPort"));

        string source = ReadRepositoryFile("app", "Hardware", "Hp", "HpFanRpmEcProbe.cs");
        Assert.DoesNotContain("0x81", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ReadEc(address", source, StringComparison.Ordinal);
        Assert.DoesNotContain("WriteEc(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SetFan", source, StringComparison.Ordinal);
        Assert.DoesNotContain("RyzenSMU", source, StringComparison.Ordinal);
        Assert.DoesNotContain("WinRing0", source, StringComparison.Ordinal);
        Assert.Contains("mutex?.ReleaseMutex();", source, StringComparison.Ordinal);
        Assert.Contains("handle.Dispose();", source, StringComparison.Ordinal);
        Assert.Contains("last status 0x", source, StringComparison.Ordinal);
    }

    [Fact]
    public void PublicTypedSurface_HasNoArbitraryAddressOrPortMethod()
    {
        MethodInfo[] publicMethods = typeof(HpFanRpmEcProbeBackend)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

        Assert.Empty(publicMethods);

        MethodInfo[] sessionMethods = typeof(IHpFanRpmEcProbeSession).GetMethods();
        Assert.Collection(sessionMethods, method =>
        {
            Assert.Equal("ReadFanRpmSnapshot", method.Name);
            Assert.Collection(method.GetParameters(), parameter => Assert.Equal(typeof(DateTimeOffset), parameter.ParameterType));
        });
    }

    [Fact]
    public void FixedRawBytes_DecodeWithTheOnlyCandidateScale()
    {
        HpFanRpmEcSnapshot snapshot = HpFanRpmEcProbeSemantics.FromFixedRawBytes(32, 61, Now);

        Assert.True(snapshot.IsAvailable);
        Assert.Equal(3200, snapshot.Fan1RpmCandidate);
        Assert.Equal(6100, snapshot.Fan2RpmCandidate);
        Assert.Contains("candidate", snapshot.Detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ZeroRemainsAnExplicitStoppedFanCandidate_NotEstablishedRpm()
    {
        HpFanRpmEcSnapshot snapshot = HpFanRpmEcProbeSemantics.FromFixedRawBytes(0, 0, Now);

        Assert.True(snapshot.IsAvailable);
        Assert.Equal(0, snapshot.Fan1RpmCandidate);
        Assert.Equal(0, snapshot.Fan2RpmCandidate);
        Assert.Contains("stopped-fan candidate", snapshot.Detail, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(59, 0)]
    [InlineData(0, 62)]
    public void ImplausibleCandidate_FailsClosed(int fan1Raw, int fan2Raw)
    {
        HpFanRpmEcSnapshot snapshot = HpFanRpmEcProbeSemantics.FromFixedRawBytes((byte)fan1Raw, (byte)fan2Raw, Now);

        Assert.Equal(HpFanRpmEcProbeAvailability.ImplausibleCandidate, snapshot.Availability);
        Assert.False(snapshot.IsAvailable);
    }

    [Fact]
    public void ReadFailure_IsUnavailableAndDoesNotCreateCandidate()
    {
        HpFanRpmEcSnapshot snapshot = HpFanRpmEcSnapshot.Unavailable(HpFanRpmEcProbeAvailability.EcReadTimeout, "bounded timeout");

        Assert.False(snapshot.IsAvailable);
        Assert.Null(snapshot.Fan1RpmCandidate);
        Assert.Null(snapshot.Fan2RpmCandidate);
    }

    [Fact]
    public void DiagnosticRoute_ExitsBeforeNormalApplicationStartup()
    {
        string program = ReadRepositoryFile("app", "Program.cs");
        int route = program.IndexOf("HpFanRpmEcProbeCommand.TryRun(args)", StringComparison.Ordinal);
        int startup = program.IndexOf("MainCore(args)", StringComparison.Ordinal);

        Assert.True(route >= 0 && route < startup);
    }

    private static T ReadConstant<T>(string name) where T : struct
    {
        FieldInfo field = typeof(HpFanRpmEcProbeBackend).GetField(name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!;
        return (T)field.GetRawConstantValue()!;
    }

    private static HpFanRpmEcProbeDevice ExactTarget() => new(
        new HpRyzenTemperatureProbeDevice(ExactIdentity(), "AMD Ryzen 5 7640HS", 0x19, 0x74, 0x1), "8BD4");

    private static HpRyzenTemperatureProbeIdentity ExactIdentity() => new(
        HpRyzenTemperatureProbeIdentitySource.NativeSmbios, "HP", "Victus 16-s0035nt", "7Z5Z2EA#AB8", "F.31", "test");

    private static string ReadRepositoryFile(params string[] segments)
    {
        string? root = Directory.GetCurrentDirectory();
        while (root is not null && !File.Exists(Path.Combine(root, "VictusXHub.sln"))) root = Directory.GetParent(root)?.FullName;
        return File.ReadAllText(Path.Combine([root!, .. segments]));
    }
}
