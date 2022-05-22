using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BleRecorder.UI.WPF.Data.Lookups;
using BleRecorder.UI.WPF.Event;
using Prism.Events;

namespace BleRecorder.UI.WPF.ViewModels
{
  public class NavigationViewModel : ViewModelBase, INavigationViewModel
  {
    private ITestSubjectLookupDataService _testSubjectLookupService;
    private IEventAggregator _eventAggregator;
    private IMeasurementLookupDataService _measurementLookupService;

    public NavigationViewModel(ITestSubjectLookupDataService testSubjectLookupService,
      IMeasurementLookupDataService measurementLookupService,
      IEventAggregator eventAggregator)
    {
      _testSubjectLookupService = testSubjectLookupService;
      _measurementLookupService = measurementLookupService;
      _eventAggregator = eventAggregator;
      TestSubjects = new ObservableCollection<NavigationItemViewModel>();
      Measurements = new ObservableCollection<NavigationItemViewModel>();
      _eventAggregator.GetEvent<AfterDetailSavedEvent>().Subscribe(AfterDetailSaved);
      _eventAggregator.GetEvent<AfterDetailDeletedEvent>().Subscribe(AfterDetailDeleted);
    }

    public async Task LoadAsync()
    {
      var lookup = await _testSubjectLookupService.GetTestSubjectLookupAsync();
      TestSubjects.Clear();
      foreach (var item in lookup)
      {
        TestSubjects.Add(new NavigationItemViewModel(item.Id, item.DisplayMember,
          nameof(TestSubjectDetailViewModel),
          _eventAggregator));
      }
      lookup = await _measurementLookupService.GetMeasurementLookupAsync();
      Measurements.Clear();
      foreach (var item in lookup)
      {
        Measurements.Add(new NavigationItemViewModel(item.Id, item.DisplayMember,
          nameof(MeasurementDetailViewModel),
          _eventAggregator));
      }
    }

    public ObservableCollection<NavigationItemViewModel> TestSubjects { get; }

    public ObservableCollection<NavigationItemViewModel> Measurements { get; }

    private void AfterDetailDeleted(AfterDetailDeletedEventArgs args)
    {
      switch (args.ViewModelName)
      {
        case nameof(TestSubjectDetailViewModel):
          AfterDetailDeleted(TestSubjects, args);
          break;
        case nameof(MeasurementDetailViewModel):
          AfterDetailDeleted(Measurements, args);
          break;
      }
    }

    private void AfterDetailDeleted(ObservableCollection<NavigationItemViewModel> items,
      AfterDetailDeletedEventArgs args)
    {
      var item = items.SingleOrDefault(f => f.Id == args.Id);
      if (item != null)
      {
        items.Remove(item);
      }
    }

    private void AfterDetailSaved(AfterDetailSavedEventArgs args)
    {
      switch (args.ViewModelName)
      {
        case nameof(TestSubjectDetailViewModel):
          AfterDetailSaved(TestSubjects, args);
          break;
        case nameof(MeasurementDetailViewModel):
          AfterDetailSaved(Measurements, args);
          break;
      }
    }

    private void AfterDetailSaved(ObservableCollection<NavigationItemViewModel> items,
      AfterDetailSavedEventArgs args)
    {
      var lookupItem = items.SingleOrDefault(l => l.Id == args.Id);
      if (lookupItem == null)
      {
        items.Add(new NavigationItemViewModel(args.Id, args.DisplayMember,
          args.ViewModelName,
          _eventAggregator));
      }
      else
      {
        lookupItem.DisplayMember = args.DisplayMember;
      }
    }
  }
}
