using System.Collections.ObjectModel;

namespace JsonFormatterApp.Models
{
    public class JsonTreeNode
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public ObservableCollection<JsonTreeNode> Children { get; set; } = new();
        public bool IsExpanded { get; set; } = false;
    }
}
