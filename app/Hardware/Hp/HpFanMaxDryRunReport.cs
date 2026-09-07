namespace GHelper.Hardware.Hp;

public sealed record HpFanMaxDryRunReport
{
    public const string FirstWriteGateStatus = "NO-GO";
    public const string FirstWriteGateReason = "Missing exact-device proof: payload length, restore/disable behavior, thermal/power state, failure/recovery proof, and human approval.";

    private HpFanMaxDryRunReport(
        bool dryRunEvaluated,
        int? deviceValidatedInputLength,
        string[] blockedReasons,
        string nextRequiredProof)
    {
        SetFanMaxDryRunEvaluated = dryRunEvaluated;
        SetFanMaxDeviceValidatedInputLength = deviceValidatedInputLength;
        SetFanMaxDryRunBlockedReasons = blockedReasons;
        SetFanMaxNextRequiredProof = nextRequiredProof;
    }

    public bool SetFanMaxWriteImplemented => false;
    public bool SetFanMaxWriteAllowed => false;
    public string SetFanMaxFirstWriteGateStatus => FirstWriteGateStatus;
    public bool SetFanMaxFirstWriteGateSatisfied => false;
    public string SetFanMaxFirstWriteGateReason => FirstWriteGateReason;
    public string SetFanMaxExperimentalPayloadCandidate => "FourByte";
    public bool SetFanMaxPhysicalResponseObserved => true;
    public int SetFanMaxPhysicalResponseConfirmationCount => 2;
    public bool SetFanMaxReadbackReliable => false;
    public bool SetFanMaxDeveloperExperimentAllowed => true;
    public string SetFanMaxDeveloperExperimentPayload => "FourByte";
    public bool SetFanMaxNormalControlValidated => false;
    public bool SetFanMaxUserFacingControlAllowed => false;
    public bool SetFanMaxDryRunEvaluated { get; }
    public int? SetFanMaxDeviceValidatedInputLength { get; }
    public string[] SetFanMaxDryRunBlockedReasons { get; }
    public string SetFanMaxNextRequiredProof { get; }

    public static HpFanMaxDryRunReport CreateDefaultBlocked()
    {
        HpFanMaxDeviceValidationEvidence knownEvidence = new()
        {
            HasObservedOneByteReferenceEvidence = true,
            HasObservedFourByteReferenceEvidence = true,
            IsFanMaxGetReadbackAvailable = true,
            BaselineMaxFanEnabled = false,
            HasEnableVerificationReadbackPlan = true,
            HasRestoreVerificationReadbackPlan = true,
            HasHumanReviewedReferenceEvidence = true
        };

        return FromEvidence(knownEvidence);
    }

    public static HpFanMaxDryRunReport FromEvidence(HpFanMaxDeviceValidationEvidence evidence)
    {
        HpFanMaxDeviceValidationDecision decision = HpFanMaxDeviceValidationEvaluator.Evaluate(evidence);
        string[] blockedReasons = decision.StopReasons.Select(reason => reason.ToString()).ToArray();

        return new HpFanMaxDryRunReport(
            dryRunEvaluated: true,
            deviceValidatedInputLength: decision.SelectedInputLength is null
                ? null
                : (int)decision.SelectedInputLength.Value,
            blockedReasons,
            GetNextRequiredProof(decision));
    }

    private static string GetNextRequiredProof(HpFanMaxDeviceValidationDecision decision)
    {
        if (decision.StopReasons.Contains(HpFanMaxDeviceValidationStopReason.NoDeviceValidatedInputLength))
        {
            return "Device-validate exactly one SetFanMax input length (1 or 4) and prove its restore/disable behavior.";
        }

        if (decision.StopReasons.Contains(HpFanMaxDeviceValidationStopReason.ConflictingDeviceValidatedInputLengths))
        {
            return "Resolve the conflicting input-length evidence; do not select a default payload shape.";
        }

        if (decision.StopReasons.Contains(HpFanMaxDeviceValidationStopReason.RestoreDisableEvidenceRequired))
        {
            return "Prove restore/disable behavior for the selected device-validated input length.";
        }

        if (!decision.CanProceedToNextDesignStep)
        {
            return "Satisfy every reported evidence, FanMaxGet readback, and human-confirmation requirement.";
        }

        return "Complete a separate human safety review before considering any guarded runtime design.";
    }
}
