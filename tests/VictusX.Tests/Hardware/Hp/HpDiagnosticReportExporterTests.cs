using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpDiagnosticReportExporterTests
{
    [Fact]
    public void Export_WritesTimestampedMarkdownWithReadOnlyDisclaimer()
    {
        string outputDirectory = Path.Combine(Path.GetTempPath(), "VictusX.Tests", Guid.NewGuid().ToString("N"));
        DateTimeOffset timestamp = new(2026, 8, 31, 12, 34, 56, TimeSpan.Zero);

        string filePath = HpDiagnosticReportExporter.Export(
            "Model: Victus\nFan count: 2\nRaw values are not RPM or percent.\nSetFanMax is NO-GO / design-only.",
            timestamp,
            outputDirectory);

        string content = File.ReadAllText(filePath);

        Assert.Equal(Path.Combine(outputDirectory, "hp-diagnostic-20260831-123456.md"), filePath);
        Assert.Contains("cached diagnostic data only", content, StringComparison.Ordinal);
        Assert.Contains("does not invoke WMI", content, StringComparison.Ordinal);
        Assert.Contains(HpDiagnosticStatusText.FanControlNotImplemented, content, StringComparison.Ordinal);
        Assert.Contains("Model: Victus", content, StringComparison.Ordinal);
        Assert.Contains("SetFanMax is NO-GO / design-only.", content, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildMarkdown_IncludesNoWriteManualEvidenceCaptureTemplate()
    {
        string content = HpDiagnosticReportExporter.BuildMarkdown("Model: Victus", DateTimeOffset.UnixEpoch);

        Assert.Contains("## SetFanMax Manual Evidence Capture Template", content, StringComparison.Ordinal);
        Assert.Contains("NO-GO: no fan write has been performed.", content, StringComparison.Ordinal);
        Assert.Contains("Payload length is not selected; do not guess 1-byte vs 4-byte.", content, StringComparison.Ordinal);
        Assert.Contains("- Device model: ", content, StringComparison.Ordinal);
        Assert.Contains("- SystemDesignData summary: ", content, StringComparison.Ordinal);
        Assert.Contains("- Evidence timestamps/conditions: ", content, StringComparison.Ordinal);
        Assert.Contains("- FanGetLevel raw baseline: ", content, StringComparison.Ordinal);
        Assert.Contains("- Non-write temperature source/baseline, if available: ", content, StringComparison.Ordinal);
        Assert.Contains("- Payload evidence method/command/type/input size: ", content, StringComparison.Ordinal);
        Assert.Contains("- Payload state-byte meaning: ", content, StringComparison.Ordinal);
        Assert.Contains("- Enable readback evidence: ", content, StringComparison.Ordinal);
        Assert.Contains("- Restore readback evidence: ", content, StringComparison.Ordinal);
        Assert.Contains("- Restore/disable observation: ", content, StringComparison.Ordinal);
        Assert.Contains("- Reviewer name/date: ", content, StringComparison.Ordinal);
        Assert.Contains("- Evidence references/conflicts reviewed: ", content, StringComparison.Ordinal);
        Assert.Contains("- Human approval checkpoint: ", content, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildMarkdown_PreservesExperimentalEvidenceAndNoControlStatusFromSummary()
    {
        string summary = HpDiagnosticDashboardFormatter.BuildSummary(new()
        {
            SetFanMaxExperimentalPayloadCandidate = "FourByte",
            SetFanMaxPhysicalResponseObserved = "True",
            SetFanMaxPhysicalResponseConfirmationCount = "2",
            SetFanMaxReadbackReliable = "False",
            SetFanMaxNormalControlValidated = "False",
            SetFanMaxUserFacingControlAllowed = "False"
        });

        string content = HpDiagnosticReportExporter.BuildMarkdown(summary, DateTimeOffset.UnixEpoch);

        Assert.Contains("Experimental payload candidate: FourByte - experimental only", content, StringComparison.Ordinal);
        Assert.Contains("Physical response observed: True - observed in two manual four-byte experiments; experimental only", content, StringComparison.Ordinal);
        Assert.Contains("Normal control validated: " + HpDiagnosticDashboardFormatter.SetFanMaxNormalControlNotValidated, content, StringComparison.Ordinal);
        Assert.Contains("User-facing control allowed: " + HpDiagnosticDashboardFormatter.SetFanMaxUserFacingControlNotAllowed, content, StringComparison.Ordinal);
    }

    [Fact]
    public void Exporter_HasNoWmiDependencyOrInvocationSurface()
    {
        Type exporterType = typeof(HpDiagnosticReportExporter);

        Assert.DoesNotContain(
            exporterType.GetMethods(),
            method => method.Name.Contains("Invoke", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            exporterType.GetMethods().SelectMany(method => method.GetParameters()),
            parameter => parameter.ParameterType.Namespace?.Contains("Management", StringComparison.Ordinal) == true);
    }
}
