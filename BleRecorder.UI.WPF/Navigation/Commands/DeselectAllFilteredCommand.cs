using System.Linq;
using BleRecorder.UI.WPF.ViewModels;
using Swordfish.NET.Collections.Auxiliary;

namespace BleRecorder.UI.WPF.Navigation.Commands;

public class DeselectAllFilteredCommand : SelectAllFilteredCommand
{
    public DeselectAllFilteredCommand(INavigationViewModel viewModel) : base(viewModel)
    { }

    public override void Execute(object? parameter)
    {
        ViewModel.TestSubjectsNavigationItems
            .Cast<NavigationItemViewModel>()
            .ForEach(i => i.IsSelectedForExport = false);
    }
}