using System.Collections.ObjectModel;
using System.Windows.Input;
using JsonFormatterApp.Helpers;
using JsonFormatterApp.Infrastructure;
using JsonFormatterApp.Models;
using JsonFormatterApp.Services;

namespace JsonFormatterApp.ViewModels
{
    /// <summary>
    /// Main ViewModel - Orchestrates child ViewModels and exposes properties for XAML binding
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;

        // Single document model
        public DocumentModel Document { get; }

        // Child ViewModels
        public FileOperationsViewModel FileOperations { get; }
        public JsonOperationsViewModel JsonOperations { get; }
        public ConversionViewModel Conversions { get; }
        public EncodingViewModel Encoding { get; }

        public MainViewModel()
        {
            // Create event aggregator
            _eventAggregator = new EventAggregator();

            // Create single document instance
            Document = new DocumentModel();

            // Create services (will be replaced with DI later)
            var jsonService = new JsonService();
            var treeService = new JsonTreeService();
            var tableService = new JsonTableService();
            var conversionService = new ConversionService();
            var fileService = new FileService();
            var schemaValidationService = new SchemaValidationService();
            var diffService = new DiffService();
            var encodingService = new EncodingService();

            // Create child ViewModels
            FileOperations = new FileOperationsViewModel(_eventAggregator, fileService, Document);
            JsonOperations = new JsonOperationsViewModel(
                _eventAggregator,
                jsonService,
                treeService,
                tableService,
                diffService,
                schemaValidationService,
                fileService,
                Document);
            Conversions = new ConversionViewModel(_eventAggregator, conversionService, Document);
            Encoding = new EncodingViewModel(_eventAggregator, encodingService, Document, JsonOperations);

            // Initialize theme command
            ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());
        }

        #region Properties for XAML Binding

        public int IndentSize
        {
            get => JsonOperations.IndentSize;
            set => JsonOperations.IndentSize = value;
        }

        public ObservableCollection<RecentFile> RecentFiles => FileOperations.RecentFiles;

        #endregion

        #region Commands - Delegated to Child ViewModels

        // File Operations
        public ICommand NewFileCommand => FileOperations.NewFileCommand;
        public ICommand OpenFileCommand => FileOperations.OpenFileCommand;
        public ICommand SaveFileCommand => FileOperations.SaveFileCommand;
        public ICommand SaveAsCommand => FileOperations.SaveAsCommand;
        public ICommand OpenRecentFileCommand => FileOperations.OpenRecentFileCommand;
        public ICommand ClearRecentFilesCommand => FileOperations.ClearRecentFilesCommand;

        // JSON Operations
        public ICommand FormatJsonCommand => JsonOperations.FormatJsonCommand;
        public ICommand MinifyJsonCommand => JsonOperations.MinifyJsonCommand;
        public ICommand ValidateJsonCommand => JsonOperations.ValidateJsonCommand;
        public ICommand BuildTreeCommand => JsonOperations.BuildTreeCommand;
        public ICommand BuildTableCommand => JsonOperations.BuildTableCommand;
        public ICommand CompareJsonCommand => JsonOperations.CompareJsonCommand;
        public ICommand ValidateSchemaCommand => JsonOperations.ValidateSchemaCommand;
        public ICommand CopyCommand => JsonOperations.CopyCommand;
        public ICommand PasteCommand => JsonOperations.PasteCommand;
        public ICommand ExportTableToCsvCommand => JsonOperations.ExportTableToCsvCommand;
        public ICommand ExportTableToHtmlCommand => JsonOperations.ExportTableToHtmlCommand;
        public ICommand ExportTableToJsonCommand => JsonOperations.ExportTableToJsonCommand;

        // Conversion Operations
        public ICommand ConvertToXmlCommand => Conversions.ConvertToXmlCommand;
        public ICommand ConvertToCSharpCommand => Conversions.ConvertToCSharpCommand;
        public ICommand ConvertToSqlCommand => Conversions.ConvertToSqlCommand;
        public ICommand ConvertToYamlCommand => Conversions.ConvertToYamlCommand;

        // Encoding Operations
        public ICommand Base64EncodeCommand => Encoding.Base64EncodeCommand;
        public ICommand Base64DecodeCommand => Encoding.Base64DecodeCommand;
        public ICommand UrlEncodeCommand => Encoding.UrlEncodeCommand;
        public ICommand UrlDecodeCommand => Encoding.UrlDecodeCommand;

        // Theme Management (still handled here as it's app-level)
        public ICommand ToggleThemeCommand { get; }

        #endregion

        #region Theme Management

        private void ToggleTheme()
        {
            var currentTheme = JsonFormatterApp.Properties.Settings.Default.Theme;
            var newTheme = currentTheme == "Dark" ? "Light" : "Dark";
            App.ApplyTheme(newTheme);

            Document.StatusMessage = $"Switched to {newTheme} theme";

            _eventAggregator.Publish(new ThemeChangedMessage { ThemeName = newTheme });
        }

        #endregion
    }
}
