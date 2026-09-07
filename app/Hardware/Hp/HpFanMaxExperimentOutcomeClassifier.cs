namespace GHelper.Hardware.Hp;

public static class HpFanMaxExperimentOutcomeClassifier
{
    public static HpFanMaxExperimentalOutcomeClassification Classify(HpFanMaxExperimentLogRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        if (!record.WriteExecuted)
        {
            return HpFanMaxExperimentalOutcomeClassification.BlockedBeforeWrite;
        }

        if (record.UnsafeAbortObserved)
        {
            return HpFanMaxExperimentalOutcomeClassification.UnsafeAbort;
        }

        if (record.RestoreCommandSucceeded == false)
        {
            return HpFanMaxExperimentalOutcomeClassification.RestoreFailed;
        }

        if (record.EnableCommandSucceeded == false)
        {
            return HpFanMaxExperimentalOutcomeClassification.CommandFailed;
        }

        if (record.EnableCommandSucceeded == true && record.PhysicalFanResponseObserved == true)
        {
            return record.FanMaxGetConfirmedEnable == true
                ? HpFanMaxExperimentalOutcomeClassification.Unknown
                : HpFanMaxExperimentalOutcomeClassification.CommandSucceededPhysicalResponseObservedReadbackInconclusive;
        }

        if (record.EnableCommandSucceeded == true)
        {
            return HpFanMaxExperimentalOutcomeClassification.CommandSucceededNoPhysicalConfirmation;
        }

        return HpFanMaxExperimentalOutcomeClassification.Unknown;
    }
}
