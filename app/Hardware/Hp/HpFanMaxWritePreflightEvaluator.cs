namespace GHelper.Hardware.Hp;

public static class HpFanMaxWritePreflightEvaluator
{
    public static HpFanWritePreflightResult Evaluate(HpFanMaxWritePreflightRequest request)
    {
        HpFanWriteSafetyPolicy policy = new();
        List<HpFanWriteAbortReason> abortReasons = [];

        if (request.RequestedCommandName != HpFanMaxWriteExperimentPlan.CommandName ||
            request.RequestedCommandId != HpFanMaxWriteExperimentPlan.CommandId)
        {
            abortReasons.Add(HpFanWriteAbortReason.CommandNotSetFanMax);
        }

        if (policy.RequiredFlags.Any(flag => !request.PresentFlags.Contains(flag, StringComparer.Ordinal)))
        {
            abortReasons.Add(HpFanWriteAbortReason.MissingRequiredRuntimeFlag);
        }

        if (policy.RequiresAdministrator && !request.IsAdministrator)
        {
            abortReasons.Add(HpFanWriteAbortReason.AdministratorRequired);
        }

        if (!request.HasInteractiveHumanConfirmation)
        {
            abortReasons.Add(HpFanWriteAbortReason.InteractiveHumanConfirmationRequired);
        }

        if (!request.HasApprovedDeviceBaseline)
        {
            abortReasons.Add(HpFanWriteAbortReason.ApprovedDeviceBaselineRequired);
        }

        if (!request.HasHealthyReadOnlyBaseline)
        {
            abortReasons.Add(HpFanWriteAbortReason.HealthyReadOnlyBaselineRequired);
        }

        if (!request.HasStableAcPower)
        {
            abortReasons.Add(HpFanWriteAbortReason.StableAcPowerRequired);
        }

        if (!request.HasIndependentThermalObservation)
        {
            abortReasons.Add(HpFanWriteAbortReason.IndependentThermalObservationRequired);
        }

        if (policy.RequiresReadbackBeforeWrite && !request.HasSuccessfulPreReadFanMaxGet)
        {
            abortReasons.Add(HpFanWriteAbortReason.PreWriteReadbackRequired);
        }

        if (request.CurrentMaxFanEnabled is null)
        {
            abortReasons.Add(HpFanWriteAbortReason.CurrentMaxFanStateUnknown);
        }
        else if (request.CurrentMaxFanEnabled.Value)
        {
            abortReasons.Add(HpFanWriteAbortReason.BaselineMaxFanMustBeDisabled);
        }

        HpFanMaxValidatedInputLength? validatedInputLength = request.Plan.DeviceValidatedInputLength;
        bool inputLengthIsValid = validatedInputLength is not null &&
            HpFanMaxPayloadDescription.IsSupportedInputLength(validatedInputLength.Value);

        if (validatedInputLength is null)
        {
            abortReasons.Add(HpFanWriteAbortReason.DeviceValidatedInputLengthRequired);
        }
        else if (!inputLengthIsValid)
        {
            abortReasons.Add(HpFanWriteAbortReason.DeviceValidatedInputLengthInvalid);
        }

        if (!inputLengthIsValid || !MatchesPayloadDescription(
                request.Plan.EnablePayloadDescription,
                HpFanMaxTargetState.EnableMaxFan,
                validatedInputLength!.Value))
        {
            abortReasons.Add(HpFanWriteAbortReason.EnablePayloadDescriptionRequired);
        }

        if (!inputLengthIsValid || !MatchesPayloadDescription(
                request.Plan.RestorePayloadDescription,
                HpFanMaxTargetState.RestoreDisableMaxFan,
                validatedInputLength!.Value))
        {
            abortReasons.Add(HpFanWriteAbortReason.RestorePayloadDescriptionRequired);
        }

        if (request.Plan.TargetState is null)
        {
            abortReasons.Add(HpFanWriteAbortReason.WriteTargetStateRequired);
        }
        else if (request.Plan.TargetState != HpFanMaxTargetState.EnableMaxFan)
        {
            abortReasons.Add(HpFanWriteAbortReason.InitialWriteMustEnableMaxFan);
        }

        if (policy.RequiresReadbackAfterWrite && !request.HasPostWriteReadbackPlan)
        {
            abortReasons.Add(HpFanWriteAbortReason.PostWriteReadbackRequired);
        }

        if (policy.RequiresVerifiedRestore &&
            (request.Plan.RestoreTargetState != HpFanMaxTargetState.RestoreDisableMaxFan ||
             !request.HasRestoreReadbackPlan))
        {
            abortReasons.Add(HpFanWriteAbortReason.RestorePlanRequired);
        }

        if (!request.IsSingleWriteAttempt)
        {
            abortReasons.Add(HpFanWriteAbortReason.SingleWriteAttemptRequired);
        }

        return new HpFanWritePreflightResult
        {
            IsAllowed = abortReasons.Count == 0,
            AbortReasons = abortReasons
        };
    }

    private static bool MatchesPayloadDescription(
        HpFanMaxPayloadDescription? description,
        HpFanMaxTargetState expectedTargetState,
        HpFanMaxValidatedInputLength expectedInputLength) =>
        description is not null &&
        description.TargetState == expectedTargetState &&
        description.DeviceValidatedInputLength == expectedInputLength &&
        description.StateByteValue == (expectedTargetState == HpFanMaxTargetState.EnableMaxFan ? 1 : 0) &&
        description.ZeroFilledTrailingByteCount == (int)expectedInputLength - 1;
}
