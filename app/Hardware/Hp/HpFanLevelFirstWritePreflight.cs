using System.Text.Json;

namespace GHelper.Hardware.Hp;

internal sealed record HpFanLevelTargetEvidence(string Manufacturer, string Model, string Sku, string Bios, int ThermalPolicyVersion);

internal sealed record HpFanLevelFirstWriteRequest
{
    public int? RequestedPercent { get; init; }
    public int? Fan0RawCandidate { get; init; }
    public int? Fan1RawCandidate { get; init; }
    public HpFanLevelTargetEvidence? Target { get; init; }
    public bool? IsAdministrator { get; init; }
    public bool? AcOnline { get; init; }
    public bool? BaselineTelemetrySucceeded { get; init; }
    public string? BaselineEvidenceReference { get; init; }
    public string? RecoveryPlanReference { get; init; }
    public bool? RecoveryPlanReviewed { get; init; }
    public string? LogDestination { get; init; }
    public bool? AppendOnlyLoggingRequired { get; init; }
    public bool? PhysicalFanResponseObserved { get; init; }
    public bool? RestoreObserved { get; init; }
    public bool? UnsafeAbortObserved { get; init; }
    public string? ManualObservationNotes { get; init; }
}

// Offline declarations only. Neither a successful evaluation nor JSON can authorize execution.
internal sealed class HpFanLevelFirstWritePlan
{
    internal HpFanLevelFirstWritePlan(HpFanLevelFirstWriteRequest request, int? raw, List<string> reasons)
    {
        Request = request;
        MappedRawValue = raw;
        Reasons = reasons.AsReadOnly();
    }

    public HpFanLevelFirstWriteRequest Request { get; }
    public int? MappedRawValue { get; }
    public IReadOnlyList<string> Reasons { get; }
    public bool PreflightSatisfied => Reasons.Count == 0;
    public string Status => PreflightSatisfied ? "Preflight satisfied; not executable / not validated" : "Preflight blocked; not executable / not validated";
    public string RequiredTarget => "HP / Victus 16-s0035nt or Victus by HP Gaming Laptop 16-s0xxx / 7Z5Z2EA#AB8 / F.31 / ThermalPolicyVersion 1";
    public string RequiredEvidence => "Administrator, AC online, successful baseline with reference, reviewed recovery plan, append-only log destination";
    public string MappingFormula => "p == 100 ? 100 : p * 55 / 100 (integer division); admitted offline range 2..99 only";
    public string EvidenceConfidence => "Close-device evidence; F.30 is not F.31 validation";
    public string CommandCandidate => "0x20008 / 0x2E";
    public IReadOnlyList<byte>? CandidatePayload => PreflightSatisfied && MappedRawValue is int raw
        ? Array.AsReadOnly(new byte[] { (byte)raw, (byte)raw, 0, 0 }) : null;
    public bool IsExecutable => false;
    public bool WriteExecuted => false;
    public bool NoHardwareInvocation => true;
    public bool NoWmiInvocation => true;
    public bool FirstWriteReady => false;
    public bool NormalFanControlReady => false;
    public int? DeviceValidatedInputLength => null;
    public string ToJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
}

internal static class HpFanLevelFirstWritePreflight
{
    public static HpFanLevelFirstWritePlan Evaluate(HpFanLevelFirstWriteRequest? request)
    {
        request ??= new();
        List<string> reasons = [];
        int? raw = request.RequestedPercent is >= 2 and <= 99 ? request.RequestedPercent * 55 / 100 : null;
        if (raw is null) reasons.Add("Requested percent must be 2..99; endpoints are excluded.");
        if (request.Fan0RawCandidate is not (>= 1 and <= 54) || request.Fan1RawCandidate is not (>= 1 and <= 54))
            reasons.Add("Both raw candidates must be 1..54.");
        if (request.Fan0RawCandidate != request.Fan1RawCandidate) reasons.Add("Asymmetric candidates are prohibited.");
        if (raw is not null && (request.Fan0RawCandidate != raw || request.Fan1RawCandidate != raw))
            reasons.Add("Raw candidates must match the requested percentage mapping.");
        var target = request.Target;
        if (target is null || target.Manufacturer != "HP"
            || (target.Model != "Victus 16-s0035nt" && target.Model != "Victus by HP Gaming Laptop 16-s0xxx")
            || target.Sku != "7Z5Z2EA#AB8" || target.Bios != "F.31" || target.ThermalPolicyVersion != 1)
            reasons.Add("Exact reviewed target identity is required.");
        if (request.IsAdministrator != true) reasons.Add("Administrator evidence is required.");
        if (request.AcOnline != true) reasons.Add("AC online evidence is required.");
        if (request.BaselineTelemetrySucceeded != true || string.IsNullOrWhiteSpace(request.BaselineEvidenceReference))
            reasons.Add("Successful baseline telemetry and evidence reference are required.");
        if (request.RecoveryPlanReviewed != true || string.IsNullOrWhiteSpace(request.RecoveryPlanReference))
            reasons.Add("A reviewed recovery plan reference is required.");
        if (request.AppendOnlyLoggingRequired != true || string.IsNullOrWhiteSpace(request.LogDestination))
            reasons.Add("An append-only log destination is required.");
        if (request.UnsafeAbortObserved == true) reasons.Add("Unsafe abort observation blocks preflight.");
        string? notes = request.ManualObservationNotes;
        if (notes is not null)
            notes = new string(notes.Take(1024).Select(c => char.IsControl(c) ? ' ' : c).ToArray());
        return new(request with { ManualObservationNotes = notes }, raw, reasons);
    }
}
