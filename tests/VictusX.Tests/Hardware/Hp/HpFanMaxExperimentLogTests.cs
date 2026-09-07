using System.Text.Json;
using System.Text.Json.Serialization;
using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpFanMaxExperimentLogTests
{
    [Fact]
    public void BlockedRecord_IsAlwaysNoGoAndWriteDisabled()
    {
        HpFanMaxExperimentLogRecord record = HpFanMaxExperimentLogRecord.CreateBlocked();

        Assert.False(record.WriteExecuted);
        Assert.False(record.FirstWriteGateSatisfied);
        Assert.Null(record.DeviceValidatedInputLength);
        Assert.Equal(HpFanMaxExperimentOutcome.Unknown, record.Outcome);
        Assert.Equal(HpFanMaxExperimentalOutcomeClassification.BlockedBeforeWrite, record.ExperimentalOutcomeClassification);
        Assert.NotEmpty(record.BlockedReasons);
        Assert.Contains("NO-GO", record.BlockedReasons[0], StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(HpFanMaxExperimentPayloadLengthCandidate.OneByteHypothesis, "01")]
    [InlineData(HpFanMaxExperimentPayloadLengthCandidate.FourByteHypothesis, "01-00-00-00")]
    public void PayloadCandidates_AreRecordedAsHypothesesOnly(
        HpFanMaxExperimentPayloadLengthCandidate candidate,
        string payloadBytesHypothesis)
    {
        HpFanMaxExperimentLogRecord record = HpFanMaxExperimentLogRecord.CreateBlocked(candidate, payloadBytesHypothesis);

        Assert.Equal(candidate, record.PayloadLengthCandidate);
        Assert.Equal(payloadBytesHypothesis, record.PayloadBytesHypothesis);
        Assert.False(record.WriteExecuted);
        Assert.Null(record.DeviceValidatedInputLength);
    }

    [Fact]
    public void Formatter_SerializesRequiredFieldsAndSafeDefaults()
    {
        HpFanMaxExperimentLogRecord record = HpFanMaxExperimentLogRecord.CreateBlocked(
            HpFanMaxExperimentPayloadLengthCandidate.FourByteHypothesis,
            "01-00-00-00") with
        {
            Model = "HP Victus 16-s0035nt",
            Sku = "7Z5Z2EA#AB8",
            BiosVersion = "F.31",
            ThermalPolicyVersion = 1,
            BaselineFanGetCount = 2,
            BaselineFanMaxGet = false,
            BaselineFanGetLevelRaw = "17-00",
            BaselineCapturePerformed = true,
            BaselineCaptureResult = "Approved read-only baseline capture completed.",
            BaselineReadOnlyProbeSummary = ["FanMaxGet: attempted=True; succeeded=True; decodeSucceeded=True; returnedByteCount=4"]
        };

        using JsonDocument document = JsonDocument.Parse(HpFanMaxExperimentLogFormatter.Format(record));
        JsonElement root = document.RootElement;

        Assert.Equal("HP Victus 16-s0035nt", root.GetProperty("Model").GetString());
        Assert.Equal("0x20008", root.GetProperty("Command").GetString());
        Assert.Equal("0x27", root.GetProperty("CommandType").GetString());
        Assert.Equal("hpqBIntM", root.GetProperty("WmiClass").GetString());
        Assert.Equal("hpqBIOSInt0", root.GetProperty("WmiMethod").GetString());
        Assert.False(root.GetProperty("WriteExecuted").GetBoolean());
        Assert.False(root.GetProperty("FirstWriteGateSatisfied").GetBoolean());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("DeviceValidatedInputLength").ValueKind);
        Assert.Equal("FourByteHypothesis", root.GetProperty("PayloadLengthCandidate").GetString());
        Assert.Equal("01-00-00-00", root.GetProperty("PayloadBytesHypothesis").GetString());
        Assert.True(root.GetProperty("BaselineCapturePerformed").GetBoolean());
        Assert.Equal("Approved read-only baseline capture completed.", root.GetProperty("BaselineCaptureResult").GetString());
        Assert.Equal(JsonValueKind.Array, root.GetProperty("BaselineReadOnlyProbeSummary").ValueKind);
        Assert.Equal("Unknown", root.GetProperty("Outcome").GetString());
        Assert.Equal("BlockedBeforeWrite", root.GetProperty("ExperimentalOutcomeClassification").GetString());
        Assert.Equal(JsonValueKind.Array, root.GetProperty("BlockedReasons").ValueKind);
    }

    [Fact]
    public void Formatter_SerializesExplicitManualObservations()
    {
        HpFanMaxExperimentLogRecord record = HpFanMaxExperimentLogRecord.CreateBlocked() with
        {
            PhysicalFanResponseObserved = true,
            RestoreObserved = true,
            ManualObservationNotes = "Airflow increased; restore observed."
        };

        using JsonDocument document = JsonDocument.Parse(HpFanMaxExperimentLogFormatter.Format(record));
        JsonElement root = document.RootElement;

        Assert.True(root.GetProperty("PhysicalFanResponseObserved").GetBoolean());
        Assert.True(root.GetProperty("RestoreObserved").GetBoolean());
        Assert.Equal("Airflow increased; restore observed.", root.GetProperty("ManualObservationNotes").GetString());
        Assert.False(root.GetProperty("WriteExecuted").GetBoolean());
        Assert.Null(record.DeviceValidatedInputLength);
    }

    [Fact]
    public void WriterPath_StaysUnderVictusXLogsFanExperiments()
    {
        string appDataDirectory = HpDiagnosticPaths.BuildAppDataDirectory(Path.Combine("C:", "AppData", "Roaming"));
        string directory = HpFanMaxExperimentLogWriter.BuildExperimentDirectory(appDataDirectory);

        Assert.Equal(Path.Combine(appDataDirectory, "Logs", "FanExperiments"), directory);
    }

    [Fact]
    public void OlderLogWithoutOutcomeFields_DeserializesToBlockedSafeDefaults()
    {
        HpFanMaxExperimentLogRecord record = Assert.IsType<HpFanMaxExperimentLogRecord>(
            JsonSerializer.Deserialize<HpFanMaxExperimentLogRecord>(
                "{\"WriteExecuted\":false,\"Outcome\":\"Unknown\",\"BlockedReasons\":[\"legacy record\"]}",
                new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } }));

        Assert.Equal(HpFanMaxExperimentalOutcomeClassification.BlockedBeforeWrite, record.ExperimentalOutcomeClassification);
        Assert.Equal(HpFanMaxExperimentReadbackReliability.Unknown, record.ReadbackReliability);
        Assert.Null(record.DeviceValidatedInputLength);
    }

    [Fact]
    public void Infrastructure_HasNoWmiDependencyOrInvocationSurface()
    {
        Type[] types =
        [
            typeof(HpFanMaxExperimentLogRecord),
            typeof(HpFanMaxExperimentOutcomeClassifier),
            typeof(HpFanMaxExperimentLogFormatter),
            typeof(HpFanMaxExperimentLogWriter)
        ];

        Assert.DoesNotContain(
            types.SelectMany(type => type.GetMethods()),
            method => method.Name.Contains("Invoke", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            types.SelectMany(type => type.GetMethods()).SelectMany(method => method.GetParameters()),
            parameter => parameter.ParameterType.Namespace?.Contains("Management", StringComparison.Ordinal) == true);
    }
}
