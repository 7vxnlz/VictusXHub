using System.Globalization;
using System.Management;
using System.Runtime.InteropServices;

namespace GHelper.Hardware.Hp;

public sealed class HpWmiInvocationClient
{
    private const string HpWmiScopePath = @"\\.\root\wmi";
    private const string BiosMethodClassName = "hpqBIntM";
    private const string BiosDataClassName = "hpqBDataIn";
    private const string BiosDataInputFieldName = "hpqBData";
    private const string BiosMethodInstance = @"ACPI\PNP0C14\0_0";
    private const string SystemDesignDataCommandName = "SystemDesignData";
    private const string FanGetCountCommandName = "FanGetCount";
    private const string FanMaxGetCommandName = "FanMaxGet";
    private const string FanGetLevelCommandName = "FanGetLevel";
    private const uint DefaultBiosCommand = 0x20008;

    private static readonly byte[] BiosSign = [0x53, 0x45, 0x43, 0x55];

    private readonly Action<string>? _log;

    public HpWmiInvocationClient(Action<string>? log = null)
    {
        _log = log;
    }

    public HpWmiInvocationSandboxStatus ValidateCatalog(
        HpWmiReadOnlySnapshot wmiSnapshot,
        IEnumerable<HpBiosWmiCommandDefinition> definitions)
    {
        var errors = new List<string>();
        int safeReadOnlyCommandCount = 0;
        int rejectedCommandCount = 0;
        bool sandboxAvailable = IsRequiredWmiSurfaceAvailable(wmiSnapshot);
        var exposedMethods = new HashSet<string>(wmiSnapshot.HpqBIntMMethodNames, StringComparer.OrdinalIgnoreCase);

        foreach (HpBiosWmiCommandDefinition definition in definitions)
        {
            string? rejectionReason = GetRejectionReason(definition, sandboxAvailable, exposedMethods);
            if (rejectionReason is null)
            {
                safeReadOnlyCommandCount++;
                continue;
            }

            rejectedCommandCount++;
            _log?.Invoke($"HP WMI invocation sandbox rejected '{definition.Name}': {rejectionReason}");
        }

        if (!sandboxAvailable)
        {
            errors.Add("Required HP WMI surface is unavailable.");
        }

        errors.AddRange(wmiSnapshot.Errors);

        return new HpWmiInvocationSandboxStatus(
            sandboxAvailable,
            safeReadOnlyCommandCount,
            rejectedCommandCount,
            errors.Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
    }

    public HpWmiInvocationResult TryInvoke(HpWmiInvocationRequest request, HpWmiReadOnlySnapshot wmiSnapshot)
    {
        if (!request.HpVictusModeEnabled)
        {
            const string reason = "--hp-victus mode is required for HP BIOS WMI invocation";
            _log?.Invoke($"HP WMI invocation sandbox rejected '{request.CommandDefinition.Name}': {reason}");
            return HpWmiInvocationResult.Rejected(request.CommandDefinition, reason);
        }

        if (!request.HpWmiReadOnlyTestModeEnabled)
        {
            const string reason = "skipped by missing explicit --hp-wmi-readonly-test flag";
            _log?.Invoke($"HP WMI invocation sandbox skipped '{request.CommandDefinition.Name}': {reason}");
            return HpWmiInvocationResult.Rejected(request.CommandDefinition, reason);
        }

        if (!request.ProcessElevated)
        {
            const string reason = "skipped because process is not elevated; run controlled HP WMI read-only tests as Administrator";
            _log?.Invoke($"HP WMI invocation sandbox skipped '{request.CommandDefinition.Name}': {reason}");
            return HpWmiInvocationResult.Rejected(request.CommandDefinition, reason);
        }

        if (!IsApprovedInvocationCommand(request.CommandDefinition.Name))
        {
            const string reason = "only SystemDesignData, FanGetCount, FanMaxGet, and FanGetLevel are approved for real HP BIOS WMI invocation";
            _log?.Invoke($"HP WMI invocation sandbox rejected '{request.CommandDefinition.Name}': {reason}");
            return HpWmiInvocationResult.Rejected(request.CommandDefinition, reason);
        }

        bool sandboxAvailable = IsRequiredWmiSurfaceAvailable(wmiSnapshot);
        var exposedMethods = new HashSet<string>(wmiSnapshot.HpqBIntMMethodNames, StringComparer.OrdinalIgnoreCase);
        string? rejectionReason = GetRejectionReason(request.CommandDefinition, sandboxAvailable, exposedMethods);

        if (rejectionReason is not null)
        {
            _log?.Invoke($"HP WMI invocation sandbox rejected '{request.CommandDefinition.Name}': {rejectionReason}");
            return HpWmiInvocationResult.Rejected(request.CommandDefinition, rejectionReason);
        }

        LogBeforeInvocation(request.CommandDefinition);
        HpWmiInvocationResult result = InvokeSafeReadOnlyCommand(request.CommandDefinition);
        LogAfterInvocation(result);

        return result;
    }

    public HpWmiInvocationResult DryRun(
        string commandName,
        HpWmiReadOnlySnapshot wmiSnapshot,
        IEnumerable<HpBiosWmiCommandDefinition> definitions)
    {
        HpBiosWmiCommandDefinition? definition = definitions.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, commandName, StringComparison.OrdinalIgnoreCase));

        if (definition is null)
        {
            const string reason = "command definition not found";
            _log?.Invoke($"HP WMI invocation sandbox rejected '{commandName}': {reason}");
            return HpWmiInvocationResult.Rejected(commandName, reason);
        }

        return DryRun(new HpWmiInvocationRequest(definition), wmiSnapshot);
    }

    public HpWmiInvocationResult DryRun(HpWmiInvocationRequest request, HpWmiReadOnlySnapshot wmiSnapshot)
    {
        bool sandboxAvailable = IsRequiredWmiSurfaceAvailable(wmiSnapshot);
        var exposedMethods = new HashSet<string>(wmiSnapshot.HpqBIntMMethodNames, StringComparer.OrdinalIgnoreCase);
        string? rejectionReason = GetRejectionReason(request.CommandDefinition, sandboxAvailable, exposedMethods);

        if (rejectionReason is not null)
        {
            _log?.Invoke($"HP WMI invocation sandbox dry-run rejected '{request.CommandDefinition.Name}': {rejectionReason}");
            return HpWmiInvocationResult.Rejected(request.CommandDefinition, rejectionReason);
        }

        _log?.Invoke($"HP WMI invocation sandbox dry-run ready for '{request.CommandDefinition.Name}'. No HP BIOS WMI method was invoked.");
        return HpWmiInvocationResult.DryRunReady(request.CommandDefinition);
    }

    private static bool IsRequiredWmiSurfaceAvailable(HpWmiReadOnlySnapshot wmiSnapshot) =>
        wmiSnapshot.RootWmiAvailability == HpVictusProbeAvailability.Available &&
        wmiSnapshot.HpqBIntMAvailability == HpVictusProbeAvailability.Available &&
        wmiSnapshot.HpqBDataInAvailability == HpVictusProbeAvailability.Available;

    private static string? GetRejectionReason(
        HpBiosWmiCommandDefinition definition,
        bool sandboxAvailable,
        ISet<string> exposedMethods)
    {
        if (!sandboxAvailable)
        {
            return "required HP WMI surface is unavailable";
        }

        if (definition.Access != HpBiosWmiCommandAccess.ReadOnly)
        {
            return "command is not read-only";
        }

        if (definition.Safety != HpBiosWmiCommandSafety.SafeReadOnlyInvocation)
        {
            return "command is not explicitly marked safe for invocation";
        }

        if (definition.ExpectedInputSize < 0 || definition.ExpectedOutputSize < 0)
        {
            return "command has invalid input or output size metadata";
        }

        if (string.IsNullOrWhiteSpace(definition.MethodName) || !definition.MethodName.StartsWith("hpqBIOSInt", StringComparison.Ordinal))
        {
            return "command method name is not an HP BIOS WMI method";
        }

        if (!exposedMethods.Contains(definition.MethodName))
        {
            return "command method is not exposed by hpqBIntM on this machine";
        }

        string expectedMethodName = "hpqBIOSInt" + definition.ExpectedOutputSize.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (!string.Equals(definition.MethodName, expectedMethodName, StringComparison.Ordinal))
        {
            return "command method name does not match expected output size";
        }

        return null;
    }

    private static bool IsApprovedInvocationCommand(string commandName) =>
        string.Equals(commandName, SystemDesignDataCommandName, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(commandName, FanGetCountCommandName, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(commandName, FanMaxGetCommandName, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(commandName, FanGetLevelCommandName, StringComparison.OrdinalIgnoreCase);

    private HpWmiInvocationResult InvokeSafeReadOnlyCommand(HpBiosWmiCommandDefinition definition)
    {
        try
        {
            var scope = new ManagementScope(HpWmiScopePath);
            scope.Connect();

            using var biosInstance = FindBiosMethodInstance(scope);
            if (biosInstance is null)
            {
                return HpWmiInvocationResult.Failed(definition, "hpqBIntM BIOS method instance was not found");
            }

            using var dataClass = new ManagementClass(scope, new ManagementPath(BiosDataClassName), null);
            using var inputData = dataClass.CreateInstance();
            if (inputData is null)
            {
                return HpWmiInvocationResult.Failed(definition, "hpqBDataIn input object could not be created");
            }

            inputData["Sign"] = BiosSign;
            inputData["Command"] = DefaultBiosCommand;
            inputData["CommandType"] = definition.CommandId;
            inputData["Size"] = (uint)definition.ExpectedInputSize;
            if (definition.ExpectedInputSize > 0)
            {
                inputData[BiosDataInputFieldName] = new byte[definition.ExpectedInputSize];
            }

            using var inParams = biosInstance.GetMethodParameters(definition.MethodName);
            inParams["InData"] = inputData;

            using var outParams = biosInstance.InvokeMethod(definition.MethodName, inParams, null);
            if (outParams is null)
            {
                return HpWmiInvocationResult.Failed(definition, "HP BIOS WMI invocation returned no output parameters");
            }

            using var outData = outParams["OutData"] as ManagementBaseObject;
            if (outData is null)
            {
                return HpWmiInvocationResult.Failed(definition, "HP BIOS WMI invocation returned no OutData object");
            }

            int? returnCode = TryReadReturnCode(outData);
            if (returnCode is not null && returnCode != 0)
            {
                return HpWmiInvocationResult.Failed(definition, $"HP BIOS WMI returned code {returnCode.Value}", returnCode);
            }

            byte[] returnedBytes = outData["Data"] as byte[] ?? [];
            return HpWmiInvocationResult.SuccessfulInvocation(definition, returnedBytes, returnCode);
        }
        catch (Exception ex) when (ex is ManagementException or UnauthorizedAccessException or COMException or InvalidOperationException)
        {
            return HpWmiInvocationResult.Failed(definition, $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    private static ManagementObject? FindBiosMethodInstance(ManagementScope scope)
    {
        using var methodClass = new ManagementClass(scope, new ManagementPath(BiosMethodClassName), null);
        using var instances = methodClass.GetInstances();

        foreach (ManagementObject instance in instances)
        {
            string instanceName = instance["InstanceName"]?.ToString() ?? string.Empty;
            if (instanceName.Contains(BiosMethodInstance, StringComparison.OrdinalIgnoreCase))
            {
                return instance;
            }

            instance.Dispose();
        }

        return null;
    }

    private static int? TryReadReturnCode(ManagementBaseObject outData)
    {
        try
        {
            return Convert.ToInt32(outData["rwReturnCode"], CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    private void LogBeforeInvocation(HpBiosWmiCommandDefinition definition)
    {
        _log?.Invoke(
            "HP BIOS WMI read-only invocation starting: " +
            $"Timestamp={DateTimeOffset.Now.ToString("O", CultureInfo.InvariantCulture)}, " +
            $"Command={definition.Name}, Method={definition.MethodName}, Access={definition.Access}, " +
            $"Safety={definition.Safety}, ExpectedInputSize={definition.ExpectedInputSize}, " +
            $"ExpectedOutputSize={definition.ExpectedOutputSize}");
    }

    private void LogAfterInvocation(HpWmiInvocationResult result)
    {
        string error = result.Errors.Length == 0 ? "none" : string.Join(" | ", result.Errors);
        _log?.Invoke(
            "HP BIOS WMI read-only invocation completed: " +
            $"Command={result.CommandName}, Success={result.Success}, Invoked={result.Invoked}, " +
            $"ReturnedByteCount={result.ReturnedByteCount?.ToString(CultureInfo.InvariantCulture) ?? "unknown"}, " +
            $"ReturnCode={result.ReturnCode?.ToString(CultureInfo.InvariantCulture) ?? "unknown"}, Error={error}");
    }
}
