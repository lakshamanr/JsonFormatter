using System.Linq;
using System.Windows;
using System.Windows.Controls;
using JsonFormatterApp.ViewModels;

namespace JsonFormatterApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            // Check for unsaved changes in all tabs
            foreach (var tab in _viewModel.Tabs)
            {
                if (tab.IsDirty)
                {
                    var result = MessageBox.Show(
                        "You have unsaved changes in one or more tabs. Do you want to exit anyway?",
                        "Unsaved Changes",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning
                    );

                    if (result == MessageBoxResult.No)
                    {
                        return;
                    }
                    break;
                }
            }

            Application.Current.Shutdown();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            var helpWindow = new HelpWindow
            {
                Owner = this
            };
            helpWindow.Show();
        }

        private void KeyboardShortcuts_Click(object sender, RoutedEventArgs e)
        {
            var helpWindow = new HelpWindow
            {
                Owner = this
            };
            // Show keyboard shortcuts section
            helpWindow.Show();
            // Programmatically click the keyboard shortcuts button
            // This would require accessing the button from code, simplified for now
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow
            {
                Owner = this
            };
            aboutWindow.ShowDialog();
        }
    }
}
