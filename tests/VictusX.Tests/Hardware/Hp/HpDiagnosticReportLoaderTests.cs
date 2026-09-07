using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpDiagnosticReportLoaderTests
{
    [Fact]
    public void Load_MissingReport_ReturnsNotAvailable()
    {
        HpDiagnosticReportLoadResult result = HpDiagnosticReportLoader.Load(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json"));

        Assert.Equal(HpDiagnosticReportLoadStatus.NotAvailable, result.Status);
        Assert.Contains("Report not available", result.SourceDescription, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{\"LooksLikeHp\":true")]
    public void Load_EmptyOrTruncatedReport_ReturnsCouldNotBeRead(string contents)
    {
        string reportPath = WriteTemporaryReport(contents);
        try
        {
            HpDiagnosticReportLoadResult result = HpDiagnosticReportLoader.Load(reportPath);

            Assert.Equal(HpDiagnosticReportLoadStatus.CouldNotBeRead, result.Status);
            Assert.Contains("Report could not be read", result.SourceDescription, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(reportPath);
        }
    }

    [Fact]
    public void Load_OlderPartialReport_LoadsAvailableValuesAndLeavesNewFieldsMissing()
    {
        string reportPath = WriteTemporaryReport("{\"LooksLikeHp\":true,\"LooksLikeVictus\":true,\"Manufacturer\":\"HP\"}");
        try
        {
            HpDiagnosticReportLoadResult result = HpDiagnosticReportLoader.Load(reportPath);

            Assert.Equal(HpDiagnosticReportLoadStatus.Loaded, result.Status);
            Assert.True(result.GetHpVictusDetected());
            Assert.Equal("HP", result.GetValue("Manufacturer"));
            Assert.Null(result.GetValue("FanGetLevelDecoded.Fan1RawValue"));
            Assert.Contains("Some fields are not available", result.SourceDescription, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(reportPath);
        }
    }

    [Fact]
    public void RefreshExistingReport_UpgradesSchemaV1WithSetFanMaxExperimentalFieldsWithoutChangingInvocationFlags()
    {
        string reportPath = WriteTemporaryReport(
            "{\"ReportSchemaVersion\":1,\"ReportGeneratedBy\":\"VictusX\",\"ReportMode\":\"HP read-only diagnostic\",\"SystemDesignDataInvocationAttempted\":false,\"FanGetCountInvocationAttempted\":false,\"FanMaxGetInvocationAttempted\":false,\"FanGetLevelInvocationAttempted\":false}");
        try
        {
            Assert.True(HpDiagnosticReportSchemaV2Refresher.TryRefreshExistingReport(reportPath));

            HpDiagnosticReportLoadResult result = HpDiagnosticReportLoader.Load(reportPath);

            Assert.Equal(HpDiagnosticReportLoadStatus.Loaded, result.Status);
            Assert.Equal("2", result.GetValue("ReportSchemaVersion"));
            Assert.Equal("FourByte", result.GetValue("SetFanMaxExperimentalPayloadCandidate"));
            Assert.Equal("true", result.GetValue("SetFanMaxPhysicalResponseObserved"));
            Assert.Equal("2", result.GetValue("SetFanMaxPhysicalResponseConfirmationCount"));
            Assert.Equal("false", result.GetValue("SetFanMaxReadbackReliable"));
            Assert.Equal("true", result.GetValue("SetFanMaxDeveloperExperimentAllowed"));
            Assert.Equal("FourByte", result.GetValue("SetFanMaxDeveloperExperimentPayload"));
            Assert.Equal("false", result.GetValue("SetFanMaxNormalControlValidated"));
            Assert.Equal("false", result.GetValue("SetFanMaxUserFacingControlAllowed"));
            Assert.Equal("false", result.GetValue("SetFanMaxWriteImplemented"));
            Assert.Equal("false", result.GetValue("SetFanMaxWriteAllowed"));
            Assert.Null(result.GetValue("SetFanMaxDeviceValidatedInputLength"));
            Assert.Equal("false", result.GetValue("SystemDesignDataInvocationAttempted"));
            Assert.Equal("false", result.GetValue("FanGetCountInvocationAttempted"));
            Assert.Equal("false", result.GetValue("FanMaxGetInvocationAttempted"));
            Assert.Equal("false", result.GetValue("FanGetLevelInvocationAttempted"));
        }
        finally
        {
            File.Delete(reportPath);
            File.Delete(reportPath + ".bak");
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("{\"ReportSchemaVersion\":1")]
    public void RefreshExistingReport_InvalidOrEmptyReportFailsClosed(string contents)
    {
        string reportPath = WriteTemporaryReport(contents);
        try
        {
            Assert.False(HpDiagnosticReportSchemaV2Refresher.TryRefreshExistingReport(reportPath));
        }
        finally
        {
            File.Delete(reportPath);
        }
    }

    [Fact]
    public void Loader_HasNoWmiDependencyOrInvocationSurface()
    {
        Type[] types = [typeof(HpDiagnosticReportLoader), typeof(HpDiagnosticReportLoadResult)];

        Assert.DoesNotContain(
            types.SelectMany(type => type.GetMethods()),
            method => method.Name.Contains("Invoke", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            types.SelectMany(type => type.GetMethods()).SelectMany(method => method.GetParameters()),
            parameter => parameter.ParameterType.Namespace?.Contains("Management", StringComparison.Ordinal) == true);
    }

    private static string WriteTemporaryReport(string contents)
    {
        string reportPath = Path.Combine(Path.GetTempPath(), "VictusX-" + Guid.NewGuid() + ".json");
        File.WriteAllText(reportPath, contents);
        return reportPath;
    }
}
