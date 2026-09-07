using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpFanMaxExperimentRunnerTests
{
    [Fact]
    public void Parse_MissingFlagsAndAcknowledgement_FailsClosed()
    {
        HpFanMaxExperimentRunnerCommandResult result = HpFanMaxExperimentRunnerCommand.Parse(
            ["--hp-fan-write-experiment", "--set-fan-max-payload-length=4"]);

        Assert.True(result.IsRequested);
        Assert.False(result.IsValidRequest);
        Assert.Contains(result.ValidationReasons, reason => reason.Contains("--hp-victus", StringComparison.Ordinal));
        Assert.Contains(result.ValidationReasons, reason => reason.Contains("--i-understand-this-can-affect-fans", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("--set-fan-max-payload-length=2")]
    [InlineData("--set-fan-max-payload-length=1", "--set-fan-max-payload-length=4")]
    public void Parse_InvalidOrAmbiguousPayloadLength_FailsClosed(params string[] payloadArguments)
    {
        HpFanMaxExperimentRunnerCommandResult result = HpFanMaxExperimentRunnerCommand.Parse(
        [
            "--hp-victus",
            "--hp-wmi-readonly-test",
            "--hp-fan-write-experiment",
            "--i-understand-this-can-affect-fans",
            .. payloadArguments
        ]);

        Assert.False(result.IsValidRequest);
        Assert.Null(result.Payload);
    }

    [Theory]
    [InlineData("1", "01", "00")]
    [InlineData("4", "01-00-00-00", "00-00-00-00")]
    public void Parse_PayloadHypotheses_MapOnlyMatchingPayloadPair(string length, string enable, string restore)
    {
        HpFanMaxExperimentRunnerCommandResult result = HpFanMaxExperimentRunnerCommand.Parse(
            ValidArguments(length, includeFourByteApprovals: length == "4", includeOneByteComparisonApproval: length == "1"));

        HpFanMaxExperimentPayload payload = Assert.IsType<HpFanMaxExperimentPayload>(result.Payload);
        Assert.True(result.IsValidRequest);
        Assert.Equal(enable, payload.EnableBytesHex);
        Assert.Equal(restore, payload.RestoreBytesHex);
    }

    [Fact]
    public void Parse_FourByteWithoutOneTimeApproval_FailsClosedWithExactReason()
    {
        HpFanMaxExperimentRunnerCommandResult result = HpFanMaxExperimentRunnerCommand.Parse(ValidArguments("4"));

        Assert.False(result.IsValidRequest);
        Assert.Contains(result.ValidationReasons, reason => reason.Contains(HpFanMaxExperimentRunnerCommand.OneTimeFourByteApprovalFlag, StringComparison.Ordinal));
    }

    [Fact]
    public void Parse_SecondFourByteConfirmationWithoutSecondApproval_FailsClosedWithExactReason()
    {
        HpFanMaxExperimentRunnerCommandResult result = HpFanMaxExperimentRunnerCommand.Parse(ValidArguments("4", includeFirstFourByteApproval: true));

        Assert.False(result.IsValidRequest);
        Assert.Contains(result.ValidationReasons, reason => reason.Contains(HpFanMaxExperimentRunnerCommand.SecondFourByteConfirmationApprovalFlag, StringComparison.Ordinal));
    }

    [Fact]
    public void Parse_OneByteWithoutComparisonApproval_FailsClosedWithExactReason()
    {
        HpFanMaxExperimentRunnerCommandResult result = HpFanMaxExperimentRunnerCommand.Parse(ValidArguments("1"));

        Assert.False(result.IsValidRequest);
        Assert.Contains(result.ValidationReasons, reason => reason.Contains(HpFanMaxExperimentRunnerCommand.OneTimeOneByteComparisonApprovalFlag, StringComparison.Ordinal));
    }

    [Fact]
    public void Parse_ObservationsDoNotBypassRequiredApproval()
    {
        HpFanMaxExperimentRunnerCommandResult result = HpFanMaxExperimentRunnerCommand.Parse(
        [
            "--hp-victus",
            "--hp-wmi-readonly-test",
            "--hp-fan-write-experiment",
            "--set-fan-max-payload-length=1",
            "--i-understand-this-can-affect-fans",
            "--physical-fan-response-observed=true",
            "--restore-observed=true",
            "--manual-observation-notes=Observed airflow increase"
        ]);

        Assert.False(result.IsValidRequest);
        Assert.True(result.ManualObservation.PhysicalFanResponseObserved);
        Assert.True(result.ManualObservation.RestoreObserved);
        Assert.Contains(result.ValidationReasons, reason => reason.Contains(HpFanMaxExperimentRunnerCommand.OneTimeOneByteComparisonApprovalFlag, StringComparison.Ordinal));
    }

    [Fact]
    public void Parse_LongManualNotes_AreSanitizedAndBounded()
    {
        string notes = new string('a', HpFanMaxExperimentManualObservation.MaximumNotesLength + 10) + "\nignored";
        HpFanMaxExperimentRunnerCommandResult result = HpFanMaxExperimentRunnerCommand.Parse(
            [.. ValidArguments("4", includeFourByteApprovals: true), "--manual-observation-notes=" + notes]);

        Assert.True(result.IsValidRequest);
        string storedNotes = Assert.IsType<string>(result.ManualObservation.ManualObservationNotes);
        Assert.Equal(HpFanMaxExperimentManualObservation.MaximumNotesLength, storedNotes.Length);
        Assert.DoesNotContain('\n', storedNotes);
    }

    [Fact]
    public void LogMapper_UsesManualResponseForInconclusiveReadbackClassification()
    {
        var transport = new RecordingTransport();
        HpFanMaxExperimentRunResult result = CreateRunner(transport).Run(ValidCommand("4"), ApprovedGates());
        HpFanMaxExperimentLogRecord record = HpFanMaxExperimentRunLogMapper.Create(
            result,
            new HpFanMaxExperimentManualObservation(true, true, "Fan ramp observed and restore observed.", [])) with
            {
                FanMaxGetConfirmedEnable = false,
                PostEnableFanMaxGet = false
            };

        Assert.True(record.PhysicalFanResponseObserved);
        Assert.True(record.RestoreObserved);
        Assert.Equal(
            HpFanMaxExperimentalOutcomeClassification.CommandSucceededPhysicalResponseObservedReadbackInconclusive,
            record.ExperimentalOutcomeClassification);
        Assert.Null(record.DeviceValidatedInputLength);
    }

    [Fact]
    public void LogMapper_MissingManualObservations_LeavesPhysicalResponseUnknown()
    {
        var transport = new RecordingTransport();
        HpFanMaxExperimentLogRecord record = HpFanMaxExperimentRunLogMapper.Create(
            CreateRunner(transport).Run(ValidCommand("4"), ApprovedGates()));

        Assert.Null(record.PhysicalFanResponseObserved);
        Assert.Null(record.RestoreObserved);
        Assert.Equal(HpFanMaxExperimentalOutcomeClassification.CommandSucceededNoPhysicalConfirmation, record.ExperimentalOutcomeClassification);
        Assert.Null(record.DeviceValidatedInputLength);
    }

    [Fact]
    public void Run_ApprovedSecondFourByteConfirmation_PassesTheHumanApprovalGatesInTestDouble()
    {
        var transport = new RecordingTransport();
        HpFanMaxExperimentRunResult result = CreateRunner(transport).Run(ValidCommand("4"), ApprovedGates());

        Assert.Equal(HpFanMaxExperimentOutcome.Pass, result.Outcome);
        Assert.Equal(["01-00-00-00", "00-00-00-00"], transport.Payloads);
        Assert.Null(HpFanMaxExperimentRunLogMapper.Create(result).DeviceValidatedInputLength);
    }

    [Fact]
    public void Run_ApprovedOneByteComparison_PassesOnlyItsOwnApprovalGateInTestDouble()
    {
        var transport = new RecordingTransport();
        HpFanMaxExperimentRunResult result = CreateRunner(transport).Run(ValidCommand("1"), ApprovedGates());

        Assert.Equal(HpFanMaxExperimentOutcome.Pass, result.Outcome);
        Assert.Equal(["01", "00"], transport.Payloads);
        Assert.Null(HpFanMaxExperimentRunLogMapper.Create(result).DeviceValidatedInputLength);
    }

    [Fact]
    public void Parse_OneByteWithSecondFourByteApproval_FailsClosed()
    {
        HpFanMaxExperimentRunnerCommandResult result = HpFanMaxExperimentRunnerCommand.Parse(
            ValidArguments("1", includeSecondFourByteApproval: true, includeOneByteComparisonApproval: true));

        Assert.False(result.IsValidRequest);
        Assert.Contains(result.ValidationReasons, reason => reason.Contains("4-byte hypothesis only", StringComparison.Ordinal));
    }

    [Fact]
    public void Run_OneByteComparisonWithoutRuntimeApproval_FailsClosedBeforeAnyWrite()
    {
        var transport = new RecordingTransport();
        HpFanMaxExperimentRunResult result = CreateRunner(transport).Run(
            ValidCommand("1"),
            ApprovedGates() with { HasOneTimeOneByteComparisonApproval = false });

        Assert.Equal(HpFanMaxExperimentOutcome.Unknown, result.Outcome);
        Assert.Empty(transport.Payloads);
        Assert.Contains(result.BlockedReasons, reason => reason.Contains("one-byte comparison", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Parse_FourByteWithOneByteComparisonApproval_FailsClosed()
    {
        HpFanMaxExperimentRunnerCommandResult result = HpFanMaxExperimentRunnerCommand.Parse(
            ValidArguments("4", includeFourByteApprovals: true, includeOneByteComparisonApproval: true));

        Assert.False(result.IsValidRequest);
        Assert.Contains(result.ValidationReasons, reason => reason.Contains("one-byte comparison approval", StringComparison.Ordinal));
    }

    [Fact]
    public void Run_NonTargetIdentity_FailsClosedBeforeAnyWrite()
    {
        var transport = new RecordingTransport();
        HpFanMaxExperimentRunResult result = CreateRunner(transport, CreateBaseline() with { Sku = "other" }).Run(ValidCommand("4"), ApprovedGates());

        Assert.Equal(HpFanMaxExperimentOutcome.Unknown, result.Outcome);
        Assert.Empty(transport.Payloads);
        Assert.Contains(result.BlockedReasons, reason => reason.Contains("SKU", StringComparison.Ordinal));
    }

    [Fact]
    public void Run_CurrentNoGoGate_FailsClosedBeforeAnyWrite()
    {
        var transport = new RecordingTransport();
        HpFanMaxExperimentRunResult result = CreateRunner(transport).Run(ValidCommand("4"), ApprovedGates() with { IsFirstWriteGateApproved = false });

        Assert.False(result.EnableWrite.Attempted);
        Assert.False(result.RestoreWrite.Attempted);
        Assert.Empty(transport.Payloads);
        Assert.Contains(result.BlockedReasons, reason => reason.Contains("NO-GO", StringComparison.Ordinal));
    }

    [Fact]
    public void Run_MissingSecondConfirmationRuntimeApproval_FailsClosedBeforeAnyWrite()
    {
        var transport = new RecordingTransport();
        HpFanMaxExperimentRunResult result = CreateRunner(transport).Run(
            ValidCommand("4"),
            ApprovedGates() with { HasSecondFourByteConfirmationApproval = false });

        Assert.Equal(HpFanMaxExperimentOutcome.Unknown, result.Outcome);
        Assert.Empty(transport.Payloads);
        Assert.Contains(result.BlockedReasons, reason => reason.Contains("second 4-byte confirmation", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Parse_MixedBaselineOrDryRunFlag_FailsClosed()
    {
        HpFanMaxExperimentRunnerCommandResult result = HpFanMaxExperimentRunnerCommand.Parse(
        [
            .. ValidArguments("4"),
            "--hp-fan-write-experiment-baseline"
        ]);

        Assert.False(result.IsValidRequest);
        Assert.Contains(result.ValidationReasons, reason => reason.Contains("cannot be combined", StringComparison.Ordinal));
    }

    [Fact]
    public void Run_ApprovedTestDouble_UsesOnlySelectedPayloadAndMatchingRestore()
    {
        var transport = new RecordingTransport();
        HpFanMaxExperimentRunResult result = CreateRunner(transport).Run(ValidCommand("4"), ApprovedGates());

        Assert.Equal(HpFanMaxExperimentOutcome.Pass, result.Outcome);
        Assert.Equal(["01-00-00-00", "00-00-00-00"], transport.Payloads);
        Assert.True(result.EnableWrite.Attempted);
        Assert.True(result.RestoreWrite.Attempted);
    }

    [Fact]
    public void Run_EnableTransportThrows_RestoresUsingTheMatchingPayload()
    {
        var transport = new ThrowingEnableTransport();
        HpFanMaxExperimentRunResult result = CreateRunner(transport).Run(ValidCommand("4"), ApprovedGates());

        Assert.Equal(HpFanMaxExperimentOutcome.Fail, result.Outcome);
        Assert.Equal(["01-00-00-00", "00-00-00-00"], transport.Payloads);
        Assert.True(result.EnableWrite.Attempted);
        Assert.True(result.RestoreWrite.Attempted);
    }

    [Fact]
    public void BlockedLog_KeepsWriteExecutedFalseAndInputLengthUnset()
    {
        var transport = new RecordingTransport();
        HpFanMaxExperimentRunResult result = CreateRunner(transport).Run(ValidCommand("4"), ApprovedGates() with { IsFirstWriteGateApproved = false });
        HpFanMaxExperimentLogRecord record = HpFanMaxExperimentRunLogMapper.Create(result);

        Assert.False(record.WriteExecuted);
        Assert.Null(record.DeviceValidatedInputLength);
    }

    [Fact]
    public void ActualExperimentResult_CanRecordWriteExecutedOnlyWhenTransportWasAttempted()
    {
        var transport = new RecordingTransport();
        HpFanMaxExperimentLogRecord record = HpFanMaxExperimentRunLogMapper.Create(
            CreateRunner(transport).Run(ValidCommand("4"), ApprovedGates()));

        Assert.True(record.WriteExecuted);
        Assert.False(record.FirstWriteGateSatisfied);
        Assert.Null(record.DeviceValidatedInputLength);
    }

    private static HpFanMaxExperimentRunner CreateRunner(RecordingTransport transport, HpFanMaxExperimentBaseline? baseline = null)
    {
        var provider = new FixedReadOnlyProvider(baseline ?? CreateBaseline());
        transport.Attach(provider);
        return new HpFanMaxExperimentRunner(provider, transport, new NoDelay());
    }

    private static HpFanMaxExperimentRunner CreateRunner(ThrowingEnableTransport transport, HpFanMaxExperimentBaseline? baseline = null)
    {
        var provider = new FixedReadOnlyProvider(baseline ?? CreateBaseline());
        transport.Attach(provider);
        return new HpFanMaxExperimentRunner(provider, transport, new NoDelay());
    }

    private static HpFanMaxExperimentRunnerCommandResult ValidCommand(string length) =>
        HpFanMaxExperimentRunnerCommand.Parse(
            ValidArguments(length, includeFourByteApprovals: length == "4", includeOneByteComparisonApproval: length == "1"));

    private static string[] ValidArguments(
        string length,
        bool includeFirstFourByteApproval = false,
        bool includeSecondFourByteApproval = false,
        bool includeFourByteApprovals = false,
        bool includeOneByteComparisonApproval = false)
    {
        var arguments = new List<string>
        {
        "--hp-victus",
        "--hp-wmi-readonly-test",
        "--hp-fan-write-experiment",
        "--set-fan-max-payload-length=" + length,
        "--i-understand-this-can-affect-fans"
        };

        if (includeFourByteApprovals || includeFirstFourByteApproval)
        {
            arguments.Add(HpFanMaxExperimentRunnerCommand.OneTimeFourByteApprovalFlag);
        }

        if (includeFourByteApprovals || includeSecondFourByteApproval)
        {
            arguments.Add(HpFanMaxExperimentRunnerCommand.SecondFourByteConfirmationApprovalFlag);
        }

        if (includeOneByteComparisonApproval)
        {
            arguments.Add(HpFanMaxExperimentRunnerCommand.OneTimeOneByteComparisonApprovalFlag);
        }

        return arguments.ToArray();
    }

    private static HpFanMaxExperimentRuntimeGates ApprovedGates() => new(true, true, true, true, true, true);

    private static HpFanMaxExperimentBaseline CreateBaseline() =>
        new(true, "Victus by HP Gaming Laptop 16-s0xxx", "7Z5Z2EA#AB8", "F.31", 1, 2, false, "22-25", true, []);

    private sealed class FixedReadOnlyProvider(HpFanMaxExperimentBaseline baseline) : IHpFanMaxExperimentReadOnlyProvider
    {
        private bool _enabled;
        public HpFanMaxExperimentBaseline CaptureBaseline() => baseline;
        public HpFanMaxExperimentFanReadback ReadFanStatus() => new(true, _enabled, _enabled ? "22-25" : "22-25", null);
        public void SetEnabled(bool enabled) => _enabled = enabled;
    }

    private sealed class RecordingTransport : IHpFanMaxExperimentWriteTransport
    {
        public List<string> Payloads { get; } = [];
        private FixedReadOnlyProvider? _provider;

        public void Attach(FixedReadOnlyProvider provider) => _provider = provider;

        public HpFanMaxExperimentWriteResult TrySetFanMax(byte[] payload)
        {
            Payloads.Add(Convert.ToHexString(payload).Chunk(2).Select(static pair => new string(pair)).Aggregate(static (left, right) => left + "-" + right));
            _provider?.SetEnabled(payload[0] == 0x01);
            return new HpFanMaxExperimentWriteResult(true, true, null);
        }
    }

    private sealed class ThrowingEnableTransport : IHpFanMaxExperimentWriteTransport
    {
        public List<string> Payloads { get; } = [];
        private FixedReadOnlyProvider? _provider;

        public void Attach(FixedReadOnlyProvider provider) => _provider = provider;

        public HpFanMaxExperimentWriteResult TrySetFanMax(byte[] payload)
        {
            string hex = Convert.ToHexString(payload).Chunk(2).Select(static pair => new string(pair)).Aggregate(static (left, right) => left + "-" + right);
            Payloads.Add(hex);
            if (payload[0] == 0x01)
            {
                throw new InvalidOperationException("Injected enable transport failure.");
            }

            _provider?.SetEnabled(false);
            return new HpFanMaxExperimentWriteResult(true, true, null);
        }
    }

    private sealed class NoDelay : IHpFanMaxExperimentDelay { public void WaitAfterEnable(TimeSpan duration) { } }
}
