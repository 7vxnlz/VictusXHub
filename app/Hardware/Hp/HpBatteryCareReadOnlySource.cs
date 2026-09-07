using System.Management;

namespace GHelper.Hardware.Hp;

internal sealed class HpBatteryCareReadOnlySource
{
    private const string ScopePath = @"root\HP\InstrumentedBIOS";
    private const string Query = "SELECT Name, CurrentValue FROM HP_BIOSSetting " +
        "WHERE Name = 'Adaptive Battery Extender' OR Name = 'Adaptive Battery Optimizer'";

    public HpBatteryCareProbeResult Read()
    {
        if (!OperatingSystem.IsWindows()) return HpBatteryCareProbeResult.Unavailable;

        try
        {
            using var searcher = new ManagementObjectSearcher(new ManagementScope(ScopePath), new ObjectQuery(Query));
            foreach (ManagementObject setting in searcher.Get())
            {
                using (setting)
                {
                    string? name = setting["Name"]?.ToString();
                    if (!IsKnownSetting(name)) continue;
                    return HpBatteryCareProbeResult.FromSetting(name!, setting["CurrentValue"]?.ToString());
                }
            }
            return HpBatteryCareProbeResult.NotExposed;
        }
        catch (Exception)
        {
            // Namespace absence, permissions, provider failures and malformed data all fail closed.
            return HpBatteryCareProbeResult.Unavailable;
        }
    }

    private static bool IsKnownSetting(string? name) =>
        name?.Equals("Adaptive Battery Extender", StringComparison.OrdinalIgnoreCase) == true ||
        name?.Equals("Adaptive Battery Optimizer", StringComparison.OrdinalIgnoreCase) == true;
}
