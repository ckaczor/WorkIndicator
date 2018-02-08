﻿using Common.Wpf.Extensions;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace WorkIndicator.Options
{
    public partial class WindowPatternWindow
    {
        public WindowPatternWindow()
        {
            InitializeComponent();

            Icon = ((Icon)Properties.Resources.ResourceManager.GetObject("ApplicationIcon")).ToImageSource();
        }

        public bool? Display(WindowPattern windowPattern, Window owner)
        {
            // Set the data context
            DataContext = windowPattern;

            // Set the title based on the state of the item
            Title = string.IsNullOrWhiteSpace(windowPattern.Name) ? Properties.Resources.WindowPatternWindowAdd : Properties.Resources.WindowPatternWindowEdit;

            // Set the window owner
            Owner = owner;

            // Show the dialog and result the result
            return ShowDialog();
        }

        private void HandleOkayButtonClick(object sender, RoutedEventArgs e)
        {
            // Get a list of all framework elements and explicit binding expressions
            var bindingExpressions = this.GetBindingExpressions(new[] { UpdateSourceTrigger.Explicit });

            // Loop over each binding expression and clear any existing error
            this.ClearAllValidationErrors(bindingExpressions);

            // Force all explicit bindings to update the source
            this.UpdateAllSources(bindingExpressions);

            // See if there are any errors
            var hasError = bindingExpressions.Any(b => b.BindingExpression.HasError);

            // If there was an error then set focus to the bad controls
            if (hasError)
            {
                // Get the first framework element with an error
                var firstErrorElement = bindingExpressions.First(b => b.BindingExpression.HasError).FrameworkElement;

                // Set focus
                firstErrorElement.Focus();

                return;
            }

            // Dialog is good
            DialogResult = true;

            // Close the dialog
            Close();
        }
    }
}
