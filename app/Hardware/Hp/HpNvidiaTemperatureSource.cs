using NvAPIWrapper.GPU;
using NvAPIWrapper.Native;
using NvAPIWrapper.Native.GPU;

namespace GHelper.Hardware.Hp;

internal static class HpNvidiaTemperatureSource
{
    public static double? Read()
    {
        if (!OperatingSystem.IsWindows()) return null;
        var devices = PhysicalGPU.GetPhysicalGPUs();
        // Do not guess which device/sensor represents the laptop GPU on multi-GPU systems.
        if (devices.Length != 1) return null;
        var sensors = GPUApi.GetThermalSettings(devices[0].Handle).Sensors;
        if (sensors is null) return null;
        var gpuSensors = sensors.Where(sensor => sensor.Target == ThermalSettingsTarget.GPU).ToArray();
        return gpuSensors.Length == 1 ? gpuSensors[0].CurrentTemperature : null;
    }
}
