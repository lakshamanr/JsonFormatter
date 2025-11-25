using System;
using System.Collections.ObjectModel;
using JsonFormatterApp.Helpers;
using JsonFormatterApp.Models;

namespace JsonFormatterApp.Models
{
    public class TabItem : ViewModelBase
    {
        private string _header = "Untitled";
        private string _jsonText = string.Empty;
        private string _filePath = string.Empty;
        private bool _isDirty = false;
        private bool _isValid = true;
        private ObservableCollection<JsonTreeNode> _treeNodes = new();
        private System.Data.DataTable? _tableData;
        private string _statusMessage = "Ready";
        private bool _isInitializing = false;

        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        public string JsonText
        {
            get => _jsonText;
            set
            {
                if (SetProperty(ref _jsonText, value))
                {
                    // Only mark as dirty if not during initialization
                    if (!_isInitializing)
                    {
                        IsDirty = true;
                    }
                }
            }
        }

        public string FilePath
        {
            get => _filePath;
            set
            {
                if (SetProperty(ref _filePath, value))
                {
                    UpdateHeader();
                }
            }
        }

        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (SetProperty(ref _isDirty, value))
                {
                    UpdateHeader();
                }
            }
        }

        public bool IsValid
        {
            get => _isValid;
            set => SetProperty(ref _isValid, value);
        }

        public ObservableCollection<JsonTreeNode> TreeNodes
        {
            get => _treeNodes;
            set => SetProperty(ref _treeNodes, value);
        }

        public System.Data.DataTable? TableData
        {
            get => _tableData;
            set => SetProperty(ref _tableData, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Set JSON text without marking the tab as dirty (for initial load)
        /// </summary>
        public void SetJsonTextWithoutDirty(string jsonText)
        {
            _isInitializing = true;
            try
            {
                JsonText = jsonText;
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private void UpdateHeader()
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                Header = IsDirty ? "Untitled*" : "Untitled";
            }
            else
            {
                var fileName = System.IO.Path.GetFileName(FilePath);
                Header = IsDirty ? $"{fileName}*" : fileName;
            }
        }
    }
}
