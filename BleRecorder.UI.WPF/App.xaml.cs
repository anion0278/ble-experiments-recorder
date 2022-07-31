using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Windows.Foundation.Metadata;
using Autofac;
using BleRecorder.Business.Device;
using BleRecorder.DataAccess;
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
            // TODO disconnect from devices
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _exceptionHandler.HandleException(e.Exception);
            e.Handled = true;
        }
    }
}
