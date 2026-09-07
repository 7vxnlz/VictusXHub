using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpFanMaxExperimentBaselineCaptureCommandTests
{
    [Theory]
    [InlineData("1", HpFanMaxExperimentPayloadLengthCandidate.OneByteHypothesis, "01")]
    [InlineData("4", HpFanMaxExperimentPayloadLengthCandidate.FourByteHypothesis, "01-00-00-00")]
    public void Parse_ValidReadOnlyBaselineRequest_AcceptsHypothesisOnly(
        string payloadLength,
        HpFanMaxExperimentPayloadLengthCandidate expectedCandidate,
        string expectedPayload)
    {
        HpFanMaxExperimentBaselineCaptureCommandResult result = HpFanMaxExperimentBaselineCaptureCommand.Parse(
        [
            "--hp-victus",
            "--hp-wmi-readonly-test",
            "--hp-fan-write-experiment-baseline",
            "--set-fan-max-payload-length=" + payloadLength
        ]);

        Assert.True(result.IsRequested);
        Assert.True(result.ShouldExit);
        Assert.True(result.IsValidRequest);
        Assert.Equal(expectedCandidate, result.PayloadLengthCandidate);
        Assert.Equal(expectedPayload, result.PayloadBytesHypothesis);
        Assert.Empty(result.ValidationReasons);
    }

    [Theory]
    [InlineData("--hp-wmi-readonly-test")]
    [InlineData("--hp-victus")]
    [InlineData("--hp-victus", "--hp-wmi-readonly-test", "--set-fan-max-payload-length=2")]
    public void Parse_MissingRequiredGateOrInvalidLength_RejectsRequest(params string[] suppliedArguments)
    {
        string[] arguments = ["--hp-fan-write-experiment-baseline", .. suppliedArguments];

        HpFanMaxExperimentBaselineCaptureCommandResult result = HpFanMaxExperimentBaselineCaptureCommand.Parse(arguments);

        Assert.True(result.IsRequested);
        Assert.True(result.ShouldExit);
        Assert.False(result.IsValidRequest);
        Assert.NotEmpty(result.ValidationReasons);
    }

    [Fact]
    public void Parse_DryRunCombination_IsRejected()
    {
        HpFanMaxExperimentBaselineCaptureCommandResult result = HpFanMaxExperimentBaselineCaptureCommand.Parse(
        [
            "--hp-victus",
            "--hp-wmi-readonly-test",
            "--hp-fan-write-experiment-baseline",
            "--hp-fan-write-experiment-dry-run",
            "--set-fan-max-payload-length=4"
        ]);

        Assert.False(result.IsValidRequest);
        Assert.Contains(result.ValidationReasons, reason => reason.Contains("cannot be combined", StringComparison.Ordinal));
    }

    [Fact]
    public void Parser_HasNoWmiDependencyOrInvocationSurface()
    {
        Type commandType = typeof(HpFanMaxExperimentBaselineCaptureCommand);

        Assert.DoesNotContain(
            commandType.GetMethods(),
            method => method.Name.Contains("Invoke", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            commandType.GetMethods().SelectMany(method => method.GetParameters()),
            parameter => parameter.ParameterType.Namespace?.Contains("Management", StringComparison.Ordinal) == true);
    }
}
