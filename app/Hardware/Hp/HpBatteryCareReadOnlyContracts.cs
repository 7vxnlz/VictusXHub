namespace GHelper.Hardware.Hp;

internal enum HpBatteryCareAvailability
{
    Unavailable,
    NotExposed,
    Supported
}

internal readonly record struct HpBatteryCareProbeResult(
    HpBatteryCareAvailability Availability,
    bool? Enabled,
    string? SettingName)
{
    public static HpBatteryCareProbeResult Unavailable { get; } = new(HpBatteryCareAvailability.Unavailable, null, null);
    public static HpBatteryCareProbeResult NotExposed { get; } = new(HpBatteryCareAvailability.NotExposed, null, null);

    public static HpBatteryCareProbeResult FromSetting(string settingName, string? currentValue)
    {
        bool? enabled = currentValue?.Trim() switch
        {
            string value when value.Equals("Enable", StringComparison.OrdinalIgnoreCase) ||
                              value.Equals("Enabled", StringComparison.OrdinalIgnoreCase) => true,
            string value when value.Equals("Disable", StringComparison.OrdinalIgnoreCase) ||
                              value.Equals("Disabled", StringComparison.OrdinalIgnoreCase) => false,
            _ => null
        };
        return new(HpBatteryCareAvailability.Supported, enabled, settingName);
    }
}

internal readonly record struct HpBatteryCareSample(DateTimeOffset SampledAt, HpBatteryCareProbeResult Result);

// Called on the UI thread. The inventory read stays in one worker slot and never invokes a method.
internal sealed class HpBatteryCarePoller(Func<HpBatteryCareProbeResult> read)
{
    private Task<HpBatteryCareProbeResult>? pending;
    private DateTimeOffset startedAt;
    private DateTimeOffset? lastStartedAt;
    private HpBatteryCareSample? sample;
    private bool discardPending;
    internal static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(5);
    internal static readonly TimeSpan MaximumSampleAge = TimeSpan.FromMinutes(10);

    public void Reset()
    {
        sample = null;
        lastStartedAt = null;
        discardPending = pending is not null;
    }

    public HpBatteryCareSample? Poll(DateTimeOffset now)
    {
        if (pending?.IsCompleted == true)
        {
            HpBatteryCareProbeResult result = pending.GetAwaiter().GetResult();
            sample = discardPending ? null : new(startedAt, result);
            pending = null;
            discardPending = false;
        }

        if (pending is null && (lastStartedAt is null || now < lastStartedAt || now - lastStartedAt >= PollInterval))
        {
            startedAt = now;
            lastStartedAt = now;
            pending = Task.Run(() =>
            {
                try { return read(); }
                catch (Exception) { return HpBatteryCareProbeResult.Unavailable; }
            });
        }

        return sample is { } current && now >= current.SampledAt && now - current.SampledAt <= MaximumSampleAge
            ? current : null;
    }
}
