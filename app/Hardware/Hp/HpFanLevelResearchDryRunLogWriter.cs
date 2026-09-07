using System.Globalization;
using System.Text;

namespace GHelper.Hardware.Hp;

public static class HpFanLevelResearchDryRunLogWriter
{
    public static string OutputDirectory => Path.Combine(HpDiagnosticPaths.AppDataDirectory, "Logs", "FanExperiments");

    public static string Write(HpFanLevelResearchDryRunRecord record, string? outputDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(record);
        string directory = Path.GetFullPath(outputDirectory ?? OutputDirectory);
        Directory.CreateDirectory(directory);
        string timestamp = record.TimestampUtc.ToString("yyyyMMdd-HHmmss-fffffff", CultureInfo.InvariantCulture);
        string path = Path.Combine(directory, $"set-fan-level-research-dry-run-{timestamp}-{Guid.NewGuid():N}.json");
        using FileStream stream = new(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
        using StreamWriter writer = new(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.Write(record.ToJson());
        return path;
    }
}
