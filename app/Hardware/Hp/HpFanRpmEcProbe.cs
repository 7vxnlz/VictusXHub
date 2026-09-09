using Microsoft.Win32.SafeHandles;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace GHelper.Hardware.Hp;

// Diagnostic-only fixed ACPI EC read path. This deliberately exposes no arbitrary EC, port, or PawnIO API.
internal enum HpFanRpmEcProbeAvailability
{
    Available,
    DeviceMismatch,
    BoardMismatch,
    PawnIoNotInstalled,
    PawnIoAccessDenied,
    PawnIoOpenFailed,
    ModuleResourceMissing,
    ModuleHashMismatch,
    ModuleLoadFailed,
    EcMutexUnavailable,
    EcReadTimeout,
    EcReadFailed,
    ImplausibleCandidate
}

internal sealed record HpFanRpmEcProbeDevice(HpRyzenTemperatureProbeDevice RyzenDevice, string BoardProduct);

internal sealed record HpFanRpmEcProbeGateResult(bool IsAccepted, HpFanRpmEcProbeAvailability Availability, string Reason)
{
    internal static HpFanRpmEcProbeGateResult Accept { get; } = new(true, HpFanRpmEcProbeAvailability.Available,
        "Exact Victus 8BD4 diagnostic gate accepted.");
}

internal static class HpFanRpmEcProbeGate
{
    internal const string ExpectedBoardProduct = "8BD4";

    internal static HpFanRpmEcProbeGateResult Evaluate(HpFanRpmEcProbeDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);
        HpRyzenTemperatureProbeGateResult ryzenGate = HpRyzenTemperatureProbeGate.Evaluate(device.RyzenDevice);
        if (!ryzenGate.IsAccepted)
        {
            return new(false, HpFanRpmEcProbeAvailability.DeviceMismatch, ryzenGate.Reason);
        }

        string board = Normalize(device.BoardProduct);
        if (board.Length > 0 && !string.Equals(board, ExpectedBoardProduct, StringComparison.Ordinal))
        {
            return new(false, HpFanRpmEcProbeAvailability.BoardMismatch,
                "Baseboard product did not match exact 8BD4 evidence: " + device.BoardProduct);
        }

        return HpFanRpmEcProbeGateResult.Accept;
    }

    internal static bool MayOpenPawnIo(HpFanRpmEcProbeGateResult gate) => gate.IsAccepted;

    private static string Normalize(string value) => new string(value.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
}

internal sealed record HpFanRpmEcSnapshot(
    HpFanRpmEcProbeAvailability Availability,
    DateTimeOffset? SampledAt,
    byte? Fan1Raw,
    byte? Fan2Raw,
    int? Fan1RpmCandidate,
    int? Fan2RpmCandidate,
    string Detail)
{
    internal const string BackendName = "Direct PawnIO LpcACPIEC fixed read-only transaction";
    internal const string Fan1Name = "Fan 1";
    internal const string Fan2Name = "Fan 2";

    internal bool IsAvailable => Availability == HpFanRpmEcProbeAvailability.Available && SampledAt.HasValue &&
        Fan1Raw.HasValue && Fan2Raw.HasValue && Fan1RpmCandidate.HasValue && Fan2RpmCandidate.HasValue;

    internal static HpFanRpmEcSnapshot Unavailable(HpFanRpmEcProbeAvailability availability, string detail) =>
        new(availability, null, null, null, null, null, detail);
}

internal static class HpFanRpmEcProbeSemantics
{
    internal const int RpmMultiplier = 100;
    internal const int Fan1PlausibleMaximum = 5800;
    internal const int Fan2PlausibleMaximum = 6100;

    // This converts a fixed-board tachometer candidate only. It does not validate the mapping by itself.
    internal static HpFanRpmEcSnapshot FromFixedRawBytes(byte fan1Raw, byte fan2Raw, DateTimeOffset sampledAt)
    {
        int fan1Candidate = fan1Raw * RpmMultiplier;
        int fan2Candidate = fan2Raw * RpmMultiplier;
        if (fan1Candidate > Fan1PlausibleMaximum || fan2Candidate > Fan2PlausibleMaximum)
        {
            return HpFanRpmEcSnapshot.Unavailable(HpFanRpmEcProbeAvailability.ImplausibleCandidate,
                "Fixed 8BD4 EC tachometer candidate exceeded the documented broad board plausibility bound.");
        }

        return new(HpFanRpmEcProbeAvailability.Available, sampledAt, fan1Raw, fan2Raw, fan1Candidate, fan2Candidate,
            "8BD4 fixed-register RPM candidates only; physical semantics validation is required. Raw zero is a stopped-fan candidate, not independently proven RPM.");
    }
}

internal interface IHpFanRpmEcProbeSession : IDisposable
{
    HpFanRpmEcSnapshot ReadFanRpmSnapshot(DateTimeOffset sampledAt);
}

internal sealed class HpFanRpmEcProbeBackend
{
    private const string ModuleResourceName = "GHelper.Hardware.Hp.Resources.PawnIO.LpcACPIEC.bin";
    private const string PawnIoDevicePath = @"\\?\GLOBALROOT\Device\PawnIO";
    private const string EcMutexName = @"Global\Access_EC";
    private const uint GenericReadWrite = 0xC0000000;
    private const uint ShareReadWrite = 0x00000003;
    private const uint OpenExisting = 3;
    private const uint DeviceType = 41394u << 16;
    private const uint LoadModuleIoctl = DeviceType | (0x821u << 2);
    private const uint ExecuteModuleIoctl = DeviceType | (0x841u << 2);
    private const int FunctionNameLength = 32;
    private const int StatusPollLimit = 50;
    private const byte InputBufferFull = 0x02;
    private const byte OutputBufferFull = 0x01;

    // The only permitted protocol-selection writes in this class are these constants.
    internal const byte Fan1Register = 0x11;
    internal const byte Fan2Register = 0x14;
    internal const byte AcpiEcReadCommand = 0x80;
    internal const ushort CommandPort = 0x66;
    internal const ushort DataPort = 0x62;
    internal const string ModuleRelease = "PawnIO.Modules 0.2.2";
    internal const string ModuleSha256 = "C38FD116E7AFF4D1FDB0A494E296BE0A6708E5A22FC72F14587442FB7F8F7906";
    internal const string ModuleArchiveSha256 = "B9E05E52C07FD76B7F9DB3DA0F542B710D55511DC995D4F4CA08E9179B4C4032";

    internal HpFanRpmEcProbeSessionOpenResult Open()
    {
        byte[] module;
        try
        {
            using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ModuleResourceName);
            if (stream is null)
            {
                return HpFanRpmEcProbeSessionOpenResult.Failed(HpFanRpmEcProbeAvailability.ModuleResourceMissing,
                    "Embedded LpcACPIEC module resource was not found.");
            }

            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            module = memory.ToArray();
        }
        catch (Exception ex) when (ex is IOException or NotSupportedException)
        {
            return HpFanRpmEcProbeSessionOpenResult.Failed(HpFanRpmEcProbeAvailability.ModuleResourceMissing, ex.Message);
        }

        if (!string.Equals(Convert.ToHexString(SHA256.HashData(module)), ModuleSha256, StringComparison.Ordinal))
        {
            return HpFanRpmEcProbeSessionOpenResult.Failed(HpFanRpmEcProbeAvailability.ModuleHashMismatch,
                "Embedded LpcACPIEC module SHA-256 did not match approved provenance.");
        }

        SafeFileHandle handle = CreateFile(PawnIoDevicePath, GenericReadWrite, ShareReadWrite, IntPtr.Zero, OpenExisting, 0, IntPtr.Zero);
        if (handle.IsInvalid)
        {
            int error = Marshal.GetLastWin32Error();
            handle.Dispose();
            HpFanRpmEcProbeAvailability availability = error switch
            {
                2 or 3 => HpFanRpmEcProbeAvailability.PawnIoNotInstalled,
                5 => HpFanRpmEcProbeAvailability.PawnIoAccessDenied,
                _ => HpFanRpmEcProbeAvailability.PawnIoOpenFailed
            };
            return HpFanRpmEcProbeSessionOpenResult.Failed(availability,
                error == 5 ? "PawnIO device open failed (Win32 5: elevated Administrator access required by the device ACL)." :
                "PawnIO device open failed (Win32 " + error + ").");
        }

        if (!DeviceIoControl(handle, LoadModuleIoctl, module, (uint)module.Length, null, 0, out _, IntPtr.Zero))
        {
            int error = Marshal.GetLastWin32Error();
            handle.Dispose();
            return HpFanRpmEcProbeSessionOpenResult.Failed(HpFanRpmEcProbeAvailability.ModuleLoadFailed,
                "LpcACPIEC module load failed (Win32 " + error + ").");
        }

        return HpFanRpmEcProbeSessionOpenResult.Opened(new HpFanRpmEcProbeSession(handle));
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFile(string fileName, uint desiredAccess, uint shareMode, IntPtr securityAttributes,
        uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool DeviceIoControl(SafeFileHandle device, uint controlCode,
        [In] byte[] inputBuffer, uint inputBufferSize, [Out] byte[]? outputBuffer, uint outputBufferSize,
        out uint bytesReturned, IntPtr overlapped);

    internal sealed class HpFanRpmEcProbeSession : IHpFanRpmEcProbeSession
    {
        private readonly SafeFileHandle handle;
        private bool disposed;

        internal HpFanRpmEcProbeSession(SafeFileHandle handle) => this.handle = handle;

        public HpFanRpmEcSnapshot ReadFanRpmSnapshot(DateTimeOffset sampledAt)
        {
            if (disposed || handle.IsClosed || handle.IsInvalid)
            {
                return HpFanRpmEcSnapshot.Unavailable(HpFanRpmEcProbeAvailability.PawnIoOpenFailed, "PawnIO EC session is unavailable.");
            }

            Mutex? mutex = null;
            bool acquired = false;
            try
            {
                mutex = OpenEcMutex();
                try { acquired = mutex.WaitOne(TimeSpan.FromMilliseconds(250)); }
                catch (AbandonedMutexException) { acquired = true; }
                if (!acquired)
                {
                    return HpFanRpmEcSnapshot.Unavailable(HpFanRpmEcProbeAvailability.EcMutexUnavailable,
                        "Timed out acquiring Global\\Access_EC for the fixed ACPI EC read transaction.");
                }

                if (!TryReadFan1(out byte fan1, out string detail) || !TryReadFan2(out byte fan2, out detail))
                {
                    return HpFanRpmEcSnapshot.Unavailable(HpFanRpmEcProbeAvailability.EcReadFailed, detail);
                }

                return HpFanRpmEcProbeSemantics.FromFixedRawBytes(fan1, fan2, sampledAt);
            }
            catch (UnauthorizedAccessException ex)
            {
                return HpFanRpmEcSnapshot.Unavailable(HpFanRpmEcProbeAvailability.EcMutexUnavailable, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return HpFanRpmEcSnapshot.Unavailable(HpFanRpmEcProbeAvailability.EcMutexUnavailable, ex.Message);
            }
            finally
            {
                if (acquired) mutex?.ReleaseMutex();
                mutex?.Dispose();
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            handle.Dispose();
        }

        // The three write calls below are fixed ACPI EC READ protocol selection only: 0x80, then 0x11 or 0x14.
        private bool TryReadFan1(out byte value, out string detail) => TryReadFixedRegister(Fan1Register, out value, out detail);
        private bool TryReadFan2(out byte value, out string detail) => TryReadFixedRegister(Fan2Register, out value, out detail);

        private bool TryReadFixedRegister(byte register, out byte value, out string detail)
        {
            value = 0;
            if (!WaitForInputBufferReady(out detail) || !WriteEcReadCommand(out detail) ||
                !WaitForInputBufferReady(out detail) || !WriteFixedAddress(register, out detail) ||
                !WaitForInputBufferReady(out detail) ||
                !WaitForOutputBufferReady(out detail))
            {
                return false;
            }

            return ReadEcData(out value, out detail);
        }

        private bool WaitForInputBufferReady(out string detail)
        {
            byte lastStatus = 0;
            for (int poll = 0; poll < StatusPollLimit; poll++)
            {
                if (!ReadEcStatus(out byte status, out detail)) return false;
                lastStatus = status;
                if ((status & InputBufferFull) == 0) { detail = string.Empty; return true; }
                Thread.Sleep(1);
            }
            detail = "ACPI EC input buffer did not become ready within the bounded read transaction (last status 0x" + lastStatus.ToString("X2") + ").";
            return false;
        }

        private bool WaitForOutputBufferReady(out string detail)
        {
            byte lastStatus = 0;
            for (int poll = 0; poll < StatusPollLimit; poll++)
            {
                if (!ReadEcStatus(out byte status, out detail)) return false;
                lastStatus = status;
                if ((status & OutputBufferFull) != 0) { detail = string.Empty; return true; }
                Thread.Sleep(1);
            }
            detail = "ACPI EC output buffer did not become ready within the bounded read transaction (last status 0x" + lastStatus.ToString("X2") + ").";
            return false;
        }

        private bool WriteEcReadCommand(out string detail) => InvokePioWrite(CommandPort, AcpiEcReadCommand, "ACPI EC READ command", out detail);

        private bool WriteFixedAddress(byte register, out string detail)
        {
            if (register == Fan1Register) return InvokePioWrite(DataPort, Fan1Register, "Fan 1 fixed EC address", out detail);
            if (register == Fan2Register) return InvokePioWrite(DataPort, Fan2Register, "Fan 2 fixed EC address", out detail);
            detail = "Rejected a non-fixed EC register before any port access.";
            return false;
        }

        private bool ReadEcStatus(out byte value, out string detail) => InvokePioRead(CommandPort, out value, "ACPI EC status", out detail);
        private bool ReadEcData(out byte value, out string detail) => InvokePioRead(DataPort, out value, "ACPI EC data", out detail);

        private bool InvokePioRead(ushort fixedPort, out byte value, string operation, out string detail)
        {
            value = 0;
            byte[] input = CreateModuleCall("ioctl_pio_read", (ulong)fixedPort);
            byte[] output = new byte[sizeof(ulong)];
            if (!DeviceIoControl(handle, ExecuteModuleIoctl, input, (uint)input.Length, output, (uint)output.Length, out uint returned, IntPtr.Zero))
            {
                detail = operation + " read failed (Win32 " + Marshal.GetLastWin32Error() + ").";
                return false;
            }
            if (returned < sizeof(ulong))
            {
                detail = operation + " read returned an incomplete value.";
                return false;
            }
            value = (byte)BitConverter.ToUInt64(output);
            detail = string.Empty;
            return true;
        }

        private bool InvokePioWrite(ushort fixedPort, byte fixedValue, string operation, out string detail)
        {
            byte[] input = CreateModuleCall("ioctl_pio_write", (ulong)fixedPort, fixedValue);
            if (!DeviceIoControl(handle, ExecuteModuleIoctl, input, (uint)input.Length, null, 0, out _, IntPtr.Zero))
            {
                detail = operation + " selection write failed (Win32 " + Marshal.GetLastWin32Error() + ").";
                return false;
            }
            detail = string.Empty;
            return true;
        }

        private static byte[] CreateModuleCall(string export, params ulong[] arguments)
        {
            byte[] input = new byte[FunctionNameLength + arguments.Length * sizeof(ulong)];
            Encoding.ASCII.GetBytes(export).CopyTo(input, 0);
            for (int index = 0; index < arguments.Length; index++)
                BitConverter.TryWriteBytes(input.AsSpan(FunctionNameLength + index * sizeof(ulong)), arguments[index]);
            return input;
        }

        private static Mutex OpenEcMutex()
        {
            try { return Mutex.OpenExisting(EcMutexName); }
            catch (WaitHandleCannotBeOpenedException) { return new Mutex(false, EcMutexName); }
        }
    }
}

internal sealed record HpFanRpmEcProbeSessionOpenResult(
    HpFanRpmEcProbeAvailability Availability,
    IHpFanRpmEcProbeSession? Session,
    string Detail)
{
    internal bool IsOpened => Availability == HpFanRpmEcProbeAvailability.Available && Session is not null;
    internal static HpFanRpmEcProbeSessionOpenResult Opened(IHpFanRpmEcProbeSession session) =>
        new(HpFanRpmEcProbeAvailability.Available, session, "PawnIO opened; signed LpcACPIEC module loaded for fixed ACPI EC reads.");
    internal static HpFanRpmEcProbeSessionOpenResult Failed(HpFanRpmEcProbeAvailability availability, string detail) =>
        new(availability, null, detail);
}
