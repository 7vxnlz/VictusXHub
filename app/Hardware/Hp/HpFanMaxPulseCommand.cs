namespace GHelper.Hardware.Hp;

public sealed record HpFanMaxPulseCommandResult(
    bool IsRequested,
    bool IsValidRequest,
    HpFanMaxExperimentManualObservation ManualObservation,
    string[] ValidationReasons)
{
    public bool ShouldExit => IsRequested;
    public IHpFanMaxPulseResearchOperation Operation { get; } = new FourByteMaxFanPulseResearchOperation();

    public HpFanMaxExperimentRunnerCommandResult CreateRunnerCommand()
    {
        if (!IsValidRequest ||
            Operation.Descriptor.Kind != HpFanResearchOperationKind.FourByteMaxFanPulse ||
            Operation.Descriptor.Status != HpFanResearchOperationStatus.DeveloperOnlyResearch)
        {
            throw new InvalidOperationException("An invalid Max Fan Pulse request cannot create a runner command.");
        }

        return new HpFanMaxExperimentRunnerCommandResult(
            true,
            true,
            new HpFanMaxExperimentPayload(
                HpFanMaxExperimentPayloadLengthCandidate.FourByteHypothesis,
                Operation.EnablePayload.ToArray(),
                Operation.RestorePayload.ToArray()),
            true,
            true,
            false,
            ManualObservation,
            []);
    }
}

public static class HpFanMaxPulseCommand
{
    public const string PulseFlag = "--hp-fan-max-pulse";
    public const string HpVictusFlag = "--hp-victus";
    public const string ReadOnlyTestFlag = "--hp-wmi-readonly-test";
    public const string AcknowledgementFlag = "--i-understand-this-can-affect-fans";
    public const string ApprovalFlag = "--i-approve-4-byte-max-fan-pulse";

    public static HpFanMaxPulseCommandResult Parse(IEnumerable<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        string[] args = arguments.ToArray();
        bool isRequested = args.Any(argument => string.Equals(argument, PulseFlag, StringComparison.OrdinalIgnoreCase));
        if (!isRequested)
        {
            return new HpFanMaxPulseCommandResult(false, false, new HpFanMaxExperimentManualObservation(null, null, null, []), []);
        }

        List<string> reasons = [];
        RequireFlag(args, HpVictusFlag, reasons);
        RequireFlag(args, ReadOnlyTestFlag, reasons);
        RequireFlag(args, AcknowledgementFlag, reasons);
        RequireFlag(args, ApprovalFlag, reasons);

        HpFanMaxExperimentManualObservation manualObservation = HpFanMaxExperimentManualObservation.Parse(args);
        reasons.AddRange(manualObservation.ValidationReasons);

        if (args.Any(argument => argument.StartsWith(HpFanMaxExperimentRunnerCommand.PayloadLengthPrefix, StringComparison.OrdinalIgnoreCase)))
        {
            reasons.Add("Max Fan Pulse request rejected: payload length is fixed to the approved four-byte experiment pair.");
        }

        if (args.Any(argument => string.Equals(argument, HpFanMaxExperimentRunnerCommand.ExperimentFlag, StringComparison.OrdinalIgnoreCase)))
        {
            reasons.Add("Max Fan Pulse request rejected: it cannot be combined with the generic fan-write experiment command.");
        }

        return new HpFanMaxPulseCommandResult(true, reasons.Count == 0, manualObservation, reasons.ToArray());
    }

    private static void RequireFlag(IEnumerable<string> arguments, string flag, ICollection<string> reasons)
    {
        if (!arguments.Any(argument => string.Equals(argument, flag, StringComparison.OrdinalIgnoreCase)))
        {
            reasons.Add("Max Fan Pulse request rejected: " + flag + " is required.");
        }
    }
}
