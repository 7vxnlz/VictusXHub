namespace GHelper.Hardware.Hp;

public static class HpFanGetCountDecoder
{
    public const int ExpectedLength = 4;
    private const int MinimumKnownLength = 2;

    public static HpFanGetCountSnapshot Decode(byte[]? data)
    {
        if (data is null)
        {
            return Invalid(0, "FanGetCount output is null.");
        }

        if (data.Length < MinimumKnownLength)
        {
            return Invalid(data.Length, $"FanGetCount output must contain at least {MinimumKnownLength} bytes for the currently known fields.");
        }

        byte protectionStatus = data[1];

        return new HpFanGetCountSnapshot(
            IsValid: true,
            Length: data.Length,
            ExpectedLength: ExpectedLength,
            ValidationError: data.Length == ExpectedLength ? string.Empty : $"Expected {ExpectedLength} bytes, but received {data.Length}. Decoding only the known prefix.",
            FanCount: data[0],
            ProtectionStatusRaw: protectionStatus,
            OverCurrentProtectionTripped: (protectionStatus & 0x01) != 0,
            OverTemperatureProtectionTripped: (protectionStatus & 0x02) != 0,
            KnownPrefixHex: ToHex(data.Take(MinimumKnownLength)),
            UnknownByteCount: Math.Max(0, data.Length - MinimumKnownLength),
            UnknownByteRange: data.Length > MinimumKnownLength ? $"{MinimumKnownLength}..{data.Length - 1}" : string.Empty,
            UnknownNonZeroByteCount: data.Skip(MinimumKnownLength).Count(value => value != 0));
    }

    private static HpFanGetCountSnapshot Invalid(int length, string validationError) =>
        new(
            IsValid: false,
            Length: length,
            ExpectedLength: ExpectedLength,
            ValidationError: validationError,
            FanCount: null,
            ProtectionStatusRaw: null,
            OverCurrentProtectionTripped: null,
            OverTemperatureProtectionTripped: null,
            KnownPrefixHex: string.Empty,
            UnknownByteCount: 0,
            UnknownByteRange: string.Empty,
            UnknownNonZeroByteCount: 0);

    private static string ToHex(IEnumerable<byte> bytes) =>
        string.Join("-", bytes.Select(value => value.ToString("X2", System.Globalization.CultureInfo.InvariantCulture)));
}

public sealed record HpFanGetCountSnapshot(
    bool IsValid,
    int Length,
    int ExpectedLength,
    string ValidationError,
    byte? FanCount,
    byte? ProtectionStatusRaw,
    bool? OverCurrentProtectionTripped,
    bool? OverTemperatureProtectionTripped,
    string KnownPrefixHex,
    int UnknownByteCount,
    string UnknownByteRange,
    int UnknownNonZeroByteCount);
