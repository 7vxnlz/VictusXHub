using GHelper.Ally;
using GHelper.AnimeMatrix;
using GHelper.AutoUpdate;
using GHelper.Battery;
using GHelper.Display;
using GHelper.Fan;
using GHelper.Gpu;
using GHelper.Hardware.Hp;
using GHelper.Helpers;
using GHelper.Input;
using GHelper.Mode;
using GHelper.Peripherals;
using GHelper.Peripherals.Keyboard;
using GHelper.Peripherals.Mouse;
using GHelper.Properties;
using GHelper.UI;
using GHelper.USB;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;

namespace GHelper
{
    public partial class SettingsForm : RForm
    {
        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string? pszSubIdList);

        ContextMenuStrip contextMenuStrip = new CustomContextMenu();
        ToolStripMenuItem menuEco, menuStandard, menuUltimate, menuOptimized;
        DonateControl donateControl;
        Panel? hpMainShellPanel;
        RForm? hpDiagnosticForm;
        RButton? hpDiagnosticFooterButton;
        Panel? hpReadOnlyTelemetryPanel;
        TableLayoutPanel? hpReadOnlyTelemetrySummary;
        TableLayoutPanel? hpReadOnlyTelemetryDetails;
        Panel? hpReadOnlyTelemetryAdvanced;
        RButton? hpReadOnlyTelemetryAdvancedToggle;
        string? hpLastUserDiagnosticSummarySignature;
        Label? hpReadOnlyTelemetrySource;
        Label? hpReadOnlyTelemetryHealth;
        Label? hpReadOnlyTelemetryWarning;
        HpDiagnosticReportLoadResult? hpCachedDiagnosticReport;
        HpFanMaxPulseHistoryLoadResult? hpPulseHistory;
        HpFanProofGapAnalysis? hpFanProofGaps;
        HpReadOnlyTelemetryProvider? hpLiveTelemetryProvider;
        HpReadOnlyTelemetrySnapshot hpLiveTelemetry = HpReadOnlyTelemetrySnapshot.Unavailable;
        System.Windows.Forms.Timer? hpLiveTelemetryTimer;
        Label? hpLiveTelemetrySummary;
        HpDisplayRefreshRateState hpDisplayRefreshRateState = HpDisplayRefreshRateState.Unavailable;
        readonly List<ToolStripMenuItem> hpTrayStatusItems = [];
        ToolStripMenuItem? hpTrayModeStatusItem;
        HpTrayTelemetryStatus hpTrayTelemetryStatus = HpTrayTelemetryStatus.Unavailable;

        public GPUModeControl gpuControl;
        public AllyControl allyControl;
        AutoUpdateControl updateControl;

        AsusMouseSettings? mouseSettings;
        AsusKeyboardSettings? keyboardSettings;

        public AniMatrixControl matrixControl;

        public static System.Timers.Timer sensorTimer = default!;
        private static readonly bool sensorsAlways = AppConfig.Is("sensors_always");
        private readonly System.Windows.Forms.Timer batteryTimer = new() { Interval = 200 };

        public Matrix? matrixForm;
        public Slash? slashForm;
        public Fans? fansForm;
        public Extra? extraForm;
        public Updates? updatesForm;
        public Handheld? handheldForm;

        static long lastRefresh;
        static long lastBatteryRefresh;
        static long lastLostFocus;

        bool isGpuSection = true;
        bool isMuxGpu = true;

        bool batteryMouseOver = false;
        bool batteryFullMouseOver = false;

        bool sliderGammaIgnore = false;
        bool activateCheck = false;

        public SettingsForm()
        {

            InitializeComponent();
            InitTheme(true);

            gpuControl = new GPUModeControl(this);
            updateControl = new AutoUpdateControl(this);
            matrixControl = new AniMatrixControl(this);
            allyControl = new AllyControl(this);

            buttonSilent.Text = Properties.Strings.Silent;
            buttonBalanced.Text = Properties.Strings.Balanced;
            buttonTurbo.Text = Properties.Strings.Turbo;
            buttonFans.Text = Properties.Strings.FansPower;

            buttonEco.Text = Properties.Strings.EcoMode;
            buttonUltimate.Text = Properties.Strings.UltimateMode;
            buttonStandard.Text = Properties.Strings.StandardMode;
            buttonOptimized.Text = Properties.Strings.Optimized;
            buttonStopGPU.Text = Properties.Strings.StopGPUApps;

            buttonScreenAuto.Text = Properties.Strings.AutoMode;
            buttonMiniled.Text = Properties.Strings.Multizone;

            buttonKeyboardColor.Text = Properties.Strings.Color;
            buttonKeyboard.Text = Properties.Strings.Extra;

            labelPerf.Text = Properties.Strings.PerformanceMode;
            labelGPU.Text = Properties.Strings.GPUMode;
            labelSreen.Text = Properties.Strings.LaptopScreen;
            UpdateKeyboardLabel();
            labelMatrix.Text = Properties.Strings.AnimeMatrix;
            labelBatteryTitle.Text = Properties.Strings.BatteryChargeLimit;

            checkStartup.Text = Properties.Strings.RunOnStartup;

            buttonMatrix.Text = "Matrix";
            buttonQuit.Text = Properties.Strings.Quit;
            buttonUpdates.Text = Properties.Strings.Updates;
            buttonDonate.Text = AppConfig.IsHpVictusHardwareMode() ? "Thank You" : Properties.Strings.Donate;

            buttonController.Text = Properties.Strings.Controller;
            labelAlly.Text = Properties.Strings.AllyController;

            // Accessible Labels

            panelMatrix.AccessibleName = Properties.Strings.AnimeMatrix;
            sliderBattery.AccessibleName = Properties.Strings.BatteryChargeLimit;
            buttonQuit.AccessibleName = Properties.Strings.Quit;
            buttonUpdates.AccessibleName = Properties.Strings.BiosAndDriverUpdates;
            panelPerformance.AccessibleName = Properties.Strings.PerformanceMode;
            buttonSilent.AccessibleName = Properties.Strings.Silent;
            buttonBalanced.AccessibleName = Properties.Strings.Balanced;
            buttonTurbo.AccessibleName = Properties.Strings.Turbo;
            buttonFans.AccessibleName = Properties.Strings.FansAndPower;
            panelGPU.AccessibleName = Properties.Strings.GPUMode;
            buttonEco.AccessibleName = Properties.Strings.EcoMode;
            buttonStandard.AccessibleName = Properties.Strings.StandardMode;
            buttonOptimized.AccessibleName = Properties.Strings.Optimized;
            buttonUltimate.AccessibleName = Properties.Strings.UltimateMode;
            panelScreen.AccessibleName = Properties.Strings.LaptopScreen;

            buttonScreenAuto.AccessibleName = Properties.Strings.AutoMode;
            //button60Hz.AccessibleName = "60Hz Refresh Rate";
            //button120Hz.AccessibleName = "Maximum Refresh Rate";

            panelKeyboard.AccessibleName = Properties.Strings.LaptopKeyboard;
            buttonKeyboard.AccessibleName = Properties.Strings.ExtraSettings;
            buttonKeyboardColor.AccessibleName = Properties.Strings.LaptopKeyboard + " " + Properties.Strings.Color;
            comboKeyboard.AccessibleName = Properties.Strings.LaptopBacklight;

            FormClosing += SettingsForm_FormClosing;
            Deactivate += SettingsForm_LostFocus;
            Activated += SettingsForm_Focused;

            buttonSilent.BorderColor = colorEco;
            buttonBalanced.BorderColor = colorStandard;
            buttonTurbo.BorderColor = colorTurbo;
            buttonFans.BorderColor = colorCustom;

            buttonEco.BorderColor = colorEco;
            buttonStandard.BorderColor = colorStandard;
            buttonUltimate.BorderColor = colorTurbo;
            buttonOptimized.BorderColor = colorEco;
            buttonXGM.BorderColor = colorTurbo;

            button60Hz.BorderColor = colorGray;
            button120Hz.BorderColor = colorGray;
            buttonScreenAuto.BorderColor = colorGray;
            buttonMiniled.BorderColor = colorTurbo;

            buttonEnergySaver.BackColor = colorEco;
            buttonEnergySaver.ForeColor = SystemColors.ControlLightLight;
            buttonEnergySaver.Click += ButtonEnergySaver_Click;

            buttonAmdOled.BackColor = colorTurbo;
            buttonAmdOled.ForeColor = SystemColors.ControlLightLight;
            buttonAmdOled.Click += ButtonAmdOled_Click;

            buttonArmoury.BackColor = colorTurbo;
            buttonArmoury.ForeColor = SystemColors.ControlLightLight;
            buttonArmoury.Click += ButtonArmoury_Click;

            buttonSilent.Click += ButtonSilent_Click;
            buttonBalanced.Click += ButtonBalanced_Click;
            buttonTurbo.Click += ButtonTurbo_Click;

            buttonEco.Click += ButtonEco_Click;
            buttonStandard.Click += ButtonStandard_Click;
            buttonUltimate.Click += ButtonUltimate_Click;
            buttonOptimized.Click += ButtonOptimized_Click;
            buttonStopGPU.Click += ButtonStopGPU_Click;
            pictureGPU.Click += PictureGPU_Click;

            VisibleChanged += SettingsForm_VisibleChanged;

            button60Hz.Click += Button60Hz_Click;
            button120Hz.Click += Button120Hz_Click;
            buttonScreenAuto.Click += ButtonScreenAuto_Click;
            buttonMiniled.Click += ButtonMiniled_Click;
            buttonFHD.Click += ButtonFHD_Click;
            buttonHDRControl.Click += ButtonHDRControl_Click;

            buttonQuit.Click += ButtonQuit_Click;

            buttonKeyboardColor.Click += ButtonKeyboardColor_Click;
            buttonKeyboardColor.Swatch2Click += ButtonKeyboardColor2_Click;

            buttonFans.Click += ButtonFans_Click;
            buttonKeyboard.Click += ButtonKeyboard_Click;
            buttonController.Click += ButtonHandheld_Click;

            labelCPUFan.Click += LabelCPUFan_Click;
            labelGPUFan.Click += LabelCPUFan_Click;

            comboMatrix.DropDownStyle = ComboBoxStyle.DropDownList;
            comboMatrixRunning.DropDownStyle = ComboBoxStyle.DropDownList;

            comboMatrix.DropDownClosed += ComboMatrix_SelectedValueChanged;
            comboMatrixRunning.DropDownClosed += ComboMatrixRunning_SelectedValueChanged;

            buttonMatrix.Click += ButtonMatrix_Click;

            if (!AppConfig.IsHpVictusHardwareMode())
            {
                checkStartup.Checked = Startup.IsScheduled();
                checkStartup.CheckedChanged += CheckStartup_CheckedChanged;
            }

            labelVersion.Click += LabelVersion_Click;
            labelVersion.ForeColor = Color.FromArgb(128, Color.Gray);

            buttonOptimized.MouseMove += ButtonOptimized_MouseHover;
            buttonOptimized.MouseLeave += ButtonGPU_MouseLeave;

            buttonEco.MouseMove += ButtonEco_MouseHover;
            buttonEco.MouseLeave += ButtonGPU_MouseLeave;

            buttonStandard.MouseMove += ButtonStandard_MouseHover;
            buttonStandard.MouseLeave += ButtonGPU_MouseLeave;

            buttonUltimate.MouseMove += ButtonUltimate_MouseHover;
            buttonUltimate.MouseLeave += ButtonGPU_MouseLeave;

            tableGPU.MouseMove += ButtonXGM_MouseMove;
            tableGPU.MouseLeave += ButtonGPU_MouseLeave;

            buttonXGM.Click += ButtonXGM_Click;

            buttonScreenAuto.MouseMove += ButtonScreenAuto_MouseHover;
            buttonScreenAuto.MouseLeave += ButtonScreen_MouseLeave;

            button60Hz.MouseMove += Button60Hz_MouseHover;
            button60Hz.MouseLeave += ButtonScreen_MouseLeave;

            button120Hz.MouseMove += Button120Hz_MouseHover;
            button120Hz.MouseLeave += ButtonScreen_MouseLeave;

            buttonFHD.MouseMove += ButtonFHD_MouseHover;
            buttonFHD.MouseLeave += ButtonScreen_MouseLeave;

            buttonUpdates.Click += ButtonUpdates_Click;

            sliderBattery.MouseUp += SliderBattery_MouseUp;
            sliderBattery.KeyUp += SliderBattery_KeyUp;
            sliderBattery.ValueChanged += SliderBattery_ValueChanged;
            batteryTimer.Tick += (_, _) => { batteryTimer.Stop(); BatteryControl.SetBatteryChargeLimit(sliderBattery.Value); };
            if (AppConfig.IsChargeLimit6080()) sliderBattery.supportedValues = new() { 60, 65, 70, 75, 80, 100 };

            sensorTimer = new System.Timers.Timer(AppConfig.Get("sensor_timer", 1000));
            sensorTimer.Elapsed += OnTimedEvent;
            sensorTimer.Enabled = !AppConfig.IsHpVictusHardwareMode() && sensorsAlways;

            labelCharge.MouseEnter += PanelBattery_MouseEnter;
            labelCharge.MouseLeave += PanelBattery_MouseLeave;
            labelBattery.Click += LabelBattery_Click;

            buttonPeripheral1.Click += ButtonPeripheral_Click;
            buttonPeripheral2.Click += ButtonPeripheral_Click;
            buttonPeripheral3.Click += ButtonPeripheral_Click;

            buttonPeripheral1.MouseEnter += ButtonPeripheral_MouseEnter;
            buttonPeripheral2.MouseEnter += ButtonPeripheral_MouseEnter;
            buttonPeripheral3.MouseEnter += ButtonPeripheral_MouseEnter;

            buttonBatteryFull.MouseEnter += ButtonBatteryFull_MouseEnter;
            buttonBatteryFull.MouseLeave += ButtonBatteryFull_MouseLeave;
            buttonBatteryFull.Click += ButtonBatteryFull_Click;

            buttonControllerMode.Click += ButtonControllerMode_Click;
            buttonBacklight.Click += ButtonBacklight_Click;

            buttonFPS.Click += ButtonFPS_Click;
            buttonOverlay.Click += ButtonOverlay_Click;
            buttonOverlay.BorderColor = colorStandard;

            buttonAutoTDP.Click += ButtonAutoTDP_Click;
            buttonAutoTDP.BorderColor = colorTurbo;

            Text = AppConfig.IsHpVictusHardwareMode()
                ? "VictusX"
                : "VictusX " + (ProcessHelper.IsUserAdministrator() ? "—" : "-") + " " + AppConfig.GetModelShort();
            TopMost = AppConfig.Is("topmost");

            //This will auto position the window again when it resizes. Might mess with position if people drag the window somewhere else.
            this.Resize += SettingsForm_Resize;

            if (!AppConfig.IsHpVictusHardwareMode())
            {
                VisualiseFnLock();
                buttonFnLock.Click += ButtonFnLock_Click;
            }

            labelVisual.Click += LabelVisual_Click;
            labelCharge.Click += LabelCharge_Click;

            if (AppConfig.IsHpVictusHardwareMode())
            {
                buttonDonate.AccessibleName = "Thank You - unavailable in HP read-only mode";
                buttonDonate.Badge = 0;
                buttonDonate.Enabled = false;
            }
            else
            {
                donateControl = new DonateControl(this, buttonDonate);
                donateControl.Init();
            }

            labelBacklight.ForeColor = colorStandard;
            labelBacklight.Click += LabelBacklight_Click;

            AddHpReadOnlyTelemetryPanel();

            if (AppConfig.IsHpVictusHardwareMode())
            {
                ConfigureHpReadOnlyShell();
                InitializeHpLiveTelemetry();
                hpMainShellPanel?.Focus();
            }
            else
            {
                panelPerformance.Focus();
                InitVisual();
            }
        }

        private void ConfigureHpReadOnlyShell()
        {
            Control[] hpShellSections =
            {
                panelPerformance,
                panelGPU,
                panelScreen,
                panelKeyboard,
                panelBattery
            };

            foreach (Control section in hpShellSections)
            {
                section.Enabled = true;
                section.Visible = true;
                section.TabStop = false;
                ConfigureHpReadOnlySection(section);
            }

            foreach (Control control in new Control[]
            {
                labelCPUFan,
                labelGPUFan,
                labelBattery,
                labelCharge,
                labelBacklight,
                pictureGPU
            })
            {
                control.Enabled = false;
                control.TabStop = false;
            }

            foreach (Control control in new Control[]
            {
                panelGamma,
                panelMatrix,
                panelAlly,
                panelRearLight,
                panelPeripherals,
                panelStartup,
                panelVersion
            })
            {
                control.Visible = false;
                control.TabStop = false;
            }

            buttonUpdates.Enabled = false;
            buttonUpdates.Visible = true;
            buttonUpdates.TabStop = false;
            checkStartup.Visible = false;
            checkStartup.TabStop = false;

            labelGPU.Text = "GPU Mode: Unavailable";
            labelBacklight.Text = "Keyboard Lighting: Unavailable";
            labelCharge.Text = "Battery Care: Unavailable";
            buttonFans.Text = "Fan Control\nNO-GO";
            buttonFans.AccessibleName = "Fan Control: NO-GO";
            buttonFans.AccessibleDescription = "Normal fan control: NO-GO.";
            toolTip.SetToolTip(buttonFans, "Normal fan control: NO-GO.");

            hpMainShellPanel = panelPerformance;
            ConfigureHpRefreshRateControl();
            contextMenuStrip.Opening += (_, _) => RefreshHpLiveTelemetry(force: true);
            AddHpDiagnosticFooterAction();
        }

        private void ConfigureHpRefreshRateControl()
        {
            buttonScreenAuto.Click -= ButtonScreenAuto_Click;
            button60Hz.Click -= Button60Hz_Click;
            button120Hz.Click -= Button120Hz_Click;
            buttonMiniled.Click -= ButtonMiniled_Click;

            buttonScreenAuto.Text = "Automatic";
            buttonScreenAuto.AccessibleName = "Automatic refresh rate unavailable";
            buttonScreenAuto.AccessibleDescription = "Automatic refresh-rate switching is not available in HP mode.";
            buttonScreenAuto.Enabled = false;
            buttonScreenAuto.TabStop = false;
            buttonScreenAuto.Activated = false;
            buttonScreenAuto.Visible = true;
            toolTip.SetToolTip(buttonScreenAuto, "Automatic refresh-rate switching is unavailable in HP mode.");

            button60Hz.Text = "60Hz";
            button60Hz.Tag = 60;
            button60Hz.Click += ButtonHpRefreshRate_Click;
            button60Hz.AccessibleName = "Set internal display to 60Hz";

            button120Hz.Text = "144Hz";
            button120Hz.Tag = 144;
            button120Hz.Click += ButtonHpRefreshRate_Click;
            button120Hz.AccessibleName = "Set internal display to 144Hz";

            foreach (RButton button in new[] { button60Hz, button120Hz })
            {
                button.Enabled = false;
                button.TabStop = false;
                button.Activated = false;
            }
            buttonMiniled.Visible = false;
            buttonMiniled.TabStop = false;

            tableScreen.ColumnCount = 3;
            tableScreen.ColumnStyles.Clear();
            for (int column = 0; column < 3; column++)
                tableScreen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 3F));
            tableScreen.SetColumn(buttonScreenAuto, 0);
            tableScreen.SetColumnSpan(buttonScreenAuto, 1);
            tableScreen.SetColumn(button60Hz, 1);
            tableScreen.SetColumn(button120Hz, 2);
            RefreshHpRefreshRateState();
        }

        private void AddHpDiagnosticFooterAction()
        {
            tableButtons.SuspendLayout();
            tableButtons.ColumnCount = 4;
            tableButtons.ColumnStyles.Clear();
            for (int column = 0; column < 4; column++)
            {
                tableButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            }

            tableButtons.AutoSize = false;
            tableButtons.MinimumSize = new Size(0, buttonQuit.Height + buttonQuit.Margin.Vertical);
            tableButtons.Height = buttonQuit.Height + buttonQuit.Margin.Vertical;

            hpDiagnosticFooterButton = new RButton
            {
                AccessibleName = "Diagnostic",
                Activated = false,
                BackColor = buttonSecond,
                BorderColor = Color.Transparent,
                BorderRadius = 2,
                Dock = DockStyle.Top,
                FlatStyle = FlatStyle.Flat,
                ForeColor = foreMain,
                Image = Properties.Resources.icons8_log_32,
                Margin = new Padding(4, 5, 4, 5),
                Secondary = true,
                Text = "Diagnostic",
                UseVisualStyleBackColor = false
            };
            hpDiagnosticFooterButton.FlatAppearance.BorderColor = borderSecond;
            hpDiagnosticFooterButton.Click += ButtonHpDiagnostic_Click;

            tableButtons.SetColumn(buttonDonate, 0);
            tableButtons.SetColumn(buttonUpdates, 1);
            tableButtons.Controls.Add(hpDiagnosticFooterButton, 2, 0);
            tableButtons.SetColumn(buttonQuit, 3);
            tableButtons.ResumeLayout();
        }

        private void InitializeHpLiveTelemetry()
        {
            hpLiveTelemetryProvider = new HpReadOnlyTelemetryProvider(new HpWindowsTelemetrySource(),
                new HpGpuTemperaturePoller(HpNvidiaTemperatureSource.Read),
                ReadHpDisplayRefreshRate,
                new HpBatteryCarePoller(new HpBatteryCareReadOnlySource().Read));
            components ??= new System.ComponentModel.Container();
            hpLiveTelemetryTimer = new System.Windows.Forms.Timer(components) { Interval = 1000 };
            hpLiveTelemetryTimer.Tick += (_, _) => RefreshHpLiveTelemetry();

            // These existing labels become status-only; remove their inherited action handlers.
            labelCPUFan.Click -= LabelCPUFan_Click;
            labelGPUFan.Click -= LabelCPUFan_Click;
            labelBattery.Click -= LabelBattery_Click;
            foreach (Label label in new[] { labelCPUFan, labelGPUFan, labelBattery, labelTipGPU })
            {
                label.Enabled = true;
                label.Cursor = Cursors.Default;
                label.AccessibleRole = AccessibleRole.StaticText;
                label.ForeColor = foreMain;
                label.TabStop = false;
            }
            ApplyHpLiveTelemetry();
        }

        private void RefreshHpLiveTelemetry(bool force = false)
        {
            if (!AppConfig.IsHpVictusHardwareMode() || (!force && !Visible) || IsDisposed || Disposing || hpLiveTelemetryProvider is null) return;
            hpLiveTelemetry = hpLiveTelemetryProvider.Capture(DateTimeOffset.UtcNow);
            ApplyHpLiveTelemetry();
        }

        private void ApplyHpLiveTelemetry()
        {
            HpVictusCapabilitySnapshot? snapshot = Program.hpVictusCapabilitySnapshot;
            bool cachedIdentity = snapshot is null;
            bool? detected = snapshot?.IsHpVictus ?? hpCachedDiagnosticReport?.GetHpVictusDetected();
            HpKeyboardBacklightStatus keyboard = HpKeyboardBacklightStatus.Resolve(
                detected,
                GetSnapshotOrReportValue(snapshot?.Model, hpCachedDiagnosticReport, "Model"),
                GetSnapshotOrReportValue(snapshot?.SystemSku, hpCachedDiagnosticReport, "Sku"));
            HpGpuModeStatus gpuMode = HpGpuModeStatus.Resolve(
                detected,
                GetSnapshotOrReportValue(snapshot?.Model, hpCachedDiagnosticReport, "Model"),
                GetSnapshotOrReportValue(snapshot?.SystemSku, hpCachedDiagnosticReport, "Sku"),
                GetHpGpuModeSwitchRaw(snapshot, hpCachedDiagnosticReport));
            byte? fanCount = GetHpFanCount(snapshot, hpCachedDiagnosticReport);
            byte? thermalPolicyVersion = GetHpThermalPolicyVersion(snapshot, hpCachedDiagnosticReport);
            HpReadOnlyTelemetryDisplay display = HpReadOnlyTelemetryFormatter.Format(
                hpLiveTelemetry, DateTimeOffset.UtcNow, detected, cachedIdentity);
            hpTrayTelemetryStatus = display.TrayStatus;
            string modeStatus = HpTrayIconSelector.FormatModeStatus(HpPerformanceModeStatus.CurrentBaseMode);
            UpdateHpTrayStatusItems();
            labelCPUFan.Text = hpTrayTelemetryStatus.Cpu;
            labelGPUFan.Text = hpTrayTelemetryStatus.Gpu;
            labelTipGPU.Text = display.FanAndDevice;
            labelBattery.Text = hpTrayTelemetryStatus.Battery;
            labelCharge.Text = $"Battery Care: {display.BatteryCareCapability}";
            labelBacklight.Text = $"Keyboard Lighting: {keyboard.CapabilityText}";
            labelGPU.Text = $"GPU Mode: {gpuMode.CapabilityText}";
            labelSreen.Text = hpTrayTelemetryStatus.Screen;
            UpdateHpRefreshRateButtons(hpLiveTelemetry.DisplayRefreshRateHz);
            labelPerf.Text = modeStatus;
            panelPerformance.AccessibleName = modeStatus;
            foreach (RButton button in new[] { buttonSilent, buttonBalanced, buttonTurbo })
            {
                button.Enabled = false;
                button.Activated = false;
                button.AccessibleDescription = HpPerformanceModeStatus.Blocker;
                toolTip.SetToolTip(button, HpPerformanceModeStatus.Blocker);
            }
            VisualiseIcon();
            panelScreen.AccessibleName = display.Display;
            if (hpLiveTelemetrySummary is not null)
                hpLiveTelemetrySummary.Text = display.Summary + Environment.NewLine + keyboard.EvidenceText +
                    Environment.NewLine + gpuMode.EvidenceText + Environment.NewLine + HpPerformanceModeStatus.Blocker;
            PopulateHpUserDiagnosticSummary(new HpDiagnosticUserSummaryInput
            {
                Model = GetSnapshotOrReportValue(snapshot?.Model, hpCachedDiagnosticReport, "Model"),
                Sku = GetSnapshotOrReportValue(snapshot?.SystemSku, hpCachedDiagnosticReport, "Sku"),
                BiosVersion = GetSnapshotOrReportValue(snapshot?.BiosVersion, hpCachedDiagnosticReport, "BiosVersion"),
                HpVictusDetection = detected switch { true => "Detected", false => "Not supported", _ => "Unavailable" },
                FanCount = HpReadOnlyTelemetryFormatter.FormatFanCount(fanCount),
                ThermalPolicy = HpReadOnlyTelemetryFormatter.FormatThermalPolicy(thermalPolicyVersion),
                CpuLoad = display.CpuLoad,
                GpuTemperature = display.GpuTemperature,
                BatteryPower = display.BatteryPower,
                RefreshRate = display.RefreshRate,
                CpuTemperature = display.CpuTemperature,
                FanRpm = display.FanRpm,
                GpuSwitchingCapability = gpuMode.SwitchingCapabilityText,
                KeyboardBacklightCapability = keyboard.CapabilityText,
                BatteryCareCapability = display.BatteryCareCapability,
                FanControlStatus = "NO-GO"
            });
        }

        private static byte? GetHpGpuModeSwitchRaw(
            HpVictusCapabilitySnapshot? snapshot, HpDiagnosticReportLoadResult? report)
        {
            if (snapshot?.SystemDesignDataInvocationSucceeded == true && snapshot.SystemDesignDataDecodeSucceeded)
                return snapshot.SystemDesignDataDecoded?.GpuModeSwitchRaw;

            return report?.GetBool("SystemDesignDataDecodeSucceeded") == true &&
                byte.TryParse(report.GetValue("SystemDesignDataDecoded.GpuModeSwitchRaw"), out byte raw)
                     ? raw : null;
        }

        private static byte? GetHpFanCount(
            HpVictusCapabilitySnapshot? snapshot, HpDiagnosticReportLoadResult? report)
        {
            if (snapshot?.FanGetCountInvocationSucceeded == true && snapshot.FanGetCountDecodeSucceeded)
                return snapshot.FanGetCountDecoded?.FanCount;

            return report?.GetBool("FanGetCountDecodeSucceeded") == true &&
                byte.TryParse(report.GetValue("FanGetCountDecoded.FanCount"), out byte count)
                    ? count : null;
        }

        private static byte? GetHpThermalPolicyVersion(
            HpVictusCapabilitySnapshot? snapshot, HpDiagnosticReportLoadResult? report)
        {
            if (snapshot?.SystemDesignDataInvocationSucceeded == true && snapshot.SystemDesignDataDecodeSucceeded)
                return snapshot.SystemDesignDataDecoded?.ThermalPolicyVersion;

            return report?.GetBool("SystemDesignDataDecodeSucceeded") == true &&
                byte.TryParse(report.GetValue("SystemDesignDataDecoded.ThermalPolicyVersion"), out byte version)
                    ? version : null;
        }

        private static int? ReadHpDisplayRefreshRate()
        {
            string? laptopScreen = ScreenNative.FindUniqueHardwareInternalScreen();
            int refreshRate = ScreenNative.GetRefreshRate(laptopScreen);
            return refreshRate > 0 ? refreshRate : null;
        }

        private void RefreshHpRefreshRateState()
        {
            string? displayName = ScreenNative.FindUniqueHardwareInternalScreen();
            hpDisplayRefreshRateState = HpDisplayRefreshRateControl.BuildState(
                ScreenNative.GetDisplayMode(displayName),
                ScreenNative.GetDisplayModes(displayName));
            UpdateHpRefreshRateButtons(hpDisplayRefreshRateState.CurrentRateHz);
        }

        private void ButtonHpRefreshRate_Click(object? sender, EventArgs e)
        {
            if (!AppConfig.IsHpVictusHardwareMode() || sender is not RButton { Tag: int requestedRate }) return;
            HpDisplayRefreshRateApplyResult result = ApplyHpRefreshRate(requestedRate);
            if (!result.Succeeded)
                MessageBox.Show(this, result.Message, "VictusX Display", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UpdateHpRefreshRateButtons(int? currentRateHz)
        {
            UpdateHpRefreshRateButton(button60Hz, 60, currentRateHz);
            UpdateHpRefreshRateButton(button120Hz, 144, currentRateHz);
            buttonScreenAuto.Activated = false;
        }

        private void UpdateHpRefreshRateButton(RButton button, int rate, int? currentRateHz)
        {
            button.Visible = hpDisplayRefreshRateState.SupportedRates.Contains(rate);
            button.Enabled = button.Visible;
            button.TabStop = button.Visible;
            button.Activated = button.Visible && currentRateHz == rate;
            toolTip.SetToolTip(button, button.Visible
                ? $"Set the internal display to {rate}Hz using Windows display settings."
                : $"{rate}Hz is not reported for the current internal-panel mode.");
        }

        private HpDisplayRefreshRateApplyResult ApplyHpRefreshRate(int requestedRate)
        {
            HpDisplayRefreshRateApplyResult result;
            try
            {
                string? displayName = ScreenNative.FindUniqueHardwareInternalScreen();
                hpDisplayRefreshRateState = HpDisplayRefreshRateControl.BuildState(
                    ScreenNative.GetDisplayMode(displayName),
                    ScreenNative.GetDisplayModes(displayName));
                result = displayName is null
                    ? new(false, hpDisplayRefreshRateState.CurrentRateHz, "The internal display could not be uniquely identified.")
                    : HpDisplayRefreshRateControl.Apply(
                        hpDisplayRefreshRateState,
                        requestedRate,
                        rate => ScreenNative.SetRefreshRateValidated(displayName, rate),
                        () => ScreenNative.GetDisplayMode(displayName)?.RefreshRateHz);
            }
            catch (Exception ex)
            {
                Logger.WriteLine("HP refresh-rate change failed: " + ex.Message);
                result = new(false, hpDisplayRefreshRateState.CurrentRateHz, "Windows could not apply the refresh-rate change.");
            }

            RefreshHpRefreshRateState();
            RefreshHpLiveTelemetry(force: true);
            return result;
        }

        private void ConfigureHpReadOnlySection(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                control.ForeColor = foreMain;

                if (control is RButton or ComboBox or Slider or CheckBox or PictureBox)
                {
                    control.Enabled = false;
                    control.TabStop = false;
                    control.AccessibleDescription = "Unavailable in HP mode.";
                    toolTip.SetToolTip(control, "Unavailable in HP mode.");
                }

                ConfigureHpReadOnlySection(control);
            }
        }

        private void AddHpReadOnlyTelemetryPanel()
        {
            if (!AppConfig.IsHpVictusHardwareMode()) return;

            var scrollHost = new Panel
            {
                AccessibleName = "Diagnostic panel content",
                AutoScroll = true,
                BackColor = formBack,
                Dock = DockStyle.Fill
            };
            ApplyHpDarkExplorerTheme(scrollHost);
            scrollHost.HorizontalScroll.Visible = false;
            scrollHost.VerticalScroll.Visible = false;

            var panel = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = formBack,
                Dock = DockStyle.Top,
                Padding = new Padding(11, 5, 11, 5),
                AccessibleName = "Diagnostic panel"
            };

            var heading = new Label
            {
                AutoSize = true,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = foreMain,
                Padding = new Padding(10, 5, 10, 5),
                Text = "Diagnostic"
            };

            hpReadOnlyTelemetrySource = new Label
            {
                AutoSize = true,
                Dock = DockStyle.Top,
                ForeColor = Color.Gray,
                Padding = new Padding(10, 0, 10, 5)
            };

            hpReadOnlyTelemetryHealth = new Label
            {
                AutoSize = true,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Top,
                ForeColor = foreMain,
                MaximumSize = new Size(760, 0),
                Padding = new Padding(10, 5, 10, 5)
            };

            hpReadOnlyTelemetryWarning = new Label
            {
                AutoSize = true,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Top,
                ForeColor = colorTurbo,
                Padding = new Padding(10, 5, 10, 5),
                Text = HpDiagnosticStatusText.SafetyWarning
            };

            hpLiveTelemetrySummary = new Label
            {
                AutoSize = true,
                Dock = DockStyle.Top,
                ForeColor = foreMain,
                MaximumSize = new Size(760, 0),
                Padding = new Padding(10, 5, 10, 5),
                AccessibleName = "Read-only OS telemetry sources and freshness"
            };

            var userSummary = new TableLayoutPanel
            {
                AccessibleName = "User-facing diagnostic summary",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                Dock = DockStyle.Top,
                Padding = new Padding(10, 0, 10, 5)
            };
            userSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
            userSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            var details = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                Dock = DockStyle.Top,
                Padding = new Padding(10, 0, 10, 5)
            };
            details.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
            details.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            var advancedPanel = new Panel
            {
                AccessibleName = "Advanced developer diagnostics",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = formBack,
                Dock = DockStyle.Top,
                Visible = false
            };
            advancedPanel.Controls.Add(details);
            advancedPanel.Controls.Add(hpLiveTelemetrySummary);

            hpReadOnlyTelemetryAdvancedToggle = CreateHpDiagnosticActionButton(
                "Show Advanced / Developer diagnostics", ButtonHpDiagnosticAdvanced_Click);
            hpReadOnlyTelemetryAdvancedToggle.AccessibleName = "Show advanced developer diagnostics";
            hpReadOnlyTelemetryAdvancedToggle.Dock = DockStyle.Top;
            hpReadOnlyTelemetryAdvancedToggle.Margin = new Padding(10, 5, 10, 5);

            var actions = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(10, 0, 10, 5),
                WrapContents = true
            };
            actions.Controls.Add(CreateHpDiagnosticActionButton("Main", ButtonHpDiagnosticMain_Click));
            actions.Controls.Add(CreateHpDiagnosticActionButton("Copy summary", ButtonHpDiagnosticCopy_Click));
            actions.Controls.Add(CreateHpDiagnosticActionButton("Reload cached report", ButtonHpDiagnosticReload_Click));
            actions.Controls.Add(CreateHpDiagnosticActionButton("Open diagnostic folder", ButtonHpDiagnosticOpenReportFolder_Click));
            actions.Controls.Add(CreateHpDiagnosticActionButton("Export diagnostic report", ButtonHpDiagnosticExport_Click));

            hpReadOnlyTelemetrySummary = userSummary;
            hpReadOnlyTelemetryDetails = details;
            hpReadOnlyTelemetryAdvanced = advancedPanel;
            ReloadHpCachedDiagnosticReport();
            PopulateHpReadOnlyTelemetryPanel();

            panel.Controls.Add(actions);
            panel.Controls.Add(advancedPanel);
            panel.Controls.Add(hpReadOnlyTelemetryAdvancedToggle);
            panel.Controls.Add(userSummary);
            panel.Controls.Add(hpReadOnlyTelemetryWarning);
            panel.Controls.Add(hpReadOnlyTelemetryHealth);
            panel.Controls.Add(hpReadOnlyTelemetrySource);
            panel.Controls.Add(heading);
            scrollHost.Controls.Add(panel);
            hpReadOnlyTelemetryPanel = scrollHost;
        }

        private void ButtonHpDiagnostic_Click(object? sender, EventArgs e)
        {
            ShowHpReadOnlyDiagnostic();
        }

        private void ButtonHpDiagnosticMain_Click(object? sender, EventArgs e)
        {
            ShowHpReadOnlyMainShell();
        }

        private void ButtonHpDiagnosticAdvanced_Click(object? sender, EventArgs e)
        {
            if (hpReadOnlyTelemetryAdvanced is null || hpReadOnlyTelemetryAdvancedToggle is null) return;

            hpReadOnlyTelemetryAdvanced.Visible = !hpReadOnlyTelemetryAdvanced.Visible;
            hpReadOnlyTelemetryAdvancedToggle.Text = hpReadOnlyTelemetryAdvanced.Visible
                ? "Hide Advanced / Developer diagnostics"
                : "Show Advanced / Developer diagnostics";
        }

        private void ShowHpReadOnlyDiagnostic()
        {
            if (hpReadOnlyTelemetryPanel is null) return;

            EnsureHpDiagnosticForm();
            if (hpDiagnosticForm is null) return;
            RefreshHpLiveTelemetry(force: true);

            if (hpDiagnosticForm.Visible)
            {
                PositionHpDiagnosticForm();
                hpDiagnosticForm.Activate();
                hpDiagnosticForm.BringToFront();
                return;
            }

            if (!Visible)
            {
                WindowState = FormWindowState.Normal;
                Show();
            }

            PositionHpDiagnosticForm();
            hpDiagnosticForm.Show();
            hpDiagnosticForm.Activate();
        }

        private void ShowHpReadOnlyMainShell()
        {
            if (hpMainShellPanel is null) return;

            hpDiagnosticForm?.Hide();
            hpMainShellPanel.Visible = true;
            if (!Visible)
            {
                WindowState = FormWindowState.Normal;
                Show();
            }

            ShowAll();
            hpMainShellPanel.Focus();
        }

        private void EnsureHpDiagnosticForm()
        {
            if (hpDiagnosticForm is not null && !hpDiagnosticForm.IsDisposed) return;
            if (hpReadOnlyTelemetryPanel is null) return;

            hpDiagnosticForm = new RForm
            {
                AutoScroll = false,
                AutoSize = false,
                BackColor = formBack,
                ForeColor = foreMain,
                MinimizeBox = false,
                Name = "HpDiagnosticSidePanel",
                ShowIcon = false,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                Text = "VictusX Diagnostic"
            };
            hpDiagnosticForm.InitTheme(true);
            hpDiagnosticForm.FormClosing += (_, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    hpDiagnosticForm.Hide();
                }
            };

            hpReadOnlyTelemetryPanel.Visible = true;
            hpDiagnosticForm.Controls.Add(hpReadOnlyTelemetryPanel);
            AddOwnedForm(hpDiagnosticForm);
        }

        private void PositionHpDiagnosticForm()
        {
            if (hpDiagnosticForm is null) return;

            Rectangle workingArea = Screen.FromControl(this).WorkingArea;
            int availableWidth = Math.Max(1, workingArea.Width - 32);
            int availableHeight = Math.Max(1, workingArea.Height - 32);
            int preferredWidth = Math.Min(900, availableWidth);
            int sideWidth = Math.Min(preferredWidth, Math.Max(640, availableWidth - Width - 24));
            int sideHeight = Math.Min(Math.Max(Height, MinimumSize.Height), availableHeight);
            int left = Left - sideWidth - 5;

            if (left < workingArea.Left)
            {
                left = Right + 5;
            }

            if (left + sideWidth > workingArea.Right)
            {
                left = Math.Max(workingArea.Left, workingArea.Right - sideWidth);
            }

            int top = Math.Max(workingArea.Top, Math.Min(Top, workingArea.Bottom - sideHeight));
            hpDiagnosticForm.MinimumSize = new Size(Math.Min(640, sideWidth), Math.Min(420, sideHeight));
            hpDiagnosticForm.MaximumSize = new Size(sideWidth, availableHeight);
            hpDiagnosticForm.Size = new Size(sideWidth, sideHeight);
            hpDiagnosticForm.Location = new Point(left, top);
        }

        private RButton CreateHpDiagnosticActionButton(string text, EventHandler clickHandler)
        {
            var button = new RButton
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = buttonSecond,
                FlatStyle = FlatStyle.Flat,
                ForeColor = foreMain,
                Margin = new Padding(0, 0, 6, 0),
                Padding = new Padding(8, 3, 8, 3),
                Secondary = true,
                Text = text,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderColor = borderSecond;
            button.Click += clickHandler;
            return button;
        }

        private static void ApplyHpDarkExplorerTheme(Control control)
        {
            void applyTheme()
            {
                if (control.IsHandleCreated)
                {
                    SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
                }
            }

            control.HandleCreated += (_, _) => applyTheme();
            applyTheme();
        }

        private void ButtonHpDiagnosticCopy_Click(object? sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(BuildHpDiagnosticSummary());
            }
            catch (ExternalException)
            {
                MessageBox.Show(this, "The diagnostic summary could not be copied to the clipboard.", "VictusX Diagnostic", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ButtonHpDiagnosticOpenReportFolder_Click(object? sender, EventArgs e)
        {
            string diagnosticDirectory = HpDiagnosticPaths.AppDataDirectory;
            if (!Directory.Exists(diagnosticDirectory))
            {
                MessageBox.Show(this, "The local VictusX diagnostic folder is not available yet:" + Environment.NewLine + diagnosticDirectory, "VictusX Diagnostic", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(diagnosticDirectory) { UseShellExecute = true });
            }
            catch (Exception)
            {
                MessageBox.Show(this, "The local VictusX diagnostic folder could not be opened.", "VictusX Diagnostic", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ButtonHpDiagnosticReload_Click(object? sender, EventArgs e)
        {
            ReloadHpCachedDiagnosticReport();
            PopulateHpReadOnlyTelemetryPanel();
            ApplyHpLiveTelemetry();
        }

        private void ButtonHpDiagnosticExport_Click(object? sender, EventArgs e)
        {
            try
            {
                string filePath = HpDiagnosticReportExporter.Export(BuildHpDiagnosticSummary());
                MessageBox.Show(this, HpDiagnosticStatusText.ReadOnlyDiagnostic + " report exported to:" + Environment.NewLine + filePath, "VictusX Diagnostic", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Logger.WriteLine("HP diagnostic export failed: " + ex.Message);
                MessageBox.Show(this, "The read-only diagnostic report could not be exported.", "VictusX Diagnostic", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ReloadHpCachedDiagnosticReport()
        {
            HpDiagnosticReportSchemaV2Refresher.TryRefreshExistingReport(HpVictusCapabilityProbe.ReportPath);
            hpCachedDiagnosticReport = HpDiagnosticReportLoader.Load(HpVictusCapabilityProbe.ReportPath);
            hpPulseHistory = HpFanMaxPulseHistoryLoader.Load(HpFanMaxExperimentLogWriter.ExperimentDirectory);
            hpFanProofGaps = HpFanProofGapAnalyzer.Analyze(HpFanMaxExperimentLogWriter.ExperimentDirectory, hpCachedDiagnosticReport);
            if (hpReadOnlyTelemetrySource is not null)
            {
                hpReadOnlyTelemetrySource.Text = hpCachedDiagnosticReport.SourceDescription;
            }
        }

        private void PopulateHpReadOnlyTelemetryPanel()
        {
            if (hpReadOnlyTelemetryDetails is null) return;

            hpReadOnlyTelemetryDetails.SuspendLayout();
            hpReadOnlyTelemetryDetails.Controls.Clear();
            hpReadOnlyTelemetryDetails.RowStyles.Clear();
            hpReadOnlyTelemetryDetails.RowCount = 0;

            HpDiagnosticDashboardInput input = CreateHpDiagnosticDashboardInput();
            UpdateHpDiagnosticHealthSummary(input);

            foreach (HpDiagnosticDashboardSection section in HpDiagnosticDashboardFormatter.BuildSections(input))
            {
                AddHpTelemetrySection(hpReadOnlyTelemetryDetails, section.Title);
                foreach (HpDiagnosticDashboardRow row in section.Rows)
                {
                    AddHpTelemetryRow(hpReadOnlyTelemetryDetails, row);
                }
            }

            hpReadOnlyTelemetryDetails.ResumeLayout();
        }

        private void PopulateHpUserDiagnosticSummary(HpDiagnosticUserSummaryInput input)
        {
            if (hpReadOnlyTelemetrySummary is null) return;

            IReadOnlyList<HpDiagnosticDashboardSection> sections = HpDiagnosticDashboardFormatter.BuildUserSummary(input);
            string signature = string.Join(Environment.NewLine,
                sections.SelectMany(section => section.Rows).Select(row => row.Label + "=" + row.Value));
            if (string.Equals(hpLastUserDiagnosticSummarySignature, signature, StringComparison.Ordinal)) return;

            hpLastUserDiagnosticSummarySignature = signature;
            hpReadOnlyTelemetrySummary.SuspendLayout();
            hpReadOnlyTelemetrySummary.Controls.Clear();
            hpReadOnlyTelemetrySummary.RowStyles.Clear();
            hpReadOnlyTelemetrySummary.RowCount = 0;
            foreach (HpDiagnosticDashboardSection section in sections)
            {
                AddHpTelemetrySection(hpReadOnlyTelemetrySummary, section.Title);
                foreach (HpDiagnosticDashboardRow row in section.Rows)
                    AddHpTelemetryRow(hpReadOnlyTelemetrySummary, row);
            }
            hpReadOnlyTelemetrySummary.ResumeLayout();
        }

        private void UpdateHpDiagnosticHealthSummary(HpDiagnosticDashboardInput input)
        {
            if (hpReadOnlyTelemetryHealth is null) return;

            HpDiagnosticDashboardHealthSummary summary = HpDiagnosticDashboardFormatter.BuildHealthSummary(input);
            hpReadOnlyTelemetryHealth.Text = HpDiagnosticDashboardFormatter.FormatHealthSummary(summary);
        }

        private string BuildHpDiagnosticSummary()
        {
            return "VictusX Read-only Diagnostic" + Environment.NewLine +
                HpDiagnosticDashboardFormatter.BuildSummary(CreateHpDiagnosticDashboardInput());
        }

        private HpDiagnosticDashboardInput CreateHpDiagnosticDashboardInput()
        {
            HpVictusCapabilitySnapshot? snapshot = Program.hpVictusCapabilitySnapshot;
            HpDiagnosticReportLoadResult? report = hpCachedDiagnosticReport;
            HpFanMaxPulseHistoryEntry? pulse = hpPulseHistory?.Entry;
            HpFanProofGapAnalysis? proofGaps = hpFanProofGaps;
            return new HpDiagnosticDashboardInput
            {
                ReportSchemaVersion = report?.GetValue("ReportSchemaVersion"),
                ReportGeneratedBy = report?.GetValue("ReportGeneratedBy"),
                ReportMode = report?.GetValue("ReportMode"),
                ReportSource = report?.GetValue("ReportSource"),
                ReportGeneratedAt = report?.GetValue("ReportGeneratedAtUtc") ?? report?.GetValue("Timestamp"),
                IsHpVictusDetected = snapshot?.IsHpVictus ?? report?.GetHpVictusDetected(),
                Manufacturer = GetSnapshotOrReportValue(snapshot?.Manufacturer, report, "Manufacturer"),
                Model = GetSnapshotOrReportValue(snapshot?.Model, report, "Model"),
                Sku = GetSnapshotOrReportValue(snapshot?.SystemSku, report, "Sku"),
                BiosVersion = GetSnapshotOrReportValue(snapshot?.BiosVersion, report, "BiosVersion"),
                RootWmiReadiness = GetSnapshotOrReportAvailability(snapshot?.RootWmiAvailability, report, "RootWmiAvailability"),
                HpqBIntMReadiness = GetSnapshotOrReportAvailability(snapshot?.HpqBIntMAvailability, report, "HpqBIntMAvailability"),
                HpqBDataInReadiness = GetSnapshotOrReportAvailability(snapshot?.HpqBDataInAvailability, report, "HpqBDataInAvailability"),
                SystemDesignDataDecodeStatus = FormatDecodedStatus(snapshot?.SystemDesignDataInvocationSucceeded == true && snapshot.SystemDesignDataDecodeSucceeded, report, "SystemDesignDataDecodeSucceeded"),
                ThermalPolicyVersion = GetHpThermalPolicyVersion(snapshot, report)?.ToString(),
                SoftwareFanControlSupport = FormatDeclaredSupport(snapshot, report),
                FanCount = GetSnapshotOrDecodedReportValue(snapshot?.FanGetCountInvocationSucceeded == true && snapshot.FanGetCountDecodeSucceeded, snapshot?.FanGetCountDecoded?.FanCount, report, "FanGetCountDecodeSucceeded", "FanGetCountDecoded.FanCount"),
                MaxFanState = FormatMaxFanState(snapshot, report),
                Fan1RawLevel = GetSnapshotOrDecodedReportValue(snapshot?.FanGetLevelInvocationSucceeded == true && snapshot.FanGetLevelDecodeSucceeded, snapshot?.FanGetLevelDecoded?.Fan1RawValue, report, "FanGetLevelDecodeSucceeded", "FanGetLevelDecoded.Fan1RawValue"),
                Fan2RawLevel = GetSnapshotOrDecodedReportValue(snapshot?.FanGetLevelInvocationSucceeded == true && snapshot.FanGetLevelDecodeSucceeded, snapshot?.FanGetLevelDecoded?.Fan2RawValue, report, "FanGetLevelDecodeSucceeded", "FanGetLevelDecoded.Fan2RawValue"),
                SetFanMaxPulseHistoryStatus = hpPulseHistory?.StatusText,
                SetFanMaxPulseTimestamp = pulse?.TimestampUtc.ToString("yyyy-MM-dd HH:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture),
                SetFanMaxPulsePayload = pulse is null ? null : (pulse.PayloadLengthCandidate ?? "Unknown") + " / " + (pulse.PayloadBytesHypothesis ?? "Unknown"),
                SetFanMaxPulseWriteExecuted = pulse?.WriteExecuted?.ToString(),
                SetFanMaxPulseEnableCommandSucceeded = pulse?.EnableCommandSucceeded?.ToString(),
                SetFanMaxPulseRestoreCommandSucceeded = pulse?.RestoreCommandSucceeded?.ToString(),
                SetFanMaxPulsePhysicalFanResponseObserved = pulse?.PhysicalFanResponseObserved?.ToString(),
                SetFanMaxPulseRestoreObserved = pulse?.RestoreObserved?.ToString(),
                SetFanMaxPulseReadbackReliability = pulse?.ReadbackReliability,
                SetFanMaxPulseOutcomeClassification = pulse?.ExperimentalOutcomeClassification,
                SetFanMaxPulseNotesSummary = pulse?.NotesSummary,
                FanProofGapEvidenceSources = proofGaps?.EvidenceSources,
                FanProofGapDeveloperPulseDecision = proofGaps?.DeveloperPulseDecision,
                FanProofGapDeviceValidatedInputLengthDecision = proofGaps?.DeviceValidatedInputLengthDecision,
                FanProofGapFanMaxGetDecision = proofGaps?.FanMaxGetDecision,
                FanProofGapFanGetLevelDecision = proofGaps?.FanGetLevelDecision,
                FanProofGapRestoreVerificationDecision = proofGaps?.RestoreVerificationDecision,
                FanProofGapRepeatabilityDecision = proofGaps?.RepeatabilityDecision,
                FanProofGapThermalPowerSafetyDecision = proofGaps?.ThermalPowerSafetyDecision,
                FanProofGapNormalFanControlDecision = proofGaps?.NormalFanControlDecision,
                SetFanMaxWriteImplemented = HpDiagnosticDashboardFormatter.FormatWriteImplementationStatus(snapshot?.SetFanMaxDryRun.SetFanMaxWriteImplemented ?? report?.GetBool("SetFanMaxWriteImplemented")),
                SetFanMaxWriteAllowed = HpDiagnosticDashboardFormatter.FormatWriteAllowedStatus(snapshot?.SetFanMaxDryRun.SetFanMaxWriteAllowed ?? report?.GetBool("SetFanMaxWriteAllowed")),
                SetFanMaxFirstWriteGateStatus = snapshot?.SetFanMaxDryRun.SetFanMaxFirstWriteGateStatus ?? report?.GetValue("SetFanMaxFirstWriteGateStatus"),
                SetFanMaxFirstWriteGateSatisfied = (snapshot?.SetFanMaxDryRun.SetFanMaxFirstWriteGateSatisfied ?? report?.GetBool("SetFanMaxFirstWriteGateSatisfied"))?.ToString(),
                SetFanMaxFirstWriteGateReason = snapshot?.SetFanMaxDryRun.SetFanMaxFirstWriteGateReason ?? report?.GetValue("SetFanMaxFirstWriteGateReason"),
                SetFanMaxExperimentalPayloadCandidate = snapshot?.SetFanMaxDryRun.SetFanMaxExperimentalPayloadCandidate ?? report?.GetValue("SetFanMaxExperimentalPayloadCandidate"),
                SetFanMaxPhysicalResponseObserved = (snapshot?.SetFanMaxDryRun.SetFanMaxPhysicalResponseObserved ?? report?.GetBool("SetFanMaxPhysicalResponseObserved"))?.ToString(),
                SetFanMaxPhysicalResponseConfirmationCount = snapshot?.SetFanMaxDryRun.SetFanMaxPhysicalResponseConfirmationCount.ToString() ?? report?.GetValue("SetFanMaxPhysicalResponseConfirmationCount"),
                SetFanMaxReadbackReliable = (snapshot?.SetFanMaxDryRun.SetFanMaxReadbackReliable ?? report?.GetBool("SetFanMaxReadbackReliable"))?.ToString(),
                SetFanMaxDeveloperExperimentAllowed = (snapshot?.SetFanMaxDryRun.SetFanMaxDeveloperExperimentAllowed ?? report?.GetBool("SetFanMaxDeveloperExperimentAllowed"))?.ToString(),
                SetFanMaxDeveloperExperimentPayload = snapshot?.SetFanMaxDryRun.SetFanMaxDeveloperExperimentPayload ?? report?.GetValue("SetFanMaxDeveloperExperimentPayload"),
                SetFanMaxNormalControlValidated = (snapshot?.SetFanMaxDryRun.SetFanMaxNormalControlValidated ?? report?.GetBool("SetFanMaxNormalControlValidated"))?.ToString(),
                SetFanMaxUserFacingControlAllowed = (snapshot?.SetFanMaxDryRun.SetFanMaxUserFacingControlAllowed ?? report?.GetBool("SetFanMaxUserFacingControlAllowed"))?.ToString(),
                SetFanMaxDeviceValidatedInputLength = snapshot?.SetFanMaxDryRun.SetFanMaxDeviceValidatedInputLength?.ToString() ?? report?.GetValue("SetFanMaxDeviceValidatedInputLength"),
                SetFanMaxBlockedReason = GetSnapshotOrReportValue(snapshot?.SetFanMaxDryRun.SetFanMaxDryRunBlockedReasons is { Length: > 0 } reasons ? string.Join(" | ", reasons) : null, report, "SetFanMaxDryRunBlockedReasons"),
                SetFanMaxNextRequiredProof = GetSnapshotOrReportValue(snapshot?.SetFanMaxDryRun.SetFanMaxNextRequiredProof, report, "SetFanMaxNextRequiredProof")
            };
        }

        private void AddHpTelemetrySection(TableLayoutPanel details, string title)
        {
            int row = details.RowCount++;
            details.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var label = new Label
            {
                AutoSize = true,
                BackColor = Color.FromArgb(32, foreMain),
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = foreMain,
                Margin = new Padding(0, 7, 0, 2),
                Padding = new Padding(6, 3, 6, 3),
                Text = title
            };
            details.Controls.Add(label, 0, row);
            details.SetColumnSpan(label, 2);
        }

        private void AddHpTelemetryRow(TableLayoutPanel details, HpDiagnosticDashboardRow row)
        {
            int layoutRow = details.RowCount++;
            details.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            details.Controls.Add(new Label
            {
                AutoSize = true,
                ForeColor = foreMain,
                Margin = new Padding(6, 2, 12, 2),
                Text = row.Label + ":"
            }, 0, layoutRow);
            details.Controls.Add(new Label
            {
                AutoSize = true,
                ForeColor = GetHpDiagnosticValueColor(row.Status),
                Margin = new Padding(0, 2, 0, 2),
                MaximumSize = new Size(560, 0),
                Text = row.Value
            }, 1, layoutRow);
        }

        private Color GetHpDiagnosticValueColor(HpDiagnosticDashboardStatus status)
        {
            return status switch
            {
                HpDiagnosticDashboardStatus.Ready => colorEco,
                HpDiagnosticDashboardStatus.Warning or HpDiagnosticDashboardStatus.Blocked => colorTurbo,
                _ => foreMain
            };
        }

        private static string GetSnapshotOrReportValue(string? snapshotValue, HpDiagnosticReportLoadResult? report, string reportPath)
        {
            return !string.IsNullOrWhiteSpace(snapshotValue)
                ? snapshotValue
                : report?.GetValue(reportPath) ?? "Not available";
        }

        private static string GetSnapshotOrReportAvailability(HpVictusProbeAvailability? snapshotValue, HpDiagnosticReportLoadResult? report, string reportPath)
        {
            if (snapshotValue.HasValue)
            {
                return snapshotValue.Value == HpVictusProbeAvailability.Available ? "Ready" : "Not available";
            }

            return string.Equals(report?.GetValue(reportPath), HpVictusProbeAvailability.Available.ToString(), StringComparison.OrdinalIgnoreCase)
                ? "Ready"
                : "Not available";
        }

        private static string FormatDecodedStatus(bool hasDecodedSnapshot, HpDiagnosticReportLoadResult? report, string decodeSucceededPath)
        {
            return hasDecodedSnapshot || report?.GetBool(decodeSucceededPath) == true ? "Succeeded" : "Not available";
        }

        private static string FormatDeclaredSupport(HpVictusCapabilitySnapshot? snapshot, HpDiagnosticReportLoadResult? report)
        {
            bool hasDecodedSystemDesignData = snapshot?.SystemDesignDataInvocationSucceeded == true && snapshot.SystemDesignDataDecodeSucceeded;
            return hasDecodedSystemDesignData
                ? snapshot!.SystemDesignDataDecoded?.DeclaresSoftwareFanControlSupport switch
                {
                    true => "Declared",
                    false => "Not declared",
                    _ => "Not available"
                }
                : report?.GetBool("SystemDesignDataDecodeSucceeded") == true
                    ? report.GetBool("SystemDesignDataDecoded.DeclaresSoftwareFanControlSupport") switch
                    {
                        true => "Declared",
                        false => "Not declared",
                        _ => "Not available"
                    }
                    : "Not available";
        }

        private static string FormatMaxFanState(HpVictusCapabilitySnapshot? snapshot, HpDiagnosticReportLoadResult? report)
        {
            bool hasDecodedFanMaxState = snapshot?.FanMaxGetInvocationSucceeded == true && snapshot.FanMaxGetDecodeSucceeded;
            return hasDecodedFanMaxState
                ? snapshot!.FanMaxGetDecoded?.IsMaxFanEnabled switch
                {
                    true => "Enabled",
                    false => "Disabled",
                    _ => "Not available"
                }
                : report?.GetBool("FanMaxGetDecodeSucceeded") == true
                    ? report.GetBool("FanMaxGetDecoded.IsMaxFanEnabled") switch
                    {
                        true => "Enabled",
                        false => "Disabled",
                        _ => "Not available"
                    }
                    : "Not available";
        }

        private static string GetSnapshotOrDecodedReportValue<T>(bool hasDecodedSnapshot, T? snapshotValue, HpDiagnosticReportLoadResult? report, string decodeSucceededPath, string valuePath) where T : struct
        {
            if (hasDecodedSnapshot && snapshotValue.HasValue)
            {
                return snapshotValue.Value.ToString() ?? "Not available";
            }

            return report?.GetBool(decodeSucceededPath) == true
                ? report.GetValue(valuePath) ?? "Not available"
                : "Not available";
        }

        private void ButtonArmoury_Click(object? sender, EventArgs e)
        {
            if (AppConfig.IsUnsupportedHardwareMode()) return;
            var dialogResult = MessageBox.Show(this, "Armoury Crate is active, download official uninstaller app?", "Armoury Crate", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes) AsusService.RunArmouryUninstaller();
        }


        private void ButtonAmdOled_Click(object? sender, EventArgs e)
        {
            AmdDisplay.RunAdrenaline();
            activateCheck = true;
        }

        private void LabelBattery_Click(object? sender, EventArgs e)
        {
            HardwareControl.chargeWatt = !HardwareControl.chargeWatt;
            RefreshSensors(true);
        }

        private void ButtonEnergySaver_Click(object? sender, EventArgs e)
        {
            KeyboardHook.KeyKeyPress(Keys.LWin, Keys.A);
        }

        private void LabelBacklight_Click(object? sender, EventArgs e)
        {
            if (AppConfig.IsDynamicLighting() && DynamicLightingHelper.IsEnabled()) DynamicLightingHelper.OpenSettings();
        }

        private void ButtonFHD_Click(object? sender, EventArgs e)
        {
            ScreenControl.ToogleFHD();
        }

        private void ButtonHDRControl_Click(object? sender, EventArgs e)
        {
            ScreenControl.ToogleHDRControl();
        }

        private void SliderBattery_ValueChanged(object? sender, EventArgs e)
        {
            VisualiseBatteryTitle(sliderBattery.Value);
        }

        private void SliderBattery_KeyUp(object? sender, KeyEventArgs e)
        {
            batteryTimer.Stop();
            batteryTimer.Start();
        }

        private void SliderBattery_MouseUp(object? sender, MouseEventArgs e)
        {
            batteryTimer.Stop();
            batteryTimer.Start();
        }

        private void ButtonAutoTDP_Click(object? sender, EventArgs e)
        {
            if (AppConfig.IsUnsupportedHardwareMode()) return;
            allyControl.ToggleAutoTDP();
        }

        private void LabelCharge_Click(object? sender, EventArgs e)
        {
            BatteryControl.BatteryReport();
        }

        private void LabelVisual_Click(object? sender, EventArgs e)
        {
            labelVisual.Visible = false;
            VisualControl.forceVisual = true;
        }

        public void InitVisual()
        {

            if (AppConfig.Is("hide_visual")) return;

            if (AppConfig.IsOLED())
            {
                panelGamma.Visible = true;
                sliderGamma.Visible = true;
                labelGammaTitle.Text = Properties.Strings.FlickerFreeDimming + " / " + Properties.Strings.VisualMode;

                VisualiseBrightness();

                sliderGamma.ValueChanged += SliderGamma_ValueChanged;
                sliderGamma.MouseUp += SliderGamma_ValueChanged;

            }
            else
            {
                labelGammaTitle.Text = Properties.Strings.VisualMode;
            }

            var gamuts = VisualControl.GetGamutModes();

            // Color profiles exist
            if (gamuts.Count > 0)
            {
                tableVisual.ColumnCount = 3;
                buttonInstallColor.Visible = false;
            }
            else
            {
                // If it's possible to retrieve color profiles
                if (ColorProfileHelper.ProfileExists())
                {
                    tableVisual.ColumnCount = 2;

                    buttonInstallColor.Text = Properties.Strings.DownloadColorProfiles;
                    buttonInstallColor.Visible = true;
                    buttonInstallColor.Click += ButtonInstallColorProfile_Click;

                    panelGamma.Visible = true;
                    tableVisual.Visible = true;
                }

                return;
            }

            panelGamma.Visible = true;
            tableVisual.Visible = true;

            var visualValue = (SplendidCommand)AppConfig.Get("visual", (int)VisualControl.GetDefaultVisualMode());
            var colorTempValue = AppConfig.Get("color_temp", VisualControl.DefaultColorTemp);

            comboVisual.DropDownStyle = ComboBoxStyle.DropDownList;
            comboVisual.DataSource = new BindingSource(VisualControl.GetVisualModes(), null);
            comboVisual.DisplayMember = "Value";
            comboVisual.ValueMember = "Key";
            comboVisual.SelectedValue = visualValue;

            comboColorTemp.DropDownStyle = ComboBoxStyle.DropDownList;
            comboColorTemp.DataSource = new BindingSource(VisualControl.GetTemperatures(), null);
            comboColorTemp.DisplayMember = "Value";
            comboColorTemp.ValueMember = "Key";
            comboColorTemp.SelectedValue = colorTempValue;

            VisualControl.SetVisual(visualValue, colorTempValue, true);

            comboVisual.SelectedValueChanged += ComboVisual_SelectedValueChanged;
            comboVisual.Visible = true;
            VisualiseDisabled();

            comboColorTemp.SelectedValueChanged += ComboVisual_SelectedValueChanged;
            comboColorTemp.Visible = true;

            if (gamuts.Count <= 1) return;

            comboGamut.DropDownStyle = ComboBoxStyle.DropDownList;
            comboGamut.DataSource = new BindingSource(gamuts, null);
            comboGamut.DisplayMember = "Value";
            comboGamut.ValueMember = "Key";
            comboGamut.SelectedValue = (SplendidGamut)AppConfig.Get("gamut", (int)VisualControl.GetDefaultGamut());

            comboGamut.SelectedValueChanged += ComboGamut_SelectedValueChanged;
            comboGamut.Visible = true;

        }

        public void CycleVisualMode(int delta)
        {

            if (comboVisual.Items.Count < 1) return;

            if (delta > 0)
            {
                if (comboVisual.SelectedIndex < comboVisual.Items.Count - 1)
                    comboVisual.SelectedIndex += 1;
                else
                    comboVisual.SelectedIndex = 0;
            }
            else
            {
                if (comboVisual.SelectedIndex > 0)
                    comboVisual.SelectedIndex -= 1;
                else
                    comboVisual.SelectedIndex = comboVisual.Items.Count - 1;
            }

            Program.toast.RunToast(comboVisual.GetItemText(comboVisual.SelectedItem), ToastIcon.BrightnessUp);
        }

        private async void ButtonInstallColorProfile_Click(object? sender, EventArgs e)
        {
            await ColorProfileHelper.InstallProfile();
            InitVisual();
        }

        private void ComboGamut_SelectedValueChanged(object? sender, EventArgs e)
        {
            VisualControl.SetGamut((int)comboGamut.SelectedValue);
        }

        private void ComboVisual_SelectedValueChanged(object? sender, EventArgs e)
        {
            VisualControl.SetVisual((SplendidCommand)comboVisual.SelectedValue, (int)comboColorTemp.SelectedValue);
            VisualiseDisabled();
        }

        public void VisualiseBrightness()
        {
            if (InvokeRequired) { Invoke(VisualiseBrightness); return; }
            sliderGammaIgnore = true;
            sliderGamma.Value = VisualControl.GetBrightness();
            labelGamma.Text = sliderGamma.Value + "%";
            sliderGammaIgnore = false;
        }

        public void VisualiseAmdOled(bool status = false)
        {
            if (InvokeRequired) { Invoke(() => VisualiseAmdOled(status)); return; }
            buttonAmdOled.Visible = status;
        }

        public void VisualiseArmoury(bool status = false)
        {
            if (InvokeRequired) { Invoke(() => VisualiseArmoury(status)); return; }
            buttonArmoury.Visible = status;
        }

        public void VisualiseDisabled()
        {
            comboGamut.Enabled = comboColorTemp.Enabled = (SplendidCommand)AppConfig.Get("visual") != SplendidCommand.Disabled;
        }

        public void VisualiseGamut()
        {
            if (InvokeRequired) { Invoke(VisualiseGamut); return; }
            if (comboGamut.Items.Count > 0) comboGamut.SelectedIndex = 0;
        }

        private void SliderGamma_ValueChanged(object? sender, EventArgs e)
        {
            if (sliderGammaIgnore) return;
            VisualControl.SetBrightness(sliderGamma.Value);
        }

        private void ButtonOverlay_Click(object? sender, EventArgs e)
        {
            ToggleOverlay();
        }

        private void ButtonHandheld_Click(object? sender, EventArgs e)
        {
            if (handheldForm == null || handheldForm.Text == "")
            {
                handheldForm = new Handheld();
                AddOwnedForm(handheldForm);
            }

            if (handheldForm.Visible)
            {
                handheldForm.Close();
            }
            else
            {
                //handheldForm.FormPosition();
                handheldForm.Show();
            }
        }

        private void ButtonFPS_Click(object? sender, EventArgs e)
        {
            if (AppConfig.IsUnsupportedHardwareMode()) return;
            allyControl.ToggleFPSLimit();
        }

        private void ButtonBacklight_Click(object? sender, EventArgs e)
        {
            if (AppConfig.IsUnsupportedHardwareMode()) return;
            allyControl.ToggleBacklight();
        }

        private void ButtonControllerMode_Click(object? sender, EventArgs e)
        {
            if (AppConfig.IsUnsupportedHardwareMode()) return;
            allyControl.ToggleMode();
        }

        public void VisualiseAlly(bool visible = false)
        {
            if (!visible) return;
            if (InvokeRequired) { Invoke(() => VisualiseAlly(visible)); return; }

            panelAlly.Visible = true;
            panelKeyboardTitle.Visible = false;
            panelKeyboard.Padding = new Padding(panelKeyboard.Padding.Left, 0, panelKeyboard.Padding.Right, panelKeyboard.Padding.Bottom);

            buttonOverlay.Text = Properties.Strings.Overlay;
            buttonOverlay.Activated = AppConfig.IsOverlay();

            tableAMD.Visible = true;
        }

        public void VisualiseController(ControllerMode mode)
        {
            switch (mode)
            {
                case ControllerMode.Gamepad:
                    buttonControllerMode.Text = "Gamepad";
                    break;
                case ControllerMode.Mouse:
                    buttonControllerMode.Text = "Mouse";
                    break;
                case ControllerMode.Skip:
                    buttonControllerMode.Text = "Skip";
                    break;
                default:
                    buttonControllerMode.Text = "Auto";
                    break;
            }
        }

        public void VisualiseBacklight(int backlight)
        {
            if (InvokeRequired) { Invoke(() => VisualiseBacklight(backlight)); return; }
            buttonBacklight.Text = Math.Round((double)backlight * 33.33).ToString() + "%";
        }

        public void VisualiseFPSLimit(int limit)
        {
            if (InvokeRequired) { Invoke(() => VisualiseFPSLimit(limit)); return; }
            buttonFPS.Text = "FPS Limit " + ((limit > 0 && limit <= 120) ? limit : "OFF");
        }

        public void VisualiseAutoTDP(bool status)
        {
            Logger.WriteLine($"Auto TDP: {status}");
            buttonAutoTDP.Activated = status;
        }

        private void SettingsForm_Focused(object? sender, EventArgs e)
        {
            if (activateCheck)
            {
                buttonAmdOled.Visible = AmdDisplay.IsOledPowerOptimization();
                activateCheck = false;
            }
        }
        private void SettingsForm_LostFocus(object? sender, EventArgs e)
        {
            lastLostFocus = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        private void ButtonBatteryFull_Click(object? sender, EventArgs e)
        {
            BatteryControl.ToggleBatteryLimitFull();
        }

        private void ButtonBatteryFull_MouseLeave(object? sender, EventArgs e)
        {
            batteryFullMouseOver = false;
            RefreshSensors(true);
        }

        private void ButtonBatteryFull_MouseEnter(object? sender, EventArgs e)
        {
            batteryFullMouseOver = true;
            labelCharge.Text = Properties.Strings.BatteryLimitFull;
        }

        private void SettingsForm_Resize(object? sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Normal)
            {
                WindowState = FormWindowState.Normal;
                return;
            }

            Left = Screen.FromControl(this).WorkingArea.Width - 10 - Width;
            Top = Screen.FromControl(this).WorkingArea.Height - 10 - Height;
        }

        private void PanelBattery_MouseEnter(object? sender, EventArgs e)
        {
            batteryMouseOver = true;
            ShowBatteryWear();
        }

        private void PanelBattery_MouseLeave(object? sender, EventArgs e)
        {
            batteryMouseOver = false;
            RefreshSensors(true);
        }

        private void ShowBatteryWear()
        {
            //Refresh again only after 15 Minutes since the last refresh
            if (lastBatteryRefresh == 0 || Math.Abs(DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastBatteryRefresh) > 15 * 60_000)
            {
                lastBatteryRefresh = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                HardwareControl.RefreshBatteryHealth();
            }

            if (HardwareControl.batteryHealth != -1)
            {
                labelCharge.Text = Properties.Strings.BatteryHealth + ": " + Math.Round(HardwareControl.batteryHealth, 1) + "%";
            }
        }

        private void SettingsForm_VisibleChanged(object? sender, EventArgs e)
        {
            sensorTimer.Enabled = !AppConfig.IsHpVictusHardwareMode() && (this.Visible || sensorsAlways);
            if (AppConfig.IsHpVictusHardwareMode() && hpLiveTelemetryTimer is not null)
            {
                hpLiveTelemetryTimer.Stop();
                hpLiveTelemetryProvider?.Reset();
                hpLiveTelemetry = HpReadOnlyTelemetrySnapshot.Unavailable;
                ApplyHpLiveTelemetry();
                if (Visible)
                {
                    RefreshHpLiveTelemetry();
                    hpLiveTelemetryTimer.Start();
                }
            }
            if (this.Visible && !AppConfig.IsHpVictusHardwareMode())
            {
                Task.Run((Action)RefreshPeripheralsBattery);
                updateControl.CheckForUpdates();
            }
        }

        private void RefreshPeripheralsBattery()
        {
            PeripheralsProvider.RefreshBatteryForAllDevices(true);
        }

        private void ButtonUpdates_Click(object? sender, EventArgs e)
        {
            if (AppConfig.IsHpVictusHardwareMode()) return;

            if (updatesForm == null || updatesForm.Text == "")
            {
                updatesForm = new Updates();
                AddOwnedForm(updatesForm);
            }

            if (updatesForm.Visible)
            {
                updatesForm.Close();
            }
            else
            {
                updatesForm.Show();
            }
        }

        public void VisualiseMatrixPicture(string image)
        {
            if (matrixForm == null || matrixForm.Text == "") return;
            matrixForm.VisualiseMatrix(image);
        }

        protected override void WndProc(ref Message m)
        {
            if (AppConfig.IsHpVictusHardwareMode())
            {
                if (m.Msg == Program.WM_TASKBARCREATED && Program.trayIcon is not null)
                {
                    Program.trayIcon.Visible = true;
                }

                base.WndProc(ref m);
                return;
            }

            if (m.Msg == NativeMethods.WM_POWERBROADCAST && m.WParam == (IntPtr)NativeMethods.PBT_APMSUSPEND)
            {
                Logger.WriteLine("System Suspend");
                GPUModeControl.suspended = true;
                Program.modeControl.SleepReset();
                m.Result = (IntPtr)1;
            }

            if (m.Msg == NativeMethods.WM_POWERBROADCAST && m.WParam == (IntPtr)NativeMethods.PBT_APMRESUMEAUTOMATIC)
            {
                Logger.WriteLine("System Resume");
                GPUModeControl.suspended = false;
                BatteryControl.AutoBattery();
                m.Result = (IntPtr)1;
            }

            if (m.Msg == NativeMethods.WM_POWERBROADCAST && m.WParam == (IntPtr)NativeMethods.PBT_POWERSETTINGCHANGE)
            {
                var settings = (NativeMethods.POWERBROADCAST_SETTING)m.GetLParam(typeof(NativeMethods.POWERBROADCAST_SETTING));
                if (settings.PowerSetting == NativeMethods.PowerSettingGuid.LIDSWITCH_STATE_CHANGE)
                {
                    switch (settings.Data)
                    {
                        case 0:
                            Logger.WriteLine("Lid Closed");
                            BatteryControl.AutoBattery();
                            InputDispatcher.lidClose = AniMatrixControl.lidClose = true;
                            Aura.ApplyBrightness(0, "Lid");
                            matrixControl.SetLidMode();
                            break;
                        case 1:
                            Logger.WriteLine("Lid Open");
                            InputDispatcher.InitFNLock();
                            InputDispatcher.lidClose = AniMatrixControl.lidClose = false;
                            Aura.ApplyBrightness(InputDispatcher.GetBacklight(), "Lid");
                            matrixControl.SetLidMode();
                            break;
                    }

                }
                else if (settings.PowerSetting == NativeMethods.PowerSettingGuid.EnergySaverStatus)
                {
                    Logger.WriteLine("Battery Saver: " + settings.Data);
                    buttonEnergySaver.Visible = settings.Data != 0;
                }
                else
                {
                    switch (settings.Data)
                    {
                        case 0:
                            Logger.WriteLine("Monitor Power Off");
                            Aura.SleepBrightness();
                            XGM.NotifyShutdown();
                            Program.hardwareOverlay?.SuspendForDisplayOff();
                            break;
                        case 1:
                            Logger.WriteLine("Monitor Power On");
                            GPUModeControl.suspended = false;
                            if (!Program.SetAutoModes(wakeup: true)) BatteryControl.AutoBattery();
                            Program.hardwareOverlay?.ResumeForDisplayOn();
                            break;
                        case 2:
                            Logger.WriteLine("Monitor Dimmed");
                            break;
                    }
                }
                m.Result = (IntPtr)1;
            }

            if (m.Msg == Program.WM_TASKBARCREATED)
            {
                Logger.WriteLine("Taskbar created, re-creating tray icon");
                if (Program.trayIcon is not null) Program.trayIcon.Visible = true;
            }

            try
            {
                base.WndProc(ref m);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public void SetContextMenu()
        {
            if (AppConfig.IsHpVictusHardwareMode())
            {
                SetHpReadOnlyContextMenu();
                return;
            }

            var currentMode = Modes.GetCurrent();

            foreach (ToolStripItem item in contextMenuStrip.Items.Cast<ToolStripItem>().ToList())
            {
                if (item is ToolStripMenuItem menuItem) menuItem.Dispose();
            }
            contextMenuStrip.Items.Clear();
            contextMenuStrip.ShowCheckMargin = true;
            contextMenuStrip.ImageScalingSize = new Size(16, 16);
            contextMenuStrip.ShowImageMargin = false;
            Padding padding = new Padding(5, 5, 5, 5);

            var title = new ToolStripMenuItem(Properties.Strings.PerformanceMode);
            title.Margin = padding;
            title.Enabled = false;
            contextMenuStrip.Items.Add(title);

            foreach (var mode in Modes.GetDictonary())
            {
                var menuMode = new ToolStripMenuItem(mode.Value);
                menuMode.Tag = mode.Key;
                menuMode.Click += (sender, args) => { Program.modeControl.SetPerformanceMode(mode.Key); };
                menuMode.Margin = padding;
                menuMode.Checked = (mode.Key == currentMode);
                contextMenuStrip.Items.Add(menuMode);
            }

            contextMenuStrip.Items.Add("-");

            if (isGpuSection)
            {
                var titleGPU = new ToolStripMenuItem(Properties.Strings.GPUMode);
                titleGPU.Margin = padding;
                titleGPU.Enabled = false;
                contextMenuStrip.Items.Add(titleGPU);

                menuEco = new ToolStripMenuItem(Properties.Strings.EcoMode);
                menuEco.Click += ButtonEco_Click;
                menuEco.Margin = padding;
                menuEco.Checked = buttonEco.Activated;
                contextMenuStrip.Items.Add(menuEco);

                menuStandard = new ToolStripMenuItem(Properties.Strings.StandardMode);
                menuStandard.Click += ButtonStandard_Click;
                menuStandard.Margin = padding;
                menuStandard.Checked = buttonStandard.Activated;
                contextMenuStrip.Items.Add(menuStandard);

                menuUltimate = new ToolStripMenuItem(Properties.Strings.UltimateMode);
                menuUltimate.Click += ButtonUltimate_Click;
                menuUltimate.Margin = padding;
                menuUltimate.Checked = buttonUltimate.Activated;
                menuUltimate.Visible = isMuxGpu;
                contextMenuStrip.Items.Add(menuUltimate);

                menuOptimized = new ToolStripMenuItem(Properties.Strings.Optimized);
                menuOptimized.Click += ButtonOptimized_Click;
                menuOptimized.Margin = padding;
                menuOptimized.Checked = buttonOptimized.Activated;
                contextMenuStrip.Items.Add(menuOptimized);

                contextMenuStrip.Items.Add("-");
            }

            var bwIcon = new ToolStripMenuItem(Properties.Strings.BWTrayIcon);
            bwIcon.Margin = padding;
            bwIcon.Checked = AppConfig.IsBWIcon();
            bwIcon.Click += (sender, args) =>
            {
                bwIcon.Checked = !bwIcon.Checked;
                AppConfig.Set("bw_icon", bwIcon.Checked ? 1 : 0);
                VisualiseIcon();
            };
            contextMenuStrip.Items.Add(bwIcon);

            contextMenuStrip.Items.Add("-");

            var menuOverlay = new ToolStripMenuItem(Properties.Strings.Overlay);
            menuOverlay.Click += (sender, args) => ToggleOverlay();
            menuOverlay.Margin = padding;
            menuOverlay.Checked = AppConfig.IsOverlay();
            contextMenuStrip.Items.Add(menuOverlay);

            var menuOverlayGameOnly = new ToolStripMenuItem(Properties.Strings.OverlayOnlyInGames);
            menuOverlayGameOnly.Click += (sender, args) => ToggleOverlayGameOnly();
            menuOverlayGameOnly.Margin = padding;
            menuOverlayGameOnly.Checked = AppConfig.IsOverlayGameOnly();
            menuOverlayGameOnly.Enabled = AppConfig.IsOverlay();
            contextMenuStrip.Items.Add(menuOverlayGameOnly);

            var quit = new ToolStripMenuItem(Properties.Strings.Quit);
            quit.Click += ButtonQuit_Click;
            quit.Margin = padding;
            contextMenuStrip.Items.Add(quit);

            //contextMenuStrip.ShowCheckMargin = true;
            contextMenuStrip.Renderer = new CustomMenuRenderer();

            InitContextMenuTheme();

            if (Program.trayIcon is not null) Program.trayIcon.ContextMenuStrip = contextMenuStrip;


        }

        private void SetHpReadOnlyContextMenu()
        {
            foreach (ToolStripItem item in contextMenuStrip.Items.Cast<ToolStripItem>().ToList())
            {
                if (item is ToolStripMenuItem menuItem) menuItem.Dispose();
            }

            contextMenuStrip.Items.Clear();
            hpTrayStatusItems.Clear();
            contextMenuStrip.ShowCheckMargin = true;
            contextMenuStrip.ShowImageMargin = false;
            contextMenuStrip.ImageScalingSize = new Size(16, 16);

            var openVictusX = new ToolStripMenuItem("Open VictusX");
            openVictusX.Click += (sender, args) => ShowHpReadOnlyMainShell();
            contextMenuStrip.Items.Add(openVictusX);

            var openDiagnostic = new ToolStripMenuItem("Open Diagnostic");
            openDiagnostic.Click += (sender, args) => ShowHpReadOnlyDiagnostic();
            contextMenuStrip.Items.Add(openDiagnostic);

            contextMenuStrip.Items.Add("-");
            string modeStatus = HpTrayIconSelector.FormatModeStatus(HpPerformanceModeStatus.CurrentBaseMode);
            hpTrayModeStatusItem = new ToolStripMenuItem(modeStatus)
            {
                Enabled = false,
                AccessibleName = modeStatus
            };
            contextMenuStrip.Items.Add(hpTrayModeStatusItem);
            foreach (string row in hpTrayTelemetryStatus.Rows)
            {
                var status = new ToolStripMenuItem(row)
                {
                    Enabled = false,
                    AccessibleName = row
                };
                hpTrayStatusItems.Add(status);
                contextMenuStrip.Items.Add(status);
            }
            contextMenuStrip.Items.Add("-");

            RefreshHpRefreshRateState();
            var refreshRate = new ToolStripMenuItem("Refresh Rate")
            {
                Enabled = hpDisplayRefreshRateState.IsAvailable
            };
            foreach (HpDisplayRefreshRateMenuItem model in HpDisplayRefreshRateControl.BuildMenu(hpDisplayRefreshRateState))
            {
                var rate = new ToolStripMenuItem(model.Text)
                {
                    Checked = model.IsCurrent,
                    Tag = model.RefreshRateHz
                };
                rate.Click += ButtonHpTrayRefreshRate_Click;
                refreshRate.DropDownItems.Add(rate);
            }
            contextMenuStrip.Items.Add(refreshRate);

            var quit = new ToolStripMenuItem(Properties.Strings.Quit);
            quit.Click += ButtonQuit_Click;
            contextMenuStrip.Items.Add(quit);

            contextMenuStrip.Renderer = new CustomMenuRenderer();
            InitContextMenuTheme();

            if (Program.trayIcon is not null) Program.trayIcon.ContextMenuStrip = contextMenuStrip;
        }

        private void UpdateHpTrayStatusItems()
        {
            if (hpTrayModeStatusItem is not null)
            {
                string modeStatus = HpTrayIconSelector.FormatModeStatus(HpPerformanceModeStatus.CurrentBaseMode);
                hpTrayModeStatusItem.Text = modeStatus;
                hpTrayModeStatusItem.AccessibleName = modeStatus;
            }

            IReadOnlyList<string> rows = hpTrayTelemetryStatus.Rows;
            for (int index = 0; index < Math.Min(rows.Count, hpTrayStatusItems.Count); index++)
            {
                hpTrayStatusItems[index].Text = rows[index];
                hpTrayStatusItems[index].AccessibleName = rows[index];
            }
        }

        private void ButtonHpTrayRefreshRate_Click(object? sender, EventArgs e)
        {
            if (!AppConfig.IsHpVictusHardwareMode() || sender is not ToolStripMenuItem { Tag: int requestedRate }) return;

            HpDisplayRefreshRateApplyResult result = ApplyHpRefreshRate(requestedRate);
            BeginInvoke((Action)SetHpReadOnlyContextMenu);
            if (!result.Succeeded)
                MessageBox.Show(result.Message, "VictusX Display", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void InitContextMenuTheme()
        {
            if (contextMenuStrip is not null)
            {
                contextMenuStrip.BackColor = this.BackColor;
                contextMenuStrip.ForeColor = this.ForeColor;
            }

            donateControl?.ApplyTheme();
        }

        private void ButtonXGM_Click(object? sender, EventArgs e)
        {
            gpuControl.ToggleXGM();
        }


        public void SetVersionLabel(string label, bool update = false)
        {
            if (InvokeRequired)
                Invoke(delegate
                {
                    labelVersion.Text = label;
                    if (update) labelVersion.ForeColor = colorTurbo;
                });
            else
            {
                labelVersion.Text = label;
                if (update) labelVersion.ForeColor = colorTurbo;
            }
        }


        private void LabelVersion_Click(object? sender, EventArgs e)
        {
            if (AppConfig.IsHpVictusHardwareMode()) return;

            updateControl.Update();
        }


        private static void OnTimedEvent(Object? source, ElapsedEventArgs? e)
        {
            if (AppConfig.IsHpVictusHardwareMode()) return;

            Program.settingsForm.RefreshSensors();
        }

        private void ButtonFHD_MouseHover(object? sender, EventArgs e)
        {
            labelTipScreen.Text = "Switch to " + ((buttonFHD.Text == "FHD") ? "UHD" : "FHD") + " Mode";
        }

        private void Button120Hz_MouseHover(object? sender, EventArgs e)
        {
            labelTipScreen.Text = Properties.Strings.MaxRefreshTooltip;
        }

        private void Button60Hz_MouseHover(object? sender, EventArgs e)
        {
            labelTipScreen.Text = Properties.Strings.MinRefreshTooltip.Replace("60", ScreenControl.MIN_RATE.ToString());
        }

        private void ButtonScreen_MouseLeave(object? sender, EventArgs e)
        {
            labelTipScreen.Text = "";
        }

        private void ButtonScreenAuto_MouseHover(object? sender, EventArgs e)
        {
            labelTipScreen.Text = Properties.Strings.AutoRefreshTooltip.Replace("60", ScreenControl.MIN_RATE.ToString());
        }

        private void ButtonUltimate_MouseHover(object? sender, EventArgs e)
        {
            labelTipGPU.Text = Properties.Strings.UltimateGPUTooltip;
        }

        private void ButtonStandard_MouseHover(object? sender, EventArgs e)
        {
            labelTipGPU.Text = Properties.Strings.StandardGPUTooltip;
        }

        private void ButtonEco_MouseHover(object? sender, EventArgs e)
        {
            labelTipGPU.Text = Properties.Strings.EcoGPUTooltip;
        }

        private void ButtonOptimized_MouseHover(object? sender, EventArgs e)
        {
            labelTipGPU.Text = Properties.Strings.OptimizedGPUTooltip;
        }

        private void ButtonGPU_MouseLeave(object? sender, EventArgs e)
        {
            labelTipGPU.Text = "";
        }

        private void ButtonXGM_MouseMove(object? sender, MouseEventArgs e)
        {
            if (sender is null) return;
            TableLayoutPanel table = (TableLayoutPanel)sender;

            if (!buttonXGM.Visible) return;

            labelTipGPU.Text = buttonXGM.Bounds.Contains(table.PointToClient(Cursor.Position)) ?
                "XGMobile toggle works only in Standard mode" : "";

        }


        private void ButtonScreenAuto_Click(object? sender, EventArgs e)
        {
            ScreenControl.SetAutoRefresh(1);
            ScreenControl.AutoScreen();
        }


        private void CheckStartup_CheckedChanged(object? sender, EventArgs e)
        {
            if (sender is null) return;
            CheckBox chk = (CheckBox)sender;

            if (chk.Checked)
                Startup.Schedule();
            else
                Startup.UnSchedule();
        }

        private void ButtonMatrix_Click(object? sender, EventArgs e)
        {

            if (AppConfig.IsUnsupportedHardwareMode()) return;
            if (matrixControl.IsSlash)
            {
                if (slashForm == null || slashForm.Text == "")
                {
                    slashForm = new Slash();
                    AddOwnedForm(slashForm);
                }

                if (slashForm.Visible)
                {
                    slashForm.Close();
                }
                else
                {
                    slashForm.FormPosition();
                    slashForm.Show();
                }

                return;
            }

            if (matrixForm == null || matrixForm.Text == "")
            {
                matrixForm = new Matrix();
                AddOwnedForm(matrixForm);
            }

            if (matrixForm.Visible)
            {
                matrixForm.Close();
            }
            else
            {
                matrixForm.FormPosition();
                matrixForm.Show();
            }

        }

        public void VisualiseMatrixRunning(int mode)
        {
            if (InvokeRequired) { Invoke(() => VisualiseMatrixRunning(mode)); return; }
            comboMatrixRunning.SelectedIndex = mode;
            if (comboMatrix.SelectedIndex == 0) comboMatrix.SelectedIndex = 3;
        }

        public void SetMatrixRunning(int mode)
        {
            if (AppConfig.IsUnsupportedHardwareMode()) return;
            VisualiseMatrixRunning(mode);
            AppConfig.Set("matrix_running", mode);
            matrixControl.SetDevice();
            if (!matrixControl.IsSlash && matrixForm != null && matrixForm.Text != "") matrixForm.VisualiseMode();
        }

        private void ComboMatrixRunning_SelectedValueChanged(object? sender, EventArgs e)
        {
            if (AppConfig.IsUnsupportedHardwareMode()) return;
            SetMatrixRunning(comboMatrixRunning.SelectedIndex);
            if (!matrixControl.IsSlash && comboMatrixRunning.SelectedIndex == (int)MatrixMode.Text && (matrixForm == null || !matrixForm.Visible)) ButtonMatrix_Click(sender, e);
        }


        private void ComboMatrix_SelectedValueChanged(object? sender, EventArgs e)
        {
            if (AppConfig.IsUnsupportedHardwareMode()) return;
            AppConfig.Set("matrix_brightness", comboMatrix.SelectedIndex);
            matrixControl.SetDevice();
        }


        private void LabelCPUFan_Click(object? sender, EventArgs e)
        {
            FanSensorControl.fanRpm = !FanSensorControl.fanRpm;
            RefreshSensors(true);
        }

        private void ButtonKeyboardColor2_Click(object? sender, EventArgs e)
        {
            if (AppConfig.IsUnsupportedHardwareMode()) return;
            SetColorPicker("aura_color2", Aura.Color2);
        }

        private void ButtonKeyboard_Click(object? sender, EventArgs e)
        {
            if (extraForm == null || extraForm.Text == "")
            {
                extraForm = new Extra();
                AddOwnedForm(extraForm);
            }

            if (extraForm.Visible)
            {
                extraForm.Close();
            }
            else
            {
                extraForm.Show();
            }
        }

        public void FansInit()
        {
            if (fansForm == null || fansForm.Text == "") return;
            Invoke(fansForm.InitAll);
        }

        public void GPUInit()
        {
            if (fansForm == null || fansForm.Text == "") return;
            Invoke(fansForm.InitGPU);
        }

        public void FansToggle(int index = 0)
        {
            if (AppConfig.IsUnsupportedHardwareMode()) return;
            if (fansForm == null || fansForm.Text == "")
            {
                fansForm = new Fans();
                AddOwnedForm(fansForm);
            }

            if (fansForm.Visible)
            {
                fansForm.Close();
            }
            else
            {
                fansForm.FormPosition();
                fansForm.Show();
                fansForm.ToggleNavigation(index);
            }

        }

        private void ButtonFans_Click(object? sender, EventArgs e)
        {
            FansToggle();
        }

        private void SetColorPicker(string colorField, Color initial)
        {
            RColorPicker colorDlg = new RColorPicker(initial, colorField == "aura_color" && Aura.HasRandomColor());
            colorDlg.ColorChanged += c =>
            {
                AppConfig.Set(colorField, c.ToArgb());
                SetAura();
            };
            colorDlg.ShowDialog(this);
        }

        private void ButtonKeyboardColor_Click(object? sender, EventArgs e)
        {
            if (AppConfig.IsUnsupportedHardwareMode()) return;
            SetColorPicker("aura_color", Aura.Color1);
        }

        private void ButtonRearColor_Click(object? sender, EventArgs e)
        {
            if (AppConfig.IsUnsupportedHardwareMode()) return;
            SetColorPicker("rear_color", Aura.RearColor);
        }

        private void ComboRearLight_SelectedValueChanged(object? sender, EventArgs e)
        {
            if (AppConfig.IsUnsupportedHardwareMode()) return;
            AppConfig.Set("rear_mode", (int)comboRearLight.SelectedValue);
            SetAura();
        }

        public void InitRearLight()
        {
            if (AppConfig.IsUnsupportedHardwareMode()) return;
            if (!AppConfig.HasRearLight())
                return;

            Aura.RearMode = (AuraMode)AppConfig.Get("rear_mode");
            Aura.SetRearColor(AppConfig.Get("rear_color"));

            comboRearLight.DropDownStyle = ComboBoxStyle.DropDownList;
            comboRearLight.DataSource = new BindingSource(Aura.GetRearModes(), null);
            comboRearLight.DisplayMember = "Value";
            comboRearLight.ValueMember = "Key";
            comboRearLight.SelectedValue = Aura.RearMode;
            comboRearLight.SelectedValueChanged += ComboRearLight_SelectedValueChanged;

            buttonRearColor.Click += ButtonRearColor_Click;

            buttonRearColor.SwatchColor = Aura.RearColor;
            panelRearLight.Visible = true;
        }

        public void InitAura()
        {
            if (AppConfig.IsUnsupportedHardwareMode()) return;
            comboKeyboard.DropDownStyle = ComboBoxStyle.DropDownList;
            if (!Aura.IsBacklightDetected)
                Aura.Init();

            Aura.Mode = (AuraMode)AppConfig.Get("aura_mode");
            Aura.Speed = (AuraSpeed)AppConfig.Get("aura_speed");
            Aura.SetColor(AppConfig.Get("aura_color"));
            Aura.SetColor2(AppConfig.Get("aura_color2"));

            comboKeyboard.DataSource = new BindingSource(Aura.GetModes(), null);
            comboKeyboard.DisplayMember = "Value";
            comboKeyboard.ValueMember = "Key";
            comboKeyboard.SelectedValue = Aura.Mode;
            comboKeyboard.SelectedValueChanged += ComboKeyboard_SelectedValueChanged;


            if (Aura.isWhite)
            {
                buttonKeyboardColor.Visible = false;
            }

            if (AppConfig.NoAura())
            {
                comboKeyboard.Visible = false;
            }

            VisualiseAura();

            InitRearLight();
        }

        public void SetAura()
        {
            if (AppConfig.IsUnsupportedHardwareMode()) return;
            Task.Run(() =>
            {
                Aura.ApplyAura();
                VisualiseAura();
            });
        }

        private void _VisualiseAura()
        {
            buttonKeyboardColor.SwatchColor = Aura.Color1;
            buttonKeyboardColor.SwatchColor2 = Aura.HasSecondColor() ? Aura.Color2 : (Color?)null;

            if (panelRearLight.Visible) buttonRearColor.SwatchColor = Aura.RearColor;

            bool dynamic = AppConfig.IsDynamicLighting() && DynamicLightingHelper.IsEnabled() && !AppConfig.IsDynamicLightingOnly();

            if (dynamic)
            {
                labelBacklight.Cursor = Cursors.Hand;
                labelBacklight.Text = Strings.DisableDynamicLighting;
            } else if (Aura.Mode == AuraMode.AMBIENT)
            {
                labelBacklight.Cursor = Cursors.Default;
                labelBacklight.Text = Strings.AmbientModeResources;
            } else
            {
                labelBacklight.Cursor = Cursors.Default;
                labelBacklight.Text = "";
            }
        }

        public void VisualiseAura()
        {
            if (InvokeRequired)
                Invoke(_VisualiseAura);
            else
                _VisualiseAura();
        }

        public void InitMatrix()
        {

            if (AppConfig.IsUnsupportedHardwareMode()) return;
            if (!matrixControl.IsValid)
            {
                panelMatrix.Visible = false;
                return;
            }

            if (matrixControl.IsSlash)
            {
                labelMatrix.Text = "Slash Lighting";
                pictureMatrix.BackgroundImage = ControlHelper.TintImage(Properties.Resources.slash_32, foreMain);
                comboMatrixRunning.Items.Clear();

                foreach (var item in SlashDevice.Modes)
                {
                    comboMatrixRunning.Items.Add(item.Value);
                }

                buttonMatrix.Text = "Slash";
            }

            comboMatrix.SelectedIndex = Math.Max(0, Math.Min(AppConfig.Get("matrix_brightness", 0), comboMatrix.Items.Count - 1));
            comboMatrixRunning.SelectedIndex = Math.Min(AppConfig.Get("matrix_running", 0), comboMatrixRunning.Items.Count - 1);
        }


        public void CycleMatrix(int delta)
        {
            if (AppConfig.IsUnsupportedHardwareMode()) return;
            comboMatrix.SelectedIndex = Math.Min(Math.Max(0, comboMatrix.SelectedIndex + delta), comboMatrix.Items.Count - 1);
            AppConfig.Set("matrix_brightness", comboMatrix.SelectedIndex);
            matrixControl.SetDevice();
            Program.toast.RunToast(comboMatrix.GetItemText(comboMatrix.SelectedItem), delta > 0 ? ToastIcon.BacklightUp : ToastIcon.BacklightDown);
        }


        public void CycleAuraMode(int delta)
        {
            if (delta > 0)
            {
                if (comboKeyboard.SelectedIndex < comboKeyboard.Items.Count - 1)
                    comboKeyboard.SelectedIndex += 1;
                else
                    comboKeyboard.SelectedIndex = 0;
            }
            else
            {
                if (comboKeyboard.SelectedIndex > 0)
                    comboKeyboard.SelectedIndex -= 1;
                else
                    comboKeyboard.SelectedIndex = comboKeyboard.Items.Count - 1;
            }

            Program.toast.RunToast(comboKeyboard.GetItemText(comboKeyboard.SelectedItem), ToastIcon.BacklightUp);
        }

        private void ComboKeyboard_SelectedValueChanged(object? sender, EventArgs e)
        {
            AppConfig.Set("aura_mode", (int)comboKeyboard.SelectedValue);
            SetAura();
        }


        private void Button120Hz_Click(object? sender, EventArgs e)
        {
            ScreenControl.SetAutoRefresh(0);
            ScreenControl.SetScreen(ScreenControl.MAX_REFRESH, 1);
        }

        private void Button60Hz_Click(object? sender, EventArgs e)
        {
            ScreenControl.SetAutoRefresh(0);
            ScreenControl.SetScreen(ScreenControl.MIN_RATE, 0);
        }


        private void ButtonMiniled_Click(object? sender, EventArgs e)
        {
            ScreenControl.ToogleMiniled();
        }



        public void VisualiseScreen(bool screenEnabled, bool screenAuto, int frequency, int maxFrequency, int overdrive, bool overdriveSetting, int miniled1, int miniled2, bool hdr, bool acm, int fhd, int hdrControl)
        {
            bool advancedColor = hdr || acm;

            ButtonEnabled(button60Hz, screenEnabled);
            ButtonEnabled(button120Hz, screenEnabled);
            ButtonEnabled(buttonScreenAuto, screenEnabled);
            ButtonEnabled(buttonMiniled, screenEnabled);

            labelSreen.Text = screenEnabled
                ? Properties.Strings.LaptopScreen + ": " + frequency + "Hz" + ((overdrive == 1) ? " + " + Properties.Strings.Overdrive : "")
                : Properties.Strings.LaptopScreen + ": " + Properties.Strings.TurnedOff;

            panelScreen.AccessibleName = labelSreen.Text;

            button60Hz.Activated = false;
            button120Hz.Activated = false;
            buttonScreenAuto.Activated = false;

            if (screenAuto)
            {
                buttonScreenAuto.Activated = true;
            }
            else if (frequency == ScreenControl.MIN_RATE)
            {
                button60Hz.Activated = true;
            }
            else if (frequency > ScreenControl.MIN_RATE)
            {
                button120Hz.Activated = true;
            }

            button60Hz.Text = ScreenControl.MIN_RATE + "Hz";

            if (maxFrequency > ScreenControl.MIN_RATE)
            {
                button120Hz.Text = maxFrequency.ToString() + "Hz" + (overdriveSetting ? " + OD" : "");
                panelScreen.Visible = true;
                tableScreen.Visible = true;
            }
            else if (maxFrequency > 0)
            {
                tableScreen.Visible = false;
                panelScreen.Visible = AppConfig.NoGpu();
            }

            if (fhd >= 0)
            {
                buttonFHD.Visible = true;
                buttonFHD.Text = fhd > 0 ? "FHD" : "UHD";
            }

            bool hdrControlVisible = (hdr && hdrControl >= 0);

            if (miniled1 >= 0)
            {
                buttonMiniled.Visible = !hdrControlVisible;
                buttonMiniled.Enabled = !hdr;
                buttonMiniled.Activated = miniled1 == 1 || hdr;
            }
            else if (miniled2 >= 0)
            {
                buttonMiniled.Visible = !hdrControlVisible;
                buttonMiniled.Enabled = !hdr;
                if (hdr) miniled2 = 1; // Show HDR as Multizone Strong

                switch (miniled2)
                {
                    // Multizone On
                    case 0:
                        buttonMiniled.Text = Properties.Strings.Multizone;
                        buttonMiniled.BorderColor = colorStandard;
                        buttonMiniled.Activated = true;
                        break;
                    // Multizone Strong
                    case 1:
                        buttonMiniled.Text = Properties.Strings.MultizoneStrong;
                        buttonMiniled.BorderColor = colorTurbo;
                        buttonMiniled.Activated = true;
                        break;
                    // Multizone Off
                    case 2:
                        buttonMiniled.Text = Properties.Strings.OneZone;
                        buttonMiniled.BorderColor = colorStandard;
                        buttonMiniled.Activated = false;
                        break;
                }
            }
            else
            {
                buttonMiniled.Visible = false;
            }

            if (hdrControlVisible)
            {
                buttonHDRControl.Visible = true;
                buttonHDRControl.Activated = hdrControl > 0;
                buttonHDRControl.BorderColor = colorTurbo;
            } else
            {
                buttonHDRControl.Visible = false;
            }

            if (advancedColor) labelVisual.Text = Properties.Strings.VisualModesHDR;
            if (!screenEnabled) labelVisual.Text = Properties.Strings.VisualModesScreen;

            if (!screenEnabled || advancedColor)
            {
                labelVisual.Location = tableVisual.Location;
                labelVisual.Width = tableVisual.Width;
                labelVisual.Height = tableVisual.Height;
                labelVisual.Visible = true;
            }
            else
            {
                labelVisual.Visible = false;
            }


        }

        private void ButtonQuit_Click(object? sender, EventArgs e)
        {
            if (AppConfig.IsHpVictusHardwareMode())
            {
                Program.ExitHpDiagnosticShell();
                return;
            }

            AsusLampArray.Release();
            matrixControl.Dispose();
            Close();
            Program.trayIcon.Visible = false;
            Application.Exit();
        }

        /// <summary>
        /// Closes all forms except the settings. Hides the settings
        /// </summary>
        public void HideAll()
        {
            this.Hide();
            if (fansForm != null && fansForm.Text != "") fansForm.Close();
            if (extraForm != null && extraForm.Text != "") extraForm.Close();
            if (updatesForm != null && updatesForm.Text != "") updatesForm.Close();
            if (hpDiagnosticForm != null && hpDiagnosticForm.Text != "") hpDiagnosticForm.Hide();
            if (matrixForm != null && matrixForm.Text != "") matrixForm.Close();
            if (slashForm != null && slashForm.Text != "") slashForm.Close();
            if (handheldForm != null && handheldForm.Text != "") handheldForm.Close();
            if (mouseSettings != null && mouseSettings.Text != "") mouseSettings.Close();
            if (keyboardSettings != null && keyboardSettings.Text != "") keyboardSettings.Close();
            MemoryHelper.TrimAfter();
        }

        /// <summary>
        /// Brings all visible windows to the top, with settings being the focus
        /// </summary>
        public void ShowAll()
        {
            this.Activate();
            this.TopMost = true;
            this.TopMost = AppConfig.Is("topmost");
        }

        /// <summary>
        /// Check if any of fans, keyboard, update, or itself has focus
        /// </summary>
        /// <returns>Focus state</returns>
        public bool HasAnyFocus(bool lostFocusCheck = false)
        {
            return (fansForm != null && fansForm.ContainsFocus) ||
                   (extraForm != null && extraForm.ContainsFocus) ||
                   (updatesForm != null && updatesForm.ContainsFocus) ||
                   (matrixForm != null && matrixForm.ContainsFocus) ||
                   (slashForm != null && slashForm.ContainsFocus) ||
                   (handheldForm != null && handheldForm.ContainsFocus) ||
                   this.ContainsFocus ||
                   (lostFocusCheck && Math.Abs(DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastLostFocus) < 300);
        }

        private void SettingsForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                HideAll();
            }
        }

        private void ButtonUltimate_Click(object? sender, EventArgs e)
        {
            gpuControl.SetGPUMode(AsusACPI.GPUModeUltimate);
        }

        private void ButtonStandard_Click(object? sender, EventArgs e)
        {
            gpuControl.SetGPUMode(AsusACPI.GPUModeStandard);
        }

        private void ButtonEco_Click(object? sender, EventArgs e)
        {
            gpuControl.SetGPUMode(AsusACPI.GPUModeEco);
        }


        private void ButtonOptimized_Click(object? sender, EventArgs e)
        {
            if (AppConfig.IsUnsupportedHardwareMode()) return;
            AppConfig.Set("gpu_auto", (AppConfig.Get("gpu_auto") == 1) ? 0 : 1);
            VisualiseGPUMode();
            gpuControl.AutoGPUMode(true);
        }

        private void ButtonStopGPU_Click(object? sender, EventArgs e)
        {
            if (AppConfig.IsUnsupportedHardwareMode()) return;
            gpuControl.KillGPUApps();
        }

        public async void RefreshSensors(bool force = false)
        {
            if (AppConfig.IsHpVictusHardwareMode()) return;

            int throttle = (!Visible && sensorsAlways) ? 6000 : 2000;
            if (!force && Math.Abs(DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastRefresh) < throttle) return;
            lastRefresh = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            string cpuTemp = "";
            string gpuTemp = "";

            string cpuFan = "";
            string gpuFan = "";
            string midFan = "";

            string battery = "";
            string charge = "";

            await Task.Run(() => HardwareControl.ReadSensors());
            if (Visible) _ = Task.Run((Action)PeripheralsProvider.RefreshBatteryForAllDevices);

            if (HardwareControl.cpuTemp > 0)
                cpuTemp = ": " + TempHelper.FormatTemp((double)HardwareControl.cpuTemp);

            if (HardwareControl.batteryCapacity > 0)
            {
                charge = Properties.Strings.BatteryCharge + ": " + HardwareControl.batteryCharge;
            }

            if (HardwareControl.batteryRate < 0)
                battery = Properties.Strings.Discharging + ": " + Math.Round(-(decimal)HardwareControl.batteryRate, 1).ToString() + "W";
            else if (HardwareControl.batteryRate > 0)
                battery = Properties.Strings.Charging + ": " + Math.Round((decimal)HardwareControl.batteryRate, 1).ToString() + "W";


            if (HardwareControl.gpuTemp > 0)
            {
                gpuTemp = ": " + TempHelper.FormatTemp((double)HardwareControl.gpuTemp);
            }

            if (HardwareControl.cpuFan is not null) cpuFan = Strings.FanSpeed + ": " + HardwareControl.cpuFan;
            if (HardwareControl.gpuFan is not null) gpuFan = Strings.FanSpeed + ": " + HardwareControl.gpuFan;
            if (HardwareControl.midFan is not null) midFan = Strings.FanSpeed + ": " + HardwareControl.midFan;

            string trayTip = "CPU" + cpuTemp + " " + cpuFan;
            if (gpuTemp.Length > 0) trayTip += "\nGPU" + gpuTemp + " " + gpuFan;
            if (battery.Length > 0) trayTip += "\n" + battery;
            
            if (Program.settingsForm.IsHandleCreated)
                Program.settingsForm.BeginInvoke(delegate
                {
                    labelCPUFan.Text = "CPU" + cpuTemp + "  " + cpuFan;
                    labelGPUFan.Text = "GPU" + gpuTemp + "  " + gpuFan;

                    if (HardwareControl.gpuFan is not null && AppConfig.NoGpu())
                        labelMidFan.Text = "GPU" + gpuTemp + " " + gpuFan;

                    if (HardwareControl.midFan is not null) 
                        labelMidFan.Text = "Mid " + midFan;
                    
                    labelBattery.Text = battery;
                    if (!batteryMouseOver && !batteryFullMouseOver) labelCharge.Text = charge;
                });

            if (Program.trayIcon is not null) Program.trayIcon.Text = trayTip;
        }

        public void LabelFansResult(string text)
        {
            if (fansForm != null && !fansForm.IsDisposed && fansForm.Text != "")
                fansForm.LabelFansResult(text);
        }

        public void ToggleOverlay(bool fromHotkey = false)
        {
            bool enable = !AppConfig.IsOverlay();
            AppConfig.Set("overlay", enable ? 1 : 0);
            Logger.WriteLine("Overlay " + (enable ? "On" : "Off") + (AppConfig.IsOverlayGameOnly() ? " (game only)" : ""));
            if (enable)
                Program.hardwareOverlay?.StartOverlay();
            else
                Program.hardwareOverlay?.StopOverlay();

            buttonOverlay.Activated = enable;

            if (fromHotkey && AppConfig.IsOverlayGameOnly())
                Program.toast.RunToast(Properties.Strings.Overlay + " " + (enable ? Properties.Strings.On : Properties.Strings.Off));

            SetContextMenu();
        }

        public void ToggleOverlayGameOnly()
        {
            AppConfig.Set("overlay_game_only", AppConfig.IsOverlayGameOnly() ? 0 : 1);
            if (AppConfig.IsOverlay())
            {
                Program.hardwareOverlay?.StopOverlay();
                Program.hardwareOverlay?.StartOverlay();
            }
            SetContextMenu();
        }

        public void ShowMode(int mode)
        {
            if (InvokeRequired)
                Invoke(delegate
                {
                    VisualiseMode(mode);
                });
            else
                VisualiseMode(mode);
        }

        protected void VisualiseMode(int mode)
        {
            buttonSilent.Activated = false;
            buttonBalanced.Activated = false;
            buttonTurbo.Activated = false;
            buttonFans.Activated = false;

            switch (mode)
            {
                case AsusACPI.PerformanceSilent:
                    buttonSilent.Activated = true;
                    break;
                case AsusACPI.PerformanceTurbo:
                    buttonTurbo.Activated = true;
                    break;
                case AsusACPI.PerformanceBalanced:
                    buttonBalanced.Activated = true;
                    break;
                default:
                    buttonFans.Activated = true;
                    buttonFans.BorderColor = Modes.GetBase(mode) switch
                    {
                        AsusACPI.PerformanceSilent => colorEco,
                        AsusACPI.PerformanceTurbo => colorTurbo,
                        AsusACPI.PerformanceFullSpeed => Color.Orange,
                        _ => colorStandard,
                    };
                    break;
            }

            foreach (var item in contextMenuStrip.Items)
            {
                if (item is ToolStripMenuItem menuItem && menuItem.Tag is not null)
                {
                    menuItem.Checked = ((int)menuItem.Tag == mode);
                }
            }
        }


        public void SetModeLabel(string modeText)
        {
            if (InvokeRequired)
            {
                Invoke(delegate
                {
                    labelPerf.Text = modeText;
                    panelPerformance.AccessibleName = labelPerf.Text;
                });
            }
            else
            {
                labelPerf.Text = modeText;
                panelPerformance.AccessibleName = labelPerf.Text;
            }

        }



        public void VisualizeXGM(int GPUMode = -1)
        {

            if (AppConfig.IsUnsupportedHardwareMode()) { Invoke(() => VisualizeXGM(false, -1, GPUMode)); return; }
            bool connected = Program.acpi.IsXGConnected();
            int activated = connected ? Program.acpi.DeviceGet(AsusACPI.GPUXG) : -1;
            Invoke(() => VisualizeXGM(connected, activated, GPUMode));
        }

        void VisualizeXGM(bool connected, int activated, int GPUMode)
        {
            buttonXGM.Enabled = buttonXGM.Visible = connected;

            if (!connected) return;

            if (GPUMode != -1)
                ButtonEnabled(buttonXGM, AppConfig.IsAMDiGPU() || GPUMode != AsusACPI.GPUModeEco);


            Logger.WriteLine("XGM Activated flag: " + activated);

            buttonXGM.Activated = activated == 1;

            if (activated == 1)
            {
                ButtonEnabled(buttonOptimized, false);
                ButtonEnabled(buttonEco, false);
                ButtonEnabled(buttonStandard, false);
                ButtonEnabled(buttonUltimate, false);
            }
            else
            {
                ButtonEnabled(buttonOptimized, true);
                ButtonEnabled(buttonEco, true);
                ButtonEnabled(buttonStandard, true);
                ButtonEnabled(buttonUltimate, true);
            }

        }

        public void VisualiseGPUButtons(bool eco = true, bool ultimate = true)
        {
            if (InvokeRequired) { Invoke(() => VisualiseGPUButtons(eco, ultimate)); return; }
            isMuxGpu = ultimate;

            if (!eco)
            {
                menuEco.Visible = buttonEco.Visible = false;
                menuOptimized.Visible = buttonOptimized.Visible = false;
                buttonStopGPU.Visible = true;
                tableGPU.ColumnCount = 3;
                tableScreen.ColumnCount = 3;
            }
            else
            {
                buttonStopGPU.Visible = false;
            }

            if (!ultimate)
            {
                menuUltimate.Visible = buttonUltimate.Visible = false;
                tableGPU.ColumnCount = 3;
                tableScreen.ColumnCount = 3;
            }
        }

        public void HideGPUModes(bool gpuExists)
        {
            isGpuSection = false;

            buttonEco.Visible = false;
            buttonStandard.Visible = false;
            buttonUltimate.Visible = false;
            buttonOptimized.Visible = false;
            buttonStopGPU.Visible = true;

            tableGPU.ColumnCount = 0;

            SetContextMenu();

            panelGPU.Visible = gpuExists;

        }


        public void LockGPUModes(string text = null)
        {
            if (InvokeRequired) { Invoke(() => LockGPUModes(text)); return; }
            if (text is null) text = Properties.Strings.GPUMode + ": " + Properties.Strings.GPUChanging + " ...";

            ButtonEnabled(buttonOptimized, false);
            ButtonEnabled(buttonEco, false);
            ButtonEnabled(buttonStandard, false);
            ButtonEnabled(buttonUltimate, false);
            ButtonEnabled(buttonXGM, false);

            labelGPU.Text = text;
        }

        public void VisualiseGPUMode(int GPUMode = -1)
        {
            if (AppConfig.IsHpVictusHardwareMode()) return;

            if (InvokeRequired) { Invoke(() => VisualiseGPUMode(GPUMode)); return; }

            if (toolTip.GetToolTip(pictureGPU) != (GPUModeControl.gpuError ?? ""))
            {
                pictureGPU.BackgroundImage = GPUModeControl.gpuError is null ? Properties.Resources.icons8_video_card_32 : SystemIcons.Warning.ToBitmap();
                pictureGPU.Cursor = GPUModeControl.gpuError is null ? Cursors.Default : Cursors.Hand;
                toolTip.SetToolTip(pictureGPU, GPUModeControl.gpuError);
            }

            if (AppConfig.IsAlly())
            {
                tableGPU.Visible = false;
                labelGPU.Text = "GPU";
                if (Program.acpi.IsXGConnected())
                {
                    tableAMD.Controls.Add(buttonXGM, 1, 0);
                    VisualizeXGM();
                }
                VisualiseIcon();
                return;
            }

            ButtonEnabled(buttonOptimized, true);
            ButtonEnabled(buttonEco, true);
            ButtonEnabled(buttonStandard, true);
            ButtonEnabled(buttonUltimate, true);

            if (GPUMode == -1)
                GPUMode = AppConfig.Get("gpu_mode");

            bool GPUAuto = AppConfig.Is("gpu_auto");

            buttonEco.Activated = false;
            buttonStandard.Activated = false;
            buttonUltimate.Activated = false;
            buttonOptimized.Activated = false;

            switch (GPUMode)
            {
                case AsusACPI.GPUModeEco:
                    buttonOptimized.BorderColor = colorEco;
                    buttonEco.Activated = !GPUAuto;
                    buttonOptimized.Activated = GPUAuto;
                    labelGPU.Text = Properties.Strings.GPUMode + ": " + Properties.Strings.GPUModeEco;
                    panelGPU.AccessibleName = Properties.Strings.GPUMode + " - " + (GPUAuto ? Properties.Strings.Optimized : Properties.Strings.EcoMode);
                    break;
                case AsusACPI.GPUModeUltimate:
                    buttonUltimate.Activated = true;
                    labelGPU.Text = Properties.Strings.GPUMode + ": " + Properties.Strings.GPUModeUltimate;
                    panelGPU.AccessibleName = Properties.Strings.GPUMode + " - " + Properties.Strings.UltimateMode;
                    break;
                default:
                    buttonOptimized.BorderColor = colorStandard;
                    buttonStandard.Activated = !GPUAuto;
                    buttonOptimized.Activated = GPUAuto;
                    labelGPU.Text = Properties.Strings.GPUMode + ": " + (AppConfig.IsAlwaysUltimate() ? Properties.Strings.GPUModeUltimate : Properties.Strings.GPUModeStandard);
                    panelGPU.AccessibleName = Properties.Strings.GPUMode + " - " + (GPUAuto ? Properties.Strings.Optimized : Properties.Strings.StandardMode);
                    break;
            }

            VisualiseIcon();
            VisualizeXGM(GPUMode);

            if (isGpuSection)
            {
                menuEco.Checked = buttonEco.Activated;
                menuStandard.Checked = buttonStandard.Activated;
                menuUltimate.Checked = buttonUltimate.Activated;
                menuOptimized.Checked = buttonOptimized.Activated;
            }

            // UI Fix for small screeens
            if (Top < 0)
            {
                labelTipGPU.Visible = false;
                labelTipScreen.Visible = false;
                Top = 5;
            }

        }


        private (int, bool, bool)? lastIcon;
        private HpTrayIconKind? lastHpTrayIcon;
        private bool isDark = CheckSystemDarkModeStatus();

        public void VisualiseIcon(bool themeChange = false)
        {
            if (Program.trayIcon is null) return;
            if (AppConfig.IsHpVictusHardwareMode())
            {
                int basePerformanceMode = HpPerformanceModeStatus.CurrentBaseMode;
                HpTrayIconKind iconKind = HpTrayIconSelector.Select(basePerformanceMode);
                if (lastHpTrayIcon == iconKind) return;
                lastHpTrayIcon = iconKind;

                Icon? oldHpIcon = Program.trayIcon.Icon;
                Program.trayIcon.Icon = Program.GetHpTrayIcon(basePerformanceMode);
                oldHpIcon?.Dispose();
                return;
            }
            if (themeChange) isDark = CheckSystemDarkModeStatus();

            int GPUMode = AppConfig.Get("gpu_mode");
            bool bw = AppConfig.IsBWIcon();

            if (lastIcon == (GPUMode, isDark, bw)) return;
            lastIcon = (GPUMode, isDark, bw);

            Icon newIcon = GPUMode switch
            {
                AsusACPI.GPUModeEco => bw ? (isDark ? Properties.Resources.light_eco : Properties.Resources.dark_eco) : Properties.Resources.eco,
                AsusACPI.GPUModeUltimate => bw ? (isDark ? Properties.Resources.light_standard : Properties.Resources.dark_standard) : Properties.Resources.ultimate,
                _ => bw ? (isDark ? Properties.Resources.light_standard : Properties.Resources.dark_standard) : Properties.Resources.standard,
            };

            Icon? oldIcon = Program.trayIcon.Icon;
            Program.trayIcon.Icon = newIcon;
            oldIcon?.Dispose();
        }

        private void PictureGPU_Click(object? sender, EventArgs e)
        {
            if (GPUModeControl.gpuError is not null)
                Process.Start(new ProcessStartInfo("devmgmt.msc") { UseShellExecute = true });
        }

        private void ButtonSilent_Click(object? sender, EventArgs e)
        {
            Program.modeControl.SetPerformanceMode(AsusACPI.PerformanceSilent);
        }

        private void ButtonBalanced_Click(object? sender, EventArgs e)
        {
            Program.modeControl.SetPerformanceMode(AsusACPI.PerformanceBalanced);
        }

        private void ButtonTurbo_Click(object? sender, EventArgs e)
        {
            Program.modeControl.SetPerformanceMode(AsusACPI.PerformanceTurbo);
        }


        public void ButtonEnabled(RButton but, bool enabled)
        {
            but.Enabled = enabled;
            but.BackColor = but.Enabled ? Color.FromArgb(255, but.BackColor) : Color.FromArgb(100, but.BackColor);
        }

        public void VisualiseBatteryTitle(int limit)
        {
            labelBatteryTitle.Text = Properties.Strings.BatteryChargeLimit + ": " + limit.ToString() + "%";
        }

        public void VisualiseBattery(int limit)
        {
            if (InvokeRequired) { Invoke(() => VisualiseBattery(limit)); return; }
            VisualiseBatteryTitle(limit);
            sliderBattery.Value = limit;

            sliderBattery.AccessibleName = Properties.Strings.BatteryChargeLimit + ": " + limit.ToString() + "%";
            //sliderBattery.AccessibilityObject.Select(AccessibleSelection.TakeFocus);

            VisualiseBatteryFull();
        }

        public void VisualiseBatteryFull()
        {
            if (InvokeRequired) { Invoke(VisualiseBatteryFull); return; }
            if (BatteryControl.chargeFull)
            {
                buttonBatteryFull.BackColor = colorStandard;
                buttonBatteryFull.ForeColor = SystemColors.ControlLightLight;
                buttonBatteryFull.AccessibleName = Properties.Strings.BatteryChargeLimit + "100% on";
            }
            else
            {
                buttonBatteryFull.BackColor = buttonSecond;
                buttonBatteryFull.ForeColor = SystemColors.ControlDark;
                buttonBatteryFull.AccessibleName = Properties.Strings.BatteryChargeLimit + "100% off";
            }

        }


        public void UpdateKeyboardLabel()
        {
            labelKeyboard.Text = Properties.Strings.LaptopKeyboard + (PeripheralsProvider.IsAuraSync ? " +" : "");
        }

        public void VisualizePeripherals()
        {
            if (!PeripheralsProvider.IsAnyPeripheralConnect())
            {
                panelPeripherals.Visible = false;
                return;
            }

            Button[] buttons = new Button[] { buttonPeripheral1, buttonPeripheral2, buttonPeripheral3 };

            //we only support 4 devces for now. Who has more than 4 mice connected to the same PC anyways....
            List<IPeripheral> lp = PeripheralsProvider.AllPeripherals();

            for (int i = 0; i < lp.Count && i < buttons.Length; ++i)
            {
                IPeripheral m = lp.ElementAt(i);
                Button b = buttons[i];

                string id = m.GetDisplayName();
                bool ready = m.IsDeviceReady;
                bool hasBat = m.HasBattery();
                bool charging = ready && hasBat && m.Charging;
                int level = (ready && hasBat) ? Math.Min(5, (m.Battery + 10) / 20) : -1;
                bool showPercent = AppConfig.Is("mouse_battery") && ready && hasBat;
                int cacheBattery = showPercent ? m.Battery : -1;
                var state = (id, ready, charging, level, cacheBattery, b.ForeColor.ToArgb());

                if (b.Tag is ValueTuple<string, bool, bool, int, int, int> prev && prev.Equals(state) && b.Visible)
                    continue;

                b.Text = showPercent ? id + "\n" + m.Battery + "%" : id;

                Image? baseIcon = m.DeviceType() switch
                {
                    PeripheralType.Mouse => Properties.Resources.icons8_maus_48,
                    PeripheralType.Keyboard => Properties.Resources.icons8_keyboard_48,
                    _ => null,
                };

                if (baseIcon is not null)
                {
                    int ih = baseIcon.Height;
                    // icon PNG may be wider than tall (baked-in right text padding); badge/bars anchor to the glyph square
                    int iw = Math.Min(baseIcon.Width, ih);
                    Image composed = ControlHelper.TintImage(baseIcon, b.ForeColor);
                    if (!ready)
                    {
                        composed = ControlHelper.OverlayBadge(composed, Properties.Resources.icons8_cancel_48, RForm.colorTurbo, iconWidth: iw, iconHeight: ih);
                    }
                    else if (hasBat)
                    {
                        if (charging)
                            composed = ControlHelper.OverlayBadge(composed, Properties.Resources.icons8_flash_48, RForm.colorEco, iconWidth: iw, iconHeight: ih);

                        Color barColor = level <= 1 ? colorTurbo
                                       : level <= 3 ? colorStandard
                                       : colorEco;
                        composed = ControlHelper.OverlayChargeBars(composed, level, 5, barColor, iconWidth: iw, iconHeight: ih);
                    }

                    b.Image = ControlHelper.ResizeImage(composed, ControlHelper.Scale);
                }

                b.Tag = state;
                b.Visible = true;
            }

            for (int i = lp.Count; i < buttons.Length; ++i)
            {
                buttons[i].Visible = false;
            }

            panelPeripherals.Visible = true;
        }

        private void ButtonPeripheral_MouseEnter(object? sender, EventArgs e)
        {
            int index = 0;
            if (sender == buttonPeripheral2) index = 1;
            if (sender == buttonPeripheral3) index = 2;
            IPeripheral iph = PeripheralsProvider.AllPeripherals().ElementAt(index);


            if (iph is null)
            {
                return;
            }

            if (!iph.IsDeviceReady)
            {
                //Refresh battery on hover if the device is marked as "Not Ready"
                iph.ReadBattery();
            }
        }

        private void ButtonPeripheral_Click(object? sender, EventArgs e)
        {
            if (mouseSettings is not null)
            {
                mouseSettings.Close();
                return;
            }

            if (keyboardSettings is not null)
            {
                keyboardSettings.Close();
                return;
            }

            int index = 0;
            if (sender == buttonPeripheral2) index = 1;
            if (sender == buttonPeripheral3) index = 2;

            IPeripheral iph = PeripheralsProvider.AllPeripherals().ElementAt(index);

            if (iph is null)
            {
                //Can only happen when the user hits the button in the exact moment a device is disconnected.
                return;
            }

            if (iph.DeviceType() == PeripheralType.Mouse)
            {
                AsusMouse? am = iph as AsusMouse;
                if (am is null || !am.IsDeviceReady)
                {
                    //Should not happen if all device classes are implemented correctly. But better safe than sorry.
                    return;
                }
                mouseSettings = new AsusMouseSettings(am);
                mouseSettings.TopMost = AppConfig.Is("topmost");
                mouseSettings.FormClosed += MouseSettings_FormClosed;
                mouseSettings.Disposed += MouseSettings_Disposed;
                if (!mouseSettings.IsDisposed)
                {
                    mouseSettings.Show();
                }
                else
                {
                    mouseSettings = null;
                }

            }

            if (iph.DeviceType() == PeripheralType.Keyboard)
            {
                AsusKeyboard? kb = iph as AsusKeyboard;
                if (kb is null || !kb.IsDeviceReady)
                {
                    return;
                }
                ShowKeyboardSettings(kb);
            }
        }

        private void ShowKeyboardSettings(AsusKeyboard kb)
        {
            AsusKeyboardSettings.RequestReopen = ShowKeyboardSettings;
            keyboardSettings = new AsusKeyboardSettings(kb);
            keyboardSettings.TopMost = AppConfig.Is("topmost");
            keyboardSettings.FormClosed += KeyboardSettings_FormClosed;
            keyboardSettings.Disposed += KeyboardSettings_Disposed;
            if (!keyboardSettings.IsDisposed)
            {
                keyboardSettings.Show();
            }
            else
            {
                keyboardSettings = null;
            }
        }

        private void KeyboardSettings_Disposed(object? sender, EventArgs e)
        {
            keyboardSettings = null;
        }

        private void KeyboardSettings_FormClosed(object? sender, FormClosedEventArgs e)
        {
            keyboardSettings = null;
        }

        private void MouseSettings_Disposed(object? sender, EventArgs e)
        {
            mouseSettings = null;
        }

        private void MouseSettings_FormClosed(object? sender, FormClosedEventArgs e)
        {
            mouseSettings = null;
        }

        public void VisualiseAudio(double level)
        {
            if (InvokeRequired) { Invoke(() => VisualiseAudio(level)); return; }
            int filledSquares = (int)Math.Round(level/2);
            string squares = new string('|', filledSquares);
            labelMatrix.Text = $"Slash Lighting: {squares}";
        }

        public void VisualiseFnLock()
        {

            if (AppConfig.Is("fn_lock"))
            {
                buttonFnLock.BackColor = colorStandard;
                buttonFnLock.ForeColor = SystemColors.ControlLightLight;
                buttonFnLock.AccessibleName = "Fn-Lock on";
            }
            else
            {
                buttonFnLock.BackColor = buttonSecond;
                buttonFnLock.ForeColor = SystemColors.ControlDark;
                buttonFnLock.AccessibleName = "Fn-Lock off";
            }
        }


        private void ButtonFnLock_Click(object? sender, EventArgs e)
        {
            InputDispatcher.ToggleFnLock();
        }

    }


}
