namespace GHelper.Hardware.Hp;

internal enum HpKeyboardBacklightAvailability
{
    Unavailable,
    NotSupported,
    SupportedStateUnavailable,
    SupportedStateAvailable
}

internal readonly record struct HpKeyboardBacklightStatus(
    HpKeyboardBacklightAvailability Availability,
    bool? IsOn)
{
    private string? UnavailableDetail { get; init; }

    internal static HpKeyboardBacklightStatus Unavailable { get; } =
        new(HpKeyboardBacklightAvailability.Unavailable, null);

    internal static HpKeyboardBacklightStatus Resolve(
        HpKeyboardStatusReadOnlyProbeGateResult gate,
        HpKeyboardStatusReadOnlyProbeResult result)
    {
        ArgumentNullException.ThrowIfNull(gate);
        ArgumentNullException.ThrowIfNull(result);

        if (!gate.IsAccepted)
            return Unavailable;

        if (!result.IsCaptured)
        {
            return new(HpKeyboardBacklightAvailability.SupportedStateUnavailable, null)
            {
                UnavailableDetail = result.Availability + ": " + result.Detail.Trim()
            };
        }

        if (result.RawReturnCode is not 0 || result.RawData is not { Length: HpKeyboardStatusReadOnlyProbeResult.RequiredDataLength } data ||
            data.Skip(1).Any(value => value != 0))
        {
            return new(HpKeyboardBacklightAvailability.SupportedStateUnavailable, null);
        }

        return data[0] switch
        {
            0x00 => new(HpKeyboardBacklightAvailability.SupportedStateAvailable, false),
            0xE4 => new(HpKeyboardBacklightAvailability.SupportedStateAvailable, true),
            _ => new(HpKeyboardBacklightAvailability.SupportedStateUnavailable, null)
        };
    }

    public string CapabilityText => Availability switch
    {
        HpKeyboardBacklightAvailability.SupportedStateAvailable => "Supported",
        HpKeyboardBacklightAvailability.SupportedStateUnavailable => "Supported, state unavailable",
        HpKeyboardBacklightAvailability.NotSupported => "Not supported",
        _ => "Unavailable"
    };

    public string SupportText => Availability switch
    {
        HpKeyboardBacklightAvailability.SupportedStateAvailable => "Supported",
        HpKeyboardBacklightAvailability.SupportedStateUnavailable => "Supported",
        HpKeyboardBacklightAvailability.NotSupported => "Not supported",
        _ => "Unavailable"
    };

    public string DisplayText => $"Keyboard lighting: {CapabilityText}";

    public string CurrentStateText => IsOn switch
    {
        true => "On",
        false => "Off",
        _ => "Unavailable"
    };

    public string EvidenceText => Availability switch
    {
        HpKeyboardBacklightAvailability.SupportedStateAvailable =>
            "Keyboard backlight: " + CurrentStateText + " (Current startup KeyboardStatus).",
        HpKeyboardBacklightAvailability.SupportedStateUnavailable =>
            UnavailableDetail is not null
                ? "Keyboard backlight: Unavailable (Current startup KeyboardStatus; " + UnavailableDetail + ")."
                : "Keyboard backlight: Unavailable (Current startup KeyboardStatus returned an unrecognized raw shape or value).",
        HpKeyboardBacklightAvailability.NotSupported => "Keyboard backlight: Not supported for the detected non-HP/Victus identity.",
        _ => "Keyboard backlight: Unavailable; no exact-device current-state source was available."
    };
}
