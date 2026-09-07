using System.Text.Json;

namespace GHelper.Hardware.Hp;

public enum HpDiagnosticReportLoadStatus
{
    Loaded,
    NotAvailable,
    CouldNotBeRead
}

public sealed class HpDiagnosticReportLoadResult
{
    private readonly Dictionary<string, string> values;

    internal HpDiagnosticReportLoadResult(HpDiagnosticReportLoadStatus status, Dictionary<string, string>? values = null)
    {
        Status = status;
        this.values = values ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public HpDiagnosticReportLoadStatus Status { get; }

    public string SourceDescription => Status switch
    {
        HpDiagnosticReportLoadStatus.Loaded => "Cached report: Loaded locally. " + HpDiagnosticStatusText.SomeFieldsNotAvailable + " in older reports. Reload reads this file only and does not invoke WMI.",
        HpDiagnosticReportLoadStatus.CouldNotBeRead => "Cached report: " + HpDiagnosticStatusText.ReportCouldNotBeRead + ". Safe fallback values are shown.",
        _ => "Cached report: " + HpDiagnosticStatusText.ReportNotAvailable + ". Safe fallback values are shown."
    };

    public string? GetValue(string path) => values.GetValueOrDefault(path);

    public bool? GetBool(string path)
    {
        return bool.TryParse(GetValue(path), out bool value) ? value : null;
    }

    public bool? GetHpVictusDetected()
    {
        bool? looksLikeHp = GetBool("LooksLikeHp");
        bool? looksLikeVictus = GetBool("LooksLikeVictus");
        return looksLikeHp.HasValue && looksLikeVictus.HasValue ? looksLikeHp.Value && looksLikeVictus.Value : null;
    }
}

public static class HpDiagnosticReportLoader
{
    public static HpDiagnosticReportLoadResult Load(string reportPath)
    {
        if (string.IsNullOrWhiteSpace(reportPath) || !File.Exists(reportPath))
        {
            return new HpDiagnosticReportLoadResult(HpDiagnosticReportLoadStatus.NotAvailable);
        }

        try
        {
            string json = File.ReadAllText(reportPath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new HpDiagnosticReportLoadResult(HpDiagnosticReportLoadStatus.CouldNotBeRead);
            }

            using JsonDocument document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return new HpDiagnosticReportLoadResult(HpDiagnosticReportLoadStatus.CouldNotBeRead);
            }

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            AddValues(values, string.Empty, document.RootElement);
            return new HpDiagnosticReportLoadResult(HpDiagnosticReportLoadStatus.Loaded, values);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return new HpDiagnosticReportLoadResult(HpDiagnosticReportLoadStatus.CouldNotBeRead);
        }
    }

    private static void AddValues(Dictionary<string, string> values, string path, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    string propertyPath = string.IsNullOrEmpty(path) ? property.Name : path + "." + property.Name;
                    AddValues(values, propertyPath, property.Value);
                }
                break;
            case JsonValueKind.Array:
                values[path] = string.Join(" | ", element.EnumerateArray().Select(GetElementText).Where(value => !string.IsNullOrWhiteSpace(value)));
                break;
            case JsonValueKind.String:
                values[path] = element.GetString() ?? string.Empty;
                break;
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                values[path] = element.GetRawText();
                break;
        }
    }

    private static string GetElementText(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString() ?? string.Empty,
        JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => element.GetRawText(),
        _ => string.Empty
    };
}
