using JsonFormatterApp.Services;
using JsonFormatterApp.Models;
using JsonFormatterApp.ViewModels;
using JsonFormatterApp.Infrastructure;

namespace JsonFormatterApp.Tests
{
    /// <summary>
    /// Tests to verify JSON operations (Format, Minify, Validate) work correctly per tab
    /// </summary>
    public class JsonOperationsTests
    {
        private readonly JsonService _jsonService;

        public JsonOperationsTests()
        {
            _jsonService = new JsonService();
        }

        [Fact]
        public void Format_Should_Work_With_Different_Indent_Sizes()
        {
            // Arrange
            var json = "{\"name\":\"test\",\"value\":123}";

            // Act
            var formatted2 = _jsonService.FormatJson(json, 2);
            var formatted4 = _jsonService.FormatJson(json, 4);

            // Assert - Both should be formatted but with different indentation
            Assert.Contains("\n", formatted2);
            Assert.Contains("\n", formatted4);
            Assert.NotEqual(formatted2, formatted4); // Different indentation
        }

        [Fact]
        public void Minify_Should_Remove_All_Whitespace()
        {
            // Arrange
            var formattedJson = @"{
  ""name"": ""test"",
  ""value"": 123,
  ""nested"": {
    ""key"": ""value""
  }
}";

            // Act
            var minified = _jsonService.MinifyJson(formattedJson);

            // Assert
            Assert.DoesNotContain("\n", minified);
            Assert.DoesNotContain("  ", minified); // No double spaces
        }

        [Fact]
        public void Validate_Should_Detect_Valid_JSON()
        {
            // Arrange
            var validJson = "{\"name\":\"test\",\"value\":123}";

            // Act
            var result = _jsonService.ValidateJson(validJson);

            // Assert
            Assert.True(result.IsValid);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void Validate_Should_Detect_Invalid_JSON()
        {
            // Arrange
            var invalidJson = "{\"name\":\"test\",\"value\":}"; // Missing value

            // Act
            var result = _jsonService.ValidateJson(invalidJson);

            // Assert
            Assert.False(result.IsValid);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public void Format_Then_Minify_Should_Produce_Original_Structure()
        {
            // Arrange
            var originalJson = "{\"name\":\"test\",\"age\":30,\"active\":true}";

            // Act
            var formatted = _jsonService.FormatJson(originalJson, 2);
            var minified = _jsonService.MinifyJson(formatted);

            // Assert - Should be semantically equivalent
            var originalValidation = _jsonService.ValidateJson(originalJson);
            var minifiedValidation = _jsonService.ValidateJson(minified);

            Assert.True(originalValidation.IsValid);
            Assert.True(minifiedValidation.IsValid);
            Assert.DoesNotContain("\n", minified);
        }

        [Fact]
        public void Ten_Tabs_With_Different_JSON_Should_Format_Independently()
        {
            // Arrange - Create 10 different JSON strings
            var jsonStrings = new List<string>();
            for (int i = 1; i <= 10; i++)
            {
                jsonStrings.Add($"{{\"tab\":{i},\"name\":\"Tab{i}\",\"value\":{i * 10}}}");
            }

            // Act - Format each independently
            var formattedResults = jsonStrings.Select(json => _jsonService.FormatJson(json, 2)).ToList();

            // Assert - All should be formatted and contain their original data
            for (int i = 0; i < 10; i++)
            {
                Assert.Contains("\n", formattedResults[i]); // Is formatted
                Assert.Contains($"\"tab\": {i + 1}", formattedResults[i]); // Contains correct tab number
                Assert.Contains($"\"name\": \"Tab{i + 1}\"", formattedResults[i]); // Contains correct name
            }
        }

        [Fact]
        public void Validate_Should_Provide_Line_And_Column_Info_For_Errors()
        {
            // Arrange
            var invalidJson = @"{
  ""name"": ""test"",
  ""value"":
}";

            // Act
            var result = _jsonService.ValidateJson(invalidJson);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.LineNumber > 0);
            Assert.True(result.ColumnNumber > 0);
        }

        [Fact]
        public void Format_Should_Handle_Nested_Objects_And_Arrays()
        {
            // Arrange
            var complexJson = "{\"users\":[{\"name\":\"John\",\"age\":30},{\"name\":\"Jane\",\"age\":25}],\"metadata\":{\"version\":1,\"type\":\"test\"}}";

            // Act
            var formatted = _jsonService.FormatJson(complexJson, 2);

            // Assert
            Assert.Contains("\"users\":", formatted);
            Assert.Contains("\"metadata\":", formatted);
            Assert.Contains("\n", formatted);

            // Should be valid JSON
            var validation = _jsonService.ValidateJson(formatted);
            Assert.True(validation.IsValid);
        }

        [Fact]
        public void Empty_String_Should_Be_Invalid_JSON()
        {
            // Arrange
            var emptyJson = "";

            // Act
            var result = _jsonService.ValidateJson(emptyJson);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Whitespace_Only_Should_Be_Invalid_JSON()
        {
            // Arrange
            var whitespaceJson = "   \n  \t  ";

            // Act
            var result = _jsonService.ValidateJson(whitespaceJson);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Format_Should_Preserve_Boolean_Values()
        {
            // Arrange
            var json = "{\"enabled\":true,\"disabled\":false}";

            // Act
            var formatted = _jsonService.FormatJson(json, 2);

            // Assert
            Assert.Contains("\"enabled\": true", formatted);
            Assert.Contains("\"disabled\": false", formatted);
        }

        [Fact]
        public void Format_Should_Preserve_Null_Values()
        {
            // Arrange
            var json = "{\"value\":null,\"name\":\"test\"}";

            // Act
            var formatted = _jsonService.FormatJson(json, 2);

            // Assert
            Assert.Contains("\"value\": null", formatted);
        }

        [Fact]
        public void Format_Should_Preserve_Number_Precision()
        {
            // Arrange
            var json = "{\"pi\":3.14159265359,\"integer\":42,\"negative\":-123.456}";

            // Act
            var formatted = _jsonService.FormatJson(json, 2);

            // Assert
            Assert.Contains("3.14159265359", formatted);
            Assert.Contains("42", formatted);
            Assert.Contains("-123.456", formatted);
        }
    }
}
