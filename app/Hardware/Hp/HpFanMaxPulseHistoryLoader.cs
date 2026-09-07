using System.Text.Json;

namespace GHelper.Hardware.Hp;

public enum HpFanMaxPulseHistoryLoadStatus
{
    Loaded,
    NotAvailable,
    CouldNotBeRead
}

public sealed record HpFanMaxPulseHistoryEntry(
    DateTimeOffset TimestampUtc,
    string? PayloadLengthCandidate,
    string? PayloadBytesHypothesis,
    bool? WriteExecuted,
    bool? EnableCommandSucceeded,
    bool? PostEnableFanMaxGet,
    bool? RestoreCommandSucceeded,
    bool? PhysicalFanResponseObserved,
    bool? RestoreObserved,
    string? ReadbackReliability,
    string? ExperimentalOutcomeClassification,
    string? NotesSummary);

public sealed record HpFanMaxPulseHistoryEntriesLoadResult(
    HpFanMaxPulseHistoryLoadStatus Status,
    IReadOnlyList<HpFanMaxPulseHistoryEntry> Entries,
    int InvalidLogCount);

public sealed record HpFanMaxPulseHistoryLoadResult(
    HpFanMaxPulseHistoryLoadStatus Status,
    HpFanMaxPulseHistoryEntry? Entry,
    int InvalidLogCount)
{
    public string StatusText => Status switch
    {
        HpFanMaxPulseHistoryLoadStatus.Loaded => "Developer-only evidence - loaded locally; no pulse/run action is available.",
        HpFanMaxPulseHistoryLoadStatus.CouldNotBeRead => "No pulse history available - invalid local logs were ignored.",
        _ => "No pulse history available"
    };
}

public static class HpFanMaxPulseHistoryLoader
{
    private const int MaximumNotesLength = 240;

    public static HpFanMaxPulseHistoryLoadResult Load(string directoryPath)
    {
        HpFanMaxPulseHistoryEntriesLoadResult entriesResult = LoadAll(directoryPath);
        HpFanMaxPulseHistoryEntry? latest = entriesResult.Entries
            .OrderByDescending(entry => entry.TimestampUtc)
            .FirstOrDefault();

        return new HpFanMaxPulseHistoryLoadResult(entriesResult.Status, latest, entriesResult.InvalidLogCount);
    }

    public static HpFanMaxPulseHistoryEntriesLoadResult LoadAll(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
        {
            return new HpFanMaxPulseHistoryEntriesLoadResult(HpFanMaxPulseHistoryLoadStatus.NotAvailable, [], 0);
        }

        var entries = new List<HpFanMaxPulseHistoryEntry>();
        int invalidLogCount = 0;
        try
        {
            foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*.json", SearchOption.TopDirectoryOnly))
            {
                if (TryReadEntry(filePath, out HpFanMaxPulseHistoryEntry? entry) && entry is not null)
                {
                    entries.Add(entry);
                }
                else
                {
                    invalidLogCount++;
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return new HpFanMaxPulseHistoryEntriesLoadResult(HpFanMaxPulseHistoryLoadStatus.CouldNotBeRead, [], invalidLogCount);
        }

        return entries.Count > 0
            ? new HpFanMaxPulseHistoryEntriesLoadResult(HpFanMaxPulseHistoryLoadStatus.Loaded, entries, invalidLogCount)
            : new HpFanMaxPulseHistoryEntriesLoadResult(
                invalidLogCount > 0 ? HpFanMaxPulseHistoryLoadStatus.CouldNotBeRead : HpFanMaxPulseHistoryLoadStatus.NotAvailable,
                [],
                invalidLogCount);
    }

    private static bool TryReadEntry(string filePath, out HpFanMaxPulseHistoryEntry? entry)
    {
        entry = null;
        try
        {
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(filePath));
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !HasExactString(root, "Command", "0x20008") ||
                !HasExactString(root, "CommandType", "0x27") ||
                !HasExactString(root, "WmiClass", "hpqBIntM") ||
                !HasExactString(root, "WmiMethod", "hpqBIOSInt0") ||
                !DateTimeOffset.TryParse(GetString(root, "TimestampUtc"), out DateTimeOffset timestamp))
            {
                return false;
            }

            entry = new HpFanMaxPulseHistoryEntry(
                timestamp,
                GetString(root, "PayloadLengthCandidate"),
                GetString(root, "PayloadBytesHypothesis"),
                GetBoolean(root, "WriteExecuted"),
                GetBoolean(root, "EnableCommandSucceeded"),
                GetBoolean(root, "PostEnableFanMaxGet"),
                GetBoolean(root, "RestoreCommandSucceeded"),
                GetBoolean(root, "PhysicalFanResponseObserved"),
                GetBoolean(root, "RestoreObserved"),
                GetString(root, "ReadbackReliability"),
                GetString(root, "ExperimentalOutcomeClassification"),
                SanitizeNotes(GetString(root, "ManualObservationNotes")));
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return false;
        }
    }

    private static bool HasExactString(JsonElement root, string propertyName, string expectedValue) =>
        string.Equals(GetString(root, propertyName), expectedValue, StringComparison.OrdinalIgnoreCase);

    private static string? GetString(JsonElement root, string propertyName) =>
        root.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static bool? GetBoolean(JsonElement root, string propertyName) =>
        root.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? value.GetBoolean()
            : null;

    private static string? SanitizeNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return null;
        }

        string sanitized = new string(notes.Select(character => char.IsControl(character) ? ' ' : character).ToArray()).Trim();
        return sanitized.Length <= MaximumNotesLength ? sanitized : sanitized[..MaximumNotesLength];
    }
}
