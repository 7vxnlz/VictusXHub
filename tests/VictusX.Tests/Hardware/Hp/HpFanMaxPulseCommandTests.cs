using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpFanMaxPulseCommandTests
{
    [Fact]
    public void Parse_MissingPulseApproval_FailsClosed()
    {
        HpFanMaxPulseCommandResult result = HpFanMaxPulseCommand.Parse(ValidArgumentsWithout(HpFanMaxPulseCommand.ApprovalFlag));

        Assert.True(result.ShouldExit);
        Assert.False(result.IsValidRequest);
        Assert.Contains(result.ValidationReasons, reason => reason.Contains(HpFanMaxPulseCommand.ApprovalFlag, StringComparison.Ordinal));
    }

    [Fact]
    public void Parse_MissingReadOnlyTestFlag_FailsClosed()
    {
        HpFanMaxPulseCommandResult result = HpFanMaxPulseCommand.Parse(ValidArgumentsWithout(HpFanMaxPulseCommand.ReadOnlyTestFlag));

        Assert.False(result.IsValidRequest);
        Assert.Contains(result.ValidationReasons, reason => reason.Contains(HpFanMaxPulseCommand.ReadOnlyTestFlag, StringComparison.Ordinal));
    }

    [Fact]
    public void Parse_RejectsPayloadSelectionAndAlwaysMapsOnlyFourBytePair()
    {
        HpFanMaxPulseCommandResult rejected = HpFanMaxPulseCommand.Parse([.. ValidArguments(), "--set-fan-max-payload-length=1"]);
        HpFanMaxPulseCommandResult accepted = HpFanMaxPulseCommand.Parse(ValidArguments());

        Assert.False(rejected.IsValidRequest);
        Assert.Equal(HpFanResearchOperationKind.FourByteMaxFanPulse, accepted.Operation.Descriptor.Kind);
        Assert.Equal(HpFanResearchOperationStatus.DeveloperOnlyResearch, accepted.Operation.Descriptor.Status);
        Assert.Null(accepted.Operation.Descriptor.DeviceValidatedInputLength);
        HpFanMaxExperimentPayload payload = accepted.CreateRunnerCommand().Payload!;
        Assert.Equal(HpFanMaxExperimentPayloadLengthCandidate.FourByteHypothesis, payload.Candidate);
        Assert.Equal("01-00-00-00", payload.EnableBytesHex);
        Assert.Equal("00-00-00-00", payload.RestoreBytesHex);
    }

    [Fact]
    public void Parse_ObservationsDoNotBypassPulseApproval()
    {
        HpFanMaxPulseCommandResult result = HpFanMaxPulseCommand.Parse(
        [
            "--hp-victus",
            "--hp-wmi-readonly-test",
            "--hp-fan-max-pulse",
            "--i-understand-this-can-affect-fans",
            "--physical-fan-response-observed=true",
            "--restore-observed=true",
            "--manual-observation-notes=Observed airflow increase"
        ]);

        Assert.False(result.IsValidRequest);
        Assert.True(result.ManualObservation.PhysicalFanResponseObserved);
        Assert.True(result.ManualObservation.RestoreObserved);
    }

    [Fact]
    public void Run_ApprovedPulse_UsesOneFourBytePairWithoutRetry()
    {
        var transport = new RecordingTransport();
        var runner = new HpFanMaxExperimentRunner(new FixedReadOnlyProvider(), transport, new NoDelay());
        HpFanMaxPulseCommandResult command = HpFanMaxPulseCommand.Parse(ValidArguments());
        HpFanMaxExperimentRunResult result = runner.Run(
            command,
            new HpFanMaxExperimentRuntimeGates(true, true, true, true, true, false));

        HpFanMaxExperimentLogRecord record = HpFanMaxExperimentRunLogMapper.Create(
            result,
            new HpFanMaxExperimentManualObservation(true, true, "Fan response and restore observed.", []));

        Assert.Equal(["01-00-00-00", "00-00-00-00"], transport.Payloads);
        Assert.True(record.WriteExecuted);
        Assert.Null(record.DeviceValidatedInputLength);
    }

    [Fact]
    public void Run_BlockedPulse_KeepsWriteDisabled()
    {
        var transport = new RecordingTransport();
        var runner = new HpFanMaxExperimentRunner(new FixedReadOnlyProvider(), transport, new NoDelay());
        HpFanMaxPulseCommandResult command = HpFanMaxPulseCommand.Parse(ValidArguments());
        HpFanMaxExperimentRunResult result = runner.Run(
            command,
            new HpFanMaxExperimentRuntimeGates(false, true, true, true, true, false));

        HpFanMaxExperimentLogRecord record = HpFanMaxExperimentRunLogMapper.Create(result);

        Assert.Empty(transport.Payloads);
        Assert.False(record.WriteExecuted);
        Assert.Null(record.DeviceValidatedInputLength);
    }

    private static string[] ValidArguments() =>
    [
        "--hp-victus",
        "--hp-wmi-readonly-test",
        "--hp-fan-max-pulse",
        "--i-understand-this-can-affect-fans",
        "--i-approve-4-byte-max-fan-pulse"
    ];

    private static string[] ValidArgumentsWithout(string excludedFlag) =>
        ValidArguments().Where(argument => !string.Equals(argument, excludedFlag, StringComparison.Ordinal)).ToArray();

    private sealed class FixedReadOnlyProvider : IHpFanMaxExperimentReadOnlyProvider
    {
        private bool _enabled;

        public HpFanMaxExperimentBaseline CaptureBaseline() =>
            new(true, "Victus by HP Gaming Laptop 16-s0xxx", "7Z5Z2EA#AB8", "F.31", 1, 2, false, "22-25", true, []);

        public HpFanMaxExperimentFanReadback ReadFanStatus() => new(true, _enabled, "22-25", null);

        public void SetEnabled(bool enabled) => _enabled = enabled;
    }

    private sealed class RecordingTransport : IHpFanMaxExperimentWriteTransport
    {
        public List<string> Payloads { get; } = [];

        public HpFanMaxExperimentWriteResult TrySetFanMax(byte[] payload)
        {
            Payloads.Add(Convert.ToHexString(payload).Chunk(2).Select(static pair => new string(pair)).Aggregate(static (left, right) => left + "-" + right));
            return new HpFanMaxExperimentWriteResult(true, true, null);
        }
    }

    private sealed class NoDelay : IHpFanMaxExperimentDelay
    {
        public void WaitAfterEnable(TimeSpan duration) { }
    }
}
