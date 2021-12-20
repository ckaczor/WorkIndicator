using Common.Helpers;
using Common.IO;
using Common.Wpf.Extensions;
using Squirrel;
using System;
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

        [STAThread]
        public static void Main(string[] args)
        {
            SquirrelAwareApp.HandleEvents(onAppUpdate: version => Common.Settings.Extensions.RestoreSettings());

            var application = new App();
            application.InitializeComponent();
            application.Run();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _dispatcher = Dispatcher.CurrentDispatcher;

            // Initialize the tray icon
            TrayIcon.Initialize();

            Task.Factory.StartNew(UpdateApp).ContinueWith(task => StartUpdate(task.Result.Result));
        }

        private void StartUpdate(bool updateRequired)
        {
            if (updateRequired)
                return;

            Task.Factory.StartNew(() =>
            {
                // Create an isolation handle to see if we are already running
                _isolationHandle = ApplicationIsolation.GetIsolationHandle();

                // If there is another copy then pass it the command line and exit
                if (_isolationHandle == null)
                {
                    InterprocessMessageSender.SendMessage(Environment.CommandLine);
                    Shutdown();
                    return;
                }

                // Set automatic start into the registry
                Current.SetStartWithWindows(Settings.Default.StartWithWindows);

                // Initialize the light controller
                LightController.Initialize();
            });
        }

        private async Task<bool> UpdateApp()
        {
            return await UpdateCheck.CheckUpdate(HandleUpdateStatus);
        }

        private void HandleUpdateStatus(UpdateCheck.UpdateStatus status, string message)
        {
            if (status == UpdateCheck.UpdateStatus.None)
                message = WorkIndicator.Properties.Resources.Loading;

            _dispatcher.Invoke(() => TrayIcon.SetText(message));
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Get rid of the light controller
            LightController.Dispose();

            // Get rid of the tray icon
            TrayIcon.Dispose();

            // Get rid of the isolation handle
            _isolationHandle.Dispose();

            base.OnExit(e);
        }
    }
}
