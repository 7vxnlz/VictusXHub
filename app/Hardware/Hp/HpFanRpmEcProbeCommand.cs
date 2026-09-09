using Microsoft.Win32;
using System.Security.Cryptography;

namespace GHelper.Hardware.Hp;

internal static class HpFanRpmEcProbeCommand
{
    internal const string ProbeFlag = "--hp-fan-rpm-ec-readonly-probe";
    private const string HpVictusFlag = "--hp-victus";
    private const int SampleCount = 15;
    private static readonly TimeSpan SampleInterval = TimeSpan.FromSeconds(1);

    internal static bool TryRun(string[] args)
    {
        if (!args.Any(arg => string.Equals(arg, ProbeFlag, StringComparison.OrdinalIgnoreCase))) return false;

        if (args.Length != 2 || args.Count(arg => string.Equals(arg, ProbeFlag, StringComparison.OrdinalIgnoreCase)) != 1 ||
            args.Count(arg => string.Equals(arg, HpVictusFlag, StringComparison.OrdinalIgnoreCase)) != 1)
        {
            Environment.ExitCode = 2;
            WriteLine("8BD4 fan RPM EC probe rejected: require exactly --hp-victus --hp-fan-rpm-ec-readonly-probe.");
            return true;
        }

        HpRyzenTemperatureProbeDevice ryzenDevice = HpRyzenTemperatureProbeCommand.ReadDevice();
        string boardProduct = ReadBoardProduct();
        var device = new HpFanRpmEcProbeDevice(ryzenDevice, boardProduct);
        HpFanRpmEcProbeGateResult gate = HpFanRpmEcProbeGate.Evaluate(device);

        WriteLine("VictusXHub 8BD4 fan RPM EC probe (diagnostic-only)");
        WriteLine("Target gate: " + (gate.IsAccepted ? "accepted" : "rejected") + " — " + gate.Reason);
        WriteLine("Identity source: " + ryzenDevice.Identity.Source + " — " + ryzenDevice.Identity.Detail);
        WriteLine("Identity: Manufacturer=" + ryzenDevice.Identity.Manufacturer + " | Model=" + ryzenDevice.Identity.Model +
                  " | SKU=" + ryzenDevice.Identity.Sku + " | BIOS=" + ryzenDevice.Identity.Bios);
        WriteLine("CPU: " + ryzenDevice.CpuName + " | Family " + FormatHex(ryzenDevice.CpuFamily) +
                  " | Model " + FormatHex(ryzenDevice.CpuModel) + " | Stepping " + FormatHex(ryzenDevice.CpuStepping));
        WriteLine("Baseboard product (read-only registry): " + (string.IsNullOrEmpty(boardProduct) ? "Unavailable" : boardProduct));
        WriteLine("Backend: " + HpFanRpmEcSnapshot.BackendName);
        WriteLine("Module: " + HpFanRpmEcProbeBackend.ModuleRelease + " | SHA-256 " + GetModuleHash());
        WriteLine("Module archive SHA-256: " + HpFanRpmEcProbeBackend.ModuleArchiveSha256);
        WriteLine("Fixed transaction: status/command 0x66; data 0x62; command 0x80; addresses 0x11 and 0x14 only.");
        WriteLine("Safety: no EC WRITE command, EC data-value write, fan command/control, RyzenSMU, or WinRing0 invoked.");
        WriteLine("WMI FanGetLevel 0x2D: not invoked; it remains RAW-only.");

        if (!HpFanRpmEcProbeGate.MayOpenPawnIo(gate))
        {
            Environment.ExitCode = 2;
            return true;
        }

        HpFanRpmEcProbeSessionOpenResult opened = new HpFanRpmEcProbeBackend().Open();
        WriteLine("PawnIO open status: " + opened.Availability + " — " + opened.Detail);
        if (!opened.IsOpened)
        {
            Environment.ExitCode = 1;
            return true;
        }

        HpRyzenTemperatureProbeSessionOpenResult cpuOpened = new HpRyzenTemperatureProbeBackend().Open();
        WriteLine("CPU temperature status: " + cpuOpened.Availability + " — " + cpuOpened.Detail);
        int validSamples = 0;
        using (opened.Session!)
        using (cpuOpened.Session)
        {
            for (int index = 1; index <= SampleCount; index++)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                HpFanRpmEcSnapshot sample = opened.Session.ReadFanRpmSnapshot(now);
                HpRyzenTemperatureProbeResult? cpu = cpuOpened.Session?.Read(now);
                if (sample.IsAvailable)
                {
                    validSamples++;
                    WriteLine($"Sample {index}/{SampleCount} {now:O}: Fan 1 raw 0x{sample.Fan1Raw!.Value:X2}, candidate {sample.Fan1RpmCandidate} RPM | " +
                              $"Fan 2 raw 0x{sample.Fan2Raw!.Value:X2}, candidate {sample.Fan2RpmCandidate} RPM | " +
                              "CPU " + FormatCpu(cpu) + " | GPU Unavailable (not queried by this EC-only probe)");
                }
                else
                {
                    WriteLine($"Sample {index}/{SampleCount} {now:O}: Unavailable ({sample.Availability}) — {sample.Detail} | CPU {FormatCpu(cpu)}");
                }

                if (index < SampleCount) Thread.Sleep(SampleInterval);
            }
        }

        WriteLine("Valid fixed-register samples: " + validSamples + "/" + SampleCount + ".");
        WriteLine("Values are RPM candidates only until physical dynamic behavior validates the exact 8BD4 mapping.");
        Environment.ExitCode = validSamples == SampleCount ? 0 : 1;
        return true;
    }

    private static string ReadBoardProduct()
    {
        try
        {
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS", writable: false);
            return key?.GetValue("BaseBoardProduct")?.ToString()?.Trim() ?? string.Empty;
        }
        catch (UnauthorizedAccessException) { return string.Empty; }
        catch (System.Security.SecurityException) { return string.Empty; }
    }

    private static string GetModuleHash()
    {
        using Stream? stream = typeof(HpFanRpmEcProbeCommand).Assembly.GetManifestResourceStream(
            "GHelper.Hardware.Hp.Resources.PawnIO.LpcACPIEC.bin");
        return stream is null ? "Unavailable" : Convert.ToHexString(SHA256.HashData(stream));
    }

    private static string FormatCpu(HpRyzenTemperatureProbeResult? sample) => sample is { IsAvailable: true }
        ? sample.Celsius!.Value.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + " C"
        : "Unavailable" + (sample is null ? string.Empty : " (" + sample.Availability + ")");

    private static string FormatHex(int? value) => value.HasValue ? "0x" + value.Value.ToString("X") : "Unavailable";
    private static void WriteLine(string message) => Console.WriteLine(message);
}
