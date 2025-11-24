using System.Windows;
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

            // Bind AvalonEdit text to ViewModel
            JsonEditor.TextChanged += (s, e) =>
            {
                _viewModel.JsonText = JsonEditor.Text;
            };

            // Update editor when text changes from ViewModel
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.JsonText) && JsonEditor.Text != _viewModel.JsonText)
                {
                    JsonEditor.Text = _viewModel.JsonText;
                }
            };
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "JSON Formatter & Validator Pro\n\n" +
                "Version 1.0.0\n\n" +
                "A professional JSON formatting, validation, and conversion tool.\n\n" +
                "Features:\n" +
                "• Format & Minify JSON\n" +
                "• JSON Validation with error highlighting\n" +
                "• Tree View Explorer\n" +
                "• Convert to XML, C#, SQL, YAML\n" +
                "• JSON Schema Validation\n" +
                "• JSON Diff & Compare\n" +
                "• Dark/Light Themes\n" +
                "• Base64 & URL Encoding/Decoding\n\n" +
                "© 2024 JSON Formatter Pro",
                "About JSON Formatter Pro",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }
}
