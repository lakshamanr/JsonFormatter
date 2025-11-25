using FluentAssertions;
using JsonFormatterApp.ViewModels;
using JsonFormatterApp.Models;
using Xunit;
using System.Linq;

namespace JsonFormatterApp.Tests.ViewModels
{
    /// <summary>
    /// Test cases for MainViewModel multi-tab functionality
    /// Validates tab creation, isolation, formatting, and cleanup
    /// </summary>
    public class MainViewModelMultiTabTests
    {
        [Fact]
        public void MainViewModel_ShouldCreateInitialTab()
        {
            // Arrange & Act
            var viewModel = new MainViewModel();

            // Assert
            viewModel.Tabs.Should().HaveCount(1, "ViewModel should create one initial tab");
            viewModel.SelectedTab.Should().NotBeNull("A tab should be selected by default");
        }

        [Fact]
        public void MainViewModel_NewFile_ShouldCreateNewTab()
        {
            // Arrange
            var viewModel = new MainViewModel();
            var initialCount = viewModel.Tabs.Count;

            // Act
            viewModel.NewFileCommand.Execute(null);

            // Assert
            viewModel.Tabs.Should().HaveCount(initialCount + 1, "new tab should be added");
            viewModel.SelectedTab.Should().Be(viewModel.Tabs.Last(), "newly created tab should be selected");
        }

        [Fact]
        public void MainViewModel_MultipleTabs_ShouldHaveUniqueIds()
        {
            // Arrange
            var viewModel = new MainViewModel();

            // Act
            viewModel.NewFileCommand.Execute(null);
            viewModel.NewFileCommand.Execute(null);

            // Assert
            var ids = viewModel.Tabs.Select(t => t.Id).ToList();
            ids.Should().OnlyHaveUniqueItems("each tab should have a unique ID");
        }

        [Fact]
        public void MainViewModel_FormatJson_ShouldOnlyAffectSelectedTab()
        {
            // Arrange
            var viewModel = new MainViewModel();

            // Create tab 1 with JSON
            viewModel.SelectedTab!.JsonText = "{\"name\":\"tab1\"}";
            var tab1 = viewModel.SelectedTab;

            // Create tab 2 with different JSON
            viewModel.NewFileCommand.Execute(null);
            viewModel.SelectedTab!.JsonText = "{\"name\":\"tab2\"}";
            var tab2 = viewModel.SelectedTab;

            // Select tab 1 and format
            viewModel.SelectedTab = tab1;

            // Act
            viewModel.FormatJsonCommand.Execute(null);

            // Assert
            tab1.JsonText.Should().Contain("\n", "tab1 should be formatted with newlines");
            tab2.JsonText.Should().NotContain("\n", "tab2 should remain unformatted");
        }

        [Fact]
        public void MainViewModel_MinifyJson_ShouldOnlyAffectSelectedTab()
        {
            // Arrange
            var viewModel = new MainViewModel();

            // Create tab 1 with formatted JSON
            viewModel.SelectedTab!.JsonText = "{\n  \"name\": \"tab1\"\n}";
            var tab1 = viewModel.SelectedTab;

            // Create tab 2 with formatted JSON
            viewModel.NewFileCommand.Execute(null);
            viewModel.SelectedTab!.JsonText = "{\n  \"name\": \"tab2\"\n}";
            var tab2 = viewModel.SelectedTab;

            // Select tab 1 and minify
            viewModel.SelectedTab = tab1;

            // Act
            viewModel.MinifyJsonCommand.Execute(null);

            // Assert
            tab1.JsonText.Should().NotContain("\n", "tab1 should be minified");
            tab2.JsonText.Should().Contain("\n", "tab2 should remain formatted");
        }

        [Fact]
        public void MainViewModel_ValidateJson_ShouldOnlyAffectSelectedTab()
        {
            // Arrange
            var viewModel = new MainViewModel();

            // Create tab 1 with valid JSON
            viewModel.SelectedTab!.JsonText = "{\"valid\": true}";
            var tab1 = viewModel.SelectedTab;

            // Create tab 2 with invalid JSON
            viewModel.NewFileCommand.Execute(null);
            viewModel.SelectedTab!.JsonText = "{invalid json}";
            var tab2 = viewModel.SelectedTab;

            // Act - Validate tab 2
            viewModel.ValidateJsonCommand.Execute(null);

            // Assert
            tab2.IsValid.Should().BeFalse("tab2 should be invalid");

            // Now validate tab 1
            viewModel.SelectedTab = tab1;
            viewModel.ValidateJsonCommand.Execute(null);

            // Assert
            tab1.IsValid.Should().BeTrue("tab1 should be valid");
            tab2.IsValid.Should().BeFalse("tab2 should still be invalid");
        }

        [Fact]
        public void MainViewModel_BuildTree_ShouldOnlyAffectSelectedTab()
        {
            // Arrange
            var viewModel = new MainViewModel();

            // Create tab 1 with JSON
            viewModel.SelectedTab!.JsonText = "{\"tab1\": \"data1\"}";
            var tab1 = viewModel.SelectedTab;

            // Create tab 2 with different JSON
            viewModel.NewFileCommand.Execute(null);
            viewModel.SelectedTab!.JsonText = "{\"tab2\": \"data2\"}";
            var tab2 = viewModel.SelectedTab;

            // Act - Build tree for tab 2
            viewModel.BuildTreeCommand.Execute(null);
            var tab2TreeCount = tab2.TreeNodes.Count;

            // Switch to tab 1 and build tree
            viewModel.SelectedTab = tab1;
            viewModel.BuildTreeCommand.Execute(null);

            // Assert
            tab1.TreeNodes.Should().NotBeEmpty("tab1 should have tree nodes");
            tab2.TreeNodes.Should().NotBeEmpty("tab2 should have tree nodes");

            // Trees should be independent
            tab1.TreeNodes.Should().NotBeSameAs(tab2.TreeNodes, "each tab should have its own tree collection");
        }

        [Fact]
        public void MainViewModel_CloseTab_ShouldRemoveTab()
        {
            // Arrange
            var viewModel = new MainViewModel();
            viewModel.NewFileCommand.Execute(null);
            var initialCount = viewModel.Tabs.Count;
            var tabToClose = viewModel.SelectedTab;

            // Act
            viewModel.CloseTabCommand.Execute(tabToClose);

            // Assert
            viewModel.Tabs.Should().HaveCount(initialCount - 1, "tab should be removed");
            viewModel.Tabs.Should().NotContain(tabToClose!, "closed tab should not be in collection");
        }

        [Fact]
        public void MainViewModel_CloseLastTab_ShouldCreateNewTab()
        {
            // Arrange
            var viewModel = new MainViewModel();
            var onlyTab = viewModel.Tabs.First();

            // Act - Close the only tab
            viewModel.CloseTabCommand.Execute(onlyTab);

            // Assert
            viewModel.Tabs.Should().HaveCount(1, "should always have at least one tab");
            viewModel.Tabs.First().Should().NotBe(onlyTab, "should be a new tab");
        }

        [Fact]
        public void MainViewModel_SwitchTabs_ShouldUpdateSelectedTab()
        {
            // Arrange
            var viewModel = new MainViewModel();
            var tab1 = viewModel.SelectedTab;

            viewModel.NewFileCommand.Execute(null);
            var tab2 = viewModel.SelectedTab;

            // Act
            viewModel.SelectedTab = tab1;

            // Assert
            viewModel.SelectedTab.Should().Be(tab1, "selected tab should be updated");

            // Act
            viewModel.SelectedTab = tab2;

            // Assert
            viewModel.SelectedTab.Should().Be(tab2, "selected tab should be updated again");
        }

        [Fact]
        public void MainViewModel_CompareTabsCommand_ShouldRequireTwoOrMoreTabs()
        {
            // Arrange
            var viewModel = new MainViewModel();

            // Act & Assert - Only one tab
            viewModel.CompareTabsCommand.CanExecute(null).Should().BeFalse("cannot compare with only one tab");

            // Add second tab
            viewModel.NewFileCommand.Execute(null);

            // Assert
            viewModel.CompareTabsCommand.CanExecute(null).Should().BeTrue("can compare with two tabs");
        }

        [Fact]
        public void MainViewModel_TabsCollection_ShouldBeObservable()
        {
            // Arrange
            var viewModel = new MainViewModel();
            var collectionChanged = false;

            viewModel.Tabs.CollectionChanged += (sender, args) =>
            {
                collectionChanged = true;
            };

            // Act
            viewModel.NewFileCommand.Execute(null);

            // Assert
            collectionChanged.Should().BeTrue("Tabs collection should notify changes");
        }

        [Fact]
        public void MainViewModel_FormatEmptyJson_ShouldShowMessage()
        {
            // Arrange
            var viewModel = new MainViewModel();
            viewModel.SelectedTab!.JsonText = "";

            // Act
            viewModel.FormatJsonCommand.Execute(null);

            // Assert
            viewModel.SelectedTab.StatusMessage.Should().Contain("No JSON content", "should inform user about empty content");
        }

        [Fact]
        public void MainViewModel_MinifyEmptyJson_ShouldShowMessage()
        {
            // Arrange
            var viewModel = new MainViewModel();
            viewModel.SelectedTab!.JsonText = "   ";

            // Act
            viewModel.MinifyJsonCommand.Execute(null);

            // Assert
            viewModel.SelectedTab.StatusMessage.Should().Contain("No JSON content", "should inform user about empty content");
        }
    }
}
