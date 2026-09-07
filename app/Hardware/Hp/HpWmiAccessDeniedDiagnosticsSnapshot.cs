namespace GHelper.Hardware.Hp;

public sealed record HpRelatedServiceSnapshot(
    string Name,
    string State,
    string StartMode);

public sealed record HpWmiAccessDeniedDiagnosticsSnapshot(
    bool ProcessElevated,
    string WindowsIdentitySummary,
    bool WmiNamespaceReadable,
    bool HpBIntMClassReadable,
    bool HpBIntMMethodMetadataReadable,
    HpRelatedServiceSnapshot[] HpRelatedServices,
    string[] AccessDeniedInvestigationErrors);
