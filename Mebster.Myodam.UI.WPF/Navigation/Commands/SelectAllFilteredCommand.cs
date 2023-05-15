using System.Linq;
using Mebster.Myodam.UI.WPF.ViewModels;
using Mebster.Myodam.UI.WPF.ViewModels.Commands;
using Swordfish.NET.Collections.Auxiliary;

namespace Mebster.Myodam.UI.WPF.Navigation.Commands;

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
            .Cast<NavigationItemViewModel>()
            .ForEach(i => i.IsSelected = true);
    }
}