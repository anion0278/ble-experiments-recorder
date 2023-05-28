using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using BleRecorder.UI.WPF.Exception;

namespace BleRecorder.UI.WPF.ViewModels.Services;

/// <summary>
/// Factory for creating <see cref="AsyncRelayCommand"/>s with exception handling.
/// </summary>
public interface IAsyncRelayCommandFactory
{
    /// <summary>
    /// Create a reference to AsyncRelayCommand.
    /// </summary>
    /// <param name="cancelableExecute">The cancelable execution logic.</param>
    /// <returns>AsyncRelayCommand.</returns>
    AsyncRelayCommand Create(Func<CancellationToken, Task> cancelableExecute);

    /// <summary>
    /// Create a reference to AsyncRelayCommand.
    /// </summary>
    /// <param name="cancelableExecute">The cancelable execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    /// <returns>AsyncRelayCommand.</returns>
    AsyncRelayCommand Create(Func<CancellationToken, Task> cancelableExecute, Func<bool> canExecute);

    /// <summary>
    /// Create a reference to AsyncRelayCommand.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <returns>AsyncRelayCommand.</returns>
    AsyncRelayCommand Create(Func<Task> execute);

    /// <summary>
    /// Create a reference to AsyncRelayCommand.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    /// <returns>AsyncRelayCommand.</returns>
    AsyncRelayCommand Create(Func<Task> execute, Func<bool> canExecute);

    /// <summary>
    /// Create a reference to AsyncRelayCommand.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    /// <param name="cancelableExecute">The cancelable execution logic.</param>
    /// <returns>AsyncRelayCommand.</returns>
    AsyncRelayCommand<T> Create<T>(Func<T?, CancellationToken, Task> cancelableExecute);

    /// <summary>
    /// Create a reference to AsyncRelayCommand.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    /// <param name="cancelableExecute">The cancelable execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    /// <returns>AsyncRelayCommand.</returns>
    AsyncRelayCommand<T> Create<T>(Func<T?, CancellationToken, Task> cancelableExecute, Predicate<T?> canExecute);

    /// <summary>
    /// Create a reference to AsyncRelayCommand.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    /// <param name="execute">The execution logic.</param>
    /// <returns>AsyncRelayCommand.</returns>
    AsyncRelayCommand<T> Create<T>(Func<T?, Task> execute);

    /// <summary>
    /// Create a reference to AsyncRelayCommand.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    /// <param name="execute">The execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    /// <returns>AsyncRelayCommand.</returns>
    AsyncRelayCommand<T> Create<T>(Func<T?, Task> execute, Predicate<T?> canExecute);
}

/// <summary>
/// Factory for creating <see cref="AsyncRelayCommand"/>s with exception handling.
/// </summary>
public class AsyncRelayCommandFactory : IAsyncRelayCommandFactory // TODO remove since no longer needed
{
    private readonly IGlobalExceptionHandler _exceptionHandler;

    public AsyncRelayCommandFactory(IGlobalExceptionHandler exceptionHandler) //ILogger<AsyncRelayCommandFactory> logger,
    {
        _exceptionHandler = exceptionHandler;
    }

    public AsyncRelayCommand<T> Create<T>(Func<T?, Task> execute)
        => Register(new AsyncRelayCommand<T>(execute));

    public AsyncRelayCommand<T> Create<T>(Func<T?, CancellationToken, Task> cancelableExecute)
        => Register(new AsyncRelayCommand<T>(cancelableExecute));

    public AsyncRelayCommand<T> Create<T>(Func<T?, Task> execute, Predicate<T?> canExecute)
        => Register(new AsyncRelayCommand<T>(execute, canExecute));

    public AsyncRelayCommand<T> Create<T>(Func<T?, CancellationToken, Task> cancelableExecute, Predicate<T?> canExecute)
        => Register(new AsyncRelayCommand<T>(cancelableExecute, canExecute));

    public AsyncRelayCommand Create(Func<Task> execute)
        => Register(new AsyncRelayCommand(execute));

    public AsyncRelayCommand Create(Func<CancellationToken, Task> cancelableExecute)
        => Register(new AsyncRelayCommand(cancelableExecute));

    public AsyncRelayCommand Create(Func<Task> execute, Func<bool> canExecute)
        => Register(new AsyncRelayCommand(execute, canExecute));

    public AsyncRelayCommand Create(Func<CancellationToken, Task> cancelableExecute, Func<bool> canExecute)
        => Register(new AsyncRelayCommand(cancelableExecute, canExecute));

    private AsyncRelayCommand Register(AsyncRelayCommand command)
    {
        RegisterError(command);
        return command;
    }

    private AsyncRelayCommand<T> Register<T>(AsyncRelayCommand<T> command)
    {
        RegisterError(command);
        return command;
    }

    private void RegisterError(IAsyncRelayCommand command)
    {
        // exception is now handled by the AsyncRelay itself (rethrown to UI thread)

        //command.PropertyChanged += (s, e) =>
        //{
        //    if (s is null)
        //    {
        //        return;
        //    }

        //    if (e.PropertyName == nameof(AsyncRelayCommand.ExecutionTask) &&
        //        ((IAsyncRelayCommand)s).ExecutionTask is Task task &&
        //        task.Exception is AggregateException exception)
        //    {
        //        //_exceptionHandler.HandleException(exception);
        //        
        //    }
        //};
    }
}
