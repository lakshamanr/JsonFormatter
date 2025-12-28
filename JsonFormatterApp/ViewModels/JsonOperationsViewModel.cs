using System;
using System.Windows;
using System.Windows.Input;
using JsonFormatterApp.Helpers;
using JsonFormatterApp.Infrastructure;
using JsonFormatterApp.Models;
using JsonFormatterApp.Services;
using JsonFormatterApp.Views;
using Microsoft.Win32;

namespace JsonFormatterApp.ViewModels
{
    /// <summary>
    /// ViewModel responsible for JSON operations (format, minify, validate, tree, table, comparison, schema)
    /// </summary>
    public class JsonOperationsViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly JsonService _jsonService;
        private readonly JsonTreeService _treeService;
        private readonly JsonTableService _tableService;
        private readonly DiffService _diffService;
        private readonly SchemaValidationService _schemaValidationService;
        private readonly FileService _fileService;
        private readonly TabManagerViewModel _tabManager;
        private int _indentSize = 2;

        public JsonOperationsViewModel(
            IEventAggregator eventAggregator,
            JsonService jsonService,
            JsonTreeService treeService,
            JsonTableService tableService,
            DiffService diffService,
            SchemaValidationService schemaValidationService,
            FileService fileService,
            TabManagerViewModel tabManager)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _jsonService = jsonService ?? throw new ArgumentNullException(nameof(jsonService));
            _treeService = treeService ?? throw new ArgumentNullException(nameof(treeService));
            _tableService = tableService ?? throw new ArgumentNullException(nameof(tableService));
            _diffService = diffService ?? throw new ArgumentNullException(nameof(diffService));
            _schemaValidationService = schemaValidationService ?? throw new ArgumentNullException(nameof(schemaValidationService));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _tabManager = tabManager ?? throw new ArgumentNullException(nameof(tabManager));

            InitializeCommands();

            // Subscribe to events
            _eventAggregator.Subscribe<TabSelectedMessage>(OnTabSelected);
            _eventAggregator.Subscribe<RefreshViewsMessage>(OnRefreshViews);
            _eventAggregator.Subscribe<TabContentChangedMessage>(OnTabContentChanged);
        }

        #region Properties

        public int IndentSize
        {
            get => _indentSize;
            set => SetProperty(ref _indentSize, value);
        }

        private TabItem? CurrentTab => _tabManager.SelectedTab;

        #endregion

        #region Commands

        public ICommand FormatJsonCommand { get; private set; } = null!;
        public ICommand MinifyJsonCommand { get; private set; } = null!;
        public ICommand ValidateJsonCommand { get; private set; } = null!;
        public ICommand BuildTreeCommand { get; private set; } = null!;
        public ICommand BuildTableCommand { get; private set; } = null!;
        public ICommand CompareJsonCommand { get; private set; } = null!;
        public ICommand CompareTabsCommand { get; private set; } = null!;
        public ICommand ValidateSchemaCommand { get; private set; } = null!;
        public ICommand CopyCommand { get; private set; } = null!;
        public ICommand PasteCommand { get; private set; } = null!;

        #endregion

        private void InitializeCommands()
        {
            FormatJsonCommand = new RelayCommand(_ => FormatJson(), _ => CurrentTab != null);
            MinifyJsonCommand = new RelayCommand(_ => MinifyJson(), _ => CurrentTab != null);
            ValidateJsonCommand = new RelayCommand(_ => ValidateJson(), _ => CurrentTab != null);
            BuildTreeCommand = new RelayCommand(_ => BuildTree(), _ => CurrentTab != null);
            BuildTableCommand = new RelayCommand(_ => BuildTable(), _ => CurrentTab != null);
            CompareJsonCommand = new RelayCommand(_ => CompareJson(), _ => CurrentTab != null);
            CompareTabsCommand = new RelayCommand(_ => CompareTabs(), _ => _tabManager.Tabs.Count >= 2);
            ValidateSchemaCommand = new RelayCommand(_ => ValidateSchema(), _ => CurrentTab != null);
            CopyCommand = new RelayCommand(_ => CopyToClipboard(), _ => CurrentTab != null);
            PasteCommand = new RelayCommand(_ => PasteFromClipboard());
        }

        #region Event Handlers

        private void OnTabSelected(TabSelectedMessage message)
        {
            if (message.SelectedTab != null)
            {
                // Only refresh views if the tab has JSON content
                if (!string.IsNullOrWhiteSpace(message.SelectedTab.JsonText))
                {
                    ValidateJson();
                    BuildTree();
                    BuildTable();
                }
                else
                {
                    // Clear views for empty tabs
                    message.SelectedTab.IsValid = true;
                    message.SelectedTab.StatusMessage = "Ready";
                    message.SelectedTab.TreeNodes.Clear();
                    message.SelectedTab.TableData = null;
                }
            }

            // Refresh command can execute states
            CommandManager.InvalidateRequerySuggested();
        }

        private void OnRefreshViews(RefreshViewsMessage message)
        {
            if (message.Tab == _tabManager.SelectedTab)
            {
                ValidateJson();
                BuildTree();
                BuildTable();
            }
        }

        private void OnTabContentChanged(TabContentChangedMessage message)
        {
            // Auto-refresh views when content changes in the selected tab
            if (message.Tab == _tabManager.SelectedTab)
            {
                if (!string.IsNullOrWhiteSpace(message.Tab.JsonText))
                {
                    ValidateJson();
                    BuildTree();
                    BuildTable();
                }
                else
                {
                    // Clear views for empty content
                    message.Tab.IsValid = true;
                    message.Tab.StatusMessage = "Ready";
                    message.Tab.TreeNodes.Clear();
                    message.Tab.TableData = null;
                }
            }
        }

        #endregion

        #region JSON Operations

        private void FormatJson()
        {
            if (CurrentTab == null)
                return;

            if (string.IsNullOrWhiteSpace(CurrentTab.JsonText))
            {
                UpdateStatus("No JSON content to format");
                MessageBox.Show("Please enter some JSON content first.", "No Content", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                CurrentTab.JsonText = _jsonService.FormatJson(CurrentTab.JsonText, IndentSize);
                UpdateStatus("JSON formatted successfully");
                BuildTree();
                BuildTable();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Format error: {ex.Message}");
                MessageBox.Show(ex.Message, "Format Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MinifyJson()
        {
            if (CurrentTab == null)
                return;

            if (string.IsNullOrWhiteSpace(CurrentTab.JsonText))
            {
                UpdateStatus("No JSON content to minify");
                MessageBox.Show("Please enter some JSON content first.", "No Content", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                CurrentTab.JsonText = _jsonService.MinifyJson(CurrentTab.JsonText);
                UpdateStatus("JSON minified successfully");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Minify error: {ex.Message}");
                MessageBox.Show(ex.Message, "Minify Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ValidateJson()
        {
            if (CurrentTab == null)
                return;

            var result = _jsonService.ValidateJson(CurrentTab.JsonText);
            CurrentTab.IsValid = result.IsValid;

            if (result.IsValid)
            {
                CurrentTab.StatusMessage = "✓ Valid JSON";
            }
            else
            {
                CurrentTab.StatusMessage = $"✗ Invalid JSON: {result.ErrorMessage} (Line {result.LineNumber}, Col {result.ColumnNumber})";
            }
        }

        public void BuildTree()
        {
            if (CurrentTab == null)
                return;

            try
            {
                CurrentTab.TreeNodes = _treeService.BuildTree(CurrentTab.JsonText);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Tree build error: {ex.Message}");
            }
        }

        public void BuildTable()
        {
            if (CurrentTab == null)
                return;

            try
            {
                CurrentTab.TableData = _tableService.ConvertToTable(CurrentTab.JsonText);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Table build error: {ex.Message}");
            }
        }

        #endregion

        #region Comparison Operations

        private void CompareJson()
        {
            if (CurrentTab == null)
                return;

            var dialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Select JSON File to Compare"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var compareJson = _fileService.ReadFile(dialog.FileName);
                    var compareWindow = new CompareWindow(
                        CurrentTab.Header,
                        CurrentTab.JsonText,
                        System.IO.Path.GetFileName(dialog.FileName),
                        compareJson
                    );
                    compareWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Comparison Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CompareTabs()
        {
            if (_tabManager.Tabs.Count < 2)
            {
                MessageBox.Show("You need at least 2 tabs open to compare.", "Compare Tabs", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Create a selection dialog with theme support
            var selectionWindow = new Window
            {
                Title = "Select Tabs to Compare",
                Width = 450,
                Height = 350,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Resources = Application.Current.Resources
            };

            var stackPanel = new System.Windows.Controls.StackPanel { Margin = new Thickness(20) };

            var titleBlock = new System.Windows.Controls.TextBlock
            {
                Text = "Compare Tabs",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var label1 = new System.Windows.Controls.TextBlock
            {
                Text = "Select first tab:",
                Margin = new Thickness(0, 0, 0, 5),
                FontSize = 12
            };

            var combo1 = new System.Windows.Controls.ComboBox
            {
                ItemsSource = _tabManager.Tabs,
                DisplayMemberPath = "Header",
                SelectedIndex = CurrentTab != null ? _tabManager.Tabs.IndexOf(CurrentTab) : 0,
                Margin = new Thickness(0, 0, 0, 20),
                Padding = new Thickness(5),
                FontSize = 12
            };

            var label2 = new System.Windows.Controls.TextBlock
            {
                Text = "Select second tab:",
                Margin = new Thickness(0, 0, 0, 5),
                FontSize = 12
            };

            var combo2 = new System.Windows.Controls.ComboBox
            {
                ItemsSource = _tabManager.Tabs,
                DisplayMemberPath = "Header",
                SelectedIndex = _tabManager.Tabs.Count > 1 ? (CurrentTab != null && _tabManager.Tabs.IndexOf(CurrentTab) == 0 ? 1 : 0) : 0,
                Margin = new Thickness(0, 0, 0, 30),
                Padding = new Thickness(5),
                FontSize = 12
            };

            var buttonPanel = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var compareButton = new System.Windows.Controls.Button
            {
                Content = "Compare",
                Width = 100,
                Height = 35,
                Margin = new Thickness(5),
                FontSize = 12,
                FontWeight = FontWeights.Bold
            };

            var cancelButton = new System.Windows.Controls.Button
            {
                Content = "Cancel",
                Width = 100,
                Height = 35,
                Margin = new Thickness(5),
                FontSize = 12
            };

            compareButton.Click += (s, e) =>
            {
                var tab1 = combo1.SelectedItem as TabItem;
                var tab2 = combo2.SelectedItem as TabItem;

                if (tab1 != null && tab2 != null && tab1.Id != tab2.Id)
                {
                    try
                    {
                        var compareWindow = new CompareWindow(
                            tab1.Header,
                            tab1.JsonText,
                            tab2.Header,
                            tab2.JsonText
                        );
                        selectionWindow.Close();
                        compareWindow.ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error comparing tabs: {ex.Message}", "Comparison Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Please select two different tabs.", "Compare Tabs", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };

            cancelButton.Click += (s, e) => selectionWindow.Close();

            buttonPanel.Children.Add(compareButton);
            buttonPanel.Children.Add(cancelButton);

            stackPanel.Children.Add(titleBlock);
            stackPanel.Children.Add(label1);
            stackPanel.Children.Add(combo1);
            stackPanel.Children.Add(label2);
            stackPanel.Children.Add(combo2);
            stackPanel.Children.Add(buttonPanel);

            selectionWindow.Content = stackPanel;
            selectionWindow.ShowDialog();
        }

        #endregion

        #region Schema Validation

        private void ValidateSchema()
        {
            if (CurrentTab == null)
                return;

            var dialog = new OpenFileDialog
            {
                Filter = "JSON Schema Files (*.schema.json)|*.schema.json|JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                Title = "Select JSON Schema File"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var schemaText = _fileService.ReadFile(dialog.FileName);
                    var result = _schemaValidationService.ValidateAgainstSchema(CurrentTab.JsonText, schemaText);

                    if (result.IsValid)
                    {
                        MessageBox.Show("JSON is valid according to the schema!", "Schema Validation", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var errors = string.Join("\n", result.Errors);
                        MessageBox.Show($"Schema validation errors:\n\n{errors}", "Schema Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Schema Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Clipboard Operations

        private void CopyToClipboard()
        {
            if (CurrentTab == null)
                return;

            try
            {
                Clipboard.SetText(CurrentTab.JsonText);
                UpdateStatus("Copied to clipboard");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Copy Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PasteFromClipboard()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    if (CurrentTab == null)
                    {
                        _tabManager.CreateNewTab();
                    }

                    if (CurrentTab != null)
                    {
                        CurrentTab.JsonText = Clipboard.GetText();
                        UpdateStatus("Pasted from clipboard");
                        ValidateJson();
                        BuildTree();
                        BuildTable();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Paste Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Helper Methods

        private void UpdateStatus(string message)
        {
            if (CurrentTab != null)
            {
                CurrentTab.StatusMessage = message;
            }

            _eventAggregator.Publish(new StatusUpdateMessage { Message = message });
        }

        #endregion
    }
}
