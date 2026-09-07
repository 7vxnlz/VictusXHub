namespace GHelper.Hardware.Hp;

public sealed record HpWmiInvocationResult(
    bool Success,
    bool Invoked,
    string CommandName,
    string MethodName,
    string Status,
    int? ReturnedByteCount,
    int? ReturnCode,
    string[] Errors,
    byte[]? ReturnedBytes = null)
{
    public static HpWmiInvocationResult Rejected(HpBiosWmiCommandDefinition definition, string reason) =>
        new(false, false, definition.Name, definition.MethodName, "Rejected", null, null, [reason]);

    public static HpWmiInvocationResult Rejected(string commandName, string reason) =>
        new(false, false, commandName, string.Empty, "Rejected", null, null, [reason]);

    public static HpWmiInvocationResult DryRunReady(HpBiosWmiCommandDefinition definition) =>
        new(true, false, definition.Name, definition.MethodName, "DryRunReady", null, null, []);

    public static HpWmiInvocationResult SuccessfulInvocation(HpBiosWmiCommandDefinition definition, byte[] returnedBytes, int? returnCode) =>
        new(true, true, definition.Name, definition.MethodName, "Invoked", returnedBytes.Length, returnCode, [], returnedBytes.ToArray());

    public static HpWmiInvocationResult Failed(HpBiosWmiCommandDefinition definition, string reason, int? returnCode = null) =>
        new(false, true, definition.Name, definition.MethodName, "Failed", null, returnCode, [reason]);
}

public sealed record HpWmiInvocationSandboxStatus(
    bool InvocationSandboxAvailable,
    int SafeReadOnlyCommandCount,
    int RejectedCommandCount,
    string[] Errors);
