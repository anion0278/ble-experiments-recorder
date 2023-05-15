using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Util;
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
using BleRecorder.UI.WPF.Extensions;
using BleRecorder.UI.WPF.Navigation;
using BleRecorder.UI.WPF.Navigation.Commands;
using BleRecorder.UI.WPF.ViewModels;
using BleRecorder.UI.WPF.ViewModels.Services;
using BleRecorder.UI.WPF.Views;
using BleRecorder.UI.WPF.Views.Resouces;

namespace BleRecorder.UI.WPF.Startup
{
    public class Bootstrapper
    {
        public IContainer Bootstrap()
        {
            // TODO implement logging interceptor https://autofac.readthedocs.io/en/latest/advanced/interceptors.html
            var builder = new ContainerBuilder();

            builder.RegisterInstance<IMessenger>(WeakReferenceMessenger.Default);
            builder.RegisterInstance(SetupMapper());

            builder.RegisterType<TestSubjectDetailViewModel>().Keyed<IDetailViewModel>(nameof(TestSubjectDetailViewModel));
            builder.RegisterType<MeasurementDetailViewModel>().Keyed<IDetailViewModel>(nameof(MeasurementDetailViewModel));
            builder.RegisterType<ExperimentsDbContext>().AsSelf();
            builder.RegisterType<MainWindow>().AsSelf();
            builder.RegisterType<MainViewModel>().AsSelf();

            builder.RegisterAsInterfaceSingleton<NavigationViewModel>();
            builder.RegisterAsInterfaceSingleton<DeviceCalibrationViewModel>();
            builder.RegisterAsInterfaceSingleton<NavigationViewModelCommandsFactory>();
            builder.RegisterAsInterfaceSingleton<NavigationItemViewModelFactory>();

            builder.RegisterAsInterfaceSingleton<MessageDialogService>();
            builder.RegisterAsInterfaceSingleton<AppCenterIntegration>();
            builder.RegisterAsInterfaceSingleton<Logger>();
            builder.RegisterAsInterfaceSingleton<AsyncRelayCommandFactory>();
            builder.RegisterAsInterfaceSingleton<DateTimeService>();
            builder.RegisterAsInterfaceSingleton<AppConfigurationLoader>();

            builder.RegisterAsInterfaceSingleton<BluetoothManager>();
            builder.RegisterAsInterfaceSingleton<BleRecorderReplyParser>();
            builder.RegisterAsInterfaceSingleton<BleRecorderManagerUiWrapper>();
            builder.RegisterAsInterfaceSingleton<BleRecorderManager>();
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
