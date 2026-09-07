using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpFanMaxExperimentOutcomeClassifierTests
{
    [Fact]
    public void SuccessfulCommandWithPhysicalResponseAndFalseFanMaxGet_IsReadbackInconclusive()
    {
        HpFanMaxExperimentLogRecord record = CreateWrittenRecord() with
        {
            EnableCommandSucceeded = true,
            RestoreCommandSucceeded = true,
            FanMaxGetConfirmedEnable = false,
            PhysicalFanResponseObserved = true,
            RestoreObserved = true,
            ReadbackReliability = HpFanMaxExperimentReadbackReliability.Inconclusive
        };

        Assert.Equal(
            HpFanMaxExperimentalOutcomeClassification.CommandSucceededPhysicalResponseObservedReadbackInconclusive,
            record.ExperimentalOutcomeClassification);
        Assert.Null(record.DeviceValidatedInputLength);
    }

    [Fact]
    public void Mapper_ReadbackInconclusivePhysicalResponse_UsesUnknownLegacyOutcomeAndRemovesFanMaxGetFailure()
    {
        HpFanMaxExperimentRunResult result = new(
            new HpFanMaxExperimentPayload(HpFanMaxExperimentPayloadLengthCandidate.FourByteHypothesis, [0x01, 0x00, 0x00, 0x00], [0x00, 0x00, 0x00, 0x00]),
            null,
            true,
            new HpFanMaxExperimentWriteResult(true, true, null),
            new HpFanMaxExperimentFanReadback(true, false, "33-00", null),
            new HpFanMaxExperimentWriteResult(true, true, null),
            new HpFanMaxExperimentFanReadback(true, false, "33-00", null),
            HpFanMaxExperimentOutcome.Fail,
            ["Enable result or post-enable FanMaxGet readback did not confirm max fan enabled."]);

        HpFanMaxExperimentLogRecord record = HpFanMaxExperimentRunLogMapper.Create(
            result,
            new HpFanMaxExperimentManualObservation(true, true, "Fan ramp observed and restore observed.", []));

        Assert.Equal(HpFanMaxExperimentOutcome.Unknown, record.Outcome);
        Assert.Equal(
            HpFanMaxExperimentalOutcomeClassification.CommandSucceededPhysicalResponseObservedReadbackInconclusive,
            record.ExperimentalOutcomeClassification);
        Assert.DoesNotContain(record.BlockedReasons, reason => reason.Contains("FanMaxGet", StringComparison.Ordinal));
        Assert.Null(record.DeviceValidatedInputLength);
    }

    [Fact]
    public void SuccessfulCommandWithoutPhysicalConfirmation_RemainsFailOrUnknownRatherThanValidated()
    {
        HpFanMaxExperimentLogRecord record = CreateWrittenRecord() with
        {
            EnableCommandSucceeded = true,
            RestoreCommandSucceeded = true,
            PhysicalFanResponseObserved = false,
            Outcome = HpFanMaxExperimentOutcome.Fail
        };

        Assert.Equal(HpFanMaxExperimentalOutcomeClassification.CommandSucceededNoPhysicalConfirmation, record.ExperimentalOutcomeClassification);
        Assert.Equal(HpFanMaxExperimentOutcome.Fail, record.Outcome);
        Assert.Null(record.DeviceValidatedInputLength);
    }

    [Fact]
    public void RestoreFailure_IsClassifiedAsRestoreFailed()
    {
        HpFanMaxExperimentLogRecord record = CreateWrittenRecord() with
        {
            EnableCommandSucceeded = true,
            RestoreCommandSucceeded = false,
            PhysicalFanResponseObserved = true,
            Outcome = HpFanMaxExperimentOutcome.Fail
        };

        Assert.Equal(HpFanMaxExperimentalOutcomeClassification.RestoreFailed, record.ExperimentalOutcomeClassification);
        Assert.Null(record.DeviceValidatedInputLength);
    }

    [Fact]
    public void UnsafeAbort_IsClassifiedWithoutGrantingAnyApproval()
    {
        HpFanMaxExperimentLogRecord record = CreateWrittenRecord() with { UnsafeAbortObserved = true };

        Assert.Equal(HpFanMaxExperimentalOutcomeClassification.UnsafeAbort, record.ExperimentalOutcomeClassification);
        Assert.False(record.FirstWriteGateSatisfied);
        Assert.Null(record.DeviceValidatedInputLength);
    }

    private static HpFanMaxExperimentLogRecord CreateWrittenRecord() => new()
    {
        WriteExecuted = true,
        PayloadLengthCandidate = HpFanMaxExperimentPayloadLengthCandidate.FourByteHypothesis,
        PayloadBytesHypothesis = "01-00-00-00"
    };
}
