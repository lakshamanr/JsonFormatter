using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using JsonFormatterApp.Helpers;
using JsonFormatterApp.Infrastructure;
using JsonFormatterApp.Models;
using JsonFormatterApp.Services;
using Microsoft.Win32;

namespace JsonFormatterApp.ViewModels
{
    /// <summary>
    /// ViewModel responsible for file operations and recent files management
    /// </summary>
    public class FileOperationsViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly FileService _fileService;
        private readonly DocumentModel _document;
        private ObservableCollection<RecentFile> _recentFiles = new();

        public FileOperationsViewModel(
            IEventAggregator eventAggregator,
            FileService fileService,
            DocumentModel document)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _document = document ?? throw new ArgumentNullException(nameof(document));

            InitializeCommands();
            LoadRecentFiles();
        }

        #region Properties

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
        public ICommand OpenRecentFileCommand { get; private set; } = null!;
        public ICommand ClearRecentFilesCommand { get; private set; } = null!;

        #endregion

        private void InitializeCommands()
        {
            NewFileCommand = new RelayCommand(_ => NewFile(), _ => CanCreateNew());
            OpenFileCommand = new RelayCommand(_ => OpenFile());
            SaveFileCommand = new RelayCommand(_ => SaveFile(), _ => CanSave());
            SaveAsCommand = new RelayCommand(_ => SaveFileAs());
            OpenRecentFileCommand = new RelayCommand(param => OpenRecentFile(param as string));
            ClearRecentFilesCommand = new RelayCommand(_ => ClearRecentFiles());
        }

        #region File Operations

        private bool CanCreateNew()
        {
            // Allow creating new file if there are unsaved changes
            return _document.IsDirty;
        }

        private bool CanSave()
        {
            return !string.IsNullOrEmpty(_document.FilePath);
        }

        public void NewFile()
        {
            // Check for unsaved changes
            if (_document.IsDirty)
            {
                var result = MessageBox.Show(
                    "Do you want to save changes to the current document?",
                    "Unsaved Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveFile();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            // Clear the document
            _document.SetJsonTextWithoutDirty(string.Empty);
            _document.FilePath = string.Empty;
            _document.IsDirty = false;
            _document.TreeNodes.Clear();
            _document.TableData = null;
            UpdateStatus("New document created");

            // Notify that views should be refreshed
            _eventAggregator.Publish(new RefreshViewsMessage());
        }

        public void OpenFile()
        {
            // Check for unsaved changes
            if (_document.IsDirty)
            {
                var result = MessageBox.Show(
                    "Do you want to save changes to the current document?",
                    "Unsaved Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveFile();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

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
                    _document.SetJsonTextWithoutDirty(content);
                    _document.FilePath = dialog.FileName;
                    _document.IsDirty = false;

                    // Notify that views should be refreshed
                    _eventAggregator.Publish(new RefreshViewsMessage());

                    UpdateStatus($"Opened: {dialog.FileName}");
                    LoadRecentFiles();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Open Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void SaveFile()
        {
            if (string.IsNullOrEmpty(_document.FilePath))
            {
                SaveFileAs();
                return;
            }

            try
            {
                _fileService.WriteFile(_document.FilePath, _document.JsonText);
                _document.IsDirty = false;
                UpdateStatus($"Saved: {_document.FilePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SaveFileAs()
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
                    _fileService.WriteFile(dialog.FileName, _document.JsonText);
                    _document.FilePath = dialog.FileName;
                    _document.IsDirty = false;
                    UpdateStatus($"Saved: {dialog.FileName}");
                    LoadRecentFiles();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenRecentFile(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            // Check for unsaved changes
            if (_document.IsDirty)
            {
                var result = MessageBox.Show(
                    "Do you want to save changes to the current document?",
                    "Unsaved Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveFile();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            try
            {
                var content = _fileService.ReadFile(filePath);
                _document.SetJsonTextWithoutDirty(content);
                _document.FilePath = filePath;
                _document.IsDirty = false;

                // Notify that views should be refreshed
                _eventAggregator.Publish(new RefreshViewsMessage());

                UpdateStatus($"Opened: {filePath}");
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

        private void LoadRecentFiles()
        {
            RecentFiles = new ObservableCollection<RecentFile>(_fileService.GetRecentFiles());
        }

        #endregion

        #region Helper Methods

        private void UpdateStatus(string message)
        {
            _document.StatusMessage = message;
            _eventAggregator.Publish(new StatusUpdateMessage { Message = message });
        }

        #endregion
    }
}
