using System.Management;
using System.Runtime.InteropServices;

namespace GHelper.Hardware.Hp;

internal sealed record HpGpuModeReadOnlyProbeCommandRequest(bool ShouldExit, bool IsValidRequest, string[] ValidationErrors);

internal static class HpGpuModeReadOnlyProbeCommand
{
    internal const string ProbeFlag = "--hp-gpu-mode-readonly-probe";
    private const string HpVictusFlag = "--hp-victus";

    internal static bool TryRun(string[] args)
    {
        HpGpuModeReadOnlyProbeCommandRequest request = Parse(args);
        if (!request.ShouldExit)
        {
            return false;
        }

        if (!request.IsValidRequest)
        {
            Environment.ExitCode = 2;
            WriteLines(request.ValidationErrors);
            return true;
        }

        HpGpuModeReadOnlyProbeDevice device = ReadDevice();
        HpGpuModeReadOnlyProbeGateResult gate = HpGpuModeReadOnlyProbeGate.Evaluate(device);
        WriteLine("VictusXHub GPU BIOS-selection read-only probe");
        WriteLine("Target gate: " + (gate.IsAccepted ? "accepted" : "rejected") + " — " + gate.Reason);
        if (!gate.IsAccepted)
        {
            WriteEvidenceStatus(HpGpuModeReadOnlyProbeEvidenceWriter.Write(
                new HpGpuModeReadOnlyProbeEvidence(DateTimeOffset.UtcNow, gate, null)));
            Environment.ExitCode = 2;
            return true;
        }

        HpBiosWmiCommandDefinition definition = HpBiosWmiCommandCatalog.Definitions.Single(candidate =>
            string.Equals(candidate.Name, "GpuBiosSelection", StringComparison.Ordinal));
        var invocationClient = new HpWmiInvocationClient();
        HpWmiInvocationResult invocation = invocationClient.TryInvoke(
            new HpWmiInvocationRequest(
                definition,
                HpVictusModeEnabled: true,
                HpWmiReadOnlyTestModeEnabled: true,
                ProcessElevated: global::GHelper.Helpers.ProcessHelper.IsUserAdministrator()),
            new HpWmiReadOnlyClient().Probe());
        HpGpuModeReadOnlyProbeResult result = HpGpuModeReadOnlyProbeResult.FromInvocation(invocation);
        WriteEvidenceStatus(HpGpuModeReadOnlyProbeEvidenceWriter.Write(
            new HpGpuModeReadOnlyProbeEvidence(DateTimeOffset.UtcNow, gate, result)));
        WriteLines(HpGpuModeReadOnlyProbeFormatter.Format(result));
        Environment.ExitCode = result.IsCaptured ? 0 : 1;
        return true;
    }

    internal static HpGpuModeReadOnlyProbeCommandRequest Parse(string[] args)
    {
        bool hasProbeFlag = args.Any(argument => string.Equals(argument, ProbeFlag, StringComparison.OrdinalIgnoreCase));
        if (!hasProbeFlag)
        {
            return new(false, false, []);
        }

        bool hasExactlyOneProbeFlag = args.Count(argument => string.Equals(argument, ProbeFlag, StringComparison.OrdinalIgnoreCase)) == 1;
        bool hasExactlyOneHpVictusFlag = args.Count(argument => string.Equals(argument, HpVictusFlag, StringComparison.OrdinalIgnoreCase)) == 1;
        if (hasExactlyOneProbeFlag && hasExactlyOneHpVictusFlag && args.Length == 2)
        {
            return new(true, true, []);
        }

        return new(true, false,
            ["GPU BIOS-selection probe rejected: require exactly --hp-victus --hp-gpu-mode-readonly-probe."]);
    }

    internal static HpGpuModeReadOnlyProbeDevice ReadDevice() =>
        new(HpRyzenTemperatureProbeCommand.ReadDevice(), ReadBoardProduct());

    private static string ReadBoardProduct()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("root\\cimv2", "SELECT Product FROM Win32_BaseBoard");
            using ManagementObjectCollection rows = searcher.Get();
            return rows.Cast<ManagementObject>().FirstOrDefault()?["Product"]?.ToString()?.Trim() ?? string.Empty;
        }
        catch (Exception ex) when (ex is ManagementException or UnauthorizedAccessException or COMException)
        {
            return string.Empty;
        }
    }

    private static void WriteLines(IEnumerable<string> lines)
    {
        foreach (string line in lines)
        {
            WriteLine(line);
        }
    }

    private static void WriteEvidenceStatus(HpGpuModeReadOnlyProbeEvidenceWriteResult evidence)
    {
        WriteLine(evidence.IsPersisted
            ? "Evidence file: " + evidence.FilePath
            : "Evidence file unavailable: " + evidence.Error);
    }

    private static void WriteLine(string line)
    {
        try { Console.WriteLine(line); }
        catch (IOException) { }
    }
}
