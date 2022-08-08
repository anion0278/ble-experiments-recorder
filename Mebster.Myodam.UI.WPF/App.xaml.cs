using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Foundation.Metadata;
using Autofac;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.DataAccess;
using Mebster.Myodam.UI.WPF.Exception;
using Mebster.Myodam.UI.WPF.Startup;
using Mebster.Myodam.UI.WPF.Views;

namespace Mebster.Myodam.UI.WPF
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
            // TODO disconnect from devices
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var sourceSite = (e.Exception.TargetSite?.DeclaringType?.FullName ?? string.Empty);
            if (e.Exception.Source == "PresentationFramework" && sourceSite == "System.Windows.Automation.Peers.DataGridItemAutomationPeer")
            {
                // see https://stackoverflow.com/questions/16245732/nullreferenceexception-from-presentationframework-dll/16256740#16256740
                Debug.Print("Myodam DataGrid non-fatal exception swallowed.");
                e.Handled = true;
                return;
            }
            _exceptionHandler.HandleException(e.Exception);
            e.Handled = true;
        }
    }
}
