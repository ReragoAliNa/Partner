using System.Windows;

namespace DesktopPet
{
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Application Crash: {e.Exception.Message}\n\nStack Trace:\n{e.Exception.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true; // Prevent immediate termination if possible
        }
    }
}
