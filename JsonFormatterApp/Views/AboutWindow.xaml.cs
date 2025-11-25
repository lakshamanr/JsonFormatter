using System;
using System.Reflection;
using System.Windows;

namespace JsonFormatterApp.Views
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            LoadVersionInfo();
        }

        private void LoadVersionInfo()
        {
            try
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                VersionText.Text = $"Version {version?.Major}.{version?.Minor}.{version?.Build} Professional";
            }
            catch
            {
                VersionText.Text = "Version 2.0.0 Professional";
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SystemInfo_Click(object sender, RoutedEventArgs e)
        {
            var sysInfo = new System.Text.StringBuilder();
            sysInfo.AppendLine("=== SYSTEM INFORMATION ===");
            sysInfo.AppendLine();
            sysInfo.AppendLine($"Operating System: {Environment.OSVersion}");
            sysInfo.AppendLine($"64-bit OS: {Environment.Is64BitOperatingSystem}");
            sysInfo.AppendLine($"64-bit Process: {Environment.Is64BitProcess}");
            sysInfo.AppendLine($"Processor Count: {Environment.ProcessorCount}");
            sysInfo.AppendLine($"CLR Version: {Environment.Version}");
            sysInfo.AppendLine($"Machine Name: {Environment.MachineName}");
            sysInfo.AppendLine($"User Name: {Environment.UserName}");
            sysInfo.AppendLine($"System Directory: {Environment.SystemDirectory}");
            sysInfo.AppendLine($"Current Directory: {Environment.CurrentDirectory}");
            sysInfo.AppendLine();
            sysInfo.AppendLine($"Application Version: {Assembly.GetExecutingAssembly().GetName().Version}");
            sysInfo.AppendLine($"Framework: .NET 8.0");

            MessageBox.Show(
                sysInfo.ToString(),
                "System Information",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }
}
