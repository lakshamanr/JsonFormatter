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
        private readonly DocumentModel _document;

        public ConversionViewModel(
            IEventAggregator eventAggregator,
            ConversionService conversionService,
            DocumentModel document)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _conversionService = conversionService ?? throw new ArgumentNullException(nameof(conversionService));
            _document = document ?? throw new ArgumentNullException(nameof(document));

            InitializeCommands();
        }

        #region Commands

        public ICommand ConvertToXmlCommand { get; private set; } = null!;
        public ICommand ConvertToCSharpCommand { get; private set; } = null!;
        public ICommand ConvertToSqlCommand { get; private set; } = null!;
        public ICommand ConvertToYamlCommand { get; private set; } = null!;

        #endregion

        private void InitializeCommands()
        {
            ConvertToXmlCommand = new RelayCommand(_ => ConvertToXml());
            ConvertToCSharpCommand = new RelayCommand(_ => ConvertToCSharp());
            ConvertToSqlCommand = new RelayCommand(_ => ConvertToSql());
            ConvertToYamlCommand = new RelayCommand(_ => ConvertToYaml());
        }

        #region Conversion Operations

        private void ConvertToXml()
        {
            try
            {
                var xml = _conversionService.JsonToXml(_document.JsonText);
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
                var csharp = _conversionService.JsonToCSharpClasses(_document.JsonText);
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
                var sql = _conversionService.JsonToSql(_document.JsonText);
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
                var yaml = _conversionService.JsonToYaml(_document.JsonText);
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
