using GHelper.Ally;
using GHelper.Battery;
using GHelper.Display;
using GHelper.Gpu;
using GHelper.Helpers;
using GHelper.Hardware.Hp;
using GHelper.Input;
using GHelper.Mode;
using GHelper.Overlay;
using GHelper.Peripherals;
using GHelper.UI;
using GHelper.USB;
using Microsoft.Win32;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using static NativeMethods;

namespace GHelper
{

    static class Program
    {
        public static NotifyIcon trayIcon;
        private const string UnsupportedHardwareFlag = "--unsupported-hardware";
        private const string HpVictusHardwareFlag = "--hp-victus";
        private const string HpWmiReadOnlyTestFlag = "--hp-wmi-readonly-test";
        public static IHardwareController acpi;
        private static bool unsupportedHardwareMode;
        private static bool hpVictusMode;
        private static bool hpWmiReadOnlyTestMode;

        public static SettingsForm settingsForm;
        public static HpVictusCapabilitySnapshot? hpVictusCapabilitySnapshot;

        public static ModeControl modeControl;
        public static GPUModeControl gpuControl;
        public static AllyControl allyControl;
        public static ClamshellModeControl clamshellControl;

        public static ToastForm toast;

        public static HardwareOverlay? hardwareOverlay;

        public static IntPtr unRegPowerNotify, unRegPowerNotifyLid, unRegPowerNotifyEnergy, unRegSuspendResume;
        public static int WM_TASKBARCREATED = 0;

        private static long lastAuto;
        private static readonly object autoLock = new();
        private static long lastTheme;
        private static System.Windows.Forms.Timer? trayRetryTimer;
        private static int exitCleanupStarted;
        private static int hpDiagnosticExitStarted;
        private static bool systemLifecycleHooksRegistered;

        public static InputDispatcher? inputDispatcher;

        // The main entry point for the application
        public static void Main(string[] args)
        {
            if (TryRunHpFanLevelResearchDryRun(args))
            {
                return;
            }

            if (TryRunHpFanMaxHold(args))
            {
                return;
            }

            if (TryRunHpFanMaxPulse(args))
            {
                return;
            }

            if (TryRunHpFanMaxExperiment(args))
            {
                return;
            }

            if (TryRunHpFanMaxExperimentBaselineCapture(args))
            {
                return;
            }

            if (TryRunHpFanMaxExperimentDryRun(args))
            {
                return;
            }

            try
            {
                MainCore(args);
            }
            catch (Exception ex)
            {
                WriteStartupCrashLog(args, ex);
                throw;
            }
        }

        private static bool TryRunHpFanLevelResearchDryRun(string[] args)
        {
            HpFanLevelResearchDryRunCommandResult result = HpFanLevelResearchDryRunCommand.Parse(args);
            if (!result.ShouldExit)
            {
                return false;
            }

            Environment.ExitCode = result.IsValidRequest ? 0 : 2;
            string path;
            try
            {
                path = HpFanLevelResearchDryRunLogWriter.Write(result.Record!);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Environment.ExitCode = 1;
                try { Console.Error.WriteLine("SetFanLevel dry-run result could not be saved: " + ex.Message); }
                catch (IOException) { }
                return true;
            }

            // Persistence must succeed independently of WinExe console availability.
            try
            {
                Console.WriteLine("SetFanLevel dry-run result saved: " + path);
                Console.WriteLine(result.Record!.ToJson());
            }
            catch (IOException) { }
            return true;
        }

        private static bool TryRunHpFanMaxHold(string[] args)
        {
            HpFanMaxHoldCommandResult command = HpFanMaxHoldCommand.Parse(args);
            if (!command.ShouldExit)
            {
                return false;
            }

            Console.WriteLine(command.FormatCliSummary());
            HpFanMaxExperimentRunResult result;
            if (!command.IsValidRequest)
            {
                foreach (string reason in command.ValidationReasons)
                {
                    Console.Error.WriteLine(reason);
                }

                result = new HpFanMaxExperimentRunResult(
                    command.Payload,
                    null,
                    false,
                    HpFanMaxExperimentWriteResult.NotAttempted("Blocked by required Max Fan Hold command-line flags or pre-restore wait."),
                    null,
                    HpFanMaxExperimentWriteResult.NotAttempted("Blocked by required Max Fan Hold command-line flags or pre-restore wait."),
                    null,
                    HpFanMaxExperimentOutcome.Unknown,
                    command.ValidationReasons);
            }
            else
            {
                AppConfig.SetHpVictusHardwareMode(true);
                AppConfig.SetHpWmiReadOnlyTestMode(true);
                AppConfig.SetUnsupportedHardwareMode(true);

                var runner = new HpFanMaxExperimentRunner(
                    new HpFanMaxExperimentReadOnlyProvider(),
                    new HpFanMaxExperimentWmiTransport(),
                    new HpFanMaxExperimentDelay());
                var gates = new HpFanMaxExperimentRuntimeGates(
                    ProcessHelper.IsUserAdministrator(),
                    new HpFanMaxExperimentAcPowerProvider().IsAcPowerOnline(),
                    IsFirstWriteGateApproved: true,
                    HasReviewedHumanApproval: true,
                    HasSecondFourByteConfirmationApproval: true,
                    HasOneTimeOneByteComparisonApproval: false);
                result = runner.Run(command, gates);
            }

            try
            {
                string path = HpFanMaxExperimentLogWriter.Write(command.CreateLogRecord(result));
                Console.WriteLine("SetFanMax developer-only Max Fan Hold record written: " + path);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("SetFanMax developer-only Max Fan Hold record could not be written: " + ex.Message);
            }

            return true;
        }

        private static bool TryRunHpFanMaxPulse(string[] args)
        {
            HpFanMaxPulseCommandResult command = HpFanMaxPulseCommand.Parse(args);
            if (!command.ShouldExit)
            {
                return false;
            }

            HpFanMaxExperimentRunResult result;
            if (!command.IsValidRequest)
            {
                result = new HpFanMaxExperimentRunResult(
                    null,
                    null,
                    false,
                    HpFanMaxExperimentWriteResult.NotAttempted("Blocked by required Max Fan Pulse command-line flags."),
                    null,
                    HpFanMaxExperimentWriteResult.NotAttempted("Blocked by required Max Fan Pulse command-line flags."),
                    null,
                    HpFanMaxExperimentOutcome.Unknown,
                    command.ValidationReasons);
            }
            else
            {
                AppConfig.SetHpVictusHardwareMode(true);
                AppConfig.SetHpWmiReadOnlyTestMode(true);
                AppConfig.SetUnsupportedHardwareMode(true);

                var runner = new HpFanMaxExperimentRunner(
                    new HpFanMaxExperimentReadOnlyProvider(),
                    new HpFanMaxExperimentWmiTransport(),
                    new HpFanMaxExperimentDelay());
                var gates = new HpFanMaxExperimentRuntimeGates(
                    ProcessHelper.IsUserAdministrator(),
                    new HpFanMaxExperimentAcPowerProvider().IsAcPowerOnline(),
                    IsFirstWriteGateApproved: true,
                    HasReviewedHumanApproval: true,
                    HasSecondFourByteConfirmationApproval: true,
                    HasOneTimeOneByteComparisonApproval: false);
                result = runner.Run(command, gates);
            }

            try
            {
                string path = HpFanMaxExperimentLogWriter.Write(HpFanMaxExperimentRunLogMapper.Create(result, command.ManualObservation));
                Console.WriteLine("SetFanMax Max Fan Pulse record written: " + path);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("SetFanMax Max Fan Pulse record could not be written: " + ex.Message);
            }

            return true;
        }

        private static bool TryRunHpFanMaxExperiment(string[] args)
        {
            HpFanMaxExperimentRunnerCommandResult command = HpFanMaxExperimentRunnerCommand.Parse(args);
            if (!command.ShouldExit)
            {
                return false;
            }

            HpFanMaxExperimentRunResult result;
            if (!command.IsValidRequest)
            {
                result = new HpFanMaxExperimentRunResult(
                    command.Payload,
                    null,
                    false,
                    HpFanMaxExperimentWriteResult.NotAttempted("Blocked by required command-line flags."),
                    null,
                    HpFanMaxExperimentWriteResult.NotAttempted("Blocked by required command-line flags."),
                    null,
                    HpFanMaxExperimentOutcome.Unknown,
                    command.ValidationReasons);
            }
            else
            {
                AppConfig.SetHpVictusHardwareMode(true);
                AppConfig.SetHpWmiReadOnlyTestMode(true);
                AppConfig.SetUnsupportedHardwareMode(true);

                var runner = new HpFanMaxExperimentRunner(
                    new HpFanMaxExperimentReadOnlyProvider(),
                    new HpFanMaxExperimentWmiTransport(),
                    new HpFanMaxExperimentDelay());
                var gates = new HpFanMaxExperimentRuntimeGates(
                    ProcessHelper.IsUserAdministrator(),
                    new HpFanMaxExperimentAcPowerProvider().IsAcPowerOnline(),
                    IsFirstWriteGateApproved: command.HasOneTimeFourByteApproval || command.HasOneTimeOneByteComparisonApproval,
                    HasReviewedHumanApproval: command.HasOneTimeFourByteApproval || command.HasOneTimeOneByteComparisonApproval,
                    HasSecondFourByteConfirmationApproval: command.HasSecondFourByteConfirmationApproval,
                    HasOneTimeOneByteComparisonApproval: command.HasOneTimeOneByteComparisonApproval);
                result = runner.Run(command, gates);
            }

            try
            {
                string path = HpFanMaxExperimentLogWriter.Write(HpFanMaxExperimentRunLogMapper.Create(result, command.ManualObservation));
                Console.WriteLine("SetFanMax first-write experiment record written: " + path);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("SetFanMax first-write experiment record could not be written: " + ex.Message);
            }

            return true;
        }

        private static bool TryRunHpFanMaxExperimentBaselineCapture(string[] args)
        {
            HpFanMaxExperimentBaselineCaptureCommandResult result = HpFanMaxExperimentBaselineCaptureCommand.Parse(args);
            if (!result.ShouldExit)
            {
                return false;
            }

            HpFanMaxExperimentLogRecord record;
            if (!result.IsValidRequest)
            {
                record = HpFanMaxExperimentBaselineCaptureMapper.CreateBlocked(result);
            }
            else if (!ProcessHelper.IsUserAdministrator())
            {
                record = HpFanMaxExperimentBaselineCaptureMapper.CreateBlocked(
                    result,
                    "Baseline capture requires an elevated Administrator process before approved read-only probes can run.");
            }
            else
            {
                AppConfig.SetHpVictusHardwareMode(true);
                AppConfig.SetHpWmiReadOnlyTestMode(true);
                AppConfig.SetUnsupportedHardwareMode(true);

                try
                {
                    HpVictusCapabilitySnapshot snapshot = HpVictusCapabilityProbe.Probe();
                    record = HpFanMaxExperimentBaselineCaptureMapper.CreateCaptured(
                        result,
                        CreateHpFanMaxExperimentBaselineData(snapshot));
                }
                catch (Exception ex)
                {
                    record = HpFanMaxExperimentBaselineCaptureMapper.CreateBlocked(
                        result,
                        "Baseline capture failed safely before a complete read-only record was collected: " + ex.GetType().Name + ".");
                }
            }

            try
            {
                string path = HpFanMaxExperimentLogWriter.Write(record);
                Console.WriteLine("SetFanMax read-only baseline record written: " + path);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("SetFanMax read-only baseline record could not be written: " + ex.Message);
            }

            return true;
        }

        private static HpFanMaxExperimentBaselineCaptureData CreateHpFanMaxExperimentBaselineData(
            HpVictusCapabilitySnapshot snapshot)
        {
            int? thermalPolicyVersion = snapshot.SystemDesignDataInvocationSucceeded && snapshot.SystemDesignDataDecodeSucceeded
                ? snapshot.SystemDesignDataDecoded?.ThermalPolicyVersion
                : null;
            int? fanCount = snapshot.FanGetCountInvocationSucceeded && snapshot.FanGetCountDecodeSucceeded
                ? snapshot.FanGetCountDecoded?.FanCount
                : null;
            bool? maxFanEnabled = snapshot.FanMaxGetInvocationSucceeded && snapshot.FanMaxGetDecodeSucceeded
                ? snapshot.FanMaxGetDecoded?.IsMaxFanEnabled
                : null;
            string? fanGetLevelRaw = snapshot.FanGetLevelInvocationSucceeded && snapshot.FanGetLevelDecodeSucceeded
                ? snapshot.FanGetLevelDecoded?.KnownPrefixHex
                : null;

            return new HpFanMaxExperimentBaselineCaptureData(
                snapshot.Model,
                snapshot.SystemSku,
                snapshot.BiosVersion,
                thermalPolicyVersion,
                fanCount,
                maxFanEnabled,
                fanGetLevelRaw,
                [
                    BuildReadOnlyProbeSummary(
                        "SystemDesignData",
                        snapshot.SystemDesignDataInvocationAttempted,
                        snapshot.SystemDesignDataInvocationSucceeded,
                        snapshot.SystemDesignDataDecodeSucceeded,
                        snapshot.SystemDesignDataReturnedByteCount),
                    BuildReadOnlyProbeSummary(
                        "FanGetCount",
                        snapshot.FanGetCountInvocationAttempted,
                        snapshot.FanGetCountInvocationSucceeded,
                        snapshot.FanGetCountDecodeSucceeded,
                        snapshot.FanGetCountReturnedByteCount),
                    BuildReadOnlyProbeSummary(
                        "FanMaxGet",
                        snapshot.FanMaxGetInvocationAttempted,
                        snapshot.FanMaxGetInvocationSucceeded,
                        snapshot.FanMaxGetDecodeSucceeded,
                        snapshot.FanMaxGetReturnedByteCount),
                    BuildReadOnlyProbeSummary(
                        "FanGetLevel",
                        snapshot.FanGetLevelInvocationAttempted,
                        snapshot.FanGetLevelInvocationSucceeded,
                        snapshot.FanGetLevelDecodeSucceeded,
                        snapshot.FanGetLevelReturnedByteCount)
                ]);
        }

        private static string BuildReadOnlyProbeSummary(
            string commandName,
            bool attempted,
            bool succeeded,
            bool decodeSucceeded,
            int returnedByteCount) =>
            $"{commandName}: attempted={attempted}; succeeded={succeeded}; decodeSucceeded={decodeSucceeded}; returnedByteCount={returnedByteCount}";

        private static bool TryRunHpFanMaxExperimentDryRun(string[] args)
        {
            HpFanMaxExperimentDryRunCommandResult result = HpFanMaxExperimentDryRunCommand.Parse(args);
            if (!result.ShouldExit)
            {
                return false;
            }

            try
            {
                string path = HpFanMaxExperimentLogWriter.Write(result.LogRecord!);
                Console.WriteLine("SetFanMax dry-run record written: " + path);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("SetFanMax dry-run record could not be written: " + ex.Message);
            }

            return true;
        }

        private static void MainCore(string[] args)
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Logger.WriteLine("Unhandled: " + e.ExceptionObject);
                WriteStartupCrashLog(args, e.ExceptionObject as Exception ?? new Exception(e.ExceptionObject?.ToString() ?? "Unknown unhandled exception"));
            };
            TaskScheduler.UnobservedTaskException += (s, e) => { Logger.WriteLine("Unobserved: " + e.Exception); e.SetObserved(); };

            bool explicitUnsupportedHardwareMode = args.Any(arg => string.Equals(arg, UnsupportedHardwareFlag, StringComparison.OrdinalIgnoreCase));
            hpVictusMode = args.Any(arg => string.Equals(arg, HpVictusHardwareFlag, StringComparison.OrdinalIgnoreCase));
            hpWmiReadOnlyTestMode = args.Any(arg => string.Equals(arg, HpWmiReadOnlyTestFlag, StringComparison.OrdinalIgnoreCase));

            // Normalize against the live command line so launch profiles and UI gating use the same HP-mode decision.
            AppConfig.SetHpVictusHardwareMode(hpVictusMode);
            AppConfig.SetHpWmiReadOnlyTestMode(hpWmiReadOnlyTestMode);
            hpVictusMode = AppConfig.IsHpVictusHardwareMode();
            hpWmiReadOnlyTestMode = AppConfig.IsHpWmiReadOnlyTestMode();
            unsupportedHardwareMode = explicitUnsupportedHardwareMode || hpVictusMode;
            AppConfig.SetHpVictusHardwareMode(hpVictusMode);
            AppConfig.SetHpWmiReadOnlyTestMode(hpWmiReadOnlyTestMode);
            AppConfig.SetUnsupportedHardwareMode(unsupportedHardwareMode);
            string action = args.FirstOrDefault(arg =>
                !string.Equals(arg, UnsupportedHardwareFlag, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(arg, HpVictusHardwareFlag, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(arg, HpWmiReadOnlyTestFlag, StringComparison.OrdinalIgnoreCase)) ?? "";

            if (action == "charge")
            {
                Charge();
                return;
            }

            string language = AppConfig.GetString("language");
            try
            {
                if (language != null && language.Length > 0)
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(language);
                else
                {
                    var culture = CultureInfo.CurrentUICulture;
                    if (culture.ToString() == "kr") culture = CultureInfo.GetCultureInfo("ko");
                    Thread.CurrentThread.CurrentUICulture = culture;
                }
            } catch
            {
                Logger.WriteLine("Unknown Language: " + language);
            }

            Logger.WriteLine("----------------------");
            Logger.WriteLine("App launched: " + AppConfig.GetModel() + " :" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + CultureInfo.CurrentUICulture + (ProcessHelper.IsUserAdministrator() ? "." : ""));

            if (hpVictusMode)
            {
                try
                {
                    var hpSnapshot = HpVictusCapabilityProbe.Probe();
                    string hpReportPath = HpVictusCapabilityProbe.WriteReport(hpSnapshot);
                    hpVictusCapabilitySnapshot = hpSnapshot;
                    Logger.WriteLine("HP Victus capability probe: " + hpSnapshot.ToLogString());
                    Logger.WriteLine("HP Victus capability report: " + hpReportPath);
                }
                catch (Exception ex)
                {
                    Logger.WriteLine("HP Victus capability startup failed safely: " + ex);
                    WriteStartupCrashLog(args, ex);
                }
            }

            settingsForm = new SettingsForm();
            modeControl = new ModeControl();
            gpuControl = new GPUModeControl(settingsForm);
            allyControl = new AllyControl(settingsForm);
            clamshellControl = new ClamshellModeControl();
            toast = new ToastForm();

            hardwareOverlay = new HardwareOverlay();

            ProcessHelper.CheckAlreadyRunning(settingsForm);
            ProcessHelper.SetPriority();

            CleanupLegacyFiles();

            var startCount = AppConfig.Get("start_count") + 1;
            AppConfig.Set("start_count", startCount);
            Logger.WriteLine("Start Count: " + startCount);

            acpi = global::HardwareControllerFactory.CreateController(unsupportedHardwareMode, hpVictusMode);

            if (!unsupportedHardwareMode && !acpi.IsConnected() && AppConfig.IsASUS() && !AppConfig.IsDesktop())
            {
                DialogResult dialogResult = MessageBox.Show(Properties.Strings.ACPIError, Properties.Strings.StartupError, MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo("https://www.asus.com/support/FAQ/1047338/") { UseShellExecute = true });
                }

                Application.Exit();
                return;
            }

            if (!unsupportedHardwareMode)
            {
                ProcessHelper.KillSmartDisplayControl();
                AsusService.StopOnStartup();
            }

            Application.EnableVisualStyles();

            if (!unsupportedHardwareMode) HardwareControl.RecreateGpuControl();

            trayIcon = new NotifyIcon
            {
                Text = "VictusX",
                Icon = GetTrayIcon(),
                Visible = true
            };

            trayRetryTimer = new System.Windows.Forms.Timer { Interval = 5000 };
            trayRetryTimer.Tick += TrayRetryTimer_Tick;
            trayRetryTimer.Start();

            WM_TASKBARCREATED = RegisterWindowMessage("TaskbarCreated");
            Logger.WriteLine($"Tray Icon: {trayIcon.Visible} | {WM_TASKBARCREATED}");

            Modes.InitFullSpeed();
            settingsForm.SetContextMenu();
            trayIcon.MouseClick += TrayIcon_MouseClick;
            trayIcon.MouseMove += TrayIcon_MouseMove;


            if (!hpVictusMode)
            {
                inputDispatcher = new InputDispatcher();
            }

            if (!unsupportedHardwareMode)
            {
                settingsForm.InitAura();
                settingsForm.InitMatrix();

                ScreenControl.InitScreen();

                SetAutoModes(init: true);
            }

            if (!hpVictusMode)
            {
                powerSettleTimer.Elapsed += OnPowerSettled;

                // These hooks drive inherited ASUS controls and are intentionally absent from HP diagnostic-only mode.
                SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
                SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
                SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
                SystemEvents.SessionEnding += SystemEvents_SessionEnding;

                clamshellControl.RegisterDisplayEvents();
                clamshellControl.ToggleLidAction();

                unRegPowerNotify = NativeMethods.RegisterPowerSettingNotification(settingsForm.Handle, PowerSettingGuid.ConsoleDisplayState, NativeMethods.DEVICE_NOTIFY_WINDOW_HANDLE);
                unRegPowerNotifyLid = NativeMethods.RegisterPowerSettingNotification(settingsForm.Handle, PowerSettingGuid.LIDSWITCH_STATE_CHANGE, NativeMethods.DEVICE_NOTIFY_WINDOW_HANDLE);
                unRegPowerNotifyEnergy = NativeMethods.RegisterPowerSettingNotification(settingsForm.Handle, PowerSettingGuid.EnergySaverStatus, NativeMethods.DEVICE_NOTIFY_WINDOW_HANDLE);
                unRegSuspendResume = NativeMethods.RegisterSuspendResumeNotification(settingsForm.Handle, NativeMethods.DEVICE_NOTIFY_WINDOW_HANDLE);
                systemLifecycleHooksRegistered = true;
            }


            if (!unsupportedHardwareMode)
            {
                Task task = Task.Run(() =>
                {
                    PeripheralsProvider.DetectAllAsusMice();
                    PeripheralsProvider.DetectAllAsusKeyboards();
                });
                PeripheralsProvider.RegisterForDeviceEvents();
            }

            if (hpVictusMode || Environment.CurrentDirectory.Trim('\\') == Application.StartupPath.Trim('\\') || action.Length > 0)
            {
                SettingsToggle(false);
            }

            if (!unsupportedHardwareMode)
            {
                switch (action)
                {
                    case "cpu":
                        Startup.ReScheduleAdmin();
                        settingsForm.FansToggle();
                        break;
                    case "gpu":
                        Startup.ReScheduleAdmin();
                        settingsForm.FansToggle(1);
                        break;
                    case "services":
                        settingsForm.extraForm = new Extra();
                        settingsForm.extraForm.Show();
                        settingsForm.extraForm.ServiesToggle();
                        break;
                    case "uv":
                        Startup.ReScheduleAdmin();
                        settingsForm.FansToggle(2);
                        modeControl.SetRyzen();
                        break;
                    case "colors":
                        Task.Run(async () =>
                        {
                            await ColorProfileHelper.InstallProfile();
                            settingsForm.Invoke(delegate
                            {
                                settingsForm.InitVisual();
                            });
                        });
                        break;
                    default:
                        Task.Run(Startup.StartupCheck);
                        break;
                }
            }
            else if (action.Length > 0)
            {
                Logger.WriteLine("Unsupported hardware mode: skipped startup action " + action);
            }

            if (!unsupportedHardwareMode)
            {
                Task.Run(() =>
                {
                    settingsForm.VisualiseArmoury(AsusService.IsArmouryRunning());
                });
            }

            if (!unsupportedHardwareMode && AppConfig.IsOverlay())
                hardwareOverlay?.StartOverlay();

            Application.ApplicationExit += OnExit;
            Application.Run();
        }


        private static void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            if (unsupportedHardwareMode) return;

            gpuControl.StandardModeFix();
            modeControl.ShutdownReset();
            BatteryControl.AutoBattery();
            InputDispatcher.ShutdownStatusLed();
            XGM.NotifyShutdown();
        }

        private static void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (unsupportedHardwareMode) return;

            if (e.Reason == SessionSwitchReason.SessionLogon || e.Reason == SessionSwitchReason.SessionUnlock || e.Reason == SessionSwitchReason.ConsoleConnect)
            {
                Logger.WriteLine("Session:" + e.Reason.ToString());
                ProcessHelper.KillSmartDisplayControl();
                bool wasLocked = Aura.sessionLock;
                Aura.sessionLock = false;
                Aura.ApplyAura();
                Task.Delay(2000).ContinueWith(_ =>
                {
                    ScreenControl.AutoScreen();
                    if (!wasLocked) return;
                    if (Math.Abs(DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastAuto) < 10000) return;
                    modeControl.AutoCPUTemp();
                });
            }
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                Logger.WriteLine("Session:" + e.Reason.ToString());
                Aura.sessionLock = true;
            }
        }

        static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {

            if (Math.Abs(DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastTheme) < 2000) return;

            switch (e.Category)
            {
                case UserPreferenceCategory.General:
                    bool changed = settingsForm.InitTheme();
                    settingsForm.InitContextMenuTheme();
                    settingsForm.VisualiseIcon(true);
                    settingsForm.VisualiseFnLock();
                    settingsForm.VisualiseBatteryFull();

                    if (changed)
                    {
                        Debug.WriteLine("Theme Changed");
                        lastTheme = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    }

                    if (settingsForm.fansForm is not null && settingsForm.fansForm.Text != "")
                        settingsForm.fansForm.InitTheme();

                    if (settingsForm.extraForm is not null && settingsForm.extraForm.Text != "")
                        settingsForm.extraForm.InitTheme();

                    if (settingsForm.updatesForm is not null && settingsForm.updatesForm.Text != "")
                        settingsForm.updatesForm.InitTheme();

                    if (settingsForm.matrixForm is not null && settingsForm.matrixForm.Text != "")
                        settingsForm.matrixForm.InitTheme();

                    if (settingsForm.handheldForm is not null && settingsForm.handheldForm.Text != "")
                        settingsForm.handheldForm.InitTheme();

                    break;
            }
        }



        public static bool SetAutoModes(bool powerChanged = false, bool init = false, bool wakeup = false)
        {
            if (unsupportedHardwareMode) return false;

            int skipDelay = wakeup ? 10000 : 3000;

            if (init) gpuControl.CaptureNvBootState();

            lock (autoLock)
            {
                if (Math.Abs(DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastAuto) < skipDelay) return false;
                lastAuto = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            }

            currentSource = ReadPowerSource();
            Logger.WriteLine("AutoSetting for " + SystemInformation.PowerStatus.PowerLineStatus.ToString());

            BatteryControl.AutoBattery(init);
            if (init) InputDispatcher.InitScreenpad();
            DynamicLightingHelper.Init();
            ScreenControl.InitOptimalBrightness();

            inputDispatcher.Init();
            //HardwareControl.ReadSensors(true);

            modeControl.AutoPerformance(powerChanged);

            if (powerChanged) settingsForm.matrixControl.SetMatrix(true);
            else settingsForm.matrixControl.SetDevice(true);
            InputDispatcher.InitStatusLed();
            if (init) NumberPad.Init();
            XGM.Init();

            if (AppConfig.IsAlly())
            {
                allyControl.Init();
            }
            else
            {
                InputDispatcher.AutoKeyboard();
            }

            bool switched = gpuControl.AutoGPUMode(delay: 1000);
            if (!switched)
            {
                gpuControl.InitGPUMode();
                if (init) gpuControl.CheckStandardHalfState();
                ScreenControl.AutoScreen();
            }

            ScreenControl.InitMiniled();
            VisualControl.InitBrightness();

            return true;
        }

        public enum PowerSource { Battery, Barrel, USBC }

        public static PowerSource currentSource = PowerSource.Battery;
        private static PowerLineStatus lastLineStatus = SystemInformation.PowerStatus.PowerLineStatus;
        private static readonly System.Timers.Timer powerSettleTimer = new() { AutoReset = false };

        public static PowerSource ReadPowerSource()
        {
            if (SystemInformation.PowerStatus.PowerLineStatus != PowerLineStatus.Online)
                return PowerSource.Battery;

            int chargerMode = acpi?.DeviceGet(AsusACPI.ChargerMode) ?? 0;
            if (chargerMode > 0 && (chargerMode & AsusACPI.ChargerBarrel) == 0)
                return PowerSource.USBC;

            return PowerSource.Barrel;
        }

        public static bool usbcProfile = AppConfig.Is("usbc_profile");

        public static int PerformanceKey() =>
            usbcProfile ? (int)ReadPowerSource() : (int)SystemInformation.PowerStatus.PowerLineStatus;

        public static void SchedulePowerCheck()
        {
            if (AppConfig.Is("disable_power_event")) return;
            powerSettleTimer.Interval = Math.Max(AppConfig.Get("charger_delay"), 2000);
            powerSettleTimer.Stop();
            powerSettleTimer.Start();
        }

        private static void OnPowerSettled(object? sender, System.Timers.ElapsedEventArgs e)
        {
            PowerSource source = ReadPowerSource();
            if (source == currentSource) return;

            Logger.WriteLine($"Power source: {currentSource} -> {source}");
            currentSource = source;
            SetAutoModes(powerChanged: true);
        }

        public static void OnChargerEvent() => SchedulePowerCheck();

        private static void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (unsupportedHardwareMode) return;

            if (e.Mode == PowerModes.Suspend)
            {
                Logger.WriteLine("Power Mode Changed:" + e.Mode.ToString());
                gpuControl.StandardModeFix();
                modeControl.ShutdownReset();
                InputDispatcher.ShutdownStatusLed();
                XGM.NotifyShutdown();
                return;
            }

            PowerLineStatus status = SystemInformation.PowerStatus.PowerLineStatus;
            if (status != lastLineStatus)
            {
                lastLineStatus = status;
                Logger.WriteLine($"Power Mode {e.Mode}: {status}");
            }

            SchedulePowerCheck();
        }

        public static void SettingsToggle(bool checkForFocus = true, bool trayClick = false)
        {
            if (settingsForm.Visible)
            {
                // If helper window is not on top, this just focuses on the app again
                // Pressing the ghelper button again will hide the app
                if (checkForFocus && !settingsForm.HasAnyFocus(trayClick) && !AppConfig.Is("topmost"))
                {
                    settingsForm.ShowAll();
                }
                else
                {
                    settingsForm.HideAll();
                }
            }
            else
            {
                var screen = Screen.PrimaryScreen;
                if (screen is null) screen = Screen.FromControl(settingsForm);

                settingsForm.WindowState = FormWindowState.Normal;

                settingsForm.Location = screen.WorkingArea.Location;
                settingsForm.Left = screen.WorkingArea.Width - 10 - settingsForm.Width;
                settingsForm.Top = screen.WorkingArea.Height - 10 - settingsForm.Height;

                settingsForm.Show();
                settingsForm.ShowAll();

                settingsForm.Left = screen.WorkingArea.Width - 10 - settingsForm.Width;

                if (AppConfig.IsAlly())
                    settingsForm.Top = Math.Max(10, screen.Bounds.Height - 110 - settingsForm.Height);
                else
                    settingsForm.Top = screen.WorkingArea.Height - 10 - settingsForm.Height;

                settingsForm.VisualiseGPUMode();
            }
        }

        static void TrayIcon_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                SettingsToggle(trayClick: true);

        }

        static void TrayIcon_MouseMove(object? sender, MouseEventArgs e)
        {
            if (AppConfig.IsHpVictusHardwareMode()) return;

            settingsForm.RefreshSensors();
        }

        internal static Icon GetTrayIcon()
        {
            if (!AppConfig.IsHpVictusHardwareMode()) return Properties.Resources.standard;

            return GetHpTrayIcon(HpPerformanceModeStatus.CurrentBaseMode);
        }

        internal static Icon GetHpTrayIcon(int basePerformanceMode)
        {
            string resourceName = HpTrayIconSelector.GetResourceName(
                HpTrayIconSelector.Select(basePerformanceMode));

            try
            {
                using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                if (stream is not null)
                {
                    using var icon = new Icon(stream);
                    return (Icon)icon.Clone();
                }
            }
            catch (Exception)
            {
                // Fall through to the fixed application identity icon.
            }

            try
            {
                return Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? Properties.Resources.standard;
            }
            catch (Exception)
            {
                return Properties.Resources.standard;
            }
        }

        private static void TrayRetryTimer_Tick(object? sender, EventArgs e)
        {
            System.Windows.Forms.Timer? retryTimer = trayRetryTimer;
            trayRetryTimer = null;
            retryTimer?.Stop();
            retryTimer?.Dispose();

            if (trayIcon is null) return;

            try
            {
                trayIcon.Visible = false;
                trayIcon.Visible = true;
            }
            catch (ObjectDisposedException)
            {
                // Exit cleanup won the race with the one-shot tray refresh.
            }
        }

        static void OnExit(object sender, EventArgs e)
        {
            if (Interlocked.Exchange(ref exitCleanupStarted, 1) != 0) return;

            TryShutdownCleanup("tray retry timer", () =>
            {
                trayRetryTimer?.Stop();
                trayRetryTimer?.Dispose();
                trayRetryTimer = null;
            });
            TryShutdownCleanup("single-instance exit listener", ProcessHelper.StopExitListener);
            if (systemLifecycleHooksRegistered)
            {
                TryShutdownCleanup("system event handlers", () =>
                {
                    SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
                    SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
                    SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
                    SystemEvents.SessionEnding -= SystemEvents_SessionEnding;
                });
                TryShutdownCleanup("power settle timer", () =>
                {
                    powerSettleTimer.Stop();
                    powerSettleTimer.Elapsed -= OnPowerSettled;
                    powerSettleTimer.Dispose();
                });
                TryShutdownCleanup("display events", clamshellControl.UnregisterDisplayEvents);
                TryShutdownCleanup("power notifications", () =>
                {
                    NativeMethods.UnregisterPowerSettingNotification(unRegPowerNotify);
                    NativeMethods.UnregisterPowerSettingNotification(unRegPowerNotifyLid);
                    NativeMethods.UnregisterPowerSettingNotification(unRegPowerNotifyEnergy);
                    NativeMethods.UnregisterSuspendResumeNotification(unRegSuspendResume);
                });
            }
            TryShutdownCleanup("tray icon", () =>
            {
                if (trayIcon is null) return;
                trayIcon.Visible = false;
                trayIcon.Dispose();
                trayIcon = null!;
            });
            if (!unsupportedHardwareMode)
            {
                TryShutdownCleanup("peripheral device events", PeripheralsProvider.UnregisterForDeviceEvents);
            }
        }

        internal static void ExitHpDiagnosticShell()
        {
            if (!AppConfig.IsHpVictusHardwareMode())
            {
                Application.Exit();
                return;
            }

            if (Interlocked.Exchange(ref hpDiagnosticExitStarted, 1) != 0) return;

            void ExitOnUiThread()
            {
                TryShutdownCleanup("HP diagnostic tray retry timer", () =>
                {
                    trayRetryTimer?.Stop();
                    trayRetryTimer?.Dispose();
                    trayRetryTimer = null;
                });

                TryShutdownCleanup("HP diagnostic tray icon", () =>
                {
                    if (trayIcon is null) return;

                    ContextMenuStrip? menu = trayIcon.ContextMenuStrip;
                    trayIcon.ContextMenuStrip = null;
                    trayIcon.Visible = false;
                    trayIcon.Dispose();
                    trayIcon = null!;
                    menu?.Dispose();
                });

                Application.Exit();
                Application.ExitThread();
            }

            if (settingsForm is not null && settingsForm.InvokeRequired && !settingsForm.IsDisposed && settingsForm.IsHandleCreated)
            {
                settingsForm.BeginInvoke((System.Windows.Forms.MethodInvoker)ExitOnUiThread);
                return;
            }

            ExitOnUiThread();
        }

        private static void TryShutdownCleanup(string name, Action cleanup)
        {
            try
            {
                cleanup();
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"Shutdown cleanup failed for {name}: {ex.Message}");
            }
        }

        private static void WriteStartupCrashLog(string[] args, Exception exception)
        {
            try
            {
                string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VictusX", "Logs");
                Directory.CreateDirectory(logDirectory);

                var log = new StringBuilder();
                log.AppendLine("--- VictusX startup exception ---");
                log.AppendLine("Timestamp: " + DateTimeOffset.Now.ToString("O", CultureInfo.InvariantCulture));
                log.AppendLine("Args: " + string.Join(" ", args.Select(SanitizeStartupLogArg)));
                log.AppendLine("Exception: " + exception.GetType().FullName);
                log.AppendLine("Message: " + exception.Message);
                log.AppendLine("StackTrace:");
                log.AppendLine(exception.StackTrace ?? "<no stack trace>");
                log.AppendLine();

                File.AppendAllText(Path.Combine(logDirectory, "startup.log"), log.ToString());
            }
            catch
            {
                // Startup crash logging must never create a second startup failure.
            }
        }

        private static string SanitizeStartupLogArg(string arg)
        {
            if (string.IsNullOrWhiteSpace(arg)) return "\"\"";

            string[] sensitiveMarkers = ["password", "passwd", "secret", "token", "apikey", "api-key", "credential"];
            if (sensitiveMarkers.Any(marker => arg.Contains(marker, StringComparison.OrdinalIgnoreCase))) return "[redacted]";

            return arg.Contains(' ') ? "\"" + arg.Replace("\"", "\\\"") + "\"" : arg;
        }
        static void Charge()
        {
            if (!unsupportedHardwareMode && AppConfig.IsZ13())
            {
                AsusHid.Write([
                    Encoding.ASCII.GetBytes("]ASUS Tech.Inc."),
                    [AsusHid.AURA_ID, 0xC0, 0x03, 0x01]
                ], "Init");
            }

            try
            {
                int limit = AppConfig.Get("charge_limit");
                acpi = global::HardwareControllerFactory.CreateController(unsupportedHardwareMode, hpVictusMode);
                if (limit > 0 && limit < 100)
                {
                    Logger.WriteLine($"------- Startup Battery Limit {limit} -------");
                    if (acpi.IsConnected()) acpi.DeviceSet(AsusACPI.BatteryLimit, limit, "Limit");
                    else if (!unsupportedHardwareMode) AsusACPI.DeviceSetWmi(AsusACPI.BatteryLimit, limit);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Startup Battery Limit Error: " + ex.Message);
            }

            try
            {
                if (!unsupportedHardwareMode) InputDispatcher.StartupBacklight();
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"Startup Backlight: {ex.Message}");
            }

            Application.Exit();
        }

        static void CleanupLegacyFiles()
        {
            string appDir = Path.GetDirectoryName(Application.ExecutablePath) ?? "";
            string[] legacyFiles = ["WinRing0x64.sys", "WinRing0x64.dll"];

            foreach (string fileName in legacyFiles)
            {
                string filePath = Path.Combine(appDir, fileName);
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                        Logger.WriteLine($"Deleted legacy file: {fileName}");
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteLine($"Failed to delete legacy file {fileName}: {ex.Message}");
                    }
                }
            }
        }

    }
}










