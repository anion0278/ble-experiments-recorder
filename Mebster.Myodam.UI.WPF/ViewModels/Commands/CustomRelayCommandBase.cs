using CommunityToolkit.Mvvm.Input;
using System;
using System.Runtime.CompilerServices;

namespace Mebster.Myodam.UI.WPF.ViewModels.Commands;

public abstract class CustomRelayCommandBase : IRelayCommand
{
    /// <inheritdoc/>
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc/>
    public void NotifyCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public abstract bool CanExecute(object? parameter);

    public abstract void Execute(object? parameter);
}