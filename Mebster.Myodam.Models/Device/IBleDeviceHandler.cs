using System.ComponentModel;

namespace Mebster.Myodam.Models.Device;

public interface IBleDeviceHandler : IDisposable
{
    event EventHandler<string> DataReceived;
    event EventHandler? DeviceStatusChanged;

    ulong Address { get; set; }
    string Name { get; set; }
    short SignalStrength { get; set; }
    bool IsConnected { get; }

    Task Send(string msg);

    Task<IBleDeviceHandler> ConnectDevice();

    void Disconnect();
}

public enum MyodamAvailabilityStatus
{
    [Description("Disconnected, unavailable")]
    DisconnectedUnavailable,
    [Description("Disconnected, available")]
    DisconnectedAvailable,
    [Description("Connected")]
    Connected,
    [Description("Error on device!")]
    ErrorOnDevice
}

