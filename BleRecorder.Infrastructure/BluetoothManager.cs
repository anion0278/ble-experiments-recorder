using System.Collections.ObjectModel;
using System.Text;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace BleRecorder.Infrastructure
{
    public class BluetoothManager
    {
        BluetoothLEAdvertisementWatcher _bleWatcher = new ();

        public List<BleDevice> AvailableBleDevices { get; } = new();


        private void StartScanning()
        {
            string[] requestedProperties =
            {
                "System.Devices.Aep.DeviceAddress", 
                "System.Devices.Aep.IsConnected"
            };

            var deviceWatcher = DeviceInformation.CreateWatcher(
                                                                BluetoothLEDevice.GetDeviceSelector(),
                                                                        requestedProperties,
                                                                        DeviceInformationKind.AssociationEndpoint);
            _bleWatcher.ScanningMode = BluetoothLEScanningMode.Active;
            _bleWatcher.Received += BleWatcher_Received;
            //bleWatvher.Stopped += BleWatvher_Stopped;

            //deviceWatcher.Stopped += DevWatcher_Stopped;
            //deviceWatcher.Added += DevWatcher_Added;

            _bleWatcher.Start();
        }

        private void BleWatcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            if (args.Advertisement.LocalName != "")
            {
                //address.Add(args.Advertisement.LocalName);
                AvailableBleDevices.Add(new BleDevice { Address = args.BluetoothAddress, Name = args.Advertisement.LocalName, SignalStrength = args.RawSignalStrengthInDBm });
            }
        }


        void Uart_Receive(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            // An Indicate or Notify reported that the value has changed.
            var reader = DataReader.FromBuffer(args.CharacteristicValue);
            byte[] input = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(input);
            var res = Encoding.UTF8.GetString(input);

            //receive_Text = res;
            //receive_Text += Environment.NewLine;
        }

        public async void ConnectDevice(BleDevice device)
        {
            try
            {
                var bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(device.Address);
                bleDevice.ConnectionStatusChanged += BleDeviceOnConnectionStatusChanged;
                var y = bleDevice.DeviceInformation.Pairing.CanPair;
                var x = bleDevice.ConnectionStatus;
                var allDeviceServices = (await bleDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached)).Services.ToArray();
                String uart_guid = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";
                var ser = await bleDevice.GetGattServicesForUuidAsync(Guid.Parse(uart_guid));

                if (ser.Status != GattCommunicationStatus.Success) throw new ArgumentNullException();
                var UARTservice = ser.Services.First();

                Guid rxId = new Guid("6e400003b5a3f393e0a9e50e24dcca9e");

                var rxCharacteristic = (await UARTservice.GetCharacteristicsAsync()).Characteristics.Single(s => s.Uuid == rxId);

                var notifyResult = await rxCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.Notify);

                rxCharacteristic.ValueChanged += Uart_Receive;
            }
            catch (Exception xx)
            {

            }
        }

        private void BleDeviceOnConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            // unsubcribe RX, TX
        }
    }

    public class BleDevice
    {
        public ulong Address { get; set; }
        public string Name { get; set; }
        public short SignalStrength { get; set; }
    }
}