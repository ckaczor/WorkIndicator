using Common.Wpf.Windows;
using System.Collections.Generic;
using System.Windows.Forms;
using WorkIndicator.Options;
using WorkIndicator.Properties;

using Application = System.Windows.Application;

namespace WorkIndicator
{
    internal static class TrayIcon
    {
        private static NotifyIcon _trayIcon;

        private static bool _initialized;

        private static CategoryWindow _optionsWindow;

        public static void Initialize()
        {
            // Create the tray icon
            _trayIcon = new NotifyIcon { Icon = Resources.ApplicationIcon, Text = Resources.ApplicationName };

            // Setup the menu
            var contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Opening += HandleContextMenuStripOpening;

            // Add the menu items
            var menuItem = new ToolStripMenuItem(Resources.Auto, null, HandleStatusMenuClick) { Tag = Status.Auto };
            contextMenuStrip.Items.Add(menuItem);

            // --

            menuItem = new ToolStripMenuItem(Resources.Free, null, HandleStatusMenuClick) { Tag = Status.Free };
            contextMenuStrip.Items.Add(menuItem);

            // --

            menuItem = new ToolStripMenuItem(Resources.Working, null, HandleStatusMenuClick) { Tag = Status.Working };
            contextMenuStrip.Items.Add(menuItem);

            // --

            menuItem = new ToolStripMenuItem(Resources.OnPhone, null, HandleStatusMenuClick) { Tag = Status.OnPhone };
            contextMenuStrip.Items.Add(menuItem);

            // --

            menuItem = new ToolStripMenuItem(Resources.Talking, null, HandleStatusMenuClick) { Tag = Status.Talking };
            contextMenuStrip.Items.Add(menuItem);

            // --

            contextMenuStrip.Items.Add("-");
            contextMenuStrip.Items.Add(Resources.TrayIconContextMenuSettings, null, HandleContextMenuSettingsClick);

            // --

            contextMenuStrip.Items.Add("-");
            contextMenuStrip.Items.Add(Resources.TrayIconContextMenuExit, null, HandleContextMenuExitClick);

            // Set the menu into the icon
            _trayIcon.ContextMenuStrip = contextMenuStrip;

            // Show the icon
            _trayIcon.Visible = true;

            _initialized = true;
        }

        public static void SetText(string text)
        {
            _trayIcon.Text = text;
        }

        static void HandleContextMenuStripOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (ToolStripItem menuItem in _trayIcon.ContextMenuStrip.Items)
            {
                if (menuItem.Tag == null)
                    continue;

                var status = (Status)menuItem.Tag;

                ((ToolStripMenuItem)menuItem).Checked = (LightController.Status == status);
            }
        }

        private static void HandleStatusMenuClick(object sender, System.EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;

            var status = (Status)menuItem.Tag;

            LightController.Status = status;
        }

        private static void HandleContextMenuSettingsClick(object sender, System.EventArgs e)
        {
            ShowSettings();
        }

        private static void HandleContextMenuExitClick(object sender, System.EventArgs e)
        {
            // Shutdown the application
            Application.Current.Shutdown();
        }

        public static void Dispose()
        {
            if (!_initialized)
                return;

            // Get rid of the icon
            _trayIcon.Visible = false;
            _trayIcon.Dispose();

            _initialized = false;
        }

        private static void ShowSettings()
        {
            var panels = new List<CategoryPanel>
            {
                new GeneralOptionsPanel(),
                new AboutOptionsPanel()
            };

            if (_optionsWindow == null)
            {
                _optionsWindow = new CategoryWindow(null, panels, Resources.ResourceManager, "OptionsWindow");
                _optionsWindow.Closed += (o, args) => { _optionsWindow = null; };
            }

            var dialogResult = _optionsWindow.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            {
                Settings.Default.Save();
            }
        }
    }
}
