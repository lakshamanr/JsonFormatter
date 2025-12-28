using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonFormatterApp.Models;

namespace JsonFormatterApp.Services
{
    public class JsonService
    {
        /// <summary>
        /// Format JSON with pretty print
        /// </summary>
        public string FormatJson(string jsonText, int indentSize = 2)
        {
            try
            {
                var jToken = JToken.Parse(jsonText);

                // Use JsonTextWriter to support custom indent sizes
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                using (var writer = new JsonTextWriter(sw))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.IndentChar = ' ';
                    writer.Indentation = indentSize;

                    jToken.WriteTo(writer);
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to format JSON: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Minify JSON by removing whitespace
        /// </summary>
        public string MinifyJson(string jsonText)
        {
            try
            {
                var jToken = JToken.Parse(jsonText);
                return JsonConvert.SerializeObject(jToken, Formatting.None);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to minify JSON: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validate JSON and return detailed error information
        /// </summary>
        public JsonValidationResult ValidateJson(string jsonText)
        {
            var result = new JsonValidationResult { IsValid = true };

            if (string.IsNullOrWhiteSpace(jsonText))
            {
                result.IsValid = false;
                result.ErrorMessage = "JSON text is empty";
                return result;
            }

            try
            {
                JToken.Parse(jsonText);
            }
            catch (JsonReaderException ex)
            {
                result.IsValid = false;
                result.ErrorMessage = ex.Message;
                result.LineNumber = ex.LineNumber;
                result.ColumnNumber = ex.LinePosition;
                result.Path = ex.Path ?? string.Empty;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Escape JSON string for embedding
        /// </summary>
        public string EscapeJson(string jsonText)
        {
            return JsonConvert.ToString(jsonText);
        }

        /// <summary>
        /// Unescape JSON string
        /// </summary>
        public string UnescapeJson(string escapedJson)
        {
            if (escapedJson.StartsWith("\"") && escapedJson.EndsWith("\""))
            {
                return JsonConvert.DeserializeObject<string>(escapedJson) ?? string.Empty;
            }
            return escapedJson;
        }
    }
}
