using System.Diagnostics;
using System.Text.RegularExpressions;
using Xunit;

namespace VictusX.Tests.Packaging;

public sealed class PreviewPackageVerificationScriptTests
{
    private static readonly string[] LicenseFiles =
    [
        "FftSharp-LICENSE.txt",
        "GPL-3.0.txt",
        "HidSharpCore-LICENSE.txt",
        "HidSharpCore-NOTICE.txt",
        "LICENSE-SOURCES.md",
        "Microsoft.NET-10.0.11-win-x64-RUNTIME-EVIDENCE.md",
        "Microsoft.NETCore.App.Runtime.win-x64-10.0.11-LICENSE.TXT",
        "Microsoft.NETCore.App.Runtime.win-x64-10.0.11-THIRD-PARTY-NOTICES.TXT",
        "Microsoft.WindowsDesktop.App.Runtime.win-x64-10.0.11-LICENSE.TXT",
        "NAudio-LICENSE.txt",
        "NvAPIWrapper-LGPL-3.0.txt",
        "NvAPIWrapper-README.txt",
        "System.Management-LICENSE.txt",
        "System.Management-THIRD-PARTY-NOTICES.txt",
        "TaskScheduler-LICENSE.txt",
        "WinForms.DataVisualization-LICENSE.txt"
    ];

    [Fact]
    public void Inspector_IsReadOnlyAndRequiresAnExplicitDirectory()
    {
        string script = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(), "tools", "verify-victusx-preview-package.ps1"));

        Assert.Contains("[Parameter(Mandatory = $true)]", script, StringComparison.Ordinal);
        Assert.Contains("[string]$PublishDirectory", script, StringComparison.Ordinal);
        foreach (string forbidden in new[]
        {
            "Start-Process", "Remove-Item", "Copy-Item", "Move-Item", "Set-Content", "Out-File",
            "Set-AuthenticodeSignature", "signtool", "dotnet publish", "hpqBIOSInt", "FanGetLevel", "DeviceSet("
        })
        {
            Assert.DoesNotContain(forbidden, script, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void MissingRequiredFile_IsNoGo()
    {
        using var fixture = PreviewPackageFixture.Create();
        File.Delete(Path.Combine(fixture.DirectoryPath, "LICENSE"));

        ScriptResult result = RunInspector(fixture.DirectoryPath);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("FAIL required-files: missing: LICENSE", result.Output, StringComparison.Ordinal);
        Assert.EndsWith("Preview package: NO-GO", result.Output.TrimEnd(), StringComparison.Ordinal);
    }

    [Fact]
    public void ForbiddenSourceArtifact_IsNoGo()
    {
        using var fixture = PreviewPackageFixture.Create();
        File.WriteAllText(Path.Combine(fixture.DirectoryPath, "accidental-source.cs"), "// must not ship");

        ScriptResult result = RunInspector(fixture.DirectoryPath);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("FAIL source-artifacts: accidental-source.cs", result.Output, StringComparison.Ordinal);
        Assert.EndsWith("Preview package: NO-GO", result.Output.TrimEnd(), StringComparison.Ordinal);
    }

    [Fact]
    public void NvApiWrapperOutsidePackageRoot_IsNoGo()
    {
        using var fixture = PreviewPackageFixture.Create();
        string sidecar = Path.Combine(fixture.DirectoryPath, "NvAPIWrapper.dll");
        string nestedDirectory = Path.Combine(fixture.DirectoryPath, "runtime");
        Directory.CreateDirectory(nestedDirectory);
        File.Move(sidecar, Path.Combine(nestedDirectory, "NvAPIWrapper.dll"));

        ScriptResult result = RunInspector(fixture.DirectoryPath);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("FAIL nvapi-sidecar: NvAPIWrapper.dll must exist exactly once beside VictusX.exe", result.Output, StringComparison.Ordinal);
        Assert.EndsWith("Preview package: NO-GO", result.Output.TrimEnd(), StringComparison.Ordinal);
    }

    [Fact]
    public void NoticeContentMismatch_IsNoGo()
    {
        using var fixture = PreviewPackageFixture.Create();
        File.AppendAllText(Path.Combine(fixture.DirectoryPath, "THIRD-PARTY-NOTICES.md"), "modified");

        ScriptResult result = RunInspector(fixture.DirectoryPath);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("FAIL notice-matching: content mismatch: THIRD-PARTY-NOTICES.md", result.Output, StringComparison.Ordinal);
        Assert.EndsWith("Preview package: NO-GO", result.Output.TrimEnd(), StringComparison.Ordinal);
    }

    [Fact]
    public void MissingReviewedDependencyEntry_IsNoGo()
    {
        using var fixture = PreviewPackageFixture.Create();
        string noticesPath = Path.Combine(fixture.DirectoryPath, "THIRD-PARTY-NOTICES.md");
        File.WriteAllText(
            noticesPath,
            File.ReadAllText(noticesPath).Replace("| FftSharp | 2.2.0 | Direct |", "| FftSharp | omitted |", StringComparison.Ordinal));

        ScriptResult result = RunInspector(fixture.DirectoryPath);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("FAIL notice-inventory: missing reviewed evidence: | FftSharp | 2.2.0 | Direct |", result.Output, StringComparison.Ordinal);
        Assert.EndsWith("Preview package: NO-GO", result.Output.TrimEnd(), StringComparison.Ordinal);
    }

    [Fact]
    public void MissingRuntimeNotice_IsNoGo()
    {
        using var fixture = PreviewPackageFixture.Create();
        File.Delete(Path.Combine(fixture.DirectoryPath, "Assets", "Licenses", "Microsoft.NETCore.App.Runtime.win-x64-10.0.11-THIRD-PARTY-NOTICES.TXT"));

        ScriptResult result = RunInspector(fixture.DirectoryPath);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("FAIL required-files: missing: Assets/Licenses/Microsoft.NETCore.App.Runtime.win-x64-10.0.11-THIRD-PARTY-NOTICES.TXT", result.Output, StringComparison.Ordinal);
        Assert.EndsWith("Preview package: NO-GO", result.Output.TrimEnd(), StringComparison.Ordinal);
    }

    [Fact]
    public void ModifiedRuntimeNotice_IsNoGo()
    {
        using var fixture = PreviewPackageFixture.Create();
        File.AppendAllText(Path.Combine(fixture.DirectoryPath, "Assets", "Licenses", "Microsoft.NETCore.App.Runtime.win-x64-10.0.11-LICENSE.TXT"), "modified");

        ScriptResult result = RunInspector(fixture.DirectoryPath);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("FAIL notice-matching: content mismatch: Assets/Licenses/Microsoft.NETCore.App.Runtime.win-x64-10.0.11-LICENSE.TXT", result.Output, StringComparison.Ordinal);
        Assert.EndsWith("Preview package: NO-GO", result.Output.TrimEnd(), StringComparison.Ordinal);
    }

    [Fact]
    public void WrongRuntimeIdentity_IsNoGo()
    {
        using var fixture = PreviewPackageFixture.Create();
        string evidencePath = Path.Combine(fixture.DirectoryPath, "Assets", "Licenses", "Microsoft.NET-10.0.11-win-x64-RUNTIME-EVIDENCE.md");
        File.WriteAllText(evidencePath, File.ReadAllText(evidencePath).Replace("10.0.11", "10.0.10", StringComparison.Ordinal));

        ScriptResult result = RunInspector(fixture.DirectoryPath);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("FAIL runtime-notices: expected 10.0.11 win-x64 runtime evidence is incomplete or mismatched.", result.Output, StringComparison.Ordinal);
        Assert.EndsWith("Preview package: NO-GO", result.Output.TrimEnd(), StringComparison.Ordinal);
    }

    [Fact]
    public void ValidLayout_ProducesDeterministicChecksumsAndAutomatedGo()
    {
        using var fixture = PreviewPackageFixture.Create();

        ScriptResult first = RunInspector(fixture.DirectoryPath);
        ScriptResult second = RunInspector(fixture.DirectoryPath);

        Assert.True(first.ExitCode == 0, first.Output);
        Assert.Equal(first.Output, second.Output);
        Assert.Matches(new Regex(@"PASS checksum-evidence: \d+ files; deterministic manifest SHA256 [A-F0-9]{64}\."), first.Output);
        Assert.Contains("PASS notice-matching", first.Output, StringComparison.Ordinal);
        Assert.Contains("PASS notice-inventory", first.Output, StringComparison.Ordinal);
        Assert.Contains("PASS runtime-notices", first.Output, StringComparison.Ordinal);
        Assert.Contains("WARN manual-release-evidence", first.Output, StringComparison.Ordinal);
        Assert.EndsWith("Preview package: GO", first.Output.TrimEnd(), StringComparison.Ordinal);
    }

    private static ScriptResult RunInspector(string packageDirectory)
    {
        string repositoryRoot = FindRepositoryRoot();
        var startInfo = new ProcessStartInfo("powershell.exe")
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-ExecutionPolicy");
        startInfo.ArgumentList.Add("Bypass");
        startInfo.ArgumentList.Add("-File");
        startInfo.ArgumentList.Add(Path.Combine(repositoryRoot, "tools", "verify-victusx-preview-package.ps1"));
        startInfo.ArgumentList.Add("-PublishDirectory");
        startInfo.ArgumentList.Add(packageDirectory);

        using Process process = Process.Start(startInfo) ?? throw new InvalidOperationException("PowerShell did not start.");
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return new ScriptResult(process.ExitCode, output + error);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "VictusX.sln")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }

    private sealed class PreviewPackageFixture : IDisposable
    {
        private PreviewPackageFixture(string directoryPath) => DirectoryPath = directoryPath;

        public string DirectoryPath { get; }

        public static PreviewPackageFixture Create()
        {
            string repositoryRoot = FindRepositoryRoot();
            string directoryPath = Path.Combine(Path.GetTempPath(), "VictusX.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directoryPath);
            File.WriteAllBytes(Path.Combine(directoryPath, "VictusX.exe"), [0x4D, 0x5A, 0x00, 0x00]);
            File.Copy(Path.Combine(repositoryRoot, "app", "app.config"), Path.Combine(directoryPath, "VictusX.dll.config"));
            File.WriteAllText(Path.Combine(directoryPath, "NvAPIWrapper.dll"), "fixture sidecar");
            File.Copy(Path.Combine(repositoryRoot, "LICENSE"), Path.Combine(directoryPath, "LICENSE"));
            File.Copy(Path.Combine(repositoryRoot, "THIRD-PARTY-NOTICES.md"), Path.Combine(directoryPath, "THIRD-PARTY-NOTICES.md"));

            string packageTools = Path.Combine(directoryPath, "tools");
            Directory.CreateDirectory(packageTools);
            File.Copy(
                Path.Combine(repositoryRoot, "tools", "run-victusx-hp-diagnostic.ps1"),
                Path.Combine(packageTools, "run-victusx-hp-diagnostic.ps1"));

            string packageLicenses = Path.Combine(directoryPath, "Assets", "Licenses");
            Directory.CreateDirectory(packageLicenses);
            foreach (string name in LicenseFiles)
            {
                File.Copy(
                    Path.Combine(repositoryRoot, "app", "Assets", "Licenses", name),
                    Path.Combine(packageLicenses, name));
            }

            return new PreviewPackageFixture(directoryPath);
        }

        public void Dispose()
        {
            if (Directory.Exists(DirectoryPath)) Directory.Delete(DirectoryPath, recursive: true);
        }
    }

    private sealed record ScriptResult(int ExitCode, string Output);
}
