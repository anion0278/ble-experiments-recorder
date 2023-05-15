using System.Linq;
using Mebster.Myodam.UI.WPF.ViewModels;
using Swordfish.NET.Collections.Auxiliary;

namespace Mebster.Myodam.UI.WPF.Navigation.Commands;

public class DeselectAllFilteredCommand : SelectAllFilteredCommand
{
    public DeselectAllFilteredCommand(INavigationViewModel viewModel) : base(viewModel)
    { }

    public override void Execute(object? parameter)
    {
        ViewModel.TestSubjectsNavigationItems
            .Cast<NavigationTestSubjectItemViewModel>()
            .ForEach(i => i.IsSelected = false);
    }
}