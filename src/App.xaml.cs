using System;
using System.Windows;

namespace MDPlus
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    MessageBox.Show($"An unexpected error occurred:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                        "MDPlus Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }
    }
}
