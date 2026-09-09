using GHelper.UI;
using Xunit;

namespace VictusXHub.Tests.Hardware.Hp;

public sealed class HpTrayIconSelectorTests
{
    [Theory]
    [InlineData(2, "Mode: Silent")]
    [InlineData(0, "Mode: Balanced")]
    [InlineData(1, "Mode: Turbo")]
    [InlineData(-1, "Mode: Unavailable")]
    [InlineData(3, "Mode: Unavailable")]
    public void HpIdentity_UsesOneResourceWithoutChangingModeStatusPresentation(
        int basePerformanceMode,
        string expectedStatus)
    {
        Assert.Equal("GHelper.Assets.VictusXHub.ico", HpTrayIconSelector.ResourceName);
        Assert.Equal(expectedStatus, HpTrayIconSelector.FormatModeStatus(basePerformanceMode));
    }
}
