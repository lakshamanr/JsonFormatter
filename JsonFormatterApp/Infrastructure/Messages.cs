namespace JsonFormatterApp.Infrastructure
{
    /// <summary>
    /// Messages for inter-ViewModel communication
    /// </summary>

    public class RefreshViewsMessage
    {
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
