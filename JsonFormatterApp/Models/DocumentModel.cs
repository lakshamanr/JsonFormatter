using System.Collections.ObjectModel;
using JsonFormatterApp.Helpers;
using JsonFormatterApp.Models;
using System.Collections.Generic;

namespace JsonFormatterApp.Models
{
    /// <summary>
    /// Represents the current document being edited
    /// </summary>
    public class DocumentModel : ViewModelBase
    {
        private string _jsonText = string.Empty;
        private string _filePath = string.Empty;
        private bool _isDirty = false;
        private bool _isValid = true;
        private ObservableCollection<JsonTreeNode> _treeNodes = new();
        private ObservableCollection<JsonTreeNode> _filteredTreeNodes = new();
        private string _treeSearchText = string.Empty;
        private System.Data.DataTable? _tableData;
        private string _statusMessage = "Ready";
        private bool _isInitializing = false;
        private ObservableCollection<TableInfo> _allTables = new();
        private TableInfo? _selectedTable;

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
            set => SetProperty(ref _filePath, value);
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => SetProperty(ref _isDirty, value);
        }

        public bool IsValid
        {
            get => _isValid;
            set => SetProperty(ref _isValid, value);
        }

        public ObservableCollection<JsonTreeNode> TreeNodes
        {
            get => _treeNodes;
            set
            {
                if (SetProperty(ref _treeNodes, value))
                {
                    ApplyTreeFilter();
                }
            }
        }

        public ObservableCollection<JsonTreeNode> FilteredTreeNodes
        {
            get => _filteredTreeNodes;
            set => SetProperty(ref _filteredTreeNodes, value);
        }

        public string TreeSearchText
        {
            get => _treeSearchText;
            set
            {
                if (SetProperty(ref _treeSearchText, value))
                {
                    ApplyTreeFilter();
                }
            }
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

        public ObservableCollection<TableInfo> AllTables
        {
            get => _allTables;
            set => SetProperty(ref _allTables, value);
        }

        public TableInfo? SelectedTable
        {
            get => _selectedTable;
            set => SetProperty(ref _selectedTable, value);
        }

        /// <summary>
        /// Set JSON text without marking the document as dirty (for initial load)
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

        /// <summary>
        /// Gets the display title for the window (filename or "Untitled")
        /// </summary>
        public string GetDisplayTitle()
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                return IsDirty ? "Untitled*" : "Untitled";
            }
            else
            {
                var fileName = System.IO.Path.GetFileName(FilePath);
                return IsDirty ? $"{fileName}*" : fileName;
            }
        }

        /// <summary>
        /// Apply filter to tree nodes based on search text
        /// </summary>
        public void ApplyTreeFilter()
        {
            if (string.IsNullOrWhiteSpace(TreeSearchText))
            {
                // No filter, show all nodes
                FilteredTreeNodes = new ObservableCollection<JsonTreeNode>(TreeNodes);
            }
            else
            {
                // Filter tree nodes
                var filtered = new ObservableCollection<JsonTreeNode>();
                foreach (var node in TreeNodes)
                {
                    var filteredNode = FilterTreeNode(node, TreeSearchText.ToLower());
                    if (filteredNode != null)
                    {
                        filtered.Add(filteredNode);
                    }
                }
                FilteredTreeNodes = filtered;
            }
        }

        private JsonTreeNode? FilterTreeNode(JsonTreeNode node, string searchText)
        {
            // Check if current node matches
            bool nodeMatches = node.Key.ToLower().Contains(searchText) ||
                              (node.Value?.ToLower().Contains(searchText) ?? false);

            // Filter children recursively
            var filteredChildren = new ObservableCollection<JsonTreeNode>();
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    var filteredChild = FilterTreeNode(child, searchText);
                    if (filteredChild != null)
                    {
                        filteredChildren.Add(filteredChild);
                    }
                }
            }

            // Include node if it matches or has matching children
            if (nodeMatches || filteredChildren.Count > 0)
            {
                return new JsonTreeNode
                {
                    Key = node.Key,
                    Value = node.Value,
                    Type = node.Type,
                    Children = filteredChildren,
                    IsExpanded = node.IsExpanded
                };
            }

            return null;
        }
    }
}
