using GHelper.Hardware.Hp;
using System.Text.Json;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpFanMaxPulseHistoryLoaderTests
{
    [Fact]
    public void Load_MissingFolder_ReturnsSafeNoHistoryFallback()
    {
        HpFanMaxPulseHistoryLoadResult result = HpFanMaxPulseHistoryLoader.Load(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));

        Assert.Equal(HpFanMaxPulseHistoryLoadStatus.NotAvailable, result.Status);
        Assert.Null(result.Entry);
        Assert.Equal("No pulse history available", result.StatusText);
    }

    [Fact]
    public void Load_InvalidJson_ReturnsSafeFallback()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            File.WriteAllText(Path.Combine(directory, "truncated.json"), "{\"TimestampUtc\":");

            HpFanMaxPulseHistoryLoadResult result = HpFanMaxPulseHistoryLoader.Load(directory);

            Assert.Equal(HpFanMaxPulseHistoryLoadStatus.CouldNotBeRead, result.Status);
            Assert.Null(result.Entry);
            Assert.Equal(1, result.InvalidLogCount);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Load_SelectsLatestValidRelevantLogAndPreservesExperimentalClassification()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            File.WriteAllText(Path.Combine(directory, "older.json"), CreateLogJson("2026-09-02T10:00:00+00:00", "Older note"));
            File.WriteAllText(Path.Combine(directory, "latest.json"), CreateLogJson("2026-09-02T11:00:00+00:00", "Latest\nobservation"));
            File.WriteAllText(Path.Combine(directory, "other.json"), "{\"Command\":\"0x2E\"}");

            HpFanMaxPulseHistoryLoadResult result = HpFanMaxPulseHistoryLoader.Load(directory);
            HpFanMaxPulseHistoryEntry entry = Assert.IsType<HpFanMaxPulseHistoryEntry>(result.Entry);

            Assert.Equal(HpFanMaxPulseHistoryLoadStatus.Loaded, result.Status);
            Assert.Equal(new DateTimeOffset(2026, 9, 2, 11, 0, 0, TimeSpan.Zero), entry.TimestampUtc);
            Assert.Equal("FourByteHypothesis", entry.PayloadLengthCandidate);
            Assert.Equal("01-00-00-00", entry.PayloadBytesHypothesis);
            Assert.True(entry.WriteExecuted);
            Assert.True(entry.PhysicalFanResponseObserved);
            Assert.Equal("CommandSucceededPhysicalResponseObservedReadbackInconclusive", entry.ExperimentalOutcomeClassification);
            Assert.Equal("Latest observation", entry.NotesSummary);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Dashboard_ShowsPulseClassificationAndKeepsNormalControlDisabled()
    {
        HpDiagnosticDashboardSection history = Assert.Single(
            HpDiagnosticDashboardFormatter.BuildSections(new()
            {
                SetFanMaxPulseHistoryStatus = "Developer-only evidence - loaded locally; no pulse/run action is available.",
                SetFanMaxPulseTimestamp = "2026-09-02 11:00:00 +00:00",
                SetFanMaxPulsePayload = "FourByteHypothesis / 01-00-00-00",
                SetFanMaxPulseWriteExecuted = "True",
                SetFanMaxPulseEnableCommandSucceeded = "True",
                SetFanMaxPulseRestoreCommandSucceeded = "True",
                SetFanMaxPulsePhysicalFanResponseObserved = "True",
                SetFanMaxPulseRestoreObserved = "True",
                SetFanMaxPulseReadbackReliability = "Inconclusive",
                SetFanMaxPulseOutcomeClassification = "CommandSucceededPhysicalResponseObservedReadbackInconclusive",
                SetFanMaxPulseNotesSummary = "Fan response observed."
            }),
            section => section.Title == "Latest SetFanMax pulse/history");

        Assert.Contains(history.Rows, row => row.Label == "Experimental outcome classification" && row.Value == "CommandSucceededPhysicalResponseObservedReadbackInconclusive");
        Assert.Contains(history.Rows, row => row.Label == "Normal fan control" && row.Value == HpDiagnosticDashboardFormatter.PulseHistoryNormalControlDisabled);
    }

    [Fact]
    public void Loader_HasNoWmiOrHardwareInvocationSurface()
    {
        Type[] types = [typeof(HpFanMaxPulseHistoryLoader), typeof(HpFanMaxPulseHistoryLoadResult), typeof(HpFanMaxPulseHistoryEntry)];

        Assert.DoesNotContain(types.SelectMany(type => type.GetMethods()), method => method.Name.Contains("Invoke", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            types.SelectMany(type => type.GetMethods()).SelectMany(method => method.GetParameters()),
            parameter => parameter.ParameterType.Namespace?.Contains("Management", StringComparison.Ordinal) == true);
    }

    private static string CreateTemporaryDirectory()
    {
        string directory = Path.Combine(Path.GetTempPath(), "VictusX.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static string CreateLogJson(string timestamp, string notes) =>
        JsonSerializer.Serialize(new
        {
            TimestampUtc = timestamp,
            Command = "0x20008",
            CommandType = "0x27",
            WmiClass = "hpqBIntM",
            WmiMethod = "hpqBIOSInt0",
            PayloadLengthCandidate = "FourByteHypothesis",
            PayloadBytesHypothesis = "01-00-00-00",
            WriteExecuted = true,
            EnableCommandSucceeded = true,
            RestoreCommandSucceeded = true,
            PhysicalFanResponseObserved = true,
            RestoreObserved = true,
            ReadbackReliability = "Inconclusive",
            ExperimentalOutcomeClassification = "CommandSucceededPhysicalResponseObservedReadbackInconclusive",
            ManualObservationNotes = notes
        });
}
