using System;
using System.Threading.Tasks;
using System.Windows;

namespace SqliteEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            // Show splash window
            var splash = new SplashWindow();
            splash.Show();

            // Simulate some startup work (load resources, etc.)
            await Task.Delay(800); // keep splash visible briefly

            // Create and show main window
            var main = new MainWindow();
            Current.MainWindow = main;

            // Optionally animate splash out (fade)
            try
            {
                splash.FadeOutAndClose();
            }
            catch
            {
                splash.Close();
            }

            main.Show();
        }
    }
}
