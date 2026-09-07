using System.ComponentModel;

namespace GHelper.Hardware.Hp;

internal readonly record struct HpCpuTimes(ulong Idle, ulong Kernel, ulong User);
internal readonly record struct HpPowerStatus(byte AcLineStatus, byte BatteryFlag, byte BatteryLifePercent);

internal interface IHpReadOnlyTelemetrySource
{
    HpCpuTimes? ReadCpuTimes();
    HpPowerStatus? ReadPowerStatus();
}

internal sealed record HpReadOnlyTelemetrySnapshot(
    DateTimeOffset? PolledAt,
    int? CpuLoadPercent,
    int? BatteryPercent,
    bool? BatteryPresent,
    bool? AcOnline,
    bool? Charging,
    int? DisplayRefreshRateHz = null)
{
    // CPU temperature and tachometer discovery remain unsupported on the V1 target.
    public double? CpuTemperatureCelsius => null;
    public HpGpuTemperatureSample? GpuTemperature { get; init; }
    public double? GpuTemperatureCelsius => GpuTemperature?.Celsius;
    public int? FanRpm => null;
    public HpBatteryCareSample? BatteryCare { get; init; }

    public static HpReadOnlyTelemetrySnapshot Unavailable { get; } = new(null, null, null, null, null, null);
}

internal sealed class HpReadOnlyTelemetryProvider(
    IHpReadOnlyTelemetrySource source,
    HpGpuTemperaturePoller? gpu = null,
    Func<int?>? displayRefreshRateReader = null,
    HpBatteryCarePoller? batteryCare = null)
{
    private HpCpuTimes? previousCpu;
    private DateTimeOffset? previousCpuTime;
    internal static readonly TimeSpan MaximumSampleAge = TimeSpan.FromSeconds(5);

    public void Reset()
    {
        previousCpu = null;
        previousCpuTime = null;
        gpu?.Reset();
        batteryCare?.Reset();
    }

    public HpReadOnlyTelemetrySnapshot Capture(DateTimeOffset now)
    {
        HpCpuTimes? cpu = ReadSafely(source.ReadCpuTimes);
        int? load = CalculateLoad(cpu, now);
        HpPowerStatus? power = ReadSafely(source.ReadPowerStatus);
        bool? ac = power?.AcLineStatus switch { 0 => false, 1 => true, _ => null };
        bool? present = null;
        bool? charging = null;
        int? percent = null;
        if (power is { } value)
        {
            if (value.BatteryFlag == 128)
                present = false;
            else if (value.BatteryFlag <= 15)
            {
                present = true;
                percent = value.BatteryLifePercent <= 100 ? value.BatteryLifePercent : null;
                charging = (value.BatteryFlag & 8) != 0;
                if (ac == false && charging == true) charging = null;
            }
        }

        int? displayRefreshRate = displayRefreshRateReader is null ? null : ReadSafely(displayRefreshRateReader);
        displayRefreshRate = displayRefreshRate is > 0 and <= 1000 ? displayRefreshRate : null;

        return new(now, load, percent, present, ac, charging, displayRefreshRate)
        {
            GpuTemperature = gpu?.Poll(now),
            BatteryCare = batteryCare?.Poll(now)
        };
    }

    private int? CalculateLoad(HpCpuTimes? current, DateTimeOffset now)
    {
        HpCpuTimes? previous = previousCpu;
        DateTimeOffset? previousTime = previousCpuTime;
        previousCpu = current;
        previousCpuTime = current.HasValue ? now : null;

        if (current is not { } next || previous is not { } before || previousTime is not { } sampledAt ||
            now <= sampledAt || now - sampledAt > MaximumSampleAge ||
            next.Idle < before.Idle || next.Kernel < before.Kernel || next.User < before.User)
            return null;

        // Kernel time includes idle time. Reject inconsistent counters instead of fabricating load.
        double total = (double)(next.Kernel - before.Kernel) + (next.User - before.User);
        double idle = next.Idle - before.Idle;
        if (total <= 0 || idle > total) return null;
        return (int)Math.Round(100 * (1 - idle / total));
    }

    private static T? ReadSafely<T>(Func<T?> read) where T : struct
    {
        try { return read(); }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or
            DllNotFoundException or EntryPointNotFoundException or BadImageFormatException or
            PlatformNotSupportedException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}
