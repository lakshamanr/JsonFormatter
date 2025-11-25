using FluentAssertions;
using JsonFormatterApp.Models;
using Xunit;

namespace JsonFormatterApp.Tests.Models
{
    /// <summary>
    /// Test cases for TabItem model - validates multi-tab isolation and state management
    /// </summary>
    public class TabItemTests
    {
        [Fact]
        public void TabItem_ShouldHaveUniqueId()
        {
            // Arrange & Act
            var tab1 = new TabItem();
            var tab2 = new TabItem();

            // Assert
            tab1.Id.Should().NotBe(tab2.Id, "each tab should have a unique GUID");
        }

        [Fact]
        public void TabItem_ShouldStartWithDefaultValues()
        {
            // Arrange & Act
            var tab = new TabItem();

            // Assert
            tab.Header.Should().Be("Untitled", "new tabs should have default header");
            tab.JsonText.Should().BeEmpty("new tabs should have empty JSON text");
            tab.FilePath.Should().BeEmpty("new tabs should have no file path");
            tab.IsDirty.Should().BeFalse("new tabs should not be marked as dirty");
            tab.IsValid.Should().BeTrue("new tabs should be valid by default");
            tab.StatusMessage.Should().Be("Ready", "new tabs should show Ready status");
        }

        [Fact]
        public void TabItem_SetJsonText_ShouldMarkAsDirty()
        {
            // Arrange
            var tab = new TabItem();
            tab.IsDirty = false;

            // Act
            tab.JsonText = "{\"test\": true}";

            // Assert
            tab.IsDirty.Should().BeTrue("setting JSON text should mark tab as dirty");
        }

        [Fact]
        public void TabItem_SetJsonTextWithoutDirty_ShouldNotMarkAsDirty()
        {
            // Arrange
            var tab = new TabItem();
            tab.IsDirty = false;

            // Act
            tab.SetJsonTextWithoutDirty("{\"test\": true}");

            // Assert
            tab.IsDirty.Should().BeFalse("SetJsonTextWithoutDirty should not mark tab as dirty");
            tab.JsonText.Should().Be("{\"test\": true}", "JSON text should still be set");
        }

        [Fact]
        public void TabItem_SetFilePath_ShouldUpdateHeader()
        {
            // Arrange
            var tab = new TabItem();

            // Act
            tab.FilePath = @"C:\test\sample.json";

            // Assert
            tab.Header.Should().Be("sample.json", "header should show file name");
        }

        [Fact]
        public void TabItem_SetFilePath_WhenDirty_ShouldShowAsterisk()
        {
            // Arrange
            var tab = new TabItem();
            tab.FilePath = @"C:\test\sample.json";

            // Act
            tab.IsDirty = true;

            // Assert
            tab.Header.Should().Be("sample.json*", "dirty file should show asterisk");
        }

        [Fact]
        public void TabItem_MultipleInstances_ShouldBeIndependent()
        {
            // Arrange
            var tab1 = new TabItem();
            var tab2 = new TabItem();

            // Act
            tab1.JsonText = "{\"tab1\": true}";
            tab1.IsValid = false;
            tab1.StatusMessage = "Error in tab1";

            tab2.JsonText = "{\"tab2\": true}";
            tab2.IsValid = true;
            tab2.StatusMessage = "Tab2 is valid";

            // Assert
            tab1.JsonText.Should().Be("{\"tab1\": true}", "tab1 should have its own JSON");
            tab2.JsonText.Should().Be("{\"tab2\": true}", "tab2 should have its own JSON");

            tab1.IsValid.Should().BeFalse("tab1 should be invalid");
            tab2.IsValid.Should().BeTrue("tab2 should be valid");

            tab1.StatusMessage.Should().Be("Error in tab1", "tab1 should have its own status");
            tab2.StatusMessage.Should().Be("Tab2 is valid", "tab2 should have its own status");
        }

        [Fact]
        public void TabItem_SetJsonTextWithoutDirty_ShouldHandleNullSafely()
        {
            // Arrange
            var tab = new TabItem();

            // Act
            Action act = () => tab.SetJsonTextWithoutDirty(null!);

            // Assert - should not throw, should handle gracefully
            act.Should().NotThrow();
        }

        [Fact]
        public void TabItem_PropertyChangedEvent_ShouldFireForJsonText()
        {
            // Arrange
            var tab = new TabItem();
            var propertyChangedFired = false;
            string? changedPropertyName = null;

            tab.PropertyChanged += (sender, args) =>
            {
                propertyChangedFired = true;
                changedPropertyName = args.PropertyName;
            };

            // Act
            tab.JsonText = "{\"test\": true}";

            // Assert
            propertyChangedFired.Should().BeTrue("PropertyChanged event should fire");
            changedPropertyName.Should().Be("JsonText", "event should indicate JsonText changed");
        }

        [Fact]
        public void TabItem_TreeNodes_ShouldBeIndependentBetweenTabs()
        {
            // Arrange
            var tab1 = new TabItem();
            var tab2 = new TabItem();

            var node1 = new JsonTreeNode { Key = "tab1", Value = "data1" };
            var node2 = new JsonTreeNode { Key = "tab2", Value = "data2" };

            // Act
            tab1.TreeNodes.Add(node1);
            tab2.TreeNodes.Add(node2);

            // Assert
            tab1.TreeNodes.Should().HaveCount(1, "tab1 should have one node");
            tab2.TreeNodes.Should().HaveCount(1, "tab2 should have one node");

            tab1.TreeNodes[0].Key.Should().Be("tab1", "tab1 should have its own data");
            tab2.TreeNodes[0].Key.Should().Be("tab2", "tab2 should have its own data");
        }
    }
}
