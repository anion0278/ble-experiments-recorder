using System;
using System.Transactions;
using BleRecorder.UI.WPF.ViewModels.Services;
using Microsoft.AppCenter.Crashes;

namespace BleRecorder.UI.WPF.Exception;

public interface IGlobalExceptionHandler
{
    void HandleException(System.Exception exception);
}

public class GlobalExceptionHandler : IGlobalExceptionHandler
{
    private readonly IMessageDialogService _dialogService;
    private readonly IAppCenterIntegration _appCenter;

    public GlobalExceptionHandler(IMessageDialogService dialogService, IAppCenterIntegration appCenter)
    {
        _dialogService = dialogService;
        _appCenter = appCenter;
    }

    public void HandleException(System.Exception exception) 
    {
        _dialogService.ShowInfoDialog("Unexpected error occurred." + Environment.NewLine + exception.Message); // add title  "Unexpected error"
        _appCenter.TrackException(exception);
        // TODO add App Center logging and issue tracking
    }
}