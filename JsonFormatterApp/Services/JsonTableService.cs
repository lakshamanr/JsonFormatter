using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json.Linq;
using JsonFormatterApp.Models;

namespace JsonFormatterApp.Services
{
    public class JsonTableService
    {
        /// <summary>
        /// Convert JSON array to DataTable for tabular display
        /// </summary>
        public DataTable? ConvertToTable(string jsonText)
        {
            try
            {
                var jToken = JToken.Parse(jsonText);

                // Check if it's an array
                if (jToken is JArray jArray && jArray.Count > 0)
                {
                    return CreateTableFromArray(jArray);
                }
                // Check if it's an object containing an array property
                else if (jToken is JObject jObj)
                {
                    // Try to find the first array property
                    foreach (var property in jObj.Properties())
                    {
                        if (property.Value is JArray array && array.Count > 0)
                        {
                            return CreateTableFromArray(array);
                        }
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private DataTable CreateTableFromArray(JArray jArray)
        {
            var dataTable = new DataTable();

            // Check if array contains objects
            if (jArray[0] is not JObject)
            {
                // Simple array of primitives
                dataTable.Columns.Add("Index", typeof(int));
                dataTable.Columns.Add("Value", typeof(string));

                for (int i = 0; i < jArray.Count; i++)
                {
                    dataTable.Rows.Add(i, jArray[i].ToString());
                }

                return dataTable;
            }

            // Array of objects - extract all unique properties
            var allProperties = new HashSet<string>();
            foreach (JObject item in jArray)
            {
                foreach (var prop in item.Properties())
                {
                    allProperties.Add(prop.Name);
                }
            }

            // Add columns
            foreach (var propName in allProperties.OrderBy(p => p))
            {
                dataTable.Columns.Add(propName, typeof(string));
            }

            // Add rows
            foreach (JObject item in jArray)
            {
                var row = dataTable.NewRow();
                foreach (var prop in item.Properties())
                {
                    row[prop.Name] = FormatValue(prop.Value);
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        private string FormatValue(JToken token)
        {
            return token.Type switch
            {
                JTokenType.Object => "{...}",
                JTokenType.Array => $"[{((JArray)token).Count} items]",
                JTokenType.Null => "null",
                JTokenType.String => token.ToString(),
                _ => token.ToString()
            };
        }

        /// <summary>
        /// Check if JSON can be displayed as a table
        /// </summary>
        public bool CanDisplayAsTable(string jsonText)
        {
            try
            {
                var jToken = JToken.Parse(jsonText);

                if (jToken is JArray jArray && jArray.Count > 0)
                {
                    return true;
                }

                if (jToken is JObject jObj)
                {
                    foreach (var property in jObj.Properties())
                    {
                        if (property.Value is JArray array && array.Count > 0)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Extract all tables from JSON including nested ones
        /// </summary>
        public List<TableInfo> ExtractAllTables(string jsonText)
        {
            var tables = new List<TableInfo>();

            try
            {
                var jToken = JToken.Parse(jsonText);
                ExtractTablesRecursive(jToken, "", "Root", tables);
            }
            catch
            {
                // Return empty list on error
            }

            return tables;
        }

        private void ExtractTablesRecursive(JToken token, string path, string name, List<TableInfo> tables)
        {
            if (token is JArray jArray && jArray.Count > 0)
            {
                // Create table from array
                var tableInfo = CreateTableInfo(jArray, path, name);
                if (tableInfo != null)
                {
                    tables.Add(tableInfo);

                    // Check for nested arrays/objects in the first item to detect nested tables
                    if (jArray[0] is JObject firstObj)
                    {
                        foreach (var prop in firstObj.Properties())
                        {
                            if (prop.Value is JArray || prop.Value is JObject)
                            {
                                var nestedPath = string.IsNullOrEmpty(path) ? prop.Name : $"{path}.{prop.Name}";
                                ExtractTablesRecursive(prop.Value, nestedPath, prop.Name, tables);
                            }
                        }
                    }
                }
            }
            else if (token is JObject jObj)
            {
                // Create table for this object (Property-Value format)
                if (jObj.Properties().Any())
                {
                    var objectTable = CreateTableFromObject(jObj, path, name);
                    tables.Add(objectTable);
                }

                // Also look for nested array/object properties
                foreach (var property in jObj.Properties())
                {
                    if (property.Value is JArray || property.Value is JObject)
                    {
                        var nestedPath = string.IsNullOrEmpty(path) ? property.Name : $"{path}.{property.Name}";
                        ExtractTablesRecursive(property.Value, nestedPath, property.Name, tables);
                    }
                }
            }
        }

        private TableInfo? CreateTableInfo(JArray jArray, string path, string name)
        {
            try
            {
                var dataTable = CreateTableFromArray(jArray);
                var tableInfo = new TableInfo
                {
                    Path = path,
                    Name = name,
                    Data = dataTable,
                    RowCount = dataTable.Rows.Count,
                    ColumnCount = dataTable.Columns.Count,
                    Columns = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList(),
                    Description = $"{name} ({dataTable.Rows.Count} rows, {dataTable.Columns.Count} columns)"
                };

                // Detect nested tables
                DetectNestedTables(jArray, tableInfo);

                // Initialize filter
                tableInfo.InitializeFilter();

                return tableInfo;
            }
            catch
            {
                return null;
            }
        }

        private void DetectNestedTables(JArray jArray, TableInfo tableInfo)
        {
            for (int rowIndex = 0; rowIndex < jArray.Count; rowIndex++)
            {
                if (jArray[rowIndex] is JObject jObj)
                {
                    foreach (var prop in jObj.Properties())
                    {
                        if (prop.Value is JArray nestedArray && nestedArray.Count > 0)
                        {
                            var key = $"{prop.Name}_Row{rowIndex}";
                            tableInfo.NestedTables[key] = new NestedTableReference
                            {
                                ParentColumn = prop.Name,
                                ParentRow = rowIndex,
                                Path = $"{tableInfo.Path}.{prop.Name}[{rowIndex}]",
                                Type = "Array",
                                NestedTable = CreateTableInfo(nestedArray, $"{tableInfo.Path}.{prop.Name}", prop.Name)
                            };
                        }
                        else if (prop.Value is JObject nestedObj)
                        {
                            var key = $"{prop.Name}_Row{rowIndex}";
                            tableInfo.NestedTables[key] = new NestedTableReference
                            {
                                ParentColumn = prop.Name,
                                ParentRow = rowIndex,
                                Path = $"{tableInfo.Path}.{prop.Name}[{rowIndex}]",
                                Type = "Object",
                                NestedTable = CreateTableFromObject(nestedObj, $"{tableInfo.Path}.{prop.Name}", prop.Name)
                            };
                        }
                    }
                }
            }
        }

        private TableInfo CreateTableFromObject(JObject jObj, string path, string name)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("Property", typeof(string));
            dataTable.Columns.Add("Value", typeof(string));

            foreach (var prop in jObj.Properties())
            {
                var row = dataTable.NewRow();
                row["Property"] = prop.Name;
                row["Value"] = FormatValue(prop.Value);
                dataTable.Rows.Add(row);
            }

            var tableInfo = new TableInfo
            {
                Path = path,
                Name = name,
                Data = dataTable,
                RowCount = dataTable.Rows.Count,
                ColumnCount = 2,
                Columns = new List<string> { "Property", "Value" },
                Description = $"{name} (Object with {dataTable.Rows.Count} properties)"
            };

            // Initialize filter
            tableInfo.InitializeFilter();

            return tableInfo;
        }
    }
}
