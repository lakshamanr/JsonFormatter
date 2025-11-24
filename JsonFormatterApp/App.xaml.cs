using System;
using System.Windows;

namespace JsonFormatterApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Load saved theme preference
            var theme = JsonFormatterApp.Properties.Settings.Default.Theme;
            ApplyTheme(theme);
        }

        public static void ApplyTheme(string themeName)
        {
            var app = Application.Current;
            var themeDict = new ResourceDictionary();

            if (themeName == "Dark")
            {
                themeDict.Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative);
            }
            else
            {
                themeDict.Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative);
            }

            app.Resources.MergedDictionaries.Clear();
            app.Resources.MergedDictionaries.Add(themeDict);

            JsonFormatterApp.Properties.Settings.Default.Theme = themeName;
            JsonFormatterApp.Properties.Settings.Default.Save();
        }
    }
}
