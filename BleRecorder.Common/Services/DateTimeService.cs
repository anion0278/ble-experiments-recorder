namespace BleRecorder.Common.Services;

public interface IDateTimeService
{
    DateTimeOffset Now { get; }
}

public class DateTimeService : IDateTimeService
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}
