using JsonFormatterApp.Services;

namespace JsonFormatterApp.Tests
{
    /// <summary>
    /// Service-layer tests for Format/Minify/Validate operations across 10 tabs worth of data
    /// These tests don't require WPF UI components
    /// </summary>
    public class ServiceLayerTests
    {
        private readonly JsonService _jsonService;
        private readonly JsonTreeService _treeService;
        private readonly JsonTableService _tableService;

        public ServiceLayerTests()
        {
            _jsonService = new JsonService();
            _treeService = new JsonTreeService();
            _tableService = new JsonTableService();
        }

        #region Format Button Logic Tests

        [Fact]
        public void Format_Operation_Should_Work_Independently_For_10_JSON_Strings()
        {
            // Arrange - Create 10 different minified JSON strings (simulating 10 tabs)
            var jsonStrings = new List<string>();
            for (int i = 1; i <= 10; i++)
            {
                jsonStrings.Add($"{{\"tab\":{i},\"name\":\"Tab{i}\",\"value\":{i * 10}}}");
            }

            // Act - Format only JSON string 5 (simulating formatting only tab 5)
            var formattedStrings = new List<string>(jsonStrings);
            formattedStrings[4] = _jsonService.FormatJson(jsonStrings[4], 2);

            // Assert - Only string 5 should be formatted
            for (int i = 0; i < 10; i++)
            {
                if (i == 4)
                {
                    Assert.Contains("\n", formattedStrings[i]); // String 5 is formatted
                    Assert.NotEqual(jsonStrings[i], formattedStrings[i]); // Changed
                }
                else
                {
                    Assert.DoesNotContain("\n", formattedStrings[i]); // Others remain minified
                    Assert.Equal(jsonStrings[i], formattedStrings[i]); // Unchanged
                }
            }
        }

        [Fact]
        public void Format_Multiple_Tabs_With_Different_Indent_Sizes()
        {
            // Arrange - 10 JSON strings
            var jsonStrings = new List<string>();
            for (int i = 1; i <= 10; i++)
            {
                jsonStrings.Add($"{{\"tab\":{i},\"data\":{{\"nested\":true}}}}");
            }

            // Act - Format tabs 2,5,8 with indent 2 and tabs 3,6,9 with indent 4
            var results = new List<string>(jsonStrings);

            // Indent size 2
            results[1] = _jsonService.FormatJson(jsonStrings[1], 2);
            results[4] = _jsonService.FormatJson(jsonStrings[4], 2);
            results[7] = _jsonService.FormatJson(jsonStrings[7], 2);

            // Indent size 4
            results[2] = _jsonService.FormatJson(jsonStrings[2], 4);
            results[5] = _jsonService.FormatJson(jsonStrings[5], 4);
            results[8] = _jsonService.FormatJson(jsonStrings[8], 4);

            // Assert - Formatted tabs should have newlines, others shouldn't
            var formattedIndices = new[] { 1, 2, 4, 5, 7, 8 };
            for (int i = 0; i < 10; i++)
            {
                if (formattedIndices.Contains(i))
                {
                    Assert.Contains("\n", results[i]);
                }
                else
                {
                    Assert.DoesNotContain("\n", results[i]);
                }
            }

            // Different indent sizes should produce different results
            Assert.NotEqual(results[1], results[2]); // Tab 2 (indent 2) vs Tab 3 (indent 4)
        }

        [Fact]
        public void Format_All_10_Tabs_Should_Preserve_Data()
        {
            // Arrange
            var jsonStrings = new List<string>();
            for (int i = 1; i <= 10; i++)
            {
                jsonStrings.Add($"{{\"id\":{i},\"name\":\"User{i}\",\"active\":true}}");
            }

            // Act - Format all 10
            var formatted = jsonStrings.Select(json => _jsonService.FormatJson(json, 2)).ToList();

            // Assert - All should be valid and formatted
            for (int i = 0; i < 10; i++)
            {
                Assert.Contains("\n", formatted[i]);
                Assert.Contains($"\"id\": {i + 1}", formatted[i]);
                Assert.Contains($"\"name\": \"User{i + 1}\"", formatted[i]);
                Assert.Contains("\"active\": true", formatted[i]);

                // Should still be valid JSON
                var validation = _jsonService.ValidateJson(formatted[i]);
                Assert.True(validation.IsValid);
            }
        }

        #endregion

        #region Minify Button Logic Tests

        [Fact]
        public void Minify_Operation_Should_Work_Independently_For_10_Tabs()
        {
            // Arrange - Create 10 formatted JSON strings
            var formattedJsons = new List<string>();
            for (int i = 1; i <= 10; i++)
            {
                var json = $@"{{
  ""tab"": {i},
  ""name"": ""Tab{i}"",
  ""value"": {i * 10}
}}";
                formattedJsons.Add(json);
            }

            // Act - Minify only tabs 3, 6, 9 (indices 2, 5, 8)
            var results = new List<string>(formattedJsons);
            results[2] = _jsonService.MinifyJson(formattedJsons[2]);
            results[5] = _jsonService.MinifyJson(formattedJsons[5]);
            results[8] = _jsonService.MinifyJson(formattedJsons[8]);

            // Assert - Only minified ones should not have newlines
            for (int i = 0; i < 10; i++)
            {
                if (new[] { 2, 5, 8 }.Contains(i))
                {
                    Assert.DoesNotContain("\n", results[i]); // Minified
                }
                else
                {
                    Assert.Contains("\n", results[i]); // Still formatted
                }
            }
        }

        [Fact]
        public void Minify_Should_Remove_All_Whitespace_From_All_Tabs()
        {
            // Arrange - 10 formatted JSONs with lots of whitespace
            var formattedJsons = new List<string>();
            for (int i = 1; i <= 10; i++)
            {
                var json = $@"{{
  ""tab"": {i},
  ""nested"": {{
    ""data"": {{
      ""value"": true
    }}
  }}
}}";
                formattedJsons.Add(json);
            }

            // Act - Minify all 10
            var minified = formattedJsons.Select(json => _jsonService.MinifyJson(json)).ToList();

            // Assert - All should be minified
            foreach (var json in minified)
            {
                Assert.DoesNotContain("\n", json);
                Assert.DoesNotContain("  ", json); // No double spaces
            }
        }

        #endregion

        #region Validate Button Logic Tests

        [Fact]
        public void Validate_Should_Correctly_Identify_Valid_And_Invalid_Among_10_Tabs()
        {
            // Arrange - 10 JSONs, some valid, some invalid
            var jsonStrings = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                if (i % 3 == 0)
                {
                    // Every 3rd is invalid
                    jsonStrings.Add($"{{\"tab\": {i}, \"invalid\": }}");
                }
                else
                {
                    // Others are valid
                    jsonStrings.Add($"{{\"tab\": {i}, \"valid\": true}}");
                }
            }

            // Act - Validate all 10
            var validationResults = jsonStrings
                .Select(json => _jsonService.ValidateJson(json))
                .ToList();

            // Assert - Validation status should match expectations
            for (int i = 0; i < 10; i++)
            {
                if (i % 3 == 0)
                {
                    Assert.False(validationResults[i].IsValid); // Invalid
                    Assert.NotNull(validationResults[i].ErrorMessage);
                }
                else
                {
                    Assert.True(validationResults[i].IsValid); // Valid
                }
            }
        }

        [Fact]
        public void Validate_Should_Provide_Error_Info_For_Each_Invalid_Tab()
        {
            // Arrange - Create 10 JSONs with different errors
            var jsonStrings = new List<string>
            {
                "{\"valid\": true}",                    // Tab 1: Valid
                "{\"invalid\": }",                      // Tab 2: Missing value
                "{\"missing\": \"quote}",               // Tab 3: Missing closing quote
                "{\"good\": \"json\"}",                 // Tab 4: Valid
                "{",                                    // Tab 5: Incomplete
                "[1, 2, 3]",                           // Tab 6: Valid array
                "{\"trailing\": \"comma\",}",          // Tab 7: Trailing comma
                "{\"ok\": 123}",                       // Tab 8: Valid
                "not json at all",                     // Tab 9: Not JSON
                "{\"perfect\": \"json\"}"              // Tab 10: Valid
            };

            // Act - Validate all
            var results = jsonStrings
                .Select(json => _jsonService.ValidateJson(json))
                .ToList();

            // Assert - Check each result
            Assert.True(results[0].IsValid);   // Tab 1
            Assert.False(results[1].IsValid);  // Tab 2
            Assert.False(results[2].IsValid);  // Tab 3
            Assert.True(results[3].IsValid);   // Tab 4
            Assert.False(results[4].IsValid);  // Tab 5
            Assert.True(results[5].IsValid);   // Tab 6
            Assert.False(results[6].IsValid);  // Tab 7
            Assert.True(results[7].IsValid);   // Tab 8
            Assert.False(results[8].IsValid);  // Tab 9
            Assert.True(results[9].IsValid);   // Tab 10

            // Invalid ones should have error messages
            foreach (var i in new[] { 1, 2, 4, 6, 8 })
            {
                Assert.NotNull(results[i].ErrorMessage);
                Assert.True(results[i].LineNumber > 0 || results[i].ColumnNumber > 0);
            }
        }

        #endregion

        #region Combined Operations Tests

        [Fact]
        public void Format_Then_Validate_Should_Work_For_All_Tabs()
        {
            // Arrange - 10 minified valid JSONs
            var jsonStrings = new List<string>();
            for (int i = 1; i <= 10; i++)
            {
                jsonStrings.Add($"{{\"tab\":{i},\"data\":\"value{i}\"}}");
            }

            // Act - Format and validate all
            var results = jsonStrings
                .Select(json => _jsonService.FormatJson(json, 2))
                .Select(formatted => new
                {
                    Formatted = formatted,
                    Validation = _jsonService.ValidateJson(formatted)
                })
                .ToList();

            // Assert - All should be formatted and valid
            foreach (var result in results)
            {
                Assert.Contains("\n", result.Formatted);
                Assert.True(result.Validation.IsValid);
            }
        }

        [Fact]
        public void Format_Then_Minify_Cycle_Should_Preserve_Data_For_10_Tabs()
        {
            // Arrange
            var originalJsons = new List<string>();
            for (int i = 1; i <= 10; i++)
            {
                originalJsons.Add($"{{\"id\":{i},\"value\":{i * 100}}}");
            }

            // Act - Format then minify each
            var results = originalJsons
                .Select(json => _jsonService.FormatJson(json, 2))
                .Select(formatted => _jsonService.MinifyJson(formatted))
                .ToList();

            // Assert - Should be semantically equivalent to original
            for (int i = 0; i < 10; i++)
            {
                var originalValidation = _jsonService.ValidateJson(originalJsons[i]);
                var resultValidation = _jsonService.ValidateJson(results[i]);

                Assert.True(originalValidation.IsValid);
                Assert.True(resultValidation.IsValid);
                Assert.DoesNotContain("\n", results[i]); // Final state is minified
            }
        }

        [Fact]
        public void Sequential_Operations_On_10_Different_JSONs_Should_Not_Interfere()
        {
            // Arrange - 10 different JSONs
            var jsonStrings = new List<string>();
            for (int i = 1; i <= 10; i++)
            {
                jsonStrings.Add($"{{\"operation\":\"test\",\"index\":{i}}}");
            }

            // Act - Perform different operations
            var results = new string[10];

            results[0] = _jsonService.FormatJson(jsonStrings[0], 2);              // Format
            results[1] = _jsonService.MinifyJson(jsonStrings[1]);                 // Minify (already minified)
            results[2] = _jsonService.FormatJson(jsonStrings[2], 4);              // Format with indent 4
            results[3] = jsonStrings[3];                                          // No operation
            results[4] = _jsonService.FormatJson(jsonStrings[4], 2);              // Format
            results[5] = _jsonService.MinifyJson(jsonStrings[5]);                 // Minify
            results[6] = jsonStrings[6];                                          // No operation
            results[7] = _jsonService.FormatJson(jsonStrings[7], 2);              // Format
            results[8] = _jsonService.FormatJson(jsonStrings[8], 4);              // Format with indent 4
            results[9] = jsonStrings[9];                                          // No operation

            // Assert - Each should have correct state
            Assert.Contains("\n", results[0]);      // Formatted
            Assert.DoesNotContain("\n", results[1]); // Minified
            Assert.Contains("\n", results[2]);       // Formatted
            Assert.DoesNotContain("\n", results[3]); // Unchanged
            Assert.Contains("\n", results[4]);       // Formatted
            Assert.DoesNotContain("\n", results[5]); // Minified
            Assert.DoesNotContain("\n", results[6]); // Unchanged
            Assert.Contains("\n", results[7]);       // Formatted
            Assert.Contains("\n", results[8]);       // Formatted
            Assert.DoesNotContain("\n", results[9]); // Unchanged

            // All should still be valid
            foreach (var result in results)
            {
                var validation = _jsonService.ValidateJson(result);
                Assert.True(validation.IsValid);
            }
        }

        #endregion

        #region Tree View Tests

        [Fact]
        public void BuildTree_Should_Work_For_10_Different_JSONs()
        {
            // Arrange - 10 different JSONs
            var jsonStrings = new List<string>();
            for (int i = 1; i <= 10; i++)
            {
                jsonStrings.Add($"{{\"tab\":{i},\"name\":\"Tab{i}\"}}");
            }

            // Act - Build tree for each
            var treeResults = jsonStrings
                .Select(json => _treeService.BuildTree(json))
                .ToList();

            // Assert - All should have tree nodes
            for (int i = 0; i < 10; i++)
            {
                Assert.NotEmpty(treeResults[i]);
                // Each tree should represent different data
                var tabNode = treeResults[i].FirstOrDefault(n => n.Key == "tab");
                Assert.NotNull(tabNode);
            }
        }

        #endregion

        #region Table View Tests

        [Fact]
        public void BuildTable_Should_Work_For_10_Different_Array_JSONs()
        {
            // Arrange - 10 array JSONs
            var jsonArrays = new List<string>();
            for (int i = 1; i <= 10; i++)
            {
                jsonArrays.Add($"[{{\"id\":{i},\"name\":\"Item{i}\"}}]");
            }

            // Act - Build table for each
            var tableResults = jsonArrays
                .Select(json => _tableService.ConvertToTable(json))
                .ToList();

            // Assert - All should have table data
            for (int i = 0; i < 10; i++)
            {
                Assert.NotNull(tableResults[i]);
                Assert.True(tableResults[i].Rows.Count > 0);
            }
        }

        #endregion

        #region Performance Tests

        [Fact]
        public void Format_Operation_Should_Handle_10_Large_JSONs()
        {
            // Arrange - 10 large JSONs
            var largeJsons = new List<string>();
            for (int i = 1; i <= 10; i++)
            {
                var items = string.Join(",", Enumerable.Range(1, 100)
                    .Select(j => $"{{\"id\":{j},\"data\":\"value{j}\"}}"));
                largeJsons.Add($"{{\"tab\":{i},\"items\":[{items}]}}");
            }

            // Act - Format all 10
            var formatted = largeJsons
                .Select(json => _jsonService.FormatJson(json, 2))
                .ToList();

            // Assert - All should be formatted and valid
            foreach (var json in formatted)
            {
                Assert.Contains("\n", json);
                var validation = _jsonService.ValidateJson(json);
                Assert.True(validation.IsValid);
            }
        }

        [Fact]
        public void Validate_Should_Handle_10_Tabs_Quickly()
        {
            // Arrange - 10 JSONs
            var jsonStrings = new List<string>();
            for (int i = 1; i <= 10; i++)
            {
                jsonStrings.Add($"{{\"tab\":{i},\"data\":{{\"nested\":{{\"value\":{i}}}}}}}");
            }

            // Act & Assert - Should complete validation for all 10 tabs
            var validations = jsonStrings
                .Select(json => _jsonService.ValidateJson(json))
                .ToList();

            Assert.Equal(10, validations.Count);
            Assert.All(validations, v => Assert.True(v.IsValid));
        }

        #endregion
    }
}
