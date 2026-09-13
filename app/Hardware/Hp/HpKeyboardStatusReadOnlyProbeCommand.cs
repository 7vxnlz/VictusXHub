using System.Management;
using System.Runtime.InteropServices;

namespace GHelper.Hardware.Hp;

internal sealed record HpKeyboardStatusReadOnlyProbeCommandRequest(bool ShouldExit, bool IsValidRequest, string[] ValidationErrors);

internal static class HpKeyboardStatusReadOnlyProbeCommand
{
    internal const string ProbeFlag = "--hp-keyboard-status-readonly-probe";
    private const string HpVictusFlag = "--hp-victus";

    internal static bool TryRun(string[] args)
    {
        HpKeyboardStatusReadOnlyProbeCommandRequest request = Parse(args);
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

        HpKeyboardStatusReadOnlyProbeDevice device = ReadDevice();
        HpKeyboardStatusReadOnlyProbeGateResult gate = HpKeyboardStatusReadOnlyProbeGate.Evaluate(device);
        WriteLine("VictusXHub keyboard STATUS read-only probe (raw evidence collection only)");
        WriteLine("Target gate: " + (gate.IsAccepted ? "accepted" : "rejected") + " — " + gate.Reason);
        if (!gate.IsAccepted)
        {
            Environment.ExitCode = 2;
            return true;
        }

        HpBiosWmiCommandDefinition definition = HpBiosWmiCommandCatalog.Definitions.Single(candidate =>
            string.Equals(candidate.Name, "KeyboardStatus", StringComparison.Ordinal));
        var invocationClient = new HpWmiInvocationClient();
        HpWmiInvocationResult invocation = invocationClient.TryInvoke(
            new HpWmiInvocationRequest(
                definition,
                HpVictusModeEnabled: true,
                HpWmiReadOnlyTestModeEnabled: true,
                ProcessElevated: global::GHelper.Helpers.ProcessHelper.IsUserAdministrator()),
            new HpWmiReadOnlyClient().Probe());
        HpKeyboardStatusReadOnlyProbeResult result = HpKeyboardStatusReadOnlyProbeResult.FromInvocation(invocation);
        WriteLines(HpKeyboardStatusReadOnlyProbeFormatter.Format(result));
        Environment.ExitCode = result.IsCaptured ? 0 : 1;
        return true;
    }

    internal static HpKeyboardStatusReadOnlyProbeCommandRequest Parse(string[] args)
    {
        bool hasProbeFlag = args.Any(argument => string.Equals(argument, ProbeFlag, StringComparison.OrdinalIgnoreCase));
        if (!hasProbeFlag)
        {
            return new(false, false, []);
        }

        bool hasExactlyOneProbeFlag = args.Count(argument => string.Equals(argument, ProbeFlag, StringComparison.OrdinalIgnoreCase)) == 1;
        bool hasExactlyOneHpVictusFlag = args.Count(argument => string.Equals(argument, HpVictusFlag, StringComparison.OrdinalIgnoreCase)) == 1;
        bool exactArgumentCount = args.Length == 2;
        if (hasExactlyOneProbeFlag && hasExactlyOneHpVictusFlag && exactArgumentCount)
        {
            return new(true, true, []);
        }

        return new(true, false,
            ["Keyboard STATUS probe rejected: require exactly --hp-victus --hp-keyboard-status-readonly-probe."]);
    }

    private static HpKeyboardStatusReadOnlyProbeDevice ReadDevice() =>
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

    private static void WriteLine(string line)
    {
        try { Console.WriteLine(line); }
        catch (IOException) { }
    }
}
