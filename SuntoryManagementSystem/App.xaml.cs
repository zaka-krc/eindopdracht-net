using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using SuntoryManagementSystem.Models;

namespace SuntoryManagementSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Start direct met MainWindow in Guest mode
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
