namespace GHelper.Hardware.Hp;

// Reference metadata only. No transport, payload builder, or execution capability.
internal static class HpPerformanceModeStatus
{
    internal static bool CanSwitch => false;
    internal static int CurrentBaseMode => -1;
    internal static string DisplayText => "Performance mode: Unavailable";
    internal static string Blocker => "HP mode switching is not validated on BIOS F.31; current mode and safe recovery are unverified.";

    // omencore V1 aliases; these are not validated target-device commands.
    internal static byte? ReferenceModeByte(int inheritedBaseMode) => inheritedBaseMode switch
    {
        2 => 0x50,
        0 => 0x30,
        1 => 0x31,
        _ => null
    };
}
