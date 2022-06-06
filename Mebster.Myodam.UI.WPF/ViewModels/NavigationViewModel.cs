using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Mebster.Myodam.UI.WPF.Data.Lookups;
using Mebster.Myodam.UI.WPF.Event;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Mebster.Myodam.UI.WPF.ViewModels
{
    public class NavigationViewModel : ViewModelBase, INavigationViewModel
    {
        private readonly IMessenger _messenger;
        private readonly IMeasurementLookupDataService _measurementLookupService;
        private readonly ITestSubjectLookupDataService _testSubjectLookupService;

        public ObservableCollection<NavigationItemViewModel> TestSubjects { get; } = new();

        public ObservableCollection<NavigationItemViewModel> Measurements { get; } = new();

        public NavigationViewModel(ITestSubjectLookupDataService testSubjectLookupService,
              IMeasurementLookupDataService measurementLookupService,
              IMessenger messenger)
        {
            _testSubjectLookupService = testSubjectLookupService;
            _measurementLookupService = measurementLookupService;
            _messenger = messenger;
            _messenger.Register<AfterDetailSavedEventArgs>(this, (s, e) => AfterDetailSaved(e));
            _messenger.Register<AfterDetailDeletedEventArgs>(this, (s, e) => AfterDetailDeleted(e));
        }

        public async Task LoadAsync()
        {
            var lookup = await _testSubjectLookupService.GetTestSubjectLookupAsync();
            TestSubjects.Clear();
            foreach (var item in lookup)
            {
                TestSubjects.Add(new NavigationItemViewModel(item.Id, item.DisplayMember,
                  nameof(TestSubjectDetailViewModel),
                  _messenger));
            }
            lookup = await _measurementLookupService.GetMeasurementLookupAsync();
            Measurements.Clear();
            foreach (var item in lookup)
            {
                Measurements.Add(new NavigationItemViewModel(item.Id, item.DisplayMember,
                  nameof(MeasurementDetailViewModel),
                  _messenger));
            }
        }

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

        private void AfterDetailSaved(AfterDetailSavedEventArgs args) // TODO refactoring!
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

        private void AfterDetailSaved(ObservableCollection<NavigationItemViewModel> items, AfterDetailSavedEventArgs args)
        {
            var lookupItem = items.SingleOrDefault(l => l.Id == args.Id);
            if (lookupItem == null)
            {
                items.Add(new NavigationItemViewModel(args.Id, args.DisplayMember,
                  args.ViewModelName,
                  _messenger));
            }
            else
            {
                lookupItem.DisplayMember = args.DisplayMember;
            }
        }
    }
}
