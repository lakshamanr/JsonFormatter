using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json.Linq;

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
    }
}
