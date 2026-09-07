using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpBiosWmiCommandCatalogTests
{
    [Fact]
    public void FanGetCount_IsPreparedAsSafeReadOnlyFanStatusCandidate()
    {
        HpBiosWmiCommandDefinition command = Assert.Single(
            HpBiosWmiCommandCatalog.Definitions,
            definition => definition.Name == "FanGetCount");

        Assert.Equal(HpBiosWmiCommandFamily.Fan, command.Family);
        Assert.Equal(0x10u, command.CommandId);
        Assert.Equal("hpqBIOSInt4", command.MethodName);
        Assert.Equal(4, command.ExpectedInputSize);
        Assert.Equal(4, command.ExpectedOutputSize);
        Assert.Equal(HpBiosWmiCommandAccess.ReadOnly, command.Access);
        Assert.Equal(HpBiosWmiCommandSafety.SafeReadOnlyInvocation, command.Safety);
    }

    [Fact]
    public void FanMaxGet_IsPreparedAsSafeReadOnlyFanStatusCandidate()
    {
        HpBiosWmiCommandDefinition command = Assert.Single(
            HpBiosWmiCommandCatalog.Definitions,
            definition => definition.Name == "FanMaxGet");

        Assert.Equal(HpBiosWmiCommandFamily.Fan, command.Family);
        Assert.Equal(0x26u, command.CommandId);
        Assert.Equal("hpqBIOSInt4", command.MethodName);
        Assert.Equal(4, command.ExpectedInputSize);
        Assert.Equal(4, command.ExpectedOutputSize);
        Assert.Equal(HpBiosWmiCommandAccess.ReadOnly, command.Access);
        Assert.Equal(HpBiosWmiCommandSafety.SafeReadOnlyInvocation, command.Safety);
    }

    [Fact]
    public void FanGetLevel_IsPreparedAsSafeReadOnlyRawStatusCandidate()
    {
        HpBiosWmiCommandDefinition command = Assert.Single(
            HpBiosWmiCommandCatalog.Definitions,
            definition => definition.Name == "FanGetLevel");

        Assert.Equal(HpBiosWmiCommandFamily.Fan, command.Family);
        Assert.Equal(0x2Du, command.CommandId);
        Assert.Equal("hpqBIOSInt128", command.MethodName);
        Assert.Equal(4, command.ExpectedInputSize);
        Assert.Equal(128, command.ExpectedOutputSize);
        Assert.Equal(HpBiosWmiCommandAccess.ReadOnly, command.Access);
        Assert.Equal(HpBiosWmiCommandSafety.SafeReadOnlyInvocation, command.Safety);
    }

    [Theory]
    [InlineData("FanModeWrite", 0x1A)]
    [InlineData("FanLevelWrite", 0x2E)]
    [InlineData("FanMaxWrite", 0x27)]
    [InlineData("FanLevelV2Ambiguous", 0x37)]
    public void FanWriteOrAmbiguousCommands_RemainBlocked(string name, uint commandId)
    {
        HpBiosWmiCommandDefinition command = Assert.Single(
            HpBiosWmiCommandCatalog.Definitions,
            definition => definition.Name == name);

        Assert.Equal(commandId, command.CommandId);
        Assert.NotEqual(HpBiosWmiCommandSafety.SafeReadOnlyInvocation, command.Safety);
        Assert.True(command.Safety is HpBiosWmiCommandSafety.Forbidden or HpBiosWmiCommandSafety.Unknown);
    }
}
