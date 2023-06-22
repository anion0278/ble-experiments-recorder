using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using Autofac;
using Autofac.Util;
using AutoMapper;
using CommunityToolkit.Mvvm.Messaging;
using BleRecorder.Business.Device;
using BleRecorder.Common;
using BleRecorder.Common.Logging;
using BleRecorder.Common.Services;
using BleRecorder.DataAccess;
using BleRecorder.DataAccess.DataExport;
using BleRecorder.DataAccess.FileStorage;
using BleRecorder.DataAccess.Repositories;
using BleRecorder.Infrastructure.Bluetooth;
using BleRecorder.Models.Device;
using BleRecorder.UI.WPF.Calibration;
using BleRecorder.UI.WPF.Exception;
using BleRecorder.UI.WPF.Extensions;
using BleRecorder.UI.WPF.Measurements;
using BleRecorder.UI.WPF.Navigation;
using BleRecorder.UI.WPF.Navigation.Commands;
using BleRecorder.UI.WPF.TestSubjects;
using BleRecorder.UI.WPF.ViewModels;
using BleRecorder.UI.WPF.ViewModels.Services;
using BleRecorder.UI.WPF.Views;
using BleRecorder.UI.WPF.Views.Resouces;
using Microsoft.Extensions.Configuration;
using IConfigurationProvider = Microsoft.Extensions.Configuration.IConfigurationProvider;

namespace BleRecorder.UI.WPF.Startup
{
    public class Bootstrapper
    {
        public IContainer Bootstrap()
        {
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
            builder.RegisterAsInterfaceSingleton<DateTimeService>();
            builder.RegisterAsInterfaceSingleton<AppConfigurationLoader>();

            builder.RegisterAsInterfaceSingleton<BluetoothManager>();
            builder.RegisterAsInterfaceSingleton<BleRecorderReplyParser>();

            RegisterDeviceManagerWithWrapper(builder);

            builder.RegisterAsInterfaceSingleton<SynchronizationContextProvider>();
            builder.RegisterAsInterfaceSingleton<GlobalExceptionHandler>();

            builder.RegisterAsInterface<TestSubjectRepository>();
            builder.RegisterAsInterface<MeasurementRepository>();
            builder.RegisterAsInterface<StimulationParametersRepository>();
            builder.RegisterAsInterface<FileSystemManager>();
            builder.RegisterAsInterface<JsonManager>();
            builder.RegisterAsInterface<ExcelDocumentManager>();

            RegisterConfiguration(builder);

            return builder.Build();
        }

        private static void RegisterConfiguration(ContainerBuilder builder)
        {
            var config = new ConfigurationBuilder().AddUserSecrets<App>().Build();
            builder.RegisterInstance(config.Providers.First());
        }

        private static void RegisterDeviceManagerWithWrapper(ContainerBuilder builder)
        {
            builder.RegisterAsInterfaceSingleton<BleRecorderManagerUiWrapper>();
            builder.RegisterType<BleRecorderManager>();
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
