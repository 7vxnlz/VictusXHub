using System.Management;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GHelper.Hardware.Hp;

public sealed class HpFanMaxExperimentReadOnlyProvider : IHpFanMaxExperimentReadOnlyProvider
{
    public HpFanMaxExperimentBaseline CaptureBaseline()
    {
        HpVictusCapabilitySnapshot snapshot = HpVictusCapabilityProbe.Probe();
        bool systemDesignDataSucceeded = snapshot.SystemDesignDataInvocationSucceeded && snapshot.SystemDesignDataDecodeSucceeded;
        bool fanGetCountSucceeded = snapshot.FanGetCountInvocationSucceeded && snapshot.FanGetCountDecodeSucceeded;
        bool fanMaxGetSucceeded = snapshot.FanMaxGetInvocationSucceeded && snapshot.FanMaxGetDecodeSucceeded;
        bool fanGetLevelSucceeded = snapshot.FanGetLevelInvocationSucceeded && snapshot.FanGetLevelDecodeSucceeded;

        return new HpFanMaxExperimentBaseline(
            snapshot.IsHpVictus,
            snapshot.Model,
            snapshot.SystemSku,
            snapshot.BiosVersion,
            systemDesignDataSucceeded ? snapshot.SystemDesignDataDecoded?.ThermalPolicyVersion : null,
            fanGetCountSucceeded ? snapshot.FanGetCountDecoded?.FanCount : null,
            fanMaxGetSucceeded ? snapshot.FanMaxGetDecoded?.IsMaxFanEnabled : null,
            fanGetLevelSucceeded ? snapshot.FanGetLevelDecoded?.KnownPrefixHex : null,
            systemDesignDataSucceeded && fanGetCountSucceeded && fanMaxGetSucceeded && fanGetLevelSucceeded,
            [
                Summary("SystemDesignData", snapshot.SystemDesignDataInvocationAttempted, systemDesignDataSucceeded),
                Summary("FanGetCount", snapshot.FanGetCountInvocationAttempted, fanGetCountSucceeded),
                Summary("FanMaxGet", snapshot.FanMaxGetInvocationAttempted, fanMaxGetSucceeded),
                Summary("FanGetLevel", snapshot.FanGetLevelInvocationAttempted, fanGetLevelSucceeded)
            ]);
    }

    public HpFanMaxExperimentFanReadback ReadFanStatus()
    {
        // Probe reuses only the approved read-only commands and produces the required FanMaxGet/FanGetLevel readback.
        HpVictusCapabilitySnapshot snapshot = HpVictusCapabilityProbe.Probe();
        bool fanMaxGetSucceeded = snapshot.FanMaxGetInvocationSucceeded && snapshot.FanMaxGetDecodeSucceeded;
        bool fanGetLevelSucceeded = snapshot.FanGetLevelInvocationSucceeded && snapshot.FanGetLevelDecodeSucceeded;
        string? error = fanMaxGetSucceeded && fanGetLevelSucceeded
            ? null
            : string.Join(" | ", new[] { snapshot.FanMaxGetInvocationError, snapshot.FanGetLevelInvocationError }.Where(static value => !string.IsNullOrWhiteSpace(value)));

        return new HpFanMaxExperimentFanReadback(
            fanMaxGetSucceeded && fanGetLevelSucceeded,
            fanMaxGetSucceeded ? snapshot.FanMaxGetDecoded?.IsMaxFanEnabled : null,
            fanGetLevelSucceeded ? snapshot.FanGetLevelDecoded?.KnownPrefixHex : null,
            error);
    }

    private static string Summary(string name, bool attempted, bool succeeded) =>
        name + ": attempted=" + attempted + "; succeeded=" + succeeded + "; decodeSucceeded=" + succeeded;
}

public sealed class HpFanMaxExperimentWmiTransport : IHpFanMaxExperimentWriteTransport
{
    private const string ScopePath = @"\\.\root\wmi";
    private const string MethodClassName = "hpqBIntM";
    private const string DataClassName = "hpqBDataIn";
    private const string MethodInstanceName = @"ACPI\PNP0C14\0_0";
    private const string MethodName = "hpqBIOSInt0";
    private static readonly byte[] Sign = [0x53, 0x45, 0x43, 0x55];

    public HpFanMaxExperimentWriteResult TrySetFanMax(byte[] payload)
    {
        if (!IsExactSetFanMaxPayload(payload))
        {
            return HpFanMaxExperimentWriteResult.NotAttempted("Rejected: only the selected exact one-byte or four-byte SetFanMax payload is permitted.");
        }

        try
        {
            var scope = new ManagementScope(ScopePath);
            scope.Connect();
            using ManagementObject? biosInstance = FindBiosMethodInstance(scope);
            if (biosInstance is null)
            {
                return new HpFanMaxExperimentWriteResult(false, false, "hpqBIntM BIOS method instance was not found.");
            }

            using var dataClass = new ManagementClass(scope, new ManagementPath(DataClassName), null);
            using ManagementObject? inputData = dataClass.CreateInstance();
            if (inputData is null)
            {
                return new HpFanMaxExperimentWriteResult(false, false, "hpqBDataIn input object could not be created.");
            }

            inputData["Sign"] = Sign;
            inputData["Command"] = 0x20008u;
            inputData["CommandType"] = 0x27u;
            inputData["Size"] = (uint)payload.Length;
            inputData["hpqBData"] = payload.ToArray();

            using ManagementBaseObject inParams = biosInstance.GetMethodParameters(MethodName);
            inParams["InData"] = inputData;
            using ManagementBaseObject? outParams = biosInstance.InvokeMethod(MethodName, inParams, null);
            if (outParams is null)
            {
                return new HpFanMaxExperimentWriteResult(true, false, "SetFanMax returned no output parameters.");
            }

            using var outData = outParams["OutData"] as ManagementBaseObject;
            int? returnCode = TryReadReturnCode(outData);
            return returnCode is null or 0
                ? new HpFanMaxExperimentWriteResult(true, true, null)
                : new HpFanMaxExperimentWriteResult(true, false, "SetFanMax returned code " + returnCode.Value + ".");
        }
        catch (Exception ex)
        {
            return new HpFanMaxExperimentWriteResult(true, false, ex.GetType().Name + ": " + ex.Message);
        }
    }

    private static bool IsExactSetFanMaxPayload(byte[]? payload) =>
        payload is not null &&
        (payload.SequenceEqual(new byte[] { 0x01 }) || payload.SequenceEqual(new byte[] { 0x00 }) ||
         payload.SequenceEqual(new byte[] { 0x01, 0x00, 0x00, 0x00 }) || payload.SequenceEqual(new byte[] { 0x00, 0x00, 0x00, 0x00 }));

    private static ManagementObject? FindBiosMethodInstance(ManagementScope scope)
    {
        using var methodClass = new ManagementClass(scope, new ManagementPath(MethodClassName), null);
        using var instances = methodClass.GetInstances();
        foreach (ManagementObject instance in instances)
        {
            if ((instance["InstanceName"]?.ToString() ?? string.Empty).Contains(MethodInstanceName, StringComparison.OrdinalIgnoreCase))
            {
                return instance;
            }

            instance.Dispose();
        }

        return null;
    }

    private static int? TryReadReturnCode(ManagementBaseObject? outData)
    {
        try { return outData is null ? null : Convert.ToInt32(outData["rwReturnCode"]); }
        catch { return null; }
    }
}

public sealed class HpFanMaxExperimentAcPowerProvider
{
    public bool IsAcPowerOnline()
    {
        try { return SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online; }
        catch { return false; }
    }
}

public sealed class HpFanMaxExperimentDelay : IHpFanMaxExperimentDelay
{
    public void WaitAfterEnable(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero || duration > TimeSpan.FromSeconds(HpFanMaxHoldCommand.MaximumHoldSeconds))
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "SetFanMax wait duration must be positive and bounded.");
        }

        Thread.Sleep(duration);
    }
}
