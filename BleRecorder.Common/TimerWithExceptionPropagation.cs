using System.Drawing;
using System.Timers;
using BleRecorder.Common.Services;
using Timer = System.Timers.Timer;

namespace BleRecorder.Common;

public interface ITimerWithExceptionPropagation
{
    void Start();
    void Enable();

    void StopAndDispose();
}

public class TimerWithExceptionPropagation : ITimerWithExceptionPropagation
{
    private readonly Action _elapsedAction;
    private readonly ISynchronizationContextProvider _contextProvider;
    private readonly Timer _timer;// thread-safe timer

    public TimerWithExceptionPropagation(Action elapsedAction, TimeSpan period, ISynchronizationContextProvider contextProvider, bool AutoReset = true)
    {
        _elapsedAction = elapsedAction;
        _contextProvider = contextProvider;
        _timer = new Timer(period.TotalMilliseconds);
        _timer.AutoReset = AutoReset; // requires manual enabling after each Elapsed event fired
        _timer.Elapsed += _timer_Elapsed;
    }

    private void _timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            _elapsedAction.Invoke();
        }
        catch (Exception exception)
        {
            _contextProvider.RunInContext(() => throw exception);
        }
    }

    public void Enable()
    {
        _timer.Enabled = true;
    }

    public void Start()
    {
        _timer.Start();
    }

    public void StopAndDispose()
    {
        _timer.Stop();
        _timer.Elapsed -= _timer_Elapsed;
        _timer.Dispose();
    }
}