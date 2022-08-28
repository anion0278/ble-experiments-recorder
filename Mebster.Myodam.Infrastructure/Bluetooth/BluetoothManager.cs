using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;
using Mebster.Myodam.Common.Services;
using Mebster.Myodam.Models.Device;
using Swordfish.NET.Collections;

namespace Mebster.Myodam.Infrastructure.Bluetooth
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

            //string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };
            //_bleWatcher =
            //    DeviceInformation.CreateWatcher(string.Empty, requestedProperties, 
            //        DeviceInformationKind.AssociationEndpoint);
            _bleWatcher = new() { ScanningMode = BluetoothLEScanningMode.Active }; // Active - possible Windows bug https://stackoverflow.com/questions/54525137/why-does-bluetoothleadvertisementwatcher-stop-firing-received-events
            _bleWatcher.SignalStrengthFilter.OutOfRangeThresholdInDBm = -75;
            _advertisementWatchdog = new System.Timers.Timer(_advertisementWatchdogInterval.TotalMilliseconds); // TODO dispose
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
            foreach (var disposedDevice in AvailableBleDevices.Where(d=>d.IsDisposed))
            {
                AvailableBleDevices.Remove(disposedDevice);
            }

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