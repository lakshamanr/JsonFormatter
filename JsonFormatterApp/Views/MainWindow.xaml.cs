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
            public EventHandler? TextChangedHandler { get; set; }
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

            // Verify this is a unique editor instance for this tab
            System.Diagnostics.Debug.WriteLine($"[Editor Loaded] Tab: {tab.Header}, TabId: {tab.Id}, EditorHashCode: {editor.GetHashCode()}");

            // Remove existing binding if any (should not happen, but safety check)
            if (_editorBindings.ContainsKey(tab))
            {
                System.Diagnostics.Debug.WriteLine($"[WARNING] Editor already exists for tab {tab.Header}, cleaning up old binding");
                CleanupEditorBinding(tab);
            }

            // CRITICAL: Each tab must have its own editor instance
            // Clear the document completely before setting new content
            editor.Document.Text = string.Empty;
            editor.Text = tab.JsonText ?? string.Empty;

            // Force document refresh to ensure clean state
            editor.Document.UndoStack.ClearAll();

            // Create text changed handler
            EventHandler textChangedHandler = (s, args) =>
            {
                // Prevent recursive updates
                if (editor.Text == tab.JsonText)
                    return;

                // Update the tab's JSON text
                tab.JsonText = editor.Text;

                // Only update views if this is the selected tab AND it has content
                if (_viewModel.SelectedTab == tab && !string.IsNullOrWhiteSpace(editor.Text))
                {
                    _viewModel.ValidateJson();
                    _viewModel.BuildTree();
                    _viewModel.BuildTable();
                }
                else if (_viewModel.SelectedTab == tab && string.IsNullOrWhiteSpace(editor.Text))
                {
                    // Clear validation for empty content
                    tab.IsValid = true;
                    tab.StatusMessage = "Ready";
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

            System.Diagnostics.Debug.WriteLine($"[Editor Unloaded] Tab: {tab.Header}, TabId: {tab.Id}, EditorHashCode: {editor.GetHashCode()}");
            CleanupEditorBinding(tab);
        }

        private void CleanupEditorBinding(Models.TabItem tab)
        {
            if (!_editorBindings.TryGetValue(tab, out var binding))
            {
                System.Diagnostics.Debug.WriteLine($"[Cleanup] No binding found for tab {tab.Header}");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[Cleanup] Removing binding for tab {tab.Header}, EditorHashCode: {binding.Editor.GetHashCode()}");

            // Remove event handlers
            if (binding.TextChangedHandler != null)
            {
                binding.Editor.TextChanged -= binding.TextChangedHandler;
            }

            if (binding.PropertyChangedHandler != null)
            {
                tab.PropertyChanged -= binding.PropertyChangedHandler;
            }

            // Clear the editor content to prevent any lingering references
            binding.Editor.Document.Text = string.Empty;

            // Remove from dictionary
            _editorBindings.Remove(tab);

            System.Diagnostics.Debug.WriteLine($"[Cleanup] Completed for tab {tab.Header}. Active bindings: {_editorBindings.Count}");
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
