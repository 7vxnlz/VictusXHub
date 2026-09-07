namespace GHelper.Hardware.Hp;

public static class HpSystemDesignDataDecoder
{
    public const int ExpectedLength = 128;
    private const int MinimumKnownLength = 12;

    public static HpSystemDesignDataSnapshot Decode(byte[]? data)
    {
        if (data is null)
        {
            return Invalid(0, "SystemDesignData output is null.");
        }

        if (data.Length < MinimumKnownLength)
        {
            return Invalid(data.Length, $"SystemDesignData output must contain at least {MinimumKnownLength} bytes for the currently known fields.");
        }

        byte platformFeatureFlags = data[4];
        byte loadLine = data[9];
        byte sensorFlags = data[10];
        byte hotkeyFlags = data[11];

        return new HpSystemDesignDataSnapshot(
            IsValid: true,
            Length: data.Length,
            ExpectedLength: ExpectedLength,
            ValidationError: data.Length == ExpectedLength ? string.Empty : $"Expected {ExpectedLength} bytes, but received {data.Length}. Decoding only the known prefix.",
            ShippingAdapterPowerRatingWatts: data[0] | (data[1] << 8),
            ReservedByte2: data[2],
            ThermalPolicyVersion: data[3],
            PlatformFeatureFlags: platformFeatureFlags,
            DeclaresSoftwareFanControlSupport: (platformFeatureFlags & 0x01) != 0,
            DeclaresExtremeModeSupport: (platformFeatureFlags & 0x02) != 0,
            DeclaresExtremeModeUnlock: (platformFeatureFlags & 0x04) != 0,
            DeclaresDtBiosControl: (platformFeatureFlags & 0x08) != 0,
            DeclaresTwoBytePl4Support: (platformFeatureFlags & 0x10) != 0,
            Pl4DefaultValue: data[5],
            DeclaresBiosDefinedOcSupport: (data[6] & 0x01) != 0,
            GpuModeSwitchRaw: data[7],
            DefaultCpuPowerLimitWithGpuWatts: data[8],
            LoadLineSupportLevels: loadLine & 0x0F,
            DefaultLoadLine: (loadLine & 0xF0) >> 4,
            SensorFlags: sensorFlags,
            ChangeIrSensorToBoard: (sensorFlags & 0x03) == 0x02,
            DeclaresPchOverheatSupport: (sensorFlags & 0x04) != 0,
            DeclaresVrSensorSupport: (sensorFlags & 0x08) != 0,
            HotkeyFlags: hotkeyFlags,
            DeclaresFnPHotkeySupport: (hotkeyFlags & 0x01) != 0,
            DeclaresFnF1HotkeySupport: (hotkeyFlags & 0x02) != 0,
            KnownPrefixHex: ToHex(data.Take(MinimumKnownLength)),
            UnknownByteCount: Math.Max(0, data.Length - MinimumKnownLength),
            UnknownByteRange: data.Length > MinimumKnownLength ? $"{MinimumKnownLength}..{data.Length - 1}" : string.Empty,
            UnknownNonZeroByteCount: data.Skip(MinimumKnownLength).Count(value => value != 0));
    }

    private static HpSystemDesignDataSnapshot Invalid(int length, string validationError) =>
        new(
            IsValid: false,
            Length: length,
            ExpectedLength: ExpectedLength,
            ValidationError: validationError,
            ShippingAdapterPowerRatingWatts: null,
            ReservedByte2: null,
            ThermalPolicyVersion: null,
            PlatformFeatureFlags: null,
            DeclaresSoftwareFanControlSupport: null,
            DeclaresExtremeModeSupport: null,
            DeclaresExtremeModeUnlock: null,
            DeclaresDtBiosControl: null,
            DeclaresTwoBytePl4Support: null,
            Pl4DefaultValue: null,
            DeclaresBiosDefinedOcSupport: null,
            GpuModeSwitchRaw: null,
            DefaultCpuPowerLimitWithGpuWatts: null,
            LoadLineSupportLevels: null,
            DefaultLoadLine: null,
            SensorFlags: null,
            ChangeIrSensorToBoard: null,
            DeclaresPchOverheatSupport: null,
            DeclaresVrSensorSupport: null,
            HotkeyFlags: null,
            DeclaresFnPHotkeySupport: null,
            DeclaresFnF1HotkeySupport: null,
            KnownPrefixHex: string.Empty,
            UnknownByteCount: 0,
            UnknownByteRange: string.Empty,
            UnknownNonZeroByteCount: 0);

    private static string ToHex(IEnumerable<byte> bytes) =>
        string.Join("-", bytes.Select(value => value.ToString("X2", System.Globalization.CultureInfo.InvariantCulture)));
}
