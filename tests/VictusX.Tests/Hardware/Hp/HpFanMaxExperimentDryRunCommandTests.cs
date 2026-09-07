using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpFanMaxExperimentDryRunCommandTests
{
    [Fact]
    public void Parse_WhenDryRunIsNotRequested_DoesNotCreateARecord()
    {
        HpFanMaxExperimentDryRunCommandResult result = HpFanMaxExperimentDryRunCommand.Parse(["--hp-victus"]);

        Assert.False(result.IsRequested);
        Assert.False(result.ShouldExit);
        Assert.Null(result.LogRecord);
    }

    [Theory]
    [InlineData("1", HpFanMaxExperimentPayloadLengthCandidate.OneByteHypothesis, "01")]
    [InlineData("4", HpFanMaxExperimentPayloadLengthCandidate.FourByteHypothesis, "01-00-00-00")]
    public void Parse_ValidHypothesis_CreatesBlockedNoGoRecord(
        string payloadLength,
        HpFanMaxExperimentPayloadLengthCandidate expectedCandidate,
        string expectedPayload)
    {
        HpFanMaxExperimentDryRunCommandResult result = HpFanMaxExperimentDryRunCommand.Parse(
        [
            "--hp-victus",
            "--hp-fan-write-experiment-dry-run",
            "--set-fan-max-payload-length=" + payloadLength
        ]);

        HpFanMaxExperimentLogRecord record = Assert.IsType<HpFanMaxExperimentLogRecord>(result.LogRecord);
        Assert.True(result.IsRequested);
        Assert.True(result.ShouldExit);
        Assert.True(result.IsValidRequest);
        Assert.Equal(expectedCandidate, record.PayloadLengthCandidate);
        Assert.Equal(expectedPayload, record.PayloadBytesHypothesis);
        Assert.False(record.WriteExecuted);
        Assert.False(record.FirstWriteGateSatisfied);
        Assert.Null(record.DeviceValidatedInputLength);
        Assert.Contains("No WMI or hardware action was attempted.", record.ManualObservationNotes, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_MissingHpVictus_RejectsAndLogsBlockedRecord()
    {
        HpFanMaxExperimentDryRunCommandResult result = HpFanMaxExperimentDryRunCommand.Parse(
        ["--hp-fan-write-experiment-dry-run", "--set-fan-max-payload-length=4"]);

        HpFanMaxExperimentLogRecord record = Assert.IsType<HpFanMaxExperimentLogRecord>(result.LogRecord);
        Assert.False(result.IsValidRequest);
        Assert.Contains(record.BlockedReasons, reason => reason.Contains("--hp-victus is required", StringComparison.Ordinal));
        Assert.False(record.WriteExecuted);
    }

    [Theory]
    [InlineData()]
    [InlineData("--set-fan-max-payload-length=2")]
    [InlineData("--set-fan-max-payload-length=4", "--set-fan-max-payload-length=1")]
    public void Parse_MissingInvalidOrDuplicatePayloadLength_RejectsTheRequest(params string[] payloadArguments)
    {
        string[] arguments = ["--hp-victus", "--hp-fan-write-experiment-dry-run", .. payloadArguments];

        HpFanMaxExperimentDryRunCommandResult result = HpFanMaxExperimentDryRunCommand.Parse(arguments);

        HpFanMaxExperimentLogRecord record = Assert.IsType<HpFanMaxExperimentLogRecord>(result.LogRecord);
        Assert.False(result.IsValidRequest);
        Assert.Null(record.DeviceValidatedInputLength);
        Assert.NotEmpty(record.BlockedReasons);
    }

    [Fact]
    public void Parse_ReadOnlyTestFlag_IsRejectedBeforeAnyStartupPath()
    {
        HpFanMaxExperimentDryRunCommandResult result = HpFanMaxExperimentDryRunCommand.Parse(
        [
            "--hp-victus",
            "--hp-fan-write-experiment-dry-run",
            "--set-fan-max-payload-length=4",
            "--hp-wmi-readonly-test"
        ]);

        HpFanMaxExperimentLogRecord record = Assert.IsType<HpFanMaxExperimentLogRecord>(result.LogRecord);
        Assert.False(result.IsValidRequest);
        Assert.Contains(record.BlockedReasons, reason => reason.Contains("not permitted", StringComparison.Ordinal));
        Assert.False(record.WriteExecuted);
    }

    [Fact]
    public void Parser_HasNoWmiDependencyOrInvocationSurface()
    {
        Type commandType = typeof(HpFanMaxExperimentDryRunCommand);

        Assert.DoesNotContain(
            commandType.GetMethods(),
            method => method.Name.Contains("Invoke", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            commandType.GetMethods().SelectMany(method => method.GetParameters()),
            parameter => parameter.ParameterType.Namespace?.Contains("Management", StringComparison.Ordinal) == true);
    }
}
