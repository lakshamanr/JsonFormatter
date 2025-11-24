using System;
using System.Collections.Generic;
using System.Text;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Newtonsoft.Json.Linq;

namespace JsonFormatterApp.Services
{
    public class DiffService
    {
        /// <summary>
        /// Compare two JSON strings and return diff result
        /// </summary>
        public string CompareJson(string leftJson, string rightJson, bool ignoreWhitespace = true, bool ignoreOrder = false)
        {
            try
            {
                string left = leftJson;
                string right = rightJson;

                // Normalize JSON if ignoring whitespace
                if (ignoreWhitespace)
                {
                    var leftToken = JToken.Parse(leftJson);
                    var rightToken = JToken.Parse(rightJson);
                    left = leftToken.ToString(Newtonsoft.Json.Formatting.Indented);
                    right = rightToken.ToString(Newtonsoft.Json.Formatting.Indented);
                }

                // Create diff
                var diffBuilder = new InlineDiffBuilder(new Differ());
                var diff = diffBuilder.BuildDiffModel(left, right, ignoreWhitespace);

                return FormatDiffResult(diff);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to compare JSON: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Format diff result as text
        /// </summary>
        private string FormatDiffResult(DiffPaneModel diff)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== JSON Comparison Result ===");
            sb.AppendLine();

            foreach (var line in diff.Lines)
            {
                var prefix = line.Type switch
                {
                    ChangeType.Inserted => "+ ",
                    ChangeType.Deleted => "- ",
                    ChangeType.Modified => "! ",
                    _ => "  "
                };

                sb.AppendLine($"{prefix}{line.Text}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get semantic differences between two JSON objects
        /// </summary>
        public List<string> GetSemanticDifferences(string leftJson, string rightJson)
        {
            var differences = new List<string>();

            try
            {
                var leftToken = JToken.Parse(leftJson);
                var rightToken = JToken.Parse(rightJson);

                CompareTokens(leftToken, rightToken, "", differences);
            }
            catch (Exception ex)
            {
                differences.Add($"Error comparing JSON: {ex.Message}");
            }

            return differences;
        }

        private void CompareTokens(JToken left, JToken right, string path, List<string> differences)
        {
            if (left.Type != right.Type)
            {
                differences.Add($"{path}: Type mismatch - {left.Type} vs {right.Type}");
                return;
            }

            switch (left.Type)
            {
                case JTokenType.Object:
                    CompareObjects((JObject)left, (JObject)right, path, differences);
                    break;

                case JTokenType.Array:
                    CompareArrays((JArray)left, (JArray)right, path, differences);
                    break;

                default:
                    if (!JToken.DeepEquals(left, right))
                    {
                        differences.Add($"{path}: Value mismatch - '{left}' vs '{right}'");
                    }
                    break;
            }
        }

        private void CompareObjects(JObject left, JObject right, string path, List<string> differences)
        {
            var leftProps = left.Properties().ToDictionary(p => p.Name);
            var rightProps = right.Properties().ToDictionary(p => p.Name);

            // Check for missing properties in right
            foreach (var prop in leftProps.Keys)
            {
                var currentPath = string.IsNullOrEmpty(path) ? prop : $"{path}.{prop}";

                if (!rightProps.ContainsKey(prop))
                {
                    differences.Add($"{currentPath}: Property missing in right object");
                }
                else
                {
                    CompareTokens(leftProps[prop].Value, rightProps[prop].Value, currentPath, differences);
                }
            }

            // Check for extra properties in right
            foreach (var prop in rightProps.Keys)
            {
                if (!leftProps.ContainsKey(prop))
                {
                    var currentPath = string.IsNullOrEmpty(path) ? prop : $"{path}.{prop}";
                    differences.Add($"{currentPath}: Property missing in left object");
                }
            }
        }

        private void CompareArrays(JArray left, JArray right, string path, List<string> differences)
        {
            if (left.Count != right.Count)
            {
                differences.Add($"{path}: Array length mismatch - {left.Count} vs {right.Count}");
            }

            var minLength = Math.Min(left.Count, right.Count);
            for (int i = 0; i < minLength; i++)
            {
                CompareTokens(left[i], right[i], $"{path}[{i}]", differences);
            }
        }
    }
}
