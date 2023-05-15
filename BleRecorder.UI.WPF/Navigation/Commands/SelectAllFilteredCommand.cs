using System.Linq;
using BleRecorder.UI.WPF.ViewModels;
using BleRecorder.UI.WPF.ViewModels.Commands;
using Swordfish.NET.Collections.Auxiliary;

namespace BleRecorder.UI.WPF.Navigation.Commands;

public class SelectAllFilteredCommand : CustomRelayCommandBase
{
    protected INavigationViewModel ViewModel { get; }

    public SelectAllFilteredCommand(INavigationViewModel viewModel)
    {
        ViewModel = viewModel;
    }

    public override bool CanExecute(object? parameter)
    {
        return true;
    }

    public override void Execute(object? parameter)
    {
        ViewModel.TestSubjectsNavigationItems
            .Cast<NavigationTestSubjectItemViewModel>()
            .ForEach(i => i.IsSelected = true);
    }
}