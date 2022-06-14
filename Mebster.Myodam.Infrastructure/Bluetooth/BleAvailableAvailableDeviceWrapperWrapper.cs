using System.Text;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using Mebster.Myodam.Models.Device;

namespace Mebster.Myodam.Infrastructure.Bluetooth;

public class BleAvailableAvailableDeviceWrapperWrapper : IBleAvailableDeviceWrapper
{
    public event EventHandler<string>? DataReceived;
    public event EventHandler<DeviceStatus>? DeviceStatusChanged;

    private BluetoothLEDevice? _device;
    private GattCharacteristic? _rxCharacteristic;

    public ulong Address { get; set; }
    public string Name { get; set; }
    public short SignalStrength { get; set; }

    public async Task ConnectDevice()
    {
        _device = await BluetoothLEDevice.FromBluetoothAddressAsync(Address);
        _device.ConnectionStatusChanged += BleDeviceOnConnectionStatusChanged;

        //var y = bleDevice.DeviceInformation.Pairing.CanPair;
        //var x = bleDevice.ConnectionStatus;
        //var allDeviceServices = (await bleDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached)).Services.ToArray();

        var uartGuid = new Guid("6e400001-b5a3-f393-e0a9-e50e24dcca9e");
        var uartService = (await _device.GetGattServicesForUuidAsync(uartGuid)).Services.Single();

        if ((await _device.GetGattServicesForUuidAsync(uartGuid)).Status != GattCommunicationStatus.Success) throw new Exception("Connection with BLE device failed!");

        var rxId = new Guid("6e400003-b5a3-f393-e0a9-e50e24dcca9e");
        _rxCharacteristic = (await uartService.GetCharacteristicsAsync()).Characteristics.Single(s => s.Uuid == rxId);

        await _rxCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify); // required to receive data
        _rxCharacteristic.ValueChanged += Uart_ReceivedData;
    }

    private void BleDeviceOnConnectionStatusChanged(BluetoothLEDevice sender, object args)
    {
        switch (sender.ConnectionStatus)
        {
            case BluetoothConnectionStatus.Disconnected:
                DeviceStatusChanged?.Invoke(this, DeviceStatus.DisconnectedUnavailable);
                break;
            case BluetoothConnectionStatus.Connected:
                DeviceStatusChanged?.Invoke(this, DeviceStatus.Connected);
                break;
            default:
                DeviceStatusChanged?.Invoke(this, DeviceStatus.ErrorOnDevice);
                break;
        }
    }

    private void Uart_ReceivedData(GattCharacteristic sender, GattValueChangedEventArgs args)
    {
        var reader = DataReader.FromBuffer(args.CharacteristicValue);
        var input = new byte[reader.UnconsumedBufferLength];
        reader.ReadBytes(input);
        var receivedMsg = Encoding.UTF8.GetString(input);

        DataReceived?.Invoke(this, receivedMsg);
    }


    public async Task Send(string msg)
    {

    }

    public void Disconnect()
    {
        _rxCharacteristic.ValueChanged -= Uart_ReceivedData;
        _device.Dispose();
    }
}