namespace JsonFormatterApp.Models
{
    public class JsonValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        public string Path { get; set; } = string.Empty;
    }
}
