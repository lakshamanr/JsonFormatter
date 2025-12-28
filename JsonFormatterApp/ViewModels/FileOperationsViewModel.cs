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
        private readonly TabManagerViewModel _tabManager;
        private ObservableCollection<RecentFile> _recentFiles = new();

        public FileOperationsViewModel(
            IEventAggregator eventAggregator,
            FileService fileService,
            TabManagerViewModel tabManager)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _tabManager = tabManager ?? throw new ArgumentNullException(nameof(tabManager));

            InitializeCommands();
            LoadRecentFiles();

            // Subscribe to tab selection changes
            _eventAggregator.Subscribe<TabSelectedMessage>(OnTabSelected);
        }

        #region Properties

        public ObservableCollection<RecentFile> RecentFiles
        {
            get => _recentFiles;
            set => SetProperty(ref _recentFiles, value);
        }

        private TabItem? CurrentTab => _tabManager.SelectedTab;

        #endregion

        #region Commands

        public ICommand OpenFileCommand { get; private set; } = null!;
        public ICommand SaveFileCommand { get; private set; } = null!;
        public ICommand SaveAsCommand { get; private set; } = null!;
        public ICommand OpenRecentFileCommand { get; private set; } = null!;
        public ICommand ClearRecentFilesCommand { get; private set; } = null!;

        #endregion

        private void InitializeCommands()
        {
            OpenFileCommand = new RelayCommand(_ => OpenFile());
            SaveFileCommand = new RelayCommand(_ => SaveFile(), _ => CanSave());
            SaveAsCommand = new RelayCommand(_ => SaveFileAs(), _ => CurrentTab != null);
            OpenRecentFileCommand = new RelayCommand(param => OpenRecentFile(param as string));
            ClearRecentFilesCommand = new RelayCommand(_ => ClearRecentFiles());
        }

        #region Event Handlers

        private void OnTabSelected(TabSelectedMessage message)
        {
            // Refresh command can execute states
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion

        #region File Operations

        private bool CanSave()
        {
            return CurrentTab != null && !string.IsNullOrEmpty(CurrentTab.FilePath);
        }

        public void OpenFile()
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
                    var newTab = _tabManager.CreateTab(content, dialog.FileName);

                    // Notify that views should be refreshed
                    _eventAggregator.Publish(new RefreshViewsMessage { Tab = newTab });

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
            if (CurrentTab == null)
                return;

            if (string.IsNullOrEmpty(CurrentTab.FilePath))
            {
                SaveFileAs();
                return;
            }

            try
            {
                _fileService.WriteFile(CurrentTab.FilePath, CurrentTab.JsonText);
                CurrentTab.IsDirty = false;
                UpdateStatus($"Saved: {CurrentTab.FilePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SaveFileAs()
        {
            if (CurrentTab == null)
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
                    _fileService.WriteFile(dialog.FileName, CurrentTab.JsonText);
                    CurrentTab.FilePath = dialog.FileName;
                    CurrentTab.IsDirty = false;
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

            try
            {
                var content = _fileService.ReadFile(filePath);
                var newTab = _tabManager.CreateTab(content, filePath);

                // Notify that views should be refreshed
                _eventAggregator.Publish(new RefreshViewsMessage { Tab = newTab });

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
            if (CurrentTab != null)
            {
                CurrentTab.StatusMessage = message;
            }

            _eventAggregator.Publish(new StatusUpdateMessage { Message = message });
        }

        #endregion
    }
}
