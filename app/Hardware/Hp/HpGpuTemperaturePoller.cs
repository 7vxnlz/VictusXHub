namespace GHelper.Hardware.Hp;

internal readonly record struct HpGpuTemperatureSample(DateTimeOffset SampledAt, double Celsius);

// Called on the UI thread. A stalled native read occupies the sole worker slot, never the UI.
internal sealed class HpGpuTemperaturePoller(Func<double?> read)
{
    private Task<double?>? pending;
    private DateTimeOffset startedAt;
    private DateTimeOffset? lastStartedAt;
    private HpGpuTemperatureSample? sample;
    private bool discardPending;
    internal static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    public void Reset()
    {
        sample = null;
        discardPending = pending is not null;
    }

    public HpGpuTemperatureSample? Poll(DateTimeOffset now)
    {
        if (pending?.IsCompleted == true)
        {
            double? value = pending.GetAwaiter().GetResult();
            sample = !discardPending && value is > 0 and <= 125
                ? new(startedAt, value.Value) : null;
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
                catch (Exception) { return null; } // Optional vendor API failures must not escape to WinForms.
            });
        }

        return sample is { } current && now >= current.SampledAt &&
            now - current.SampledAt <= HpReadOnlyTelemetryProvider.MaximumSampleAge ? current : null;
    }
}
