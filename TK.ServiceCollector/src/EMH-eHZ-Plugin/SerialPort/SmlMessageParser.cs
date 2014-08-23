using EMH_eHZ_Plugin.SmlStructures;

namespace EMH_eHZ_Plugin.SerialPort
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    //using EMH_eHZ_Plugin.SmlStructures;

    public class SmlMessageParser
    {
        private int _position = 0;
        private byte[] _smlMessage;
        private bool _isValid = false;

        public bool IsValid { get { return _isValid; } }
        public string FirstTransactionId { get; set; }
        public string SecondTransactionId { get; set; }
        public string FirstServerId { get; set; }
        public string SecondServerId { get; set; }
        public string CompanyName { get; set; }
        public string Device { get; set; }
        public double ConsumedEnergy1_Wh { get; set; }
        public double ProducedEnergy1_Wh { get; set; }
        public double ConsumedEnergy2_Wh { get; set; }
        public double ProducedEnergy2_Wh { get; set; }

        public SmlMessageParser(byte[] smlMessage)
        {
            if (smlMessage == null) throw new ArgumentNullException("smlMessage");
            if (smlMessage.Length >= 392)
            {
                _smlMessage = smlMessage;
                Parse();
            }
        }

        private void Parse()
        {
            //http://wiki.volkszaehler.org/hardware/channels/meters/power/edl-ehz/emh-ehz-h1
            ParseFirstTransactionId();
            ParseSeverId();
            CheckFirstCRC();
            ParseSecondTransactionId();
            ParseSecondServerId();

            byte[] sekundenIndex = ActSensorTime.Parse(_smlMessage, ref _position);
            CompanyName = CompanyInfo.Parse(_smlMessage, ref _position);
            Device = DeviceId.Parse(_smlMessage, ref _position);

            ConsumedEnergy1_Wh = ConsumedEnergy.Parse(_smlMessage, ref _position);
            ProducedEnergy1_Wh = ProducedEnergy.Parse(_smlMessage, ref _position);

            byte[] seqConsumedEnergy = { 0x77, 0x07, 0x01, 0x00, 0x01, 0x08, 0x01, 0xFF, 0x01, 0x01 };
            byte[] seqProducedEnergy = { 0x77, 0x07, 0x01, 0x00, 0x02, 0x08, 0x01, 0xFF, 0x01, 0x01 };

            ConsumedEnergy2_Wh = Energy.Parse(_smlMessage, ref _position, seqConsumedEnergy);
            ProducedEnergy2_Wh = Energy.Parse(_smlMessage, ref _position, seqProducedEnergy);

            byte[] sequence3 = { 0x77, 0x07, 0x01, 0x00, 0x01, 0x08, 0x02, 0xFF, 0x01, 0x01 };
            byte[] sequence4 = { 0x77, 0x07, 0x01, 0x00, 0x02, 0x08, 0x02, 0xFF, 0x01, 0x01 };
            //double test3 = Energy.Parse(_smlMessage, ref _position, sequence3);
            //double test4 = Energy.Parse(_smlMessage, ref _position, sequence4);
            _isValid = true;
        }

        private void ParseFirstTransactionId()
        {
            // 1B 1B 1B 1B            Start Escape Zeichenfolge
            // 01 01 01 01            Start Übertragung Version 1
            // 76                     Liste mit 6 Einträgen
            // 07 00 14 04 EC 6D 20   transactionID (7 Byte)
            byte[] startSequence = { 0x1B, 0x1B, 0x1B, 0x1B, 0x01, 0x01, 0x01, 0x01, 0x76 };
            CheckSequence(startSequence, "Sequence Start Version 1");
            FirstTransactionId = BitConverter.ToString(ArrayHelpers.ReadAndCreateArray(_smlMessage, ref _position, 7));
        }

        private void ParseSecondTransactionId()
        {
            // 76                     Liste mit 6 Einträgen
            // 07 00 14 04 EC 6D 21   transactionID
            CheckByte(0x76, "list with 6 entries");
            SecondTransactionId = BitConverter.ToString(ArrayHelpers.ReadAndCreateArray(_smlMessage, ref _position, 7));
        }

        private void CheckFirstCRC()
        {
            // 01       username (leer)
            // 01       password (leer)
            // 63 B7 96 CRC
            // 00       Ende Nachricht
            CheckByte(0x01, "username (empty)");
            CheckByte(0x01, "password (empty)");

            //byte[] crcArray = new byte[_smlMessage.Length - 2];
            //Array.Copy(_smlMessage, crcArray, _smlMessage.Length - 2);
            //ushort crc = Crc16CcittKermit.ComputeChecksum(crcArray);
            _position += 3; // CRC
            CheckByte(0x00, "End of message");
        }

        private void ParseSeverId()
        {
            // 62 00                            groupNo
            // 62 00                            abortOnError
            // 72                               Liste mit 2 Einträgen
            // 63 07 01                         Nachricht 0701 = SML_GetList.Res
            // 77                               Liste mit 7 Einträgen
            // 01                               clientID (leer)
            // 0B 06 45 4D 48 xx xx xx xx xx xx serverID 
            byte[] groupNoAndAbort = { 0x62, 0x00, 0x62, 0x00 };
            CheckSequence(groupNoAndAbort, "group number / abortOnError");
            CheckByte(0x72, "List with 2 entries");
            byte[] smlPublicOpen = { 0x63, 0x01, 0x01 };
            CheckSequence(smlPublicOpen, "SML_PublicOpen.Res");
            CheckByte(0x76, "List with 6 entries");
            _position += 1;  // codepage (leer)
            _position += 1;  // clientID (leer)
            _position += 7;  // reqFileID 
            FirstServerId = BitConverter.ToString(ArrayHelpers.ReadAndCreateArray(_smlMessage, ref _position, 9));
        }

        private void ParseSecondServerId()
        {
            byte[] groupNoAndAbort = { 0x62, 0x00, 0x62, 0x00 };
            CheckSequence(groupNoAndAbort, "group number / abortOnError");
            CheckByte(0x72, "List with 2 entries");
            byte[] smlGetListRes = { 0x63, 0x07, 0x01 };
            CheckSequence(smlGetListRes, "SML_GetList.Res");
            CheckByte(0x77, "List with 7 entries");
            CheckByte(0x01, "clientId (empty)");
            SecondServerId = BitConverter.ToString(ArrayHelpers.ReadAndCreateArray(_smlMessage, ref _position, 10));
            CheckByte(0x01, "listName (empty)");
            _position += 5;  // ist nicht in der vorlage
        }

        private void CheckSequence(byte[] sequence, string sequenceName)
        {
            if (ArrayHelpers.CheckSequence(_smlMessage, ref _position, sequence))
            {
                return;
            }
            throw new Exception("Sequence is not valid: " + sequenceName + " / Position:" + _position.ToString());
        }

        private void CheckByte(byte sequence, string byteName)
        {
            if (sequence != _smlMessage[_position++])
            {
                throw new Exception("Byte is not valid: " + byteName + " / Position:" + _position.ToString());
            }
        }

        private byte[] ReadAndCreateArray(int numberOfBytes)
        {
            return ArrayHelpers.ReadAndCreateArray(_smlMessage, ref _position, numberOfBytes);
        }
    }
}
