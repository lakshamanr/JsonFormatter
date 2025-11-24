using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit;
using JsonFormatterApp.ViewModels;

namespace JsonFormatterApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly Dictionary<object, TextEditor> _editorMap = new();

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
        }

        private void JsonEditor_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextEditor editor)
            {
                var tab = editor.DataContext as Models.TabItem;
                if (tab != null && !_editorMap.ContainsKey(tab))
                {
                    _editorMap[tab] = editor;

                    // Set initial text
                    editor.Text = tab.JsonText;

                    // Bind text changes from editor to view model
                    editor.TextChanged += (s, args) =>
                    {
                        if (tab.JsonText != editor.Text)
                        {
                            tab.JsonText = editor.Text;
                            _viewModel.ValidateJson();
                            _viewModel.BuildTree();
                            _viewModel.BuildTable();
                        }
                    };

                    // Bind text changes from view model to editor
                    tab.PropertyChanged += (s, args) =>
                    {
                        if (args.PropertyName == nameof(Models.TabItem.JsonText) && editor.Text != tab.JsonText)
                        {
                            editor.Text = tab.JsonText;
                        }
                    };
                }
            }
        }

        private void JsonEditor_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextEditor editor)
            {
                var tab = editor.DataContext as Models.TabItem;
                if (tab != null && _editorMap.ContainsKey(tab))
                {
                    _editorMap.Remove(tab);
                }
            }
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

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "JSON Formatter & Validator Pro\n\n" +
                "Version 2.0.0 - Multi-Tab Edition\n\n" +
                "A professional JSON formatting, validation, and conversion tool.\n\n" +
                "New Features in v2.0:\n" +
                "• Multi-tab support for working with multiple files\n" +
                "• Tabular view for JSON arrays\n" +
                "• Compare tabs feature\n" +
                "• Enhanced UI with better organization\n\n" +
                "Core Features:\n" +
                "• Format & Minify JSON\n" +
                "• JSON Validation with error highlighting\n" +
                "• Tree View Explorer\n" +
                "• Table View for arrays\n" +
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
