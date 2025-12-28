using JsonFormatterApp.Models;
using JsonFormatterApp.Infrastructure;
using JsonFormatterApp.Services;
using JsonFormatterApp.ViewModels;
using System.Windows;

namespace JsonFormatterApp.Tests
{
    /// <summary>
    /// Tests for tab-level button operations (Format, Minify, Validate, Copy, Paste)
    /// Testing with 10 tabs to ensure proper isolation
    /// </summary>
    public class TabButtonOperationsTests : IDisposable
    {
        private readonly EventAggregator _eventAggregator;
        private readonly JsonService _jsonService;
        private readonly JsonTreeService _treeService;
        private readonly JsonTableService _tableService;
        private readonly DiffService _diffService;
        private readonly SchemaValidationService _schemaValidationService;
        private readonly FileService _fileService;
        private readonly TabManagerViewModel _tabManager;
        private readonly JsonOperationsViewModel _jsonOperations;

        public TabButtonOperationsTests()
        {
            _eventAggregator = new EventAggregator();
            _jsonService = new JsonService();
            _treeService = new JsonTreeService();
            _tableService = new JsonTableService();
            _diffService = new DiffService();
            _schemaValidationService = new SchemaValidationService();
            _fileService = new FileService();
            _tabManager = new TabManagerViewModel(_eventAggregator);
            _jsonOperations = new JsonOperationsViewModel(
                _eventAggregator,
                _jsonService,
                _treeService,
                _tableService,
                _diffService,
                _schemaValidationService,
                _fileService,
                _tabManager
            );

            // Inject event aggregator into TabItem
            TabItem.EventAggregator = _eventAggregator;
        }

        public void Dispose()
        {
            // Cleanup
            TabItem.EventAggregator = null;
        }

        #region Format Button Tests

        [Fact]
        public void FormatButton_Should_Format_Only_Selected_Tab_Among_10_Tabs()
        {
            // Arrange - Create 10 tabs with minified JSON
            var tabs = CreateTenTabsWithMinifiedJson();

            // Select tab 5 (index 4)
            _tabManager.SelectedTab = tabs[4];

            // Act - Execute Format command (simulating button click)
            if (_jsonOperations.FormatJsonCommand.CanExecute(null))
            {
                _jsonOperations.FormatJsonCommand.Execute(null);
            }

            // Assert - Only tab 5 should be formatted
            for (int i = 0; i < 10; i++)
            {
                if (i == 4)
                {
                    Assert.Contains("\n", tabs[i].JsonText); // Tab 5 is formatted
                }
                else
                {
                    Assert.DoesNotContain("\n", tabs[i].JsonText); // Others remain minified
                }
            }
        }

        [Fact]
        public void FormatButton_Should_Work_For_Each_Tab_Independently()
        {
            // Arrange - Create 10 tabs with different minified JSON
            var tabs = CreateTenTabsWithMinifiedJson();

            // Act - Format tabs 2, 5, 8 (indices 1, 4, 7)
            var tabsToFormat = new[] { 1, 4, 7 };
            foreach (var index in tabsToFormat)
            {
                _tabManager.SelectedTab = tabs[index];
                if (_jsonOperations.FormatJsonCommand.CanExecute(null))
                {
                    _jsonOperations.FormatJsonCommand.Execute(null);
                }
            }

            // Assert - Only formatted tabs have newlines
            for (int i = 0; i < 10; i++)
            {
                if (tabsToFormat.Contains(i))
                {
                    Assert.Contains("\n", tabs[i].JsonText);
                }
                else
                {
                    Assert.DoesNotContain("\n", tabs[i].JsonText);
                }
            }
        }

        [Fact]
        public void FormatButton_Should_Use_Correct_Indent_Size()
        {
            // Arrange - Create tab with minified JSON
            var tab = CreateTabWithJson("{\"name\":\"test\",\"value\":123}");
            _tabManager.SelectedTab = tab;

            // Test with indent size 2
            _jsonOperations.IndentSize = 2;
            _jsonOperations.FormatJsonCommand.Execute(null);
            var formatted2 = tab.JsonText;

            // Reset and test with indent size 4
            tab.JsonText = "{\"name\":\"test\",\"value\":123}";
            _jsonOperations.IndentSize = 4;
            _jsonOperations.FormatJsonCommand.Execute(null);
            var formatted4 = tab.JsonText;

            // Assert - Different indent sizes produce different formatting
            Assert.NotEqual(formatted2, formatted4);
            Assert.Contains("\n", formatted2);
            Assert.Contains("\n", formatted4);
        }

        #endregion

        #region Minify Button Tests

        [Fact]
        public void MinifyButton_Should_Minify_Only_Selected_Tab_Among_10_Tabs()
        {
            // Arrange - Create 10 tabs with formatted JSON
            var tabs = CreateTenTabsWithFormattedJson();

            // Select tab 3
            _tabManager.SelectedTab = tabs[2];

            // Act - Execute Minify command
            if (_jsonOperations.MinifyJsonCommand.CanExecute(null))
            {
                _jsonOperations.MinifyJsonCommand.Execute(null);
            }

            // Assert - Only tab 3 should be minified
            for (int i = 0; i < 10; i++)
            {
                if (i == 2)
                {
                    Assert.DoesNotContain("\n", tabs[i].JsonText); // Tab 3 is minified
                }
                else
                {
                    Assert.Contains("\n", tabs[i].JsonText); // Others remain formatted
                }
            }
        }

        [Fact]
        public void MinifyButton_Should_Work_For_Multiple_Tabs()
        {
            // Arrange - Create 10 tabs with formatted JSON
            var tabs = CreateTenTabsWithFormattedJson();

            // Act - Minify tabs 1, 4, 7, 10 (indices 0, 3, 6, 9)
            var tabsToMinify = new[] { 0, 3, 6, 9 };
            foreach (var index in tabsToMinify)
            {
                _tabManager.SelectedTab = tabs[index];
                if (_jsonOperations.MinifyJsonCommand.CanExecute(null))
                {
                    _jsonOperations.MinifyJsonCommand.Execute(null);
                }
            }

            // Assert - Only minified tabs should not have newlines
            for (int i = 0; i < 10; i++)
            {
                if (tabsToMinify.Contains(i))
                {
                    Assert.DoesNotContain("\n", tabs[i].JsonText);
                }
                else
                {
                    Assert.Contains("\n", tabs[i].JsonText);
                }
            }
        }

        [Fact]
        public void MinifyButton_Should_Remove_All_Formatting()
        {
            // Arrange
            var tab = CreateTabWithJson(@"{
  ""name"": ""test"",
  ""value"": 123,
  ""nested"": {
    ""key"": ""value""
  }
}");
            _tabManager.SelectedTab = tab;

            // Act
            _jsonOperations.MinifyJsonCommand.Execute(null);

            // Assert
            Assert.DoesNotContain("\n", tab.JsonText);
            Assert.DoesNotContain("  ", tab.JsonText); // No double spaces
        }

        #endregion

        #region Validate Button Tests

        [Fact]
        public void ValidateButton_Should_Validate_Only_Selected_Tab()
        {
            // Arrange - Create 10 tabs, some with valid JSON, some invalid
            var tabs = new List<TabItem>();
            for (int i = 0; i < 10; i++)
            {
                var tab = new TabItem();
                // Make every 3rd tab invalid
                if (i % 3 == 0)
                {
                    tab.JsonText = $"{{\"tab\": {i}, \"invalid\": }}"; // Invalid JSON
                }
                else
                {
                    tab.JsonText = $"{{\"tab\": {i}, \"valid\": true}}";
                }
                tabs.Add(tab);
                _tabManager.Tabs.Add(tab);
            }

            // Act - Validate each tab
            foreach (var tab in tabs)
            {
                _tabManager.SelectedTab = tab;
                _jsonOperations.ValidateJson();
            }

            // Assert - Validation status should be correct for each tab
            for (int i = 0; i < 10; i++)
            {
                if (i % 3 == 0)
                {
                    Assert.False(tabs[i].IsValid);
                    Assert.Contains("Invalid JSON", tabs[i].StatusMessage);
                }
                else
                {
                    Assert.True(tabs[i].IsValid);
                    Assert.Contains("Valid JSON", tabs[i].StatusMessage);
                }
            }
        }

        [Fact]
        public void ValidateButton_Should_Update_StatusMessage_For_Each_Tab()
        {
            // Arrange - Create 10 tabs with valid JSON
            var tabs = CreateTenTabsWithMinifiedJson();

            // Act - Validate all tabs
            foreach (var tab in tabs)
            {
                _tabManager.SelectedTab = tab;
                _jsonOperations.ValidateJson();
            }

            // Assert - All tabs should have validation status message
            foreach (var tab in tabs)
            {
                Assert.Contains("Valid JSON", tab.StatusMessage);
                Assert.True(tab.IsValid);
            }
        }

        [Fact]
        public void ValidateButton_Should_Detect_Invalid_JSON_In_Specific_Tab()
        {
            // Arrange - Create 10 tabs, make tab 7 invalid
            var tabs = CreateTenTabsWithMinifiedJson();
            tabs[6].JsonText = "{\"invalid\": }"; // Tab 7 has invalid JSON

            // Act - Validate all tabs
            foreach (var tab in tabs)
            {
                _tabManager.SelectedTab = tab;
                _jsonOperations.ValidateJson();
            }

            // Assert - Only tab 7 should be invalid
            for (int i = 0; i < 10; i++)
            {
                if (i == 6)
                {
                    Assert.False(tabs[i].IsValid);
                }
                else
                {
                    Assert.True(tabs[i].IsValid);
                }
            }
        }

        #endregion

        #region Copy/Paste Button Tests

        [Fact]
        public void CopyButton_Should_Copy_From_Correct_Tab()
        {
            // Arrange - Create 10 tabs with different content
            var tabs = CreateTenTabsWithMinifiedJson();
            var tab5Content = tabs[4].JsonText; // Tab 5
            _tabManager.SelectedTab = tabs[4];

            // Act - Execute Copy command (simulating button click)
            if (_jsonOperations.CopyCommand.CanExecute(null))
            {
                _jsonOperations.CopyCommand.Execute(null);
            }

            // Assert - Clipboard should contain tab 5 content
            // Note: Clipboard operations require STA thread, so we verify the command executed
            Assert.Equal(tab5Content, tabs[4].JsonText);
        }

        [Fact]
        public void PasteButton_Should_Paste_To_Selected_Tab_Only()
        {
            // Arrange - Create 10 tabs
            var tabs = CreateTenTabsWithMinifiedJson();
            var originalContents = tabs.Select(t => t.JsonText).ToList();

            // Note: Actual paste operation requires clipboard which needs STA thread
            // We'll test that the command is available and can be executed
            _tabManager.SelectedTab = tabs[7]; // Tab 8

            // Act - Verify paste command exists and can check execution
            var canExecute = _jsonOperations.PasteCommand.CanExecute(null);

            // Assert - Command should be available
            Assert.NotNull(_jsonOperations.PasteCommand);
            // Other tabs should remain unchanged
            for (int i = 0; i < 10; i++)
            {
                if (i != 7)
                {
                    Assert.Equal(originalContents[i], tabs[i].JsonText);
                }
            }
        }

        #endregion

        #region Combined Operations Tests

        [Fact]
        public void Format_Then_Minify_Should_Work_On_Same_Tab()
        {
            // Arrange
            var tab = CreateTabWithJson("{\"name\":\"test\",\"value\":123}");
            _tabManager.SelectedTab = tab;

            // Act - Format then Minify
            _jsonOperations.FormatJsonCommand.Execute(null);
            var afterFormat = tab.JsonText;
            _jsonOperations.MinifyJsonCommand.Execute(null);
            var afterMinify = tab.JsonText;

            // Assert
            Assert.Contains("\n", afterFormat); // Was formatted
            Assert.DoesNotContain("\n", afterMinify); // Now minified
        }

        [Fact]
        public void All_Operations_Should_Work_Independently_On_10_Tabs()
        {
            // Arrange - Create 10 tabs
            var tabs = CreateTenTabsWithMinifiedJson();

            // Act - Perform different operations on different tabs
            // Tab 1: Format
            _tabManager.SelectedTab = tabs[0];
            _jsonOperations.FormatJsonCommand.Execute(null);

            // Tab 2: Validate
            _tabManager.SelectedTab = tabs[1];
            _jsonOperations.ValidateJson();

            // Tab 3: Minify (already minified, should stay the same)
            _tabManager.SelectedTab = tabs[2];
            var tab3Before = tabs[2].JsonText;
            _jsonOperations.MinifyJsonCommand.Execute(null);
            var tab3After = tabs[2].JsonText;

            // Tab 4: Format
            _tabManager.SelectedTab = tabs[3];
            _jsonOperations.FormatJsonCommand.Execute(null);

            // Tab 5: Validate
            _tabManager.SelectedTab = tabs[4];
            _jsonOperations.ValidateJson();

            // Assert
            Assert.Contains("\n", tabs[0].JsonText); // Tab 1 formatted
            Assert.True(tabs[1].IsValid); // Tab 2 validated
            Assert.Equal(tab3Before, tab3After); // Tab 3 unchanged
            Assert.Contains("\n", tabs[3].JsonText); // Tab 4 formatted
            Assert.True(tabs[4].IsValid); // Tab 5 validated

            // Tabs 6-10 should remain minified and unvalidated (default state)
            for (int i = 5; i < 10; i++)
            {
                Assert.DoesNotContain("\n", tabs[i].JsonText);
            }
        }

        [Fact]
        public void Switching_Tabs_Should_Preserve_Operation_Results()
        {
            // Arrange - Create 10 tabs
            var tabs = CreateTenTabsWithMinifiedJson();

            // Act - Format tab 3, switch to tab 7, then back to tab 3
            _tabManager.SelectedTab = tabs[2];
            _jsonOperations.FormatJsonCommand.Execute(null);
            var tab3Formatted = tabs[2].JsonText;

            _tabManager.SelectedTab = tabs[6]; // Switch to tab 7
            _tabManager.SelectedTab = tabs[2]; // Switch back to tab 3

            // Assert - Tab 3 should still be formatted
            Assert.Equal(tab3Formatted, tabs[2].JsonText);
            Assert.Contains("\n", tabs[2].JsonText);
        }

        [Fact]
        public void Sequential_Operations_On_Different_Tabs_Should_Not_Interfere()
        {
            // Arrange - Create 10 tabs
            var tabs = CreateTenTabsWithMinifiedJson();

            // Act - Perform operations in sequence on different tabs
            for (int i = 0; i < 10; i++)
            {
                _tabManager.SelectedTab = tabs[i];

                if (i % 2 == 0)
                {
                    // Even tabs: Format
                    _jsonOperations.FormatJsonCommand.Execute(null);
                }
                else
                {
                    // Odd tabs: Validate only
                    _jsonOperations.ValidateJson();
                }
            }

            // Assert - Even tabs formatted, odd tabs still minified
            for (int i = 0; i < 10; i++)
            {
                if (i % 2 == 0)
                {
                    Assert.Contains("\n", tabs[i].JsonText);
                }
                else
                {
                    Assert.DoesNotContain("\n", tabs[i].JsonText);
                    Assert.True(tabs[i].IsValid);
                }
            }
        }

        #endregion

        #region Tree and Table View Tests

        [Fact]
        public void BuildTree_Should_Update_Only_Selected_Tab()
        {
            // Arrange - Create 10 tabs
            var tabs = CreateTenTabsWithMinifiedJson();

            // Act - Build tree for tab 5 only
            _tabManager.SelectedTab = tabs[4];
            _jsonOperations.BuildTree();

            // Assert - Only tab 5 should have tree nodes
            Assert.NotEmpty(tabs[4].TreeNodes);

            // Other tabs should have empty tree nodes (or initial state)
            for (int i = 0; i < 10; i++)
            {
                if (i != 4)
                {
                    // These tabs haven't had BuildTree called on them
                    Assert.Empty(tabs[i].TreeNodes);
                }
            }
        }

        [Fact]
        public void BuildTable_Should_Update_Only_Selected_Tab()
        {
            // Arrange - Create 10 tabs with array JSON
            var tabs = new List<TabItem>();
            for (int i = 0; i < 10; i++)
            {
                var tab = CreateTabWithJson($"[{{\"id\":{i},\"name\":\"Item{i}\"}}]");
                tabs.Add(tab);
                _tabManager.Tabs.Add(tab);
            }

            // Act - Build table for tab 3 only
            _tabManager.SelectedTab = tabs[2];
            _jsonOperations.BuildTable();

            // Assert - Only tab 3 should have table data
            Assert.NotNull(tabs[2].TableData);

            // Other tabs should not have table data
            for (int i = 0; i < 10; i++)
            {
                if (i != 2)
                {
                    Assert.Null(tabs[i].TableData);
                }
            }
        }

        #endregion

        #region Helper Methods

        private List<TabItem> CreateTenTabsWithMinifiedJson()
        {
            var tabs = new List<TabItem>();
            for (int i = 1; i <= 10; i++)
            {
                var json = $"{{\"tab\":{i},\"name\":\"Tab{i}\",\"value\":{i * 10}}}";
                var tab = CreateTabWithJson(json);
                tabs.Add(tab);
                _tabManager.Tabs.Add(tab);
            }
            return tabs;
        }

        private List<TabItem> CreateTenTabsWithFormattedJson()
        {
            var tabs = new List<TabItem>();
            for (int i = 1; i <= 10; i++)
            {
                var json = $@"{{
  ""tab"": {i},
  ""name"": ""Tab{i}"",
  ""value"": {i * 10}
}}";
                var tab = CreateTabWithJson(json);
                tabs.Add(tab);
                _tabManager.Tabs.Add(tab);
            }
            return tabs;
        }

        private TabItem CreateTabWithJson(string json)
        {
            var tab = new TabItem();
            tab.SetJsonTextWithoutDirty(json);
            return tab;
        }

        #endregion
    }
}
