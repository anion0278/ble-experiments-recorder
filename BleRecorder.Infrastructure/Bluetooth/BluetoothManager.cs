using System.Collections.ObjectModel;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;
using BleRecorder.Models.Device;

namespace BleRecorder.Infrastructure.Bluetooth
{
    public interface IBluetoothManager
    {
        public event EventHandler? AvailableDevicesChanged;
        void StartScanning();
        Task<IBleAvailableDeviceWrapper> Connect();
    }

    public class BluetoothManager : IBluetoothManager
    {
        public event EventHandler? AvailableDevicesChanged;

        readonly BluetoothLEAdvertisementWatcher _bleWatcher = new();
        public List<BleAvailableAvailableDeviceWrapperWrapper> AvailableBleDevices { get; } = new();

        private string _bleRecorderName = "Aggregator";

        public void StartScanning()
        {
            _bleWatcher.ScanningMode = BluetoothLEScanningMode.Active;
            _bleWatcher.Received += BleWatcher_Received;

            _bleWatcher.Start();
        }

        private void BleWatcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.Advertisement.LocalName) && args.Advertisement.LocalName.Equals(_bleRecorderName) && AvailableBleDevices.All(d => d.Address != args.BluetoothAddress))
            {
                AvailableBleDevices.Add(new BleAvailableAvailableDeviceWrapperWrapper() { Address = args.BluetoothAddress, Name = args.Advertisement.LocalName, SignalStrength = args.RawSignalStrengthInDBm });
                AvailableDevicesChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public async Task<IBleAvailableDeviceWrapper> Connect()
        {
            var device = AvailableBleDevices.First();
            await device.ConnectDevice();
            return device;
        }
    }


    //public class BleUart
    //{
    //    private readonly GattCharacteristic _rxCharacteristic;

    //    public BleUart(GattCharacteristic rxCharacteristic)
    //    {
    //        _rxCharacteristic = rxCharacteristic;
    //    }
    //}
}