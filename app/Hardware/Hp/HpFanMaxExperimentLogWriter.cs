using System.Text;

namespace GHelper.Hardware.Hp;

public static class HpFanMaxExperimentLogWriter
{
    public static string ExperimentDirectory => BuildExperimentDirectory(HpDiagnosticPaths.AppDataDirectory);

    public static string BuildExperimentDirectory(string appDataDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appDataDirectory);
        return Path.Combine(appDataDirectory, "Logs", "FanExperiments");
    }

    public static string Write(HpFanMaxExperimentLogRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        Directory.CreateDirectory(ExperimentDirectory);
        string fileName = "set-fan-max-experiment-" + record.TimestampUtc.ToString("yyyyMMdd-HHmmss-fffffff") + ".json";
        string filePath = Path.Combine(ExperimentDirectory, fileName);

        using FileStream stream = new(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
        using StreamWriter writer = new(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.Write(HpFanMaxExperimentLogFormatter.Format(record));
        return filePath;
    }
}
