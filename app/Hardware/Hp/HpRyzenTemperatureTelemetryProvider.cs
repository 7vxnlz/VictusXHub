namespace GHelper.Hardware.Hp;

// Owns only the exact validated temperature session. No PawnIO handle, module, or address
// escapes this class; callers can receive only the typed Core (Tctl/Tdie) result.
internal sealed class HpRyzenTemperatureTelemetryProvider : IDisposable
{
    private readonly Func<HpRyzenTemperatureProbeDevice> readDevice;
    private readonly Func<HpRyzenTemperatureProbeSessionOpenResult> openSession;
    private readonly object sync = new();
    private IHpRyzenTemperatureProbeSession? session;
    private HpRyzenTemperatureProbeResult? retryFailure;
    private DateTimeOffset? nextOpenAttempt;
    private bool disposed;

    // UI refreshes every second. Five seconds avoids reloading a module every frame while
    // permitting recovery from a transient device/open failure; successful samples are not cached here.
    internal static readonly TimeSpan RetryDelay = HpReadOnlyTelemetryProvider.MaximumSampleAge;

    internal HpRyzenTemperatureTelemetryProvider(
        Func<HpRyzenTemperatureProbeDevice> readDevice,
        Func<HpRyzenTemperatureProbeSessionOpenResult> openSession)
    {
        this.readDevice = readDevice ?? throw new ArgumentNullException(nameof(readDevice));
        this.openSession = openSession ?? throw new ArgumentNullException(nameof(openSession));
    }

    public HpRyzenTemperatureProbeResult Poll(DateTimeOffset now)
    {
        lock (sync)
        {
            if (disposed)
            {
                return HpRyzenTemperatureProbeResult.Unavailable(HpRyzenTemperatureProbeAvailability.PawnIoOpenFailed,
                    "Ryzen temperature telemetry provider is disposed.");
            }

            if (session is null)
            {
                if (nextOpenAttempt is { } retryAt && now < retryAt && retryFailure is not null)
                    return retryFailure;

                HpRyzenTemperatureProbeGateResult gate;
                try
                {
                    gate = HpRyzenTemperatureProbeGate.Evaluate(readDevice());
                }
                catch (Exception ex)
                {
                    return RememberOpenFailure(now, HpRyzenTemperatureProbeAvailability.DeviceMismatch,
                        "Exact-device identity read failed: " + ex.Message);
                }

                if (!gate.IsAccepted)
                    return RememberOpenFailure(now, gate.Availability, gate.Reason);

                HpRyzenTemperatureProbeSessionOpenResult opened;
                try
                {
                    opened = openSession();
                }
                catch (Exception ex)
                {
                    return RememberOpenFailure(now, HpRyzenTemperatureProbeAvailability.PawnIoOpenFailed,
                        "PawnIO temperature session initialization failed: " + ex.Message);
                }

                if (!opened.IsOpened)
                    return RememberOpenFailure(now, opened.Availability, opened.Detail);

                session = opened.Session;
                retryFailure = null;
                nextOpenAttempt = null;
            }

            try
            {
                // The session accepts no caller-selected module, export, or SMN address.
                if (session is not { } activeSession)
                {
                    return HpRyzenTemperatureProbeResult.Unavailable(HpRyzenTemperatureProbeAvailability.PawnIoOpenFailed,
                        "Validated PawnIO temperature session was not retained.");
                }

                return activeSession.Read(now);
            }
            catch (Exception ex)
            {
                return HpRyzenTemperatureProbeResult.Unavailable(HpRyzenTemperatureProbeAvailability.SmnReadFailed,
                    "Validated Ryzen temperature read failed: " + ex.Message);
            }
        }
    }

    public void Reset()
    {
        lock (sync)
        {
            session?.Dispose();
            session = null;
            retryFailure = null;
            nextOpenAttempt = null;
        }
    }

    public void Dispose()
    {
        lock (sync)
        {
            if (disposed) return;
            disposed = true;
            session?.Dispose();
            session = null;
            retryFailure = null;
            nextOpenAttempt = null;
        }
    }

    private HpRyzenTemperatureProbeResult RememberOpenFailure(
        DateTimeOffset now, HpRyzenTemperatureProbeAvailability availability, string detail)
    {
        retryFailure = HpRyzenTemperatureProbeResult.Unavailable(availability, detail);
        nextOpenAttempt = now + RetryDelay;
        return retryFailure;
    }
}
