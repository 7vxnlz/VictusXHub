using Xunit;

namespace VictusXHub.Tests.Hardware.Hp;

public sealed class HpDiagnosticPreviewConfigurationTests
{
    [Fact]
    public void HpReadOnlyDiscovery_DoesNotDependOnRestrictedMmiRuntime()
    {
        string project = ReadRepositoryFile("app", "VictusXHub.csproj");
        string capabilityProbe = ReadRepositoryFile("app", "Hardware", "Hp", "HpVictusCapabilityProbe.cs");
        string wmiClient = ReadRepositoryFile("app", "Hardware", "Hp", "HpWmiReadOnlyClient.cs");
        string accessDiagnostics = ReadRepositoryFile("app", "Hardware", "Hp", "HpWmiAccessDeniedDiagnostics.cs");

        Assert.DoesNotContain("Microsoft.Management.Infrastructure", project, StringComparison.Ordinal);
        Assert.Contains("System.Management", project, StringComparison.Ordinal);
        Assert.DoesNotContain("HpCimReadinessProbe", capabilityProbe, StringComparison.Ordinal);
        Assert.Contains("new HpWmiReadOnlyClient().Probe()", capabilityProbe, StringComparison.Ordinal);
        Assert.Contains("HpWmiAccessDeniedDiagnostics.Probe()", capabilityProbe, StringComparison.Ordinal);
        Assert.DoesNotContain("Microsoft.Management.Infrastructure", capabilityProbe + wmiClient + accessDiagnostics, StringComparison.Ordinal);
        Assert.Contains("ManagementClass", wmiClient + accessDiagnostics, StringComparison.Ordinal);
    }

    [Fact]
    public void HpStartup_AllowsOnlyThePinnedSystemDesignDataFanGetCountAndExactKeyboardStatusReads()
    {
        string catalog = ReadRepositoryFile("app", "Hardware", "Hp", "HpBiosWmiCommandCatalog.cs");
        string client = ReadRepositoryFile("app", "Hardware", "Hp", "HpWmiInvocationClient.cs");
        string probe = ReadRepositoryFile("app", "Hardware", "Hp", "HpVictusCapabilityProbe.cs");

        Assert.Contains("\"SystemDesignData\",", catalog, StringComparison.Ordinal);
        Assert.Contains("0x28", catalog, StringComparison.Ordinal);
        Assert.Contains("\"hpqBIOSInt128\"", catalog, StringComparison.Ordinal);
        Assert.Contains("0,\n            128,", catalog.Replace("\r\n", "\n"), StringComparison.Ordinal);
        Assert.Contains("HpBiosWmiCommandAccess.ReadOnly", catalog, StringComparison.Ordinal);
        Assert.Contains("HpBiosWmiCommandSafety.SafeReadOnlyInvocation", catalog, StringComparison.Ordinal);
        Assert.Contains("IsApprovedStartupSystemDesignData", client, StringComparison.Ordinal);
        Assert.Contains("AllowSystemDesignDataAtStartup", client, StringComparison.Ordinal);
        Assert.Contains("IsApprovedStartupFanGetCount", client, StringComparison.Ordinal);
        Assert.Contains("AllowFanGetCountAtStartup", client, StringComparison.Ordinal);
        Assert.Contains("IsApprovedStartupKeyboardStatus", client, StringComparison.Ordinal);
        Assert.Contains("AllowKeyboardStatusAtStartup", client, StringComparison.Ordinal);
        Assert.Contains("ExactKeyboardStatusDeviceGateAccepted", client, StringComparison.Ordinal);
        Assert.Contains("definition.CommandId == 0x28", client, StringComparison.Ordinal);
        Assert.Contains("definition.ExpectedOutputSize == 128", client, StringComparison.Ordinal);
        Assert.Contains("\"FanGetCount\",", catalog, StringComparison.Ordinal);
        Assert.Contains("0x10", catalog, StringComparison.Ordinal);
        Assert.Contains("\"hpqBIOSInt4\"", catalog, StringComparison.Ordinal);
        Assert.Contains("4,\n            4,", catalog.Replace("\r\n", "\n"), StringComparison.Ordinal);
        Assert.Contains("definition.CommandId == 0x10", client, StringComparison.Ordinal);
        Assert.Contains("definition.ExpectedInputSize == 4", client, StringComparison.Ordinal);
        Assert.Contains("definition.ExpectedOutputSize == 4", client, StringComparison.Ordinal);
        Assert.Contains("AllowSystemDesignDataAtStartup: true", probe, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(probe, "AllowSystemDesignDataAtStartup: true"));
        Assert.Contains("AllowFanGetCountAtStartup: true", probe, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(probe, "AllowFanGetCountAtStartup: true"));
        Assert.Contains("var fanGetCountInvocation = TryInvokeFanGetCount(", probe, StringComparison.Ordinal);
        Assert.Contains("var keyboardStatusInvocation = TryInvokeKeyboardStatus(", probe, StringComparison.Ordinal);
        Assert.Contains("AllowKeyboardStatusAtStartup: true", probe, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(probe, "AllowKeyboardStatusAtStartup: true"));
        Assert.DoesNotContain("AllowFanGetLevelAtStartup", client + probe, StringComparison.Ordinal);
        Assert.DoesNotContain("AllowFanMaxGetAtStartup", client + probe, StringComparison.Ordinal);

        string dashboard = ReadRepositoryFile("app", "Hardware", "Hp", "HpDiagnosticDashboardFormatter.cs");
        Assert.Contains("SystemDesignData, FanGetCount, and exact-device KeyboardStatus requests", dashboard, StringComparison.Ordinal);
        Assert.DoesNotContain("FanGetLevel request", dashboard, StringComparison.Ordinal);
    }

    [Fact]
    public void Launcher_UsesOnlyTheSafeHpVictusArgument()
    {
        string launcher = ReadRepositoryFile("tools", "run-victusxhub-hp-diagnostic.ps1");

        Assert.Contains("-ArgumentList \"--hp-victus\"", launcher, StringComparison.Ordinal);
        Assert.DoesNotContain("--hp-wmi-readonly-test", launcher, StringComparison.Ordinal);
    }

    [Fact]
    public void PublishProfile_DoesNotContainDeveloperOnlyWmiTestArgument()
    {
        string profile = ReadRepositoryFile(
            "app",
            "Properties",
            "PublishProfiles",
            "VictusXHub-HP-Diagnostic-win-x64.pubxml");

        Assert.Contains("<SelfContained>true</SelfContained>", profile, StringComparison.Ordinal);
        Assert.DoesNotContain("--hp-wmi-readonly-test", profile, StringComparison.Ordinal);
    }

    [Fact]
    public void PublishProfile_IsFailClosedAndPackagesRequiredExternalFiles()
    {
        string profile = ReadRepositoryFile(
            "app",
            "Properties",
            "PublishProfiles",
            "VictusXHub-HP-Diagnostic-win-x64.pubxml");
        string project = ReadRepositoryFile("app", "VictusXHub.csproj");

        Assert.Contains("<RuntimeIdentifier>win-x64</RuntimeIdentifier>", profile, StringComparison.Ordinal);
        Assert.Contains("<SelfContained>true</SelfContained>", profile, StringComparison.Ordinal);
        Assert.Contains("<PublishSingleFile>true</PublishSingleFile>", profile, StringComparison.Ordinal);
        Assert.Contains("<PublishTrimmed>false</PublishTrimmed>", profile, StringComparison.Ordinal);
        Assert.Contains("<DebugType>None</DebugType>", profile, StringComparison.Ordinal);
        Assert.Contains("<DebugSymbols>false</DebugSymbols>", profile, StringComparison.Ordinal);
        Assert.Contains("<CopyOutputSymbolsToPublishDirectory>false</CopyOutputSymbolsToPublishDirectory>", profile, StringComparison.Ordinal);
        Assert.Contains("ValidateVictusXHubHpPreviewPublishInputs", profile, StringComparison.Ordinal);
        Assert.Contains("!Exists('$(MSBuildProjectDirectory)\\Assets\\VictusXHub.ico')", profile, StringComparison.Ordinal);

        Assert.Contains("..\\LICENSE", project, StringComparison.Ordinal);
        Assert.Contains("..\\THIRD-PARTY-NOTICES.md", project, StringComparison.Ordinal);
        Assert.Contains("..\\tools\\run-victusxhub-hp-diagnostic.ps1", project, StringComparison.Ordinal);
        Assert.Contains("<CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>", project, StringComparison.Ordinal);
        Assert.Contains("<ExcludeFromSingleFile>true</ExcludeFromSingleFile>", project, StringComparison.Ordinal);
    }

    [Fact]
    public void PublishProfile_OptsOutOfTheInheritedZipOnPublishTarget()
    {
        string profile = ReadRepositoryFile(
            "app",
            "Properties",
            "PublishProfiles",
            "VictusXHub-HP-Diagnostic-win-x64.pubxml");
        string project = ReadRepositoryFile("app", "VictusXHub.csproj");

        Assert.Contains("<SkipLegacySingleExeZip>true</SkipLegacySingleExeZip>", profile, StringComparison.Ordinal);
        Assert.Contains("'$(SkipLegacySingleExeZip)'!='true'", project, StringComparison.Ordinal);
    }

    [Fact]
    public void PublishProfile_KeepsNvApiWrapperAsAReplaceableExternalLibrary()
    {
        string profile = ReadRepositoryFile(
            "app",
            "Properties",
            "PublishProfiles",
            "VictusXHub-HP-Diagnostic-win-x64.pubxml");
        string project = ReadRepositoryFile("app", "VictusXHub.csproj");
        string temperatureSource = ReadRepositoryFile("app", "Hardware", "Hp", "HpNvidiaTemperatureSource.cs");

        Assert.Contains("<PublishSingleFile>true</PublishSingleFile>", profile, StringComparison.Ordinal);
        Assert.Contains("KeepNvApiWrapperReplaceable", profile, StringComparison.Ordinal);
        Assert.Contains("NvAPIWrapper.dll", profile, StringComparison.Ordinal);
        Assert.Contains("<ExcludeFromSingleFile>true</ExcludeFromSingleFile>", profile, StringComparison.Ordinal);
        Assert.Contains("<ResolvedFileToPublish Update=\"@(ResolvedFileToPublish)\"", profile, StringComparison.Ordinal);
        Assert.DoesNotContain("_NvApiWrapperPublishFile", profile, StringComparison.Ordinal);
        Assert.Contains("replaceable external library", profile, StringComparison.Ordinal);
        Assert.Contains("<PackageReference Include=\"NvAPIWrapper.Net\" Version=\"0.8.1.101\"", project, StringComparison.Ordinal);
        Assert.Contains("GPUApi.GetThermalSettings", temperatureSource, StringComparison.Ordinal);
        Assert.Contains("ThermalSettingsTarget.GPU", temperatureSource, StringComparison.Ordinal);
    }

    [Fact]
    public void SettingsUi_HasNoSetFanMaxExperimentRoute()
    {
        string settings = ReadRepositoryFile("app", "Settings.cs");

        Assert.DoesNotContain("HpFanMaxExperimentRunner", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("HpFanMaxExperimentWmiTransport", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("--hp-fan-write-experiment", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("HpFanMaxPulseCommand", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("--hp-fan-max-pulse", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("HpFanMaxHoldCommand", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("--hp-fan-max-hold", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("HpWmiInvocationClient", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("HpFanLevelResearch", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("--hp-fan-level-research-dry-run", settings, StringComparison.Ordinal);
    }

    [Fact]
    public void FanLevelDryRun_ExitsBeforeAllHardwareStartupAndHasNoUiOrTransportDependency()
    {
        string program = ReadRepositoryFile("app", "Program.cs").Replace("\r\n", "\n");
        Assert.DoesNotContain("HpFanRpmEcProbeCommand", program, StringComparison.Ordinal);
        Assert.DoesNotContain("--hp-fan-rpm-ec-readonly-probe", program, StringComparison.Ordinal);
        int probeRoute = program.IndexOf("HpRyzenTemperatureProbeCommand.TryRun(args)", StringComparison.Ordinal);
        int fanRoute = program.IndexOf("TryRunHpFanLevelResearchDryRun(args)", StringComparison.Ordinal);
        int applicationStartup = program.IndexOf("MainCore(args)", StringComparison.Ordinal);
        Assert.True(probeRoute >= 0 && fanRoute > probeRoute && applicationStartup > fanRoute,
            "Both bounded diagnostic command routes must exit before normal application startup.");
        int start = program.IndexOf("private static bool TryRunHpFanLevelResearchDryRun", StringComparison.Ordinal);
        int end = program.IndexOf("private static bool TryRunHpFanMaxHold", start, StringComparison.Ordinal);
        string route = program[start..end];
        Assert.Contains("Console.WriteLine(result.Record!.ToJson());", route, StringComparison.Ordinal);
        Assert.Contains("Environment.ExitCode = result.IsValidRequest ? 0 : 2;", route, StringComparison.Ordinal);
        Assert.True(route.IndexOf("HpFanLevelResearchDryRunLogWriter.Write", StringComparison.Ordinal) < route.IndexOf("Console.WriteLine", StringComparison.Ordinal));
        Assert.Contains("Environment.ExitCode = 1;", route, StringComparison.Ordinal);
        Assert.DoesNotContain("TryRunHpFanLevelResearchDryRun", program[end..], StringComparison.Ordinal);

        string source = ReadRepositoryFile("app", "Hardware", "Hp", "HpFanLevelResearchDryRunCommand.cs");
        source += ReadRepositoryFile("app", "Hardware", "Hp", "HpFanLevelResearchDryRunLogWriter.cs");
        string[] forbidden =
        [
            "System.Management", "Microsoft.Management", "DllImport", "LibraryImport", "Process.Start",
            "HpWmiInvocationClient", "HpFanMaxExperiment", "IHpFanResearchOperation", "HpBiosWmiCommandCatalog",
            "PawnIO", "PwnIO", "WinRing0", "EmbeddedController", "LibreHardwareMonitor", "0x37",
            "SetFanMode", "System.Windows.Forms", "Slider", "Toggle", "Button", "Task.Run", "Timer"
        ];
        foreach (string term in forbidden)
        {
            Assert.DoesNotContain(term, source + route, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void DeveloperHoldRoute_HasNoLowLevelOrGenericFanFallbackSurface()
    {
        string source = string.Join(
            Environment.NewLine,
            ReadRepositoryFile("app", "Hardware", "Hp", "HpFanMaxHoldCommand.cs"),
            ReadRepositoryFile("app", "Hardware", "Hp", "HpFanMaxExperimentRunner.cs"),
            ReadRepositoryFile("app", "Hardware", "Hp", "HpFanMaxExperimentRuntime.cs"));
        string[] forbiddenTerms =
        [
            "PawnIO",
            "PwnIO",
            "WinRing0",
            "LibreHardwareMonitor",
            "EmbeddedController",
            "SetFanMode",
            "SetFanLevel",
            "0x37"
        ];

        foreach (string forbiddenTerm in forbiddenTerms)
        {
            Assert.DoesNotContain(forbiddenTerm, source, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void HpDiagnosticQuit_UsesDedicatedUiThreadShellExit()
    {
        string settings = ReadRepositoryFile("app", "Settings.cs");
        string program = ReadRepositoryFile("app", "Program.cs");

        Assert.Contains("Program.ExitHpDiagnosticShell();", settings, StringComparison.Ordinal);
        Assert.Contains("internal static void ExitHpDiagnosticShell()", program, StringComparison.Ordinal);
        Assert.Contains("Application.ExitThread();", program, StringComparison.Ordinal);
        Assert.Contains("if (!hpVictusMode)", program, StringComparison.Ordinal);
    }

    [Fact]
    public void HpDiagnosticDashboard_UsesScrollableContentWithinWorkingAreaBounds()
    {
        string settings = ReadRepositoryFile("app", "Settings.cs");

        Assert.Contains("AutoScroll = true", settings, StringComparison.Ordinal);
        Assert.Contains("scrollHost.VerticalScroll.Visible = false;", settings, StringComparison.Ordinal);
        Assert.Contains("scrollHost.HorizontalScroll.Visible = false;", settings, StringComparison.Ordinal);
        Assert.Contains("Dock = DockStyle.Fill", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("ConfigureHpDiagnosticWindowBounds", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("AddHpReadOnlyMainShell", settings, StringComparison.Ordinal);
        Assert.Contains("Text = \"Diagnostic\"", settings, StringComparison.Ordinal);
        Assert.Contains("EnsureHpDiagnosticForm();", settings, StringComparison.Ordinal);
        Assert.Contains("PositionHpDiagnosticForm();", settings, StringComparison.Ordinal);
        Assert.Contains("hpDiagnosticForm?.Hide();", settings, StringComparison.Ordinal);
        Assert.Contains("HpFanMaxPulseHistoryLoader.Load", settings, StringComparison.Ordinal);
        Assert.Contains("HpFanProofGapAnalyzer.Analyze", settings, StringComparison.Ordinal);
        Assert.Contains("AccessibleName = \"User-facing diagnostic summary\"", settings, StringComparison.Ordinal);
        Assert.Contains("Show Advanced / Developer diagnostics", settings, StringComparison.Ordinal);
        Assert.Contains("Visible = false", settings, StringComparison.Ordinal);
        Assert.Contains("advancedPanel.Controls.Add(details);", settings, StringComparison.Ordinal);
        Assert.Contains("advancedPanel.Controls.Add(hpLiveTelemetrySummary);", settings, StringComparison.Ordinal);
        Assert.Contains("advancedPanel.Controls.Add(hpReadOnlyTelemetryHealth);", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("hpReadOnlyTelemetryWarning", settings, StringComparison.Ordinal);
        Assert.Contains("HpDiagnosticDashboardFormatter.BuildCompleteSummary", settings, StringComparison.Ordinal);
        Assert.Contains("HpDiagnosticDashboardStatus.Warning => Color.Orange", settings, StringComparison.Ordinal);
        Assert.Contains("HpDiagnosticDashboardStatus.Blocked => colorTurbo", settings, StringComparison.Ordinal);
        Assert.Contains("if (!AppConfig.IsHpVictusHardwareMode()) return;", settings, StringComparison.Ordinal);
    }

    [Fact]
    public void HpDiagnosticRuntimeActions_UseStaClipboardAndOmitRedundantMainButton()
    {
        string settings = ReadRepositoryFile("app", "Settings.cs");
        string program = ReadRepositoryFile("app", "Program.cs").Replace("\r\n", "\n");

        Assert.Contains("[STAThread]\n        public static void Main(string[] args)", program, StringComparison.Ordinal);
        Assert.Contains("Clipboard.SetText(BuildHpDiagnosticSummary());", settings, StringComparison.Ordinal);
        Assert.Contains("HpDiagnosticDashboardFormatter.BuildCompleteSummary", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("CreateHpDiagnosticActionButton(\"Main\"", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("ButtonHpDiagnosticMain_Click", settings, StringComparison.Ordinal);
    }

    [Fact]
    public void HpDiagnosticLiveSummary_UpdatesValueLabelsWithoutRebuildingTheTable()
    {
        string settings = ReadRepositoryFile("app", "Settings.cs");
        int start = settings.IndexOf("private void PopulateHpUserDiagnosticSummary", StringComparison.Ordinal);
        int end = settings.IndexOf("private void UpdateHpDiagnosticHealthSummary", start, StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start);
        string updateMethod = settings[start..end];

        Assert.Contains("hpUserDiagnosticValueLabels", updateMethod, StringComparison.Ordinal);
        Assert.Contains("TryGetValue", updateMethod, StringComparison.Ordinal);
        Assert.Contains("if (!string.Equals(valueLabel.Text, row.Value, StringComparison.Ordinal))", updateMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("Controls.Clear()", updateMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("RowStyles.Clear()", updateMethod, StringComparison.Ordinal);
    }

    [Fact]
    public void HpMode_UsesDisabledInheritedShellWithFooterDiagnosticAction()
    {
        string settings = ReadRepositoryFile("app", "Settings.cs");

        Assert.Contains("hpMainShellPanel = panelPerformance;", settings, StringComparison.Ordinal);
        Assert.Contains("panelPerformance,", settings, StringComparison.Ordinal);
        Assert.Contains("panelGPU,", settings, StringComparison.Ordinal);
        Assert.Contains("panelScreen,", settings, StringComparison.Ordinal);
        Assert.Contains("panelKeyboard,", settings, StringComparison.Ordinal);
        Assert.Contains("panelBattery", settings, StringComparison.Ordinal);
        Assert.Contains("section.Enabled = true;", settings, StringComparison.Ordinal);
        Assert.Contains("ConfigureHpReadOnlySection(section);", settings, StringComparison.Ordinal);
        Assert.Contains("control.ForeColor = foreMain;", settings, StringComparison.Ordinal);
        Assert.Contains("control is RButton or ComboBox or Slider or CheckBox or PictureBox", settings, StringComparison.Ordinal);
        Assert.Contains("control.AccessibleDescription = \"Unavailable in HP mode.\";", settings, StringComparison.Ordinal);
        Assert.Contains("toolTip.SetToolTip(control, \"Unavailable in HP mode.\");", settings, StringComparison.Ordinal);
        Assert.Contains("labelGPU.Text = $\"GPU Mode: {gpuMode.CapabilityText}\";", settings, StringComparison.Ordinal);
        Assert.Contains("labelBacklight.Text = $\"Keyboard Lighting: {keyboard.CapabilityText}\";", settings, StringComparison.Ordinal);
        Assert.Contains("labelCharge.Text = $\"Battery Care: {display.BatteryCareCapability}\";", settings, StringComparison.Ordinal);
        Assert.Contains("buttonFans.Text = \"Fan Control\\nNO-GO\";", settings, StringComparison.Ordinal);
        Assert.Contains("buttonFans.AccessibleName = \"Fan Control: NO-GO\";", settings, StringComparison.Ordinal);
        Assert.Contains("buttonDonate.Text = AppConfig.IsHpVictusHardwareMode() ? \"Thank You\"", settings, StringComparison.Ordinal);
        Assert.Contains("buttonUpdates.Enabled = false;", settings, StringComparison.Ordinal);
        Assert.Contains("tableButtons.ColumnCount = 4;", settings, StringComparison.Ordinal);
        Assert.Contains("for (int column = 0; column < 4; column++)", settings, StringComparison.Ordinal);
        Assert.Contains("new ColumnStyle(SizeType.Percent, 25F)", settings, StringComparison.Ordinal);
        Assert.Contains("tableButtons.AutoSize = false;", settings, StringComparison.Ordinal);
        Assert.Contains("BackColor = buttonSecond,", settings, StringComparison.Ordinal);
        Assert.Contains("Image = Properties.Resources.icons8_log_32,", settings, StringComparison.Ordinal);
        Assert.Contains("hpDiagnosticFooterButton.FlatAppearance.BorderColor = borderSecond;", settings, StringComparison.Ordinal);
        Assert.Contains("hpDiagnosticFooterButton.Click += ButtonHpDiagnostic_Click;", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("ConfigureHpFooterButtonText", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("CreateHpCompactButtonImage", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("AddHpReadOnlyOverviewPanel", settings, StringComparison.Ordinal);
    }

    [Fact]
    public void HpDiagnostic_UsesOwnedSidePanelLikeUpdatesInsteadOfMainWindowReplacement()
    {
        string settings = ReadRepositoryFile("app", "Settings.cs");

        Assert.Contains("RForm? hpDiagnosticForm;", settings, StringComparison.Ordinal);
        Assert.Contains("Name = \"HpDiagnosticSidePanel\"", settings, StringComparison.Ordinal);
        Assert.Contains("ShowInTaskbar = false", settings, StringComparison.Ordinal);
        Assert.Contains("StartPosition = FormStartPosition.Manual", settings, StringComparison.Ordinal);
        Assert.Contains("AddOwnedForm(hpDiagnosticForm);", settings, StringComparison.Ordinal);
        Assert.Contains("ApplyHpDarkExplorerTheme(scrollHost);", settings, StringComparison.Ordinal);
        Assert.Contains("SetWindowTheme(control.Handle, \"DarkMode_Explorer\", null);", settings, StringComparison.Ordinal);
        Assert.Contains("int left = Left - sideWidth - 5;", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("hpMainShellPanel?.Hide();", settings, StringComparison.Ordinal);
    }

    [Fact]
    public void HpTrayQuickActionsReuseMainAndSingleDiagnosticRoutes()
    {
        string settings = ReadRepositoryFile("app", "Settings.cs");

        Assert.Contains("new ToolStripMenuItem(\"Open VictusXHub\")", settings);
        Assert.Contains("openVictusXHub.Click += (sender, args) => ShowHpReadOnlyMainShell();", settings);
        Assert.Contains("new ToolStripMenuItem(\"Open Diagnostic\")", settings);
        Assert.Contains("openDiagnostic.Click += (sender, args) => ShowHpReadOnlyDiagnostic();", settings);
        Assert.Contains("if (hpDiagnosticForm is not null && !hpDiagnosticForm.IsDisposed) return;", settings);
        Assert.Contains("if (hpDiagnosticForm.Visible)", settings);
        Assert.Contains("hpDiagnosticForm.Activate();", settings);
        Assert.Contains("hpDiagnosticForm.BringToFront();", settings);
        Assert.DoesNotContain("if (hpDiagnosticForm.Visible)\r\n            {\r\n                hpDiagnosticForm.Hide();", settings);

        int hpBranch = settings.IndexOf("if (AppConfig.IsHpVictusHardwareMode())", settings.IndexOf("public void SetContextMenu()", StringComparison.Ordinal), StringComparison.Ordinal);
        int nonHpMenu = settings.IndexOf("var currentMode = Modes.GetCurrent();", hpBranch, StringComparison.Ordinal);
        Assert.True(hpBranch >= 0 && nonHpMenu > hpBranch);
    }

    [Fact]
    public void DisabledButtons_KeepThemeSafeTextAndReserveHorizontalIconSpace()
    {
        string button = ReadRepositoryFile("app", "UI", "RButton.cs");

        Assert.Contains("TextImageRelation == TextImageRelation.ImageAboveText", button, StringComparison.Ordinal);
        Assert.Contains("TextImageRelation == TextImageRelation.ImageBeforeText", button, StringComparison.Ordinal);
        Assert.Contains("int horizontalImageReserve = Image.Width + Padding.Left + 6;", button, StringComparison.Ordinal);
        Assert.Contains("rect.X += horizontalImageReserve;", button, StringComparison.Ordinal);
        Assert.Contains("rect.Width -= horizontalImageReserve;", button, StringComparison.Ordinal);
        Assert.Contains("pevent.Graphics.FillRectangle(brush, ClientRectangle);", button, StringComparison.Ordinal);
        Assert.Contains("pevent.Graphics.DrawImage(Image, imageRect);", button, StringComparison.Ordinal);
        Assert.Contains("else if (Image is null)", button, StringComparison.Ordinal);
        Assert.Contains("Color disabledTextColor = Color.FromArgb(", button, StringComparison.Ordinal);
        Assert.Contains("imageBeforeText ? TextFormatFlags.Left : TextFormatFlags.HorizontalCenter", button, StringComparison.Ordinal);
        Assert.Contains("TextRenderer.DrawText(pevent.Graphics, Text, Font, rect, disabledTextColor, flags);", button, StringComparison.Ordinal);
        Assert.Contains("TextFormatFlags.SingleLine | TextFormatFlags.NoPadding", button, StringComparison.Ordinal);
    }

    [Fact]
    public void HpLiveTelemetry_HasOnlyOsQueriesAndCannotReachInheritedControls()
    {
        string source = ReadRepositoryFile("app", "Hardware", "Hp", "HpWindowsTelemetrySource.cs");
        string provider = ReadRepositoryFile("app", "Hardware", "Hp", "HpReadOnlyTelemetry.cs");
        foreach (string forbidden in new[] { "System.Management", "HpWmi", "hpqBIOSInt", "AsusACPI",
            "HardwareControl", "IHardwareController", "DeviceSet", "Process.Start", "HpFanMaxExperiment", "DeviceValidatedInputLength" })
        {
            Assert.DoesNotContain(forbidden, source, StringComparison.Ordinal);
            Assert.DoesNotContain(forbidden, provider, StringComparison.Ordinal);
        }
        Assert.Contains("GetSystemTimes", source, StringComparison.Ordinal);
        Assert.Contains("GetSystemPowerStatus", source, StringComparison.Ordinal);

        string settings = ReadRepositoryFile("app", "Settings.cs");
        Assert.Contains("ScreenNative.FindUniqueHardwareInternalScreen()", settings, StringComparison.Ordinal);
        Assert.Contains("ScreenNative.GetRefreshRate(laptopScreen)", settings, StringComparison.Ordinal);
        int refreshReaderStart = settings.IndexOf("private static int? ReadHpDisplayRefreshRate", StringComparison.Ordinal);
        int refreshReaderEnd = settings.IndexOf("private void ConfigureHpReadOnlySection", refreshReaderStart, StringComparison.Ordinal);
        string refreshReader = settings.Substring(refreshReaderStart, refreshReaderEnd - refreshReaderStart);
        Assert.DoesNotContain("ScreenControl.SetScreen", refreshReader, StringComparison.Ordinal);
        Assert.Contains("System.Windows.Forms.Timer(components)", settings, StringComparison.Ordinal);
        Assert.Contains("hpLiveTelemetryTimer.Stop();", settings, StringComparison.Ordinal);
        Assert.Contains("hpLiveTelemetryProvider?.Reset();", settings, StringComparison.Ordinal);
        Assert.Contains("labelCPUFan.Click -= LabelCPUFan_Click;", settings, StringComparison.Ordinal);
        Assert.Contains("labelGPUFan.Click -= LabelCPUFan_Click;", settings, StringComparison.Ordinal);
        Assert.Contains("labelBattery.Click -= LabelBattery_Click;", settings, StringComparison.Ordinal);
        Assert.Contains("label.AccessibleRole = AccessibleRole.StaticText;", settings, StringComparison.Ordinal);
        Assert.Contains("if (AppConfig.IsHpVictusHardwareMode()) return;", settings, StringComparison.Ordinal);
    }

    [Fact]
    public void HpRefreshRateControl_UsesOnlyTheUniqueInternalDisplayAndWindowsApi()
    {
        string settings = ReadRepositoryFile("app", "Settings.cs");
        string native = ReadRepositoryFile("app", "Display", "ScreenNative.cs");
        string control = ReadRepositoryFile("app", "Hardware", "Hp", "HpDisplayRefreshRateControl.cs");

        Assert.Contains("ScreenNative.FindUniqueHardwareInternalScreen()", settings);
        Assert.Contains("ScreenNative.GetDisplayModes(displayName)", settings);
        Assert.Contains("ScreenNative.SetRefreshRateValidated(displayName, rate)", settings);
        Assert.Contains("var refreshRate = new ToolStripMenuItem(\"Refresh Rate\")", settings);
        Assert.Contains("HpDisplayRefreshRateControl.BuildMenu(hpDisplayRefreshRateState)", settings);
        Assert.Contains("rate.Click += ButtonHpTrayRefreshRate_Click;", settings);
        Assert.Contains("HpDisplayRefreshRateApplyResult result = ApplyHpRefreshRate(requestedRate);", settings);
        Assert.Contains("BeginInvoke((Action)SetHpReadOnlyContextMenu);", settings);
        Assert.Contains("RefreshHpLiveTelemetry(force: true);", settings);
        Assert.Contains("var currentMode = Modes.GetCurrent();", settings);
        Assert.Contains("contextMenuStrip.Opening += (_, _) => RefreshHpLiveTelemetry(force: true);", settings);
        Assert.Contains("foreach (string row in hpTrayTelemetryStatus.Rows)", settings);
        Assert.Contains("HpTrayIconSelector.FormatModeStatus(HpPerformanceModeStatus.CurrentBaseMode)", settings);
        Assert.Contains("contextMenuStrip.Items.Add(hpTrayModeStatusItem);", settings);
        Assert.Contains("Enabled = false", settings);
        Assert.Contains("UpdateHpTrayStatusItems();", settings);
        Assert.Contains("labelCPUFan.Text = hpTrayTelemetryStatus.Cpu;", settings);
        Assert.Contains("labelGPUFan.Text = hpTrayTelemetryStatus.Gpu;", settings);
        Assert.Contains("labelBattery.Text = hpTrayTelemetryStatus.Battery;", settings);
        Assert.Contains("labelSreen.Text = hpTrayTelemetryStatus.Screen;", settings);
        Assert.Contains("labelPerf.Text = modeStatus;", settings);
        Assert.Contains("buttonScreenAuto.Text = \"Automatic\";", settings);
        Assert.Contains("buttonScreenAuto.Enabled = false;", settings);
        Assert.Contains("button60Hz.Tag = 60;", settings);
        Assert.Contains("button120Hz.Tag = 144;", settings);
        Assert.Contains("button60Hz.Click += ButtonHpRefreshRate_Click;", settings);
        Assert.Contains("button120Hz.Click += ButtonHpRefreshRate_Click;", settings);
        Assert.Contains("tableScreen.ColumnCount = 3;", settings);
        Assert.Contains("tableScreen.SetColumnSpan(buttonScreenAuto, 1);", settings);
        Assert.Contains("button.Visible = hpDisplayRefreshRateState.SupportedRates.Contains(rate);", settings);
        Assert.Contains("button.Activated = button.Visible && currentRateHz == rate;", settings);
        Assert.Contains("buttonScreenAuto.Click -= ButtonScreenAuto_Click;", settings);
        Assert.Contains("button60Hz.Click -= Button60Hz_Click;", settings);
        Assert.Contains("button120Hz.Click -= Button120Hz_Click;", settings);
        Assert.Contains("DisplaySettingsFlags.CDS_TEST", native);
        Assert.Contains("dm.dmFields = DM_DISPLAYFREQUENCY;", native);
        Assert.Contains("DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INTERNAL", native);
        Assert.Contains("DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EMBEDDED", native);
        foreach (string forbidden in new[] { "hpqBIOSInt", "SetFan", "PawnIO", "AsusACPI", "DeviceSet", "0x37" })
        {
            Assert.DoesNotContain(forbidden, control);
            Assert.DoesNotContain(forbidden, native);
        }
    }

    [Fact]
    public void HpGpuTemperature_IsolatedReadDoesNotReachFanOrGpuControls()
    {
        string source = ReadRepositoryFile("app", "Hardware", "Hp", "HpNvidiaTemperatureSource.cs");
        Assert.Contains("GPUApi.GetThermalSettings", source);
        Assert.Contains("ThermalSettingsTarget.GPU", source);
        Assert.Contains("devices.Length != 1", source);
        Assert.Contains("gpuSensors.Length == 1", source);
        foreach (string forbidden in new[] { "hpqBIOSInt", "HpWmi", "SetFan", "PawnIO", "NvidiaGpuControl", "HardwareControl", "GPUApi.Set", "FanGetLevel" })
            Assert.DoesNotContain(forbidden, source);
        string settings = ReadRepositoryFile("app", "Settings.cs");
        Assert.Contains("control.Enabled = false;", settings);
        Assert.Contains("hpLiveTelemetryProvider?.Reset();", settings);
    }

    [Fact]
    public void HpBatteryCareStatus_IsReadOnlyAndCannotEnableTheInheritedControl()
    {
        string source = ReadRepositoryFile("app", "Hardware", "Hp", "HpBatteryCareReadOnlySource.cs");
        string settings = ReadRepositoryFile("app", "Settings.cs");

        Assert.Contains("root\\HP\\InstrumentedBIOS", source, StringComparison.Ordinal);
        Assert.Contains("SELECT Name, CurrentValue FROM HP_BIOSSetting", source, StringComparison.Ordinal);
        Assert.Contains("Adaptive Battery Extender", source, StringComparison.Ordinal);
        Assert.Contains("Adaptive Battery Optimizer", source, StringComparison.Ordinal);
        Assert.Contains("labelCharge.Text = $\"Battery Care: {display.BatteryCareCapability}\";", settings, StringComparison.Ordinal);
        Assert.Contains("labelCharge,", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("labelCharge.Enabled = true", settings, StringComparison.Ordinal);

        foreach (string forbidden in new[]
        {
            "InvokeMethod", "SetBIOSSetting", "HP_BIOSSettingInterface", "hpqBIOSInt", "BatteryCareWrite"
        })
        {
            Assert.DoesNotContain(forbidden, source, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void HpKeyboardBacklightStatus_UsesExactDeviceRawStateAndKeepsControlsDisabled()
    {
        string source = ReadRepositoryFile("app", "Hardware", "Hp", "HpKeyboardBacklightReadOnlyStatus.cs");
        string settings = ReadRepositoryFile("app", "Settings.cs");

        Assert.Contains("HpKeyboardStatusReadOnlyProbeGateResult", source, StringComparison.Ordinal);
        Assert.Contains("0xE4", source, StringComparison.Ordinal);
        Assert.Contains("data.Skip(1).Any(value => value != 0)", source, StringComparison.Ordinal);
        Assert.Contains("Supported, state unavailable", source, StringComparison.Ordinal);
        Assert.Contains("labelBacklight.Text = $\"Keyboard Lighting: {keyboard.CapabilityText}\";", settings, StringComparison.Ordinal);
        Assert.Contains("labelBacklight,", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("labelBacklight.Enabled = true", settings, StringComparison.Ordinal);

        foreach (string forbidden in new[]
        {
            "System.Management", "InvokeMethod", "hpqBIOSInt", "SetBacklight", "SetBrightness",
            "HardwareControl", "AsusACPI", "Aura.Apply"
        })
        {
            Assert.DoesNotContain(forbidden, source, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void HpGpuModeStatus_UsesDecodedCapabilityOnlyAndKeepsControlsDisabled()
    {
        string source = ReadRepositoryFile("app", "Hardware", "Hp", "HpGpuModeReadOnlyStatus.cs");
        string settings = ReadRepositoryFile("app", "Settings.cs");
        int thermalPolicyStart = settings.IndexOf("private static HpCapabilityEvidenceValue GetHpThermalPolicyVersion", StringComparison.Ordinal);
        int thermalPolicyEnd = settings.IndexOf("private static int? ReadHpDisplayRefreshRate", thermalPolicyStart, StringComparison.Ordinal);
        string thermalPolicyResolver = settings[thermalPolicyStart..thermalPolicyEnd];

        Assert.Contains("SystemDesignDataDecoded.GpuModeSwitchRaw", settings, StringComparison.Ordinal);
        Assert.Contains("ResolveThermalPolicy(snapshot.SystemDesignDataDecoded?.ThermalPolicyVersion, null)", thermalPolicyResolver, StringComparison.Ordinal);
        Assert.Contains("ResolveThermalPolicy(null, historical)", thermalPolicyResolver, StringComparison.Ordinal);
        Assert.DoesNotContain("report?.GetBool(\"SystemDesignDataDecodeSucceeded\")", thermalPolicyResolver, StringComparison.Ordinal);
        Assert.Contains("Current startup SystemDesignData", settings, StringComparison.Ordinal);
        Assert.Contains("snapshot.FanGetCountDecoded?.FanCount", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("report?.GetBool(\"FanGetCountDecodeSucceeded\")", settings, StringComparison.Ordinal);
        Assert.Contains("Current startup FanGetCount", settings, StringComparison.Ordinal);
        Assert.Contains("GpuSwitchingCapability = gpuMode.SwitchingCapabilityText", settings, StringComparison.Ordinal);
        Assert.Contains("FanCount = HpReadOnlyTelemetryFormatter.FormatFanCount(fanCount.Value)", settings, StringComparison.Ordinal);
        Assert.Contains("ThermalPolicy = HpReadOnlyTelemetryFormatter.FormatThermalPolicy(thermalPolicyVersion.Value)", settings, StringComparison.Ordinal);
        Assert.Contains("labelGPU.Text = $\"GPU Mode: {gpuMode.CapabilityText}\";", settings, StringComparison.Ordinal);
        Assert.Contains("panelGPU,", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("labelGPU.Enabled = true", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("buttonEco.Enabled = true", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("buttonStandard.Enabled = true", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("buttonUltimate.Enabled = true", settings, StringComparison.Ordinal);

        foreach (string forbidden in new[]
        {
            "System.Management", "InvokeMethod", "hpqBIOSInt", "SetGpuMode", "Process.Start",
            "Application.Restart", "HardwareControl", "AsusACPI"
        })
        {
            Assert.DoesNotContain(forbidden, source, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void HpTrayIdentity_UsesOneFixedVictusXHubIconAndPreservesNonHpRouting()
    {
        string project = ReadRepositoryFile("app", "VictusXHub.csproj");
        string program = ReadRepositoryFile("app", "Program.cs");
        string settings = ReadRepositoryFile("app", "Settings.cs");
        string selector = ReadRepositoryFile("app", "UI", "HpTrayIconSelector.cs");

        Assert.Contains("<ApplicationIcon>Assets\\VictusXHub.ico</ApplicationIcon>", project, StringComparison.Ordinal);
        Assert.DoesNotContain("<ApplicationIcon>favicon.ico</ApplicationIcon>", project, StringComparison.Ordinal);
        Assert.Contains("<LogicalName>GHelper.Assets.VictusXHub.ico</LogicalName>", project, StringComparison.Ordinal);
        Assert.Contains("Icon = GetTrayIcon(),", program, StringComparison.Ordinal);
        Assert.Contains("internal static Icon GetTrayIcon()", program, StringComparison.Ordinal);
        Assert.Contains("return GetHpTrayIcon();", program, StringComparison.Ordinal);
        Assert.Contains("internal static Icon GetHpTrayIcon()", program, StringComparison.Ordinal);
        Assert.Contains("HpTrayIconSelector.ResourceName", program, StringComparison.Ordinal);
        Assert.Contains("GetManifestResourceStream(HpTrayIconSelector.ResourceName)", program, StringComparison.Ordinal);
        Assert.Contains("Icon.ExtractAssociatedIcon(Application.ExecutablePath)", program, StringComparison.Ordinal);
        Assert.Contains("if (!AppConfig.IsHpVictusHardwareMode()) return Properties.Resources.standard;", program, StringComparison.Ordinal);
        Assert.Contains("if (AppConfig.IsHpVictusHardwareMode())", settings, StringComparison.Ordinal);
        Assert.Contains("Program.GetHpTrayIcon()", settings, StringComparison.Ordinal);
        Assert.Contains("Icon newIcon = GPUMode switch", settings, StringComparison.Ordinal);

        string combined = project + program + settings + selector;
        Assert.DoesNotContain("VictusXHub.Silent.ico", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("VictusXHub.Balanced.ico", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("VictusXHub.Turbo.ico", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("HpTrayIconKind", combined, StringComparison.Ordinal);
    }

    [Fact]
    public void HpPerformanceStatusCannotPromoteSavedConfigurationToHardwareState()
    {
        string settings = ReadRepositoryFile("app", "Settings.cs");
        string status = ReadRepositoryFile("app", "Hardware", "Hp", "HpPerformanceModeStatus.cs");
        Assert.Contains("button.Activated = false;", settings);
        Assert.Contains("button.Enabled = false;", settings);
        Assert.Contains("button.AccessibleDescription = HpPerformanceModeStatus.Blocker;", settings);
        Assert.Contains("string modeStatus = HpTrayIconSelector.FormatModeStatus(HpPerformanceModeStatus.CurrentBaseMode);", settings);
        Assert.Contains("labelPerf.Text = modeStatus;", settings);
        foreach (string forbidden in new[] { "hpqBIOSInt", "ManagementObject", "SetFan", "PawnIO", "0x37", "Modes.GetCurrentBase", "Action<", "Func<" })
            Assert.DoesNotContain(forbidden, status);
    }

    [Fact]
    public void LocalizedDisplayValues_UseVictusXHubBrandingWhileCompatibilityKeysRemain()
    {
        string propertiesDirectory = Path.Combine(FindRepositoryRoot(), "app", "Properties");
        string[] resourceFiles = Directory.GetFiles(propertiesDirectory, "Strings*.resx");

        Assert.NotEmpty(resourceFiles);
        foreach (string resourceFile in resourceFiles)
        {
            System.Xml.Linq.XDocument document = System.Xml.Linq.XDocument.Load(resourceFile);
            foreach (System.Xml.Linq.XElement data in document.Descendants("data"))
            {
                string value = data.Element("value")?.Value ?? string.Empty;
                bool containsInheritedBrand =
                    value.Contains("GHelper", StringComparison.OrdinalIgnoreCase) ||
                    value.Contains("G-Helper", StringComparison.OrdinalIgnoreCase) ||
                    value.Contains("G Helper", StringComparison.OrdinalIgnoreCase);

                Assert.False(
                    containsInheritedBrand,
                    $"Inherited branding remains in {Path.GetFileName(resourceFile)}:{data.Attribute("name")?.Value}.");
            }
        }

        System.Xml.Linq.XDocument baseResource = System.Xml.Linq.XDocument.Load(
            Path.Combine(propertiesDirectory, "Strings.resx"));
        Assert.Contains(
            baseResource.Descendants("data"),
            data => string.Equals(data.Attribute("name")?.Value, "OpenGHelper", StringComparison.Ordinal));
    }

    private static string ReadRepositoryFile(params string[] segments)
    {
        string repositoryRoot = FindRepositoryRoot();
        return File.ReadAllText(Path.Combine([repositoryRoot, .. segments]));
    }

    private static int CountOccurrences(string value, string fragment)
    {
        int count = 0;
        int index = 0;
        while ((index = value.IndexOf(fragment, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += fragment.Length;
        }

        return count;
    }

    private static string FindRepositoryRoot()
    {
        string? configuredRoot = Environment.GetEnvironmentVariable("VICTUSX_REPOSITORY_ROOT");
        if (!string.IsNullOrWhiteSpace(configuredRoot) && File.Exists(Path.Combine(configuredRoot, "VictusXHub.sln")))
        {
            return configuredRoot;
        }

        DirectoryInfo? directory = FindRepositoryRootFrom(AppContext.BaseDirectory);
        if (directory is not null)
        {
            return directory.FullName;
        }

        directory = FindRepositoryRootFrom(Directory.GetCurrentDirectory());
        if (directory is not null)
        {
            return directory.FullName;
        }

        throw new DirectoryNotFoundException("Could not locate the VictusXHub repository root.");
    }

    private static DirectoryInfo? FindRepositoryRootFrom(string startPath)
    {
        DirectoryInfo? directory = new(startPath);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "VictusXHub.sln")))
            {
                return directory;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
