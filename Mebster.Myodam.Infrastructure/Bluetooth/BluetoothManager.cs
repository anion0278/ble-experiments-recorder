using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;
using Mebster.Myodam.Models.Device;
using Swordfish.NET.Collections;

namespace Mebster.Myodam.Infrastructure.Bluetooth
{
    public interface IBluetoothManager
    {
        ConcurrentObservableCollection<BleDeviceHandler> AvailableBleDevices { get; }
        void StartScanning();
    }

    public class BluetoothManager : IBluetoothManager
    {
        readonly BluetoothLEAdvertisementWatcher _bleWatcher;
        private readonly System.Timers.Timer _advertisementWatchdog;
        public ConcurrentObservableCollection<BleDeviceHandler> AvailableBleDevices { get; } = new();

        private TimeSpan _advertisementWatchdogInterval = TimeSpan.FromSeconds(1);
        private TimeSpan _advertisementWatchdogTimeout = TimeSpan.FromSeconds(2);

        public BluetoothManager()
        {
            _bleWatcher = new() { ScanningMode = BluetoothLEScanningMode.Active }; // Active - possible Windows bug https://stackoverflow.com/questions/54525137/why-does-bluetoothleadvertisementwatcher-stop-firing-received-events
            _bleWatcher.SignalStrengthFilter.InRangeThresholdInDBm = -70;
            _bleWatcher.SignalStrengthFilter.OutOfRangeThresholdInDBm = -75;
            _advertisementWatchdog = new System.Timers.Timer(_advertisementWatchdogInterval.TotalMilliseconds); 
            _advertisementWatchdog.Elapsed += OnAdvertisementPeriodElapsed;
            _advertisementWatchdog.Start();
        }

        private void OnAdvertisementPeriodElapsed(object? sender, ElapsedEventArgs elapsedEventArgs)
        {
            foreach (var availableBleDevice in AvailableBleDevices.ToArray())
            {
                if (DateTimeOffset.Now - availableBleDevice.LatestTimestamp > _advertisementWatchdogTimeout)
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
            if (string.IsNullOrEmpty(args.Advertisement.LocalName)) return;

            var existing = AvailableBleDevices.SingleOrDefault(d => d.Address == args.BluetoothAddress);
            if (existing == null)
            {
                AvailableBleDevices.Add(new BleDeviceHandler(args.Advertisement.LocalName, args.BluetoothAddress, args.RawSignalStrengthInDBm, args.Timestamp));
            }
            else
            {
                existing.LatestTimestamp = args.Timestamp;
            }
        }
    }
}