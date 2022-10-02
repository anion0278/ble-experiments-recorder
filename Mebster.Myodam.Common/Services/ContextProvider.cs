namespace Mebster.Myodam.Common.Services;

public interface ISynchronizationContextProvider
{
    SynchronizationContext Context { get; }
}

public class SynchronizationContextProvider : ISynchronizationContextProvider
{
    public SynchronizationContext Context { get; }

    public SynchronizationContextProvider()
    {
        Context = SynchronizationContext.Current ?? throw new ArgumentException("Synchronization context initialization was not successful.");
    }
}