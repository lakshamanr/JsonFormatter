using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using JsonFormatterApp.Helpers;
using JsonFormatterApp.Models;
using JsonFormatterApp.Services;
using JsonFormatterApp.Views;
using Microsoft.Win32;

namespace JsonFormatterApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly JsonService _jsonService;
        private readonly JsonTreeService _treeService;
        private readonly JsonTableService _tableService;
        private readonly ConversionService _conversionService;
        private readonly FileService _fileService;
        private readonly SchemaValidationService _schemaValidationService;
        private readonly DiffService _diffService;
        private readonly EncodingService _encodingService;

        private ObservableCollection<TabItem> _tabs = new();
        private TabItem? _selectedTab;
        private int _indentSize = 2;
        private ObservableCollection<RecentFile> _recentFiles = new();

        public MainViewModel()
        {
            _jsonService = new JsonService();
            _treeService = new JsonTreeService();
            _tableService = new JsonTableService();
            _conversionService = new ConversionService();
            _fileService = new FileService();
            _schemaValidationService = new SchemaValidationService();
            _diffService = new DiffService();
            _encodingService = new EncodingService();

            InitializeCommands();
            LoadRecentFiles();

            // Create initial tab
            NewFileCommand.Execute(null);

            // Watch for tab changes
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedTab) && SelectedTab != null)
                {
                    // Refresh views when switching tabs
                    ValidateJson();
                    BuildTree();
                    BuildTable();
                }
            };
        }

        #region Properties

        public ObservableCollection<TabItem> Tabs
        {
            get => _tabs;
            set => SetProperty(ref _tabs, value);
        }

        public TabItem? SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        public int IndentSize
        {
            get => _indentSize;
            set => SetProperty(ref _indentSize, value);
        }

        public ObservableCollection<RecentFile> RecentFiles
        {
            get => _recentFiles;
            set => SetProperty(ref _recentFiles, value);
        }

        #endregion

        #region Commands

        public ICommand NewFileCommand { get; private set; } = null!;
        public ICommand OpenFileCommand { get; private set; } = null!;
        public ICommand SaveFileCommand { get; private set; } = null!;
        public ICommand SaveAsCommand { get; private set; } = null!;
        public ICommand CloseTabCommand { get; private set; } = null!;
        public ICommand FormatJsonCommand { get; private set; } = null!;
        public ICommand MinifyJsonCommand { get; private set; } = null!;
        public ICommand ValidateJsonCommand { get; private set; } = null!;
        public ICommand CopyCommand { get; private set; } = null!;
        public ICommand PasteCommand { get; private set; } = null!;
        public ICommand BuildTreeCommand { get; private set; } = null!;
        public ICommand BuildTableCommand { get; private set; } = null!;
        public ICommand ConvertToXmlCommand { get; private set; } = null!;
        public ICommand ConvertToCSharpCommand { get; private set; } = null!;
        public ICommand ConvertToSqlCommand { get; private set; } = null!;
        public ICommand ConvertToYamlCommand { get; private set; } = null!;
        public ICommand ToggleThemeCommand { get; private set; } = null!;
        public ICommand ValidateSchemaCommand { get; private set; } = null!;
        public ICommand CompareJsonCommand { get; private set; } = null!;
        public ICommand CompareTabsCommand { get; private set; } = null!;
        public ICommand Base64EncodeCommand { get; private set; } = null!;
        public ICommand Base64DecodeCommand { get; private set; } = null!;
        public ICommand UrlEncodeCommand { get; private set; } = null!;
        public ICommand UrlDecodeCommand { get; private set; } = null!;
        public ICommand OpenRecentFileCommand { get; private set; } = null!;
        public ICommand ClearRecentFilesCommand { get; private set; } = null!;

        #endregion

        private void InitializeCommands()
        {
            NewFileCommand = new RelayCommand(_ => NewFile());
            OpenFileCommand = new RelayCommand(_ => OpenFile());
            SaveFileCommand = new RelayCommand(_ => SaveFile(), _ => SelectedTab != null && !string.IsNullOrEmpty(SelectedTab.FilePath));
            SaveAsCommand = new RelayCommand(_ => SaveFileAs(), _ => SelectedTab != null);
            CloseTabCommand = new RelayCommand(param => CloseTab(param as TabItem));
            FormatJsonCommand = new RelayCommand(_ => FormatJson(), _ => SelectedTab != null);
            MinifyJsonCommand = new RelayCommand(_ => MinifyJson(), _ => SelectedTab != null);
            ValidateJsonCommand = new RelayCommand(_ => ValidateJson(), _ => SelectedTab != null);
            CopyCommand = new RelayCommand(_ => CopyToClipboard(), _ => SelectedTab != null);
            PasteCommand = new RelayCommand(_ => PasteFromClipboard());
            BuildTreeCommand = new RelayCommand(_ => BuildTree(), _ => SelectedTab != null);
            BuildTableCommand = new RelayCommand(_ => BuildTable(), _ => SelectedTab != null);
            ConvertToXmlCommand = new RelayCommand(_ => ConvertToXml(), _ => SelectedTab != null);
            ConvertToCSharpCommand = new RelayCommand(_ => ConvertToCSharp(), _ => SelectedTab != null);
            ConvertToSqlCommand = new RelayCommand(_ => ConvertToSql(), _ => SelectedTab != null);
            ConvertToYamlCommand = new RelayCommand(_ => ConvertToYaml(), _ => SelectedTab != null);
            ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());
            ValidateSchemaCommand = new RelayCommand(_ => ValidateSchema(), _ => SelectedTab != null);
            CompareJsonCommand = new RelayCommand(_ => CompareJson(), _ => SelectedTab != null);
            CompareTabsCommand = new RelayCommand(_ => CompareTabs(), _ => Tabs.Count >= 2);
            Base64EncodeCommand = new RelayCommand(_ => Base64Encode(), _ => SelectedTab != null);
            Base64DecodeCommand = new RelayCommand(_ => Base64Decode(), _ => SelectedTab != null);
            UrlEncodeCommand = new RelayCommand(_ => UrlEncode(), _ => SelectedTab != null);
            UrlDecodeCommand = new RelayCommand(_ => UrlDecode(), _ => SelectedTab != null);
            OpenRecentFileCommand = new RelayCommand(param => OpenRecentFile(param as string));
            ClearRecentFilesCommand = new RelayCommand(_ => ClearRecentFiles());
        }

        #region Command Implementations

        private void NewFile()
        {
            var newTab = new TabItem
            {
                Header = $"Untitled {Tabs.Count + 1}",
                JsonText = string.Empty
            };

            Tabs.Add(newTab);
            SelectedTab = newTab;
        }

        private void OpenFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Open JSON File"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var content = _fileService.ReadFile(dialog.FileName);
                    var newTab = new TabItem
                    {
                        JsonText = content,
                        FilePath = dialog.FileName,
                        IsDirty = false
                    };

                    Tabs.Add(newTab);
                    SelectedTab = newTab;

                    ValidateJson();
                    BuildTree();
                    BuildTable();
                    LoadRecentFiles();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Open Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveFile()
        {
            if (SelectedTab == null)
                return;

            if (string.IsNullOrEmpty(SelectedTab.FilePath))
            {
                SaveFileAs();
                return;
            }

            try
            {
                _fileService.WriteFile(SelectedTab.FilePath, SelectedTab.JsonText);
                SelectedTab.IsDirty = false;
                SelectedTab.StatusMessage = $"Saved: {SelectedTab.FilePath}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveFileAs()
        {
            if (SelectedTab == null)
                return;

            var dialog = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Save JSON File",
                DefaultExt = ".json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _fileService.WriteFile(dialog.FileName, SelectedTab.JsonText);
                    SelectedTab.FilePath = dialog.FileName;
                    SelectedTab.IsDirty = false;
                    SelectedTab.StatusMessage = $"Saved: {dialog.FileName}";
                    LoadRecentFiles();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseTab(TabItem? tab)
        {
            if (tab == null)
                return;

            if (tab.IsDirty)
            {
                var result = MessageBox.Show(
                    $"Do you want to save changes to '{tab.Header}'?",
                    "Unsaved Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    SelectedTab = tab;
                    SaveFile();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            Tabs.Remove(tab);

            if (Tabs.Count == 0)
            {
                NewFile();
            }
        }

        private void FormatJson()
        {
            if (SelectedTab == null)
                return;

            try
            {
                SelectedTab.JsonText = _jsonService.FormatJson(SelectedTab.JsonText, IndentSize);
                SelectedTab.StatusMessage = "JSON formatted successfully";
                BuildTree();
                BuildTable();
            }
            catch (Exception ex)
            {
                SelectedTab.StatusMessage = $"Format error: {ex.Message}";
                MessageBox.Show(ex.Message, "Format Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MinifyJson()
        {
            if (SelectedTab == null)
                return;

            try
            {
                SelectedTab.JsonText = _jsonService.MinifyJson(SelectedTab.JsonText);
                SelectedTab.StatusMessage = "JSON minified successfully";
            }
            catch (Exception ex)
            {
                SelectedTab.StatusMessage = $"Minify error: {ex.Message}";
                MessageBox.Show(ex.Message, "Minify Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ValidateJson()
        {
            if (SelectedTab == null)
                return;

            var result = _jsonService.ValidateJson(SelectedTab.JsonText);
            SelectedTab.IsValid = result.IsValid;

            if (result.IsValid)
            {
                SelectedTab.StatusMessage = "✓ Valid JSON";
            }
            else
            {
                SelectedTab.StatusMessage = $"✗ Invalid JSON: {result.ErrorMessage} (Line {result.LineNumber}, Col {result.ColumnNumber})";
            }
        }

        private void CopyToClipboard()
        {
            if (SelectedTab == null)
                return;

            try
            {
                Clipboard.SetText(SelectedTab.JsonText);
                SelectedTab.StatusMessage = "Copied to clipboard";
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
                    if (SelectedTab == null)
                    {
                        NewFile();
                    }

                    if (SelectedTab != null)
                    {
                        SelectedTab.JsonText = Clipboard.GetText();
                        SelectedTab.StatusMessage = "Pasted from clipboard";
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

        public void BuildTree()
        {
            if (SelectedTab == null)
                return;

            try
            {
                SelectedTab.TreeNodes = _treeService.BuildTree(SelectedTab.JsonText);
            }
            catch (Exception ex)
            {
                SelectedTab.StatusMessage = $"Tree build error: {ex.Message}";
            }
        }

        public void BuildTable()
        {
            if (SelectedTab == null)
                return;

            try
            {
                SelectedTab.TableData = _tableService.ConvertToTable(SelectedTab.JsonText);
            }
            catch (Exception ex)
            {
                SelectedTab.StatusMessage = $"Table build error: {ex.Message}";
            }
        }

        private void ConvertToXml()
        {
            if (SelectedTab == null)
                return;

            try
            {
                var xml = _conversionService.JsonToXml(SelectedTab.JsonText);
                ShowConversionResult("XML", xml);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Conversion Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConvertToCSharp()
        {
            if (SelectedTab == null)
                return;

            try
            {
                var csharp = _conversionService.JsonToCSharpClasses(SelectedTab.JsonText);
                ShowConversionResult("C# Classes", csharp);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Conversion Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConvertToSql()
        {
            if (SelectedTab == null)
                return;

            try
            {
                var sql = _conversionService.JsonToSql(SelectedTab.JsonText);
                ShowConversionResult("SQL", sql);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Conversion Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConvertToYaml()
        {
            if (SelectedTab == null)
                return;

            try
            {
                var yaml = _conversionService.JsonToYaml(SelectedTab.JsonText);
                ShowConversionResult("YAML", yaml);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Conversion Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleTheme()
        {
            var currentTheme = JsonFormatterApp.Properties.Settings.Default.Theme;
            var newTheme = currentTheme == "Dark" ? "Light" : "Dark";
            App.ApplyTheme(newTheme);

            if (SelectedTab != null)
            {
                SelectedTab.StatusMessage = $"Switched to {newTheme} theme";
            }
        }

        private void ValidateSchema()
        {
            if (SelectedTab == null)
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
                    var result = _schemaValidationService.ValidateAgainstSchema(SelectedTab.JsonText, schemaText);

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

        private void CompareJson()
        {
            if (SelectedTab == null)
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
                        SelectedTab.Header,
                        SelectedTab.JsonText,
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
            if (Tabs.Count < 2)
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
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            // Apply current theme resources to the dialog
            selectionWindow.Resources = Application.Current.Resources;

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
                ItemsSource = Tabs,
                DisplayMemberPath = "Header",
                SelectedIndex = SelectedTab != null ? Tabs.IndexOf(SelectedTab) : 0,
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
                ItemsSource = Tabs,
                DisplayMemberPath = "Header",
                SelectedIndex = Tabs.Count > 1 ? (SelectedTab != null && Tabs.IndexOf(SelectedTab) == 0 ? 1 : 0) : 0,
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

            cancelButton.Click += (s, e) =>
            {
                selectionWindow.Close();
            };

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

        private void Base64Encode()
        {
            if (SelectedTab == null)
                return;

            try
            {
                var encoded = _encodingService.EncodeBase64(SelectedTab.JsonText);
                ShowConversionResult("Base64 Encoded", encoded);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Encoding Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Base64Decode()
        {
            if (SelectedTab == null)
                return;

            try
            {
                var decoded = _encodingService.DecodeBase64(SelectedTab.JsonText);
                SelectedTab.JsonText = decoded;
                SelectedTab.StatusMessage = "Base64 decoded";
                ValidateJson();
                BuildTree();
                BuildTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Decoding Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UrlEncode()
        {
            if (SelectedTab == null)
                return;

            try
            {
                var encoded = _encodingService.UrlEncode(SelectedTab.JsonText);
                ShowConversionResult("URL Encoded", encoded);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Encoding Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UrlDecode()
        {
            if (SelectedTab == null)
                return;

            try
            {
                var decoded = _encodingService.UrlDecode(SelectedTab.JsonText);
                SelectedTab.JsonText = decoded;
                SelectedTab.StatusMessage = "URL decoded";
                ValidateJson();
                BuildTree();
                BuildTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Decoding Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenRecentFile(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            try
            {
                var content = _fileService.ReadFile(filePath);
                var newTab = new TabItem
                {
                    JsonText = content,
                    FilePath = filePath,
                    IsDirty = false
                };

                Tabs.Add(newTab);
                SelectedTab = newTab;

                ValidateJson();
                BuildTree();
                BuildTable();
                LoadRecentFiles();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Open Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearRecentFiles()
        {
            _fileService.ClearRecentFiles();
            LoadRecentFiles();
        }

        #endregion

        #region Helper Methods

        private void LoadRecentFiles()
        {
            RecentFiles = new ObservableCollection<RecentFile>(_fileService.GetRecentFiles());
        }

        private void ShowConversionResult(string title, string content)
        {
            var window = new Window
            {
                Title = title,
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var textBox = new System.Windows.Controls.TextBox
            {
                Text = content,
                IsReadOnly = true,
                VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 12,
                Padding = new Thickness(10)
            };

            window.Content = textBox;
            window.ShowDialog();
        }

        #endregion
    }
}
