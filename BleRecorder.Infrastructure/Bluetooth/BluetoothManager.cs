using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;
using BleRecorder.Common.Services;
using BleRecorder.Models.Device;
using Swordfish.NET.Collections;

namespace BleRecorder.Infrastructure.Bluetooth
{
    public interface IBluetoothManager
    {
        ConcurrentObservableCollection<BleDeviceHandler> AvailableBleDevices { get; }
        void StartScanning();
        void AddDeviceNameFilter(string deviceName);
    }

    public class BluetoothManager : IBluetoothManager
    {
        private readonly IDateTimeService _dateTimeService;
        readonly BluetoothLEAdvertisementWatcher _bleWatcher;
        private readonly System.Timers.Timer _advertisementWatchdog;
        public ConcurrentObservableCollection<BleDeviceHandler> AvailableBleDevices { get; } = new();

        private TimeSpan _advertisementWatchdogInterval = TimeSpan.FromSeconds(1);
        private TimeSpan _advertisementWatchdogTimeout = TimeSpan.FromSeconds(3);

        public BluetoothManager(IDateTimeService dateTimeService)
        {
            _dateTimeService = dateTimeService;
            // DeviceInformation.Pairing.IsPaired;
            // TODO Checkout System.Devices.Aep.IsConnected param, since sometimes device is already connected
            //public bool IsConnected => (bool?)DeviceInformation.Properties["System.Devices.Aep.IsConnected"] == true;

            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };
            //_bleWatcher =
            //    DeviceInformation.CreateWatcher(string.Empty, requestedProperties, 
            //        DeviceInformationKind.AssociationEndpoint);
            _bleWatcher = new() { ScanningMode = BluetoothLEScanningMode.Active }; // Active - possible Windows bug https://stackoverflow.com/questions/54525137/why-does-bluetoothleadvertisementwatcher-stop-firing-received-events
            _bleWatcher.SignalStrengthFilter.OutOfRangeThresholdInDBm = -75;
            _advertisementWatchdog = new System.Timers.Timer(_advertisementWatchdogInterval.TotalMilliseconds);
            _advertisementWatchdog.Elapsed += OnAdvertisementPeriodElapsed;
            _advertisementWatchdog.Start();
        }

        public void AddDeviceNameFilter(string deviceName)
        {
            _bleWatcher.AdvertisementFilter.Advertisement.LocalName = deviceName;
        }

        private void OnAdvertisementPeriodElapsed(object? sender, ElapsedEventArgs elapsedEventArgs)
        {
            foreach (var availableBleDevice in AvailableBleDevices.ToArray())
            {
                if (_dateTimeService.Now - availableBleDevice.LatestTimestamp > _advertisementWatchdogTimeout)
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
            // BluetoothAddress = 274378077492446
            var existing = AvailableBleDevices.SingleOrDefault(d => d.Address == args.BluetoothAddress);
            if (existing == null)
            {
                AvailableBleDevices.Add(new BleDeviceHandler(
                    _dateTimeService,
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