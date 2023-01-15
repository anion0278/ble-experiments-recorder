using Autofac;
using AutoMapper;
using CommunityToolkit.Mvvm.Messaging;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Common.Services;
using Mebster.Myodam.DataAccess;
using Mebster.Myodam.DataAccess.DataExport;
using Mebster.Myodam.DataAccess.FileStorage;
using Mebster.Myodam.Infrastructure.Bluetooth;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.UI.WPF.Data.Repositories;
using Mebster.Myodam.UI.WPF.Exception;
using Mebster.Myodam.UI.WPF.ViewModels;
using Mebster.Myodam.UI.WPF.ViewModels.Services;
using Mebster.Myodam.UI.WPF.Views;
using Mebster.Myodam.UI.WPF.Views.Resouces;

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
            builder.RegisterType<MyodamReplyParser>().As<IMyodamReplyParser>().SingleInstance();
            builder.RegisterType<MyodamManagerUiWrapper>().As<IMyodamManager>().SingleInstance();
            builder.RegisterType<MyodamManager>().SingleInstance();
            builder.RegisterType<SynchronizationContextProvider>().As<ISynchronizationContextProvider>().SingleInstance();

            builder.RegisterType<ExperimentsDbContext>().AsSelf();
            builder.RegisterType<TestSubjectRepository>().As<ITestSubjectRepository>();
            builder.RegisterType<MeasurementRepository>().As<IMeasurementRepository>();
            builder.RegisterType<StimulationParametersRepository>().As<IStimulationParametersRepository>();
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
