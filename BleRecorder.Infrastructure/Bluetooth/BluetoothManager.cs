using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;
using BleRecorder.Common;
using BleRecorder.Common.Services;
using BleRecorder.Models.Device;
using Swordfish.NET.Collections;

namespace BleRecorder.Infrastructure.Bluetooth
{
    public interface IBluetoothManager
    {
        ConcurrentObservableCollection<BluetoothDeviceHandler> AvailableBleDevices { get; }
        void StartScanning();
        void AddDeviceNameFilter(string deviceName);
    }

    public class BluetoothManager : IBluetoothManager
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly ISynchronizationContextProvider _contextProvider;
        private readonly TimeSpan _advertisementWatchdogInterval = TimeSpan.FromSeconds(1);
        private readonly TimeSpan _advertisementWatchdogTimeout = TimeSpan.FromSeconds(3);
        private readonly BluetoothLEAdvertisementWatcher _bleWatcher;
        private readonly ITimerWithExceptionPropagation _advertisementWatchdog;
        public ConcurrentObservableCollection<BluetoothDeviceHandler> AvailableBleDevices { get; } = new();

        public BluetoothManager(IDateTimeService dateTimeService, ISynchronizationContextProvider contextProvider)
        {
            _dateTimeService = dateTimeService;
            _contextProvider = contextProvider;

            _bleWatcher = new()
            {
                ScanningMode = BluetoothLEScanningMode.Active,
                SignalStrengthFilter = { OutOfRangeThresholdInDBm = -75 }
            }; // Active - possible Windows bug https://stackoverflow.com/questions/54525137/why-does-bluetoothleadvertisementwatcher-stop-firing-received-events
            
            _advertisementWatchdog = new TimerWithExceptionPropagation(
                OnAdvertisementPeriodElapsed,
                _advertisementWatchdogInterval,
                _contextProvider); // TODO dispose
            _advertisementWatchdog.Start();
        }

        public void AddDeviceNameFilter(string deviceName)
        {
            _bleWatcher.AdvertisementFilter.Advertisement.LocalName = deviceName;
        }

        private void OnAdvertisementPeriodElapsed()
        {
            foreach (var availableBleDevice in AvailableBleDevices.ToArray())
            {
                var now = _dateTimeService.Now;
                if (now - availableBleDevice.LatestTimestamp > _advertisementWatchdogTimeout)
                {
                    Debug.Print($"Advertisement interval timeout: {now - availableBleDevice.LatestTimestamp}");
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
            foreach (var disposedDevice in AvailableBleDevices.Where(d => d.IsDisposed))
            {
                AvailableBleDevices.Remove(disposedDevice);
            }

            var existing = AvailableBleDevices.SingleOrDefault(d => d.Address == args.BluetoothAddress);
            if (existing == null)
            {
                AvailableBleDevices.Add(new BluetoothDeviceHandler(
                    _dateTimeService,
                    _contextProvider,
                    args.Advertisement.LocalName,
                    args.BluetoothAddress,
                    args.RawSignalStrengthInDBm,
                    args.Timestamp));
            }
            else
            {
                existing.LatestTimestamp = args.Timestamp;
            }
        }
    }
}