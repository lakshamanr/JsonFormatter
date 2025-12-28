using System;
using System.Data;
using System.IO;
using System.Text;
using JsonFormatterApp.Models;

namespace JsonFormatterApp.Services
{
    public class TableExportService
    {
        /// <summary>
        /// Export table to CSV format
        /// </summary>
        public void ExportToCsv(TableInfo tableInfo, string filePath)
        {
            if (tableInfo?.Data == null)
                throw new ArgumentException("Table data is null");

            var csv = new StringBuilder();

            // Add headers
            var headers = new string[tableInfo.Data.Columns.Count];
            for (int i = 0; i < tableInfo.Data.Columns.Count; i++)
            {
                headers[i] = EscapeCsvField(tableInfo.Data.Columns[i].ColumnName);
            }
            csv.AppendLine(string.Join(",", headers));

            // Add rows
            foreach (DataRow row in tableInfo.Data.Rows)
            {
                var fields = new string[tableInfo.Data.Columns.Count];
                for (int i = 0; i < tableInfo.Data.Columns.Count; i++)
                {
                    fields[i] = EscapeCsvField(row[i]?.ToString() ?? "");
                }
                csv.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Export table to HTML format
        /// </summary>
        public void ExportToHtml(TableInfo tableInfo, string filePath)
        {
            if (tableInfo?.Data == null)
                throw new ArgumentException("Table data is null");

            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine($"<title>{tableInfo.Name}</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
            html.AppendLine("h1 { color: #333; }");
            html.AppendLine("table { border-collapse: collapse; width: 100%; margin-top: 20px; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 12px; text-align: left; }");
            html.AppendLine("th { background-color: #4CAF50; color: white; font-weight: bold; }");
            html.AppendLine("tr:nth-child(even) { background-color: #f2f2f2; }");
            html.AppendLine("tr:hover { background-color: #ddd; }");
            html.AppendLine(".info { margin-bottom: 20px; padding: 10px; background-color: #e7f3fe; border-left: 6px solid #2196F3; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine($"<h1>{tableInfo.Name}</h1>");
            html.AppendLine($"<div class='info'>");
            html.AppendLine($"<p><strong>Path:</strong> {tableInfo.Path}</p>");
            html.AppendLine($"<p><strong>Rows:</strong> {tableInfo.RowCount} | <strong>Columns:</strong> {tableInfo.ColumnCount}</p>");
            if (tableInfo.HasNestedTables)
            {
                html.AppendLine($"<p><strong>Nested Tables:</strong> {tableInfo.NestedTableCount}</p>");
            }
            html.AppendLine("</div>");
            html.AppendLine("<table>");

            // Headers
            html.AppendLine("<thead><tr>");
            foreach (DataColumn column in tableInfo.Data.Columns)
            {
                html.AppendLine($"<th>{System.Web.HttpUtility.HtmlEncode(column.ColumnName)}</th>");
            }
            html.AppendLine("</tr></thead>");

            // Rows
            html.AppendLine("<tbody>");
            foreach (DataRow row in tableInfo.Data.Rows)
            {
                html.AppendLine("<tr>");
                foreach (DataColumn column in tableInfo.Data.Columns)
                {
                    var cellValue = row[column]?.ToString() ?? "";
                    html.AppendLine($"<td>{System.Web.HttpUtility.HtmlEncode(cellValue)}</td>");
                }
                html.AppendLine("</tr>");
            }
            html.AppendLine("</tbody>");
            html.AppendLine("</table>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            File.WriteAllText(filePath, html.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Export table to JSON format
        /// </summary>
        public void ExportToJson(TableInfo tableInfo, string filePath)
        {
            if (tableInfo?.Data == null)
                throw new ArgumentException("Table data is null");

            var json = new StringBuilder();
            json.AppendLine("{");
            json.AppendLine($"  \"name\": \"{EscapeJsonString(tableInfo.Name)}\",");
            json.AppendLine($"  \"path\": \"{EscapeJsonString(tableInfo.Path)}\",");
            json.AppendLine($"  \"rowCount\": {tableInfo.RowCount},");
            json.AppendLine($"  \"columnCount\": {tableInfo.ColumnCount},");
            json.AppendLine("  \"data\": [");

            bool firstRow = true;
            foreach (DataRow row in tableInfo.Data.Rows)
            {
                if (!firstRow)
                    json.AppendLine(",");
                firstRow = false;

                json.Append("    {");
                bool firstCol = true;
                foreach (DataColumn column in tableInfo.Data.Columns)
                {
                    if (!firstCol)
                        json.Append(", ");
                    firstCol = false;

                    var value = row[column]?.ToString() ?? "";
                    json.Append($"\"{EscapeJsonString(column.ColumnName)}\": \"{EscapeJsonString(value)}\"");
                }
                json.Append("}");
            }

            json.AppendLine();
            json.AppendLine("  ]");
            json.AppendLine("}");

            File.WriteAllText(filePath, json.ToString(), Encoding.UTF8);
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "\"\"";

            // Escape quotes and wrap in quotes if contains comma, quote, or newline
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }

            return field;
        }

        private string EscapeJsonString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return "";

            return str.Replace("\\", "\\\\")
                      .Replace("\"", "\\\"")
                      .Replace("\n", "\\n")
                      .Replace("\r", "\\r")
                      .Replace("\t", "\\t");
        }
    }
}
