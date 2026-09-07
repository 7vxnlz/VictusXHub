namespace GHelper.Hardware.Hp;

public static class HpFanMaxDeviceValidationEvaluator
{
    public static HpFanMaxDeviceValidationDecision Evaluate(HpFanMaxDeviceValidationEvidence evidence)
    {
        List<HpFanMaxDeviceValidationStopReason> stopReasons = [];
        HpFanMaxValidatedInputLength? selectedInputLength = SelectInputLength(evidence, stopReasons);

        if (selectedInputLength is not null)
        {
            bool hasReferenceEvidence = selectedInputLength == HpFanMaxValidatedInputLength.OneByte
                ? evidence.HasObservedOneByteReferenceEvidence
                : evidence.HasObservedFourByteReferenceEvidence;
            bool hasRestoreEvidence = selectedInputLength == HpFanMaxValidatedInputLength.OneByte
                ? evidence.HasOneByteRestoreDisableEvidence
                : evidence.HasFourByteRestoreDisableEvidence;

            if (!hasReferenceEvidence)
            {
                stopReasons.Add(HpFanMaxDeviceValidationStopReason.SelectedLengthLacksReferenceEvidence);
            }

            if (!hasRestoreEvidence)
            {
                stopReasons.Add(HpFanMaxDeviceValidationStopReason.RestoreDisableEvidenceRequired);
            }
        }

        if (!evidence.IsFanMaxGetReadbackAvailable)
        {
            stopReasons.Add(HpFanMaxDeviceValidationStopReason.FanMaxGetReadbackUnavailable);
        }

        if (evidence.BaselineMaxFanEnabled is null)
        {
            stopReasons.Add(HpFanMaxDeviceValidationStopReason.BaselineFanMaxGetReadbackRequired);
        }
        else if (evidence.BaselineMaxFanEnabled.Value)
        {
            stopReasons.Add(HpFanMaxDeviceValidationStopReason.BaselineMaxFanStateMustBeDisabled);
        }

        if (!evidence.HasEnableVerificationReadbackPlan)
        {
            stopReasons.Add(HpFanMaxDeviceValidationStopReason.EnableVerificationReadbackPlanRequired);
        }

        if (!evidence.HasRestoreVerificationReadbackPlan)
        {
            stopReasons.Add(HpFanMaxDeviceValidationStopReason.RestoreVerificationReadbackPlanRequired);
        }

        if (!evidence.HasHumanReviewedReferenceEvidence)
        {
            stopReasons.Add(HpFanMaxDeviceValidationStopReason.HumanReferenceReviewRequired);
        }

        if (!evidence.HasHumanConfirmedSelectedInputLength)
        {
            stopReasons.Add(HpFanMaxDeviceValidationStopReason.HumanInputLengthConfirmationRequired);
        }

        if (!evidence.HasHumanApprovedRecoveryPlan)
        {
            stopReasons.Add(HpFanMaxDeviceValidationStopReason.HumanRecoveryPlanConfirmationRequired);
        }

        return new HpFanMaxDeviceValidationDecision
        {
            CanProceedToNextDesignStep = stopReasons.Count == 0,
            SelectedInputLength = selectedInputLength,
            StopReasons = stopReasons
        };
    }

    private static HpFanMaxValidatedInputLength? SelectInputLength(
        HpFanMaxDeviceValidationEvidence evidence,
        ICollection<HpFanMaxDeviceValidationStopReason> stopReasons)
    {
        if (evidence.IsOneByteShapeDeviceValidated && evidence.IsFourByteShapeDeviceValidated)
        {
            stopReasons.Add(HpFanMaxDeviceValidationStopReason.ConflictingDeviceValidatedInputLengths);
            return null;
        }

        if (evidence.IsOneByteShapeDeviceValidated)
        {
            return HpFanMaxValidatedInputLength.OneByte;
        }

        if (evidence.IsFourByteShapeDeviceValidated)
        {
            return HpFanMaxValidatedInputLength.FourBytes;
        }

        stopReasons.Add(HpFanMaxDeviceValidationStopReason.NoDeviceValidatedInputLength);
        return null;
    }
}
