using System.Text.Json;
using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpFanLevelResearchDryRunCommandTests
{
    [Theory]
    [InlineData(0, 0, "00-00-00-00")]
    [InlineData(1, 0, "00-00-00-00")]
    [InlineData(2, 1, "01-01-00-00")]
    [InlineData(25, 13, "0D-0D-00-00")]
    [InlineData(50, 27, "1B-1B-00-00")]
    [InlineData(75, 41, "29-29-00-00")]
    [InlineData(99, 54, "36-36-00-00")]
    [InlineData(100, 100, "64-64-00-00")]
    public void PercentageMapping_RecordsCloseDeviceArithmeticOnly(int percent, byte raw, string payload)
    {
        var result = HpFanLevelResearchDryRunCommand.Parse(PercentArguments(percent.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        Assert.True(result.ShouldExit);
        Assert.True(result.IsValidRequest);
        var record = result.Record!;
        Assert.Equal(percent, record.RequestedPercentCandidate);
        Assert.Equal(raw, record.MappedRawLevelCandidate);
        Assert.Null(record.RawLevelCandidate);
        Assert.Equal(payload, record.PayloadHexCandidate);
        Assert.Equal(4, record.CandidateInputLength);
        Assert.Equal(2, record.SchemaVersion);
        Assert.Equal("Close-device evidence", record.EvidenceConfidence);
        Assert.Contains("integer division", record.MappingFormula);
        Assert.Contains("b39b449", record.MappingSource);
        Assert.Contains("truncate to raw zero", record.BoundaryCaution);
        Assert.False(record.TargetBiosValidated);
        Assert.Equal("F.31", record.TargetBios);
        Assert.Null(record.DeviceValidatedInputLength);
        Assert.False(record.FirstWriteReady);
        Assert.False(record.NormalFanControlReady);
        Assert.False(record.IsExecutable);
        Assert.False(record.WriteExecuted);
        Assert.True(record.NoHardwareInvocation);
        Assert.True(record.NoWmiInvocation);
    }

    [Fact]
    public void AllPercentages_AreUnflaggedAndPersistWithoutChangingLegacyRawFormat()
    {
        for (int percent = 0; percent <= 100; percent++)
        {
            var record = HpFanLevelResearchDryRunCommand.Parse(PercentArguments(percent.ToString())).Record!;
            Assert.Equal(0, record.MappedRawLevelCandidate!.Value & 0x80);
        }
        var legacy = HpFanLevelResearchDryRunCommand.Parse(Arguments("128")).Record!;
        Assert.Equal("80-80", legacy.PayloadHexCandidate);
        Assert.Contains("Deprecated", legacy.CandidateModel);
        Assert.Null(legacy.RequestedPercentCandidate);
        Assert.Equal(1, legacy.SchemaVersion);
        DirectoryInfo temporary = Directory.CreateTempSubdirectory("victusx-percent-dry-run-");
        try
        {
            var record = HpFanLevelResearchDryRunCommand.Parse(PercentArguments("50")).Record!;
            string path = HpFanLevelResearchDryRunLogWriter.Write(record, temporary.FullName);
            using var json = JsonDocument.Parse(File.ReadAllText(path));
            Assert.Equal(50, json.RootElement.GetProperty("RequestedPercentCandidate").GetInt32());
            Assert.Equal(27, json.RootElement.GetProperty("MappedRawLevelCandidate").GetInt32());
            Assert.Equal("1B-1B-00-00", json.RootElement.GetProperty("PayloadHexCandidate").GetString());
            Assert.True(json.RootElement.GetProperty("NoHardwareInvocation").GetBoolean());
            Assert.True(json.RootElement.GetProperty("NoWmiInvocation").GetBoolean());
            Assert.False(json.RootElement.GetProperty("FirstWriteReady").GetBoolean());
            Assert.False(json.RootElement.GetProperty("NormalFanControlReady").GetBoolean());
            Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("DeviceValidatedInputLength").ValueKind);
        }
        finally { temporary.Delete(recursive: true); }
    }

    [Theory]
    [InlineData("")]
    [InlineData("-1")]
    [InlineData("101")]
    [InlineData("128")]
    [InlineData("1.5")]
    [InlineData("+1")]
    [InlineData("NaN")]
    [InlineData("99999999999999")]
    public void InvalidPercentages_FailClosed(string value) => AssertRejected(PercentArguments(value));

    [Fact]
    public void PercentageArguments_CannotBypassOrMixStartupGates()
    {
        string[] valid = PercentArguments("50");
        foreach (string required in valid)
        {
            AssertRejected(valid.Where(arg => arg != required).ToArray());
            AssertRejected([.. valid, required]);
        }
        foreach (string extra in new[] { "--fan-level-candidate=128", "--hp-wmi-readonly-test", "--hp-fan-max-hold", "--hp-fan-max-pulse", "--hp-fan-write-experiment" })
            AssertRejected([.. valid, extra]);
        AssertRejected(["--hp-victus", "--fan-percent-candidate"]);
        AssertRejected(["--hp-victus", "--hp-fan-max-hold", "--fan-percent-candidate=50"]);
    }

    private static string[] PercentArguments(string value) =>
        [HpFanLevelResearchDryRunCommand.HpVictusFlag, HpFanLevelResearchDryRunCommand.DryRunFlag,
         HpFanLevelResearchDryRunCommand.PercentPrefix + value];

    [Fact]
    public void UnrelatedArguments_DoNotCreateRecordOrRequestExit()
    {
        var result = HpFanLevelResearchDryRunCommand.Parse(["--hp-victus"]);
        Assert.False(result.ShouldExit);
        Assert.Null(result.Record);
    }

    [Theory]
    [InlineData("0", "00-00")]
    [InlineData("42", "2A-2A")]
    [InlineData("255", "FF-FF")]
    public void ValidRawByte_SerializesOnlyAnUnvalidatedHypothesis(string value, string payload)
    {
        var result = HpFanLevelResearchDryRunCommand.Parse(Arguments(value));
        Assert.True(result.ShouldExit);
        Assert.True(result.IsValidRequest);
        using JsonDocument json = JsonDocument.Parse(result.Record!.ToJson());
        JsonElement record = json.RootElement;
        Assert.Equal(payload, record.GetProperty("PayloadHexCandidate").GetString());
        Assert.Equal(2, record.GetProperty("CandidateInputLength").GetInt32());
        Assert.Equal("0x20008", record.GetProperty("CommandCandidate").GetString());
        Assert.Equal("0x2E", record.GetProperty("CommandTypeCandidate").GetString());
        Assert.Equal("hpqBIOSInt0", record.GetProperty("WmiMethodCandidate").GetString());
        foreach (string property in new[] { "IsExecutable", "WriteExecuted", "WmiInvoked", "FirstWriteGateSatisfied", "NormalControlValidated", "UserFacingControlAllowed" })
        {
            Assert.False(record.GetProperty(property).GetBoolean());
        }
        Assert.Equal(JsonValueKind.Null, record.GetProperty("DeviceValidatedInputLength").ValueKind);
        Assert.Equal("NO-GO", record.GetProperty("NormalFanControlDecision").GetString());
        Assert.Contains("not RPM or percent", result.Record.LevelUnits);
        Assert.Contains("not a safe hardware range", result.Record.SafetyNote);
        Assert.Null(HpFanResearchOperationDescriptor.FourByteMaxFanPulse.DeviceValidatedInputLength);
    }

    [Theory]
    [InlineData("")]
    [InlineData("-1")]
    [InlineData("256")]
    [InlineData("9999999999999999999")]
    [InlineData("1.5")]
    [InlineData("0x20")]
    [InlineData("NaN")]
    [InlineData("+1")]
    [InlineData(" 1")]
    public void InvalidLevel_IsRejectedWithoutCandidatePayload(string value) => AssertRejected(Arguments(value));

    [Fact]
    public void MissingOrDuplicateRequiredArguments_FailClosed()
    {
        string[] valid = Arguments("42");
        foreach (string required in valid)
        {
            AssertRejected(valid.Where(arg => arg != required).ToArray());
            AssertRejected([.. valid, required]);
        }
    }

    [Theory]
    [InlineData("--hp-wmi-readonly-test")]
    [InlineData("--hp-fan-max-hold")]
    [InlineData("--hp-fan-max-pulse")]
    [InlineData("--hp-fan-write-experiment")]
    [InlineData("--hp-fan-write-experiment-baseline-capture")]
    [InlineData("--hp-fan-write-experiment-dry-run")]
    [InlineData("--i-understand-this-can-affect-fans")]
    [InlineData("--set-fan-max-payload-length=1")]
    [InlineData("--set-fan-max-payload-length=4")]
    [InlineData("--unknown-option")]
    public void MixedCommandsAndApprovals_AreConsumedAndRejected(string extra) =>
        AssertRejected([.. Arguments("42"), extra]);

    [Theory]
    [InlineData("--hp-fan-level-research-dry-run=true")]
    [InlineData("--fan-level-candidate")]
    [InlineData("--fan-level-candidate=42")]
    public void MalformedOrOrphanResearchArguments_CannotFallThroughToHardwareRoute(string argument) =>
        AssertRejected(["--hp-victus", "--hp-fan-max-hold", argument]);

    [Fact]
    public void Flags_AreCaseInsensitive()
    {
        var result = HpFanLevelResearchDryRunCommand.Parse(Arguments("42").Select(arg => arg.ToUpperInvariant()));
        Assert.True(result.IsValidRequest);
    }

    private static string[] Arguments(string value) =>
        [HpFanLevelResearchDryRunCommand.HpVictusFlag, HpFanLevelResearchDryRunCommand.DryRunFlag,
         HpFanLevelResearchDryRunCommand.LevelPrefix + value];

    [Theory]
    [InlineData("128", true)]
    [InlineData("256", false)]
    public void PersistedResult_ContainsCandidateOrRejectionAndNoHardwareMarkers(string value, bool valid)
    {
        DirectoryInfo temporary = Directory.CreateTempSubdirectory("victusx-level-dry-run-");
        try
        {
            var result = HpFanLevelResearchDryRunCommand.Parse(Arguments(value));
            string directory = Path.Combine(temporary.FullName, "FanExperiments");
            string path = HpFanLevelResearchDryRunLogWriter.Write(result.Record!, directory);
            Assert.StartsWith("set-fan-level-research-dry-run-", Path.GetFileName(path));
            using JsonDocument json = JsonDocument.Parse(File.ReadAllText(path));
            JsonElement record = json.RootElement;
            Assert.Equal(result.Record!.TimestampUtc, record.GetProperty("TimestampUtc").GetDateTimeOffset());
            Assert.Equal(valid, record.GetProperty("IsValidCandidate").GetBoolean());
            Assert.True(record.GetProperty("NoHardwareInvocation").GetBoolean());
            Assert.True(record.GetProperty("NoWmiInvocation").GetBoolean());
            Assert.False(record.GetProperty("WriteExecuted").GetBoolean());
            Assert.False(record.GetProperty("IsExecutable").GetBoolean());
            Assert.Equal(JsonValueKind.Null, record.GetProperty("DeviceValidatedInputLength").ValueKind);
            Assert.Equal("Not executable / not validated", record.GetProperty("Status").GetString());
            if (valid)
            {
                Assert.Equal(128, record.GetProperty("RawLevelCandidate").GetInt32());
                Assert.Equal("80-80", record.GetProperty("PayloadHexCandidate").GetString());
            }
            else
            {
                Assert.Equal(JsonValueKind.Null, record.GetProperty("PayloadHexCandidate").ValueKind);
                Assert.NotEmpty(result.Record.ValidationReasons);
            }

            string original = File.ReadAllText(path);
            string second = HpFanLevelResearchDryRunLogWriter.Write(result.Record, directory);
            Assert.NotEqual(path, second);
            Assert.Equal(original, File.ReadAllText(path));
            Assert.Equal(2, Directory.GetFiles(directory).Length);
            Assert.Empty(HpFanMaxPulseHistoryLoader.LoadAll(directory).Entries);
        }
        finally { temporary.Delete(recursive: true); }
    }

    [Fact]
    public void PersistenceFailure_IsReportedWithoutChangingSafetyState()
    {
        DirectoryInfo temporary = Directory.CreateTempSubdirectory("victusx-level-dry-run-");
        try
        {
            string occupied = Path.Combine(temporary.FullName, "occupied");
            File.WriteAllText(occupied, "existing file");
            var result = HpFanLevelResearchDryRunCommand.Parse(Arguments("128"));
            Assert.Throws<IOException>(() => HpFanLevelResearchDryRunLogWriter.Write(result.Record!, occupied));
            Assert.False(result.Record!.WriteExecuted);
            Assert.Null(result.Record.DeviceValidatedInputLength);
            Assert.Equal("existing file", File.ReadAllText(occupied));
        }
        finally { temporary.Delete(recursive: true); }
    }

    private static void AssertRejected(string[] arguments)
    {
        var result = HpFanLevelResearchDryRunCommand.Parse(arguments);
        Assert.True(result.ShouldExit);
        Assert.False(result.IsValidRequest);
        Assert.NotEmpty(result.Record!.ValidationReasons);
        Assert.Null(result.Record.PayloadHexCandidate);
        Assert.Null(result.Record.RawLevelCandidate);
        Assert.Null(result.Record.DeviceValidatedInputLength);
        Assert.False(result.Record.IsExecutable);
        Assert.False(result.Record.WriteExecuted);
    }
}
