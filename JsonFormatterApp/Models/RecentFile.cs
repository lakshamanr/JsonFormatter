using System;

namespace JsonFormatterApp.Models
{
    public class RecentFile
    {
        public string FilePath { get; set; } = string.Empty;
        public DateTime LastAccessed { get; set; }
    }
}
