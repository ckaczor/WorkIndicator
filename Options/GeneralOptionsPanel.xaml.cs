using System;
using Common.Wpf.Extensions;
using System.Collections.ObjectModel;
using System.Windows;

namespace WorkIndicator.Options
{
    public partial class GeneralOptionsPanel
    {
        public class StatusItem
        {
            public Status Value { get; set; }
            public string Text { get; set; }

            public StatusItem(Status value, string text)
            {
                Value = value;
                Text = text;
            }
        }

        public GeneralOptionsPanel()
        {
            InitializeComponent();
        }

        public override void LoadPanel(object data)
        {
            base.LoadPanel(data);

            var settings = Properties.Settings.Default;

            StartWithWindows.IsChecked = settings.StartWithWindows;

            DefaultStatus.SelectedValue = Enum.Parse(typeof(Status), settings.DefaultStatus);
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

            settings.DefaultStatus = DefaultStatus.SelectedValue.ToString();
        }

        public override string CategoryName => Properties.Resources.OptionCategory_General;

        public ObservableCollection<StatusItem> DefaultStatusList => new ObservableCollection<StatusItem>
        {
            new StatusItem(Status.Free, Properties.Resources.Free),
            new StatusItem(Status.Working, Properties.Resources.Working),
            new StatusItem(Status.OnPhone, Properties.Resources.OnPhone),
            new StatusItem(Status.Talking, Properties.Resources.Talking)
        };
    }
}
