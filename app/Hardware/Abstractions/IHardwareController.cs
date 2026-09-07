using GHelper.USB;
using System.Drawing;
using System.Management;

public interface IEmbeddedController
{
    bool IsConnected();
    void Close();
    byte[] DeviceInit();
    byte[] DeviceWatchDog();
    int DeviceGet(uint deviceId);
    byte[] DeviceGetBuffer(uint deviceId, uint status = 0);
    int DeviceSet(uint deviceId, int status, string? logName);
    int DeviceSet(uint deviceId, byte[] parameters, string? logName);
    bool IsSupported(uint deviceId);
    void SubscribeToEvents(Action<object, EventArrivedEventArgs> eventHandler);
}

public interface IDeviceController : IEmbeddedController
{
}

public interface IFanController
{
    int GetFan(AsusFan device);
    int SetFanRange(AsusFan device, byte[] curve);
    int SetFanCurve(AsusFan device, byte[] curve);
    byte[] GetFanCurve(AsusFan device, int mode = 0);
    bool IsMidFanSupported();
    (int up, int down) GetFanHysteresis();
    int SetFanHysteresis(int up, int down);
}

public interface IPerformanceModeController
{
    int SetPerformanceMode(int mode, string log = "Mode");
    int SetVivoMode(int mode);
    (int eCores, int pCores) GetCores(uint device = AsusACPI.CORES_CPU);
    void SetCores(int eCores, int pCores);
}

public interface IKeyboardController
{
    void TUFKeyboardBrightness(int brightness, string log = "TUF Backlight");
    void TUFKeyboardRGB(AuraMode mode, Color color, int speed, string? log = "TUF RGB");
    void TUFKeyboardPower(bool awake = true, bool boot = false, bool sleep = false, bool shutdown = false);
}

public interface IBatteryController
{
    decimal? GetBatteryDischarge();
}

public interface IDisplayController
{
    bool IsOverdriveSupported();
}

public interface IGpuModeController
{
    int SetGPUEco(int eco);
    bool IsXGConnected();
    bool IsAllAmdPPT();
    bool IsNVidiaGPU();
    int[] GetVramOptions(out int unitMb);
    int GetVramMem();
    void SetVramMem(int value);
    int GetAPUMem();
    void SetAPUMem(int memory = 4);
}

public interface IHardwareController :
    IDeviceController,
    IFanController,
    IPerformanceModeController,
    IKeyboardController,
    IBatteryController,
    IDisplayController,
    IGpuModeController
{
    string ScanRange();
}
