using GHelper.Hardware.Hp;
using System.Text.Json;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpFanMaxHoldCommandTests
{
    [Theory]
    [InlineData(HpFanMaxHoldCommand.HpVictusFlag)]
    [InlineData(HpFanMaxHoldCommand.ReadOnlyTestFlag)]
    [InlineData(HpFanMaxHoldCommand.AcknowledgementFlag)]
    [InlineData(HpFanMaxHoldCommand.ApprovalFlag)]
    public void Parse_MissingRequiredGate_FailsClosed(string missingFlag)
    {
        HpFanMaxHoldCommandResult result = HpFanMaxHoldCommand.Parse(ValidArgumentsWithout(missingFlag));

        Assert.True(result.ShouldExit);
        Assert.False(result.IsValidRequest);
        Assert.Contains(result.ValidationReasons, reason => reason.Contains(missingFlag, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("9")]
    [InlineData("181")]
    public void Parse_MissingMalformedOrOutOfRangeDuration_FailsClosed(string? duration)
    {
        string[] arguments = duration is null
            ? ValidArguments().Where(argument => !argument.StartsWith(HpFanMaxHoldCommand.HoldSecondsPrefix, StringComparison.Ordinal)).ToArray()
            : [.. ValidArguments().Where(argument => !argument.StartsWith(HpFanMaxHoldCommand.HoldSecondsPrefix, StringComparison.Ordinal)), HpFanMaxHoldCommand.HoldSecondsPrefix + duration];

        HpFanMaxHoldCommandResult result = HpFanMaxHoldCommand.Parse(arguments);

        Assert.False(result.IsValidRequest);
        Assert.Contains(result.ValidationReasons, reason => reason.Contains("pre-restore wait", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(HpFanMaxHoldCommand.MinimumHoldSeconds)]
    [InlineData(HpFanMaxHoldCommand.MaximumHoldSeconds)]
    public void Parse_InclusiveDurationBounds_AreAccepted(int seconds)
    {
        HpFanMaxHoldCommandResult result = HpFanMaxHoldCommand.Parse(ValidArguments(seconds));

        Assert.True(result.IsValidRequest);
        Assert.Equal(seconds, result.HoldSeconds);
    }

    [Fact]
    public void Parse_DuplicateDuration_FailsClosed()
    {
        HpFanMaxHoldCommandResult result = HpFanMaxHoldCommand.Parse(
            [.. ValidArguments(), HpFanMaxHoldCommand.HoldSecondsPrefix + "20"]);

        Assert.False(result.IsValidRequest);
        Assert.Contains(result.ValidationReasons, reason => reason.Contains("exactly once", StringComparison.Ordinal));
    }

    [Fact]
    public void Parse_RejectsPayloadSelectionAndUsesOnlyFourBytePulseMetadata()
    {
        HpFanMaxHoldCommandResult rejected = HpFanMaxHoldCommand.Parse(
            [.. ValidArguments(), "--set-fan-max-payload-length=1"]);
        HpFanMaxHoldCommandResult accepted = HpFanMaxHoldCommand.Parse(ValidArguments());

        Assert.False(rejected.IsValidRequest);
        Assert.Equal(HpFanResearchOperationKind.FourByteMaxFanPulse, accepted.Operation.Descriptor.Kind);
        Assert.Equal(HpFanResearchOperationStatus.DeveloperOnlyResearch, accepted.Operation.Descriptor.Status);
        Assert.Null(accepted.Operation.Descriptor.DeviceValidatedInputLength);
        Assert.Equal(HpFanMaxExperimentPayloadLengthCandidate.FourByteHypothesis, accepted.Payload.Candidate);
        Assert.Equal("01-00-00-00", accepted.Payload.EnableBytesHex);
        Assert.Equal("00-00-00-00", accepted.Payload.RestoreBytesHex);
    }

    [Fact]
    public void Parse_RejectsOtherExperimentModes()
    {
        HpFanMaxHoldCommandResult result = HpFanMaxHoldCommand.Parse(
            [.. ValidArguments(), HpFanMaxPulseCommand.PulseFlag]);

        Assert.False(result.IsValidRequest);
        Assert.Contains(result.ValidationReasons, reason => reason.Contains("cannot be combined", StringComparison.Ordinal));
    }

    [Fact]
    public void Run_ApprovedHold_UsesRequestedDelayAndOneFourBytePair()
    {
        var provider = new FixedReadOnlyProvider();
        var transport = new RecordingTransport(provider);
        var delay = new RecordingDelay();
        var runner = new HpFanMaxExperimentRunner(provider, transport, delay);
        HpFanMaxHoldCommandResult command = HpFanMaxHoldCommand.Parse(
        [
            .. ValidArguments(30),
            "--physical-fan-response-observed=true",
            "--restore-observed=true",
            "--manual-observation-notes=Bounded hold response and restore observed."
        ]);

        HpFanMaxExperimentRunResult result = runner.Run(command, ApprovedGates());
        HpFanMaxExperimentLogRecord record = command.CreateLogRecord(result);

        Assert.Equal(TimeSpan.FromSeconds(30), delay.Duration);
        Assert.Equal(["01-00-00-00", "00-00-00-00"], transport.Payloads);
        Assert.True(record.WriteExecuted);
        Assert.Equal(HpFanMaxHoldCommand.DeveloperOnlyOperationName, record.DeveloperOnlyOperation);
        Assert.Equal(30, record.RequestedHoldSeconds);
        using JsonDocument json = JsonDocument.Parse(HpFanMaxExperimentLogFormatter.Format(record));
        Assert.Equal(30, json.RootElement.GetProperty("RequestedHoldSeconds").GetInt32());
        Assert.Equal(30, json.RootElement.GetProperty("RequestedPreRestoreWaitSeconds").GetInt32());
        Assert.Contains("not validated", json.RootElement.GetProperty("HoldDurationSemantics").GetString()!);
        Assert.Equal(HpFanMaxExperimentReadbackReliability.Inconclusive, record.ReadbackReliability);
        Assert.Equal(
            HpFanMaxExperimentalOutcomeClassification.CommandSucceededPhysicalResponseObservedReadbackInconclusive,
            record.ExperimentalOutcomeClassification);
        Assert.Equal(HpFanMaxExperimentOutcome.Unknown, record.Outcome);
        Assert.Empty(record.BlockedReasons);
        Assert.Null(record.DeviceValidatedInputLength);
    }

    [Fact]
    public void CliSummary_ExplainsWaitAndUnvalidatedPhysicalDuration()
    {
        string summary = HpFanMaxHoldCommand.Parse(ValidArguments(10)).FormatCliSummary();

        Assert.Contains("Requested pre-restore wait seconds: 10", summary);
        Assert.Contains("--max-fan-hold-seconds", summary);
        Assert.Contains("not an exact restore deadline", summary);
        Assert.Contains("Physical fan duration is BIOS-dependent and not validated", summary);
        Assert.Contains("may remain high after restore or wait expiry", summary);
        Assert.Contains("Normal/user-facing fan control remains NO-GO", summary);
    }

    [Fact]
    public void OldHoldLog_PreservesRequestedWaitWithoutInferringPhysicalEvidence()
    {
        HpFanMaxExperimentLogRecord record = JsonSerializer.Deserialize<HpFanMaxExperimentLogRecord>(
            "{\"DeveloperOnlyOperation\":\"DeveloperOnlyFourByteMaxFanHold\",\"RequestedHoldSeconds\":10}")!;

        Assert.Equal(10, record.RequestedPreRestoreWaitSeconds);
        Assert.Contains("bounded pre-restore wait", record.HoldDurationSemantics!);
        Assert.Null(record.PhysicalFanResponseObserved);
        Assert.Null(record.RestoreObserved);
        Assert.Null(record.DeviceValidatedInputLength);
        Assert.False(record.FirstWriteGateSatisfied);
        HpFanMaxExperimentLogRecord missing = JsonSerializer.Deserialize<HpFanMaxExperimentLogRecord>("{}")!;
        Assert.Null(missing.RequestedPreRestoreWaitSeconds);
        Assert.Null(missing.HoldDurationSemantics);
    }

    [Fact]
    public void Observations_ChangeLogClassificationButNotPayloadsOrWait()
    {
        HpFanMaxHoldCommandResult plain = HpFanMaxHoldCommand.Parse(ValidArguments());
        HpFanMaxHoldCommandResult observed = HpFanMaxHoldCommand.Parse(
            [.. ValidArguments(), "--physical-fan-response-observed=true", "--restore-observed=true",
             "--manual-observation-notes=Operator report"]);
        var provider = new FixedReadOnlyProvider();
        var transport = new RecordingTransport(provider);
        var delay = new RecordingDelay();
        var runner = new HpFanMaxExperimentRunner(provider, transport, delay);

        HpFanMaxExperimentLogRecord plainLog = plain.CreateLogRecord(runner.Run(plain, ApprovedGates()));
        TimeSpan? plainWait = delay.Duration;
        HpFanMaxExperimentLogRecord observedLog = observed.CreateLogRecord(runner.Run(observed, ApprovedGates()));

        Assert.Equal(plainWait, delay.Duration);
        Assert.Equal(["01-00-00-00", "00-00-00-00", "01-00-00-00", "00-00-00-00"], transport.Payloads);
        Assert.Null(plainLog.PhysicalFanResponseObserved);
        Assert.Null(plainLog.RestoreObserved);
        Assert.Equal(HpFanMaxExperimentalOutcomeClassification.CommandSucceededNoPhysicalConfirmation, plainLog.ExperimentalOutcomeClassification);
        Assert.Equal(HpFanMaxExperimentOutcome.Fail, plainLog.Outcome);
        Assert.Equal(HpFanMaxExperimentalOutcomeClassification.CommandSucceededPhysicalResponseObservedReadbackInconclusive, observedLog.ExperimentalOutcomeClassification);
        Assert.Equal("Operator report", observedLog.ManualObservationNotes);
        Assert.Null(observedLog.DeviceValidatedInputLength);
    }

    [Fact]
    public void Observations_DoNotBypassHoldApprovalOrCauseWrites()
    {
        HpFanMaxHoldCommandResult command = HpFanMaxHoldCommand.Parse(
            [.. ValidArgumentsWithout(HpFanMaxHoldCommand.ApprovalFlag),
             "--physical-fan-response-observed=true", "--restore-observed=true"]);
        var provider = new FixedReadOnlyProvider();
        var transport = new RecordingTransport(provider);
        var delay = new RecordingDelay();
        var runner = new HpFanMaxExperimentRunner(provider, transport, delay);

        HpFanMaxExperimentLogRecord record = command.CreateLogRecord(runner.Run(command, ApprovedGates()));

        Assert.False(command.IsValidRequest);
        Assert.Empty(transport.Payloads);
        Assert.Null(delay.Duration);
        Assert.False(record.WriteExecuted);
        Assert.Equal(HpFanMaxExperimentalOutcomeClassification.BlockedBeforeWrite, record.ExperimentalOutcomeClassification);
        Assert.Null(record.DeviceValidatedInputLength);
    }

    [Fact]
    public void Run_DelayThrows_RestoreRemainsFinallyProtectedWithoutRetry()
    {
        var provider = new FixedReadOnlyProvider();
        var transport = new RecordingTransport(provider);
        var runner = new HpFanMaxExperimentRunner(provider, transport, new RecordingDelay(throwOnWait: true));

        HpFanMaxExperimentRunResult result = runner.Run(
            HpFanMaxHoldCommand.Parse(ValidArguments()),
            ApprovedGates());

        Assert.Equal(["01-00-00-00", "00-00-00-00"], transport.Payloads);
        Assert.True(result.EnableWrite.Attempted);
        Assert.True(result.RestoreWrite.Attempted);
        Assert.Equal(HpFanMaxExperimentOutcome.Fail, result.Outcome);
    }

    [Fact]
    public void Run_BlockedHold_DoesNotWriteAndKeepsInputLengthUnset()
    {
        var provider = new FixedReadOnlyProvider();
        var transport = new RecordingTransport(provider);
        var runner = new HpFanMaxExperimentRunner(provider, transport, new RecordingDelay());
        HpFanMaxHoldCommandResult command = HpFanMaxHoldCommand.Parse(ValidArguments());

        HpFanMaxExperimentRunResult result = runner.Run(
            command,
            ApprovedGates() with { IsAdministrator = false });
        HpFanMaxExperimentLogRecord record = command.CreateLogRecord(result);

        Assert.Empty(transport.Payloads);
        Assert.False(record.WriteExecuted);
        Assert.Null(record.DeviceValidatedInputLength);
    }

    private static string[] ValidArguments(int seconds = HpFanMaxHoldCommand.MinimumHoldSeconds) =>
    [
        HpFanMaxHoldCommand.HpVictusFlag,
        HpFanMaxHoldCommand.ReadOnlyTestFlag,
        HpFanMaxHoldCommand.HoldFlag,
        HpFanMaxHoldCommand.AcknowledgementFlag,
        HpFanMaxHoldCommand.ApprovalFlag,
        HpFanMaxHoldCommand.HoldSecondsPrefix + seconds
    ];

    private static string[] ValidArgumentsWithout(string excludedFlag) =>
        ValidArguments().Where(argument => !string.Equals(argument, excludedFlag, StringComparison.Ordinal)).ToArray();

    private static HpFanMaxExperimentRuntimeGates ApprovedGates() => new(true, true, true, true, true, false);

    private sealed class FixedReadOnlyProvider : IHpFanMaxExperimentReadOnlyProvider
    {
        public HpFanMaxExperimentBaseline CaptureBaseline() =>
            new(true, "Victus by HP Gaming Laptop 16-s0xxx", "7Z5Z2EA#AB8", "F.31", 1, 2, false, "22-25", true, []);

        public HpFanMaxExperimentFanReadback ReadFanStatus() => new(true, false, "33-00", null);
    }

    private sealed class RecordingTransport(FixedReadOnlyProvider provider) : IHpFanMaxExperimentWriteTransport
    {
        public List<string> Payloads { get; } = [];

        public HpFanMaxExperimentWriteResult TrySetFanMax(byte[] payload)
        {
            _ = provider;
            Payloads.Add(Convert.ToHexString(payload).Chunk(2).Select(static pair => new string(pair)).Aggregate(static (left, right) => left + "-" + right));
            return new HpFanMaxExperimentWriteResult(true, true, null);
        }
    }

    private sealed class RecordingDelay(bool throwOnWait = false) : IHpFanMaxExperimentDelay
    {
        public TimeSpan? Duration { get; private set; }

        public void WaitAfterEnable(TimeSpan duration)
        {
            Duration = duration;
            if (throwOnWait)
            {
                throw new InvalidOperationException("Injected wait failure.");
            }
        }
    }
}
