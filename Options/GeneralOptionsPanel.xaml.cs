using Common.Wpf.Extensions;
using System.Windows;

namespace WorkIndicator.Options
{
    public partial class GeneralOptionsPanel
    {
        public GeneralOptionsPanel()
        {
            InitializeComponent();
        }

        public override void LoadPanel(object data)
        {
            base.LoadPanel(data);

            var settings = Properties.Settings.Default;

            StartWithWindows.IsChecked = settings.StartWithWindows;
        }

        public override bool ValidatePanel()
        {
            return true;
        }

        public override void SavePanel()
        {
            var settings = Properties.Settings.Default;

            if (StartWithWindows.IsChecked.HasValue && settings.StartWithWindows != StartWithWindows.IsChecked.Value)
                settings.StartWithWindows = StartWithWindows.IsChecked.Value;

            Application.Current.SetStartWithWindows(settings.StartWithWindows);
        }

        public override string CategoryName => Properties.Resources.OptionCategory_General;
    }
}
