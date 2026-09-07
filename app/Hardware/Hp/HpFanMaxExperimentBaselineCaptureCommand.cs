namespace GHelper.Hardware.Hp;

public sealed record HpFanMaxExperimentBaselineCaptureCommandResult(
    bool IsRequested,
    bool IsValidRequest,
    HpFanMaxExperimentPayloadLengthCandidate? PayloadLengthCandidate,
    string? PayloadBytesHypothesis,
    string[] ValidationReasons)
{
    public bool ShouldExit => IsRequested;
}

public static class HpFanMaxExperimentBaselineCaptureCommand
{
    public const string BaselineCaptureFlag = "--hp-fan-write-experiment-baseline";
    public const string HpVictusFlag = "--hp-victus";
    public const string ReadOnlyTestFlag = "--hp-wmi-readonly-test";
    public const string PayloadLengthPrefix = "--set-fan-max-payload-length=";
    private const string DryRunFlag = "--hp-fan-write-experiment-dry-run";

    public static HpFanMaxExperimentBaselineCaptureCommandResult Parse(IEnumerable<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        string[] args = arguments.ToArray();
        bool isRequested = args.Any(arg => string.Equals(arg, BaselineCaptureFlag, StringComparison.OrdinalIgnoreCase));
        if (!isRequested)
        {
            return new HpFanMaxExperimentBaselineCaptureCommandResult(false, false, null, null, []);
        }

        var validationReasons = new List<string>();
        if (!args.Any(arg => string.Equals(arg, HpVictusFlag, StringComparison.OrdinalIgnoreCase)))
        {
            validationReasons.Add("Baseline capture request rejected: --hp-victus is required.");
        }

        if (!args.Any(arg => string.Equals(arg, ReadOnlyTestFlag, StringComparison.OrdinalIgnoreCase)))
        {
            validationReasons.Add("Baseline capture request rejected: --hp-wmi-readonly-test is required for approved read-only probes.");
        }

        if (args.Any(arg => string.Equals(arg, DryRunFlag, StringComparison.OrdinalIgnoreCase)))
        {
            validationReasons.Add("Baseline capture request rejected: dry-run and baseline capture cannot be combined.");
        }

        string[] payloadArguments = args
            .Where(arg => arg.StartsWith(PayloadLengthPrefix, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        HpFanMaxExperimentPayloadLengthCandidate? candidate = null;
        string? payloadBytesHypothesis = null;
        if (payloadArguments.Length == 0)
        {
            validationReasons.Add("Baseline capture request rejected: --set-fan-max-payload-length=1 or =4 is required.");
        }
        else if (payloadArguments.Length > 1)
        {
            validationReasons.Add("Baseline capture request rejected: specify exactly one payload-length hypothesis.");
        }
        else if (!TryParsePayloadLength(payloadArguments[0], out candidate, out payloadBytesHypothesis))
        {
            validationReasons.Add("Baseline capture request rejected: payload length must be exactly 1 or 4.");
        }

        return new HpFanMaxExperimentBaselineCaptureCommandResult(
            IsRequested: true,
            IsValidRequest: validationReasons.Count == 0,
            PayloadLengthCandidate: candidate,
            PayloadBytesHypothesis: payloadBytesHypothesis,
            ValidationReasons: validationReasons.ToArray());
    }

    private static bool TryParsePayloadLength(
        string argument,
        out HpFanMaxExperimentPayloadLengthCandidate? candidate,
        out string? payloadBytesHypothesis)
    {
        switch (argument[PayloadLengthPrefix.Length..])
        {
            case "1":
                candidate = HpFanMaxExperimentPayloadLengthCandidate.OneByteHypothesis;
                payloadBytesHypothesis = "01";
                return true;
            case "4":
                candidate = HpFanMaxExperimentPayloadLengthCandidate.FourByteHypothesis;
                payloadBytesHypothesis = "01-00-00-00";
                return true;
            default:
                candidate = null;
                payloadBytesHypothesis = null;
                return false;
        }
    }
}
