namespace GHelper.Hardware.Hp;

public sealed record HpFanMaxExperimentBaseline(
    bool IsHpVictus,
    string? Model,
    string? Sku,
    string? BiosVersion,
    int? ThermalPolicyVersion,
    int? FanGetCount,
    bool? FanMaxGetEnabled,
    string? FanGetLevelRaw,
    bool AllApprovedReadOnlyProbesSucceeded,
    string[] ReadOnlyProbeSummary);

public sealed record HpFanMaxExperimentFanReadback(
    bool Succeeded,
    bool? FanMaxGetEnabled,
    string? FanGetLevelRaw,
    string? Error);

public sealed record HpFanMaxExperimentWriteResult(bool Attempted, bool Succeeded, string? Error)
{
    public static HpFanMaxExperimentWriteResult NotAttempted(string reason) => new(false, false, reason);
}

public sealed record HpFanMaxExperimentRuntimeGates(
    bool IsAdministrator,
    bool IsAcPowerOnline,
    bool IsFirstWriteGateApproved,
    bool HasReviewedHumanApproval,
    bool HasSecondFourByteConfirmationApproval,
    bool HasOneTimeOneByteComparisonApproval);

public sealed record HpFanMaxExperimentRunResult(
    HpFanMaxExperimentPayload? Payload,
    HpFanMaxExperimentBaseline? Baseline,
    bool BaselineCapturePerformed,
    HpFanMaxExperimentWriteResult EnableWrite,
    HpFanMaxExperimentFanReadback? PostEnableReadback,
    HpFanMaxExperimentWriteResult RestoreWrite,
    HpFanMaxExperimentFanReadback? PostRestoreReadback,
    HpFanMaxExperimentOutcome Outcome,
    string[] BlockedReasons);

public interface IHpFanMaxExperimentReadOnlyProvider
{
    HpFanMaxExperimentBaseline CaptureBaseline();
    HpFanMaxExperimentFanReadback ReadFanStatus();
}

public interface IHpFanMaxExperimentWriteTransport
{
    HpFanMaxExperimentWriteResult TrySetFanMax(byte[] payload);
}

public interface IHpFanMaxExperimentDelay
{
    void WaitAfterEnable(TimeSpan duration);
}

public sealed class HpFanMaxExperimentRunner(
    IHpFanMaxExperimentReadOnlyProvider readOnlyProvider,
    IHpFanMaxExperimentWriteTransport writeTransport,
    IHpFanMaxExperimentDelay delay)
{
    private const string TargetSku = "7Z5Z2EA#AB8";
    private const string TargetBios = "F.31";
    private const int TargetThermalPolicyVersion = 1;
    private static readonly TimeSpan DefaultExperimentDelay = TimeSpan.FromSeconds(3);

    public HpFanMaxExperimentRunResult Run(
        HpFanMaxPulseCommandResult command,
        HpFanMaxExperimentRuntimeGates gates)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(gates);

        if (!command.IsValidRequest)
        {
            return Blocked(null, null, false, command.ValidationReasons);
        }

        IHpFanMaxPulseResearchOperation operation = command.Operation;
        if (operation.Descriptor.Kind != HpFanResearchOperationKind.FourByteMaxFanPulse ||
            operation.Descriptor.Status != HpFanResearchOperationStatus.DeveloperOnlyResearch)
        {
            return Blocked(null, null, false, ["Max Fan Pulse is not an allowed developer-only research operation."]);
        }

        return Run(command.CreateRunnerCommand(), gates, DefaultExperimentDelay);
    }

    public HpFanMaxExperimentRunResult Run(
        HpFanMaxHoldCommandResult command,
        HpFanMaxExperimentRuntimeGates gates)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(gates);

        if (!command.IsValidRequest ||
            command.HoldSeconds is not int holdSeconds ||
            !HpFanMaxHoldCommand.IsDurationAllowed(holdSeconds))
        {
            return Blocked(command.Payload, null, false, command.ValidationReasons);
        }

        IHpFanMaxPulseResearchOperation operation = command.Operation;
        if (operation.Descriptor.Kind != HpFanResearchOperationKind.FourByteMaxFanPulse ||
            operation.Descriptor.Status != HpFanResearchOperationStatus.DeveloperOnlyResearch)
        {
            return Blocked(command.Payload, null, false, ["Max Fan Hold is not an allowed developer-only research operation."]);
        }

        return Run(command.CreateRunnerCommand(), gates, TimeSpan.FromSeconds(holdSeconds));
    }

    public HpFanMaxExperimentRunResult Run(
        HpFanMaxExperimentRunnerCommandResult command,
        HpFanMaxExperimentRuntimeGates gates) =>
        Run(command, gates, DefaultExperimentDelay);

    private HpFanMaxExperimentRunResult Run(
        HpFanMaxExperimentRunnerCommandResult command,
        HpFanMaxExperimentRuntimeGates gates,
        TimeSpan waitAfterEnable)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(gates);

        if (!command.IsValidRequest || command.Payload is null)
        {
            return Blocked(command.Payload, null, false, command.ValidationReasons);
        }

        bool isApprovedFourByteConfirmation =
            command.Payload.Candidate == HpFanMaxExperimentPayloadLengthCandidate.FourByteHypothesis &&
            command.HasOneTimeFourByteApproval &&
            command.HasSecondFourByteConfirmationApproval;
        bool isApprovedOneByteComparison =
            command.Payload.Candidate == HpFanMaxExperimentPayloadLengthCandidate.OneByteHypothesis &&
            command.HasOneTimeOneByteComparisonApproval;
        if (!isApprovedFourByteConfirmation && !isApprovedOneByteComparison)
        {
            return Blocked(
                command.Payload,
                null,
                false,
                ["Only the explicitly approved second 4-byte confirmation or one-byte comparison may cross the developer experiment approval gate."]);
        }

        List<string> earlyFailures = [];
        if (!gates.IsAdministrator)
        {
            earlyFailures.Add("First-write experiment requires an elevated Administrator process.");
        }

        if (!gates.IsAcPowerOnline)
        {
            earlyFailures.Add("First-write experiment requires confirmed AC power. Unknown or offline power fails closed.");
        }

        if (earlyFailures.Count > 0)
        {
            return Blocked(command.Payload, null, false, earlyFailures);
        }

        HpFanMaxExperimentBaseline baseline;
        try
        {
            baseline = readOnlyProvider.CaptureBaseline();
        }
        catch (Exception ex)
        {
            return Blocked(command.Payload, null, false, ["Read-only baseline capture failed: " + ex.GetType().Name + "."]);
        }

        List<string> gateFailures = ValidateBaseline(baseline);
        if (!gates.IsFirstWriteGateApproved)
        {
            gateFailures.Add("The documented SetFanMax first-write gate remains NO-GO.");
        }

        if (!gates.HasReviewedHumanApproval)
        {
            gateFailures.Add("Separate reviewed human approval is not present.");
        }

        if (command.Payload.Candidate == HpFanMaxExperimentPayloadLengthCandidate.FourByteHypothesis &&
            !gates.HasSecondFourByteConfirmationApproval)
        {
            gateFailures.Add("Separate approval for the second 4-byte confirmation is not present.");
        }

        if (command.Payload.Candidate == HpFanMaxExperimentPayloadLengthCandidate.OneByteHypothesis &&
            !gates.HasOneTimeOneByteComparisonApproval)
        {
            gateFailures.Add("Separate approval for the one-byte comparison experiment is not present.");
        }

        if (gateFailures.Count > 0)
        {
            return Blocked(command.Payload, baseline, true, gateFailures);
        }

        HpFanMaxExperimentWriteResult enable = HpFanMaxExperimentWriteResult.NotAttempted("Enable was not reached.");
        HpFanMaxExperimentWriteResult restore = HpFanMaxExperimentWriteResult.NotAttempted("Restore was not needed because enable was not attempted.");
        HpFanMaxExperimentFanReadback? postEnable = null;
        HpFanMaxExperimentFanReadback? postRestore = null;
        var failures = new List<string>();

        try
        {
            enable = writeTransport.TrySetFanMax(command.Payload.EnableBytes);
            if (!enable.Attempted)
            {
                failures.Add("The selected SetFanMax enable payload was not attempted.");
            }
            else
            {
                delay.WaitAfterEnable(waitAfterEnable);
                postEnable = readOnlyProvider.ReadFanStatus();
                if (!enable.Succeeded || !postEnable.Succeeded || postEnable.FanMaxGetEnabled != true)
                {
                    failures.Add("Enable result or post-enable FanMaxGet readback did not confirm max fan enabled.");
                }
            }
        }
        catch (Exception ex)
        {
            // A transport can throw after its write boundary; restore conservatively in that uncertain state.
            enable = new HpFanMaxExperimentWriteResult(true, false, "Enable transport threw: " + ex.GetType().Name + ".");
            failures.Add("Enable phase failed: " + ex.GetType().Name + ".");
        }
        finally
        {
            if (enable.Attempted)
            {
                try
                {
                    restore = writeTransport.TrySetFanMax(command.Payload.RestoreBytes);
                    postRestore = readOnlyProvider.ReadFanStatus();
                    if (!restore.Succeeded || !postRestore.Succeeded || postRestore.FanMaxGetEnabled != false)
                    {
                        failures.Add("Matching restore payload or post-restore FanMaxGet readback did not confirm max fan disabled.");
                    }
                }
                catch (Exception ex)
                {
                    failures.Add("Restore phase failed: " + ex.GetType().Name + ".");
                }
            }
        }

        HpFanMaxExperimentOutcome outcome = failures.Count == 0 && enable.Attempted && restore.Attempted
            ? HpFanMaxExperimentOutcome.Pass
            : HpFanMaxExperimentOutcome.Fail;

        return new HpFanMaxExperimentRunResult(
            command.Payload,
            baseline,
            true,
            enable,
            postEnable,
            restore,
            postRestore,
            outcome,
            failures.ToArray());
    }

    private static List<string> ValidateBaseline(HpFanMaxExperimentBaseline baseline)
    {
        var reasons = new List<string>();
        if (!baseline.IsHpVictus || string.IsNullOrWhiteSpace(baseline.Model) || !baseline.Model.Contains("Victus", StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add("Exact target identity check failed: HP Victus model is required.");
        }

        if (!string.Equals(baseline.Sku, TargetSku, StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add("Exact target identity check failed: SKU 7Z5Z2EA#AB8 is required.");
        }

        if (!string.Equals(baseline.BiosVersion, TargetBios, StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add("Exact target identity check failed: BIOS F.31 is required.");
        }

        if (baseline.ThermalPolicyVersion != TargetThermalPolicyVersion)
        {
            reasons.Add("Exact target identity check failed: ThermalPolicyVersion 1 is required.");
        }

        if (!baseline.AllApprovedReadOnlyProbesSucceeded)
        {
            reasons.Add("Approved read-only baseline probes did not all succeed and decode.");
        }

        if (baseline.FanGetCount != 2)
        {
            reasons.Add("Baseline FanGetCount must be exactly 2.");
        }

        if (baseline.FanMaxGetEnabled != false)
        {
            reasons.Add("Baseline FanMaxGet must confirm max fan disabled before enable.");
        }

        if (string.IsNullOrWhiteSpace(baseline.FanGetLevelRaw))
        {
            reasons.Add("Baseline raw FanGetLevel values are required.");
        }

        return reasons;
    }

    private static HpFanMaxExperimentRunResult Blocked(
        HpFanMaxExperimentPayload? payload,
        HpFanMaxExperimentBaseline? baseline,
        bool baselineCapturePerformed,
        IEnumerable<string> reasons) =>
        new(
            payload,
            baseline,
            baselineCapturePerformed,
            HpFanMaxExperimentWriteResult.NotAttempted("Blocked before SetFanMax enable."),
            null,
            HpFanMaxExperimentWriteResult.NotAttempted("Blocked before SetFanMax restore."),
            null,
            HpFanMaxExperimentOutcome.Unknown,
            reasons.ToArray());
}
