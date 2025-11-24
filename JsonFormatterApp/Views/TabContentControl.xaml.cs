using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit;
using JsonFormatterApp.ViewModels;

namespace JsonFormatterApp.Views
{
    public partial class TabContentControl : UserControl
    {
        private Models.TabItem? _tab;
        private MainViewModel? _viewModel;
        private EventHandler? _textChangedHandler;
        private PropertyChangedEventHandler? _propertyChangedHandler;

        public TabContentControl()
        {
            InitializeComponent();
            Loaded += TabContentControl_Loaded;
            Unloaded += TabContentControl_Unloaded;
        }

        private void TabContentControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Get the TabItem from DataContext
            _tab = DataContext as Models.TabItem;
            if (_tab == null)
                return;

            // Get the MainViewModel from the Window
            var window = Window.GetWindow(this);
            _viewModel = window?.DataContext as MainViewModel;
            if (_viewModel == null)
                return;

            System.Diagnostics.Debug.WriteLine($"[TabContent Loaded] Tab: {_tab.Header}, TabId: {_tab.Id}, EditorHashCode: {JsonEditor.GetHashCode()}");

            // CRITICAL: Clear and initialize this editor with the tab's content
            JsonEditor.Document.Text = string.Empty;
            JsonEditor.Text = _tab.JsonText ?? string.Empty;
            JsonEditor.Document.UndoStack.ClearAll();

            // Create text changed handler for THIS specific editor
            _textChangedHandler = (s, args) =>
            {
                if (_tab == null || JsonEditor.Text == _tab.JsonText)
                    return;

                // Update the tab's JSON text
                _tab.JsonText = JsonEditor.Text;

                // Only update views if this is the selected tab AND it has content
                if (_viewModel?.SelectedTab == _tab && !string.IsNullOrWhiteSpace(JsonEditor.Text))
                {
                    _viewModel.ValidateJson();
                    _viewModel.BuildTree();
                    _viewModel.BuildTable();
                }
                else if (_viewModel?.SelectedTab == _tab && string.IsNullOrWhiteSpace(JsonEditor.Text))
                {
                    // Clear validation for empty content
                    _tab.IsValid = true;
                    _tab.StatusMessage = "Ready";
                }
            };

            // Create property changed handler
            _propertyChangedHandler = (s, args) =>
            {
                if (args.PropertyName == nameof(Models.TabItem.JsonText) && _tab != null)
                {
                    // Prevent recursive updates
                    if (JsonEditor.Text == _tab.JsonText)
                        return;

                    // Temporarily remove text changed handler to prevent recursion
                    if (_textChangedHandler != null)
                        JsonEditor.TextChanged -= _textChangedHandler;

                    JsonEditor.Text = _tab.JsonText;

                    if (_textChangedHandler != null)
                        JsonEditor.TextChanged += _textChangedHandler;
                }
            };

            // Attach handlers
            JsonEditor.TextChanged += _textChangedHandler;
            _tab.PropertyChanged += _propertyChangedHandler;

            System.Diagnostics.Debug.WriteLine($"[TabContent Loaded] Handlers attached for tab {_tab.Header}");
        }

        private void TabContentControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_tab == null)
                return;

            System.Diagnostics.Debug.WriteLine($"[TabContent Unloaded] Tab: {_tab.Header}, TabId: {_tab.Id}, EditorHashCode: {JsonEditor.GetHashCode()}");

            // Remove event handlers
            if (_textChangedHandler != null)
            {
                JsonEditor.TextChanged -= _textChangedHandler;
                _textChangedHandler = null;
            }

            if (_propertyChangedHandler != null && _tab != null)
            {
                _tab.PropertyChanged -= _propertyChangedHandler;
                _propertyChangedHandler = null;
            }

            // Clear editor content
            JsonEditor.Document.Text = string.Empty;

            System.Diagnostics.Debug.WriteLine($"[TabContent Unloaded] Cleanup completed for tab {_tab.Header}");
        }
    }
}
