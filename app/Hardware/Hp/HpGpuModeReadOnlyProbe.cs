using System.Globalization;

namespace GHelper.Hardware.Hp;

internal enum HpGpuModeReadOnlyProbeAvailability
{
    Captured,
    DeviceMismatch,
    TransportUnavailable,
    InvalidReturnCode,
    InvalidOutputLength
}

internal enum HpSelectedBiosGraphicsConfiguration
{
    Unknown,
    Hybrid,
    Discrete,
    Optimus,
    Uma
}

internal sealed record HpGpuModeReadOnlyProbeDevice(
    HpRyzenTemperatureProbeDevice RyzenDevice,
    string BoardProduct);

internal sealed record HpGpuModeReadOnlyProbeGateResult(
    bool IsAccepted,
    HpGpuModeReadOnlyProbeAvailability Availability,
    string Reason)
{
    internal static HpGpuModeReadOnlyProbeGateResult Accepted { get; } =
        new(true, HpGpuModeReadOnlyProbeAvailability.Captured, "Exact HP Victus GPU BIOS-selection target gate accepted.");
}

internal static class HpGpuModeReadOnlyProbeGate
{
    internal const string ExpectedBoardProduct = "8BD4";

    internal static HpGpuModeReadOnlyProbeGateResult Evaluate(HpGpuModeReadOnlyProbeDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        if (!HpRyzenTemperatureProbeGate.Evaluate(device.RyzenDevice).IsAccepted)
        {
            return new(false, HpGpuModeReadOnlyProbeAvailability.DeviceMismatch,
                "Exact HP Victus SKU/CPU identity gate did not match.");
        }

        if (!string.Equals(device.RyzenDevice.Identity.NormalizedBios, "F 31", StringComparison.Ordinal))
        {
            return new(false, HpGpuModeReadOnlyProbeAvailability.DeviceMismatch,
                "Exact F.31 BIOS gate did not match.");
        }

        if (!string.Equals(NormalizeBoardProduct(device.BoardProduct), ExpectedBoardProduct, StringComparison.Ordinal))
        {
            return new(false, HpGpuModeReadOnlyProbeAvailability.DeviceMismatch,
                "Exact 8BD4 board-product gate did not match.");
        }

        return HpGpuModeReadOnlyProbeGateResult.Accepted;
    }

    private static string NormalizeBoardProduct(string value) =>
        new string(value.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
}

internal sealed record HpGpuModeReadOnlyProbeResult(
    HpGpuModeReadOnlyProbeAvailability Availability,
    bool Invoked,
    int? RawReturnCode,
    int? ReturnedDataLength,
    byte[]? RawData,
    byte? RawByte0,
    byte? MaskedByte0,
    HpSelectedBiosGraphicsConfiguration SelectedBiosGraphicsConfiguration,
    string Detail)
{
    internal const int RequiredDataLength = 4;
    internal const string SemanticBoundary = "BIOS-selected configuration; applied/current topology not yet proven";

    internal bool IsCaptured =>
        Availability == HpGpuModeReadOnlyProbeAvailability.Captured &&
        RawReturnCode == 0 &&
        RawData?.Length == RequiredDataLength;

    internal string SelectedBiosGraphicsConfigurationText => SelectedBiosGraphicsConfiguration switch
    {
        HpSelectedBiosGraphicsConfiguration.Uma => "UMA",
        _ => SelectedBiosGraphicsConfiguration.ToString()
    };

    internal static HpGpuModeReadOnlyProbeResult FromInvocation(HpWmiInvocationResult invocation)
    {
        ArgumentNullException.ThrowIfNull(invocation);

        if (!invocation.Invoked || !invocation.Success)
        {
            return Failure(HpGpuModeReadOnlyProbeAvailability.TransportUnavailable, invocation,
                string.Join(" | ", invocation.Errors));
        }

        if (invocation.ReturnCode != 0)
        {
            return Failure(HpGpuModeReadOnlyProbeAvailability.InvalidReturnCode, invocation,
                "Expected raw rwReturnCode 0 for a decodable response.");
        }

        if (invocation.ReturnedBytes is not { Length: RequiredDataLength } data)
        {
            return Failure(HpGpuModeReadOnlyProbeAvailability.InvalidOutputLength, invocation,
                $"Expected exactly {RequiredDataLength} returned data bytes; received {invocation.ReturnedBytes?.Length ?? 0}.");
        }

        byte rawByte0 = data[0];
        byte maskedByte0 = (byte)(rawByte0 & 0x7F);
        return new(
            HpGpuModeReadOnlyProbeAvailability.Captured,
            true,
            invocation.ReturnCode,
            data.Length,
            data.ToArray(),
            rawByte0,
            maskedByte0,
            Decode(maskedByte0),
            "Raw BIOS graphics-selection data captured with fail-closed decoding.");
    }

    private static HpGpuModeReadOnlyProbeResult Failure(
        HpGpuModeReadOnlyProbeAvailability availability,
        HpWmiInvocationResult invocation,
        string detail)
    {
        byte[]? rawData = invocation.ReturnedBytes?.ToArray();
        return new(availability, invocation.Invoked, invocation.ReturnCode, rawData?.Length,
            rawData, rawData is { Length: > 0 } ? rawData[0] : null, null,
            HpSelectedBiosGraphicsConfiguration.Unknown, detail);
    }

    private static HpSelectedBiosGraphicsConfiguration Decode(byte value) => value switch
    {
        0x00 => HpSelectedBiosGraphicsConfiguration.Hybrid,
        0x01 => HpSelectedBiosGraphicsConfiguration.Discrete,
        0x02 => HpSelectedBiosGraphicsConfiguration.Optimus,
        0x03 => HpSelectedBiosGraphicsConfiguration.Uma,
        _ => HpSelectedBiosGraphicsConfiguration.Unknown
    };
}

internal static class HpGpuModeReadOnlyProbeFormatter
{
    internal static IReadOnlyList<string> Format(HpGpuModeReadOnlyProbeResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var lines = new List<string>
        {
            "WMI transport: " + result.Availability,
            "rwReturnCode (raw): " + FormatReturnCode(result.RawReturnCode),
            "Returned data length: " + (result.ReturnedDataLength?.ToString(CultureInfo.InvariantCulture) ?? "unavailable"),
            "Data[0] (raw hex): " + FormatByte(result.RawByte0),
            "Data[0] & 0x7F: " + FormatByte(result.MaskedByte0),
            "Selected BIOS graphics configuration: " + result.SelectedBiosGraphicsConfigurationText,
            "Semantic boundary: " + HpGpuModeReadOnlyProbeResult.SemanticBoundary
        };

        if (result.IsCaptured)
        {
            lines.Add("Data (raw hex): " + Convert.ToHexString(result.RawData!));
        }

        lines.Add("Detail: " + result.Detail);
        return lines;
    }

    private static string FormatByte(byte? value) => value.HasValue
        ? "0x" + value.Value.ToString("X2", CultureInfo.InvariantCulture)
        : "unavailable";

    private static string FormatReturnCode(int? returnCode) => returnCode.HasValue
        ? "0x" + unchecked((uint)returnCode.Value).ToString("X8", CultureInfo.InvariantCulture)
        : "unavailable";
}
