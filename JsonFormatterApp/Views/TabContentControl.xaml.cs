using System.Windows.Controls;

namespace JsonFormatterApp.Views
{
    /// <summary>
    /// Tab content control - contains JSON editor and tree/table views
    /// Binding is handled by AvalonEditBehavior attached property
    /// </summary>
    public partial class TabContentControl : UserControl
    {
        public TabContentControl()
        {
            InitializeComponent();
        }
    }
}
