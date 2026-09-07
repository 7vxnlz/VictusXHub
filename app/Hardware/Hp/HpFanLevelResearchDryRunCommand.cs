using System.Globalization;
using System.Text.Json;

namespace GHelper.Hardware.Hp;

// Descriptive research data only; this is deliberately not a transport command or operation contract.
public sealed record HpFanLevelResearchDryRunRecord(
    byte? RawLevelCandidate,
    IReadOnlyList<string> ValidationReasons)
{
    public bool UsesPercentageMapping { get; init; }
    public int? RequestedPercentCandidate { get; init; }
    public int SchemaVersion => UsesPercentageMapping ? 2 : 1;
    public DateTimeOffset TimestampUtc { get; } = DateTimeOffset.UtcNow;
    public string Operation => "SetFanLevelResearchDryRun";
    public bool IsValidCandidate => ValidationReasons.Count == 0 && (UsesPercentageMapping
        ? RequestedPercentCandidate is >= 0 and <= 100 && RawLevelCandidate is null
        : RawLevelCandidate.HasValue && RequestedPercentCandidate is null);
    public string CandidateModel => UsesPercentageMapping ? "Victus V1 shared-percentage research" : "Deprecated legacy raw-byte research only";
    public int? ReferenceDefaultV1Ceiling => UsesPercentageMapping ? 55 : null;
    public string? MappingFormula => UsesPercentageMapping ? "p = clamp(percent, 0, 100); raw = p == 100 ? 100 : (p * 55) / 100 (integer division)" : null;
    public string? MappingSource => UsesPercentageMapping ? "omencore b39b44978902606aa708cc0d78bcfd87e95fd88b; src/OmenCoreApp/Hardware/WmiFanController.cs MapFanPercentToWmiLevel; HpWmiBios.cs DetectMaxFanLevel / SetFanLevel; default V1 ceiling, no overrides" : null;
    public string EvidenceConfidence => UsesPercentageMapping ? "Close-device evidence" : "Unknown";
    public string TargetBios => "F.31";
    public bool TargetBiosValidated => false;
    public byte? MappedRawLevelCandidate => IsValidCandidate && UsesPercentageMapping
        ? (byte)(RequestedPercentCandidate == 100 ? 100 : RequestedPercentCandidate!.Value * 55 / 100) : null;
    public string BoundaryCaution => "Mapping only: upstream exact 0% routes to auto recovery; 100% routes to MaxFan first, with raw 100 fallback. Small positive percentages can truncate to raw zero. None of those hardware behaviors is implemented or approved here.";
    public string Status => "Not executable / not validated";
    public string WmiNamespaceCandidate => @"root\wmi";
    public string WmiClassCandidate => "hpqBIntM";
    public string WmiMethodCandidate => "hpqBIOSInt0";
    public string CommandCandidate => "0x20008";
    public string CommandTypeCandidate => "0x2E";
    public string PayloadShapeHypothesis => UsesPercentageMapping
        ? "Two equal mapped fan arguments followed by two zero bytes; F.31 ABI and physical fan mapping unvalidated"
        : "Two equal raw bytes; fan mapping and input ABI unvalidated";
    public string? PayloadHexCandidate => !IsValidCandidate ? null : UsesPercentageMapping
        ? $"{MappedRawLevelCandidate:X2}-{MappedRawLevelCandidate:X2}-00-00" : $"{RawLevelCandidate:X2}-{RawLevelCandidate:X2}";
    public int? CandidateInputLength => IsValidCandidate ? (UsesPercentageMapping ? 4 : 2) : null;
    public string LevelUnits => "Unknown; raw byte only, not RPM or percent";
    public bool IsExecutable => false;
    public bool WriteExecuted => false;
    public bool WmiInvoked => false;
    public bool NoHardwareInvocation => true;
    public bool NoWmiInvocation => true;
    public bool FirstWriteGateSatisfied => false;
    public bool FirstWriteReady => false;
    public bool NormalFanControlReady => false;
    public int? DeviceValidatedInputLength => null;
    public bool NormalControlValidated => false;
    public bool UserFacingControlAllowed => false;
    public string NormalFanControlDecision => "NO-GO";
    public string SafetyNote => UsesPercentageMapping
        ? "Close-device F.30 mapping is not F.31 validation, physical units, or a safe write range. No first-write value selected; normal fan control NO-GO. No hardware action is available."
        : "Deprecated developer-only raw serialization. The 0-255 bound is byte representation only, not a safe hardware range. Raw 128 / 80-80 is not 128% or an approved cleaning request. No restore semantics are validated. No hardware action is available.";

    public string ToJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
}

public sealed record HpFanLevelResearchDryRunCommandResult(
    bool ShouldExit,
    HpFanLevelResearchDryRunRecord? Record)
{
    public bool IsValidRequest => Record?.IsValidCandidate == true;
}

public static class HpFanLevelResearchDryRunCommand
{
    public const string DryRunFlag = "--hp-fan-level-research-dry-run";
    public const string HpVictusFlag = "--hp-victus";
    public const string LevelPrefix = "--fan-level-candidate=";
    public const string PercentPrefix = "--fan-percent-candidate=";
    private const string PercentFlag = "--fan-percent-candidate";
    private const string LevelFlag = "--fan-level-candidate";

    public static HpFanLevelResearchDryRunCommandResult Parse(IEnumerable<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        string[] args = arguments.ToArray();

        // Consume malformed/mixed research requests before any hardware-capable startup route.
        if (!args.Any(arg => IsFlagOrAssignment(arg, DryRunFlag) || IsFlagOrAssignment(arg, LevelFlag) || IsFlagOrAssignment(arg, PercentFlag)))
        {
            return new(false, null);
        }

        List<string> reasons = [];
        if (args.Count(arg => EqualsFlag(arg, DryRunFlag)) != 1)
        {
            reasons.Add("Exactly one --hp-fan-level-research-dry-run flag is required.");
        }

        if (args.Count(arg => EqualsFlag(arg, HpVictusFlag)) != 1)
        {
            reasons.Add("Exactly one --hp-victus flag is required.");
        }

        if (args.Any(arg => !EqualsFlag(arg, DryRunFlag) && !EqualsFlag(arg, HpVictusFlag)
            && !arg.StartsWith(LevelPrefix, StringComparison.OrdinalIgnoreCase)
            && !arg.StartsWith(PercentPrefix, StringComparison.OrdinalIgnoreCase)))
        {
            reasons.Add("Only HP mode, the dry-run flag, and one candidate are permitted; probes, experiments, approvals, and other arguments are rejected.");
        }

        string[] levels = args.Where(arg => arg.StartsWith(LevelPrefix, StringComparison.OrdinalIgnoreCase)).ToArray();
        string[] percentages = args.Where(arg => arg.StartsWith(PercentPrefix, StringComparison.OrdinalIgnoreCase)).ToArray();
        bool percentageMode = args.Any(arg => IsFlagOrAssignment(arg, PercentFlag));
        byte? candidate = null;
        int? percent = null;
        if (percentages.Length == 1 && levels.Length == 0 && int.TryParse(percentages[0][PercentPrefix.Length..], NumberStyles.None,
            CultureInfo.InvariantCulture, out int requested) && requested is >= 0 and <= 100)
        {
            percent = requested;
        }
        else if (!percentageMode && levels.Length == 1 && byte.TryParse(levels[0][LevelPrefix.Length..], NumberStyles.None,
            CultureInfo.InvariantCulture, out byte rawLevel))
        {
            candidate = rawLevel;
        }
        else
        {
            reasons.Add("Require exactly one --fan-percent-candidate=<integer 0-100> (preferred) or deprecated --fan-level-candidate=<integer 0-255>, never both. Neither is a validated hardware range.");
        }

        return new(true, new(reasons.Count == 0 ? candidate : null, reasons.ToArray())
        {
            UsesPercentageMapping = percentageMode,
            RequestedPercentCandidate = percent
        });
    }

    private static bool EqualsFlag(string argument, string flag) =>
        string.Equals(argument, flag, StringComparison.OrdinalIgnoreCase);

    private static bool IsFlagOrAssignment(string argument, string flag) =>
        EqualsFlag(argument, flag) || argument.StartsWith(flag + "=", StringComparison.OrdinalIgnoreCase);
}
