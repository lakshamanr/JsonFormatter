using System;
using System.Linq;
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
        private readonly TableExportService _tableExportService;
        private readonly DocumentModel _document;
        private int _indentSize = 2;

        public JsonOperationsViewModel(
            IEventAggregator eventAggregator,
            JsonService jsonService,
            JsonTreeService treeService,
            JsonTableService tableService,
            DiffService diffService,
            SchemaValidationService schemaValidationService,
            FileService fileService,
            DocumentModel document)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _jsonService = jsonService ?? throw new ArgumentNullException(nameof(jsonService));
            _treeService = treeService ?? throw new ArgumentNullException(nameof(treeService));
            _tableService = tableService ?? throw new ArgumentNullException(nameof(tableService));
            _diffService = diffService ?? throw new ArgumentNullException(nameof(diffService));
            _schemaValidationService = schemaValidationService ?? throw new ArgumentNullException(nameof(schemaValidationService));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _tableExportService = new TableExportService();
            _document = document ?? throw new ArgumentNullException(nameof(document));

            InitializeCommands();

            // Subscribe to events
            _eventAggregator.Subscribe<RefreshViewsMessage>(OnRefreshViews);
        }

        #region Properties

        public int IndentSize
        {
            get => _indentSize;
            set => SetProperty(ref _indentSize, value);
        }

        #endregion

        #region Commands

        public ICommand FormatJsonCommand { get; private set; } = null!;
        public ICommand MinifyJsonCommand { get; private set; } = null!;
        public ICommand ValidateJsonCommand { get; private set; } = null!;
        public ICommand BuildTreeCommand { get; private set; } = null!;
        public ICommand BuildTableCommand { get; private set; } = null!;
        public ICommand CompareJsonCommand { get; private set; } = null!;
        public ICommand ValidateSchemaCommand { get; private set; } = null!;
        public ICommand CopyCommand { get; private set; } = null!;
        public ICommand PasteCommand { get; private set; } = null!;
        public ICommand ExportTableToCsvCommand { get; private set; } = null!;
        public ICommand ExportTableToHtmlCommand { get; private set; } = null!;
        public ICommand ExportTableToJsonCommand { get; private set; } = null!;

        #endregion

        private void InitializeCommands()
        {
            FormatJsonCommand = new RelayCommand(_ => FormatJson(), _ => true);
            MinifyJsonCommand = new RelayCommand(_ => MinifyJson(), _ => true);
            ValidateJsonCommand = new RelayCommand(_ => ValidateJson(), _ => true);
            BuildTreeCommand = new RelayCommand(_ => BuildTree(), _ => true);
            BuildTableCommand = new RelayCommand(_ => BuildTable(), _ => true);
            CompareJsonCommand = new RelayCommand(_ => CompareJson(), _ => true);
            ValidateSchemaCommand = new RelayCommand(_ => ValidateSchema(), _ => true);
            CopyCommand = new RelayCommand(_ => CopyToClipboard(), _ => true);
            PasteCommand = new RelayCommand(_ => PasteFromClipboard());
            ExportTableToCsvCommand = new RelayCommand(_ => ExportTableToCsv(), _ => _document.SelectedTable != null);
            ExportTableToHtmlCommand = new RelayCommand(_ => ExportTableToHtml(), _ => _document.SelectedTable != null);
            ExportTableToJsonCommand = new RelayCommand(_ => ExportTableToJson(), _ => _document.SelectedTable != null);
        }

        #region Event Handlers

        private void OnRefreshViews(RefreshViewsMessage message)
        {
            ValidateJson();
            BuildTree();
            BuildTable();
        }

        #endregion

        #region JSON Operations

        private void FormatJson()
        {
            if (string.IsNullOrWhiteSpace(_document.JsonText))
            {
                UpdateStatus("No JSON content to format");
                MessageBox.Show("Please enter some JSON content first.", "No Content", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                _document.JsonText = _jsonService.FormatJson(_document.JsonText, IndentSize);
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
            if (string.IsNullOrWhiteSpace(_document.JsonText))
            {
                UpdateStatus("No JSON content to minify");
                MessageBox.Show("Please enter some JSON content first.", "No Content", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                _document.JsonText = _jsonService.MinifyJson(_document.JsonText);
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
            var result = _jsonService.ValidateJson(_document.JsonText);
            _document.IsValid = result.IsValid;

            if (result.IsValid)
            {
                _document.StatusMessage = "Valid JSON";
            }
            else
            {
                _document.StatusMessage = $"Invalid JSON: {result.ErrorMessage} (Line {result.LineNumber}, Col {result.ColumnNumber})";
            }
        }

        public void BuildTree()
        {
            try
            {
                _document.TreeNodes = _treeService.BuildTree(_document.JsonText);
                // Initialize filtered tree nodes
                _document.ApplyTreeFilter();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Tree build error: {ex.Message}");
            }
        }

        public void BuildTable()
        {
            try
            {
                _document.TableData = _tableService.ConvertToTable(_document.JsonText);
                BuildAllTables();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Table build error: {ex.Message}");
            }
        }

        public void BuildAllTables()
        {
            try
            {
                var tables = _tableService.ExtractAllTables(_document.JsonText);
                _document.AllTables.Clear();
                foreach (var table in tables)
                {
                    _document.AllTables.Add(table);
                }

                // Select the first table by default
                if (_document.AllTables.Any())
                {
                    _document.SelectedTable = _document.AllTables[0];
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"All tables build error: {ex.Message}");
            }
        }

        #endregion

        #region Comparison Operations

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
                    var compareWindow = new CompareWindow(
                        _document.GetDisplayTitle(),
                        _document.JsonText,
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

        #endregion

        #region Schema Validation

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
                    var result = _schemaValidationService.ValidateAgainstSchema(_document.JsonText, schemaText);

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
            try
            {
                Clipboard.SetText(_document.JsonText);
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
                    _document.JsonText = Clipboard.GetText();
                    UpdateStatus("Pasted from clipboard");
                    ValidateJson();
                    BuildTree();
                    BuildTable();
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
            _document.StatusMessage = message;
            _eventAggregator.Publish(new StatusUpdateMessage { Message = message });
        }

        #endregion

        #region Table Export Operations

        private void ExportTableToCsv()
        {
            if (_document.SelectedTable == null)
            {
                MessageBox.Show("Please select a table to export.", "No Table Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                FileName = $"{_document.SelectedTable.Name}.csv",
                Title = "Export Table to CSV"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _tableExportService.ExportToCsv(_document.SelectedTable, dialog.FileName);
                    UpdateStatus($"Table exported to CSV: {dialog.FileName}");
                    MessageBox.Show($"Table successfully exported to:\n{dialog.FileName}", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export table:\n{ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportTableToHtml()
        {
            if (_document.SelectedTable == null)
            {
                MessageBox.Show("Please select a table to export.", "No Table Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "HTML Files (*.html)|*.html|All Files (*.*)|*.*",
                FileName = $"{_document.SelectedTable.Name}.html",
                Title = "Export Table to HTML"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _tableExportService.ExportToHtml(_document.SelectedTable, dialog.FileName);
                    UpdateStatus($"Table exported to HTML: {dialog.FileName}");
                    MessageBox.Show($"Table successfully exported to:\n{dialog.FileName}", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export table:\n{ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportTableToJson()
        {
            if (_document.SelectedTable == null)
            {
                MessageBox.Show("Please select a table to export.", "No Table Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                FileName = $"{_document.SelectedTable.Name}.json",
                Title = "Export Table to JSON"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _tableExportService.ExportToJson(_document.SelectedTable, dialog.FileName);
                    UpdateStatus($"Table exported to JSON: {dialog.FileName}");
                    MessageBox.Show($"Table successfully exported to:\n{dialog.FileName}", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export table:\n{ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion
    }
}
