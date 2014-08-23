namespace EMH_eHZ_Plugin.SmlStructures
{
    using System;

    /// <summary>
    /// Bytes 	Beschreibung 
    /// 1       SML T/L     77  valListEntry (sequence) 
    /// 1       SML T/L     07 objName (TL[1] + octet_string[6]) 
    /// 2       SML data    01 00 objName Teil A und B 
    /// 4       SML data    00 00 09 FF objName Teil C, D, E und F 
    /// 1       SML T/L     01 status = not set 
    /// 1       SML T/L     01 valTime = not set 
    /// 1       SML T/L     01 unit = not set 
    /// 1       SML T/L     01 scaler = not set 
    /// 1       SML T/L     09 value (TL[1] + octet_string[8]) 
    /// 8       SML data    xx xx xx xx  value -> ‘12345678’ (Seriennummer in 8 ASCII Charakter) 
    /// 1       SML T/L     01 valueSignature = not set 
    ///    /// </summary>
    public sealed class DeviceId
    {
        private static byte[] sequence = { 0x77, 0x07, 0x01, 0x00, 
                                           0x00, 0x00, 0x09, 0xFF, 
                                           0x01, 0x01, 0x01, 0x01,
                                           0x09 };

        public static string Parse(byte[] smlData, ref int currentPosition)
        {
            if (ArrayHelpers.CheckSequence(smlData, ref currentPosition, sequence)) { 
                byte[] result = ArrayHelpers.ReadAndCreateArray(smlData, ref currentPosition, 8);
                string deviceId = BitConverter.ToString(result);
                if (smlData[currentPosition++] == 0x01) {
                    return deviceId;
                }
            }
            throw new Exception("cannot parse DeviceId structure: Positon:" + currentPosition.ToString());
        }
    }
}
