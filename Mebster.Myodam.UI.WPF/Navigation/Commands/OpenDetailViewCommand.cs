using CommunityToolkit.Mvvm.Messaging;
using Mebster.Myodam.UI.WPF.Event;
using Mebster.Myodam.UI.WPF.TestSubjects;
using Mebster.Myodam.UI.WPF.ViewModels;

namespace Mebster.Myodam.UI.WPF.Navigation.Commands;

public class OpenDetailViewCommand : SelectAllFilteredCommand
{
    private readonly IMessenger _messenger;

    public OpenDetailViewCommand(INavigationViewModel viewModel, IMessenger messenger) : base(viewModel)
    {
        _messenger = messenger;
    }

    public override void Execute(object? parameter)
    {
        _messenger.Send(new OpenDetailViewEventArgs
        {
            Id = -999, // for new item
            ViewModelName = nameof(TestSubjectDetailViewModel)
        });
    }
}