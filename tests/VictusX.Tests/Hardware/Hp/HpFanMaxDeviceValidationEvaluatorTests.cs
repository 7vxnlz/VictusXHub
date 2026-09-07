using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpFanMaxDeviceValidationEvaluatorTests
{
    [Fact]
    public void NoValidatedLength_IsBlocked()
    {
        HpFanMaxDeviceValidationDecision decision = HpFanMaxDeviceValidationEvaluator.Evaluate(new());

        Assert.False(decision.CanProceedToNextDesignStep);
        Assert.Null(decision.SelectedInputLength);
        Assert.Contains(HpFanMaxDeviceValidationStopReason.NoDeviceValidatedInputLength, decision.StopReasons);
        Assert.False(decision.IsWriteExecutionAllowed);
    }

    [Fact]
    public void BothValidatedLengthsClaimed_IsBlockedAsConflict()
    {
        HpFanMaxDeviceValidationDecision decision = HpFanMaxDeviceValidationEvaluator.Evaluate(ApprovedOneByteEvidence() with
        {
            IsFourByteShapeDeviceValidated = true
        });

        Assert.False(decision.CanProceedToNextDesignStep);
        Assert.Null(decision.SelectedInputLength);
        Assert.Contains(HpFanMaxDeviceValidationStopReason.ConflictingDeviceValidatedInputLengths, decision.StopReasons);
    }

    [Fact]
    public void OneByteValidatedWithoutRestoreProof_IsBlocked()
    {
        HpFanMaxDeviceValidationDecision decision = HpFanMaxDeviceValidationEvaluator.Evaluate(ApprovedOneByteEvidence() with
        {
            HasOneByteRestoreDisableEvidence = false
        });

        Assert.False(decision.CanProceedToNextDesignStep);
        Assert.Contains(HpFanMaxDeviceValidationStopReason.RestoreDisableEvidenceRequired, decision.StopReasons);
    }

    [Fact]
    public void FourByteValidatedWithoutRestoreProof_IsBlocked()
    {
        HpFanMaxDeviceValidationDecision decision = HpFanMaxDeviceValidationEvaluator.Evaluate(ApprovedFourByteEvidence() with
        {
            HasFourByteRestoreDisableEvidence = false
        });

        Assert.False(decision.CanProceedToNextDesignStep);
        Assert.Contains(HpFanMaxDeviceValidationStopReason.RestoreDisableEvidenceRequired, decision.StopReasons);
    }

    [Theory]
    [InlineData(HpFanMaxValidatedInputLength.OneByte)]
    [InlineData(HpFanMaxValidatedInputLength.FourBytes)]
    public void ExactlyOneValidatedLengthWithAllEvidence_ProceedsOnlyToNextDesignStep(
        HpFanMaxValidatedInputLength inputLength)
    {
        HpFanMaxDeviceValidationEvidence evidence = inputLength == HpFanMaxValidatedInputLength.OneByte
            ? ApprovedOneByteEvidence()
            : ApprovedFourByteEvidence();

        HpFanMaxDeviceValidationDecision decision = HpFanMaxDeviceValidationEvaluator.Evaluate(evidence);

        Assert.True(decision.CanProceedToNextDesignStep);
        Assert.Equal(inputLength, decision.SelectedInputLength);
        Assert.Empty(decision.StopReasons);
        Assert.False(decision.IsWriteExecutionAllowed);
    }

    [Fact]
    public void MissingFanMaxGetReadbackChecks_AreBlocked()
    {
        HpFanMaxDeviceValidationDecision decision = HpFanMaxDeviceValidationEvaluator.Evaluate(ApprovedOneByteEvidence() with
        {
            IsFanMaxGetReadbackAvailable = false,
            BaselineMaxFanEnabled = null,
            HasEnableVerificationReadbackPlan = false,
            HasRestoreVerificationReadbackPlan = false
        });

        Assert.False(decision.CanProceedToNextDesignStep);
        Assert.Contains(HpFanMaxDeviceValidationStopReason.FanMaxGetReadbackUnavailable, decision.StopReasons);
        Assert.Contains(HpFanMaxDeviceValidationStopReason.BaselineFanMaxGetReadbackRequired, decision.StopReasons);
        Assert.Contains(HpFanMaxDeviceValidationStopReason.EnableVerificationReadbackPlanRequired, decision.StopReasons);
        Assert.Contains(HpFanMaxDeviceValidationStopReason.RestoreVerificationReadbackPlanRequired, decision.StopReasons);
    }

    [Fact]
    public void MissingHumanConfirmations_AreBlocked()
    {
        HpFanMaxDeviceValidationDecision decision = HpFanMaxDeviceValidationEvaluator.Evaluate(ApprovedOneByteEvidence() with
        {
            HasHumanReviewedReferenceEvidence = false,
            HasHumanConfirmedSelectedInputLength = false,
            HasHumanApprovedRecoveryPlan = false
        });

        Assert.False(decision.CanProceedToNextDesignStep);
        Assert.Contains(HpFanMaxDeviceValidationStopReason.HumanReferenceReviewRequired, decision.StopReasons);
        Assert.Contains(HpFanMaxDeviceValidationStopReason.HumanInputLengthConfirmationRequired, decision.StopReasons);
        Assert.Contains(HpFanMaxDeviceValidationStopReason.HumanRecoveryPlanConfirmationRequired, decision.StopReasons);
    }

    [Fact]
    public void Evaluator_HasNoWmiDependencyOrInvocationSurface()
    {
        Type[] simulatorTypes =
        [
            typeof(HpFanMaxDeviceValidationEvidence),
            typeof(HpFanMaxDeviceValidationDecision),
            typeof(HpFanMaxDeviceValidationEvaluator)
        ];

        Assert.DoesNotContain(
            simulatorTypes.SelectMany(type => type.GetMethods()),
            method => method.Name.Contains("Invoke", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            simulatorTypes.SelectMany(type => type.GetMethods()).SelectMany(method => method.GetParameters()),
            parameter => parameter.ParameterType.Namespace?.Contains("Management", StringComparison.Ordinal) == true);
    }

    private static HpFanMaxDeviceValidationEvidence ApprovedOneByteEvidence() => ApprovedEvidence() with
    {
        IsOneByteShapeDeviceValidated = true,
        HasOneByteRestoreDisableEvidence = true
    };

    private static HpFanMaxDeviceValidationEvidence ApprovedFourByteEvidence() => ApprovedEvidence() with
    {
        IsFourByteShapeDeviceValidated = true,
        HasFourByteRestoreDisableEvidence = true
    };

    private static HpFanMaxDeviceValidationEvidence ApprovedEvidence() => new()
    {
        HasObservedOneByteReferenceEvidence = true,
        HasObservedFourByteReferenceEvidence = true,
        IsFanMaxGetReadbackAvailable = true,
        BaselineMaxFanEnabled = false,
        HasEnableVerificationReadbackPlan = true,
        HasRestoreVerificationReadbackPlan = true,
        HasHumanReviewedReferenceEvidence = true,
        HasHumanConfirmedSelectedInputLength = true,
        HasHumanApprovedRecoveryPlan = true
    };
}
