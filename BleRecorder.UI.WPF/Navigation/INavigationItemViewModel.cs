using System.ComponentModel;
using System.Windows.Input;
using BleRecorder.Models.TestSubjects;

namespace BleRecorder.UI.WPF.Navigation;

public interface INavigationItemViewModel : INotifyPropertyChanged // INPC is required to make BindingList propagate the changes
{
    TestSubject Model { get; }
    bool IsSelectedForExport { get; set; }
    string ItemName { get; }
    bool HasErrors { get; }
    int Id { get; }
    ICommand OpenDetailViewCommand { get; }
}