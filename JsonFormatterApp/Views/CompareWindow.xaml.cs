using System.Windows;
using System.Windows.Controls;
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
        }
    }
}
