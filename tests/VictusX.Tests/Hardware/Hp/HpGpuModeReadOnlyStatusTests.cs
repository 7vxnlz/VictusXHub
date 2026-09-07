using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpGpuModeReadOnlyStatusTests
{
    [Fact]
    public void ExactTargetCapability_ReportsSupportWithoutInventingCurrentMode()
    {
        HpGpuModeStatus status = HpGpuModeStatus.Resolve(
            true, "HP Victus Gaming Laptop 16-s0035nt", "7Z5Z2EA#AB8", 6);

        Assert.Equal(HpGpuModeAvailability.SupportedStateUnavailable, status.Availability);
        Assert.Null(status.CurrentMode);
        Assert.Equal((byte)6, status.CapabilityRaw);
        Assert.True(status.ModeChangeRequiresReboot);
        Assert.Equal("Supported, state unavailable", status.CapabilityText);
        Assert.Equal("Supported", status.SwitchingCapabilityText);
        Assert.Equal("GPU Mode: Supported, state unavailable", status.DisplayText);
    }

    [Fact]
    public void ExactTargetZeroCapability_ReportsNotSupported()
    {
        HpGpuModeStatus status = HpGpuModeStatus.Resolve(
            true, "HP Victus Gaming Laptop 16-s0035nt", "7Z5Z2EA#AB8", 0);

        Assert.Equal(HpGpuModeAvailability.NotSupported, status.Availability);
        Assert.Equal("Not supported", status.CapabilityText);
        Assert.Equal("Not supported", status.SwitchingCapabilityText);
        Assert.Equal("GPU Mode: Not supported", status.DisplayText);
        Assert.Null(status.CurrentMode);
    }

    [Theory]
    [InlineData(true, "Victus by HP Gaming Laptop 16-s0xxx", "8BD4", 6, "GPU Mode: Unavailable")]
    [InlineData(true, "HP Victus Gaming Laptop 16-s0035nt", "7Z5Z2EA#AB8", null, "GPU Mode: Unavailable")]
    [InlineData(null, null, null, null, "GPU Mode: Unavailable")]
    [InlineData(false, "Other laptop", "OTHER", 6, "GPU Mode: Not supported")]
    public void UnknownOrUnsupportedIdentity_FailsClosed(
        bool? detected, string? model, string? sku, int? rawValue, string expected)
    {
        byte? raw = rawValue is null ? null : checked((byte)rawValue.Value);
        HpGpuModeStatus status = HpGpuModeStatus.Resolve(detected, model, sku, raw);

        Assert.Equal(expected, status.DisplayText);
        Assert.Equal(expected.Replace("GPU Mode: ", ""), status.CapabilityText);
        Assert.Equal(status.Availability == HpGpuModeAvailability.NotSupported ? "Not supported" : "Unavailable",
            status.SwitchingCapabilityText);
        Assert.Null(status.CurrentMode);
    }

    [Fact]
    public void ReadOnlyStatusModel_HasNoWriteOrRebootCapability()
    {
        string[] methodNames = typeof(HpGpuModeStatus).GetMethods()
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .ToArray();

        Assert.DoesNotContain(methodNames, name => name.StartsWith("Set", StringComparison.Ordinal));
        Assert.DoesNotContain(methodNames, name => name.Contains("Write", StringComparison.Ordinal));
        Assert.DoesNotContain(methodNames, name => name.Contains("Invoke", StringComparison.Ordinal));
        Assert.DoesNotContain(methodNames, name => name.Contains("Reboot", StringComparison.Ordinal));
    }
}
