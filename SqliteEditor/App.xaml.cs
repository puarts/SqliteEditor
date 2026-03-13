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
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Show splash window
            var splash = new SplashWindow();
            splash.Show();

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
