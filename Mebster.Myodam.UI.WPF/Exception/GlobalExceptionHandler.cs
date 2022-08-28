﻿using System;
using System.Transactions;
using Mebster.Myodam.UI.WPF.ViewModels.Services;
using Microsoft.AppCenter.Crashes;

namespace Mebster.Myodam.UI.WPF.Exception;

public interface IGlobalExceptionHandler
{
    void HandleException(System.Exception exception);
}

public class GlobalExceptionHandler : IGlobalExceptionHandler
{
    private readonly IMessageDialogService _dialogService;
    private readonly IAppCenterIntegration _appCenter;
    private readonly ILogger _logger;

    public GlobalExceptionHandler(IMessageDialogService dialogService, IAppCenterIntegration appCenter, ILogger logger)
    {
        _dialogService = dialogService;
        _appCenter = appCenter;
        _logger = logger;
    }

    public void HandleException(System.Exception exception)
    {
        try
        {
            _dialogService.ShowInfoDialog("Unexpected error occurred." + Environment.NewLine +
                                          exception.Message);
        }
        finally
        {
            _logger.Error(exception);
            _appCenter.TrackException(exception);
        }
    }
}