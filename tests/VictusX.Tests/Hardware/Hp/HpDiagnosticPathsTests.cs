using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpDiagnosticPathsTests
{
    [Fact]
    public void Paths_UseOneVictusXAppDataRoot()
    {
        string root = Path.Combine("C:", "AppData", "Roaming");
        string appDataDirectory = HpDiagnosticPaths.BuildAppDataDirectory(root);

        Assert.Equal(Path.Combine(root, "VictusX"), appDataDirectory);
        Assert.Equal(Path.Combine(appDataDirectory, "hp-capability-report.json"), HpDiagnosticPaths.BuildCapabilityReportPath(appDataDirectory));
        Assert.Equal(Path.Combine(appDataDirectory, "Logs", "Reports"), HpDiagnosticPaths.BuildExportDirectory(appDataDirectory));
    }

    [Fact]
    public void StatusText_KeepsReadOnlyAndNoGoMessagesConsistent()
    {
        Assert.Equal("Report not available", HpDiagnosticStatusText.ReportNotAvailable);
        Assert.Equal("Report could not be read", HpDiagnosticStatusText.ReportCouldNotBeRead);
        Assert.Equal("Some fields are not available", HpDiagnosticStatusText.SomeFieldsNotAvailable);
        Assert.Contains(HpDiagnosticStatusText.FanControlNotImplemented, HpDiagnosticStatusText.SafetyWarning, StringComparison.Ordinal);
        Assert.Contains(HpDiagnosticStatusText.SetFanMaxNoGo, HpDiagnosticStatusText.SafetyWarning, StringComparison.Ordinal);
    }
}
