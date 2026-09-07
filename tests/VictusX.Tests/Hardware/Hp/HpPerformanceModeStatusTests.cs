using GHelper.Hardware.Hp;
using GHelper.UI;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpPerformanceModeStatusTests
{
    [Theory]
    [InlineData(2, 0x50)]
    [InlineData(0, 0x30)]
    [InlineData(1, 0x31)]
    public void ReferenceAliasesNeverEstablishActiveMode(int mode, int expected)
    {
        Assert.Equal((byte)expected, HpPerformanceModeStatus.ReferenceModeByte(mode));
        Assert.False(HpPerformanceModeStatus.CanSwitch);
        Assert.Equal(-1, HpPerformanceModeStatus.CurrentBaseMode);
        Assert.Equal(HpTrayIconKind.Default, HpTrayIconSelector.Select(HpPerformanceModeStatus.CurrentBaseMode));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    [InlineData(255)]
    public void UnsupportedModesHaveNoReferenceMapping(int mode)
    {
        Assert.Null(HpPerformanceModeStatus.ReferenceModeByte(mode));
        Assert.False(HpPerformanceModeStatus.CanSwitch);
        Assert.Contains("Unavailable", HpPerformanceModeStatus.DisplayText);
    }
}
