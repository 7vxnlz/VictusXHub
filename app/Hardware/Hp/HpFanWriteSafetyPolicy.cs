namespace GHelper.Hardware.Hp;

public sealed record HpFanWriteSafetyPolicy
{
    public static readonly IReadOnlyList<string> DefaultRequiredFlags = Array.AsReadOnly(
    [
        "--hp-victus",
        "--hp-fan-write-experiment",
        "--hp-wmi-write-manual-test",
        "--hp-fan-write-acknowledge-risk"
    ]);

    public IReadOnlyList<string> RequiredFlags => DefaultRequiredFlags;
    public bool RequiresAdministrator => true;
    public bool RequiresReadbackBeforeWrite => true;
    public bool RequiresReadbackAfterWrite => true;
    public bool RequiresVerifiedRestore => true;
    public bool IsWriteExecutionAllowed => false;
}
