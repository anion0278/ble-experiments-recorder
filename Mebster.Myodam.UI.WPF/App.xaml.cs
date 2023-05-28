using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Autofac;
using Mebster.Myodam.DataAccess;
using Mebster.Myodam.UI.WPF.Exception;
using Mebster.Myodam.UI.WPF.Startup;
using Mebster.Myodam.UI.WPF.Views;


namespace Mebster.Myodam.UI.WPF
{
    public partial class App : Application
    {
        private IGlobalExceptionHandler _exceptionHandler = null!;
        private readonly Mutex _mutex = new Mutex(true, "{Myodam GUI}");

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            CloseAppIfAlreadyRunning();

            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.Bootstrap();
            _exceptionHandler = container.Resolve<IGlobalExceptionHandler>();
            var x = container.Resolve<ExperimentsDbContext>();
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            var mainWindow = container.Resolve<MainWindow>();
            mainWindow.Show();
        }

        private void CloseAppIfAlreadyRunning()
        {
            if (_mutex.WaitOne(TimeSpan.Zero, true)) return;

            // Fixes problem with MessageBox disappearing due to Splash Screen
            // see https://stackoverflow.com/questions/3891719/messagebox-with-exception-details-immediately-disappears-if-use-splash-screen-in
            var splashScreenFixWindow = new Window()
            {
                AllowsTransparency = true,
                ShowInTaskbar = true,
                WindowStyle = WindowStyle.None,
                Background = Brushes.Transparent
            };
            splashScreenFixWindow.Show();

            MessageBox.Show("An instance of application is already running. Only one instance can run at a time.");
            Current.Shutdown(-1);
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
                Debug.Print("Myodam DataGrid non-fatal exception swallowed.");
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
