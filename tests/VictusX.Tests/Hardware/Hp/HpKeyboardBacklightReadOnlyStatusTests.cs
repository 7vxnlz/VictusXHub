using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpKeyboardBacklightReadOnlyStatusTests
{
    [Fact]
    public void ExactTarget_ReportsSupportedWithoutInventingStateOrLevel()
    {
        HpKeyboardBacklightStatus status = HpKeyboardBacklightStatus.Resolve(
            true, "HP Victus Gaming Laptop 16-s0035nt", "7Z5Z2EA#AB8");

        Assert.Equal(HpKeyboardBacklightAvailability.SupportedStateUnavailable, status.Availability);
        Assert.Null(status.IsOn);
        Assert.Null(status.BrightnessLevel);
        Assert.Equal("Supported, state unavailable", status.CapabilityText);
        Assert.Equal("Keyboard lighting: Supported, state unavailable", status.DisplayText);
        Assert.Contains("BIOS F.31 state is not validated", status.EvidenceText);
    }

    [Theory]
    [InlineData(true, "Victus by HP Gaming Laptop 16-s0xxx", "8BD4", "Keyboard lighting: Unavailable")]
    [InlineData(null, null, null, "Keyboard lighting: Unavailable")]
    [InlineData(false, "Other laptop", "OTHER", "Keyboard lighting: Not supported")]
    public void MissingOrUnsupportedIdentity_FailsClosed(
        bool? detected, string? model, string? sku, string expected)
    {
        HpKeyboardBacklightStatus status = HpKeyboardBacklightStatus.Resolve(detected, model, sku);

        Assert.Equal(expected, status.DisplayText);
        Assert.Equal(expected.Replace("Keyboard lighting: ", ""), status.CapabilityText);
        Assert.Null(status.IsOn);
        Assert.Null(status.BrightnessLevel);
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
}
