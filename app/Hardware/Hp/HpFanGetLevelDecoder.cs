namespace GHelper.Hardware.Hp;

public static class HpFanGetLevelDecoder
{
    public const int ExpectedLength = 128;
    private const int KnownPrefixLength = 2;

    public static HpFanGetLevelSnapshot Decode(byte[]? data)
    {
        if (data is null)
        {
            return Invalid(0, "FanGetLevel output is null.");
        }

        if (data.Length < KnownPrefixLength)
        {
            return Invalid(data.Length, $"FanGetLevel output must contain at least {KnownPrefixLength} bytes for the currently known raw values.");
        }

        byte[] rawValueBytes = data.Take(KnownPrefixLength).ToArray();

        return new HpFanGetLevelSnapshot(
            IsValid: true,
            Length: data.Length,
            ExpectedLength: ExpectedLength,
            ValidationError: data.Length == ExpectedLength ? string.Empty : $"Expected {ExpectedLength} bytes, but received {data.Length}. Decoding only the known raw prefix.",
            Fan1RawValue: rawValueBytes[0],
            Fan2RawValue: rawValueBytes[1],
            RawValueBytes: rawValueBytes,
            KnownPrefixHex: ToHex(rawValueBytes),
            UnknownByteCount: Math.Max(0, data.Length - KnownPrefixLength),
            UnknownByteRange: data.Length > KnownPrefixLength ? $"{KnownPrefixLength}..{data.Length - 1}" : string.Empty,
            UnknownNonZeroByteCount: data.Skip(KnownPrefixLength).Count(value => value != 0));
    }

    private static HpFanGetLevelSnapshot Invalid(int length, string validationError) =>
        new(
            IsValid: false,
            Length: length,
            ExpectedLength: ExpectedLength,
            ValidationError: validationError,
            Fan1RawValue: null,
            Fan2RawValue: null,
            RawValueBytes: [],
            KnownPrefixHex: string.Empty,
            UnknownByteCount: 0,
            UnknownByteRange: string.Empty,
            UnknownNonZeroByteCount: 0);

    private static string ToHex(IEnumerable<byte> bytes) =>
        string.Join("-", bytes.Select(value => value.ToString("X2", System.Globalization.CultureInfo.InvariantCulture)));
}

public sealed record HpFanGetLevelSnapshot(
    bool IsValid,
    int Length,
    int ExpectedLength,
    string ValidationError,
    byte? Fan1RawValue,
    byte? Fan2RawValue,
    byte[] RawValueBytes,
    string KnownPrefixHex,
    int UnknownByteCount,
    string UnknownByteRange,
    int UnknownNonZeroByteCount);
