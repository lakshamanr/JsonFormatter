using System.Windows;
using JsonFormatterApp.ViewModels;

namespace JsonFormatterApp.Views
{
    public partial class CompareWindow : Window
    {
        public CompareWindow(string leftTitle, string leftJson, string rightTitle, string rightJson)
        {
            InitializeComponent();

            var viewModel = new CompareViewModel(leftTitle, leftJson, rightTitle, rightJson);
            DataContext = viewModel;

            // Set editor text directly (AvalonEdit doesn't support binding on Text property)
            LeftEditor.Text = leftJson;
            RightEditor.Text = rightJson;
        }
    }
}
