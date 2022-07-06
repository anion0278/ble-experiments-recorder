using Autofac;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Common.Services;
using Mebster.Myodam.DataAccess;
using Mebster.Myodam.Infrastructure.Bluetooth;
using Mebster.Myodam.UI.WPF.Data.Repositories;
using Mebster.Myodam.UI.WPF.Exception;
using Mebster.Myodam.UI.WPF.View.Services;
using Mebster.Myodam.UI.WPF.ViewModels;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Mebster.Myodam.UI.WPF.Startup
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
            builder.RegisterType<TestSubjectDetailViewModel>().Keyed<IDetailViewModel>(nameof(TestSubjectDetailViewModel));
            builder.RegisterType<MeasurementDetailViewModel>().Keyed<IDetailViewModel>(nameof(MeasurementDetailViewModel));

            builder.RegisterType<GlobalExceptionHandler>().As<IGlobalExceptionHandler>();
            builder.RegisterType<AsyncRelayCommandFactory>().As<IAsyncRelayCommandFactory>();
            builder.RegisterType<DateTimeService>().As<IDateTimeService>();

            builder.RegisterType<BluetoothManager>().As<IBluetoothManager>();
            builder.RegisterType<MyodamManager>().As<IMyodamManager>().SingleInstance();

            builder.RegisterType<TestSubjectRepository>().As<ITestSubjectRepository>();
            builder.RegisterType<MeasurementRepository>().As<IMeasurementRepository>();

            return builder.Build();
        }
    }
}
