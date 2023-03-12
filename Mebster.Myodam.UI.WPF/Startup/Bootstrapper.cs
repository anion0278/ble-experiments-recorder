using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Util;
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
using Mebster.Myodam.UI.WPF.Extensions;
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

            builder.RegisterType<TestSubjectDetailViewModel>().Keyed<IDetailViewModel>(nameof(TestSubjectDetailViewModel));
            builder.RegisterType<MeasurementDetailViewModel>().Keyed<IDetailViewModel>(nameof(MeasurementDetailViewModel));
            builder.RegisterType<ExperimentsDbContext>().AsSelf();
            builder.RegisterType<MainWindow>().AsSelf();
            builder.RegisterType<MainViewModel>().AsSelf();

            builder.RegisterAsInterfaceSingleton<NavigationViewModel>();
            builder.RegisterAsInterfaceSingleton<DeviceCalibrationViewModel>();

            builder.RegisterAsInterfaceSingleton<MessageDialogService>();
            builder.RegisterAsInterfaceSingleton<AppCenterIntegration>();
            builder.RegisterAsInterfaceSingleton<Logger>();
            builder.RegisterAsInterfaceSingleton<AsyncRelayCommandFactory>();
            builder.RegisterAsInterfaceSingleton<DateTimeService>();
            builder.RegisterAsInterfaceSingleton<AppConfigurationLoader>();

            builder.RegisterAsInterfaceSingleton<BluetoothManager>();
            builder.RegisterAsInterfaceSingleton<MyodamReplyParser>();
            builder.RegisterAsInterfaceSingleton<MyodamManagerUiWrapper>();
            builder.RegisterAsInterfaceSingleton<MyodamManager>();
            builder.RegisterAsInterfaceSingleton<SynchronizationContextProvider>();
            builder.RegisterAsInterfaceSingleton<GlobalExceptionHandler>();

            builder.RegisterAsInterface<TestSubjectRepository>();
            builder.RegisterAsInterface<MeasurementRepository>();
            builder.RegisterAsInterface<StimulationParametersRepository>();
            builder.RegisterAsInterface<FileSystemManager>();
            builder.RegisterAsInterface<JsonManager>();
            builder.RegisterAsInterface<ExcelDocumentManager>();

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
