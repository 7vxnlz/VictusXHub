namespace GHelper.Hardware.Hp;

public sealed record HpFanMaxExperimentDryRunCommandResult(
    bool IsRequested,
    bool IsValidRequest,
    HpFanMaxExperimentLogRecord? LogRecord)
{
    public bool ShouldExit => IsRequested;
}

public static class HpFanMaxExperimentDryRunCommand
{
    public const string DryRunFlag = "--hp-fan-write-experiment-dry-run";
    public const string HpVictusFlag = "--hp-victus";
    public const string PayloadLengthPrefix = "--set-fan-max-payload-length=";
    private const string ReadOnlyTestFlag = "--hp-wmi-readonly-test";

    public static HpFanMaxExperimentDryRunCommandResult Parse(IEnumerable<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        string[] args = arguments.ToArray();
        bool isRequested = args.Any(IsDryRunFlag);
        if (!isRequested)
        {
            return new HpFanMaxExperimentDryRunCommandResult(false, false, null);
        }

        List<string> validationReasons = [];
        bool hasHpVictus = args.Any(arg => string.Equals(arg, HpVictusFlag, StringComparison.OrdinalIgnoreCase));
        if (!hasHpVictus)
        {
            validationReasons.Add("Dry-run request rejected: --hp-victus is required.");
        }

        if (args.Any(arg => string.Equals(arg, ReadOnlyTestFlag, StringComparison.OrdinalIgnoreCase)))
        {
            validationReasons.Add("Dry-run request rejected: --hp-wmi-readonly-test is not permitted.");
        }

        string[] payloadArguments = args
            .Where(arg => arg.StartsWith(PayloadLengthPrefix, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        HpFanMaxExperimentPayloadLengthCandidate? candidate = null;
        string? payloadBytesHypothesis = null;
        if (payloadArguments.Length == 0)
        {
            validationReasons.Add("Dry-run request rejected: --set-fan-max-payload-length=1 or =4 is required.");
        }
        else if (payloadArguments.Length > 1)
        {
            validationReasons.Add("Dry-run request rejected: specify exactly one payload-length hypothesis.");
        }
        else if (!TryParsePayloadLength(payloadArguments[0], out candidate, out payloadBytesHypothesis))
        {
            validationReasons.Add("Dry-run request rejected: payload length must be exactly 1 or 4.");
        }

        HpFanMaxExperimentLogRecord blockedRecord = HpFanMaxExperimentLogRecord
            .CreateBlocked(candidate, payloadBytesHypothesis) with
        {
            EnableResult = "Not attempted: dry-run command does not invoke WMI or hardware.",
            RestoreResult = "Not attempted: dry-run command does not invoke WMI or hardware.",
            ManualObservationNotes = "Developer-only dry-run log. No WMI or hardware action was attempted.",
            BlockedReasons =
            [
                .. HpFanMaxExperimentLogRecord.CreateBlocked().BlockedReasons,
                .. validationReasons
            ]
        };

        return new HpFanMaxExperimentDryRunCommandResult(
            IsRequested: true,
            IsValidRequest: validationReasons.Count == 0,
            LogRecord: blockedRecord);
    }

    private static bool IsDryRunFlag(string argument) =>
        string.Equals(argument, DryRunFlag, StringComparison.OrdinalIgnoreCase);

    private static bool TryParsePayloadLength(
        string argument,
        out HpFanMaxExperimentPayloadLengthCandidate? candidate,
        out string? payloadBytesHypothesis)
    {
        string value = argument[PayloadLengthPrefix.Length..];
        switch (value)
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
