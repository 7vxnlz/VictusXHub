namespace GHelper.Hardware.Hp;

internal enum HpGpuModeAvailability
{
    Unavailable,
    NotSupported,
    SupportedStateUnavailable
}

internal enum HpGpuModeState
{
    Hybrid,
    Discrete,
    Optimus
}

internal readonly record struct HpGpuModeStatus(
    HpGpuModeAvailability Availability,
    HpGpuModeState? CurrentMode,
    byte? CapabilityRaw,
    bool? ModeChangeRequiresReboot)
{
    private const string TargetSku = "7Z5Z2EA";
    private const string TargetModel = "16-s0035";

    public static HpGpuModeStatus Resolve(
        bool? hpVictusDetected, string? model, string? sku, byte? gpuModeSwitchRaw)
    {
        if (hpVictusDetected == false)
            return new(HpGpuModeAvailability.NotSupported, null, null, null);

        bool exactSku = sku?.StartsWith(TargetSku, StringComparison.OrdinalIgnoreCase) == true;
        bool exactModel = model?.Contains(TargetModel, StringComparison.OrdinalIgnoreCase) == true;
        if (hpVictusDetected != true || !exactSku || !exactModel || gpuModeSwitchRaw is null)
            return new(HpGpuModeAvailability.Unavailable, null, null, null);

        return gpuModeSwitchRaw == 0
            ? new(HpGpuModeAvailability.NotSupported, null, 0, null)
            : new(HpGpuModeAvailability.SupportedStateUnavailable, null, gpuModeSwitchRaw, true);
    }

    public string CapabilityText => Availability switch
    {
        HpGpuModeAvailability.SupportedStateUnavailable => "Supported, state unavailable",
        HpGpuModeAvailability.NotSupported => "Not supported",
        _ => "Unavailable"
    };

    public string DisplayText => $"GPU Mode: {CapabilityText}";

    public string SwitchingCapabilityText => Availability switch
    {
        HpGpuModeAvailability.SupportedStateUnavailable => "Supported",
        HpGpuModeAvailability.NotSupported => "Not supported",
        _ => "Unavailable"
    };

    public string EvidenceText => Availability switch
    {
        HpGpuModeAvailability.SupportedStateUnavailable =>
            $"GPU graphics switching: Supported by exact-device SystemDesignData capability 0x{CapabilityRaw:X2}; current mode is unavailable. Reference mode changes require reboot; no action is exposed.",
        HpGpuModeAvailability.NotSupported when CapabilityRaw == 0 =>
            "GPU graphics switching: Not supported by the exact-device SystemDesignData capability byte.",
        HpGpuModeAvailability.NotSupported => "GPU graphics switching: Not supported for the detected non-HP/Victus identity.",
        _ => "GPU graphics switching: Unavailable; exact-device capability evidence is missing or ambiguous."
    };
}
