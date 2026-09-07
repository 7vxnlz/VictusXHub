namespace GHelper.UI;

internal enum HpTrayIconKind
{
    Default,
    Silent,
    Balanced,
    Turbo
}

internal static class HpTrayIconSelector
{
    private const int BalancedPerformanceMode = 0;
    private const int TurboPerformanceMode = 1;
    private const int SilentPerformanceMode = 2;

    internal static HpTrayIconKind Select(int basePerformanceMode) => basePerformanceMode switch
    {
        SilentPerformanceMode => HpTrayIconKind.Silent,
        BalancedPerformanceMode => HpTrayIconKind.Balanced,
        TurboPerformanceMode => HpTrayIconKind.Turbo,
        _ => HpTrayIconKind.Default
    };

    internal static string FormatModeStatus(int basePerformanceMode) => Select(basePerformanceMode) switch
    {
        HpTrayIconKind.Silent => "Mode: Silent",
        HpTrayIconKind.Balanced => "Mode: Balanced",
        HpTrayIconKind.Turbo => "Mode: Turbo",
        _ => "Mode: Unavailable"
    };

    internal static string GetResourceName(HpTrayIconKind kind) => kind switch
    {
        HpTrayIconKind.Silent => "GHelper.Assets.VictusX.Silent.ico",
        HpTrayIconKind.Balanced => "GHelper.Assets.VictusX.Balanced.ico",
        HpTrayIconKind.Turbo => "GHelper.Assets.VictusX.Turbo.ico",
        _ => "GHelper.Assets.VictusX.ico"
    };
}
