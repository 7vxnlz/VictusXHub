using GHelper.Hardware.Hp;
using System.Reflection;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpRyzenTemperatureProbeTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 8, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ExactTargetGate_AcceptsOnlyTheValidatedVictusIdentity()
    {
        HpRyzenTemperatureProbeGateResult result = HpRyzenTemperatureProbeGate.Evaluate(ExactTarget());

        Assert.True(result.IsAccepted);
        Assert.Equal(HpRyzenTemperatureProbeAvailability.Available, result.Availability);
    }

    [Fact]
    public void ExactPhoenixCpuid_UsesDecodedFamily19Model74Stepping1()
    {
        HpRyzenTemperatureProbeGateResult result = HpRyzenTemperatureProbeGate.Evaluate(ExactTarget());

        Assert.True(result.IsAccepted);
        Assert.Equal(0x74, HpRyzenTemperatureProbeGate.ExpectedCpuModel);
        Assert.Equal(0x1, HpRyzenTemperatureProbeGate.ExpectedCpuStepping);
    }

    [Fact]
    public void OldModel70_IsNotAcceptedAsThePhoenixCpuidModel()
    {
        HpRyzenTemperatureProbeGateResult result = HpRyzenTemperatureProbeGate.Evaluate(ExactTarget() with { CpuModel = 0x70 });

        Assert.False(result.IsAccepted);
        Assert.Equal(HpRyzenTemperatureProbeAvailability.UnsupportedCpu, result.Availability);
    }

    [Fact]
    public void ExactTargetGate_RejectsWrongSkuBeforePawnIoCanOpen()
    {
        HpRyzenTemperatureProbeGateResult result = HpRyzenTemperatureProbeGate.Evaluate(ExactTarget() with
        {
            Identity = ExactIdentity() with { Sku = "8Z5Z2EA#AB8" }
        });

        Assert.False(result.IsAccepted);
        Assert.Equal(HpRyzenTemperatureProbeAvailability.DeviceMismatch, result.Availability);
        Assert.False(HpRyzenTemperatureProbeGate.MayOpenPawnIo(result));
    }

    [Fact]
    public void ExactTargetGate_RejectsWrongCpuFamilyOrModel()
    {
        HpRyzenTemperatureProbeGateResult result = HpRyzenTemperatureProbeGate.Evaluate(ExactTarget() with { CpuModel = 0x71 });

        Assert.False(result.IsAccepted);
        Assert.Equal(HpRyzenTemperatureProbeAvailability.UnsupportedCpu, result.Availability);
    }

    [Fact]
    public void ExactTargetGate_RejectsWrongCpuFamily()
    {
        HpRyzenTemperatureProbeGateResult result = HpRyzenTemperatureProbeGate.Evaluate(ExactTarget() with { CpuFamily = 0x17 });

        Assert.False(result.IsAccepted);
        Assert.Equal(HpRyzenTemperatureProbeAvailability.UnsupportedCpu, result.Availability);
    }

    [Fact]
    public void NativeSmbiosParser_ReadsExactSystemAndBiosStrings()
    {
        HpRyzenTemperatureProbeIdentity identity = HpRyzenTemperatureProbeSmbios.Parse(CreateRawSmbios());

        Assert.Equal(HpRyzenTemperatureProbeIdentitySource.NativeSmbios, identity.Source);
        Assert.Equal("HP", identity.Manufacturer);
        Assert.Equal("Victus 16-s0035nt", identity.Model);
        Assert.Equal("7Z5Z2EA#AB8", identity.Sku);
        Assert.Equal("F.31", identity.Bios);
        Assert.True(HpRyzenTemperatureProbeIdentity.IsExactTarget(identity));
    }

    [Fact]
    public void MalformedOrTruncatedSmbios_FailsClosed()
    {
        byte[] truncated = CreateRawSmbios()[..^1];

        HpRyzenTemperatureProbeIdentity identity = HpRyzenTemperatureProbeSmbios.Parse(truncated);

        Assert.Equal(HpRyzenTemperatureProbeIdentitySource.Unavailable, identity.Source);
        Assert.False(identity.HasRawIdentity);
    }

    [Fact]
    public void WmiUnavailable_NativeSmbiosValid_SelectsNativeAndGatePasses()
    {
        HpRyzenTemperatureProbeIdentity wmi = HpRyzenTemperatureProbeIdentity.Unavailable("Access denied");
        HpRyzenTemperatureProbeIdentity native = HpRyzenTemperatureProbeSmbios.Parse(CreateRawSmbios());
        HpRyzenTemperatureProbeIdentity selected = HpRyzenTemperatureProbeIdentitySelection.Select(
            wmi, native, HpRyzenTemperatureProbeIdentity.Unavailable("Registry not used"));

        HpRyzenTemperatureProbeGateResult result = HpRyzenTemperatureProbeGate.Evaluate(ExactTarget() with { Identity = selected });

        Assert.Equal(HpRyzenTemperatureProbeIdentitySource.NativeSmbios, selected.Source);
        Assert.Contains("WMI: Access denied", selected.Detail, StringComparison.Ordinal);
        Assert.True(result.IsAccepted);
    }

    [Fact]
    public void AllIdentitySourcesUnavailable_FailsBeforePawnIoGate()
    {
        HpRyzenTemperatureProbeIdentity selected = HpRyzenTemperatureProbeIdentitySelection.Select(
            HpRyzenTemperatureProbeIdentity.Unavailable("WMI denied"),
            HpRyzenTemperatureProbeIdentity.Unavailable("RSMB unavailable"),
            HpRyzenTemperatureProbeIdentity.Unavailable("Registry unavailable"));

        HpRyzenTemperatureProbeGateResult result = HpRyzenTemperatureProbeGate.Evaluate(ExactTarget() with { Identity = selected });

        Assert.False(result.IsAccepted);
        Assert.Equal(HpRyzenTemperatureProbeAvailability.DeviceMismatch, result.Availability);
    }

    [Fact]
    public void WmiAndNativeUnavailable_RegistryBiosExactSku_StillPasses()
    {
        HpRyzenTemperatureProbeIdentity registry = new(HpRyzenTemperatureProbeIdentitySource.RegistryBios,
            "HP", "Victus by HP Gaming Laptop 16-s0xxx", "7Z5Z2EA#AB8", "F.31", "test registry fallback");
        HpRyzenTemperatureProbeIdentity selected = HpRyzenTemperatureProbeIdentitySelection.Select(
            HpRyzenTemperatureProbeIdentity.Unavailable("WMI denied"),
            HpRyzenTemperatureProbeIdentity.Unavailable("RSMB denied"), registry);

        HpRyzenTemperatureProbeGateResult result = HpRyzenTemperatureProbeGate.Evaluate(ExactTarget() with { Identity = selected });

        Assert.Equal(HpRyzenTemperatureProbeIdentitySource.RegistryBios, selected.Source);
        Assert.True(result.IsAccepted);
    }

    [Fact]
    public void ExactTctlTdieRegister_DecodesToFreshCelsiusSample()
    {
        uint raw = (70u * 8) << 21;

        HpRyzenTemperatureProbeResult result = HpRyzenTemperatureProbeSemantics.FromSmnTemperatureRegister(raw, Now);

        Assert.True(result.IsAvailable);
        Assert.Equal(70, result.Celsius);
        Assert.True(result.IsFresh(Now.AddSeconds(4)));
        Assert.Equal(HpRyzenTemperatureProbeResult.SourceName, "Core (Tctl/Tdie)");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(116)]
    public void ZeroAndOutOfRangeTemperature_AreUnavailable(double value)
    {
        HpRyzenTemperatureProbeResult result = HpRyzenTemperatureProbeSemantics.FromExactSensor(
            HpRyzenTemperatureProbeResult.SourceName, value, Now);

        Assert.Equal(HpRyzenTemperatureProbeAvailability.InvalidTemperature, result.Availability);
        Assert.Null(result.Celsius);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void NonFiniteTemperature_IsUnavailable(double value)
    {
        HpRyzenTemperatureProbeResult result = HpRyzenTemperatureProbeSemantics.FromExactSensor(
            HpRyzenTemperatureProbeResult.SourceName, value, Now);

        Assert.Equal(HpRyzenTemperatureProbeAvailability.InvalidTemperature, result.Availability);
        Assert.Null(result.Celsius);
    }

    [Fact]
    public void UnrecognizedSensorName_CannotBecomeCpuPackageTemperature()
    {
        HpRyzenTemperatureProbeResult result = HpRyzenTemperatureProbeSemantics.FromExactSensor("ACPI Thermal Zone", 70, Now);

        Assert.Equal(HpRyzenTemperatureProbeAvailability.InvalidTemperature, result.Availability);
        Assert.Null(result.Celsius);
        Assert.Contains("Rejected non-package sensor", result.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void StaleSample_IsNotFresh()
    {
        HpRyzenTemperatureProbeResult result = HpRyzenTemperatureProbeSemantics.FromExactSensor(
            HpRyzenTemperatureProbeResult.SourceName, 70, Now);

        Assert.False(result.IsFresh(Now.AddSeconds(6)));
    }

    [Theory]
    [InlineData((int)HpRyzenTemperatureProbeAvailability.PawnIoNotInstalled)]
    [InlineData((int)HpRyzenTemperatureProbeAvailability.ModuleLoadFailed)]
    [InlineData((int)HpRyzenTemperatureProbeAvailability.SmnReadFailed)]
    public void BackendFailure_RemainsUnavailable(int availabilityValue)
    {
        HpRyzenTemperatureProbeAvailability availability = (HpRyzenTemperatureProbeAvailability)availabilityValue;
        HpRyzenTemperatureProbeResult result = HpRyzenTemperatureProbeResult.Unavailable(availability, "test failure");

        Assert.False(result.IsAvailable);
        Assert.False(result.IsFresh(Now));
        Assert.Null(result.Celsius);
    }

    [Fact]
    public void BackendExposesOnlyTypedReadOperation()
    {
        string[] publicMethods = typeof(HpRyzenTemperatureProbeBackend)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(method => method.Name)
            .ToArray();

        Assert.Equal(["Open"], publicMethods);
        Assert.DoesNotContain("Execute", publicMethods);
        Assert.DoesNotContain("LoadModule", publicMethods);
        Assert.DoesNotContain("ReadSmn", publicMethods);
    }

    [Fact]
    public void TelemetryProvider_ExactTargetOpensOneTypedSessionAndReturnsFreshTemperature()
    {
        int opens = 0;
        var session = new FakeSession(HpRyzenTemperatureProbeSemantics.FromExactSensor(
            HpRyzenTemperatureProbeResult.SourceName, 36.125, Now));
        using var provider = new HpRyzenTemperatureTelemetryProvider(ExactTarget, () =>
        {
            opens++;
            return HpRyzenTemperatureProbeSessionOpenResult.Opened(session);
        });

        HpRyzenTemperatureProbeResult first = provider.Poll(Now);
        HpRyzenTemperatureProbeResult second = provider.Poll(Now.AddSeconds(1));

        Assert.True(first.IsAvailable);
        Assert.True(second.IsAvailable);
        Assert.Equal(36.125, first.Celsius);
        Assert.Equal(1, opens);
        Assert.Equal(2, session.ReadCount);
        Assert.Equal(Now, first.SampledAt);
        Assert.Equal(Now.AddSeconds(1), second.SampledAt);
        Assert.Contains("AMD THM_TCON_CUR_TMP", first.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void TelemetryProvider_WrongSkuNeverOpensPawnIo()
    {
        int opens = 0;
        using var provider = new HpRyzenTemperatureTelemetryProvider(
            () => ExactTarget() with { Identity = ExactIdentity() with { Sku = "wrong" } },
            () =>
            {
                opens++;
                return HpRyzenTemperatureProbeSessionOpenResult.Failed(HpRyzenTemperatureProbeAvailability.PawnIoOpenFailed, "must not run");
            });

        HpRyzenTemperatureProbeResult result = provider.Poll(Now);

        Assert.Equal(HpRyzenTemperatureProbeAvailability.DeviceMismatch, result.Availability);
        Assert.Equal(0, opens);
    }

    [Theory]
    [InlineData((int)HpRyzenTemperatureProbeAvailability.PawnIoAccessDenied)]
    [InlineData((int)HpRyzenTemperatureProbeAvailability.ModuleHashMismatch)]
    [InlineData((int)HpRyzenTemperatureProbeAvailability.ModuleLoadFailed)]
    public void TelemetryProvider_OpenFailureFailsClosedWithoutThrowing(int availabilityValue)
    {
        HpRyzenTemperatureProbeAvailability availability = (HpRyzenTemperatureProbeAvailability)availabilityValue;
        using var provider = new HpRyzenTemperatureTelemetryProvider(ExactTarget,
            () => HpRyzenTemperatureProbeSessionOpenResult.Failed(availability, "test open failure"));

        HpRyzenTemperatureProbeResult result = provider.Poll(Now);

        Assert.Equal(availability, result.Availability);
        Assert.False(result.IsAvailable);
        Assert.Null(result.Celsius);
    }

    [Fact]
    public void TelemetryProvider_FailedReadDoesNotRetainPriorTemperature()
    {
        var session = new FakeSession(
            HpRyzenTemperatureProbeSemantics.FromExactSensor(HpRyzenTemperatureProbeResult.SourceName, 36, Now),
            HpRyzenTemperatureProbeResult.Unavailable(HpRyzenTemperatureProbeAvailability.SmnReadFailed, "read failed"));
        using var temperature = new HpRyzenTemperatureTelemetryProvider(ExactTarget,
            () => HpRyzenTemperatureProbeSessionOpenResult.Opened(session));
        var telemetry = new HpReadOnlyTelemetryProvider(new EmptyTelemetrySource(), cpuTemperature: temperature);

        HpReadOnlyTelemetrySnapshot first = telemetry.Capture(Now);
        HpReadOnlyTelemetrySnapshot second = telemetry.Capture(Now.AddSeconds(1));

        Assert.Equal(36, first.CpuTemperatureCelsius);
        Assert.Null(second.CpuTemperatureCelsius);
        Assert.Equal("Unavailable", HpReadOnlyTelemetryFormatter.Format(second, Now.AddSeconds(1), true, false).CpuTemperature);
    }

    [Fact]
    public void TelemetryProvider_DisposeClosesSessionAndPreventsFurtherReads()
    {
        var session = new FakeSession(HpRyzenTemperatureProbeSemantics.FromExactSensor(
            HpRyzenTemperatureProbeResult.SourceName, 36, Now));
        var provider = new HpRyzenTemperatureTelemetryProvider(ExactTarget,
            () => HpRyzenTemperatureProbeSessionOpenResult.Opened(session));

        provider.Poll(Now);
        provider.Dispose();
        HpRyzenTemperatureProbeResult afterDispose = provider.Poll(Now.AddSeconds(1));

        Assert.True(session.Disposed);
        Assert.Equal(1, session.ReadCount);
        Assert.False(afterDispose.IsAvailable);
    }

    [Fact]
    public void BackendKeepsTheOnlySmnAddressFixed()
    {
        FieldInfo? field = typeof(HpRyzenTemperatureProbeBackend).GetField("SmnTemperatureAddress",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(field);
        Assert.Equal(0x00059800u, (uint)field!.GetRawConstantValue()!);
        Assert.DoesNotContain(typeof(HpRyzenTemperatureTelemetryProvider).GetMethods(BindingFlags.Public | BindingFlags.Instance),
            method => method.GetParameters().Any(parameter => parameter.ParameterType == typeof(uint) || parameter.ParameterType == typeof(string)));
    }

    private static HpRyzenTemperatureProbeDevice ExactTarget() => new(
        ExactIdentity(), "AMD Ryzen 5 7640HS", 0x19, 0x74, 1);

    private static HpRyzenTemperatureProbeIdentity ExactIdentity() => new(
        HpRyzenTemperatureProbeIdentitySource.NativeSmbios, "HP", "Victus 16-s0035nt", "7Z5Z2EA#AB8", "F.31", "test");

    private sealed class EmptyTelemetrySource : IHpReadOnlyTelemetrySource
    {
        public HpCpuTimes? ReadCpuTimes() => null;
        public HpPowerStatus? ReadPowerStatus() => null;
    }

    private sealed class FakeSession(params HpRyzenTemperatureProbeResult[] results) : IHpRyzenTemperatureProbeSession
    {
        private readonly Queue<HpRyzenTemperatureProbeResult> pending = new(results);
        private HpRyzenTemperatureProbeResult? last;

        public int ReadCount { get; private set; }
        public bool Disposed { get; private set; }

        public HpRyzenTemperatureProbeResult Read(DateTimeOffset sampledAt)
        {
            ReadCount++;
            last = pending.Count > 0 ? pending.Dequeue() : last;
            return last is { IsAvailable: true } sample
                ? sample with { SampledAt = sampledAt }
                : last ?? HpRyzenTemperatureProbeResult.Unavailable(HpRyzenTemperatureProbeAvailability.SmnReadFailed, "no result");
        }

        public void Dispose() => Disposed = true;
    }

    private static byte[] CreateRawSmbios()
    {
        byte[] type0 = Structure(0, 0x12, new Dictionary<int, byte> { [4] = 1, [5] = 2 }, "HP", "F.31");
        byte[] type1 = Structure(1, 0x1B, new Dictionary<int, byte> { [4] = 1, [5] = 2, [0x19] = 3 },
            "HP", "Victus 16-s0035nt", "7Z5Z2EA#AB8");
        byte[] type127 = [127, 4, 0, 0, 0, 0];
        byte[] table = [.. type0, .. type1, .. type127];
        byte[] raw = new byte[8 + table.Length];
        BitConverter.TryWriteBytes(raw.AsSpan(4), table.Length);
        table.CopyTo(raw, 8);
        return raw;
    }

    private static byte[] Structure(byte type, int formattedLength, IReadOnlyDictionary<int, byte> indices, params string[] strings)
    {
        byte[] formatted = new byte[formattedLength];
        formatted[0] = type;
        formatted[1] = (byte)formattedLength;
        foreach ((int offset, byte index) in indices) formatted[offset] = index;
        return [.. formatted, .. System.Text.Encoding.ASCII.GetBytes(string.Join('\0', strings)), 0, 0];
    }
}
