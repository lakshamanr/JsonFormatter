using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using JsonFormatterApp.Helpers;
using JsonFormatterApp.Models;
using JsonFormatterApp.Services;
using Microsoft.Win32;

namespace JsonFormatterApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly JsonService _jsonService;
        private readonly JsonTreeService _treeService;
        private readonly ConversionService _conversionService;
        private readonly FileService _fileService;
        private readonly SchemaValidationService _schemaValidationService;
        private readonly DiffService _diffService;
        private readonly EncodingService _encodingService;

        private string _jsonText = string.Empty;
        private string _statusMessage = "Ready";
        private bool _isValid = true;
        private string _currentFilePath = string.Empty;
        private bool _isDirty = false;
        private int _indentSize = 2;
        private ObservableCollection<JsonTreeNode> _treeNodes = new();
        private ObservableCollection<RecentFile> _recentFiles = new();

        public MainViewModel()
        {
            _jsonService = new JsonService();
            _treeService = new JsonTreeService();
            _conversionService = new ConversionService();
            _fileService = new FileService();
            _schemaValidationService = new SchemaValidationService();
            _diffService = new DiffService();
            _encodingService = new EncodingService();

            InitializeCommands();
            LoadRecentFiles();
        }

        #region Properties

        public string JsonText
        {
            get => _jsonText;
            set
            {
                if (SetProperty(ref _jsonText, value))
                {
                    IsDirty = true;
                    ValidateJsonCommand.Execute(null);
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsValid
        {
            get => _isValid;
            set => SetProperty(ref _isValid, value);
        }

        public string CurrentFilePath
        {
            get => _currentFilePath;
            set => SetProperty(ref _currentFilePath, value);
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => SetProperty(ref _isDirty, value);
        }

        public int IndentSize
        {
            get => _indentSize;
            set => SetProperty(ref _indentSize, value);
        }

        public ObservableCollection<JsonTreeNode> TreeNodes
        {
            get => _treeNodes;
            set => SetProperty(ref _treeNodes, value);
        }

        public ObservableCollection<RecentFile> RecentFiles
        {
            get => _recentFiles;
            set => SetProperty(ref _recentFiles, value);
        }

        #endregion

        #region Commands

        public ICommand FormatJsonCommand { get; private set; } = null!;
        public ICommand MinifyJsonCommand { get; private set; } = null!;
        public ICommand ValidateJsonCommand { get; private set; } = null!;
        public ICommand OpenFileCommand { get; private set; } = null!;
        public ICommand SaveFileCommand { get; private set; } = null!;
        public ICommand SaveAsCommand { get; private set; } = null!;
        public ICommand NewFileCommand { get; private set; } = null!;
        public ICommand CopyCommand { get; private set; } = null!;
        public ICommand PasteCommand { get; private set; } = null!;
        public ICommand BuildTreeCommand { get; private set; } = null!;
        public ICommand ConvertToXmlCommand { get; private set; } = null!;
        public ICommand ConvertToCSharpCommand { get; private set; } = null!;
        public ICommand ConvertToSqlCommand { get; private set; } = null!;
        public ICommand ConvertToYamlCommand { get; private set; } = null!;
        public ICommand ToggleThemeCommand { get; private set; } = null!;
        public ICommand ValidateSchemaCommand { get; private set; } = null!;
        public ICommand CompareJsonCommand { get; private set; } = null!;
        public ICommand Base64EncodeCommand { get; private set; } = null!;
        public ICommand Base64DecodeCommand { get; private set; } = null!;
        public ICommand UrlEncodeCommand { get; private set; } = null!;
        public ICommand UrlDecodeCommand { get; private set; } = null!;
        public ICommand OpenRecentFileCommand { get; private set; } = null!;
        public ICommand ClearRecentFilesCommand { get; private set; } = null!;

        #endregion

        private void InitializeCommands()
        {
            FormatJsonCommand = new RelayCommand(_ => FormatJson());
            MinifyJsonCommand = new RelayCommand(_ => MinifyJson());
            ValidateJsonCommand = new RelayCommand(_ => ValidateJson());
            OpenFileCommand = new RelayCommand(_ => OpenFile());
            SaveFileCommand = new RelayCommand(_ => SaveFile(), _ => !string.IsNullOrEmpty(CurrentFilePath));
            SaveAsCommand = new RelayCommand(_ => SaveFileAs());
            NewFileCommand = new RelayCommand(_ => NewFile());
            CopyCommand = new RelayCommand(_ => CopyToClipboard());
            PasteCommand = new RelayCommand(_ => PasteFromClipboard());
            BuildTreeCommand = new RelayCommand(_ => BuildTree());
            ConvertToXmlCommand = new RelayCommand(_ => ConvertToXml());
            ConvertToCSharpCommand = new RelayCommand(_ => ConvertToCSharp());
            ConvertToSqlCommand = new RelayCommand(_ => ConvertToSql());
            ConvertToYamlCommand = new RelayCommand(_ => ConvertToYaml());
            ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());
            ValidateSchemaCommand = new RelayCommand(_ => ValidateSchema());
            CompareJsonCommand = new RelayCommand(_ => CompareJson());
            Base64EncodeCommand = new RelayCommand(_ => Base64Encode());
            Base64DecodeCommand = new RelayCommand(_ => Base64Decode());
            UrlEncodeCommand = new RelayCommand(_ => UrlEncode());
            UrlDecodeCommand = new RelayCommand(_ => UrlDecode());
            OpenRecentFileCommand = new RelayCommand(param => OpenRecentFile(param as string));
            ClearRecentFilesCommand = new RelayCommand(_ => ClearRecentFiles());
        }

        #region Command Implementations

        private void FormatJson()
        {
            try
            {
                JsonText = _jsonService.FormatJson(JsonText, IndentSize);
                StatusMessage = "JSON formatted successfully";
                BuildTree();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Format error: {ex.Message}";
                MessageBox.Show(ex.Message, "Format Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MinifyJson()
        {
            try
            {
                JsonText = _jsonService.MinifyJson(JsonText);
                StatusMessage = "JSON minified successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Minify error: {ex.Message}";
                MessageBox.Show(ex.Message, "Minify Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ValidateJson()
        {
            var result = _jsonService.ValidateJson(JsonText);
            IsValid = result.IsValid;

            if (result.IsValid)
            {
                StatusMessage = "✓ Valid JSON";
            }
            else
            {
                StatusMessage = $"✗ Invalid JSON: {result.ErrorMessage} (Line {result.LineNumber}, Col {result.ColumnNumber})";
            }
        }

        private void OpenFile()
        {
            if (IsDirty && !ConfirmUnsavedChanges())
                return;

            var dialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Open JSON File"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    JsonText = _fileService.ReadFile(dialog.FileName);
                    CurrentFilePath = dialog.FileName;
                    IsDirty = false;
                    StatusMessage = $"Opened: {dialog.FileName}";
                    BuildTree();
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
            if (string.IsNullOrEmpty(CurrentFilePath))
            {
                SaveFileAs();
                return;
            }

            try
            {
                _fileService.WriteFile(CurrentFilePath, JsonText);
                IsDirty = false;
                StatusMessage = $"Saved: {CurrentFilePath}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveFileAs()
        {
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
                    _fileService.WriteFile(dialog.FileName, JsonText);
                    CurrentFilePath = dialog.FileName;
                    IsDirty = false;
                    StatusMessage = $"Saved: {dialog.FileName}";
                    LoadRecentFiles();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void NewFile()
        {
            if (IsDirty && !ConfirmUnsavedChanges())
                return;

            JsonText = string.Empty;
            CurrentFilePath = string.Empty;
            IsDirty = false;
            TreeNodes.Clear();
            StatusMessage = "New file";
        }

        private void CopyToClipboard()
        {
            try
            {
                Clipboard.SetText(JsonText);
                StatusMessage = "Copied to clipboard";
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
                    JsonText = Clipboard.GetText();
                    StatusMessage = "Pasted from clipboard";
                    BuildTree();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Paste Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BuildTree()
        {
            try
            {
                TreeNodes = _treeService.BuildTree(JsonText);
                StatusMessage = "Tree view updated";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Tree build error: {ex.Message}";
            }
        }

        private void ConvertToXml()
        {
            try
            {
                var xml = _conversionService.JsonToXml(JsonText);
                ShowConversionResult("XML", xml);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Conversion Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConvertToCSharp()
        {
            try
            {
                var csharp = _conversionService.JsonToCSharpClasses(JsonText);
                ShowConversionResult("C# Classes", csharp);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Conversion Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConvertToSql()
        {
            try
            {
                var sql = _conversionService.JsonToSql(JsonText);
                ShowConversionResult("SQL", sql);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Conversion Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConvertToYaml()
        {
            try
            {
                var yaml = _conversionService.JsonToYaml(JsonText);
                ShowConversionResult("YAML", yaml);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Conversion Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleTheme()
        {
            var currentTheme = Properties.Settings.Default.Theme;
            var newTheme = currentTheme == "Dark" ? "Light" : "Dark";
            App.ApplyTheme(newTheme);
            StatusMessage = $"Switched to {newTheme} theme";
        }

        private void ValidateSchema()
        {
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
                    var result = _schemaValidationService.ValidateAgainstSchema(JsonText, schemaText);

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
                    var differences = _diffService.GetSemanticDifferences(JsonText, compareJson);

                    if (differences.Count == 0)
                    {
                        MessageBox.Show("The JSON files are identical!", "Comparison Result", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var diff = string.Join("\n", differences);
                        ShowConversionResult("JSON Comparison", diff);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Comparison Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Base64Encode()
        {
            try
            {
                var encoded = _encodingService.EncodeBase64(JsonText);
                ShowConversionResult("Base64 Encoded", encoded);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Encoding Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Base64Decode()
        {
            try
            {
                var decoded = _encodingService.DecodeBase64(JsonText);
                JsonText = decoded;
                StatusMessage = "Base64 decoded";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Decoding Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UrlEncode()
        {
            try
            {
                var encoded = _encodingService.UrlEncode(JsonText);
                ShowConversionResult("URL Encoded", encoded);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Encoding Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UrlDecode()
        {
            try
            {
                var decoded = _encodingService.UrlDecode(JsonText);
                JsonText = decoded;
                StatusMessage = "URL decoded";
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

            if (IsDirty && !ConfirmUnsavedChanges())
                return;

            try
            {
                JsonText = _fileService.ReadFile(filePath);
                CurrentFilePath = filePath;
                IsDirty = false;
                StatusMessage = $"Opened: {filePath}";
                BuildTree();
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
            StatusMessage = "Recent files cleared";
        }

        #endregion

        #region Helper Methods

        private void LoadRecentFiles()
        {
            RecentFiles = new ObservableCollection<RecentFile>(_fileService.GetRecentFiles());
        }

        private bool ConfirmUnsavedChanges()
        {
            var result = MessageBox.Show(
                "You have unsaved changes. Do you want to continue without saving?",
                "Unsaved Changes",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            return result == MessageBoxResult.Yes;
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
