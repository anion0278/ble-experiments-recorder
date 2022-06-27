using System.Threading.Tasks;
using System.Windows.Input;

namespace BleRecorder.UI.WPF.ViewModels;


public interface IDetailViewModel
{
    string Title { get; }
    Task LoadAsync(int measurementId, object argsData);
    bool HasChanges { get; }
    int Id { get; }
    public ICommand CloseDetailViewCommand { get; }
}
