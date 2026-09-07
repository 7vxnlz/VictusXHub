namespace GHelper.Hardware.Hp;

internal sealed record HpReadOnlyTelemetryDisplay(
    string Cpu, string Gpu, string FanAndDevice, string Battery, string BatteryCare, string Display, string Summary,
    HpTrayTelemetryStatus TrayStatus)
{
    internal string Availability { get; init; } = "Unavailable";
    internal string CpuLoad { get; init; } = "Unavailable";
    internal string GpuTemperature { get; init; } = "Unavailable";
    internal string BatteryPower { get; init; } = "Unavailable";
    internal string RefreshRate { get; init; } = "Unavailable";
    internal string CpuTemperature { get; init; } = "Unavailable";
    internal string FanRpm { get; init; } = "Unavailable";
    internal string BatteryCareStatus { get; init; } = "Unavailable";
    internal string BatteryCareCapability { get; init; } = "Unavailable";
}

internal sealed record HpTrayTelemetryStatus(string Cpu, string Gpu, string Battery, string Screen)
{
    internal IReadOnlyList<string> Rows => [Cpu, Gpu, Battery, Screen];

    internal static HpTrayTelemetryStatus Unavailable { get; } = new(
        "CPU: Unavailable", "GPU: Unavailable", "Battery: Unavailable", "Screen: Unavailable");
}

internal static class HpReadOnlyTelemetryFormatter
{
    public static HpReadOnlyTelemetryDisplay Format(
        HpReadOnlyTelemetrySnapshot snapshot, DateTimeOffset now, bool? hpVictusDetected, bool cachedIdentity)
    {
        bool fresh = snapshot.PolledAt is { } time && now >= time &&
            now - time <= HpReadOnlyTelemetryProvider.MaximumSampleAge;
        HpReadOnlyTelemetrySnapshot current = fresh ? snapshot : HpReadOnlyTelemetrySnapshot.Unavailable;
        string load = current.CpuLoadPercent is { } percent ? $"{percent}% load" : "Load: Unknown";
        string battery = current.BatteryPresent == false ? "No battery" :
            current.BatteryPercent is { } charge ? $"{charge}%" : "Unavailable";
        string ac = current.AcOnline switch { true => "AC", false => "On battery", _ => "AC unknown" };
        string charging = current.Charging switch { true => "Charging", false => "Not charging", _ => "Charging unknown" };
        string device = hpVictusDetected switch
        {
            true => "HP Victus detected",
            false => "HP Victus not detected",
            _ => "Device: Unknown"
        };
        string identitySource = cachedIdentity ? "cached report" : "startup snapshot";
        string state = snapshot.PolledAt is null ? "Not sampled" : fresh ? "Current" : "Stale";
        string poll = snapshot.PolledAt?.ToUniversalTime().ToString("u") ?? "Unavailable";
        string refreshRate = current.DisplayRefreshRateHz is { } hz ? $"{hz}Hz" : "Unavailable";
        string batteryCare = FormatBatteryCare(current.BatteryCare?.Result);
        bool gpuFresh = current.GpuTemperature is { } gpu && now >= gpu.SampledAt &&
            now - gpu.SampledAt <= HpReadOnlyTelemetryProvider.MaximumSampleAge && gpu.Celsius is > 0 and <= 125;
        string gpuTemperature = gpuFresh
            ? current.GpuTemperature!.Value.Celsius.ToString("0", System.Globalization.CultureInfo.InvariantCulture) + " C"
            : "Unavailable";
        string summary = $"Read-only OS telemetry: {state}; last poll: {poll}\n" +
            $"CPU load: {load} (GetSystemTimes); battery: {battery}, {ac}, {charging} (GetSystemPowerStatus).\n" +
            $"Battery charge limit: {batteryCare} (read-only HP BIOS setting inventory; numeric limits unavailable).\n" +
            $"Display refresh rate: {refreshRate} (Windows current settings for the internal panel when identifiable).\n" +
            $"GPU temperature: {gpuTemperature} (NVIDIA NVAPI GPU-target sensor; optional installed display driver).\n" +
            "CPU temperature: Unavailable; no verified driver-free package sensor.\n" +
            "Fan 1 / Fan 2 RPM: Unavailable; no verified V1 tachometer source; 0x38 is not enabled.\n" +
            $"{device} ({identitySource}); cached fan levels remain raw-only. Normal fan control: NO-GO.";

        string batteryStatus = current.BatteryPresent == false ? $"No battery | {ac}" :
            current.AcOnline == true && current.Charging.HasValue ? $"{battery} | AC | {charging}" : $"{battery} | {ac}";
        var trayStatus = new HpTrayTelemetryStatus(
            current.CpuLoadPercent is { } cpuLoad ? $"CPU: {cpuLoad}%" : "CPU: Unavailable",
            gpuFresh ? $"GPU: {current.GpuTemperature!.Value.Celsius:0} °C" : "GPU: Unavailable",
            FormatTrayBattery(current),
            current.DisplayRefreshRateHz is { } trayHz ? $"Screen: {trayHz} Hz" : "Screen: Unavailable");
        return new(
            $"Temp: Unavailable | {load}", $"Temp: {gpuTemperature}",
            $"Fan RPM: Unavailable | {device}" + (cachedIdentity ? " (cached)" : ""),
            batteryStatus, $"Battery care: {batteryCare}", $"Screen: {refreshRate}", summary, trayStatus)
        {
            Availability = state,
            CpuLoad = current.CpuLoadPercent is { } summaryCpuLoad ? $"{summaryCpuLoad}%" : "Unavailable",
            GpuTemperature = gpuTemperature,
            BatteryPower = batteryStatus,
            RefreshRate = refreshRate,
            CpuTemperature = "Unavailable",
            FanRpm = "Unavailable",
            BatteryCareStatus = batteryCare,
            BatteryCareCapability = FormatBatteryCareCapability(current.BatteryCare?.Result)
        };
    }

    private static string FormatTrayBattery(HpReadOnlyTelemetrySnapshot current)
    {
        if (current.BatteryPresent == false) return "Battery: Not present";
        if (current.BatteryPercent is not { } percent) return "Battery: Unavailable";
        return current.AcOnline switch
        {
            true => $"Battery: {percent}% · AC",
            false => $"Battery: {percent}% · On battery",
            _ => $"Battery: {percent}%"
        };
    }

    internal static string FormatBatteryCare(HpBatteryCareProbeResult? result) => result switch
    {
        { Availability: HpBatteryCareAvailability.Supported, Enabled: true } => "Enabled; limit values unavailable",
        { Availability: HpBatteryCareAvailability.Supported, Enabled: false } => "Disabled; limit values unavailable",
        { Availability: HpBatteryCareAvailability.Supported } => "Supported, state unavailable",
        { Availability: HpBatteryCareAvailability.NotExposed } => "Not exposed by HP BIOS settings",
        _ => "Unavailable"
    };

    internal static string FormatBatteryCareCapability(HpBatteryCareProbeResult? result) => result switch
    {
        { Availability: HpBatteryCareAvailability.Supported, Enabled: not null } => "Available",
        { Availability: HpBatteryCareAvailability.Supported } => "Supported, state unavailable",
        { Availability: HpBatteryCareAvailability.NotExposed } => "Not supported",
        _ => "Unavailable"
    };

    internal static string FormatFanCount(byte? fanCount) => fanCount switch
    {
        1 => "1 fan",
        2 => "2 fans",
        _ => "Unavailable"
    };

    internal static string FormatThermalPolicy(byte? version) => version == 1 ? "V1" : "Unavailable";
}
