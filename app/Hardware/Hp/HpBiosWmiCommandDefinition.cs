namespace GHelper.Hardware.Hp;

public enum HpBiosWmiCommandAccess
{
    Unknown,
    ReadOnly,
    WriteCapable
}

public enum HpBiosWmiCommandSafety
{
    Unknown,
    MetadataOnly,
    ReadIntent,
    SafeReadOnlyInvocation,
    Forbidden
}

public enum HpBiosWmiCommandFamily
{
    Unknown,
    System,
    Fan,
    Thermal,
    Gpu,
    Keyboard,
    Battery,
    Display,
    Lighting,
    Power
}

public sealed record HpBiosWmiCommandDefinition(
    string Name,
    HpBiosWmiCommandFamily Family,
    uint CommandId,
    string MethodName,
    int ExpectedInputSize,
    int ExpectedOutputSize,
    HpBiosWmiCommandAccess Access,
    HpBiosWmiCommandSafety Safety,
    string Description);
