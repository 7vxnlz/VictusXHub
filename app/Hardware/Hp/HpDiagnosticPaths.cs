namespace GHelper.Hardware.Hp;

public static class HpDiagnosticPaths
{
    public const string ProductDirectoryName = "VictusX";
    public const string CapabilityReportFileName = "hp-capability-report.json";

    public static string AppDataDirectory => BuildAppDataDirectory(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

    public static string CapabilityReportPath => BuildCapabilityReportPath(AppDataDirectory);

    public static string ExportDirectory => BuildExportDirectory(AppDataDirectory);

    public static string BuildAppDataDirectory(string applicationDataRoot)
    {
        return Path.Combine(applicationDataRoot, ProductDirectoryName);
    }

    public static string BuildCapabilityReportPath(string appDataDirectory)
    {
        return Path.Combine(appDataDirectory, CapabilityReportFileName);
    }

    public static string BuildExportDirectory(string appDataDirectory)
    {
        return Path.Combine(appDataDirectory, "Logs", "Reports");
    }
}
