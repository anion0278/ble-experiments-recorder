using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.UI.WPF.ViewModels;
using Mebster.Myodam.UI.WPF.ViewModels.Commands;
using Mebster.Myodam.UI.WPF.ViewModels.Services;

namespace Mebster.Myodam.UI.WPF.Navigation.Commands;

internal class ChangeMyodamConnectionCommand : CustomAsyncRelayCommand
{
    private readonly INavigationViewModel _viewModel;
    private readonly IMyodamManager _myodamManager;
    private readonly IMessageDialogService _dialogService;

    public ChangeMyodamConnectionCommand(INavigationViewModel viewModel, IMyodamManager myodamManager, IMessageDialogService dialogService) 
    {
        _viewModel = viewModel;
        _myodamManager = myodamManager;
        _dialogService = dialogService;
    }

    protected override AsyncRelayCommand CreateAsyncCommand()
    {
        return new AsyncRelayCommand(ChangeMyodamConnectionAsync, CanChangeMyodamConnection);
    }

    private bool CanChangeMyodamConnection()
    {
        return _viewModel.MyodamAvailability != MyodamAvailabilityStatus.DisconnectedUnavailable && !_myodamManager.IsCurrentlyMeasuring;
    }

    public async Task ChangeMyodamConnectionAsync()
    {
        if (_myodamManager.IsCurrentlyMeasuring)
        {
            var result = await _dialogService.ShowOkCancelDialogAsync(
                "Measurement is currently running. Are you sure you want to stop the measurement and disconnect from device?",
                "Disconnect from device?");
            if (result != MessageDialogResult.OK) return;
        }
        if (_myodamManager.MyodamDevice != null && _myodamManager.MyodamDevice.IsConnected)
        {
            await _myodamManager.MyodamDevice.DisconnectAsync();
        }
        else await _myodamManager.ConnectMyodamAsync();
    }
}