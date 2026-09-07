namespace GHelper.Hardware.Hp;

public sealed record HpWmiReadOnlySnapshot(
    HpVictusProbeAvailability RootWmiAvailability,
    HpVictusProbeAvailability HpqBIntMAvailability,
    HpVictusProbeAvailability HpqBDataInAvailability,
    string[] HpqBIntMMethodNames,
    string[] HpqBDataInMethodNames,
    string[] Errors);
