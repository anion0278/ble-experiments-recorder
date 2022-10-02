using Autofac;
using AutoMapper;
using CommunityToolkit.Mvvm.Messaging;
using BleRecorder.Business.Device;
using BleRecorder.Common.Services;
using BleRecorder.DataAccess;
using BleRecorder.DataAccess.DataExport;
using BleRecorder.DataAccess.FileStorage;
using BleRecorder.Infrastructure.Bluetooth;
using BleRecorder.Models.Device;
using BleRecorder.UI.WPF.Data.Repositories;
using BleRecorder.UI.WPF.Exception;
using BleRecorder.UI.WPF.ViewModels;
using BleRecorder.UI.WPF.ViewModels.Services;
using BleRecorder.UI.WPF.Views;

namespace BleRecorder.UI.WPF.Startup
{
    public class Bootstrapper
    {
        public IContainer Bootstrap()
        {
            // TODO implement logging interceptor https://autofac.readthedocs.io/en/latest/advanced/interceptors.html
            var builder = new ContainerBuilder();

            builder.RegisterInstance<IMessenger>(WeakReferenceMessenger.Default);
            builder.RegisterInstance<IMapper>(SetupMapper());

            builder.RegisterType<MainWindow>().AsSelf().SingleInstance();
            builder.RegisterType<MainViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<NavigationViewModel>().As<INavigationViewModel>().SingleInstance();
            builder.RegisterType<TestSubjectDetailViewModel>().Keyed<IDetailViewModel>(nameof(TestSubjectDetailViewModel));
            builder.RegisterType<MeasurementDetailViewModel>().Keyed<IDetailViewModel>(nameof(MeasurementDetailViewModel));
            builder.RegisterType<DeviceCalibrationViewModel>().As<IDeviceCalibrationViewModel>().SingleInstance();

            builder.RegisterType<MessageDialogService>().As<IMessageDialogService>().SingleInstance();
            builder.RegisterType<AppCenterIntegration>().As<IAppCenterIntegration>().SingleInstance();
            builder.RegisterType<Logger>().As<ILogger>().SingleInstance();
            builder.RegisterType<GlobalExceptionHandler>().As<IGlobalExceptionHandler>().SingleInstance();
            builder.RegisterType<AsyncRelayCommandFactory>().As<IAsyncRelayCommandFactory>().SingleInstance();
            builder.RegisterType<DateTimeService>().As<IDateTimeService>().SingleInstance();
            builder.RegisterType<AppConfigurationLoader>().As<IAppConfigurationLoader>();

            builder.RegisterType<BluetoothManager>().As<IBluetoothManager>().SingleInstance();
            builder.RegisterType<BleRecorderMessageParser>().As<IBleRecorderMessageParser>().SingleInstance();
            builder.RegisterType<BleRecorderManager>().As<IBleRecorderManager>().SingleInstance();
            builder.RegisterType<SynchronizationContextProvider>().As<ISynchronizationContextProvider>().SingleInstance();

            builder.RegisterType<ExperimentsDbContext>().AsSelf();
            builder.RegisterType<TestSubjectRepository>().As<ITestSubjectRepository>();
            builder.RegisterType<MeasurementRepository>().As<IMeasurementRepository>();
            builder.RegisterType<FileSystemManager>().As<IFileSystemManager>();
            builder.RegisterType<JsonManager>().As<IJsonManager>();
            builder.RegisterType<ExcelDocumentManager>().As<IDocumentManager>();

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
