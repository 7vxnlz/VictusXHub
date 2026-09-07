namespace GHelper.Hardware.Hp;

internal sealed record HpDisplayMode(
    int Width,
    int Height,
    int BitsPerPixel,
    int Orientation,
    int RefreshRateHz);

internal sealed record HpDisplayRefreshRateState(
    int? CurrentRateHz,
    IReadOnlyList<int> SupportedRates)
{
    internal bool IsAvailable => CurrentRateHz is > 0 && SupportedRates.Count > 0;

    internal static HpDisplayRefreshRateState Unavailable { get; } = new(null, Array.Empty<int>());
}

internal sealed record HpDisplayRefreshRateApplyResult(
    bool Succeeded,
    int? CurrentRateHz,
    string Message);

internal sealed record HpDisplayRefreshRateMenuItem(
    int RefreshRateHz,
    string Text,
    bool IsCurrent);

internal static class HpDisplayRefreshRateControl
{
    internal static IReadOnlyList<HpDisplayRefreshRateMenuItem> BuildMenu(HpDisplayRefreshRateState state) =>
        state.IsAvailable
            ? state.SupportedRates
                .Select(rate => new HpDisplayRefreshRateMenuItem(rate, $"{rate} Hz", rate == state.CurrentRateHz))
                .ToArray()
            : Array.Empty<HpDisplayRefreshRateMenuItem>();

    internal static HpDisplayRefreshRateState BuildState(
        HpDisplayMode? current,
        IEnumerable<HpDisplayMode> availableModes)
    {
        if (current is null || !IsValid(current)) return HpDisplayRefreshRateState.Unavailable;

        int[] rates = availableModes
            .Where(mode => IsValid(mode) &&
                mode.Width == current.Width &&
                mode.Height == current.Height &&
                mode.BitsPerPixel == current.BitsPerPixel &&
                mode.Orientation == current.Orientation)
            .Select(mode => mode.RefreshRateHz)
            .Distinct()
            .OrderBy(rate => rate)
            .ToArray();

        return rates.Contains(current.RefreshRateHz)
            ? new HpDisplayRefreshRateState(current.RefreshRateHz, rates)
            : HpDisplayRefreshRateState.Unavailable;
    }

    internal static HpDisplayRefreshRateApplyResult Apply(
        HpDisplayRefreshRateState state,
        int requestedRateHz,
        Func<int, int> nativeApply,
        Func<int?> readCurrentRate)
    {
        if (!state.IsAvailable || !state.SupportedRates.Contains(requestedRateHz))
            return new(false, state.CurrentRateHz, "The requested refresh rate is not available for the current internal-panel mode.");

        if (state.CurrentRateHz == requestedRateHz)
            return new(true, requestedRateHz, $"Display refresh rate is already {requestedRateHz}Hz.");

        if (nativeApply(requestedRateHz) != 0)
            return new(false, state.CurrentRateHz, "Windows rejected the refresh-rate change.");

        int? refreshedRate = readCurrentRate();
        return refreshedRate == requestedRateHz
            ? new(true, refreshedRate, $"Display refresh rate changed to {requestedRateHz}Hz.")
            : new(false, refreshedRate ?? state.CurrentRateHz, "Windows did not confirm the requested refresh rate.");
    }

    private static bool IsValid(HpDisplayMode mode) =>
        mode is { Width: > 0, Height: > 0, BitsPerPixel: > 0, RefreshRateHz: > 0 and <= 1000 };
}
