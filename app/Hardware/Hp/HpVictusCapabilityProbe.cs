using System.Management;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace GHelper.Hardware.Hp;

public static class HpVictusCapabilityProbe
{
    private const int CapabilityReportSchemaVersion = HpVictusCapabilityReportMetadata.SchemaVersion;
    private const string CapabilityReportGeneratedBy = "VictusX";
    private const string CapabilityReportMode = "HP read-only diagnostic";
    private const string CapabilityReportSource = "Startup capability snapshot; explicit probe data is developer-only.";
    private const string CimV2ScopePath = @"\\.\root\cimv2";
    private const string HpWmiScopePath = @"\\.\root\wmi";

    private static readonly JsonSerializerOptions ReportJsonOptions = new()
    {
        WriteIndented = true
    };

    public static string ReportPath => HpDiagnosticPaths.CapabilityReportPath;

    public static HpVictusCapabilitySnapshot Probe()
    {
        var errors = new List<string>();

        var computerSystem = QueryFirst(CimV2ScopePath, "SELECT Manufacturer, Model, SystemFamily, SystemSKUNumber FROM Win32_ComputerSystem", errors, "Win32_ComputerSystem");
        var computerProduct = QueryFirst(CimV2ScopePath, "SELECT Vendor, Name FROM Win32_ComputerSystemProduct", errors, "Win32_ComputerSystemProduct");
        var bios = QueryFirst(CimV2ScopePath, "SELECT SMBIOSBIOSVersion FROM Win32_BIOS", errors, "Win32_BIOS");

        string manufacturer = FirstNonEmpty(computerSystem.GetValueOrDefault("Manufacturer"), computerProduct.GetValueOrDefault("Vendor"));
        string model = FirstNonEmpty(computerSystem.GetValueOrDefault("Model"), computerProduct.GetValueOrDefault("Name"));
        string systemFamily = computerSystem.GetValueOrDefault("SystemFamily") ?? string.Empty;
        string systemSku = computerSystem.GetValueOrDefault("SystemSKUNumber") ?? string.Empty;
        string productVendor = computerProduct.GetValueOrDefault("Vendor") ?? string.Empty;
        string productName = computerProduct.GetValueOrDefault("Name") ?? string.Empty;
        string biosVersion = bios.GetValueOrDefault("SMBIOSBIOSVersion") ?? string.Empty;

        var hpWmiSnapshot = new HpWmiReadOnlyClient().Probe();
        foreach (string error in hpWmiSnapshot.Errors)
        {
            errors.Add("HP WMI read-only client: " + error);
        }

        var accessDeniedDiagnostics = HpWmiAccessDeniedDiagnostics.Probe();
        foreach (string error in accessDeniedDiagnostics.AccessDeniedInvestigationErrors)
        {
            errors.Add("HP WMI access denied diagnostics: " + error);
        }

        var invocationClient = new HpWmiInvocationClient(global::Logger.WriteLine);
        var invocationSandboxStatus = invocationClient.ValidateCatalog(
            hpWmiSnapshot,
            HpBiosWmiCommandCatalog.Definitions);
        var systemDesignDataDryRun = invocationClient.DryRun(
            "SystemDesignData",
            hpWmiSnapshot,
            HpBiosWmiCommandCatalog.Definitions);
        var fanGetCountDryRun = invocationClient.DryRun(
            "FanGetCount",
            hpWmiSnapshot,
            HpBiosWmiCommandCatalog.Definitions);
        var fanMaxGetDryRun = invocationClient.DryRun(
            "FanMaxGet",
            hpWmiSnapshot,
            HpBiosWmiCommandCatalog.Definitions);
        var fanGetLevelDryRun = invocationClient.DryRun(
            "FanGetLevel",
            hpWmiSnapshot,
            HpBiosWmiCommandCatalog.Definitions);
        bool hpVictusMode = global::AppConfig.IsHpVictusHardwareMode();
        bool hpWmiReadOnlyTestMode = global::AppConfig.IsHpWmiReadOnlyTestMode();
        bool hpWmiInvocationRequiresElevation = true;
        bool systemDesignDataInvocationAllowed =
            hpVictusMode &&
            hpWmiReadOnlyTestMode &&
            accessDeniedDiagnostics.ProcessElevated;
        bool fanGetCountInvocationAllowed =
            hpVictusMode &&
            hpWmiReadOnlyTestMode &&
            accessDeniedDiagnostics.ProcessElevated &&
            fanGetCountDryRun.Success &&
            !fanGetCountDryRun.Invoked;
        bool fanMaxGetInvocationAllowed =
            hpVictusMode &&
            hpWmiReadOnlyTestMode &&
            accessDeniedDiagnostics.ProcessElevated &&
            fanMaxGetDryRun.Success &&
            !fanMaxGetDryRun.Invoked;
        bool fanGetLevelInvocationAllowed =
            hpVictusMode &&
            hpWmiReadOnlyTestMode &&
            accessDeniedDiagnostics.ProcessElevated &&
            fanGetLevelDryRun.Success &&
            !fanGetLevelDryRun.Invoked;
        string hpWmiInvocationBlockedReason = GetHpWmiInvocationBlockedReason(
            hpVictusMode,
            hpWmiReadOnlyTestMode,
            accessDeniedDiagnostics.ProcessElevated);
        string hpWmiRecommendedNextStep = GetHpWmiRecommendedNextStep(
            hpVictusMode,
            hpWmiReadOnlyTestMode,
            accessDeniedDiagnostics.ProcessElevated);
        var systemDesignDataInvocation = TryInvokeSystemDesignData(
            invocationClient,
            hpWmiSnapshot,
            HpBiosWmiCommandCatalog.Definitions,
            hpWmiReadOnlyTestMode,
            accessDeniedDiagnostics.ProcessElevated);
        HpSystemDesignDataReportDecodeResult systemDesignDataDecode = HpSystemDesignDataReportDecoder.TryDecode(
            systemDesignDataInvocation.CommandName,
            systemDesignDataInvocation.Success,
            systemDesignDataInvocation.Invoked,
            systemDesignDataInvocation.ReturnedBytes);
        var fanGetCountInvocation = TryInvokeFanGetCount(
            invocationClient,
            hpWmiSnapshot,
            HpBiosWmiCommandCatalog.Definitions,
            hpWmiReadOnlyTestMode,
            accessDeniedDiagnostics.ProcessElevated);
        HpFanGetCountReportDecodeResult fanGetCountDecode = HpFanGetCountReportDecoder.TryDecode(
            fanGetCountInvocation.CommandName,
            fanGetCountInvocation.Success,
            fanGetCountInvocation.Invoked,
            fanGetCountInvocation.ReturnedBytes);
        var fanMaxGetInvocation = TryInvokeFanMaxGet(
            invocationClient,
            hpWmiSnapshot,
            HpBiosWmiCommandCatalog.Definitions,
            hpWmiReadOnlyTestMode,
            accessDeniedDiagnostics.ProcessElevated);
        HpFanMaxGetReportDecodeResult fanMaxGetDecode = HpFanMaxGetReportDecoder.TryDecode(
            fanMaxGetInvocation.CommandName,
            fanMaxGetInvocation.Success,
            fanMaxGetInvocation.Invoked,
            fanMaxGetInvocation.ReturnedBytes);
        var fanGetLevelInvocation = TryInvokeFanGetLevel(
            invocationClient,
            hpWmiSnapshot,
            HpBiosWmiCommandCatalog.Definitions,
            hpWmiReadOnlyTestMode,
            accessDeniedDiagnostics.ProcessElevated);
        HpFanGetLevelReportDecodeResult fanGetLevelDecode = HpFanGetLevelReportDecoder.TryDecode(
            fanGetLevelInvocation.CommandName,
            fanGetLevelInvocation.Success,
            fanGetLevelInvocation.Invoked,
            fanGetLevelInvocation.ReturnedBytes);
        HpFanMaxDryRunReport setFanMaxDryRun = HpFanMaxDryRunReport.CreateDefaultBlocked();

        return new HpVictusCapabilitySnapshot(
            manufacturer,
            model,
            systemFamily,
            systemSku,
            productVendor,
            productName,
            biosVersion,
            IsHpManufacturer(manufacturer, productVendor),
            IsVictusModel(model, productName, systemSku),
            hpWmiSnapshot.RootWmiAvailability,
            hpWmiSnapshot.HpqBIntMAvailability,
            hpWmiSnapshot.HpqBDataInAvailability,
            hpWmiSnapshot.HpqBIntMMethodNames,
            hpWmiSnapshot.HpqBDataInMethodNames,
            hpWmiSnapshot.Errors,
            invocationSandboxStatus.InvocationSandboxAvailable,
            invocationSandboxStatus.SafeReadOnlyCommandCount,
            invocationSandboxStatus.RejectedCommandCount,
            invocationSandboxStatus.Errors,
            systemDesignDataDryRun.Status,
            systemDesignDataDryRun.Success && !systemDesignDataDryRun.Invoked,
            systemDesignDataDryRun.Errors,
            systemDesignDataInvocationAllowed,
            systemDesignDataInvocation.Invoked,
            systemDesignDataInvocation.Success,
            systemDesignDataInvocation.ReturnedByteCount ?? 0,
            FirstNonEmpty(systemDesignDataInvocation.Errors),
            systemDesignDataDecode.Succeeded,
            systemDesignDataDecode.Errors,
            systemDesignDataDecode.Decoded,
            fanGetCountInvocationAllowed,
            fanGetCountInvocation.Invoked,
            fanGetCountInvocation.Success,
            fanGetCountInvocation.ReturnedByteCount ?? 0,
            FirstNonEmpty(fanGetCountInvocation.Errors),
            fanGetCountDecode.Succeeded,
            fanGetCountDecode.Errors,
            fanGetCountDecode.Decoded,
            fanMaxGetInvocationAllowed,
            fanMaxGetInvocation.Invoked,
            fanMaxGetInvocation.Success,
            fanMaxGetInvocation.ReturnedByteCount ?? 0,
            FirstNonEmpty(fanMaxGetInvocation.Errors),
            fanMaxGetDecode.Succeeded,
            fanMaxGetDecode.Errors,
            fanMaxGetDecode.Decoded,
            fanGetLevelInvocationAllowed,
            fanGetLevelInvocation.Invoked,
            fanGetLevelInvocation.Success,
            fanGetLevelInvocation.ReturnedByteCount ?? 0,
            FirstNonEmpty(fanGetLevelInvocation.Errors),
            fanGetLevelDecode.Succeeded,
            fanGetLevelDecode.Errors,
            fanGetLevelDecode.Decoded,
            setFanMaxDryRun,
            accessDeniedDiagnostics.ProcessElevated,
            accessDeniedDiagnostics.WindowsIdentitySummary,
            accessDeniedDiagnostics.WmiNamespaceReadable,
            accessDeniedDiagnostics.HpBIntMClassReadable,
            accessDeniedDiagnostics.HpBIntMMethodMetadataReadable,
            accessDeniedDiagnostics.HpRelatedServices,
            accessDeniedDiagnostics.AccessDeniedInvestigationErrors,
            hpWmiInvocationRequiresElevation,
            hpWmiInvocationBlockedReason,
            hpWmiRecommendedNextStep,
            errors.ToArray());
    }

    public static string WriteReport(HpVictusCapabilitySnapshot snapshot)
    {
        Directory.CreateDirectory(HpDiagnosticPaths.AppDataDirectory);
        File.WriteAllText(ReportPath, BuildReportJson(snapshot));
        return ReportPath;
    }

    public static string BuildReportJson(HpVictusCapabilitySnapshot snapshot)
    {
        return JsonSerializer.Serialize(CreateReport(snapshot), ReportJsonOptions);
    }

    private static HpVictusCapabilityReport CreateReport(HpVictusCapabilitySnapshot snapshot)
    {
        return new HpVictusCapabilityReport(
            CapabilityReportSchemaVersion,
            CapabilityReportGeneratedBy,
            CapabilityReportMode,
            CapabilityReportSource,
            DateTimeOffset.UtcNow,
            DateTimeOffset.Now,
            snapshot.Manufacturer,
            snapshot.Model,
            snapshot.SystemFamily,
            snapshot.SystemSku,
            snapshot.BiosVersion,
            snapshot.RootWmiAvailability.ToString(),
            snapshot.HpqBIntMAvailability.ToString(),
            snapshot.HpqBDataInAvailability.ToString(),
            snapshot.HpqBIntMMethodNames,
            snapshot.HpqBDataInMethodNames,
            snapshot.HpWmiReadOnlyClientErrors,
            snapshot.InvocationSandboxAvailable,
            snapshot.SafeReadOnlyCommandCount,
            snapshot.RejectedCommandCount,
            snapshot.InvocationSandboxErrors,
            snapshot.SystemDesignDataDryRunStatus,
            snapshot.SystemDesignDataDryRunReady,
            snapshot.SystemDesignDataDryRunErrors,
            snapshot.SystemDesignDataInvocationAllowed,
            snapshot.SystemDesignDataInvocationAttempted,
            snapshot.SystemDesignDataInvocationSucceeded,
            snapshot.SystemDesignDataReturnedByteCount,
            snapshot.SystemDesignDataInvocationError,
            snapshot.SystemDesignDataDecodeSucceeded,
            snapshot.SystemDesignDataDecodeErrors,
            snapshot.SystemDesignDataDecoded,
            snapshot.FanGetCountInvocationAllowed,
            snapshot.FanGetCountInvocationAttempted,
            snapshot.FanGetCountInvocationSucceeded,
            snapshot.FanGetCountReturnedByteCount,
            snapshot.FanGetCountInvocationError,
            snapshot.FanGetCountDecodeSucceeded,
            snapshot.FanGetCountDecodeErrors,
            snapshot.FanGetCountDecoded,
            snapshot.FanMaxGetInvocationAllowed,
            snapshot.FanMaxGetInvocationAttempted,
            snapshot.FanMaxGetInvocationSucceeded,
            snapshot.FanMaxGetReturnedByteCount,
            snapshot.FanMaxGetInvocationError,
            snapshot.FanMaxGetDecodeSucceeded,
            snapshot.FanMaxGetDecodeErrors,
            snapshot.FanMaxGetDecoded,
            snapshot.FanGetLevelInvocationAllowed,
            snapshot.FanGetLevelInvocationAttempted,
            snapshot.FanGetLevelInvocationSucceeded,
            snapshot.FanGetLevelReturnedByteCount,
            snapshot.FanGetLevelInvocationError,
            snapshot.FanGetLevelDecodeSucceeded,
            snapshot.FanGetLevelDecodeErrors,
            snapshot.FanGetLevelDecoded,
            snapshot.SetFanMaxDryRun.SetFanMaxWriteImplemented,
            snapshot.SetFanMaxDryRun.SetFanMaxWriteAllowed,
            snapshot.SetFanMaxDryRun.SetFanMaxFirstWriteGateStatus,
            snapshot.SetFanMaxDryRun.SetFanMaxFirstWriteGateSatisfied,
            snapshot.SetFanMaxDryRun.SetFanMaxFirstWriteGateReason,
            snapshot.SetFanMaxDryRun.SetFanMaxExperimentalPayloadCandidate,
            snapshot.SetFanMaxDryRun.SetFanMaxPhysicalResponseObserved,
            snapshot.SetFanMaxDryRun.SetFanMaxPhysicalResponseConfirmationCount,
            snapshot.SetFanMaxDryRun.SetFanMaxReadbackReliable,
            snapshot.SetFanMaxDryRun.SetFanMaxDeveloperExperimentAllowed,
            snapshot.SetFanMaxDryRun.SetFanMaxDeveloperExperimentPayload,
            snapshot.SetFanMaxDryRun.SetFanMaxNormalControlValidated,
            snapshot.SetFanMaxDryRun.SetFanMaxUserFacingControlAllowed,
            snapshot.SetFanMaxDryRun.SetFanMaxDryRunEvaluated,
            snapshot.SetFanMaxDryRun.SetFanMaxDeviceValidatedInputLength,
            snapshot.SetFanMaxDryRun.SetFanMaxDryRunBlockedReasons,
            snapshot.SetFanMaxDryRun.SetFanMaxNextRequiredProof,
            snapshot.ProcessElevated,
            snapshot.WindowsIdentitySummary,
            snapshot.WmiNamespaceReadable,
            snapshot.HpBIntMClassReadable,
            snapshot.HpBIntMMethodMetadataReadable,
            snapshot.HpRelatedServices,
            snapshot.AccessDeniedInvestigationErrors,
            snapshot.HpWmiInvocationRequiresElevation,
            snapshot.HpWmiInvocationBlockedReason,
            snapshot.HpWmiRecommendedNextStep,
            snapshot.IsHpManufacturer,
            snapshot.IsVictusModel,
            snapshot.Errors);
    }

    private static Dictionary<string, string> QueryFirst(string scopePath, string query, List<string> errors, string sourceName)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            using var searcher = new ManagementObjectSearcher(new ManagementScope(scopePath), new ObjectQuery(query));
            using var results = searcher.Get();

            foreach (ManagementObject result in results)
            {
                using (result)
                {
                    foreach (PropertyData property in result.Properties)
                    {
                        values[property.Name] = property.Value?.ToString()?.Trim() ?? string.Empty;
                    }
                }

                break;
            }
        }
        catch (Exception ex) when (ex is ManagementException or UnauthorizedAccessException or COMException or InvalidOperationException)
        {
            errors.Add($"{sourceName}: {ex.GetType().Name}: {ex.Message}");
        }

        return values;
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        foreach (string? value in values)
        {
            if (!string.IsNullOrWhiteSpace(value)) return value.Trim();
        }

        return string.Empty;
    }

    private static HpWmiInvocationResult TryInvokeSystemDesignData(
        HpWmiInvocationClient invocationClient,
        HpWmiReadOnlySnapshot hpWmiSnapshot,
        IEnumerable<HpBiosWmiCommandDefinition> definitions,
        bool hpWmiReadOnlyTestModeEnabled,
        bool processElevated)
    {
        HpBiosWmiCommandDefinition? definition = definitions.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, "SystemDesignData", StringComparison.OrdinalIgnoreCase));

        if (definition is null)
        {
            return HpWmiInvocationResult.Rejected("SystemDesignData", "command definition not found");
        }

        return invocationClient.TryInvoke(
            new HpWmiInvocationRequest(
                definition,
                global::AppConfig.IsHpVictusHardwareMode(),
                hpWmiReadOnlyTestModeEnabled,
                processElevated),
            hpWmiSnapshot);
    }

    private static HpWmiInvocationResult TryInvokeFanGetCount(
        HpWmiInvocationClient invocationClient,
        HpWmiReadOnlySnapshot hpWmiSnapshot,
        IEnumerable<HpBiosWmiCommandDefinition> definitions,
        bool hpWmiReadOnlyTestModeEnabled,
        bool processElevated)
    {
        HpBiosWmiCommandDefinition? definition = definitions.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, "FanGetCount", StringComparison.OrdinalIgnoreCase));

        if (definition is null)
        {
            return HpWmiInvocationResult.Rejected("FanGetCount", "command definition not found");
        }

        return invocationClient.TryInvoke(
            new HpWmiInvocationRequest(
                definition,
                global::AppConfig.IsHpVictusHardwareMode(),
                hpWmiReadOnlyTestModeEnabled,
                processElevated),
            hpWmiSnapshot);
    }

    private static HpWmiInvocationResult TryInvokeFanMaxGet(
        HpWmiInvocationClient invocationClient,
        HpWmiReadOnlySnapshot hpWmiSnapshot,
        IEnumerable<HpBiosWmiCommandDefinition> definitions,
        bool hpWmiReadOnlyTestModeEnabled,
        bool processElevated)
    {
        HpBiosWmiCommandDefinition? definition = definitions.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, "FanMaxGet", StringComparison.OrdinalIgnoreCase));

        if (definition is null)
        {
            return HpWmiInvocationResult.Rejected("FanMaxGet", "command definition not found");
        }

        return invocationClient.TryInvoke(
            new HpWmiInvocationRequest(
                definition,
                global::AppConfig.IsHpVictusHardwareMode(),
                hpWmiReadOnlyTestModeEnabled,
                processElevated),
            hpWmiSnapshot);
    }

    private static HpWmiInvocationResult TryInvokeFanGetLevel(
        HpWmiInvocationClient invocationClient,
        HpWmiReadOnlySnapshot hpWmiSnapshot,
        IEnumerable<HpBiosWmiCommandDefinition> definitions,
        bool hpWmiReadOnlyTestModeEnabled,
        bool processElevated)
    {
        HpBiosWmiCommandDefinition? definition = definitions.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, "FanGetLevel", StringComparison.OrdinalIgnoreCase));

        if (definition is null)
        {
            return HpWmiInvocationResult.Rejected("FanGetLevel", "command definition not found");
        }

        return invocationClient.TryInvoke(
            new HpWmiInvocationRequest(
                definition,
                global::AppConfig.IsHpVictusHardwareMode(),
                hpWmiReadOnlyTestModeEnabled,
                processElevated),
            hpWmiSnapshot);
    }

    private static string GetHpWmiInvocationBlockedReason(
        bool hpVictusMode,
        bool hpWmiReadOnlyTestMode,
        bool processElevated)
    {
        if (!hpVictusMode)
        {
            return "--hp-victus mode is required for HP BIOS WMI invocation";
        }

        if (!hpWmiReadOnlyTestMode)
        {
            return "Real HP WMI invocation skipped: missing explicit --hp-wmi-readonly-test flag";
        }

        if (!processElevated)
        {
            return "Real HP WMI invocation skipped: process is not elevated";
        }

        return "Real HP WMI invocation is allowed only for commands marked SafeReadOnlyInvocation";
    }

    private static string GetHpWmiRecommendedNextStep(
        bool hpVictusMode,
        bool hpWmiReadOnlyTestMode,
        bool processElevated)
    {
        if (!hpVictusMode)
        {
            return "Use --hp-victus for safe HP identity and WMI availability probing.";
        }

        if (!hpWmiReadOnlyTestMode)
        {
            return "Continue using --hp-victus for safe non-invoking probes; use --hp-wmi-readonly-test only for controlled developer testing.";
        }

        if (!processElevated)
        {
            return "If explicitly approved, rerun the controlled read-only test from an elevated Administrator terminal.";
        }

        return "Proceed only with approved read-only invocation tests for SystemDesignData, FanGetCount, FanMaxGet, and FanGetLevel; keep all hardware writes forbidden.";
    }

    private static bool IsHpManufacturer(string manufacturer, string productVendor) =>
        ContainsAny(manufacturer, "HP", "Hewlett-Packard") || ContainsAny(productVendor, "HP", "Hewlett-Packard");

    private static bool IsVictusModel(params string[] values) =>
        values.Any(value => ContainsAny(value, "Victus"));

    private static bool ContainsAny(string value, params string[] candidates) =>
        !string.IsNullOrWhiteSpace(value) && candidates.Any(candidate => value.Contains(candidate, StringComparison.OrdinalIgnoreCase));

    private sealed record HpVictusCapabilityReport(
        int ReportSchemaVersion,
        string ReportGeneratedBy,
        string ReportMode,
        string ReportSource,
        DateTimeOffset ReportGeneratedAtUtc,
        DateTimeOffset Timestamp,
        string Manufacturer,
        string Model,
        string SystemFamily,
        string Sku,
        string BiosVersion,
        string RootWmiAvailability,
        string HpqBIntMAvailability,
        string HpqBDataInAvailability,
        string[] HpqBIntMMethodNames,
        string[] HpqBDataInMethodNames,
        string[] HpWmiReadOnlyClientErrors,
        bool InvocationSandboxAvailable,
        int SafeReadOnlyCommandCount,
        int RejectedCommandCount,
        string[] InvocationSandboxErrors,
        string SystemDesignDataDryRunStatus,
        bool SystemDesignDataDryRunReady,
        string[] SystemDesignDataDryRunErrors,
        bool SystemDesignDataInvocationAllowed,
        bool SystemDesignDataInvocationAttempted,
        bool SystemDesignDataInvocationSucceeded,
        int SystemDesignDataReturnedByteCount,
        string SystemDesignDataInvocationError,
        bool SystemDesignDataDecodeSucceeded,
        string[] SystemDesignDataDecodeErrors,
        HpSystemDesignDataSnapshot? SystemDesignDataDecoded,
        bool FanGetCountInvocationAllowed,
        bool FanGetCountInvocationAttempted,
        bool FanGetCountInvocationSucceeded,
        int FanGetCountReturnedByteCount,
        string FanGetCountInvocationError,
        bool FanGetCountDecodeSucceeded,
        string[] FanGetCountDecodeErrors,
        HpFanGetCountSnapshot? FanGetCountDecoded,
        bool FanMaxGetInvocationAllowed,
        bool FanMaxGetInvocationAttempted,
        bool FanMaxGetInvocationSucceeded,
        int FanMaxGetReturnedByteCount,
        string FanMaxGetInvocationError,
        bool FanMaxGetDecodeSucceeded,
        string[] FanMaxGetDecodeErrors,
        HpFanMaxGetSnapshot? FanMaxGetDecoded,
        bool FanGetLevelInvocationAllowed,
        bool FanGetLevelInvocationAttempted,
        bool FanGetLevelInvocationSucceeded,
        int FanGetLevelReturnedByteCount,
        string FanGetLevelInvocationError,
        bool FanGetLevelDecodeSucceeded,
        string[] FanGetLevelDecodeErrors,
        HpFanGetLevelSnapshot? FanGetLevelDecoded,
        bool SetFanMaxWriteImplemented,
        bool SetFanMaxWriteAllowed,
        string SetFanMaxFirstWriteGateStatus,
        bool SetFanMaxFirstWriteGateSatisfied,
        string SetFanMaxFirstWriteGateReason,
        string SetFanMaxExperimentalPayloadCandidate,
        bool SetFanMaxPhysicalResponseObserved,
        int SetFanMaxPhysicalResponseConfirmationCount,
        bool SetFanMaxReadbackReliable,
        bool SetFanMaxDeveloperExperimentAllowed,
        string SetFanMaxDeveloperExperimentPayload,
        bool SetFanMaxNormalControlValidated,
        bool SetFanMaxUserFacingControlAllowed,
        bool SetFanMaxDryRunEvaluated,
        int? SetFanMaxDeviceValidatedInputLength,
        string[] SetFanMaxDryRunBlockedReasons,
        string SetFanMaxNextRequiredProof,
        bool ProcessElevated,
        string WindowsIdentitySummary,
        bool WmiNamespaceReadable,
        bool HpBIntMClassReadable,
        bool HpBIntMMethodMetadataReadable,
        HpRelatedServiceSnapshot[] HpRelatedServices,
        string[] AccessDeniedInvestigationErrors,
        bool HpWmiInvocationRequiresElevation,
        string HpWmiInvocationBlockedReason,
        string HpWmiRecommendedNextStep,
        bool LooksLikeHp,
        bool LooksLikeVictus,
        string[] ProbeErrors);
}
