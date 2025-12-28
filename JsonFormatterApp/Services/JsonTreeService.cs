using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using JsonFormatterApp.Models;

namespace JsonFormatterApp.Services
{
    public class JsonTreeService
    {
        public ObservableCollection<JsonTreeNode> BuildTree(string jsonText)
        {
            var tree = new ObservableCollection<JsonTreeNode>();

            try
            {
                var jToken = JToken.Parse(jsonText);
                ProcessToken(jToken, tree, "root");
            }
            catch
            {
                // Return empty tree on error
            }

            return tree;
        }

        private void ProcessToken(JToken token, ObservableCollection<JsonTreeNode> nodes, string key)
        {
            var node = new JsonTreeNode { Key = key };

            switch (token.Type)
            {
                case JTokenType.Object:
                    node.Type = "Object";
                    node.Value = $"{{ {((JObject)token).Count} properties }}";
                    node.Children = new ObservableCollection<JsonTreeNode>();
                    node.IsExpanded = true;

                    foreach (var property in ((JObject)token).Properties())
                    {
                        ProcessToken(property.Value, node.Children, property.Name);
                    }
                    break;

                case JTokenType.Array:
                    node.Type = "Array";
                    node.Value = $"[ {((JArray)token).Count} items ]";
                    node.Children = new ObservableCollection<JsonTreeNode>();
                    node.IsExpanded = true;

                    int index = 0;
                    foreach (var item in (JArray)token)
                    {
                        ProcessToken(item, node.Children, $"[{index}]");
                        index++;
                    }
                    break;

                case JTokenType.String:
                    node.Type = "String";
                    node.Value = $"\"{token}\"";
                    break;

                case JTokenType.Integer:
                case JTokenType.Float:
                    node.Type = "Number";
                    node.Value = token.ToString();
                    break;

                case JTokenType.Boolean:
                    node.Type = "Boolean";
                    node.Value = token.ToString().ToLower();
                    break;

                case JTokenType.Null:
                    node.Type = "Null";
                    node.Value = "null";
                    break;

                default:
                    node.Type = token.Type.ToString();
                    node.Value = token.ToString();
                    break;
            }

            nodes.Add(node);
        }
    }
}
