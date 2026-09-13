namespace GHelper.Hardware.Hp;

internal enum HpKeyboardStatusReadOnlyProbeAvailability
{
    Captured,
    DeviceMismatch,
    TransportUnavailable,
    InvalidOutputLength
}

internal sealed record HpKeyboardStatusReadOnlyProbeDevice(
    HpRyzenTemperatureProbeDevice RyzenDevice,
    string BoardProduct);

internal sealed record HpKeyboardStatusReadOnlyProbeGateResult(
    bool IsAccepted,
    HpKeyboardStatusReadOnlyProbeAvailability Availability,
    string Reason)
{
    internal static HpKeyboardStatusReadOnlyProbeGateResult Accepted { get; } =
        new(true, HpKeyboardStatusReadOnlyProbeAvailability.Captured, "Exact HP Victus keyboard STATUS target gate accepted.");
}

internal static class HpKeyboardStatusReadOnlyProbeGate
{
    internal const string ExpectedBoardProduct = "8BD4";

    internal static HpKeyboardStatusReadOnlyProbeGateResult Evaluate(HpKeyboardStatusReadOnlyProbeDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        if (!HpRyzenTemperatureProbeGate.Evaluate(device.RyzenDevice).IsAccepted)
        {
            return new(false, HpKeyboardStatusReadOnlyProbeAvailability.DeviceMismatch,
                "Exact HP Victus SKU/CPU identity gate did not match.");
        }

        if (!string.Equals(device.RyzenDevice.Identity.NormalizedBios, "F 31", StringComparison.Ordinal))
        {
            return new(false, HpKeyboardStatusReadOnlyProbeAvailability.DeviceMismatch,
                "Exact F.31 BIOS gate did not match.");
        }

        if (!string.Equals(NormalizeBoardProduct(device.BoardProduct), ExpectedBoardProduct, StringComparison.Ordinal))
        {
            return new(false, HpKeyboardStatusReadOnlyProbeAvailability.DeviceMismatch,
                "Exact 8BD4 board-product gate did not match.");
        }

        return HpKeyboardStatusReadOnlyProbeGateResult.Accepted;
    }

    private static string NormalizeBoardProduct(string value) =>
        new string(value.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
}

internal sealed record HpKeyboardStatusReadOnlyProbeResult(
    HpKeyboardStatusReadOnlyProbeAvailability Availability,
    bool Invoked,
    int? RawReturnCode,
    int? ReturnedDataLength,
    byte[]? RawData,
    string Detail)
{
    internal const int RequiredDataLength = 128;

    internal bool IsCaptured => Availability == HpKeyboardStatusReadOnlyProbeAvailability.Captured && RawData?.Length == RequiredDataLength;

    internal static HpKeyboardStatusReadOnlyProbeResult FromInvocation(HpWmiInvocationResult invocation)
    {
        ArgumentNullException.ThrowIfNull(invocation);

        if (!invocation.Invoked)
        {
            return new(HpKeyboardStatusReadOnlyProbeAvailability.TransportUnavailable, false, invocation.ReturnCode, invocation.ReturnedBytes?.Length, null,
                string.Join(" | ", invocation.Errors));
        }

        if (!invocation.Success)
        {
            return new(HpKeyboardStatusReadOnlyProbeAvailability.TransportUnavailable, true, invocation.ReturnCode, invocation.ReturnedBytes?.Length, null,
                string.Join(" | ", invocation.Errors));
        }

        if (invocation.ReturnedBytes is not { Length: RequiredDataLength } data)
        {
            return new(HpKeyboardStatusReadOnlyProbeAvailability.InvalidOutputLength, true, invocation.ReturnCode, invocation.ReturnedBytes?.Length, null,
                $"Expected exactly {RequiredDataLength} returned data bytes; received {invocation.ReturnedBytes?.Length ?? 0}.");
        }

        return new(HpKeyboardStatusReadOnlyProbeAvailability.Captured, true, invocation.ReturnCode, data.Length, data.ToArray(),
            "Raw keyboard STATUS data captured without interpretation.");
    }
}

internal static class HpKeyboardStatusReadOnlyProbeFormatter
{
    internal static IReadOnlyList<string> Format(HpKeyboardStatusReadOnlyProbeResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var lines = new List<string>
        {
            "WMI transport: " + result.Availability,
            "rwReturnCode (raw): " + FormatReturnCode(result.RawReturnCode),
            "Returned data length: " + (result.ReturnedDataLength?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "unavailable")
        };

        if (result.IsCaptured)
        {
            lines.Add("Data (raw hex): " + Convert.ToHexString(result.RawData!));
            lines.Add("Data[0] (raw hex): 0x" + result.RawData![0].ToString("X2", System.Globalization.CultureInfo.InvariantCulture));
        }

        lines.Add("Detail: " + result.Detail);
        return lines;
    }

    private static string FormatReturnCode(int? returnCode) => returnCode.HasValue
        ? "0x" + unchecked((uint)returnCode.Value).ToString("X8", System.Globalization.CultureInfo.InvariantCulture)
        : "unavailable";
}
