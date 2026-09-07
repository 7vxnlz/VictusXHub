using System.Text.Json;
using System.Text.Json.Serialization;

namespace GHelper.Hardware.Hp;

public static class HpFanMaxExperimentLogFormatter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    static HpFanMaxExperimentLogFormatter()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public static string Format(HpFanMaxExperimentLogRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        return JsonSerializer.Serialize(record, JsonOptions);
    }
}
