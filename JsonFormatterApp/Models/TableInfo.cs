using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using JsonFormatterApp.Helpers;

namespace JsonFormatterApp.Models
{
    /// <summary>
    /// Represents metadata about a JSON table
    /// </summary>
    public class TableInfo : ViewModelBase
    {
        private string _searchText = string.Empty;
        private DataView? _filteredData;

        public string Path { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DataTable Data { get; set; } = new DataTable();
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public List<string> Columns { get; set; } = new();
        public Dictionary<string, NestedTableReference> NestedTables { get; set; } = new();
        public string Description { get; set; } = string.Empty;
        public int NestedTableCount => NestedTables.Count;
        public bool HasNestedTables => NestedTables.Count > 0;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilter();
                }
            }
        }

        public DataView? FilteredData
        {
            get => _filteredData;
            set => SetProperty(ref _filteredData, value);
        }

        public void ApplyFilter()
        {
            if (Data == null)
            {
                FilteredData = null;
                return;
            }

            var dataView = Data.DefaultView;

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                dataView.RowFilter = string.Empty;
            }
            else
            {
                var filters = new List<string>();
                foreach (DataColumn column in Data.Columns)
                {
                    if (column.DataType == typeof(string))
                    {
                        filters.Add($"[{column.ColumnName}] LIKE '%{SearchText.Replace("'", "''")}%'");
                    }
                }

                if (filters.Count > 0)
                {
                    dataView.RowFilter = string.Join(" OR ", filters);
                }
            }

            FilteredData = dataView;
        }

        public void InitializeFilter()
        {
            FilteredData = Data?.DefaultView;
        }
    }

    /// <summary>
    /// Represents a reference to a nested table within a cell
    /// </summary>
    public class NestedTableReference
    {
        public string ParentColumn { get; set; } = string.Empty;
        public int ParentRow { get; set; }
        public string Path { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Object" or "Array"
        public TableInfo? NestedTable { get; set; }
    }
}
