namespace BleRecorder.Common.Services;

public interface ISynchronizationContextProvider
{
    SynchronizationContext Context { get; }
    void RunInContext(Action action);
}

public class SynchronizationContextProvider : ISynchronizationContextProvider
{
    public SynchronizationContext Context { get; }

    public SynchronizationContextProvider()
    {
        Context = SynchronizationContext.Current ?? throw new ArgumentException("Synchronization context initialization was not successful.");
    }

    public void RunInContext(Action action)
    {
        Context.Send(_ => action.Invoke(), null);
    }
}