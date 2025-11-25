using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace JsonFormatterApp.Views
{
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();
            ShowQuickStart_Click(null, null);
        }

        private void ShowQuickStart_Click(object? sender, RoutedEventArgs? e)
        {
            ContentPanel.Children.Clear();
            AddHeading("🚀 Quick Start Guide");

            AddParagraph("Welcome to JSON Formatter Pro! This quick start guide will help you get started with the most common tasks.");

            AddSubheading("Opening JSON Files");
            AddBullet("Click File → Open (or press Ctrl+O) to open a JSON file");
            AddBullet("Recent files appear in the File menu for quick access");
            AddBullet("Drag and drop files directly into the editor");

            AddSubheading("Working with Multiple Files");
            AddBullet("Press Ctrl+N to create a new tab");
            AddBullet("Each tab has its own editor, tree view, and table view");
            AddBullet("Close tabs with the X button or Ctrl+W");
            AddBullet("Switch between tabs by clicking on them");

            AddSubheading("Formatting & Validation");
            AddBullet("Press Ctrl+F to format (prettify) your JSON");
            AddBullet("Press Ctrl+M to minify your JSON");
            AddBullet("Validation happens automatically as you type");
            AddBullet("Errors are shown in the status bar with line and column numbers");

            AddSubheading("Viewing JSON Structure");
            AddBullet("Tree View: See your JSON as a hierarchical tree structure");
            AddBullet("Table View: View JSON arrays as sortable data tables");
            AddBullet("Both views update automatically as you edit");
        }

        private void ShowKeyboardShortcuts_Click(object? sender, RoutedEventArgs? e)
        {
            ContentPanel.Children.Clear();
            AddHeading("⌨️ Keyboard Shortcuts");

            AddSubheading("File Operations");
            AddShortcut("Ctrl+N", "Create new tab");
            AddShortcut("Ctrl+O", "Open file");
            AddShortcut("Ctrl+S", "Save file");
            AddShortcut("Ctrl+Shift+S", "Save as");
            AddShortcut("Ctrl+W", "Close current tab");

            AddSubheading("Editing");
            AddShortcut("Ctrl+F", "Format JSON");
            AddShortcut("Ctrl+M", "Minify JSON");
            AddShortcut("Ctrl+C", "Copy to clipboard");
            AddShortcut("Ctrl+V", "Paste from clipboard");

            AddSubheading("View");
            AddShortcut("Ctrl+T", "Toggle Dark/Light theme");

            AddSubheading("Advanced");
            AddParagraph("All menu items show their keyboard shortcuts. Hover over toolbar buttons to see tooltips with shortcuts.");
        }

        private void ShowFeatures_Click(object? sender, RoutedEventArgs? e)
        {
            ContentPanel.Children.Clear();
            AddHeading("📝 Features Guide");

            AddSubheading("1. JSON Formatting");
            AddParagraph("Format JSON with customizable indentation (2 or 4 spaces). The formatter preserves your data while making it human-readable.");

            AddSubheading("2. JSON Validation");
            AddParagraph("Real-time validation shows errors as you type. Error messages include exact line and column numbers for easy debugging.");

            AddSubheading("3. Tree View Explorer");
            AddParagraph("Visualize JSON structure as a collapsible tree. Perfect for understanding complex nested JSON documents.");

            AddSubheading("4. Table View");
            AddParagraph("View JSON arrays as sortable, filterable data tables. Great for analyzing data sets and finding patterns.");

            AddSubheading("5. JSON Schema Validation");
            AddParagraph("Validate JSON against JSON Schema definitions. Ensure your data meets specific structural requirements.");

            AddSubheading("6. Diff & Compare");
            AddParagraph("Compare JSON files or tabs side-by-side. See differences highlighted for easy comparison.");

            AddSubheading("7. Multi-Tab Interface");
            AddParagraph("Work with multiple JSON files simultaneously. Each tab maintains its own state, editor, and views.");

            AddSubheading("8. Dark & Light Themes");
            AddParagraph("Switch between professional dark and light themes. Easy on the eyes for long editing sessions.");
        }

        private void ShowConversion_Click(object? sender, RoutedEventArgs? e)
        {
            ContentPanel.Children.Clear();
            AddHeading("🔧 Conversion Tools");

            AddSubheading("JSON to XML");
            AddParagraph("Convert JSON to XML format. Useful for integration with XML-based systems.");

            AddSubheading("JSON to C# Classes");
            AddParagraph("Generate C# class definitions from your JSON structure. Perfect for creating DTOs and models.");

            AddSubheading("JSON to SQL");
            AddParagraph("Generate SQL INSERT statements from JSON arrays. Simplifies database population.");

            AddSubheading("JSON to/from YAML");
            AddParagraph("Convert between JSON and YAML formats. Both directions supported.");

            AddSubheading("Encoding Tools");
            AddParagraph("Base64 Encode/Decode: Encode JSON to Base64 or decode Base64 strings.");
            AddParagraph("URL Encode/Decode: Prepare JSON for URL transmission or decode URL-encoded JSON.");

            AddNote("Note: All conversions create new windows with the result. You can copy the result to use in your projects.");
        }

        private void ShowFAQ_Click(object? sender, RoutedEventArgs? e)
        {
            ContentPanel.Children.Clear();
            AddHeading("❓ Frequently Asked Questions");

            AddSubheading("Q: How large can JSON files be?");
            AddParagraph("A: The application supports files up to 50MB. Larger files may impact performance.");

            AddSubheading("Q: Can I compare two tabs?");
            AddParagraph("A: Yes! Use Tools → Compare Tabs or click the Compare button. Select two tabs to see differences side-by-side.");

            AddSubheading("Q: Why is my JSON showing as invalid?");
            AddParagraph("A: Check the status bar for the exact error location. Common issues include missing commas, quotes, or brackets.");

            AddSubheading("Q: How do I change the indent size?");
            AddParagraph("A: Use the Indent dropdown in the toolbar to select 2 or 4 spaces.");

            AddSubheading("Q: Can I save my theme preference?");
            AddParagraph("A: Yes! Your theme choice (Dark/Light) is automatically saved and restored when you reopen the application.");

            AddSubheading("Q: What JSON features are supported?");
            AddParagraph("A: All standard JSON features including objects, arrays, strings, numbers, booleans, and null values.");

            AddSubheading("Q: Is my data secure?");
            AddParagraph("A: All processing happens locally on your computer. No data is sent to external servers.");

            AddSubheading("Q: How do I report bugs or request features?");
            AddParagraph("A: Contact us at support@jsonformatterpro.com with your feedback.");
        }

        private void AddHeading(string text)
        {
            var tb = new TextBlock
            {
                Text = text,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)FindResource("ForegroundBrush")),
                Margin = new Thickness(0, 0, 0, 20)
            };
            ContentPanel.Children.Add(tb);
        }

        private void AddSubheading(string text)
        {
            var tb = new TextBlock
            {
                Text = text,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)FindResource("ForegroundBrush")),
                Margin = new Thickness(0, 15, 0, 10)
            };
            ContentPanel.Children.Add(tb);
        }

        private void AddParagraph(string text)
        {
            var tb = new TextBlock
            {
                Text = text,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush((Color)FindResource("ForegroundBrush")),
                Opacity = 0.9,
                Margin = new Thickness(0, 5, 0, 10)
            };
            ContentPanel.Children.Add(tb);
        }

        private void AddBullet(string text)
        {
            var tb = new TextBlock
            {
                Text = "• " + text,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush((Color)FindResource("ForegroundBrush")),
                Opacity = 0.9,
                Margin = new Thickness(20, 3, 0, 3)
            };
            ContentPanel.Children.Add(tb);
        }

        private void AddShortcut(string keys, string description)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(20, 5, 0, 5)
            };

            var keyBorder = new Border
            {
                Background = new SolidColorBrush((Color)FindResource("PanelHeaderBrush")),
                BorderBrush = new SolidColorBrush((Color)FindResource("BorderBrush")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(8, 4),
                Margin = new Thickness(0, 0, 15, 0)
            };

            var keyText = new TextBlock
            {
                Text = keys,
                FontFamily = new FontFamily("Consolas"),
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)FindResource("ForegroundBrush")),
                MinWidth = 100
            };

            keyBorder.Child = keyText;
            panel.Children.Add(keyBorder);

            var descText = new TextBlock
            {
                Text = description,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)FindResource("ForegroundBrush")),
                VerticalAlignment = VerticalAlignment.Center
            };

            panel.Children.Add(descText);
            ContentPanel.Children.Add(panel);
        }

        private void AddNote(string text)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(40, 0, 122, 204)),
                BorderBrush = new SolidColorBrush((Color)FindResource("AccentBrush")),
                BorderThickness = new Thickness(1, 1, 1, 1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(15, 10),
                Margin = new Thickness(0, 10, 0, 10)
            };

            var tb = new TextBlock
            {
                Text = text,
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush((Color)FindResource("ForegroundBrush"))
            };

            border.Child = tb;
            ContentPanel.Children.Add(border);
        }
    }
}
