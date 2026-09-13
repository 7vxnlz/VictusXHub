using System.Globalization;
using System.Text;

namespace GHelper.Hardware.Hp;

internal sealed record HpKeyboardStatusReadOnlyProbeEvidence(
    DateTimeOffset TimestampUtc,
    HpKeyboardStatusReadOnlyProbeGateResult IdentityGate,
    HpKeyboardStatusReadOnlyProbeResult? Result);

internal sealed record HpKeyboardStatusReadOnlyProbeEvidenceWriteResult(
    bool IsPersisted,
    string FilePath,
    string? Error);

internal static class HpKeyboardStatusReadOnlyProbeEvidenceWriter
{
    internal const string EvidenceDirectoryName = "VictusXHub";
    internal const string EvidenceFileName = "hp-keyboard-status-readonly-probe.txt";

    internal static string EvidencePath => BuildEvidencePath(Path.GetTempPath());

    internal static string BuildEvidencePath(string temporaryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(temporaryPath);
        return Path.Combine(temporaryPath, EvidenceDirectoryName, EvidenceFileName);
    }

    internal static HpKeyboardStatusReadOnlyProbeEvidenceWriteResult Write(HpKeyboardStatusReadOnlyProbeEvidence evidence) =>
        Write(evidence, EvidencePath);

    internal static HpKeyboardStatusReadOnlyProbeEvidenceWriteResult Write(
        HpKeyboardStatusReadOnlyProbeEvidence evidence,
        string filePath)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        try
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new IOException("The keyboard STATUS evidence path has no directory.");
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

    internal static string Format(HpKeyboardStatusReadOnlyProbeEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        HpKeyboardStatusReadOnlyProbeResult? result = evidence.Result;
        byte[]? rawData = result?.RawData;
        string nonZeroIndexes = rawData is null
            ? "unavailable"
            : string.Join(",", rawData
                .Select((value, index) => (value, index))
                .Where(item => item.value != 0)
                .Select(item => item.index.ToString(CultureInfo.InvariantCulture)));

        if (rawData is not null && nonZeroIndexes.Length == 0)
        {
            nonZeroIndexes = "none";
        }

        return string.Join(Environment.NewLine,
            "TimestampUtc: " + evidence.TimestampUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            "IdentityGate: " + (evidence.IdentityGate.IsAccepted ? "accepted" : "rejected"),
            "Transport: " + (result?.Availability.ToString() ?? "NotInvoked"),
            "InvocationAttempted: " + (result?.Invoked.ToString().ToLowerInvariant() ?? "unavailable"),
            "TransportDetail: " + (result?.Detail ?? "not invoked"),
            "RawReturnCode: " + FormatReturnCode(result?.RawReturnCode),
            "ReturnedDataLength: " + (result?.ReturnedDataLength?.ToString(CultureInfo.InvariantCulture) ?? "unavailable"),
            "RawByte0: " + (rawData is { Length: > 0 } ? "0x" + rawData[0].ToString("X2", CultureInfo.InvariantCulture) : "unavailable"),
            "NonZeroIndexes: " + nonZeroIndexes,
            "RawDataHex: " + (rawData is null ? "unavailable" : Convert.ToHexString(rawData)),
            string.Empty);
    }

    private static string FormatReturnCode(int? returnCode) => returnCode.HasValue
        ? "0x" + unchecked((uint)returnCode.Value).ToString("X8", CultureInfo.InvariantCulture)
        : "unavailable";
}
