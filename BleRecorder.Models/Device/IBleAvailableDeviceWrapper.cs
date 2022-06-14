namespace BleRecorder.Models.Device;

public interface IBleAvailableDeviceWrapper
{
    event EventHandler<string> DataReceived;

    ulong Address { get; set; }
    string Name { get; set; }
    short SignalStrength { get; set; }

    Task Send(string msg);

    Task ConnectDevice();

    void Disconnect();
}

public enum DeviceStatus
{
    Unknown,
    DisconnectedAvailable,
    DisconnectedUnavailable,
    Connected,
    ErrorOnDevice
}

