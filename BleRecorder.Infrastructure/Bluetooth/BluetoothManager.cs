using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;
using BleRecorder.Models.Device;
using Swordfish.NET.Collections;

namespace BleRecorder.Infrastructure.Bluetooth
{
    public interface IBluetoothManager
    {
        ConcurrentObservableCollection<BleAvailableDevice> AvailableBleDevices { get; }
        void StartScanning();
    }

    public class BluetoothManager : IBluetoothManager
    {
        readonly BluetoothLEAdvertisementWatcher _bleWatcher;
        private readonly Timer _timer;
        public ConcurrentObservableCollection<BleAvailableDevice> AvailableBleDevices { get; } = new();

        public BluetoothManager()
        {
            _bleWatcher = new() { ScanningMode = BluetoothLEScanningMode.Active }; // Active - possible Windows bug https://stackoverflow.com/questions/54525137/why-does-bluetoothleadvertisementwatcher-stop-firing-received-events
            _bleWatcher.SignalStrengthFilter.InRangeThresholdInDBm = -70;
            _bleWatcher.SignalStrengthFilter.OutOfRangeThresholdInDBm = -75;
            //_bleWatcher.SignalStrengthFilter.OutOfRangeTimeout = TimeSpan.FromSeconds(4);
            _timer = new Timer(Callback, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        private void Callback(object? state)
        {
            foreach (var availableBleDevice in AvailableBleDevices.ToArray())
            {
                if (DateTimeOffset.Now - availableBleDevice.LatestTimestamp > TimeSpan.FromSeconds(2))
                {
                    AvailableBleDevices.Remove(availableBleDevice);
                }
            }
        }

        public void StartScanning()
        {
            _bleWatcher.ScanningMode = BluetoothLEScanningMode.Active;
            _bleWatcher.Received += BleWatcher_Received;

            _bleWatcher.Start();
        }

        private void BleWatcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.Advertisement.LocalName) && args.Advertisement.LocalName == "Aggregator")
            {
                var now = DateTimeOffset.Now;
                var existing = AvailableBleDevices.SingleOrDefault(d => d.Address == args.BluetoothAddress);
                if (existing == null) AvailableBleDevices.Add(new BleAvailableDevice(args.Advertisement.LocalName, args.BluetoothAddress, args.RawSignalStrengthInDBm, args.Timestamp));
                else
                {
                    existing.LatestTimestamp = args.Timestamp;
                }

               
            }
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