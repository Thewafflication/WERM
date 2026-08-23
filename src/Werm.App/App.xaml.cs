using System.Windows;

namespace Werm.App
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var window = new MainWindow(ApplicationServices.Create());
            MainWindow = window;
            window.Show();
        }
    }
}
