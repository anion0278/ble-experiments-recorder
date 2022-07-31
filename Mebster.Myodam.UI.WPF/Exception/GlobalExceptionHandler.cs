using System;
using System.Transactions;
using Mebster.Myodam.UI.WPF.Views.Services;

namespace Mebster.Myodam.UI.WPF.Exception;

public interface IGlobalExceptionHandler
{
    void HandleException(System.Exception exception);
}

public class GlobalExceptionHandler : IGlobalExceptionHandler
{
    private readonly IMessageDialogService _dialogService;

    public GlobalExceptionHandler(IMessageDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public void HandleException(System.Exception exception) 
    {
        _dialogService.ShowInfoDialogAsync("Unexpected error occurred." + Environment.NewLine + exception.Message); // add title  "Unexpected error"
        // TODO add App Center logging and issue tracking
    }
}