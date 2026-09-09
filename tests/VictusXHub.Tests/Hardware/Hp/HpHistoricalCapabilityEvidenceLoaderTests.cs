using GHelper.Hardware.Hp;
using Xunit;

namespace VictusXHub.Tests.Hardware.Hp;

public sealed class HpHistoricalCapabilityEvidenceLoaderTests
{
    [Fact]
    public void CurrentDecodedValues_BeatHistoricalEvidence()
    {
        HpHistoricalCapabilityEvidence historical = new(1, 2, HpCapabilityEvidenceProvenance.LegacyVictusXHistoricalEvidence);

        HpCapabilityEvidenceValue thermal = HpHistoricalCapabilityEvidenceLoader.ResolveThermalPolicy(1, historical);
        HpCapabilityEvidenceValue fanCount = HpHistoricalCapabilityEvidenceLoader.ResolveFanCount(2, historical);

        Assert.Equal((byte)1, thermal.Value);
        Assert.Equal(HpCapabilityEvidenceProvenance.CurrentDecodedReadOnlyEvidence, thermal.Provenance);
        Assert.Equal((byte)2, fanCount.Value);
        Assert.Equal(HpCapabilityEvidenceProvenance.CurrentDecodedReadOnlyEvidence, fanCount.Provenance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(255)]
    public void InvalidCurrentFanCount_FailsClosedToHistoricalEvidence(int currentFanCount)
    {
        HpHistoricalCapabilityEvidence historical = new(1, 2, HpCapabilityEvidenceProvenance.LegacyVictusXHistoricalEvidence);

        HpCapabilityEvidenceValue fanCount = HpHistoricalCapabilityEvidenceLoader.ResolveFanCount((byte)currentFanCount, historical);

        Assert.Equal((byte)2, fanCount.Value);
        Assert.Equal(HpCapabilityEvidenceProvenance.LegacyVictusXHistoricalEvidence, fanCount.Provenance);
    }

    [Fact]
    public void CurrentVictusXHubEvidence_IsPreferredOverLegacyEvidence()
    {
        using EvidenceDirectories directories = new();
        directories.WriteCurrent(ValidRecord());
        directories.WriteLegacy(ValidRecord(timestamp: "2026-09-04T19:41:30.0000000+00:00"));

        HpHistoricalCapabilityEvidence evidence = Assert.IsType<HpHistoricalCapabilityEvidence>(
            HpHistoricalCapabilityEvidenceLoader.Load(directories.Current, directories.Legacy));

        Assert.Equal((byte)1, evidence.ThermalPolicyVersion);
        Assert.Equal((byte)2, evidence.FanCount);
        Assert.Equal(HpCapabilityEvidenceProvenance.HistoricalLocalEvidence, evidence.Provenance);
    }

    [Fact]
    public void LegacyVictusXEvidence_IsUsedWhenCurrentEvidenceIsAbsent()
    {
        using EvidenceDirectories directories = new();
        directories.WriteLegacy(ValidRecord());

        HpHistoricalCapabilityEvidence evidence = Assert.IsType<HpHistoricalCapabilityEvidence>(
            HpHistoricalCapabilityEvidenceLoader.Load(directories.Current, directories.Legacy));

        Assert.Equal(HpCapabilityEvidenceProvenance.LegacyVictusXHistoricalEvidence, evidence.Provenance);
        Assert.Equal((byte)1, evidence.ThermalPolicyVersion);
        Assert.Equal((byte)2, evidence.FanCount);
    }

    [Theory]
    [InlineData("OTHER#AB8", "F.31", 1, 2)]
    [InlineData("7Z5Z2EA#AB8", "F.30", 1, 2)]
    [InlineData("7Z5Z2EA#AB8", "F.31", 2, 2)]
    [InlineData("7Z5Z2EA#AB8", "F.31", 1, 3)]
    public void MismatchedOrUnprovenEvidence_IsRejected(string sku, string bios, int thermalPolicy, int fanCount)
    {
        using EvidenceDirectories directories = new();
        directories.WriteCurrent(ValidRecord(sku, bios, thermalPolicy, fanCount));

        Assert.Null(HpHistoricalCapabilityEvidenceLoader.Load(directories.Current, directories.Legacy));
    }

    [Fact]
    public void MalformedOrMissingEvidence_FailsClosed()
    {
        using EvidenceDirectories directories = new();
        directories.WriteCurrent("{ invalid json");

        Assert.Null(HpHistoricalCapabilityEvidenceLoader.Load(directories.Current, directories.Legacy));
        Assert.Equal(HpCapabilityEvidenceValue.Unavailable,
            HpHistoricalCapabilityEvidenceLoader.ResolveThermalPolicy(null, null));
        Assert.Equal(HpCapabilityEvidenceValue.Unavailable,
            HpHistoricalCapabilityEvidenceLoader.ResolveFanCount(null, null));
    }

    [Fact]
    public void BaselineWithoutSuccessfulReadOnlyProof_IsRejected()
    {
        using EvidenceDirectories directories = new();
        directories.WriteCurrent(ValidRecord(systemDesignDataSummary: "SystemDesignData: attempted=True succeeded=False decodeSucceeded=False"));

        Assert.Null(HpHistoricalCapabilityEvidenceLoader.Load(directories.Current, directories.Legacy));
    }

    [Fact]
    public void LegacyFallback_IsReadOnlyAndDoesNotCopyOrMigrateFiles()
    {
        using EvidenceDirectories directories = new();
        string legacyPath = directories.WriteLegacy(ValidRecord());
        string before = File.ReadAllText(legacyPath);

        _ = HpHistoricalCapabilityEvidenceLoader.Load(directories.Current, directories.Legacy);

        Assert.Equal(before, File.ReadAllText(legacyPath));
        Assert.False(Directory.Exists(Path.Combine(directories.Current, "Logs", "FanExperiments")));
    }

    private static string ValidRecord(
        string sku = "7Z5Z2EA#AB8",
        string bios = "F.31",
        int thermalPolicy = 1,
        int fanCount = 2,
        string timestamp = "2026-09-04T19:40:30.0000000+00:00",
        string systemDesignDataSummary = "SystemDesignData: attempted=True succeeded=True decodeSucceeded=True",
        string fanGetCountSummary = "FanGetCount: attempted=True succeeded=True decodeSucceeded=True") =>
        $$"""
        {
          "TimestampUtc": "{{timestamp}}",
          "Sku": "{{sku}}",
          "BiosVersion": "{{bios}}",
          "BaselineCapturePerformed": true,
          "ThermalPolicyVersion": {{thermalPolicy}},
          "BaselineFanGetCount": {{fanCount}},
          "BaselineReadOnlyProbeSummary": [
            "{{systemDesignDataSummary}}",
            "{{fanGetCountSummary}}"
          ]
        }
        """;

    private sealed class EvidenceDirectories : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), "VictusXHubTests", Guid.NewGuid().ToString("N"));

        public string Current => Path.Combine(root, "VictusXHub");
        public string Legacy => Path.Combine(root, "VictusX");

        public string WriteCurrent(string contents) => Write(Current, contents);
        public string WriteLegacy(string contents) => Write(Legacy, contents);

        public void Dispose()
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }

        private static string Write(string appDataDirectory, string contents)
        {
            string directory = Path.Combine(appDataDirectory, "Logs", "FanExperiments");
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "baseline.json");
            File.WriteAllText(path, contents);
            return path;
        }
    }
}
