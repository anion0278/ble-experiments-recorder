using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using BleRecorder.UI.WPF.Exception;
using BleRecorder.UI.WPF.Startup;
using BleRecorder.UI.WPF.Views;


namespace BleRecorder.UI.WPF
{
    public partial class App : Application
    {
        private IGlobalExceptionHandler _exceptionHandler = null!;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.Bootstrap();
            _exceptionHandler = container.Resolve<IGlobalExceptionHandler>();
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            var mainWindow = container.Resolve<MainWindow>();
            mainWindow.Show();
        }

        private void Application_OnExit(object sender, ExitEventArgs e)
        {
            DispatcherUnhandledException -= OnDispatcherUnhandledException;
            // TODO disconnect from devices
        }

        private async void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            if (IsAutomationNonCriticalException(e))
            {
                Debug.Print("BleRecorder DataGrid non-fatal exception swallowed.");
                return;
            }

            await _exceptionHandler.HandleExceptionAsync(e.Exception);
        }

        private static bool IsAutomationNonCriticalException(DispatcherUnhandledExceptionEventArgs e)
        {
            // see https://stackoverflow.com/questions/16245732/nullreferenceexception-from-presentationframework-dll/16256740#16256740
            var sourceSite = (e.Exception.TargetSite?.DeclaringType?.FullName ?? string.Empty);
            return e.Exception.Source == "PresentationFramework" && sourceSite == "System.Windows.Automation.Peers.DataGridItemAutomationPeer";
        }
    }
}
