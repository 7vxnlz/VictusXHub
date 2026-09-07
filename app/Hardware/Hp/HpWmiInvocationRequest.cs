namespace GHelper.Hardware.Hp;

public sealed record HpWmiInvocationRequest(
    HpBiosWmiCommandDefinition CommandDefinition,
    bool HpVictusModeEnabled = false,
    bool HpWmiReadOnlyTestModeEnabled = false,
    bool ProcessElevated = false);
