namespace GHelper.Hardware.Hp;

public enum HpFanMaxValidatedInputLength
{
    OneByte = 1,
    FourBytes = 4
}

public sealed record HpFanMaxPayloadDescription
{
    public const string ReferenceMethodName = "hpqBIOSInt0";
    public const uint ReferenceCommandValue = 0x20008;
    public const uint ReferenceCommandType = 0x27;
    public const int ReferenceExpectedOutputSize = 0;

    private HpFanMaxPayloadDescription(
        HpFanMaxTargetState targetState,
        HpFanMaxValidatedInputLength deviceValidatedInputLength,
        byte stateByteValue,
        int zeroFilledTrailingByteCount)
    {
        TargetState = targetState;
        DeviceValidatedInputLength = deviceValidatedInputLength;
        StateByteValue = stateByteValue;
        ZeroFilledTrailingByteCount = zeroFilledTrailingByteCount;
    }

    public HpFanMaxTargetState TargetState { get; }
    public HpFanMaxValidatedInputLength DeviceValidatedInputLength { get; }
    public byte StateByteValue { get; }
    public int ZeroFilledTrailingByteCount { get; }

    public static bool IsSupportedInputLength(HpFanMaxValidatedInputLength inputLength) =>
        inputLength is HpFanMaxValidatedInputLength.OneByte or HpFanMaxValidatedInputLength.FourBytes;

    public static HpFanMaxPayloadDescription Describe(
        HpFanMaxTargetState targetState,
        HpFanMaxValidatedInputLength deviceValidatedInputLength)
    {
        if (!IsSupportedInputLength(deviceValidatedInputLength))
        {
            throw new ArgumentOutOfRangeException(
                nameof(deviceValidatedInputLength),
                deviceValidatedInputLength,
                "Only a device-validated one-byte or four-byte SetFanMax input shape is supported.");
        }

        byte stateByteValue = targetState switch
        {
            HpFanMaxTargetState.EnableMaxFan => 1,
            HpFanMaxTargetState.RestoreDisableMaxFan => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(targetState), targetState, "Only named SetFanMax target states are supported.")
        };

        int zeroFilledTrailingByteCount = (int)deviceValidatedInputLength - 1;
        return new(targetState, deviceValidatedInputLength, stateByteValue, zeroFilledTrailingByteCount);
    }
}
