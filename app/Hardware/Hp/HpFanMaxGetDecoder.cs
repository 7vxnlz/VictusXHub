namespace GHelper.Hardware.Hp;

public static class HpFanMaxGetDecoder
{
    public const int ExpectedLength = 4;
    private const int MinimumKnownLength = 1;

    public static HpFanMaxGetSnapshot Decode(byte[]? data)
    {
        if (data is null)
        {
            return Invalid(0, "FanMaxGet output is null.");
        }

        if (data.Length < MinimumKnownLength)
        {
            return Invalid(data.Length, $"FanMaxGet output must contain at least {MinimumKnownLength} byte for the currently known fields.");
        }

        byte maxFanState = data[0];

        return new HpFanMaxGetSnapshot(
            IsValid: true,
            Length: data.Length,
            ExpectedLength: ExpectedLength,
            ValidationError: data.Length == ExpectedLength ? string.Empty : $"Expected {ExpectedLength} bytes, but received {data.Length}. Decoding only the known prefix.",
            MaxFanStateRaw: maxFanState,
            IsMaxFanEnabled: maxFanState != 0,
            KnownPrefixHex: ToHex(data.Take(MinimumKnownLength)),
            UnknownByteCount: Math.Max(0, data.Length - MinimumKnownLength),
            UnknownByteRange: data.Length > MinimumKnownLength ? $"{MinimumKnownLength}..{data.Length - 1}" : string.Empty,
            UnknownNonZeroByteCount: data.Skip(MinimumKnownLength).Count(value => value != 0));
    }

    private static HpFanMaxGetSnapshot Invalid(int length, string validationError) =>
        new(
            IsValid: false,
            Length: length,
            ExpectedLength: ExpectedLength,
            ValidationError: validationError,
            MaxFanStateRaw: null,
            IsMaxFanEnabled: null,
            KnownPrefixHex: string.Empty,
            UnknownByteCount: 0,
            UnknownByteRange: string.Empty,
            UnknownNonZeroByteCount: 0);

    private static string ToHex(IEnumerable<byte> bytes) =>
        string.Join("-", bytes.Select(value => value.ToString("X2", System.Globalization.CultureInfo.InvariantCulture)));
}

public sealed record HpFanMaxGetSnapshot(
    bool IsValid,
    int Length,
    int ExpectedLength,
    string ValidationError,
    byte? MaxFanStateRaw,
    bool? IsMaxFanEnabled,
    string KnownPrefixHex,
    int UnknownByteCount,
    string UnknownByteRange,
    int UnknownNonZeroByteCount);
