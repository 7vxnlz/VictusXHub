using System.Text.Json;

namespace GHelper.Hardware.Hp;

internal enum HpCapabilityEvidenceProvenance
{
    Unavailable,
    CurrentDecodedReadOnlyEvidence,
    HistoricalLocalEvidence,
    LegacyVictusXHistoricalEvidence
}

internal readonly record struct HpCapabilityEvidenceValue(byte? Value, HpCapabilityEvidenceProvenance Provenance)
{
    internal static HpCapabilityEvidenceValue Unavailable => new(null, HpCapabilityEvidenceProvenance.Unavailable);
}

// This is deliberately a narrow, read-only parser for already-recorded exact-device baseline
// evidence. It cannot invoke WMI, access hardware, or expose any fan-control operation.
internal sealed record HpHistoricalCapabilityEvidence(
    byte ThermalPolicyVersion,
    byte FanCount,
    HpCapabilityEvidenceProvenance Provenance);

internal static class HpHistoricalCapabilityEvidenceLoader
{
    private const string TargetSkuBase = "7Z5Z2EA";
    private const string TargetBios = "F.31";
    private const byte ProvenThermalPolicyVersion = 1;
    private const byte ProvenFanCount = 2;

    public static HpHistoricalCapabilityEvidence? Load(string currentAppDataDirectory, string legacyAppDataDirectory)
    {
        HpHistoricalCapabilityEvidence? current = LoadDirectory(currentAppDataDirectory, HpCapabilityEvidenceProvenance.HistoricalLocalEvidence);
        return current ?? LoadDirectory(legacyAppDataDirectory, HpCapabilityEvidenceProvenance.LegacyVictusXHistoricalEvidence);
    }

    public static HpCapabilityEvidenceValue ResolveThermalPolicy(byte? currentDecoded, HpHistoricalCapabilityEvidence? historical) =>
        currentDecoded.HasValue
            ? new HpCapabilityEvidenceValue(currentDecoded, HpCapabilityEvidenceProvenance.CurrentDecodedReadOnlyEvidence)
            : historical is { } evidence
                ? new HpCapabilityEvidenceValue(evidence.ThermalPolicyVersion, evidence.Provenance)
                : HpCapabilityEvidenceValue.Unavailable;

    public static HpCapabilityEvidenceValue ResolveFanCount(byte? currentDecoded, HpHistoricalCapabilityEvidence? historical) =>
        currentDecoded.HasValue
            ? new HpCapabilityEvidenceValue(currentDecoded, HpCapabilityEvidenceProvenance.CurrentDecodedReadOnlyEvidence)
            : historical is { } evidence
                ? new HpCapabilityEvidenceValue(evidence.FanCount, evidence.Provenance)
                : HpCapabilityEvidenceValue.Unavailable;

    private static HpHistoricalCapabilityEvidence? LoadDirectory(string appDataDirectory, HpCapabilityEvidenceProvenance provenance)
    {
        if (string.IsNullOrWhiteSpace(appDataDirectory)) return null;

        string directory = Path.Combine(appDataDirectory, "Logs", "FanExperiments");
        if (!Directory.Exists(directory)) return null;

        DateTimeOffset latestTimestamp = DateTimeOffset.MinValue;
        HpHistoricalCapabilityEvidence? latest = null;
        try
        {
            foreach (string filePath in Directory.EnumerateFiles(directory, "*.json", SearchOption.TopDirectoryOnly))
            {
                if (!TryRead(filePath, provenance, out HpHistoricalCapabilityEvidence? evidence, out DateTimeOffset timestamp) ||
                    timestamp <= latestTimestamp)
                {
                    continue;
                }

                latestTimestamp = timestamp;
                latest = evidence;
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }

        return latest;
    }

    private static bool TryRead(string filePath, HpCapabilityEvidenceProvenance provenance,
        out HpHistoricalCapabilityEvidence? evidence, out DateTimeOffset timestamp)
    {
        evidence = null;
        timestamp = DateTimeOffset.MinValue;
        try
        {
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(filePath));
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !DateTimeOffset.TryParse(GetString(root, "TimestampUtc"), out timestamp) ||
                !HasExactTargetIdentity(root) ||
                !HasProvenBaseline(root) ||
                !HasProvenReadOnlySummaries(root))
            {
                return false;
            }

            evidence = new HpHistoricalCapabilityEvidence(ProvenThermalPolicyVersion, ProvenFanCount, provenance);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return false;
        }
    }

    private static bool HasExactTargetIdentity(JsonElement root)
    {
        string? sku = GetString(root, "Sku");
        int hash = sku?.IndexOf('#') ?? -1;
        string skuBase = (hash >= 0 ? sku![..hash] : sku)?.Trim() ?? string.Empty;
        return string.Equals(skuBase, TargetSkuBase, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(GetString(root, "BiosVersion")?.Trim(), TargetBios, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasProvenBaseline(JsonElement root) =>
        GetBoolean(root, "BaselineCapturePerformed") == true &&
        GetByte(root, "ThermalPolicyVersion") == ProvenThermalPolicyVersion &&
        GetByte(root, "BaselineFanGetCount") == ProvenFanCount;

    private static bool HasProvenReadOnlySummaries(JsonElement root)
    {
        if (!root.TryGetProperty("BaselineReadOnlyProbeSummary", out JsonElement summaries) || summaries.ValueKind != JsonValueKind.Array)
            return false;

        string[] values = summaries.EnumerateArray()
            .Where(value => value.ValueKind == JsonValueKind.String)
            .Select(value => value.GetString() ?? string.Empty)
            .ToArray();
        return values.Any(IsSuccessfulSystemDesignData) && values.Any(IsSuccessfulFanGetCount);
    }

    private static bool IsSuccessfulSystemDesignData(string value) =>
        value.StartsWith("SystemDesignData:", StringComparison.Ordinal) && HasSuccessfulDecode(value);

    private static bool IsSuccessfulFanGetCount(string value) =>
        value.StartsWith("FanGetCount:", StringComparison.Ordinal) && HasSuccessfulDecode(value);

    private static bool HasSuccessfulDecode(string value) =>
        value.Contains("attempted=True", StringComparison.Ordinal) &&
        value.Contains("succeeded=True", StringComparison.Ordinal) &&
        value.Contains("decodeSucceeded=True", StringComparison.Ordinal);

    private static string? GetString(JsonElement root, string propertyName) =>
        root.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static bool? GetBoolean(JsonElement root, string propertyName) =>
        root.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? value.GetBoolean()
            : null;

    private static byte? GetByte(JsonElement root, string propertyName) =>
        root.TryGetProperty(propertyName, out JsonElement value) && value.TryGetByte(out byte result) ? result : null;
}
