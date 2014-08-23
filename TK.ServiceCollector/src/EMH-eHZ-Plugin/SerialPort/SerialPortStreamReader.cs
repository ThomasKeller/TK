namespace EMH_eHZ_Plugin.SerialPort
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO.Ports;
    using System.Diagnostics.Contracts;

    public class SerialPortStreamReader : IStreamReader
    {
        SerialPort _serialPort;

        public SerialPortStreamReader(SerialPort serialPort)
        {
            Contract.Requires(serialPort != null);
            Contract.Requires(serialPort.IsOpen);
            _serialPort = serialPort;
        }

        public long CurrentBufferSize
        {
            get
            {
                return _serialPort.BytesToRead;
            }
        }

        public byte ReadByte()
        {
            return (byte)_serialPort.ReadByte();
        }

        public byte[] ReadBytes(int numberOfBytes)
        {
            byte[] bytes = new byte[numberOfBytes];
            _serialPort.Read(bytes, 0, numberOfBytes);
            return bytes;
        }
    }
}
