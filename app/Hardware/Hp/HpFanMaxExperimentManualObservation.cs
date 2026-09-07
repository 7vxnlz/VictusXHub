namespace GHelper.Hardware.Hp;

public sealed record HpFanMaxExperimentManualObservation(
    bool? PhysicalFanResponseObserved,
    bool? RestoreObserved,
    string? ManualObservationNotes,
    string[] ValidationReasons)
{
    public const string PhysicalFanResponseObservedPrefix = "--physical-fan-response-observed=";
    public const string RestoreObservedPrefix = "--restore-observed=";
    public const string ManualObservationNotesPrefix = "--manual-observation-notes=";
    public const int MaximumNotesLength = 512;

    public static HpFanMaxExperimentManualObservation Parse(IEnumerable<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        string[] args = arguments.ToArray();
        List<string> reasons = [];
        bool? physicalFanResponseObserved = ParseBooleanOption(
            args,
            PhysicalFanResponseObservedPrefix,
            "physical fan response observation",
            reasons);
        bool? restoreObserved = ParseBooleanOption(
            args,
            RestoreObservedPrefix,
            "restore observation",
            reasons);
        string? notes = ParseNotesOption(args, reasons);

        return new HpFanMaxExperimentManualObservation(
            physicalFanResponseObserved,
            restoreObserved,
            notes,
            reasons.ToArray());
    }

    private static bool? ParseBooleanOption(
        IEnumerable<string> arguments,
        string prefix,
        string displayName,
        ICollection<string> reasons)
    {
        string[] matches = arguments.Where(argument => argument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray();
        if (matches.Length == 0)
        {
            return null;
        }

        if (matches.Length != 1 || !bool.TryParse(matches[0][prefix.Length..], out bool value))
        {
            reasons.Add($"SetFanMax experiment request rejected: {displayName} must be supplied once as true or false.");
            return null;
        }

        return value;
    }

    private static string? ParseNotesOption(IEnumerable<string> arguments, ICollection<string> reasons)
    {
        string[] matches = arguments.Where(argument => argument.StartsWith(ManualObservationNotesPrefix, StringComparison.OrdinalIgnoreCase)).ToArray();
        if (matches.Length == 0)
        {
            return null;
        }

        if (matches.Length != 1)
        {
            reasons.Add("SetFanMax experiment request rejected: manual observation notes may be supplied once only.");
            return null;
        }

        string sanitized = new string(matches[0][ManualObservationNotesPrefix.Length..]
            .Select(character => char.IsControl(character) ? ' ' : character)
            .ToArray())
            .Trim();

        return sanitized.Length <= MaximumNotesLength
            ? sanitized
            : sanitized[..MaximumNotesLength];
    }
}
