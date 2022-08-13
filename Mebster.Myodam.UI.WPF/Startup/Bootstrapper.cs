using Autofac;
using AutoMapper;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Common.Services;
using Mebster.Myodam.DataAccess;
using Mebster.Myodam.DataAccess.FileStorage;
using Mebster.Myodam.Infrastructure.Bluetooth;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.UI.WPF.Data.Repositories;
using Mebster.Myodam.UI.WPF.Exception;
using Mebster.Myodam.UI.WPF.ViewModels;
using Mebster.Myodam.UI.WPF.ViewModels.Services;
using Mebster.Myodam.UI.WPF.Views;
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
            builder.RegisterInstance<IMapper>(SetupMapper());
            builder.RegisterInstance<IMapper>(SetupMapper());

            builder.RegisterType<MainWindow>().AsSelf();
            builder.RegisterType<MainViewModel>().AsSelf();
            builder.RegisterType<NavigationViewModel>().As<INavigationViewModel>();
            builder.RegisterType<TestSubjectDetailViewModel>().Keyed<IDetailViewModel>(nameof(TestSubjectDetailViewModel));
            builder.RegisterType<MeasurementDetailViewModel>().Keyed<IDetailViewModel>(nameof(MeasurementDetailViewModel));
            builder.RegisterType<DeviceCalibrationViewModel>().As<IDeviceCalibrationViewModel>();

            builder.RegisterType<MessageDialogService>().As<IMessageDialogService>().SingleInstance();
            builder.RegisterType<AppCenterIntegration>().As<IAppCenterIntegration>().SingleInstance();
            builder.RegisterType<Logger>().As<ILogger>().SingleInstance();
            builder.RegisterType<GlobalExceptionHandler>().As<IGlobalExceptionHandler>().SingleInstance();
            builder.RegisterType<AsyncRelayCommandFactory>().As<IAsyncRelayCommandFactory>().SingleInstance();
            builder.RegisterType<DateTimeService>().As<IDateTimeService>().SingleInstance();
            builder.RegisterType<AppConfigurationLoader>().As<IAppConfigurationLoader>();

            builder.RegisterType<BluetoothManager>().As<IBluetoothManager>().SingleInstance();
            builder.RegisterType<MyodamMessageParser>().As<IMyodamMessageParser>().SingleInstance();
            builder.RegisterType<MyodamManager>().As<IMyodamManager>().SingleInstance();

            builder.RegisterType<ExperimentsDbContext>().AsSelf();
            builder.RegisterType<TestSubjectRepository>().As<ITestSubjectRepository>();
            builder.RegisterType<MeasurementRepository>().As<IMeasurementRepository>();
            builder.RegisterType<DeviceCalibrationRepository>().As<IDeviceCalibrationRepository>();
            builder.RegisterType<FileManager>().As<IFileManager>();
            builder.RegisterType<JsonManager>().As<IJsonManager>();

            return builder.Build();
        }

        public IMapper SetupMapper()
        {
            var config = new MapperConfiguration(
                cfg => cfg.CreateMap<DeviceMechanicalAdjustments, DeviceMechanicalAdjustments>()
                    .IgnoreAllPropertiesWithAnInaccessibleSetter());
            return config.CreateMapper();
        }
    }
}
