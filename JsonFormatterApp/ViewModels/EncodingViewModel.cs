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
    /// ViewModel responsible for encoding/decoding operations (Base64, URL)
    /// </summary>
    public class EncodingViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly EncodingService _encodingService;
        private readonly DocumentModel _document;
        private readonly JsonOperationsViewModel _jsonOperations;

        public EncodingViewModel(
            IEventAggregator eventAggregator,
            EncodingService encodingService,
            DocumentModel document,
            JsonOperationsViewModel jsonOperations)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _encodingService = encodingService ?? throw new ArgumentNullException(nameof(encodingService));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _jsonOperations = jsonOperations ?? throw new ArgumentNullException(nameof(jsonOperations));

            InitializeCommands();
        }

        #region Commands

        public ICommand Base64EncodeCommand { get; private set; } = null!;
        public ICommand Base64DecodeCommand { get; private set; } = null!;
        public ICommand UrlEncodeCommand { get; private set; } = null!;
        public ICommand UrlDecodeCommand { get; private set; } = null!;

        #endregion

        private void InitializeCommands()
        {
            Base64EncodeCommand = new RelayCommand(_ => Base64Encode());
            Base64DecodeCommand = new RelayCommand(_ => Base64Decode());
            UrlEncodeCommand = new RelayCommand(_ => UrlEncode());
            UrlDecodeCommand = new RelayCommand(_ => UrlDecode());
        }

        #region Encoding Operations

        private void Base64Encode()
        {
            try
            {
                var encoded = _encodingService.EncodeBase64(_document.JsonText);
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
                var decoded = _encodingService.DecodeBase64(_document.JsonText);
                _document.JsonText = decoded;
                UpdateStatus("Base64 decoded");
                _jsonOperations.ValidateJson();
                _jsonOperations.BuildTree();
                _jsonOperations.BuildTable();
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
                var encoded = _encodingService.UrlEncode(_document.JsonText);
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
                var decoded = _encodingService.UrlDecode(_document.JsonText);
                _document.JsonText = decoded;
                UpdateStatus("URL decoded");
                _jsonOperations.ValidateJson();
                _jsonOperations.BuildTree();
                _jsonOperations.BuildTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Decoding Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void UpdateStatus(string message)
        {
            _document.StatusMessage = message;
            _eventAggregator.Publish(new StatusUpdateMessage { Message = message });
        }

        #endregion
    }
}
