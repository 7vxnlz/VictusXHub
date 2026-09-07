namespace GHelper.Hardware.Hp;

public sealed record HpFanMaxExperimentPayload(
    HpFanMaxExperimentPayloadLengthCandidate Candidate,
    byte[] EnableBytes,
    byte[] RestoreBytes)
{
    public string EnableBytesHex => Convert.ToHexString(EnableBytes.AsSpan()).Chunk(2).Select(static pair => new string(pair)).Aggregate(static (left, right) => left + "-" + right);
    public string RestoreBytesHex => Convert.ToHexString(RestoreBytes.AsSpan()).Chunk(2).Select(static pair => new string(pair)).Aggregate(static (left, right) => left + "-" + right);
}

public sealed record HpFanMaxExperimentRunnerCommandResult(
    bool IsRequested,
    bool IsValidRequest,
    HpFanMaxExperimentPayload? Payload,
    bool HasOneTimeFourByteApproval,
    bool HasSecondFourByteConfirmationApproval,
    bool HasOneTimeOneByteComparisonApproval,
    HpFanMaxExperimentManualObservation ManualObservation,
    string[] ValidationReasons)
{
    public bool ShouldExit => IsRequested;
}

public static class HpFanMaxExperimentRunnerCommand
{
    public const string ExperimentFlag = "--hp-fan-write-experiment";
    public const string HpVictusFlag = "--hp-victus";
    public const string ReadOnlyTestFlag = "--hp-wmi-readonly-test";
    public const string AcknowledgementFlag = "--i-understand-this-can-affect-fans";
    public const string OneTimeFourByteApprovalFlag = "--i-approve-one-time-set-fan-max-4-byte-experiment";
    public const string SecondFourByteConfirmationApprovalFlag = "--i-approve-second-set-fan-max-4-byte-confirmation";
    public const string OneTimeOneByteComparisonApprovalFlag = "--i-approve-one-time-set-fan-max-1-byte-comparison";
    public const string PayloadLengthPrefix = "--set-fan-max-payload-length=";
    private const string DryRunFlag = "--hp-fan-write-experiment-dry-run";
    private const string BaselineCaptureFlag = "--hp-fan-write-experiment-baseline";

    public static HpFanMaxExperimentRunnerCommandResult Parse(IEnumerable<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        string[] args = arguments.ToArray();
        bool isRequested = args.Any(arg => string.Equals(arg, ExperimentFlag, StringComparison.OrdinalIgnoreCase));
        if (!isRequested)
        {
            return new HpFanMaxExperimentRunnerCommandResult(
                false,
                false,
                null,
                false,
                false,
                false,
                new HpFanMaxExperimentManualObservation(null, null, null, []),
                []);
        }

        List<string> reasons = [];
        HpFanMaxExperimentManualObservation manualObservation = HpFanMaxExperimentManualObservation.Parse(args);
        reasons.AddRange(manualObservation.ValidationReasons);
        RequireFlag(args, HpVictusFlag, reasons);
        RequireFlag(args, ReadOnlyTestFlag, reasons);
        RequireFlag(args, AcknowledgementFlag, reasons);

        if (args.Any(arg => string.Equals(arg, DryRunFlag, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(arg, BaselineCaptureFlag, StringComparison.OrdinalIgnoreCase)))
        {
            reasons.Add("First-write experiment request rejected: dry-run and baseline-capture flags cannot be combined with a write experiment.");
        }

        string[] payloadArguments = args.Where(arg => arg.StartsWith(PayloadLengthPrefix, StringComparison.OrdinalIgnoreCase)).ToArray();
        HpFanMaxExperimentPayload? payload = null;
        bool hasOneTimeFourByteApproval = args.Any(arg => string.Equals(arg, OneTimeFourByteApprovalFlag, StringComparison.OrdinalIgnoreCase));
        bool hasSecondFourByteConfirmationApproval = args.Any(arg => string.Equals(arg, SecondFourByteConfirmationApprovalFlag, StringComparison.OrdinalIgnoreCase));
        bool hasOneTimeOneByteComparisonApproval = args.Any(arg => string.Equals(arg, OneTimeOneByteComparisonApprovalFlag, StringComparison.OrdinalIgnoreCase));
        if (payloadArguments.Length != 1)
        {
            reasons.Add("First-write experiment request rejected: specify exactly one --set-fan-max-payload-length=1 or =4 hypothesis.");
        }
        else
        {
            payload = CreatePayload(payloadArguments[0]);
            if (payload is null)
            {
                reasons.Add("First-write experiment request rejected: payload length must be exactly 1 or 4.");
            }
            else if (payload.Candidate == HpFanMaxExperimentPayloadLengthCandidate.FourByteHypothesis && !hasOneTimeFourByteApproval)
            {
                reasons.Add("First-write 4-byte experiment request rejected: " + OneTimeFourByteApprovalFlag + " is required.");
            }
            else if (payload.Candidate == HpFanMaxExperimentPayloadLengthCandidate.FourByteHypothesis && !hasSecondFourByteConfirmationApproval)
            {
                reasons.Add("Second 4-byte confirmation request rejected: " + SecondFourByteConfirmationApprovalFlag + " is required.");
            }
            else if (payload.Candidate == HpFanMaxExperimentPayloadLengthCandidate.FourByteHypothesis && hasOneTimeOneByteComparisonApproval)
            {
                reasons.Add("First-write experiment request rejected: one-byte comparison approval authorizes the 1-byte hypothesis only.");
            }
            else if (payload.Candidate == HpFanMaxExperimentPayloadLengthCandidate.OneByteHypothesis && !hasOneTimeOneByteComparisonApproval)
            {
                reasons.Add("One-byte comparison experiment request rejected: " + OneTimeOneByteComparisonApprovalFlag + " is required.");
            }
            else if (payload.Candidate == HpFanMaxExperimentPayloadLengthCandidate.OneByteHypothesis && (hasOneTimeFourByteApproval || hasSecondFourByteConfirmationApproval))
            {
                reasons.Add("First-write experiment request rejected: four-byte approval flags authorize the 4-byte hypothesis only.");
            }
        }

        return new HpFanMaxExperimentRunnerCommandResult(
            true,
            reasons.Count == 0,
            payload,
            hasOneTimeFourByteApproval,
            hasSecondFourByteConfirmationApproval,
            hasOneTimeOneByteComparisonApproval,
            manualObservation,
            reasons.ToArray());
    }

    private static void RequireFlag(IEnumerable<string> arguments, string flag, ICollection<string> reasons)
    {
        if (!arguments.Any(arg => string.Equals(arg, flag, StringComparison.OrdinalIgnoreCase)))
        {
            reasons.Add("First-write experiment request rejected: " + flag + " is required.");
        }
    }

    private static HpFanMaxExperimentPayload? CreatePayload(string argument) => argument[PayloadLengthPrefix.Length..] switch
    {
        "1" => new HpFanMaxExperimentPayload(HpFanMaxExperimentPayloadLengthCandidate.OneByteHypothesis, [0x01], [0x00]),
        "4" => new HpFanMaxExperimentPayload(HpFanMaxExperimentPayloadLengthCandidate.FourByteHypothesis, [0x01, 0x00, 0x00, 0x00], [0x00, 0x00, 0x00, 0x00]),
        _ => null
    };
}
