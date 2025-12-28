using JsonFormatterApp.Models;

namespace JsonFormatterApp.Infrastructure
{
    /// <summary>
    /// Messages for inter-ViewModel communication
    /// </summary>

    public class TabSelectedMessage
    {
        public TabItem? SelectedTab { get; set; }
    }

    public class TabContentChangedMessage
    {
        public TabItem Tab { get; set; } = null!;
    }

    public class RefreshViewsMessage
    {
        public TabItem Tab { get; set; } = null!;
    }

    public class ThemeChangedMessage
    {
        public string ThemeName { get; set; } = null!;
    }

    public class StatusUpdateMessage
    {
        public string Message { get; set; } = null!;
    }
}
