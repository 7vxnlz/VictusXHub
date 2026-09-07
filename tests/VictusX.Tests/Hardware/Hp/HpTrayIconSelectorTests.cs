using GHelper.UI;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpTrayIconSelectorTests
{
    [Theory]
    [InlineData(2, "Silent", "GHelper.Assets.VictusX.Silent.ico", "Mode: Silent")]
    [InlineData(0, "Balanced", "GHelper.Assets.VictusX.Balanced.ico", "Mode: Balanced")]
    [InlineData(1, "Turbo", "GHelper.Assets.VictusX.Turbo.ico", "Mode: Turbo")]
    [InlineData(-1, "Default", "GHelper.Assets.VictusX.ico", "Mode: Unavailable")]
    [InlineData(3, "Default", "GHelper.Assets.VictusX.ico", "Mode: Unavailable")]
    public void Select_MapsInheritedBaseModeToExpectedHpIcon(
        int basePerformanceMode,
        string expectedKind,
        string expectedResource,
        string expectedStatus)
    {
        HpTrayIconKind kind = HpTrayIconSelector.Select(basePerformanceMode);

        Assert.Equal(expectedKind, kind.ToString());
        Assert.Equal(expectedResource, HpTrayIconSelector.GetResourceName(kind));
        Assert.Equal(expectedStatus, HpTrayIconSelector.FormatModeStatus(basePerformanceMode));
    }
}
