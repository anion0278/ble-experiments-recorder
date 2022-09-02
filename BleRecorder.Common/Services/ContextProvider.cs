namespace BleRecorder.Common.Services;

public interface ITimerExceptionContextProvider
{
    SynchronizationContext Context { get; }
}

public class TimerExceptionContextProvider : ITimerExceptionContextProvider
{
    public SynchronizationContext Context { get; }

    public TimerExceptionContextProvider()
    {
        Context = SynchronizationContext.Current ?? throw new ArgumentException("Synchronization context initialization was not successful.");
    }
}