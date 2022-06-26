namespace Mebster.Myodam.Models.Device;

public interface IBleAvailableDevice
{
    event EventHandler<string> DataReceived;

    ulong Address { get; set; }
    string Name { get; set; }
    short SignalStrength { get; set; }

    Task Send(string msg);

    Task<IBleAvailableDevice> ConnectDevice();

    void Disconnect();
}

public enum MyodamAvailabilityStatus
{
    DisconnectedUnavailable,
    DisconnectedAvailable,
    Connected,
    ErrorOnDevice
}

