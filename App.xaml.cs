using Common.Helpers;
using Common.IO;
using Common.Wpf.Extensions;
using Serilog;
using Squirrel;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Settings = WorkIndicator.Properties.Settings;

namespace WorkIndicator
{
    public partial class App
    {
        private IDisposable _isolationHandle;

        public static string UpdateUrl = "https://github.com/ckaczor/WorkIndicator";

        private Dispatcher _dispatcher;
        private InterprocessMessageListener _commandLineListener;

        [STAThread]
        public static void Main(string[] args)
        {
            SquirrelAwareApp.HandleEvents(onAppUpdate: version => Common.Settings.Extensions.RestoreSettings());

            var location = Assembly.GetEntryAssembly().Location;

            var path = Path.GetDirectoryName(location);

            var logFile = Path.Combine(path, "log.txt");

            Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .WriteTo.File(logFile, rollingInterval: RollingInterval.Day)
                        .CreateLogger();

            Log.Logger.Debug($"Startup");

            var application = new App();
            application.InitializeComponent();
            application.Run();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _dispatcher = Dispatcher.CurrentDispatcher;

            // Create an isolation handle to see if we are already running
            _isolationHandle = ApplicationIsolation.GetIsolationHandle();

            // If there is another copy then pass it the command line and exit
            if (_isolationHandle == null)
            {
                try
                {
                    InterprocessMessageSender.SendMessage(Environment.CommandLine);
                }
                catch { }

                Shutdown();
                return;
            }

            // Initialize the command line listener
            _commandLineListener = new InterprocessMessageListener(Assembly.GetEntryAssembly().GetName().Name);
            _commandLineListener.MessageReceived += HandleCommandLine;

            // Initialize the tray icon
            TrayIcon.Initialize();

            // Set automatic start into the registry
            Current.SetStartWithWindows(Settings.Default.StartWithWindows);

            // Initialize the light controller
            LightController.Initialize();

            Task.Factory.StartNew(CheckUpdate);
        }

        private void HandleCommandLine(object sender, InterprocessMessageListener.InterprocessMessageEventArgs e)
        {

        }

        private async Task<bool> CheckUpdate()
        {
            return await UpdateCheck.CheckUpdate(HandleUpdateStatus);
        }

        private void HandleUpdateStatus(UpdateCheck.UpdateStatus status, string message)
        {
            if (status != UpdateCheck.UpdateStatus.Installing)
                return;

            _dispatcher.Invoke(() => TrayIcon.ShowUpdateMessage(message));

            TrayIcon.Dispose();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Logger.Debug($"Exit");

            // Get rid of the light controller
            LightController.Dispose();

            // Get rid of the tray icon
            TrayIcon.Dispose();

            // Get rid of the isolation handle
            _isolationHandle?.Dispose();

            base.OnExit(e);
        }
    }
}
