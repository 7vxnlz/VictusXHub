using Microsoft.Win32.SafeHandles;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace GHelper.Hardware.Hp;

// This is intentionally not a general PawnIO wrapper. It owns exactly one signed module and
// invokes exactly one read-only export for the AMD package-control temperature register.
internal enum HpRyzenTemperatureProbeAvailability
{
    Available,
    DeviceMismatch,
    UnsupportedCpu,
    PawnIoNotInstalled,
    PawnIoAccessDenied,
    PawnIoOpenFailed,
    ModuleResourceMissing,
    ModuleHashMismatch,
    ModuleLoadFailed,
    PciMutexUnavailable,
    SmnReadFailed,
    InvalidTemperature,
    Stale
}

internal sealed record HpRyzenTemperatureProbeDevice(
    HpRyzenTemperatureProbeIdentity Identity,
    string CpuName,
    int? CpuFamily,
    int? CpuModel,
    int? CpuStepping);

internal sealed record HpRyzenTemperatureProbeGateResult(bool IsAccepted, HpRyzenTemperatureProbeAvailability Availability, string Reason)
{
    public static HpRyzenTemperatureProbeGateResult Accept { get; } = new(true, HpRyzenTemperatureProbeAvailability.Available, "Exact Victus target gate accepted.");
}

internal static class HpRyzenTemperatureProbeGate
{
    internal const string ExpectedSku = "7Z5Z2EA#AB8";
    internal const string ExpectedBios = "F.31";
    internal const string ExpectedCpuName = "Ryzen 5 7640HS";
    internal const int ExpectedCpuFamily = 0x19;
    // AMD documents Phoenix in the Family 19h Models 70h-7Fh group; CPUID leaf 1
    // decodes the observed Ryzen 5 7640HS as model 74h, stepping 1.
    internal const int ExpectedCpuModel = 0x74;
    internal const int ExpectedCpuStepping = 0x1;

    public static HpRyzenTemperatureProbeGateResult Evaluate(HpRyzenTemperatureProbeDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        if (!HpRyzenTemperatureProbeIdentity.IsExactTarget(device.Identity))
        {
            return new(false, HpRyzenTemperatureProbeAvailability.DeviceMismatch,
                "Exact HP Victus identity gate did not match from " + device.Identity.Source + ": " + device.Identity.Detail);
        }

        if (!Contains(device.CpuName, ExpectedCpuName) || device.CpuFamily != ExpectedCpuFamily ||
            device.CpuModel != ExpectedCpuModel || device.CpuStepping != ExpectedCpuStepping)
        {
            return new(false, HpRyzenTemperatureProbeAvailability.UnsupportedCpu,
                "Exact AMD Ryzen 5 7640HS Family 19h Model 74h Stepping 1 CPU gate did not match.");
        }

        return HpRyzenTemperatureProbeGateResult.Accept;
    }

    internal static bool MayOpenPawnIo(HpRyzenTemperatureProbeGateResult gate)
    {
        ArgumentNullException.ThrowIfNull(gate);
        return gate.IsAccepted;
    }

    private static bool Contains(string value, string expected) => value.Contains(expected, StringComparison.OrdinalIgnoreCase);
}

internal sealed record HpRyzenTemperatureProbeResult(
    HpRyzenTemperatureProbeAvailability Availability,
    double? Celsius,
    DateTimeOffset? SampledAt,
    string Detail)
{
    internal const string SourceName = "Core (Tctl/Tdie)";
    internal const string BackendName = "Direct PawnIO AMDFamily17 read-only SMN";
    internal static readonly TimeSpan MaximumSampleAge = TimeSpan.FromSeconds(5);

    public bool IsAvailable => Availability == HpRyzenTemperatureProbeAvailability.Available && Celsius.HasValue && SampledAt.HasValue;

    public bool IsFresh(DateTimeOffset now) => IsAvailable && now >= SampledAt && now - SampledAt <= MaximumSampleAge;

    public static HpRyzenTemperatureProbeResult Unavailable(HpRyzenTemperatureProbeAvailability availability, string detail) =>
        new(availability, null, null, detail);
}

internal static class HpRyzenTemperatureProbeSemantics
{
    private const uint TemperatureRangeSelectMask = 0x00080000;
    private const uint TemperatureTjSelectMask = 0x00030000;
    private const double MaximumPlausibleCelsius = 115;

    public static HpRyzenTemperatureProbeResult FromSmnTemperatureRegister(uint rawValue, DateTimeOffset sampledAt)
    {
        // Current upstream LHM Amd17Cpu semantics for THM_TCON_CUR_TMP: bits 31:21 in 0.125 C units.
        double celsius = ((rawValue >> 21) & 0x7ff) * 0.125;
        if ((rawValue & TemperatureRangeSelectMask) != 0 || (rawValue & TemperatureTjSelectMask) == TemperatureTjSelectMask)
        {
            celsius -= 49;
        }

        return FromExactSensor(HpRyzenTemperatureProbeResult.SourceName, celsius, sampledAt);
    }

    public static HpRyzenTemperatureProbeResult FromExactSensor(string sourceName, double? celsius, DateTimeOffset sampledAt)
    {
        if (!string.Equals(sourceName, HpRyzenTemperatureProbeResult.SourceName, StringComparison.Ordinal))
        {
            return HpRyzenTemperatureProbeResult.Unavailable(HpRyzenTemperatureProbeAvailability.InvalidTemperature,
                "Rejected non-package sensor source: " + sourceName);
        }

        if (!celsius.HasValue || !double.IsFinite(celsius.Value) || celsius.Value <= 0 || celsius.Value > MaximumPlausibleCelsius)
        {
            return HpRyzenTemperatureProbeResult.Unavailable(HpRyzenTemperatureProbeAvailability.InvalidTemperature,
                "Core (Tctl/Tdie) value was null, non-finite, zero, or outside the plausibility range.");
        }

        return new(HpRyzenTemperatureProbeAvailability.Available, celsius.Value, sampledAt,
            "AMD THM_TCON_CUR_TMP decoded with current LibreHardwareMonitor Tctl/Tdie semantics.");
    }
}

internal interface IHpRyzenTemperatureProbeSession : IDisposable
{
    HpRyzenTemperatureProbeResult Read(DateTimeOffset sampledAt);
}

internal sealed class HpRyzenTemperatureProbeBackend
{
    private const string ModuleResourceName = "GHelper.Hardware.Hp.Resources.PawnIO.AMDFamily17.bin";
    internal const string ModuleSha256 = "099DC01D6DB97EA997FEC4A461E191CC64B9D7CE47C9D2153C451C56C2ADCF50";
    private const string PawnIoDevicePath = @"\\?\GLOBALROOT\Device\PawnIO";
    private const string PciMutexName = @"Global\Access_PCI";
    private const uint GenericReadWrite = 0xC0000000;
    private const uint ShareReadWrite = 0x00000003;
    private const uint OpenExisting = 3;
    private const uint DeviceType = 41394u << 16;
    private const uint LoadModuleIoctl = DeviceType | (0x821u << 2);
    private const uint ExecuteModuleIoctl = DeviceType | (0x841u << 2);
    private const int FunctionNameLength = 32;
    private const uint SmnTemperatureAddress = 0x00059800;

    internal const string ModuleRelease = "PawnIO.Modules 0.2.2";
    internal const string ModuleArchiveSha256 = "B9E05E52C07FD76B7F9DB3DA0F542B710D55511DC995D4F4CA08E9179B4C4032";

    public HpRyzenTemperatureProbeSessionOpenResult Open()
    {
        byte[] module;
        try
        {
            using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ModuleResourceName);
            if (stream is null)
            {
                return HpRyzenTemperatureProbeSessionOpenResult.Failed(HpRyzenTemperatureProbeAvailability.ModuleResourceMissing,
                    "Embedded AMDFamily17 module resource was not found.");
            }

            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            module = memory.ToArray();
        }
        catch (Exception ex) when (ex is IOException or NotSupportedException)
        {
            return HpRyzenTemperatureProbeSessionOpenResult.Failed(HpRyzenTemperatureProbeAvailability.ModuleResourceMissing, ex.Message);
        }

        string moduleHash = Convert.ToHexString(SHA256.HashData(module));
        if (!string.Equals(moduleHash, ModuleSha256, StringComparison.Ordinal))
        {
            return HpRyzenTemperatureProbeSessionOpenResult.Failed(HpRyzenTemperatureProbeAvailability.ModuleHashMismatch,
                "Embedded AMDFamily17 module SHA-256 did not match approved provenance.");
        }

        SafeFileHandle handle = CreateFile(PawnIoDevicePath, GenericReadWrite, ShareReadWrite, IntPtr.Zero, OpenExisting, 0, IntPtr.Zero);
        if (handle.IsInvalid)
        {
            int error = Marshal.GetLastWin32Error();
            handle.Dispose();
            HpRyzenTemperatureProbeAvailability availability = error switch
            {
                2 or 3 => HpRyzenTemperatureProbeAvailability.PawnIoNotInstalled,
                5 => HpRyzenTemperatureProbeAvailability.PawnIoAccessDenied,
                _ => HpRyzenTemperatureProbeAvailability.PawnIoOpenFailed
            };
            string detail = error == 5
                ? "PawnIO device open failed (Win32 5: elevated Administrator access required by the PawnIO device ACL)."
                : "PawnIO device open failed (Win32 " + error + ").";
            return HpRyzenTemperatureProbeSessionOpenResult.Failed(availability, detail);
        }

        if (!DeviceIoControl(handle, LoadModuleIoctl, module, (uint)module.Length, null, 0, out _, IntPtr.Zero))
        {
            int error = Marshal.GetLastWin32Error();
            handle.Dispose();
            return HpRyzenTemperatureProbeSessionOpenResult.Failed(HpRyzenTemperatureProbeAvailability.ModuleLoadFailed,
                "AMDFamily17 module load failed (Win32 " + error + ").");
        }

        return HpRyzenTemperatureProbeSessionOpenResult.Opened(new HpRyzenTemperatureProbeSession(handle));
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFile(string fileName, uint desiredAccess, uint shareMode, IntPtr securityAttributes,
        uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool DeviceIoControl(SafeFileHandle device, uint controlCode,
        [In] byte[] inputBuffer, uint inputBufferSize, [Out] byte[]? outputBuffer, uint outputBufferSize,
        out uint bytesReturned, IntPtr overlapped);

    internal sealed class HpRyzenTemperatureProbeSession : IHpRyzenTemperatureProbeSession
    {
        private readonly SafeFileHandle handle;
        private bool disposed;

        internal HpRyzenTemperatureProbeSession(SafeFileHandle handle) => this.handle = handle;

        public HpRyzenTemperatureProbeResult Read(DateTimeOffset sampledAt)
        {
            if (disposed || handle.IsClosed || handle.IsInvalid)
            {
                return HpRyzenTemperatureProbeResult.Unavailable(HpRyzenTemperatureProbeAvailability.PawnIoOpenFailed,
                    "PawnIO session is unavailable.");
            }

            Mutex? mutex = null;
            bool acquired = false;
            try
            {
                mutex = OpenPciMutex();
                try
                {
                    acquired = mutex.WaitOne(TimeSpan.FromMilliseconds(10));
                }
                catch (AbandonedMutexException)
                {
                    acquired = true;
                }

                if (!acquired)
                {
                    return HpRyzenTemperatureProbeResult.Unavailable(HpRyzenTemperatureProbeAvailability.PciMutexUnavailable,
                        "Timed out acquiring Global\\Access_PCI for the audited SMN read.");
                }

                byte[] input = new byte[FunctionNameLength + sizeof(ulong)];
                Encoding.ASCII.GetBytes("ioctl_read_smn").CopyTo(input, 0);
                BitConverter.TryWriteBytes(input.AsSpan(FunctionNameLength), (ulong)SmnTemperatureAddress);
                byte[] output = new byte[sizeof(ulong)];
                if (!DeviceIoControl(handle, ExecuteModuleIoctl, input, (uint)input.Length, output, (uint)output.Length, out uint returned, IntPtr.Zero))
                {
                    return HpRyzenTemperatureProbeResult.Unavailable(HpRyzenTemperatureProbeAvailability.SmnReadFailed,
                        "AMDFamily17 ioctl_read_smn failed (Win32 " + Marshal.GetLastWin32Error() + ").");
                }

                if (returned < sizeof(ulong))
                {
                    return HpRyzenTemperatureProbeResult.Unavailable(HpRyzenTemperatureProbeAvailability.SmnReadFailed,
                        "AMDFamily17 ioctl_read_smn returned an incomplete value.");
                }

                return HpRyzenTemperatureProbeSemantics.FromSmnTemperatureRegister((uint)BitConverter.ToUInt64(output), sampledAt);
            }
            catch (UnauthorizedAccessException ex)
            {
                return HpRyzenTemperatureProbeResult.Unavailable(HpRyzenTemperatureProbeAvailability.PciMutexUnavailable, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return HpRyzenTemperatureProbeResult.Unavailable(HpRyzenTemperatureProbeAvailability.PciMutexUnavailable, ex.Message);
            }
            finally
            {
                if (acquired)
                {
                    mutex?.ReleaseMutex();
                }

                mutex?.Dispose();
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            handle.Dispose();
        }

        private static Mutex OpenPciMutex()
        {
            try
            {
                return Mutex.OpenExisting(PciMutexName);
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                return new Mutex(false, PciMutexName);
            }
        }
    }
}

internal sealed record HpRyzenTemperatureProbeSessionOpenResult(
    HpRyzenTemperatureProbeAvailability Availability,
    IHpRyzenTemperatureProbeSession? Session,
    string Detail)
{
    public bool IsOpened => Availability == HpRyzenTemperatureProbeAvailability.Available && Session is not null;

    public static HpRyzenTemperatureProbeSessionOpenResult Opened(IHpRyzenTemperatureProbeSession session) =>
        new(HpRyzenTemperatureProbeAvailability.Available, session, "PawnIO opened; signed AMDFamily17 read-only module loaded.");

    public static HpRyzenTemperatureProbeSessionOpenResult Failed(HpRyzenTemperatureProbeAvailability availability, string detail) =>
        new(availability, null, detail);
}
