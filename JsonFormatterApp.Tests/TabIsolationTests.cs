using JsonFormatterApp.Models;
using JsonFormatterApp.Infrastructure;
using JsonFormatterApp.Services;
using JsonFormatterApp.ViewModels;

namespace JsonFormatterApp.Tests
{
    /// <summary>
    /// Tests to verify that tabs maintain isolated content and operations
    /// </summary>
    public class TabIsolationTests
    {
        private readonly EventAggregator _eventAggregator;
        private readonly JsonService _jsonService;
        private readonly TabManagerViewModel _tabManager;

        public TabIsolationTests()
        {
            _eventAggregator = new EventAggregator();
            _jsonService = new JsonService();
            _tabManager = new TabManagerViewModel(_eventAggregator);

            // Inject event aggregator into TabItem
            TabItem.EventAggregator = _eventAggregator;
        }

        [Fact]
        public void Multiple_Tabs_Should_Maintain_Separate_Content()
        {
            // Arrange - Create 10 tabs with different JSON content
            var tabs = new List<TabItem>();
            for (int i = 1; i <= 10; i++)
            {
                var tab = new TabItem();
                var json = $"{{\"tab\": {i}, \"value\": \"Tab {i} content\"}}";
                tab.SetJsonTextWithoutDirty(json);
                tabs.Add(tab);
            }

            // Act & Assert - Verify each tab maintains its own content
            for (int i = 0; i < 10; i++)
            {
                var expectedContent = $"{{\"tab\": {i + 1}, \"value\": \"Tab {i + 1} content\"}}";
                Assert.Equal(expectedContent, tabs[i].JsonText);
            }
        }

        [Fact]
        public void Each_Tab_Should_Have_Unique_Content_Control()
        {
            // Arrange - Create 10 tabs
            var tabs = new List<TabItem>();
            for (int i = 1; i <= 10; i++)
            {
                tabs.Add(new TabItem());
            }

            // Act & Assert - Verify each tab has a unique Content instance
            var contentControls = tabs.Select(t => t.Content).ToList();
            var distinctCount = contentControls.Distinct().Count();

            Assert.Equal(10, distinctCount); // All 10 should be different objects
        }

        [Fact]
        public void Format_Operation_Should_Only_Affect_Current_Tab()
        {
            // Arrange - Create 3 tabs with different JSON
            var tab1 = new TabItem();
            var tab2 = new TabItem();
            var tab3 = new TabItem();

            var minifiedJson1 = "{\"name\":\"Tab1\",\"value\":1}";
            var minifiedJson2 = "{\"name\":\"Tab2\",\"value\":2}";
            var minifiedJson3 = "{\"name\":\"Tab3\",\"value\":3}";

            tab1.SetJsonTextWithoutDirty(minifiedJson1);
            tab2.SetJsonTextWithoutDirty(minifiedJson2);
            tab3.SetJsonTextWithoutDirty(minifiedJson3);

            // Act - Format only tab2
            var formattedJson = _jsonService.FormatJson(tab2.JsonText, 2);
            tab2.JsonText = formattedJson;

            // Assert - Only tab2 should be formatted (has newlines), others remain minified
            Assert.Equal(minifiedJson1, tab1.JsonText); // Still minified
            Assert.Contains("\n", tab2.JsonText); // Now formatted
            Assert.Equal(minifiedJson3, tab3.JsonText); // Still minified
        }

        [Fact]
        public void Ten_Tabs_Should_Have_Unique_IDs()
        {
            // Arrange & Act - Create 10 tabs
            var tabs = new List<TabItem>();
            for (int i = 1; i <= 10; i++)
            {
                tabs.Add(new TabItem());
            }

            // Assert - All IDs should be unique
            var ids = tabs.Select(t => t.Id).ToList();
            var distinctIds = ids.Distinct().Count();

            Assert.Equal(10, distinctIds);
        }

        [Fact]
        public void Modifying_One_Tab_Should_Not_Affect_Others()
        {
            // Arrange - Create 5 tabs with initial content
            var tabs = new List<TabItem>();
            for (int i = 1; i <= 5; i++)
            {
                var tab = new TabItem();
                tab.SetJsonTextWithoutDirty($"{{\"tab\": {i}}}");
                tabs.Add(tab);
            }

            // Act - Modify only the 3rd tab
            tabs[2].JsonText = "{\"tab\": 3, \"modified\": true}";

            // Assert - Other tabs remain unchanged
            Assert.Equal("{\"tab\": 1}", tabs[0].JsonText);
            Assert.Equal("{\"tab\": 2}", tabs[1].JsonText);
            Assert.Contains("\"modified\": true", tabs[2].JsonText);
            Assert.Equal("{\"tab\": 4}", tabs[3].JsonText);
            Assert.Equal("{\"tab\": 5}", tabs[4].JsonText);
        }

        [Fact]
        public void Tab_IsDirty_Should_Be_Independent()
        {
            // Arrange - Create 3 tabs
            var tab1 = new TabItem();
            var tab2 = new TabItem();
            var tab3 = new TabItem();

            tab1.SetJsonTextWithoutDirty("{}");
            tab2.SetJsonTextWithoutDirty("{}");
            tab3.SetJsonTextWithoutDirty("{}");

            // Act - Mark only tab2 as dirty by modifying it
            tab2.JsonText = "{\"modified\": true}";

            // Assert - Only tab2 should be dirty
            Assert.False(tab1.IsDirty);
            Assert.True(tab2.IsDirty);
            Assert.False(tab3.IsDirty);
        }

        [Fact]
        public void Twenty_Tabs_Should_Maintain_Correct_State()
        {
            // Arrange & Act - Create 20 tabs and set different states
            var tabs = new List<TabItem>();
            for (int i = 1; i <= 20; i++)
            {
                var tab = new TabItem
                {
                    Header = $"Tab {i}",
                    FilePath = i % 2 == 0 ? $"C:\\file{i}.json" : string.Empty
                };
                tab.SetJsonTextWithoutDirty($"{{\"id\": {i}}}");
                tabs.Add(tab);
            }

            // Assert - All tabs maintain their individual state
            for (int i = 0; i < 20; i++)
            {
                Assert.Equal($"Tab {i + 1}", tabs[i].Header);
                Assert.Equal($"{{\"id\": {i + 1}}}", tabs[i].JsonText);

                if ((i + 1) % 2 == 0)
                {
                    Assert.NotEmpty(tabs[i].FilePath);
                }
                else
                {
                    Assert.Empty(tabs[i].FilePath);
                }
            }
        }

        [Fact]
        public void Each_Tab_Should_Have_Independent_TreeNodes()
        {
            // Arrange - Create 3 tabs
            var tab1 = new TabItem();
            var tab2 = new TabItem();
            var tab3 = new TabItem();

            // Act - Set different tree nodes for each
            tab1.TreeNodes.Add(new JsonTreeNode { Key = "Tab1" });
            tab2.TreeNodes.Add(new JsonTreeNode { Key = "Tab2" });
            tab2.TreeNodes.Add(new JsonTreeNode { Key = "Tab2_Second" });

            // Assert - Each tab has its own tree nodes
            Assert.Single(tab1.TreeNodes);
            Assert.Equal("Tab1", tab1.TreeNodes[0].Key);

            Assert.Equal(2, tab2.TreeNodes.Count);
            Assert.Equal("Tab2", tab2.TreeNodes[0].Key);

            Assert.Empty(tab3.TreeNodes);
        }

        [Fact]
        public void SetJsonTextWithoutDirty_Should_Not_Mark_Tab_As_Dirty()
        {
            // Arrange
            var tab = new TabItem();

            // Act
            tab.SetJsonTextWithoutDirty("{\"test\": \"value\"}");

            // Assert
            Assert.Equal("{\"test\": \"value\"}", tab.JsonText);
            Assert.False(tab.IsDirty); // Should not be marked as dirty
        }

        [Fact]
        public void Regular_JsonText_Assignment_Should_Mark_Tab_As_Dirty()
        {
            // Arrange
            var tab = new TabItem();
            tab.SetJsonTextWithoutDirty("{}");

            // Act
            tab.JsonText = "{\"modified\": true}";

            // Assert
            Assert.True(tab.IsDirty); // Should be marked as dirty
        }
    }
}
