using System.Globalization;

namespace GHelper.Hardware.Hp;

public sealed record HpFanMaxHoldCommandResult(
    bool IsRequested,
    bool IsValidRequest,
    int? HoldSeconds,
    HpFanMaxExperimentManualObservation ManualObservation,
    string[] ValidationReasons)
{
    public bool ShouldExit => IsRequested;
    public string FormatCliSummary() =>
        "SetFanMax developer-only Max Fan Hold. Requested pre-restore wait seconds: " +
        (HoldSeconds?.ToString(CultureInfo.InvariantCulture) ?? "unspecified or malformed") + ". " +
        HpFanMaxHoldCommand.DurationSemantics;

    public IHpFanMaxPulseResearchOperation Operation { get; } = new FourByteMaxFanPulseResearchOperation();

    public HpFanMaxExperimentPayload Payload => new(
        HpFanMaxExperimentPayloadLengthCandidate.FourByteHypothesis,
        Operation.EnablePayload.ToArray(),
        Operation.RestorePayload.ToArray());

    public HpFanMaxExperimentRunnerCommandResult CreateRunnerCommand()
    {
        if (!IsValidRequest ||
            HoldSeconds is not int holdSeconds ||
            !HpFanMaxHoldCommand.IsDurationAllowed(holdSeconds) ||
            Operation.Descriptor.Kind != HpFanResearchOperationKind.FourByteMaxFanPulse ||
            Operation.Descriptor.Status != HpFanResearchOperationStatus.DeveloperOnlyResearch)
        {
            throw new InvalidOperationException("An invalid Max Fan Hold request cannot create a runner command.");
        }

        return new HpFanMaxExperimentRunnerCommandResult(
            true,
            true,
            Payload,
            true,
            true,
            false,
            ManualObservation,
            []);
    }

    public HpFanMaxExperimentLogRecord CreateLogRecord(HpFanMaxExperimentRunResult result) =>
        HpFanMaxExperimentRunLogMapper.Create(result, ManualObservation) with
        {
            DeveloperOnlyOperation = HpFanMaxHoldCommand.DeveloperOnlyOperationName,
            RequestedHoldSeconds = HoldSeconds
        };
}

public static class HpFanMaxHoldCommand
{
    public const string HoldFlag = "--hp-fan-max-hold";
    public const string HpVictusFlag = "--hp-victus";
    public const string ReadOnlyTestFlag = "--hp-wmi-readonly-test";
    public const string AcknowledgementFlag = "--i-understand-this-can-affect-fans";
    public const string ApprovalFlag = "--i-approve-4-byte-max-fan-hold";
    public const string HoldSecondsPrefix = "--max-fan-hold-seconds=";
    public const string DeveloperOnlyOperationName = "DeveloperOnlyFourByteMaxFanHold";
    public const string DurationSemantics =
        "--max-fan-hold-seconds specifies a bounded pre-restore wait (10-180 seconds). " +
        "Readback follows the wait before the matching restore attempt; this is not an exact restore deadline. " +
        "Physical fan duration is BIOS-dependent and not validated; the fan may remain high after restore or wait expiry. " +
        "FanMaxGet remains inconclusive. Normal/user-facing fan control remains NO-GO.";
    public const int MinimumHoldSeconds = 10;
    public const int MaximumHoldSeconds = 180;

    public static HpFanMaxHoldCommandResult Parse(IEnumerable<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        string[] args = arguments.ToArray();
        bool isRequested = args.Any(argument => string.Equals(argument, HoldFlag, StringComparison.OrdinalIgnoreCase));
        if (!isRequested)
        {
            return new HpFanMaxHoldCommandResult(
                false,
                false,
                null,
                new HpFanMaxExperimentManualObservation(null, null, null, []),
                []);
        }

        List<string> reasons = [];
        RequireFlag(args, HpVictusFlag, reasons);
        RequireFlag(args, ReadOnlyTestFlag, reasons);
        RequireFlag(args, AcknowledgementFlag, reasons);
        RequireFlag(args, ApprovalFlag, reasons);

        int? holdSeconds = ParseHoldSeconds(args, reasons);
        HpFanMaxExperimentManualObservation manualObservation = HpFanMaxExperimentManualObservation.Parse(args);
        reasons.AddRange(manualObservation.ValidationReasons);

        if (args.Any(argument => argument.StartsWith(HpFanMaxExperimentRunnerCommand.PayloadLengthPrefix, StringComparison.OrdinalIgnoreCase)))
        {
            reasons.Add("Max Fan Hold request rejected: payload length is fixed to the approved four-byte experiment pair.");
        }

        RejectFlag(args, HpFanMaxPulseCommand.PulseFlag, reasons);
        RejectFlag(args, HpFanMaxPulseCommand.ApprovalFlag, reasons);
        RejectFlag(args, HpFanMaxExperimentRunnerCommand.ExperimentFlag, reasons);
        RejectFlag(args, HpFanMaxExperimentRunnerCommand.OneTimeFourByteApprovalFlag, reasons);
        RejectFlag(args, HpFanMaxExperimentRunnerCommand.SecondFourByteConfirmationApprovalFlag, reasons);
        RejectFlag(args, HpFanMaxExperimentRunnerCommand.OneTimeOneByteComparisonApprovalFlag, reasons);
        RejectFlag(args, HpFanMaxExperimentBaselineCaptureCommand.BaselineCaptureFlag, reasons);
        RejectFlag(args, HpFanMaxExperimentDryRunCommand.DryRunFlag, reasons);

        return new HpFanMaxHoldCommandResult(
            true,
            reasons.Count == 0,
            holdSeconds,
            manualObservation,
            reasons.ToArray());
    }

    public static bool IsDurationAllowed(int seconds) =>
        seconds is >= MinimumHoldSeconds and <= MaximumHoldSeconds;

    private static int? ParseHoldSeconds(IEnumerable<string> arguments, ICollection<string> reasons)
    {
        string[] matches = arguments
            .Where(argument => argument.StartsWith(HoldSecondsPrefix, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (matches.Length == 0)
        {
            reasons.Add("Max Fan Hold request rejected: exactly one " + HoldSecondsPrefix + "<seconds> argument is required for the pre-restore wait.");
            return null;
        }

        if (matches.Length > 1)
        {
            reasons.Add("Max Fan Hold request rejected: the pre-restore wait must be supplied exactly once.");
            return null;
        }

        string value = matches[0][HoldSecondsPrefix.Length..];
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int seconds))
        {
            reasons.Add("Max Fan Hold request rejected: pre-restore wait must be a whole number of seconds.");
            return null;
        }

        if (!IsDurationAllowed(seconds))
        {
            reasons.Add($"Max Fan Hold request rejected: pre-restore wait must be between {MinimumHoldSeconds} and {MaximumHoldSeconds} seconds inclusive.");
        }

        return seconds;
    }

    private static void RequireFlag(IEnumerable<string> arguments, string flag, ICollection<string> reasons)
    {
        if (!arguments.Any(argument => string.Equals(argument, flag, StringComparison.OrdinalIgnoreCase)))
        {
            reasons.Add("Max Fan Hold request rejected: " + flag + " is required.");
        }
    }

    private static void RejectFlag(IEnumerable<string> arguments, string flag, ICollection<string> reasons)
    {
        if (arguments.Any(argument => string.Equals(argument, flag, StringComparison.OrdinalIgnoreCase)))
        {
            reasons.Add("Max Fan Hold request rejected: it cannot be combined with " + flag + ".");
        }
    }
}
