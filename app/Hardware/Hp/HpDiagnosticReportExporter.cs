using System.Text;

namespace GHelper.Hardware.Hp;

public static class HpDiagnosticReportExporter
{
    public static string ExportDirectory => HpDiagnosticPaths.ExportDirectory;

    public static string Export(string summary) => Export(summary, DateTimeOffset.Now, ExportDirectory);

    public static string Export(string summary, DateTimeOffset timestamp, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        string fileName = "hp-diagnostic-" + timestamp.ToString("yyyyMMdd-HHmmss") + ".md";
        string filePath = Path.Combine(outputDirectory, fileName);
        File.WriteAllText(filePath, BuildMarkdown(summary, timestamp), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        return filePath;
    }

    public static string BuildMarkdown(string summary, DateTimeOffset timestamp)
    {
        return string.Join(Environment.NewLine,
            "# " + HpDiagnosticStatusText.Title + " Export",
            string.Empty,
            "Exported: " + timestamp.ToString("O"),
            string.Empty,
            "> This export contains cached diagnostic data only. It does not invoke WMI or refresh hardware.",
            "> " + HpDiagnosticStatusText.FanControlNotImplemented + ". Fan writes are not implemented. " + HpDiagnosticStatusText.SetFanMaxNoGo + ".",
            string.Empty,
            "## Diagnostic Summary",
            string.Empty,
            summary.Trim(),
            string.Empty,
            "## SetFanMax Manual Evidence Capture Template",
            string.Empty,
            "> NO-GO: no fan write has been performed. Payload length is not selected; do not guess 1-byte vs 4-byte.",
            "> This is a manual record only. It does not authorize a write or an experiment.",
            string.Empty,
            "- Device model: ",
            "- SKU: ",
            "- BIOS version: ",
            "- Thermal policy version: ",
            "- SystemDesignData summary: ",
            "- AC/battery state: ",
            "- Evidence timestamps/conditions: ",
            "- FanGetCount baseline: ",
            "- FanMaxGet baseline: ",
            "- FanGetLevel raw baseline: ",
            "- Non-write temperature source/baseline, if available: ",
            "- External fan noise observation: ",
            "- Thermal observation: ",
            "- Payload length evidence candidate, if any: ",
            "- Payload evidence method/command/type/input size: ",
            "- Payload state-byte meaning: ",
            "- Enable readback evidence: ",
            "- Restore readback evidence: ",
            "- Restore/disable observation: ",
            "- Failure/recovery notes: ",
            "- Reviewer name/date: ",
            "- Evidence references/conflicts reviewed: ",
            "- Human approval checkpoint: ",
            string.Empty);
    }
}
