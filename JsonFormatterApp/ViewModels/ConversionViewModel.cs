using System;
using System.Windows;
using System.Windows.Input;
using JsonFormatterApp.Helpers;
using JsonFormatterApp.Infrastructure;
using JsonFormatterApp.Models;
using JsonFormatterApp.Services;

namespace JsonFormatterApp.ViewModels
{
    /// <summary>
    /// ViewModel responsible for format conversion operations (XML, C#, SQL, YAML)
    /// </summary>
    public class ConversionViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ConversionService _conversionService;
        private readonly TabManagerViewModel _tabManager;

        public ConversionViewModel(
            IEventAggregator eventAggregator,
            ConversionService conversionService,
            TabManagerViewModel tabManager)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _conversionService = conversionService ?? throw new ArgumentNullException(nameof(conversionService));
            _tabManager = tabManager ?? throw new ArgumentNullException(nameof(tabManager));

            InitializeCommands();

            // Subscribe to events
            _eventAggregator.Subscribe<TabSelectedMessage>(OnTabSelected);
        }

        #region Properties

        private TabItem? CurrentTab => _tabManager.SelectedTab;

        #endregion

        #region Commands

        public ICommand ConvertToXmlCommand { get; private set; } = null!;
        public ICommand ConvertToCSharpCommand { get; private set; } = null!;
        public ICommand ConvertToSqlCommand { get; private set; } = null!;
        public ICommand ConvertToYamlCommand { get; private set; } = null!;

        #endregion

        private void InitializeCommands()
        {
            ConvertToXmlCommand = new RelayCommand(_ => ConvertToXml(), _ => CurrentTab != null);
            ConvertToCSharpCommand = new RelayCommand(_ => ConvertToCSharp(), _ => CurrentTab != null);
            ConvertToSqlCommand = new RelayCommand(_ => ConvertToSql(), _ => CurrentTab != null);
            ConvertToYamlCommand = new RelayCommand(_ => ConvertToYaml(), _ => CurrentTab != null);
        }

        #region Event Handlers

        private void OnTabSelected(TabSelectedMessage message)
        {
            // Refresh command can execute states
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion

        #region Conversion Operations

        private void ConvertToXml()
        {
            if (CurrentTab == null)
                return;

            try
            {
                var xml = _conversionService.JsonToXml(CurrentTab.JsonText);
                ShowConversionResult("XML", xml);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Conversion Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConvertToCSharp()
        {
            if (CurrentTab == null)
                return;

            try
            {
                var csharp = _conversionService.JsonToCSharpClasses(CurrentTab.JsonText);
                ShowConversionResult("C# Classes", csharp);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Conversion Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConvertToSql()
        {
            if (CurrentTab == null)
                return;

            try
            {
                var sql = _conversionService.JsonToSql(CurrentTab.JsonText);
                ShowConversionResult("SQL", sql);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Conversion Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConvertToYaml()
        {
            if (CurrentTab == null)
                return;

            try
            {
                var yaml = _conversionService.JsonToYaml(CurrentTab.JsonText);
                ShowConversionResult("YAML", yaml);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Conversion Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Helper Methods

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
