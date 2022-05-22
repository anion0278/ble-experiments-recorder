using Autofac;
using Mebster.Myodam.DataAccess;
using Mebster.Myodam.UI.WPF.Data.Lookups;
using Mebster.Myodam.UI.WPF.Data.Repositories;
using Mebster.Myodam.UI.WPF.View.Services;
using Mebster.Myodam.UI.WPF.ViewModels;
using Prism.Events;

namespace Mebster.Myodam.UI.WPF.Startup
{
  public class Bootstrapper
  {
    public IContainer Bootstrap()
    {
      var builder = new ContainerBuilder();

      builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();

      builder.RegisterType<ExperimentsDbContext>().AsSelf();

      builder.RegisterType<MainWindow>().AsSelf();

      builder.RegisterType<MessageDialogService>().As<IMessageDialogService>();

      builder.RegisterType<MainViewModel>().AsSelf();
      builder.RegisterType<NavigationViewModel>().As<INavigationViewModel>();
      builder.RegisterType<TestSubjectDetailViewModel>()
        .Keyed<IDetailViewModel>(nameof(TestSubjectDetailViewModel));
      builder.RegisterType<MeasurementDetailViewModel>()
        .Keyed<IDetailViewModel>(nameof(MeasurementDetailViewModel));

      builder.RegisterType<LookupDataService>().AsImplementedInterfaces();
      builder.RegisterType<TestSubjectRepository>().As<ITestSubjectRepository>();
      builder.RegisterType<MeasurementRepository>().As<IMeasurementRepository>();

      return builder.Build();
    }
  }
}
