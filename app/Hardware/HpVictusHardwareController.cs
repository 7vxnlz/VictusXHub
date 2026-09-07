using GHelper.USB;
using System.Drawing;
using System.Management;

public sealed class HpVictusHardwareController : IHardwareController
{
    private static readonly byte[] EmptyResponse = new byte[16];

    public bool IsConnected() => false;

    public void Close()
    {
    }

    public byte[] DeviceInit() => EmptyResponse.ToArray();

    public byte[] DeviceWatchDog() => EmptyResponse.ToArray();

    public int DeviceGet(uint deviceId) => -1;

    public byte[] DeviceGetBuffer(uint deviceId, uint status = 0) => EmptyResponse.ToArray();

    public int DeviceSet(uint deviceId, int status, string? logName) => -1;

    public int DeviceSet(uint deviceId, byte[] parameters, string? logName) => -1;

    public bool IsSupported(uint deviceId) => false;

    public void SubscribeToEvents(Action<object, EventArrivedEventArgs> eventHandler)
    {
    }

    public int GetFan(AsusFan device) => -1;

    public int SetFanRange(AsusFan device, byte[] curve) => -1;

    public int SetFanCurve(AsusFan device, byte[] curve) => -1;

    public byte[] GetFanCurve(AsusFan device, int mode = 0) => EmptyResponse.ToArray();

    public bool IsMidFanSupported() => false;

    public (int up, int down) GetFanHysteresis() => (-1, -1);

    public int SetFanHysteresis(int up, int down) => -1;

    public int SetPerformanceMode(int mode, string log = "Mode") => -1;

    public int SetVivoMode(int mode) => -1;

    public (int eCores, int pCores) GetCores(uint device = AsusACPI.CORES_CPU) => (-1, -1);

    public void SetCores(int eCores, int pCores)
    {
    }

    public void TUFKeyboardBrightness(int brightness, string log = "TUF Backlight")
    {
    }

    public void TUFKeyboardRGB(AuraMode mode, Color color, int speed, string? log = "TUF RGB")
    {
    }

    public void TUFKeyboardPower(bool awake = true, bool boot = false, bool sleep = false, bool shutdown = false)
    {
    }

    public decimal? GetBatteryDischarge() => null;

    public bool IsOverdriveSupported() => false;

    public int SetGPUEco(int eco) => -1;

    public bool IsXGConnected() => false;

    public bool IsAllAmdPPT() => false;

    public bool IsNVidiaGPU() => false;

    public int[] GetVramOptions(out int unitMb)
    {
        unitMb = 0;
        return [];
    }

    public int GetVramMem() => -1;

    public void SetVramMem(int value)
    {
    }

    public int GetAPUMem() => -1;

    public void SetAPUMem(int memory = 4)
    {
    }

    public string ScanRange() => string.Empty;
}
