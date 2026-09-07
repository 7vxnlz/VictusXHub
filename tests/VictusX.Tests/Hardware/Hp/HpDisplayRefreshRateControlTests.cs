using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpDisplayRefreshRateControlTests
{
    private static readonly HpDisplayMode Current = new(1920, 1080, 32, 0, 144);

    [Fact]
    public void BuildState_UsesOnlyCurrentModeGeometryAndFiltersDuplicates()
    {
        HpDisplayRefreshRateState state = HpDisplayRefreshRateControl.BuildState(Current,
        [
            new(1920, 1080, 32, 0, 60),
            new(1920, 1080, 32, 0, 144),
            new(1920, 1080, 32, 0, 144),
            new(2560, 1440, 32, 0, 165),
            new(1920, 1080, 24, 0, 120),
            new(1920, 1080, 32, 1, 120)
        ]);

        Assert.True(state.IsAvailable);
        Assert.Equal(144, state.CurrentRateHz);
        Assert.Equal([60, 144], state.SupportedRates);
    }

    [Fact]
    public void BuildState_FailsClosedWithoutAnUnambiguousCurrentMode()
    {
        Assert.False(HpDisplayRefreshRateControl.BuildState(null, [Current]).IsAvailable);
        Assert.False(HpDisplayRefreshRateControl.BuildState(Current, []).IsAvailable);
        Assert.False(HpDisplayRefreshRateControl.BuildState(Current, [new(2560, 1440, 32, 0, 144)]).IsAvailable);
    }

    [Fact]
    public void BuildMenu_PopulatesValidatedRatesAndChecksOnlyCurrentRate()
    {
        var state = new HpDisplayRefreshRateState(120, [60, 120, 144]);

        IReadOnlyList<HpDisplayRefreshRateMenuItem> menu = HpDisplayRefreshRateControl.BuildMenu(state);

        Assert.Equal(["60 Hz", "120 Hz", "144 Hz"], menu.Select(item => item.Text));
        Assert.Equal([false, true, false], menu.Select(item => item.IsCurrent));
        Assert.Equal([60, 120, 144], menu.Select(item => item.RefreshRateHz));
    }

    [Fact]
    public void BuildMenu_ReturnsNoActionsForUnavailableState()
    {
        Assert.Empty(HpDisplayRefreshRateControl.BuildMenu(HpDisplayRefreshRateState.Unavailable));
    }

    [Fact]
    public void Apply_RejectsUnsupportedRateWithoutCallingNativeApi()
    {
        var state = new HpDisplayRefreshRateState(60, [60, 144]);
        bool called = false;

        HpDisplayRefreshRateApplyResult result = HpDisplayRefreshRateControl.Apply(
            state, 120, _ => { called = true; return 0; }, () => 120);

        Assert.False(result.Succeeded);
        Assert.False(called);
        Assert.Equal(60, result.CurrentRateHz);
    }

    [Fact]
    public void Apply_PreservesCurrentStateWhenWindowsRejectsChange()
    {
        var state = new HpDisplayRefreshRateState(60, [60, 144]);
        bool readCalled = false;

        HpDisplayRefreshRateApplyResult result = HpDisplayRefreshRateControl.Apply(
            state, 144, _ => -2, () => { readCalled = true; return 144; });

        Assert.False(result.Succeeded);
        Assert.False(readCalled);
        Assert.Equal(60, result.CurrentRateHz);
        Assert.Contains("rejected", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Apply_RequiresPostApplyConfirmation()
    {
        var state = new HpDisplayRefreshRateState(60, [60, 144]);

        HpDisplayRefreshRateApplyResult result = HpDisplayRefreshRateControl.Apply(
            state, 144, _ => 0, () => 60);

        Assert.False(result.Succeeded);
        Assert.Equal(60, result.CurrentRateHz);
        Assert.Contains("confirm", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Apply_ReportsConfirmedCurrentSelection()
    {
        var state = new HpDisplayRefreshRateState(60, [60, 144]);

        HpDisplayRefreshRateApplyResult result = HpDisplayRefreshRateControl.Apply(
            state, 144, _ => 0, () => 144);

        Assert.True(result.Succeeded);
        Assert.Equal(144, result.CurrentRateHz);
    }
}
