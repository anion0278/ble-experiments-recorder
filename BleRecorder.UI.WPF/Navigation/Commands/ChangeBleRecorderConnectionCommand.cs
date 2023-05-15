using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using BleRecorder.Business.Device;
using BleRecorder.Models.Device;
using BleRecorder.UI.WPF.ViewModels;
using BleRecorder.UI.WPF.ViewModels.Commands;
using BleRecorder.UI.WPF.ViewModels.Services;

namespace BleRecorder.UI.WPF.Navigation.Commands;

internal class ChangeBleRecorderConnectionCommand : CustomAsyncRelayCommand
{
    private readonly INavigationViewModel _viewModel;
    private readonly IBleRecorderManager _bleRecorderManager;
    private readonly IMessageDialogService _dialogService;

    public ChangeBleRecorderConnectionCommand(INavigationViewModel viewModel, IBleRecorderManager bleRecorderManager, IMessageDialogService dialogService) 
    {
        _viewModel = viewModel;
        _bleRecorderManager = bleRecorderManager;
        _dialogService = dialogService;
    }

    protected override AsyncRelayCommand CreateCommand()
    {
        return new AsyncRelayCommand(ChangeBleRecorderConnectionAsync, CanChangeBleRecorderConnection);
    }

    private bool CanChangeBleRecorderConnection()
    {
        return _viewModel.BleRecorderAvailability != BleRecorderAvailabilityStatus.DisconnectedUnavailable && !_bleRecorderManager.IsCurrentlyMeasuring;
    }

    public async Task ChangeBleRecorderConnectionAsync()
    {
        if (_bleRecorderManager.IsCurrentlyMeasuring)
        {
            var result = await _dialogService.ShowOkCancelDialogAsync(
                "Measurement is currently running. Are you sure you want to stop the measurement and disconnect from device?",
                "Disconnect from device?");
            if (result != MessageDialogResult.OK) return;
        }
        if (_bleRecorderManager.BleRecorderDevice != null && _bleRecorderManager.BleRecorderDevice.IsConnected)
        {
            await _bleRecorderManager.BleRecorderDevice.DisconnectAsync();
        }
        else await _bleRecorderManager.ConnectBleRecorderAsync();
    }
}