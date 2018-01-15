using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace XRobot
{
    public class Arduino
    {
        private SerialDevice _serialDevice;

        private DataReader _dataReader;
        private DataWriter _dataWriter;

        public bool IsOpen { get; set; }

        public async Task<bool> Open(DeviceInformation device, uint baudRate = 9600,
            SerialParity parity = SerialParity.None, ushort dataBits = 8,
            SerialStopBitCount stopBits = SerialStopBitCount.One)
        {
            // Close open port
            Close();
            // Create a serial port device from the COM port device ID
            _serialDevice = await SerialDevice.FromIdAsync(device.Id);
            // If serial device is valid...
            if (this._serialDevice != null)
            {
                _serialDevice.StopBits = stopBits;
                _serialDevice.Parity = parity;
                _serialDevice.BaudRate = baudRate;
                _serialDevice.DataBits = dataBits;

                // Create a single device writer for this port connection
                _dataWriter = new DataWriter(this._serialDevice.OutputStream);

                // Create a single device reader for this port connection
                _dataReader = new DataReader(this._serialDevice.InputStream);

                // Allow partial reads of the input stream
                _dataReader.InputStreamOptions = InputStreamOptions.Partial;

                // Port is now open
                IsOpen = true;

            }

            return IsOpen;
        }

        public void Close()
        {  
            
            // If serial device defined...
            if (this._serialDevice != null)
            {
                // Dispose and clear device
                this._serialDevice.Dispose();
                this._serialDevice = null;
            }

            // If data reader defined...
            if (this._dataReader != null)
            {
                // Detatch reader stream
                this._dataReader.DetachStream();

                // Dispose and clear data reader
                this._dataReader.Dispose();
                this._dataReader = null;
            }

            // If data writer defined...
            if (this._dataWriter != null)
            {
                // Detatch writer stream
                this._dataWriter.DetachStream();

                // Dispose and clear data writer
                this._dataWriter.Dispose();
                this._dataWriter = null;
            }
            
            // Port now closed
            this.IsOpen = false;
        }

        public async Task WriteByteAsync(byte data)
        {
            // Write block of data to serial port
            this._dataWriter.WriteByte(data);
            // Transfer data to the serial device now
            await this._dataWriter.StoreAsync();
        }

        public async Task<byte> ReadByteAsync()
        {
            await _dataReader.LoadAsync(1);
            return _dataReader.ReadByte();
        }

        public async Task<DeviceInformationCollection> GetAvailableDevices()
        {
            var devices = await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelector());

            return devices;
        }

    }
}
