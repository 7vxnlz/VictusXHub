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
    public void ExactTargetGate_RejectsWrongSkuBeforePawnIoCanOpen()
    {
        HpRyzenTemperatureProbeGateResult result = HpRyzenTemperatureProbeGate.Evaluate(ExactTarget() with { Sku = "7Z5Z2EA#XYZ" });

        Assert.False(result.IsAccepted);
        Assert.Equal(HpRyzenTemperatureProbeAvailability.DeviceMismatch, result.Availability);
    }

    [Fact]
    public void ExactTargetGate_RejectsWrongCpuFamilyOrModel()
    {
        HpRyzenTemperatureProbeGateResult result = HpRyzenTemperatureProbeGate.Evaluate(ExactTarget() with { CpuModel = 0x71 });

        Assert.False(result.IsAccepted);
        Assert.Equal(HpRyzenTemperatureProbeAvailability.UnsupportedCpu, result.Availability);
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

    private static HpRyzenTemperatureProbeDevice ExactTarget() => new(
        "HP", "Victus 16-s0035nt", "7Z5Z2EA#AB8", "F.31", "AMD Ryzen 5 7640HS", 0x19, 0x70, 1);
}
