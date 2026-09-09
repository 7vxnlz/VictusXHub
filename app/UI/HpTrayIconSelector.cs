namespace GHelper.UI;

internal static class HpTrayIconSelector
{
    private const int BalancedPerformanceMode = 0;
    private const int TurboPerformanceMode = 1;
    private const int SilentPerformanceMode = 2;

    internal const string ResourceName = "GHelper.Assets.VictusXHub.ico";

    internal static string FormatModeStatus(int basePerformanceMode) => basePerformanceMode switch
    {
        SilentPerformanceMode => "Mode: Silent",
        BalancedPerformanceMode => "Mode: Balanced",
        TurboPerformanceMode => "Mode: Turbo",
        _ => "Mode: Unavailable"
    };
}
