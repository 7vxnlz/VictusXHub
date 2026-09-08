using Microsoft.Win32;
using System.Management;
using System.Runtime.Intrinsics.X86;

namespace GHelper.Hardware.Hp;

internal static class HpRyzenTemperatureProbeCommand
{
    internal const string ProbeFlag = "--hp-ryzen-temperature-probe";
    private const string HpVictusFlag = "--hp-victus";
    private const int SampleCount = 10;
    private static readonly TimeSpan SampleInterval = TimeSpan.FromMilliseconds(750);

    public static bool TryRun(string[] args)
    {
        if (!args.Any(arg => string.Equals(arg, ProbeFlag, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (args.Length != 2 || args.Count(arg => string.Equals(arg, ProbeFlag, StringComparison.OrdinalIgnoreCase)) != 1 ||
            args.Count(arg => string.Equals(arg, HpVictusFlag, StringComparison.OrdinalIgnoreCase)) != 1)
        {
            Environment.ExitCode = 2;
            WriteLine("Ryzen temperature probe rejected: require exactly --hp-victus --hp-ryzen-temperature-probe.");
            return true;
        }

        HpRyzenTemperatureProbeDevice device = ReadDevice();
        HpRyzenTemperatureProbeGateResult gate = HpRyzenTemperatureProbeGate.Evaluate(device);
        WriteLine("VictusX Ryzen temperature probe (diagnostic-only)");
        WriteLine("Target gate: " + (gate.IsAccepted ? "accepted" : "rejected") + " — " + gate.Reason);
        WriteLine("Identity source: " + device.Identity.Source + " — " + device.Identity.Detail);
        WriteLine("Identity raw: Manufacturer=" + device.Identity.Manufacturer + " | Model=" + device.Identity.Model +
                  " | SKU=" + device.Identity.Sku + " | BIOS=" + device.Identity.Bios);
        WriteLine("Identity normalized: Manufacturer=" + device.Identity.NormalizedManufacturer + " | Model=" + device.Identity.NormalizedModel +
                  " | SKU=" + device.Identity.NormalizedSku + " | BIOS=" + device.Identity.NormalizedBios);
        WriteLine("CPU: " + device.CpuName + " | Family " + FormatHex(device.CpuFamily) + " | Model " + FormatHex(device.CpuModel) + " | Stepping " + FormatHex(device.CpuStepping));
        WriteLine("Backend: " + HpRyzenTemperatureProbeResult.BackendName);
        WriteLine("LibreHardwareMonitor: not initialized; direct official AMDFamily17 semantics are used to prevent RyzenSMU initialization.");
        WriteLine("Module: " + HpRyzenTemperatureProbeBackend.ModuleRelease + " | SHA-256 " + GetModuleHash());
        WriteLine("Module archive SHA-256: " + HpRyzenTemperatureProbeBackend.ModuleArchiveSha256);
        WriteLine("Sensor: " + HpRyzenTemperatureProbeResult.SourceName + " | SMN THM_TCON_CUR_TMP semantics.");
        WriteLine("Safety: RyzenSMU PM-table path initialized: False. Fan/EC/control command invoked: False.");

        if (!HpRyzenTemperatureProbeGate.MayOpenPawnIo(gate))
        {
            Environment.ExitCode = 2;
            return true;
        }

        HpRyzenTemperatureProbeSessionOpenResult opened = new HpRyzenTemperatureProbeBackend().Open();
        WriteLine("PawnIO open status: " + opened.Availability + " — " + opened.Detail);
        if (!opened.IsOpened)
        {
            Environment.ExitCode = 1;
            return true;
        }

        int validSamples = 0;
        using (opened.Session!)
        {
            for (int index = 1; index <= SampleCount; index++)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                HpRyzenTemperatureProbeResult sample = opened.Session.Read(now);
                if (sample.IsAvailable && sample.IsFresh(now))
                {
                    validSamples++;
                    WriteLine($"Sample {index}/{SampleCount} {now:O}: {sample.Celsius!.Value:0.000} C ({HpRyzenTemperatureProbeResult.SourceName})");
                }
                else
                {
                    WriteLine($"Sample {index}/{SampleCount} {now:O}: Unavailable ({sample.Availability}) — {sample.Detail}");
                }

                if (index < SampleCount)
                {
                    Thread.Sleep(SampleInterval);
                }
            }
        }

        WriteLine("Valid fresh samples: " + validSamples + "/" + SampleCount + ".");
        WriteLine("This command is evidence collection only. It does not enable the HP Diagnostic CPU-temperature UI.");
        Environment.ExitCode = validSamples == SampleCount ? 0 : 1;
        return true;
    }

    private static HpRyzenTemperatureProbeDevice ReadDevice()
    {
        HpRyzenTemperatureProbeIdentity wmiIdentity;
        try
        {
            using var computerSystem = new ManagementObjectSearcher("root\\cimv2", "SELECT Manufacturer, Model, SystemSKUNumber FROM Win32_ComputerSystem");
            using ManagementObjectCollection systems = computerSystem.Get();
            ManagementObject? system = systems.Cast<ManagementObject>().FirstOrDefault();
            string manufacturer = system?["Manufacturer"]?.ToString()?.Trim() ?? string.Empty;
            string model = system?["Model"]?.ToString()?.Trim() ?? string.Empty;
            string sku = system?["SystemSKUNumber"]?.ToString()?.Trim() ?? string.Empty;

            using var biosSearcher = new ManagementObjectSearcher("root\\cimv2", "SELECT SMBIOSBIOSVersion FROM Win32_BIOS");
            using ManagementObjectCollection biosRows = biosSearcher.Get();
            string bios = biosRows.Cast<ManagementObject>().FirstOrDefault()?["SMBIOSBIOSVersion"]?.ToString()?.Trim() ?? string.Empty;
            wmiIdentity = new(HpRyzenTemperatureProbeIdentitySource.Wmi, manufacturer, model, sku, bios, "WMI Win32_ComputerSystem/Win32_BIOS read.");
        }
        catch (Exception ex) when (ex is ManagementException or UnauthorizedAccessException or System.Runtime.InteropServices.COMException)
        {
            wmiIdentity = HpRyzenTemperatureProbeIdentity.Unavailable("WMI identity unavailable: " + ex.Message);
        }

        (string cpuName, int? family, int? cpuModel, int? stepping) = ReadCpuIdentity();
        HpRyzenTemperatureProbeIdentity identity = HpRyzenTemperatureProbeIdentitySelection.Select(
            wmiIdentity, HpRyzenTemperatureProbeSmbios.Read(), HpRyzenTemperatureProbeRegistryIdentity.Read());
        return new(identity, cpuName, family, cpuModel, stepping);
    }

    private static (string Name, int? Family, int? Model, int? Stepping) ReadCpuIdentity()
    {
        string name = string.Empty;
        try
        {
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            name = key?.GetValue("ProcessorNameString")?.ToString()?.Trim() ?? string.Empty;
        }
        catch (UnauthorizedAccessException)
        {
            // The CPU name is an additional exact-device gate and must not be guessed.
        }

        if (!X86Base.IsSupported)
        {
            return (name, null, null, null);
        }

        (int eax, _, _, _) = X86Base.CpuId(1, 0);
        int stepping = eax & 0xF;
        int baseModel = (eax >> 4) & 0xF;
        int baseFamily = (eax >> 8) & 0xF;
        int extendedModel = (eax >> 16) & 0xF;
        int extendedFamily = (eax >> 20) & 0xFF;
        int family = baseFamily == 0xF ? baseFamily + extendedFamily : baseFamily;
        int model = (baseFamily == 0x6 || baseFamily == 0xF) ? baseModel | (extendedModel << 4) : baseModel;
        return (name, family, model, stepping);
    }

    private static string GetModuleHash()
    {
        return "SHA-256 " + HpRyzenTemperatureProbeBackend.ModuleSha256;
    }

    private static string FormatHex(int? value) => value.HasValue ? "0x" + value.Value.ToString("X") : "Unavailable";

    private static void WriteLine(string value)
    {
        try { Console.WriteLine(value); }
        catch (IOException) { }
    }
}
