using System;
using System.Windows;

using Common.Helpers;
using Common.IO;
using Common.Wpf.Extensions;

using Settings = WorkIndicator.Properties.Settings;

namespace WorkIndicator
{
    public partial class App
    {
        private IDisposable _isolationHandle;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

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

            // Initialize the tray icon
            TrayIcon.Initialize();

            // Initialize the light controller
            LightController.Initialize();
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
