using System.Text.Json;
using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpFanLevelFirstWritePreflightTests
{
    private static HpFanLevelFirstWriteRequest Complete => new()
    {
        RequestedPercent = 50, Fan0RawCandidate = 27, Fan1RawCandidate = 27,
        Target = new("HP", "Victus 16-s0035nt", "7Z5Z2EA#AB8", "F.31", 1),
        IsAdministrator = true, AcOnline = true, BaselineTelemetrySucceeded = true,
        BaselineEvidenceReference = "test baseline", RecoveryPlanReference = "test plan",
        RecoveryPlanReviewed = true, LogDestination = "test-only-destination", AppendOnlyLoggingRequired = true
    };

    public static IEnumerable<object[]> MissingGates()
    {
        yield return [Complete with { RequestedPercent = null }];
        yield return [Complete with { Fan0RawCandidate = null }];
        yield return [Complete with { Target = null }];
        yield return [Complete with { Target = Complete.Target! with { Manufacturer = "Other" } }];
        yield return [Complete with { Target = Complete.Target! with { Model = "Victus" } }];
        yield return [Complete with { Target = Complete.Target! with { Sku = "other" } }];
        yield return [Complete with { Target = Complete.Target! with { Bios = "F.30" } }];
        yield return [Complete with { Target = Complete.Target! with { ThermalPolicyVersion = 2 } }];
        yield return [Complete with { IsAdministrator = null }];
        yield return [Complete with { IsAdministrator = false }];
        yield return [Complete with { AcOnline = null }];
        yield return [Complete with { AcOnline = false }];
        yield return [Complete with { BaselineTelemetrySucceeded = false }];
        yield return [Complete with { BaselineEvidenceReference = " " }];
        yield return [Complete with { RecoveryPlanReference = null }];
        yield return [Complete with { RecoveryPlanReviewed = false }];
        yield return [Complete with { LogDestination = " " }];
        yield return [Complete with { AppendOnlyLoggingRequired = false }];
        yield return [Complete with { UnsafeAbortObserved = true }];
        yield return [Complete with { Fan1RawCandidate = 28 }];
        yield return [Complete with { Fan0RawCandidate = 28, Fan1RawCandidate = 28 }];
    }

    [Theory, MemberData(nameof(MissingGates))]
    public void MissingOrConflictingEvidenceBlocks(object request)
    {
        var plan = HpFanLevelFirstWritePreflight.Evaluate((HpFanLevelFirstWriteRequest)request);
        Assert.False(plan.PreflightSatisfied);
        Assert.NotEmpty(plan.Reasons);
        Assert.Null(plan.CandidatePayload);
        Assert.False(plan.IsExecutable);
    }

    [Theory]
    [InlineData(-1)] [InlineData(0)] [InlineData(1)] [InlineData(100)] [InlineData(101)]
    public void ExcludedPercentBlocks(int percent) => Assert.False(
        HpFanLevelFirstWritePreflight.Evaluate(Complete with { RequestedPercent = percent }).PreflightSatisfied);

    [Theory]
    [InlineData(-1)] [InlineData(0)] [InlineData(55)] [InlineData(100)] [InlineData(128)] [InlineData(255)]
    public void ExcludedRawBlocks(int raw) => Assert.False(
        HpFanLevelFirstWritePreflight.Evaluate(Complete with { Fan0RawCandidate = raw, Fan1RawCandidate = raw }).PreflightSatisfied);

    [Theory]
    [InlineData(2, 1)] [InlineData(25, 13)] [InlineData(50, 27)] [InlineData(99, 54)]
    public void CompleteDeclarationsOnlySatisfyOfflinePreflight(int percent, int raw)
    {
        var plan = HpFanLevelFirstWritePreflight.Evaluate(Complete with
        { RequestedPercent = percent, Fan0RawCandidate = raw, Fan1RawCandidate = raw });
        Assert.True(plan.PreflightSatisfied);
        Assert.Equal(new byte[] { (byte)raw, (byte)raw, 0, 0 }, plan.CandidatePayload);
        using var json = JsonDocument.Parse(plan.ToJson());
        Assert.False(json.RootElement.GetProperty("IsExecutable").GetBoolean());
        Assert.False(plan.FirstWriteReady);
        Assert.False(plan.NormalFanControlReady);
        Assert.False(plan.WriteExecuted);
        Assert.True(plan.NoHardwareInvocation);
        Assert.True(plan.NoWmiInvocation);
        Assert.Null(plan.DeviceValidatedInputLength);
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("DeviceValidatedInputLength").ValueKind);
    }

    [Fact]
    public void ObservationsCannotSupplyMissingGatesAndNotesAreBounded()
    {
        var plan = HpFanLevelFirstWritePreflight.Evaluate(new()
        { PhysicalFanResponseObserved = true, RestoreObserved = true, ManualObservationNotes = "\n" + new string('x', 2000) });
        Assert.False(plan.PreflightSatisfied);
        Assert.Equal(1024, plan.Request.ManualObservationNotes!.Length);
        Assert.DoesNotContain('\n', plan.Request.ManualObservationNotes);
        Assert.False(HpFanLevelFirstWritePreflight.Evaluate(null).PreflightSatisfied);
    }

    [Fact]
    public void ScaffoldHasNoTransportOrStartupRoute()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "VictusX.sln"))) root = root.Parent;
        Assert.NotNull(root);
        string source = File.ReadAllText(Path.Combine(root!.FullName, "app/Hardware/Hp/HpFanLevelFirstWritePreflight.cs"));
        foreach (string forbidden in new[] { "hpqBIOSInt", "System.Management", "DllImport", "Process.Start", "PawnIO", "EmbeddedController", "Timer", "Task.Run", "delegate", "Execute(", "Write(" })
            Assert.DoesNotContain(forbidden, source);
        foreach (string file in new[] { "app/Program.cs", "app/Settings.cs" })
            Assert.DoesNotContain("HpFanLevelFirstWrite", File.ReadAllText(Path.Combine(root.FullName, file)));
    }
}
