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
        private bool _isInitializing = false;

        public TabContentControl()
        {
            InitializeComponent();

            // Use DataContextChanged instead of Loaded to ensure DataContext is available
            DataContextChanged += TabContentControl_DataContextChanged;
            Unloaded += TabContentControl_Unloaded;
        }

        private void TabContentControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Clean up previous bindings if any
            if (_tab != null)
            {
                Cleanup();
            }

            // Get the new TabItem from DataContext
            _tab = DataContext as Models.TabItem;
            if (_tab == null)
                return;

            // Get the MainViewModel from the Window
            var window = Window.GetWindow(this);
            _viewModel = window?.DataContext as MainViewModel;
            if (_viewModel == null)
                return;

            System.Diagnostics.Debug.WriteLine($"[TabContent Init] Tab: {_tab.Header}, TabId: {_tab.Id}, EditorHashCode: {JsonEditor.GetHashCode()}");

            // Initialize the editor with the tab's content
            _isInitializing = true;
            try
            {
                // CRITICAL: Clear and initialize this editor with the tab's content
                JsonEditor.Document.Text = string.Empty;
                JsonEditor.Text = _tab.JsonText ?? string.Empty;
                JsonEditor.Document.UndoStack.ClearAll();
            }
            finally
            {
                _isInitializing = false;
            }

            // Create text changed handler for THIS specific editor
            _textChangedHandler = (s, args) =>
            {
                // Skip if we're initializing
                if (_isInitializing || _tab == null)
                    return;

                // Prevent recursive updates
                if (JsonEditor.Text == _tab.JsonText)
                    return;

                // Update the tab's JSON text
                // Note: Setting JsonText will trigger IsDirty = true in the TabItem model
                _tab.JsonText = JsonEditor.Text;

                // The MainViewModel already handles validation/tree/table updates
                // when SelectedTab changes or when commands are executed.
                // We don't need to duplicate that logic here.

                System.Diagnostics.Debug.WriteLine($"[TabContent] Text changed for tab {_tab.Header}, Length: {JsonEditor.Text.Length}");
            };

            // Create property changed handler to sync external changes back to editor
            _propertyChangedHandler = (s, args) =>
            {
                if (args.PropertyName == nameof(Models.TabItem.JsonText) && _tab != null)
                {
                    // Prevent recursive updates
                    if (JsonEditor.Text == _tab.JsonText)
                        return;

                    System.Diagnostics.Debug.WriteLine($"[TabContent] Syncing JsonText to editor for tab {_tab.Header}");

                    // Temporarily remove text changed handler to prevent recursion
                    if (_textChangedHandler != null)
                        JsonEditor.TextChanged -= _textChangedHandler;

                    _isInitializing = true;
                    try
                    {
                        JsonEditor.Text = _tab.JsonText;
                    }
                    finally
                    {
                        _isInitializing = false;
                    }

                    if (_textChangedHandler != null)
                        JsonEditor.TextChanged += _textChangedHandler;
                }
            };

            // Attach handlers
            JsonEditor.TextChanged += _textChangedHandler;
            _tab.PropertyChanged += _propertyChangedHandler;

            System.Diagnostics.Debug.WriteLine($"[TabContent Init] Handlers attached for tab {_tab.Header}");
        }

        private void TabContentControl_Unloaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[TabContent Unloaded] Tab: {_tab?.Header}, EditorHashCode: {JsonEditor.GetHashCode()}");
            Cleanup();
        }

        private void Cleanup()
        {
            if (_tab == null)
                return;

            System.Diagnostics.Debug.WriteLine($"[TabContent Cleanup] Tab: {_tab.Header}");

            // Remove event handlers
            if (_textChangedHandler != null)
            {
                JsonEditor.TextChanged -= _textChangedHandler;
                _textChangedHandler = null;
            }

            if (_propertyChangedHandler != null)
            {
                _tab.PropertyChanged -= _propertyChangedHandler;
                _propertyChangedHandler = null;
            }

            // Clear editor content
            JsonEditor.Document.Text = string.Empty;

            _tab = null;
            _viewModel = null;
        }
    }
}
