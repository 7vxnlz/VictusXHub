using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpFanWriteSafetyScaffoldingTests
{
    [Fact]
    public void DefaultPolicy_IsBlockedAndRequiresEverySafetyGate()
    {
        HpFanWriteSafetyPolicy policy = new();

        Assert.False(policy.IsWriteExecutionAllowed);
        Assert.True(policy.RequiresAdministrator);
        Assert.True(policy.RequiresReadbackBeforeWrite);
        Assert.True(policy.RequiresReadbackAfterWrite);
        Assert.True(policy.RequiresVerifiedRestore);
        Assert.Equal(
            [
                "--hp-victus",
                "--hp-fan-write-experiment",
                "--hp-wmi-write-manual-test",
                "--hp-fan-write-acknowledge-risk"
            ],
            policy.RequiredFlags);
    }

    [Fact]
    public void DefaultPreflight_IsNotAllowedAndExplainsWhy()
    {
        HpFanWritePreflightResult result = new();

        Assert.False(result.IsAllowed);
        Assert.Equal([HpFanWriteAbortReason.BlockedByDefault], result.AbortReasons);
    }

    [Fact]
    public void DefaultFanMaxPlan_HasNoTargetOrRestoreAndCannotExecute()
    {
        HpFanMaxWriteExperimentPlan plan = new();

        Assert.Equal("SetFanMax", HpFanMaxWriteExperimentPlan.CommandName);
        Assert.Equal(0x27u, HpFanMaxWriteExperimentPlan.CommandId);
        Assert.Null(plan.TargetState);
        Assert.Null(plan.RestoreTargetState);
        Assert.Null(plan.DeviceValidatedInputLength);
        Assert.Null(plan.EnablePayloadDescription);
        Assert.Null(plan.RestorePayloadDescription);
        Assert.True(plan.RequiresReadbackBeforeWrite);
        Assert.True(plan.RequiresReadbackAfterWrite);
        Assert.True(plan.RequiresVerifiedRestore);
        Assert.False(plan.IsWriteExecutionAllowed);
    }

    [Theory]
    [InlineData(HpFanMaxTargetState.EnableMaxFan, 1)]
    [InlineData(HpFanMaxTargetState.RestoreDisableMaxFan, 0)]
    public void OneBytePayloadDescription_ContainsOnlyStateByteMetadata(
        HpFanMaxTargetState targetState,
        byte expectedStateByteValue)
    {
        HpFanMaxPayloadDescription description = HpFanMaxPayloadDescription.Describe(
            targetState,
            HpFanMaxValidatedInputLength.OneByte);

        Assert.Equal(targetState, description.TargetState);
        Assert.Equal(expectedStateByteValue, description.StateByteValue);
        Assert.Equal(HpFanMaxValidatedInputLength.OneByte, description.DeviceValidatedInputLength);
        Assert.Equal(0, description.ZeroFilledTrailingByteCount);
        Assert.Equal("hpqBIOSInt0", HpFanMaxPayloadDescription.ReferenceMethodName);
        Assert.Equal(0x20008u, HpFanMaxPayloadDescription.ReferenceCommandValue);
        Assert.Equal(0x27u, HpFanMaxPayloadDescription.ReferenceCommandType);
        Assert.Equal(0, HpFanMaxPayloadDescription.ReferenceExpectedOutputSize);
    }

    [Theory]
    [InlineData(HpFanMaxTargetState.EnableMaxFan, 1)]
    [InlineData(HpFanMaxTargetState.RestoreDisableMaxFan, 0)]
    public void FourBytePayloadDescription_ContainsStateByteAndZeroTailMetadata(
        HpFanMaxTargetState targetState,
        byte expectedStateByteValue)
    {
        HpFanMaxPayloadDescription description = HpFanMaxPayloadDescription.Describe(
            targetState,
            HpFanMaxValidatedInputLength.FourBytes);

        Assert.Equal(targetState, description.TargetState);
        Assert.Equal(expectedStateByteValue, description.StateByteValue);
        Assert.Equal(HpFanMaxValidatedInputLength.FourBytes, description.DeviceValidatedInputLength);
        Assert.Equal(3, description.ZeroFilledTrailingByteCount);
    }

    [Fact]
    public void PayloadDescription_InvalidInputLength_IsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => HpFanMaxPayloadDescription.Describe(
            HpFanMaxTargetState.EnableMaxFan,
            (HpFanMaxValidatedInputLength)2));
    }

    [Fact]
    public void Preflight_DefaultRequest_IsBlockedByEveryMissingRequirement()
    {
        HpFanWritePreflightResult result = HpFanMaxWritePreflightEvaluator.Evaluate(new());

        Assert.False(result.IsAllowed);
        Assert.Contains(HpFanWriteAbortReason.CommandNotSetFanMax, result.AbortReasons);
        Assert.Contains(HpFanWriteAbortReason.MissingRequiredRuntimeFlag, result.AbortReasons);
        Assert.Contains(HpFanWriteAbortReason.AdministratorRequired, result.AbortReasons);
        Assert.Contains(HpFanWriteAbortReason.InteractiveHumanConfirmationRequired, result.AbortReasons);
        Assert.Contains(HpFanWriteAbortReason.ApprovedDeviceBaselineRequired, result.AbortReasons);
        Assert.Contains(HpFanWriteAbortReason.HealthyReadOnlyBaselineRequired, result.AbortReasons);
        Assert.Contains(HpFanWriteAbortReason.StableAcPowerRequired, result.AbortReasons);
        Assert.Contains(HpFanWriteAbortReason.IndependentThermalObservationRequired, result.AbortReasons);
        Assert.Contains(HpFanWriteAbortReason.PreWriteReadbackRequired, result.AbortReasons);
        Assert.Contains(HpFanWriteAbortReason.CurrentMaxFanStateUnknown, result.AbortReasons);
        Assert.Contains(HpFanWriteAbortReason.DeviceValidatedInputLengthRequired, result.AbortReasons);
        Assert.Contains(HpFanWriteAbortReason.EnablePayloadDescriptionRequired, result.AbortReasons);
        Assert.Contains(HpFanWriteAbortReason.RestorePayloadDescriptionRequired, result.AbortReasons);
        Assert.Contains(HpFanWriteAbortReason.WriteTargetStateRequired, result.AbortReasons);
        Assert.Contains(HpFanWriteAbortReason.PostWriteReadbackRequired, result.AbortReasons);
        Assert.Contains(HpFanWriteAbortReason.RestorePlanRequired, result.AbortReasons);
        Assert.Contains(HpFanWriteAbortReason.SingleWriteAttemptRequired, result.AbortReasons);
    }

    [Fact]
    public void Preflight_UnsetDeviceValidatedInputLength_IsBlocked()
    {
        HpFanMaxWritePreflightRequest request = ApprovedRequest();

        HpFanWritePreflightResult result = HpFanMaxWritePreflightEvaluator.Evaluate(request with
        {
            Plan = request.Plan with
            {
                DeviceValidatedInputLength = null,
                EnablePayloadDescription = null,
                RestorePayloadDescription = null
            }
        });

        Assert.False(result.IsAllowed);
        Assert.Contains(HpFanWriteAbortReason.DeviceValidatedInputLengthRequired, result.AbortReasons);
    }

    [Fact]
    public void Preflight_InvalidDeviceValidatedInputLength_IsBlocked()
    {
        HpFanMaxWritePreflightRequest request = ApprovedRequest();

        HpFanWritePreflightResult result = HpFanMaxWritePreflightEvaluator.Evaluate(request with
        {
            Plan = request.Plan with
            {
                DeviceValidatedInputLength = (HpFanMaxValidatedInputLength)2
            }
        });

        Assert.False(result.IsAllowed);
        Assert.Contains(HpFanWriteAbortReason.DeviceValidatedInputLengthInvalid, result.AbortReasons);
    }

    [Fact]
    public void Preflight_MissingRestorePayloadDescription_IsBlocked()
    {
        HpFanMaxWritePreflightRequest request = ApprovedRequest();

        HpFanWritePreflightResult result = HpFanMaxWritePreflightEvaluator.Evaluate(request with
        {
            Plan = request.Plan with { RestorePayloadDescription = null }
        });

        Assert.False(result.IsAllowed);
        Assert.Contains(HpFanWriteAbortReason.RestorePayloadDescriptionRequired, result.AbortReasons);
    }

    [Fact]
    public void Preflight_NonSetFanMaxCommand_IsBlocked()
    {
        HpFanWritePreflightResult result = HpFanMaxWritePreflightEvaluator.Evaluate(ApprovedRequest() with
        {
            RequestedCommandName = "SetFanLevel",
            RequestedCommandId = 0x2E
        });

        Assert.False(result.IsAllowed);
        Assert.Contains(HpFanWriteAbortReason.CommandNotSetFanMax, result.AbortReasons);
    }

    [Fact]
    public void Preflight_MissingPreRead_IsBlocked()
    {
        HpFanWritePreflightResult result = HpFanMaxWritePreflightEvaluator.Evaluate(ApprovedRequest() with
        {
            HasSuccessfulPreReadFanMaxGet = false
        });

        Assert.False(result.IsAllowed);
        Assert.Contains(HpFanWriteAbortReason.PreWriteReadbackRequired, result.AbortReasons);
    }

    [Fact]
    public void Preflight_UnknownCurrentState_IsBlocked()
    {
        HpFanWritePreflightResult result = HpFanMaxWritePreflightEvaluator.Evaluate(ApprovedRequest() with
        {
            CurrentMaxFanEnabled = null
        });

        Assert.False(result.IsAllowed);
        Assert.Contains(HpFanWriteAbortReason.CurrentMaxFanStateUnknown, result.AbortReasons);
    }

    [Fact]
    public void Preflight_MissingRestorePlan_IsBlocked()
    {
        HpFanWritePreflightResult result = HpFanMaxWritePreflightEvaluator.Evaluate(ApprovedRequest() with
        {
            Plan = new HpFanMaxWriteExperimentPlan
            {
                TargetState = HpFanMaxTargetState.EnableMaxFan,
                DeviceValidatedInputLength = HpFanMaxValidatedInputLength.FourBytes,
                EnablePayloadDescription = HpFanMaxPayloadDescription.Describe(
                    HpFanMaxTargetState.EnableMaxFan,
                    HpFanMaxValidatedInputLength.FourBytes)
            }
        });

        Assert.False(result.IsAllowed);
        Assert.Contains(HpFanWriteAbortReason.RestorePlanRequired, result.AbortReasons);
    }

    [Fact]
    public void Preflight_NonAdmin_IsBlocked()
    {
        HpFanWritePreflightResult result = HpFanMaxWritePreflightEvaluator.Evaluate(ApprovedRequest() with
        {
            IsAdministrator = false
        });

        Assert.False(result.IsAllowed);
        Assert.Contains(HpFanWriteAbortReason.AdministratorRequired, result.AbortReasons);
    }

    [Fact]
    public void Preflight_MissingExplicitWriteFlag_IsBlocked()
    {
        HpFanWritePreflightResult result = HpFanMaxWritePreflightEvaluator.Evaluate(ApprovedRequest() with
        {
            PresentFlags = ["--hp-victus"]
        });

        Assert.False(result.IsAllowed);
        Assert.Contains(HpFanWriteAbortReason.MissingRequiredRuntimeFlag, result.AbortReasons);
    }

    [Fact]
    public void Preflight_RestoreStateCannotBeTheInitialWrite()
    {
        HpFanWritePreflightResult result = HpFanMaxWritePreflightEvaluator.Evaluate(ApprovedRequest() with
        {
            Plan = new HpFanMaxWriteExperimentPlan
            {
                TargetState = HpFanMaxTargetState.RestoreDisableMaxFan,
                RestoreTargetState = HpFanMaxTargetState.RestoreDisableMaxFan,
                DeviceValidatedInputLength = HpFanMaxValidatedInputLength.FourBytes,
                EnablePayloadDescription = HpFanMaxPayloadDescription.Describe(
                    HpFanMaxTargetState.EnableMaxFan,
                    HpFanMaxValidatedInputLength.FourBytes),
                RestorePayloadDescription = HpFanMaxPayloadDescription.Describe(
                    HpFanMaxTargetState.RestoreDisableMaxFan,
                    HpFanMaxValidatedInputLength.FourBytes)
            }
        });

        Assert.False(result.IsAllowed);
        Assert.Contains(HpFanWriteAbortReason.InitialWriteMustEnableMaxFan, result.AbortReasons);
    }

    [Fact]
    public void Preflight_MissingHumanConfirmation_IsBlocked()
    {
        HpFanWritePreflightResult result = HpFanMaxWritePreflightEvaluator.Evaluate(ApprovedRequest() with
        {
            HasInteractiveHumanConfirmation = false
        });

        Assert.False(result.IsAllowed);
        Assert.Contains(HpFanWriteAbortReason.InteractiveHumanConfirmationRequired, result.AbortReasons);
    }

    [Fact]
    public void Preflight_TheoreticalFullyApprovedRequest_IsAllowedButCannotExecute()
    {
        HpFanMaxWritePreflightRequest request = ApprovedRequest();

        HpFanWritePreflightResult result = HpFanMaxWritePreflightEvaluator.Evaluate(request);

        Assert.True(result.IsAllowed);
        Assert.Empty(result.AbortReasons);
        Assert.False(request.Plan.IsWriteExecutionAllowed);
    }

    [Fact]
    public void SafetyScaffolding_ExposesNoWmiInvocationMethod()
    {
        Type[] safetyTypes =
        [
            typeof(HpFanMaxPayloadDescription),
            typeof(HpFanMaxWriteExperimentPlan),
            typeof(HpFanMaxWritePreflightEvaluator),
            typeof(HpFanMaxWritePreflightRequest),
            typeof(HpFanWriteSafetyPolicy)
        ];

        Assert.DoesNotContain(
            safetyTypes.SelectMany(type => type.GetMethods()),
            method => method.Name.Contains("Invoke", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            safetyTypes.SelectMany(type => type.GetMethods()).SelectMany(method => method.GetParameters()),
            parameter => parameter.ParameterType.Namespace?.Contains("Management", StringComparison.Ordinal) == true);
    }

    private static HpFanMaxWritePreflightRequest ApprovedRequest() => new()
    {
        RequestedCommandName = HpFanMaxWriteExperimentPlan.CommandName,
        RequestedCommandId = HpFanMaxWriteExperimentPlan.CommandId,
        PresentFlags = HpFanWriteSafetyPolicy.DefaultRequiredFlags,
        IsAdministrator = true,
        HasInteractiveHumanConfirmation = true,
        HasApprovedDeviceBaseline = true,
        HasHealthyReadOnlyBaseline = true,
        HasStableAcPower = true,
        HasIndependentThermalObservation = true,
        HasSuccessfulPreReadFanMaxGet = true,
        CurrentMaxFanEnabled = false,
        HasPostWriteReadbackPlan = true,
        HasRestoreReadbackPlan = true,
        IsSingleWriteAttempt = true,
        Plan = new HpFanMaxWriteExperimentPlan
        {
            TargetState = HpFanMaxTargetState.EnableMaxFan,
            RestoreTargetState = HpFanMaxTargetState.RestoreDisableMaxFan,
            DeviceValidatedInputLength = HpFanMaxValidatedInputLength.FourBytes,
            EnablePayloadDescription = HpFanMaxPayloadDescription.Describe(
                HpFanMaxTargetState.EnableMaxFan,
                HpFanMaxValidatedInputLength.FourBytes),
            RestorePayloadDescription = HpFanMaxPayloadDescription.Describe(
                HpFanMaxTargetState.RestoreDisableMaxFan,
                HpFanMaxValidatedInputLength.FourBytes)
        }
    };
}
