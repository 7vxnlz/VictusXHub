using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpFanMaxExperimentBaselineCaptureMapperTests
{
    [Fact]
    public void CreateCaptured_MapsReadOnlyEvidenceAndPreservesNoGo()
    {
        HpFanMaxExperimentBaselineCaptureCommandResult command = HpFanMaxExperimentBaselineCaptureCommand.Parse(
        [
            "--hp-victus",
            "--hp-wmi-readonly-test",
            "--hp-fan-write-experiment-baseline",
            "--set-fan-max-payload-length=4"
        ]);
        var data = new HpFanMaxExperimentBaselineCaptureData(
            "HP Victus 16-s0035nt",
            "7Z5Z2EA#AB8",
            "F.31",
            1,
            2,
            false,
            "17-00",
            ["FanGetCount: attempted=True; succeeded=True; decode=True; returnedBytes=4"]);

        HpFanMaxExperimentLogRecord record = HpFanMaxExperimentBaselineCaptureMapper.CreateCaptured(command, data);

        Assert.True(record.BaselineCapturePerformed);
        Assert.Equal("HP Victus 16-s0035nt", record.Model);
        Assert.Equal("7Z5Z2EA#AB8", record.Sku);
        Assert.Equal("F.31", record.BiosVersion);
        Assert.Equal(1, record.ThermalPolicyVersion);
        Assert.Equal(2, record.BaselineFanGetCount);
        Assert.False(record.BaselineFanMaxGet);
        Assert.Equal("17-00", record.BaselineFanGetLevelRaw);
        Assert.Single(record.BaselineReadOnlyProbeSummary);
        Assert.False(record.WriteExecuted);
        Assert.False(record.FirstWriteGateSatisfied);
        Assert.Null(record.DeviceValidatedInputLength);
        Assert.Contains("NO-GO", record.BlockedReasons[0], StringComparison.Ordinal);
    }

    [Fact]
    public void CreateBlocked_LeavesBaselineUnavailableAndFailClosed()
    {
        HpFanMaxExperimentBaselineCaptureCommandResult command = HpFanMaxExperimentBaselineCaptureCommand.Parse(
        [
            "--hp-victus",
            "--hp-fan-write-experiment-baseline",
            "--set-fan-max-payload-length=1"
        ]);

        HpFanMaxExperimentLogRecord record = HpFanMaxExperimentBaselineCaptureMapper.CreateBlocked(
            command,
            "Baseline capture requires an elevated Administrator process.");

        Assert.False(record.BaselineCapturePerformed);
        Assert.Contains("not started", record.BaselineCaptureResult, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No WMI or hardware action", record.BaselineCaptureResult, StringComparison.Ordinal);
        Assert.Null(record.BaselineFanGetCount);
        Assert.Empty(record.BaselineReadOnlyProbeSummary);
        Assert.False(record.WriteExecuted);
        Assert.False(record.FirstWriteGateSatisfied);
        Assert.Null(record.DeviceValidatedInputLength);
        Assert.Contains(record.BlockedReasons, reason => reason.Contains("Administrator", StringComparison.Ordinal));
    }

    [Fact]
    public void Mapper_HasNoWmiDependencyOrInvocationSurface()
    {
        Type mapperType = typeof(HpFanMaxExperimentBaselineCaptureMapper);

        Assert.DoesNotContain(
            mapperType.GetMethods(),
            method => method.Name.Contains("Invoke", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            mapperType.GetMethods().SelectMany(method => method.GetParameters()),
            parameter => parameter.ParameterType.Namespace?.Contains("Management", StringComparison.Ordinal) == true);
    }
}
