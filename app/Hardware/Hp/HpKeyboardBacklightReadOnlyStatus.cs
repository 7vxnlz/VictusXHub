namespace GHelper.Hardware.Hp;

internal enum HpKeyboardBacklightAvailability
{
    Unavailable,
    NotSupported,
    SupportedStateUnavailable
}

internal readonly record struct HpKeyboardBacklightStatus(
    HpKeyboardBacklightAvailability Availability,
    bool? IsOn,
    int? BrightnessLevel)
{
    private const string TargetSku = "7Z5Z2EA";
    private const string TargetModel = "16-s0035";

    public static HpKeyboardBacklightStatus Resolve(bool? hpVictusDetected, string? model, string? sku)
    {
        if (hpVictusDetected == false)
            return new(HpKeyboardBacklightAvailability.NotSupported, null, null);

        bool exactSku = sku?.StartsWith(TargetSku, StringComparison.OrdinalIgnoreCase) == true;
        bool exactModel = model?.Contains(TargetModel, StringComparison.OrdinalIgnoreCase) == true;
        return hpVictusDetected == true && exactSku && exactModel
            ? new(HpKeyboardBacklightAvailability.SupportedStateUnavailable, null, null)
            : new(HpKeyboardBacklightAvailability.Unavailable, null, null);
    }

    public string CapabilityText => Availability switch
    {
        HpKeyboardBacklightAvailability.SupportedStateUnavailable => "Supported, state unavailable",
        HpKeyboardBacklightAvailability.NotSupported => "Not supported",
        _ => "Unavailable"
    };

    public string DisplayText => $"Keyboard lighting: {CapabilityText}";

    public string EvidenceText => Availability switch
    {
        HpKeyboardBacklightAvailability.SupportedStateUnavailable =>
            "Keyboard backlight: Supported by exact-SKU reference evidence; current state and brightness levels are unavailable. BIOS F.31 state is not validated.",
        HpKeyboardBacklightAvailability.NotSupported => "Keyboard backlight: Not supported for the detected non-HP/Victus identity.",
        _ => "Keyboard backlight: Unavailable; no exact supported identity or safe state source was established."
    };
}
