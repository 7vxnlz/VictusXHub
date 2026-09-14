using System.Globalization;
using System.Text;

namespace GHelper.Hardware.Hp;

internal sealed record HpGpuModeReadOnlyProbeEvidence(
    DateTimeOffset TimestampUtc,
    HpGpuModeReadOnlyProbeGateResult IdentityGate,
    HpGpuModeReadOnlyProbeResult? Result);

internal sealed record HpGpuModeReadOnlyProbeEvidenceWriteResult(
    bool IsPersisted,
    string FilePath,
    string? Error);

internal static class HpGpuModeReadOnlyProbeEvidenceWriter
{
    internal const string EvidenceDirectoryName = "VictusXHub";
    internal const string EvidenceFileName = "hp-gpu-mode-readonly-probe.txt";

    internal static string EvidencePath => BuildEvidencePath(Path.GetTempPath());

    internal static string BuildEvidencePath(string temporaryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(temporaryPath);
        return Path.Combine(temporaryPath, EvidenceDirectoryName, EvidenceFileName);
    }

    internal static HpGpuModeReadOnlyProbeEvidenceWriteResult Write(HpGpuModeReadOnlyProbeEvidence evidence) =>
        Write(evidence, EvidencePath);

    internal static HpGpuModeReadOnlyProbeEvidenceWriteResult Write(
        HpGpuModeReadOnlyProbeEvidence evidence,
        string filePath)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        try
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new IOException("The GPU mode evidence path has no directory.");
            }

            Directory.CreateDirectory(directory);
            File.WriteAllText(filePath, Format(evidence), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            return new(true, filePath, null);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return new(false, filePath, ex.Message);
        }
    }

    internal static string Format(HpGpuModeReadOnlyProbeEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        HpGpuModeReadOnlyProbeResult? result = evidence.Result;
        return string.Join(Environment.NewLine,
            "TimestampUtc: " + evidence.TimestampUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            "IdentityGate: " + (evidence.IdentityGate.IsAccepted ? "accepted" : "rejected"),
            "InvocationAttempted: " + (result?.Invoked.ToString().ToLowerInvariant() ?? "unavailable"),
            "Transport: " + (result?.Availability.ToString() ?? "NotInvoked"),
            "TransportDetail: " + (result?.Detail ?? "not invoked"),
            "RawReturnCode: " + FormatReturnCode(result?.RawReturnCode),
            "ReturnedDataLength: " + (result?.ReturnedDataLength?.ToString(CultureInfo.InvariantCulture) ?? "unavailable"),
            "RawByte0: " + FormatByte(result?.RawByte0),
            "MaskedByte0: " + FormatByte(result?.MaskedByte0),
            "SelectedBiosGraphicsConfiguration: " + (result?.SelectedBiosGraphicsConfigurationText ?? "Unknown"),
            "RawDataHex: " + (result?.RawData is null ? "unavailable" : Convert.ToHexString(result.RawData)),
            "SemanticBoundary: " + HpGpuModeReadOnlyProbeResult.SemanticBoundary,
            string.Empty);
    }

    private static string FormatByte(byte? value) => value.HasValue
        ? "0x" + value.Value.ToString("X2", CultureInfo.InvariantCulture)
        : "unavailable";

    private static string FormatReturnCode(int? returnCode) => returnCode.HasValue
        ? "0x" + unchecked((uint)returnCode.Value).ToString("X8", CultureInfo.InvariantCulture)
        : "unavailable";
}
