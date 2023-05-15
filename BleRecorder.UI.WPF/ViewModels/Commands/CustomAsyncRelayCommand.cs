using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace BleRecorder.UI.WPF.ViewModels.Commands;

public abstract class CustomAsyncRelayCommand : IAsyncRelayCommand
{
    private readonly AsyncRelayCommand _innerAsyncCommand;
    public event EventHandler? CanExecuteChanged;
    public event PropertyChangedEventHandler? PropertyChanged;
    public Task? ExecutionTask => _innerAsyncCommand.ExecutionTask;
    public bool CanBeCanceled => _innerAsyncCommand.CanBeCanceled;
    public bool IsCancellationRequested => _innerAsyncCommand.IsCancellationRequested;
    public bool IsRunning => _innerAsyncCommand.IsRunning;

    protected CustomAsyncRelayCommand()
    {
        _innerAsyncCommand = CreateCommand();
        _innerAsyncCommand.CanExecuteChanged += (s, e) => CanExecuteChanged?.Invoke(s, e);
        _innerAsyncCommand.PropertyChanged += (s, e) => PropertyChanged?.Invoke(s, e);
    }

    protected abstract AsyncRelayCommand CreateCommand();

    public bool CanExecute(object? parameter)
    {
        return _innerAsyncCommand.CanExecute(parameter);
    }

    public void Execute(object? parameter)
    {
        _innerAsyncCommand.Execute(parameter);
    }

    public void NotifyCanExecuteChanged()
    {
        _innerAsyncCommand.NotifyCanExecuteChanged();
    }

    public Task ExecuteAsync(object? parameter)
    {
        return _innerAsyncCommand.ExecuteAsync(parameter);
    }

    public void Cancel()
    {
        _innerAsyncCommand.Cancel();
    }
}