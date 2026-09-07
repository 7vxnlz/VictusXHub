using System.Runtime.InteropServices;

namespace GHelper.Hardware.Hp;

internal sealed class HpWindowsTelemetrySource : IHpReadOnlyTelemetrySource
{
    public HpCpuTimes? ReadCpuTimes()
    {
        if (!OperatingSystem.IsWindows()) return null;
        return GetSystemTimes(out ulong idle, out ulong kernel, out ulong user)
            ? new HpCpuTimes(idle, kernel, user) : null;
    }

    public HpPowerStatus? ReadPowerStatus()
    {
        if (!OperatingSystem.IsWindows()) return null;
        return GetSystemPowerStatus(out SystemPowerStatus status)
            ? new HpPowerStatus(status.AcLineStatus, status.BatteryFlag, status.BatteryLifePercent) : null;
    }

    [DllImport("kernel32.dll", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetSystemTimes(out ulong idle, out ulong kernel, out ulong user);

    [DllImport("kernel32.dll", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetSystemPowerStatus(out SystemPowerStatus status);

    [StructLayout(LayoutKind.Sequential)]
    private struct SystemPowerStatus
    {
        public byte AcLineStatus;
        public byte BatteryFlag;
        public byte BatteryLifePercent;
        public byte SystemStatusFlag;
        public uint BatteryLifeTime;
        public uint BatteryFullLifeTime;
    }
}
