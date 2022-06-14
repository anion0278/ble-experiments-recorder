using Autofac;
using BleRecorder.Business.Device;
using BleRecorder.DataAccess;
using BleRecorder.Infrastructure.Bluetooth;
using BleRecorder.UI.WPF.Data.Lookups;
using BleRecorder.UI.WPF.Data.Repositories;
using BleRecorder.UI.WPF.View.Services;
using BleRecorder.UI.WPF.ViewModels;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace BleRecorder.UI.WPF.Startup
{
    public class Bootstrapper
    {
        public IContainer Bootstrap()
        {
            // TODO implement logging interceptor https://autofac.readthedocs.io/en/latest/advanced/interceptors.html
            var builder = new ContainerBuilder();

            builder.RegisterInstance<IMessenger>(WeakReferenceMessenger.Default);

            builder.RegisterType<ExperimentsDbContext>().AsSelf();

            builder.RegisterType<MainWindow>().AsSelf();

            builder.RegisterType<MessageDialogService>().As<IMessageDialogService>();

            builder.RegisterType<MainViewModel>().AsSelf();
            builder.RegisterType<NavigationViewModel>().As<INavigationViewModel>();
            builder.RegisterType<TestSubjectDetailViewModel>()
              .Keyed<IDetailViewModel>(nameof(TestSubjectDetailViewModel));
            builder.RegisterType<MeasurementDetailViewModel>()
              .Keyed<IDetailViewModel>(nameof(MeasurementDetailViewModel));

            builder.RegisterType<BluetoothManager>().As<IBluetoothManager>();
            builder.RegisterType<BleRecorderManager>().As<IBleRecorderManager>().SingleInstance();

            builder.RegisterType<LookupDataService>().AsImplementedInterfaces();
            builder.RegisterType<TestSubjectRepository>().As<ITestSubjectRepository>();
            builder.RegisterType<MeasurementRepository>().As<IMeasurementRepository>();

            return builder.Build();
        }
    }
}
