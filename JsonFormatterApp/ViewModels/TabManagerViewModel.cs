using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using JsonFormatterApp.Helpers;
using JsonFormatterApp.Infrastructure;
using JsonFormatterApp.Models;

namespace JsonFormatterApp.ViewModels
{
    /// <summary>
    /// ViewModel responsible for tab lifecycle management
    /// </summary>
    public class TabManagerViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private ObservableCollection<TabItem> _tabs = new();
        private TabItem? _selectedTab;
        private static int _tabCounter = 0;

        public TabManagerViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            InitializeCommands();
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
            set
            {
                if (SetProperty(ref _selectedTab, value))
                {
                    // Notify other ViewModels that tab selection changed
                    _eventAggregator.Publish(new TabSelectedMessage { SelectedTab = value });
                }
            }
        }

        #endregion

        #region Commands

        public ICommand NewTabCommand { get; private set; } = null!;
        public ICommand CloseTabCommand { get; private set; } = null!;

        #endregion

        private void InitializeCommands()
        {
            NewTabCommand = new RelayCommand(_ => CreateNewTab());
            CloseTabCommand = new RelayCommand(param => CloseTab(param as TabItem));
        }

        #region Tab Management

        /// <summary>
        /// Create a new blank tab
        /// </summary>
        public TabItem CreateNewTab()
        {
            _tabCounter++;
            var newTab = new TabItem
            {
                Header = $"Untitled {_tabCounter}",
                JsonText = string.Empty
            };

            Tabs.Add(newTab);
            SelectedTab = newTab;

            return newTab;
        }

        /// <summary>
        /// Create a tab with specific content and file path
        /// </summary>
        public TabItem CreateTab(string content, string? filePath = null)
        {
            var newTab = new TabItem
            {
                FilePath = filePath ?? string.Empty
            };

            // Set content without marking as dirty
            newTab.SetJsonTextWithoutDirty(content);
            newTab.IsDirty = false;

            Tabs.Add(newTab);
            SelectedTab = newTab;

            return newTab;
        }

        /// <summary>
        /// Close a tab with unsaved changes prompt
        /// </summary>
        public bool CloseTab(TabItem? tab)
        {
            if (tab == null)
                return false;

            // If tab has unsaved changes, publish message to request save
            if (tab.IsDirty)
            {
                // This would be handled by FileOperationsViewModel
                // For now, we'll handle it inline to maintain compatibility
                var result = System.Windows.MessageBox.Show(
                    $"Do you want to save changes to '{tab.Header}'?",
                    "Unsaved Changes",
                    System.Windows.MessageBoxButton.YesNoCancel,
                    System.Windows.MessageBoxImage.Warning
                );

                if (result == System.Windows.MessageBoxResult.Cancel)
                {
                    return false;
                }

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    // Let FileOperationsViewModel handle save
                    SelectedTab = tab;
                    // Publish save request - will be handled by FileOperationsViewModel
                    return false; // Don't close yet, wait for save
                }
            }

            Tabs.Remove(tab);

            // Ensure at least one tab exists
            if (Tabs.Count == 0)
            {
                CreateNewTab();
            }

            return true;
        }

        /// <summary>
        /// Mark the current tab as dirty
        /// </summary>
        public void MarkCurrentTabDirty()
        {
            if (SelectedTab != null)
            {
                SelectedTab.IsDirty = true;
            }
        }

        #endregion
    }
}
