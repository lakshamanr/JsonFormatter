using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
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
                // Parse multiple search terms (separated by space or comma)
                var searchTerms = SearchText
                    .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim().Replace("'", "''"))
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();

                if (searchTerms.Count > 0)
                {
                    // Build filter: Each search term must match at least one column (AND logic - acts as filters)
                    var termFilters = new List<string>();

                    foreach (var term in searchTerms)
                    {
                        var columnFilters = new List<string>();
                        foreach (DataColumn column in Data.Columns)
                        {
                            if (column.DataType == typeof(string))
                            {
                                columnFilters.Add($"[{column.ColumnName}] LIKE '%{term}%'");
                            }
                        }

                        if (columnFilters.Count > 0)
                        {
                            // Each term should match at least one column
                            termFilters.Add($"({string.Join(" OR ", columnFilters)})");
                        }
                    }

                    if (termFilters.Count > 0)
                    {
                        // Combine all term filters with AND (row must match ALL search terms - acts as filters)
                        dataView.RowFilter = string.Join(" AND ", termFilters);
                    }
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
