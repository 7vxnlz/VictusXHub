using GHelper.Hardware.Hp;
using System.Text.Json;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpFanProofGapAnalyzerTests
{
    [Fact]
    public void Analyze_MissingEvidence_ReturnsFailClosedGaps()
    {
        HpFanProofGapAnalysis result = HpFanProofGapAnalyzer.Analyze(
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")),
            null);

        Assert.Equal(HpFanProofGapAnalyzer.DeviceValidatedInputLengthUnset, result.DeviceValidatedInputLengthDecision);
        Assert.Contains("Inconclusive", result.FanMaxGetDecision, StringComparison.Ordinal);
        Assert.Equal(HpFanProofGapAnalyzer.FanGetLevelRawOnly, result.FanGetLevelDecision);
        Assert.Equal(HpFanProofGapAnalyzer.NormalFanControlNoGo, result.NormalFanControlDecision);
        Assert.Equal(0, result.ValidExperimentLogCount);
    }

    [Fact]
    public void Analyze_InvalidJson_IsIgnoredSafely()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            File.WriteAllText(Path.Combine(directory, "truncated.json"), "{\"TimestampUtc\":");

            HpFanProofGapAnalysis result = HpFanProofGapAnalyzer.Analyze(directory, null);

            Assert.Equal(0, result.ValidExperimentLogCount);
            Assert.Equal(1, result.InvalidExperimentLogCount);
            Assert.Contains("invalid ignored", result.EvidenceSources, StringComparison.Ordinal);
            Assert.Equal(HpFanProofGapAnalyzer.NormalFanControlNoGo, result.NormalFanControlDecision);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Analyze_ObservedPulseKeepsDeveloperEvidenceAndFanMaxGetInconclusive()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            File.WriteAllText(Path.Combine(directory, "pulse.json"), CreateObservedPulseJson());

            HpFanProofGapAnalysis result = HpFanProofGapAnalyzer.Analyze(directory, null);

            Assert.Equal(1, result.ValidExperimentLogCount);
            Assert.Equal(HpFanProofGapAnalyzer.DeveloperPulseOperational, result.DeveloperPulseDecision);
            Assert.Equal(HpFanProofGapAnalyzer.FanMaxGetInconclusive, result.FanMaxGetDecision);
            Assert.Equal(HpFanProofGapAnalyzer.FanGetLevelRawOnly, result.FanGetLevelDecision);
            Assert.Equal(HpFanProofGapAnalyzer.NormalFanControlNoGo, result.NormalFanControlDecision);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Analyze_ReportedInputLengthNeverPromotesValidation()
    {
        string directory = CreateTemporaryDirectory();
        string reportPath = Path.Combine(directory, "hp-capability-report.json");
        try
        {
            File.WriteAllText(reportPath, "{\"SetFanMaxDeviceValidatedInputLength\":4}");

            HpFanProofGapAnalysis result = HpFanProofGapAnalyzer.Analyze(
                directory,
                HpDiagnosticReportLoader.Load(reportPath));

            Assert.Equal(HpFanProofGapAnalyzer.DeviceValidatedInputLengthUnset, result.DeviceValidatedInputLengthDecision);
            Assert.Equal(HpFanProofGapAnalyzer.NormalFanControlNoGo, result.NormalFanControlDecision);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Analyzer_HasNoWmiOrControlInvocationSurface()
    {
        Type analyzerType = typeof(HpFanProofGapAnalyzer);

        Assert.DoesNotContain(analyzerType.GetMethods(), method => method.Name.Contains("Invoke", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            analyzerType.GetMethods().SelectMany(method => method.GetParameters()),
            parameter => parameter.ParameterType.Namespace?.Contains("Management", StringComparison.Ordinal) == true);
        Assert.DoesNotContain(analyzerType.GetMethods(), method => method.Name.Contains("SetFan", StringComparison.OrdinalIgnoreCase));
    }

    private static string CreateTemporaryDirectory()
    {
        string directory = Path.Combine(Path.GetTempPath(), "VictusX.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static string CreateObservedPulseJson() => JsonSerializer.Serialize(new
    {
        TimestampUtc = "2026-09-02T12:00:00+00:00",
        Command = "0x20008",
        CommandType = "0x27",
        WmiClass = "hpqBIntM",
        WmiMethod = "hpqBIOSInt0",
        PayloadLengthCandidate = "FourByteHypothesis",
        PayloadBytesHypothesis = "01-00-00-00",
        WriteExecuted = true,
        EnableCommandSucceeded = true,
        PostEnableFanMaxGet = false,
        RestoreCommandSucceeded = true,
        PhysicalFanResponseObserved = true,
        RestoreObserved = true,
        ReadbackReliability = "Inconclusive",
        ExperimentalOutcomeClassification = "CommandSucceededPhysicalResponseObservedReadbackInconclusive"
    });
}
