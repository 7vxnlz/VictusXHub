using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpDiagnosticDashboardFormatterTests
{
    [Fact]
    public void UserSummary_PrioritizesNormalStatusAndKeepsDeveloperEvidenceOut()
    {
        IReadOnlyList<HpDiagnosticDashboardSection> summary = HpDiagnosticDashboardFormatter.BuildUserSummary(new()
        {
            Model = "HP Victus 16-s0035nt",
            Sku = "7Z5Z2EA#AB8",
            BiosVersion = "F.31",
            HpVictusDetection = "Detected",
            FanCount = "2 fans",
            ThermalPolicy = "V1",
            CpuLoad = "24%",
            GpuTemperature = "52 C",
            BatteryPower = "78% | AC | Charging",
            RefreshRate = "144Hz",
            CpuTemperature = "Unavailable",
            FanRpm = "Unavailable",
            GpuSwitchingCapability = "Supported",
            KeyboardBacklightCapability = "Supported, state unavailable",
            BatteryCareCapability = "Unavailable",
            FanControlStatus = "NO-GO"
        });

        Assert.Equal(["Device", "Live status", "Capabilities"], summary.Select(section => section.Title));
        HpDiagnosticDashboardSection live = Assert.Single(summary, section => section.Title == "Live status");
        HpDiagnosticDashboardSection device = Assert.Single(summary, section => section.Title == "Device");
        HpDiagnosticDashboardSection capabilities = Assert.Single(summary, section => section.Title == "Capabilities");
        Assert.Contains(device.Rows, row => row.Label == "SKU" && row.Value == "7Z5Z2EA#AB8");
        Assert.Contains(device.Rows, row => row.Label == "Fan count" && row.Value == "2 fans");
        Assert.Contains(device.Rows, row => row.Label == "Thermal policy" && row.Value == "V1");
        Assert.Contains(live.Rows, row => row.Label == "CPU Load" && row.Value == "24%" && row.Status == HpDiagnosticDashboardStatus.Ready);
        Assert.Contains(live.Rows, row => row.Label == "Refresh Rate" && row.Value == "144Hz" && row.Status == HpDiagnosticDashboardStatus.Ready);
        Assert.Contains(live.Rows, row => row.Label == "CPU Temp" && row.Value == "Unavailable" && row.Status == HpDiagnosticDashboardStatus.Normal);
        Assert.Contains(live.Rows, row => row.Label == "Fan RPM" && row.Value == "Unavailable" && row.Status == HpDiagnosticDashboardStatus.Normal);
        Assert.Contains(capabilities.Rows, row => row.Label == "Fan Control" && row.Value == "NO-GO" && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(capabilities.Rows, row => row.Label == "GPU Switching" && row.Value == "Supported" && row.Status == HpDiagnosticDashboardStatus.Ready);
        Assert.DoesNotContain(summary.SelectMany(section => section.Rows), row => row.Label.Contains("Payload", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(summary.SelectMany(section => section.Rows), row => row.Label.Contains("DeviceValidatedInputLength", StringComparison.Ordinal));
    }

    [Fact]
    public void UserSummary_MissingHardwareFactsRemainUnavailable()
    {
        IReadOnlyList<HpDiagnosticDashboardSection> summary = HpDiagnosticDashboardFormatter.BuildUserSummary(new());
        HpDiagnosticDashboardSection device = Assert.Single(summary, section => section.Title == "Device");
        HpDiagnosticDashboardSection capabilities = Assert.Single(summary, section => section.Title == "Capabilities");

        Assert.Contains(device.Rows, row => row.Label == "Fan count" && row.Value == "Unavailable");
        Assert.Contains(device.Rows, row => row.Label == "Thermal policy" && row.Value == "Unavailable");
        Assert.Contains(capabilities.Rows, row => row.Label == "GPU Switching" && row.Value == "Unavailable");
    }

    [Fact]
    public void MissingInput_UsesNotAvailableFallbacks()
    {
        IReadOnlyList<HpDiagnosticDashboardSection> sections = HpDiagnosticDashboardFormatter.BuildSections(new());

        HpDiagnosticDashboardSection device = Assert.Single(sections, section => section.Title == "Device");
        Assert.Contains(device.Rows, row => row.Label == "Manufacturer" && row.Value == HpDiagnosticDashboardFormatter.NotAvailable);
        Assert.Contains(device.Rows, row => row.Label == "HP/Victus detected" && row.Value == HpDiagnosticDashboardFormatter.NotAvailable);
    }

    [Fact]
    public void ReportMetadata_FormatsSchemaAndTimestamp()
    {
        IReadOnlyList<HpDiagnosticDashboardSection> sections = HpDiagnosticDashboardFormatter.BuildSections(new()
        {
            ReportSchemaVersion = "1",
            ReportGeneratedBy = "VictusX",
            ReportMode = "HP read-only diagnostic",
            ReportSource = "Startup capability snapshot",
            ReportGeneratedAt = "2026-09-01T12:34:56+03:00"
        });

        HpDiagnosticDashboardSection metadata = Assert.Single(sections, section => section.Title == "Report metadata");
        Assert.Contains(metadata.Rows, row => row.Label == "Schema version" && row.Value == "v1");
        Assert.Contains(metadata.Rows, row => row.Label == "Last generated" && row.Value == "2026-09-01 12:34:56 +03:00");
    }

    [Fact]
    public void OlderReportWithoutMetadata_UsesLegacyFallbackAndGuidance()
    {
        IReadOnlyList<HpDiagnosticDashboardSection> sections = HpDiagnosticDashboardFormatter.BuildSections(new());

        HpDiagnosticDashboardSection metadata = Assert.Single(sections, section => section.Title == "Report metadata");
        HpDiagnosticDashboardSection guidance = Assert.Single(sections, section => section.Title == "Missing-data guidance");
        Assert.Contains(metadata.Rows, row => row.Label == "Schema version" && row.Value == HpDiagnosticDashboardFormatter.LegacyReportSchema);
        Assert.Contains(guidance.Rows, row => row.Label == "Explicit probe data" && row.Value == HpDiagnosticStatusText.NormalHpModeDoesNotRunExplicitProbes);
        Assert.Contains(guidance.Rows, row => row.Label == "Developer-only tests" && row.Value == HpDiagnosticStatusText.ExplicitTestsAreDeveloperOnly);
    }

    [Fact]
    public void FanSection_KeepsFanGetLevelValuesRawOnly()
    {
        IReadOnlyList<HpDiagnosticDashboardSection> sections = HpDiagnosticDashboardFormatter.BuildSections(new()
        {
            Fan1RawLevel = "23",
            Fan2RawLevel = "0"
        });

        HpDiagnosticDashboardSection fan = Assert.Single(sections, section => section.Title == "Fan read-only status");
        Assert.Contains(fan.Rows, row => row.Label == "Fan 1 raw level byte" && row.Value == "23");
        Assert.Contains(fan.Rows, row => row.Label == "Fan 2 raw level byte" && row.Value == "0");
        Assert.Contains(fan.Rows, row => row.Label == "Raw level data" && row.Value == HpDiagnosticDashboardFormatter.RawFanLevelWarning);
    }

    [Fact]
    public void SafetySection_KeepsNoGoAndNoControlWording()
    {
        IReadOnlyList<HpDiagnosticDashboardSection> sections = HpDiagnosticDashboardFormatter.BuildSections(new()
        {
            SetFanMaxWriteAllowed = "Blocked"
        });

        HpDiagnosticDashboardSection safety = Assert.Single(sections, section => section.Title == "Safety / NO-GO status");
        Assert.Contains(safety.Rows, row => row.Label == "Fan control" && row.Value == HpDiagnosticDashboardFormatter.FanControlStatus);
        Assert.Contains(safety.Rows, row => row.Label == "SetFanMax" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxStatus);
        Assert.Contains(safety.Rows, row => row.Label == "SetFanMax write allowed" && row.Value == "Blocked" && row.Status == HpDiagnosticDashboardStatus.Blocked);
    }

    [Fact]
    public void SetFanMaxEvidenceReadiness_DefaultsFailClosed()
    {
        IReadOnlyList<HpDiagnosticDashboardSection> sections = HpDiagnosticDashboardFormatter.BuildSections(new());

        HpDiagnosticDashboardSection readiness = Assert.Single(sections, section => section.Title == "SetFanMax evidence readiness");
        Assert.Contains(readiness.Rows, row => row.Label == "Current status" && row.Value == "NO-GO" && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(readiness.Rows, row => row.Label == "First-write gate status" && row.Value == "NO-GO" && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(readiness.Rows, row => row.Label == "First-write gate satisfied" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxFirstWriteGateNotSatisfied && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(readiness.Rows, row => row.Label == "First-write gate reason" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxFirstWriteGateMissingOldReportReason && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(readiness.Rows, row => row.Label == "Experimental payload candidate" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxExperimentalPayloadUnknown && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(readiness.Rows, row => row.Label == "Physical response observed" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxPhysicalResponseUnknown && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(readiness.Rows, row => row.Label == "Normal control validated" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxNormalControlNotValidated && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(readiness.Rows, row => row.Label == "User-facing control allowed" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxUserFacingControlNotAllowed && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(readiness.Rows, row => row.Label == "Fan write implemented" && row.Value == "False - not implemented");
        Assert.Contains(readiness.Rows, row => row.Label == "Fan write allowed" && row.Value == "False - blocked" && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(readiness.Rows, row => row.Label == "DeviceValidatedInputLength" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxInputLengthUnset && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(readiness.Rows, row => row.Label == "Payload length decision" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxPayloadLengthNotSelected && row.Status == HpDiagnosticDashboardStatus.Blocked);
        string[] missingEvidenceLabels =
        [
            "Exact device payload length",
            "Restore/disable behavior proof",
            "Thermal observation proof",
            "AC/battery/power-state proof",
            "Failure/recovery proof",
            "Human approval checkpoint"
        ];
        Assert.All(
            missingEvidenceLabels,
            label => Assert.Contains(readiness.Rows, row => row.Label == label && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxEvidenceMissing && row.Status == HpDiagnosticDashboardStatus.Blocked));
    }

    [Theory]
    [InlineData("1", "1 byte reported; not approved")]
    [InlineData("4", "4 bytes reported; not approved")]
    [InlineData("2", HpDiagnosticDashboardFormatter.SetFanMaxInputLengthUnset)]
    [InlineData(null, HpDiagnosticDashboardFormatter.SetFanMaxInputLengthUnset)]
    public void SetFanMaxEvidenceReadiness_NeverTreatsReportedLengthAsApproval(string? value, string expected)
    {
        Assert.Equal(expected, HpDiagnosticDashboardFormatter.FormatDeviceValidatedInputLength(value));

        HpDiagnosticDashboardSection readiness = Assert.Single(
            HpDiagnosticDashboardFormatter.BuildSections(new() { SetFanMaxDeviceValidatedInputLength = value }),
            section => section.Title == "SetFanMax evidence readiness");
        Assert.Contains(readiness.Rows, row => row.Label == "Payload length decision" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxPayloadLengthNotSelected);
    }

    [Fact]
    public void SetFanMaxFirstWriteGate_UnexpectedCachedGoValuesRemainBlocked()
    {
        HpDiagnosticDashboardSection readiness = Assert.Single(
            HpDiagnosticDashboardFormatter.BuildSections(new()
            {
                SetFanMaxFirstWriteGateStatus = "GO",
                SetFanMaxFirstWriteGateSatisfied = "True",
                SetFanMaxFirstWriteGateReason = "Approved"
            }),
            section => section.Title == "SetFanMax evidence readiness");

        Assert.Contains(readiness.Rows, row => row.Label == "First-write gate status" && row.Value == "Blocked - unexpected GO state" && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(readiness.Rows, row => row.Label == "First-write gate satisfied" && row.Value == "Blocked - unexpected satisfied state" && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(readiness.Rows, row => row.Label == "First-write gate reason" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxFirstWriteGateUnexpectedReportReason && row.Status == HpDiagnosticDashboardStatus.Blocked);
    }

    [Fact]
    public void SetFanMaxFirstWriteGate_OldReportMissingGateFieldsShowsOldReportReason()
    {
        HpDiagnosticDashboardSection readiness = Assert.Single(
            HpDiagnosticDashboardFormatter.BuildSections(new()
            {
                SetFanMaxWriteImplemented = "Not implemented",
                SetFanMaxWriteAllowed = "Blocked"
            }),
            section => section.Title == "SetFanMax evidence readiness");

        Assert.Contains(readiness.Rows, row => row.Label == "First-write gate status" && row.Value == "NO-GO" && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(readiness.Rows, row => row.Label == "First-write gate satisfied" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxFirstWriteGateNotSatisfied && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(readiness.Rows, row => row.Label == "First-write gate reason" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxFirstWriteGateMissingOldReportReason && row.Status == HpDiagnosticDashboardStatus.Blocked);
    }

    [Fact]
    public void SetFanMaxFirstWriteGate_PartialGateFieldsStillShowOldReportReason()
    {
        HpDiagnosticDashboardSection readiness = Assert.Single(
            HpDiagnosticDashboardFormatter.BuildSections(new()
            {
                SetFanMaxFirstWriteGateStatus = "NO-GO",
                SetFanMaxFirstWriteGateReason = HpFanMaxDryRunReport.FirstWriteGateReason
            }),
            section => section.Title == "SetFanMax evidence readiness");

        Assert.Contains(readiness.Rows, row => row.Label == "First-write gate reason" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxFirstWriteGateMissingOldReportReason && row.Status == HpDiagnosticDashboardStatus.Blocked);
    }

    [Fact]
    public void SetFanMaxFirstWriteGate_CurrentNoGoReportKeepsExactReason()
    {
        HpDiagnosticDashboardSection readiness = Assert.Single(
            HpDiagnosticDashboardFormatter.BuildSections(new()
            {
                SetFanMaxFirstWriteGateStatus = "NO-GO",
                SetFanMaxFirstWriteGateSatisfied = "False",
                SetFanMaxFirstWriteGateReason = HpFanMaxDryRunReport.FirstWriteGateReason
            }),
            section => section.Title == "SetFanMax evidence readiness");

        Assert.Contains(readiness.Rows, row => row.Label == "First-write gate reason" && row.Value == HpFanMaxDryRunReport.FirstWriteGateReason && row.Status == HpDiagnosticDashboardStatus.Blocked);
    }

    [Fact]
    public void SetFanMaxExperimentalEvidence_TwoFourByteResponsesRemainExperimentalAndControlBlocked()
    {
        HpDiagnosticDashboardSection readiness = Assert.Single(
            HpDiagnosticDashboardFormatter.BuildSections(new()
            {
                SetFanMaxExperimentalPayloadCandidate = "FourByte",
                SetFanMaxPhysicalResponseObserved = "True",
                SetFanMaxPhysicalResponseConfirmationCount = "2",
                SetFanMaxReadbackReliable = "False",
                SetFanMaxDeveloperExperimentAllowed = "True",
                SetFanMaxDeveloperExperimentPayload = "FourByte",
                SetFanMaxNormalControlValidated = "False",
                SetFanMaxUserFacingControlAllowed = "False"
            }),
            section => section.Title == "SetFanMax evidence readiness");

        Assert.Contains(readiness.Rows, row => row.Label == "Experimental payload candidate" && row.Value == "FourByte - experimental only" && row.Status == HpDiagnosticDashboardStatus.Warning);
        Assert.Contains(readiness.Rows, row => row.Label == "Physical response observed" && row.Value == "True - observed in two manual four-byte experiments; experimental only" && row.Status == HpDiagnosticDashboardStatus.Warning);
        Assert.Contains(readiness.Rows, row => row.Label == "Physical response confirmation count" && row.Value == "2 - manual four-byte confirmation records");
        Assert.Contains(readiness.Rows, row => row.Label == "Readback reliable" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxReadbackNotReliable && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(readiness.Rows, row => row.Label == "Developer-only experiment allowed" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxDeveloperExperimentAllowed && row.Status == HpDiagnosticDashboardStatus.Warning);
        Assert.Contains(readiness.Rows, row => row.Label == "Developer-only experiment payload" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxDeveloperExperimentPayload && row.Status == HpDiagnosticDashboardStatus.Warning);
        Assert.Contains(readiness.Rows, row => row.Label == "Normal control validated" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxNormalControlNotValidated && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(readiness.Rows, row => row.Label == "User-facing control allowed" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxUserFacingControlNotAllowed && row.Status == HpDiagnosticDashboardStatus.Blocked);
    }

    [Fact]
    public void Summary_StatesTheLimitedFourByteDeveloperExperimentAllowanceAndNormalControlNoGo()
    {
        string summary = HpDiagnosticDashboardFormatter.BuildSummary(new()
        {
            SetFanMaxDeveloperExperimentAllowed = "True",
            SetFanMaxDeveloperExperimentPayload = "FourByte",
            SetFanMaxReadbackReliable = "False",
            SetFanMaxNormalControlValidated = "False",
            SetFanMaxUserFacingControlAllowed = "False"
        });

        Assert.Contains("Developer-only experiment allowed: " + HpDiagnosticDashboardFormatter.SetFanMaxDeveloperExperimentAllowed, summary, StringComparison.Ordinal);
        Assert.Contains("Developer-only experiment payload: " + HpDiagnosticDashboardFormatter.SetFanMaxDeveloperExperimentPayload, summary, StringComparison.Ordinal);
        Assert.Contains("Readback reliable: " + HpDiagnosticDashboardFormatter.SetFanMaxReadbackNotReliable, summary, StringComparison.Ordinal);
        Assert.Contains("Normal control validated: " + HpDiagnosticDashboardFormatter.SetFanMaxNormalControlNotValidated, summary, StringComparison.Ordinal);
        Assert.Contains("User-facing control allowed: " + HpDiagnosticDashboardFormatter.SetFanMaxUserFacingControlNotAllowed, summary, StringComparison.Ordinal);
    }

    [Fact]
    public void SetFanMaxExperimentalEvidence_UnexpectedCachedOptimismFailsClosed()
    {
        HpDiagnosticDashboardSection readiness = Assert.Single(
            HpDiagnosticDashboardFormatter.BuildSections(new()
            {
                SetFanMaxExperimentalPayloadCandidate = "OneByte",
                SetFanMaxPhysicalResponseObserved = "True",
                SetFanMaxPhysicalResponseConfirmationCount = "99",
                SetFanMaxReadbackReliable = "True",
                SetFanMaxDeveloperExperimentAllowed = "True",
                SetFanMaxDeveloperExperimentPayload = "OneByte",
                SetFanMaxNormalControlValidated = "True",
                SetFanMaxUserFacingControlAllowed = "True"
            }),
            section => section.Title == "SetFanMax evidence readiness");

        Assert.Contains(readiness.Rows, row => row.Label == "Experimental payload candidate" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxExperimentalPayloadUnknown && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(readiness.Rows, row => row.Label == "Physical response observed" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxPhysicalResponseUnknown && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(readiness.Rows, row => row.Label == "Readback reliable" && row.Value.Contains("failing closed", StringComparison.Ordinal) && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(readiness.Rows, row => row.Label == "Developer-only experiment allowed" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxDeveloperExperimentNotAllowed && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(readiness.Rows, row => row.Label == "Developer-only experiment payload" && row.Value == HpDiagnosticDashboardFormatter.SetFanMaxDeveloperExperimentPayloadUnknown && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(readiness.Rows, row => row.Label == "Normal control validated" && row.Value.Contains("failing closed", StringComparison.Ordinal) && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(readiness.Rows, row => row.Label == "User-facing control allowed" && row.Value.Contains("failing closed", StringComparison.Ordinal) && row.Status == HpDiagnosticDashboardStatus.Blocked);
    }

    [Fact]
    public void HealthSummary_AllCachedDataAvailable_IsReadyExceptFanControlNoGo()
    {
        HpDiagnosticDashboardHealthSummary summary = HpDiagnosticDashboardFormatter.BuildHealthSummary(new()
        {
            IsHpVictusDetected = true,
            RootWmiReadiness = "Ready",
            HpqBIntMReadiness = "Ready",
            HpqBDataInReadiness = "Ready",
            SystemDesignDataDecodeStatus = "Succeeded",
            SoftwareFanControlSupport = "Declared",
            FanCount = "2",
            MaxFanState = "Disabled",
            Fan1RawLevel = "23",
            Fan2RawLevel = "0"
        });

        Assert.Equal("Ready", summary.DeviceStatus);
        Assert.Equal("Ready", summary.WmiStatus);
        Assert.Equal("Ready", summary.ReadOnlyTelemetryStatus);
        Assert.Equal("Ready", summary.FanReadOnlyStatus);
        Assert.Equal(HpDiagnosticDashboardFormatter.SetFanMaxStatus, summary.FanControlStatus);
    }

    [Fact]
    public void HealthSummary_MissingReport_IsNotAvailableAndNoGo()
    {
        HpDiagnosticDashboardHealthSummary summary = HpDiagnosticDashboardFormatter.BuildHealthSummary(new());

        Assert.Equal(HpDiagnosticDashboardFormatter.NotAvailable, summary.DeviceStatus);
        Assert.Equal(HpDiagnosticDashboardFormatter.NotAvailable, summary.WmiStatus);
        Assert.Equal(HpDiagnosticDashboardFormatter.NotAvailable, summary.ReadOnlyTelemetryStatus);
        Assert.Equal(HpDiagnosticDashboardFormatter.NotAvailable, summary.FanReadOnlyStatus);
        Assert.Equal(HpDiagnosticDashboardFormatter.SetFanMaxStatus, summary.FanControlStatus);
    }

    [Fact]
    public void HealthSummary_WmiUnavailable_IsNotAvailable()
    {
        HpDiagnosticDashboardHealthSummary summary = HpDiagnosticDashboardFormatter.BuildHealthSummary(new()
        {
            IsHpVictusDetected = true,
            RootWmiReadiness = HpDiagnosticDashboardFormatter.NotAvailable,
            HpqBIntMReadiness = HpDiagnosticDashboardFormatter.NotAvailable,
            HpqBDataInReadiness = HpDiagnosticDashboardFormatter.NotAvailable
        });

        Assert.Equal(HpDiagnosticDashboardFormatter.NotAvailable, summary.WmiStatus);
    }

    [Fact]
    public void HealthSummary_MissingFanReadOnlyData_IsNotAvailable()
    {
        HpDiagnosticDashboardHealthSummary summary = HpDiagnosticDashboardFormatter.BuildHealthSummary(new()
        {
            IsHpVictusDetected = true,
            SystemDesignDataDecodeStatus = "Succeeded",
            SoftwareFanControlSupport = "Declared"
        });

        Assert.Equal("Ready", summary.ReadOnlyTelemetryStatus);
        Assert.Equal(HpDiagnosticDashboardFormatter.NotAvailable, summary.FanReadOnlyStatus);
    }

    [Fact]
    public void Summary_IncludesMissingExplicitProbeGuidanceAndNoControlWording()
    {
        string summary = HpDiagnosticDashboardFormatter.BuildSummary(new());

        Assert.Contains("Schema version: " + HpDiagnosticDashboardFormatter.LegacyReportSchema, summary, StringComparison.Ordinal);
        Assert.Contains(HpDiagnosticStatusText.NormalHpModeDoesNotRunExplicitProbes, summary, StringComparison.Ordinal);
        Assert.Contains(HpDiagnosticStatusText.FanControlNotImplemented, summary, StringComparison.Ordinal);
        Assert.Contains(HpDiagnosticStatusText.SetFanMaxNoGo, summary, StringComparison.Ordinal);
        Assert.Contains("Current status: NO-GO", summary, StringComparison.Ordinal);
        Assert.Contains("First-write gate reason: " + HpDiagnosticDashboardFormatter.SetFanMaxFirstWriteGateMissingOldReportReason, summary, StringComparison.Ordinal);
        Assert.Contains("Physical response observed: " + HpDiagnosticDashboardFormatter.SetFanMaxPhysicalResponseUnknown, summary, StringComparison.Ordinal);
        Assert.Contains("Normal control validated: " + HpDiagnosticDashboardFormatter.SetFanMaxNormalControlNotValidated, summary, StringComparison.Ordinal);
        Assert.Contains("DeviceValidatedInputLength: " + HpDiagnosticDashboardFormatter.SetFanMaxInputLengthUnset, summary, StringComparison.Ordinal);
        Assert.Contains("Failure/recovery proof: " + HpDiagnosticDashboardFormatter.SetFanMaxEvidenceMissing, summary, StringComparison.Ordinal);
    }

    [Fact]
    public void ProofGapAnalysis_ShowsReadOnlyFailClosedDecisions()
    {
        HpDiagnosticDashboardSection proofGaps = Assert.Single(
            HpDiagnosticDashboardFormatter.BuildSections(new()
            {
                FanProofGapEvidenceSources = "Local experiment logs: 1 valid, 0 invalid ignored; cached capability report loaded locally. No WMI or hardware action was performed.",
                FanProofGapDeveloperPulseDecision = HpFanProofGapAnalyzer.DeveloperPulseOperational,
                FanProofGapDeviceValidatedInputLengthDecision = HpFanProofGapAnalyzer.DeviceValidatedInputLengthUnset,
                FanProofGapFanMaxGetDecision = HpFanProofGapAnalyzer.FanMaxGetInconclusive,
                FanProofGapFanGetLevelDecision = HpFanProofGapAnalyzer.FanGetLevelRawOnly,
                FanProofGapRestoreVerificationDecision = HpFanProofGapAnalyzer.RestoreVerificationPartial,
                FanProofGapRepeatabilityDecision = HpFanProofGapAnalyzer.RepeatabilityPartial,
                FanProofGapThermalPowerSafetyDecision = HpFanProofGapAnalyzer.ThermalPowerSafetyMissing,
                FanProofGapNormalFanControlDecision = HpFanProofGapAnalyzer.NormalFanControlNoGo
            }),
            section => section.Title == "HP fan proof-gap analysis");

        Assert.Contains(proofGaps.Rows, row => row.Label == "DeviceValidatedInputLength" && row.Value == HpFanProofGapAnalyzer.DeviceValidatedInputLengthUnset && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(proofGaps.Rows, row => row.Label == "FanMaxGet reliability" && row.Value == HpFanProofGapAnalyzer.FanMaxGetInconclusive && row.Status == HpDiagnosticDashboardStatus.Blocked);
        Assert.Contains(proofGaps.Rows, row => row.Label == "FanGetLevel interpretation" && row.Value == HpFanProofGapAnalyzer.FanGetLevelRawOnly);
        Assert.Contains(proofGaps.Rows, row => row.Label == "Normal fan-control readiness" && row.Value == HpFanProofGapAnalyzer.NormalFanControlNoGo && row.Status == HpDiagnosticDashboardStatus.Blocked);
    }

    [Fact]
    public void Summary_BlocksOptimisticCachedFirstWriteGateValues()
    {
        string summary = HpDiagnosticDashboardFormatter.BuildSummary(new()
        {
            SetFanMaxFirstWriteGateStatus = "GO",
            SetFanMaxFirstWriteGateSatisfied = "True",
            SetFanMaxFirstWriteGateReason = "Approved"
        });

        Assert.Contains("First-write gate status: Blocked - unexpected GO state", summary, StringComparison.Ordinal);
        Assert.Contains("First-write gate satisfied: Blocked - unexpected satisfied state", summary, StringComparison.Ordinal);
        Assert.Contains("First-write gate reason: " + HpDiagnosticDashboardFormatter.SetFanMaxFirstWriteGateUnexpectedReportReason, summary, StringComparison.Ordinal);
    }

    [Fact]
    public void HealthSummary_KeepsSetFanMaxNoGoWording()
    {
        string text = HpDiagnosticDashboardFormatter.FormatHealthSummary(
            HpDiagnosticDashboardFormatter.BuildHealthSummary(new()));

        Assert.Contains("Fan control " + HpDiagnosticDashboardFormatter.SetFanMaxStatus, text, StringComparison.Ordinal);
    }

    [Fact]
    public void Formatter_HasNoWmiDependencyOrInvocationSurface()
    {
        Type[] types = [typeof(HpDiagnosticDashboardFormatter), typeof(HpDiagnosticDashboardInput)];

        Assert.DoesNotContain(
            types.SelectMany(type => type.GetMethods()),
            method => method.Name.Contains("Invoke", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            types.SelectMany(type => type.GetMethods()).SelectMany(method => method.GetParameters()),
            parameter => parameter.ParameterType.Namespace?.Contains("Management", StringComparison.Ordinal) == true);
    }
}
