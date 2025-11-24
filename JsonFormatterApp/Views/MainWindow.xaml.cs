using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit;
using JsonFormatterApp.ViewModels;

namespace JsonFormatterApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly Dictionary<Models.TabItem, EditorBinding> _editorBindings = new();

        private class EditorBinding
        {
            public TextEditor Editor { get; set; } = null!;
            public EventHandler<System.Windows.Controls.TextChangedEventArgs>? TextChangedHandler { get; set; }
            public PropertyChangedEventHandler? PropertyChangedHandler { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
        }

        private void JsonEditor_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not TextEditor editor)
                return;

            var tab = editor.DataContext as Models.TabItem;
            if (tab == null)
                return;

            // Remove existing binding if any
            if (_editorBindings.ContainsKey(tab))
            {
                CleanupEditorBinding(tab);
            }

            // Set initial text without triggering events
            editor.TextChanged -= null; // Ensure no handlers first
            editor.Text = tab.JsonText;

            // Create text changed handler
            EventHandler<System.Windows.Controls.TextChangedEventArgs> textChangedHandler = (s, args) =>
            {
                // Prevent recursive updates
                if (editor.Text == tab.JsonText)
                    return;

                // Update the tab's JSON text
                tab.JsonText = editor.Text;

                // Only update views if this is the selected tab
                if (_viewModel.SelectedTab == tab)
                {
                    _viewModel.ValidateJson();
                    _viewModel.BuildTree();
                    _viewModel.BuildTable();
                }
            };

            // Create property changed handler
            PropertyChangedEventHandler propertyChangedHandler = (s, args) =>
            {
                if (args.PropertyName == nameof(Models.TabItem.JsonText))
                {
                    // Prevent recursive updates
                    if (editor.Text == tab.JsonText)
                        return;

                    // Temporarily remove text changed handler to prevent recursion
                    editor.TextChanged -= textChangedHandler;
                    editor.Text = tab.JsonText;
                    editor.TextChanged += textChangedHandler;
                }
            };

            // Attach handlers
            editor.TextChanged += textChangedHandler;
            tab.PropertyChanged += propertyChangedHandler;

            // Store binding for cleanup
            _editorBindings[tab] = new EditorBinding
            {
                Editor = editor,
                TextChangedHandler = textChangedHandler,
                PropertyChangedHandler = propertyChangedHandler
            };
        }

        private void JsonEditor_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is not TextEditor editor)
                return;

            var tab = editor.DataContext as Models.TabItem;
            if (tab == null)
                return;

            CleanupEditorBinding(tab);
        }

        private void CleanupEditorBinding(Models.TabItem tab)
        {
            if (!_editorBindings.TryGetValue(tab, out var binding))
                return;

            // Remove event handlers
            if (binding.TextChangedHandler != null)
            {
                binding.Editor.TextChanged -= binding.TextChangedHandler;
            }

            if (binding.PropertyChangedHandler != null)
            {
                tab.PropertyChanged -= binding.PropertyChangedHandler;
            }

            // Remove from dictionary
            _editorBindings.Remove(tab);
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

            // Cleanup all bindings
            foreach (var tab in _editorBindings.Keys.ToArray())
            {
                CleanupEditorBinding(tab);
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
