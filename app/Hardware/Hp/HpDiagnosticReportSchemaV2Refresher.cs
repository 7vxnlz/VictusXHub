using System.Text.Json;
using System.Text.Json.Nodes;

namespace GHelper.Hardware.Hp;

public static class HpDiagnosticReportSchemaV2Refresher
{
    public static bool TryRefreshExistingReport(string reportPath)
    {
        if (string.IsNullOrWhiteSpace(reportPath) || !File.Exists(reportPath))
        {
            return false;
        }

        try
        {
            JsonNode? root = JsonNode.Parse(File.ReadAllText(reportPath));
            if (root is not JsonObject report)
            {
                return false;
            }

            int schemaVersion = GetInt(report, "ReportSchemaVersion") ?? 0;
            bool hasAllExperimentalFields = HpVictusCapabilityReportMetadata.SetFanMaxSchemaV2ExperimentalStatusFields
                .All(field => report.ContainsKey(field));
            if (schemaVersion >= HpVictusCapabilityReportMetadata.SchemaVersion && hasAllExperimentalFields)
            {
                return false;
            }

            report["ReportSchemaVersion"] = HpVictusCapabilityReportMetadata.SchemaVersion;
            report["ReportGeneratedBy"] = GetString(report, "ReportGeneratedBy") ?? "VictusX";
            report["ReportMode"] = GetString(report, "ReportMode") ?? "HP read-only diagnostic";
            report["ReportSource"] = "Cached report schema-v2 local metadata refresh; no WMI or hardware invocation.";
            report["ReportGeneratedAtUtc"] = DateTimeOffset.UtcNow;
            report["SetFanMaxExperimentalPayloadCandidate"] = "FourByte";
            report["SetFanMaxPhysicalResponseObserved"] = true;
            report["SetFanMaxPhysicalResponseConfirmationCount"] = 2;
            report["SetFanMaxReadbackReliable"] = false;
            report["SetFanMaxDeveloperExperimentAllowed"] = true;
            report["SetFanMaxDeveloperExperimentPayload"] = "FourByte";
            report["SetFanMaxNormalControlValidated"] = false;
            report["SetFanMaxUserFacingControlAllowed"] = false;
            report["SetFanMaxWriteImplemented"] = false;
            report["SetFanMaxWriteAllowed"] = false;
            report["SetFanMaxFirstWriteGateStatus"] = HpFanMaxDryRunReport.FirstWriteGateStatus;
            report["SetFanMaxFirstWriteGateSatisfied"] = false;
            report["SetFanMaxFirstWriteGateReason"] = HpFanMaxDryRunReport.FirstWriteGateReason;
            report["SetFanMaxDeviceValidatedInputLength"] = null;

            var options = new JsonSerializerOptions { WriteIndented = true };
            WriteAtomic(reportPath, report.ToJsonString(options));
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return false;
        }
    }

    private static int? GetInt(JsonObject report, string propertyName)
    {
        if (!report.TryGetPropertyValue(propertyName, out JsonNode? node) || node is null)
        {
            return null;
        }

        return node.GetValueKind() == JsonValueKind.Number && node.AsValue().TryGetValue(out int value)
            ? value
            : null;
    }

    private static string? GetString(JsonObject report, string propertyName)
    {
        if (!report.TryGetPropertyValue(propertyName, out JsonNode? node) || node is null)
        {
            return null;
        }

        return node.GetValueKind() == JsonValueKind.String ? node.GetValue<string>() : null;
    }

    private static void WriteAtomic(string path, string content)
    {
        string tempPath = path + ".tmp";
        File.WriteAllText(tempPath, content);
        if (File.Exists(path))
        {
            File.Replace(tempPath, path, path + ".bak");
        }
        else
        {
            File.Move(tempPath, path);
        }
    }
}
