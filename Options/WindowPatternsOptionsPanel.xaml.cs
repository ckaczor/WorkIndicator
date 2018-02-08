using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WorkIndicator.Options
{
    public partial class WindowPatternsOptionsPanel
    {
        public WindowPatternsOptionsPanel()
        {
            InitializeComponent();
        }

        private WindowPatterns WindowPatterns => Data as WindowPatterns;

        public override void LoadPanel(object data)
        {
            base.LoadPanel(data);

            WindowPatternsGrid.ItemsSource = WindowPatterns;

            WindowPatternsGrid.SelectedItem = WindowPatterns.FirstOrDefault();
            SetButtonStates();
        }

        public override bool ValidatePanel()
        {
            return true;
        }

        public override void SavePanel()
        {

        }

        public override string CategoryName => Properties.Resources.OptionCategory_WindowPatterns;

        private void SetButtonStates()
        {
            EditWindowPatternButton.IsEnabled = (WindowPatternsGrid.SelectedItems.Count == 1);
            DeleteWindowPatternButton.IsEnabled = (WindowPatternsGrid.SelectedItems.Count > 0);
        }

        private void AddWindowPattern()
        {
            var windowPattern = new WindowPattern();

            var windowPatternWindow = new WindowPatternWindow();

            var result = windowPatternWindow.Display(windowPattern, Window.GetWindow(this));

            if (result.HasValue && result.Value)
            {
                WindowPatterns.Add(windowPattern);

                WindowPatternsGrid.SelectedItem = windowPattern;

                SetButtonStates();
            }
        }

        private void EditSelectedWindowPattern()
        {
            if (WindowPatternsGrid.SelectedItem == null)
                return;

            var windowPattern = WindowPatternsGrid.SelectedItem as WindowPattern;

            var windowPatternWindow = new WindowPatternWindow();

            windowPatternWindow.Display(windowPattern, Window.GetWindow(this));
        }

        private void DeleteSelectedWindowPattern()
        {
            var windowPattern = WindowPatternsGrid.SelectedItem as WindowPattern;
            var index = WindowPatternsGrid.SelectedIndex;

            WindowPatterns.Remove(windowPattern);

            if (WindowPatternsGrid.Items.Count == index)
                WindowPatternsGrid.SelectedIndex = WindowPatternsGrid.Items.Count - 1;
            else if (WindowPatternsGrid.Items.Count >= index)
                WindowPatternsGrid.SelectedIndex = index;

            SetButtonStates();
        }

        private void HandleAddWindowPatternButtonClick(object sender, RoutedEventArgs e)
        {
            AddWindowPattern();
        }

        private void HandleEditWindowPatternButtonClick(object sender, RoutedEventArgs e)
        {
            EditSelectedWindowPattern();
        }

        private void HandleDeleteWindowPatternButtonClick(object sender, RoutedEventArgs e)
        {
            DeleteSelectedWindowPattern();
        }

        private void HandleWindowPatternsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetButtonStates();
        }

        private void HandleWindowPatternsDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditSelectedWindowPattern();
        }
    }
}
