namespace EMH_eHZ_Plugin.SmlStructures
{
    using System;

    /// <summary>
    /// Bytes 	Beschreibung 
    /// 1       SML T/L     77  valListEntry (sequence) 
    /// 1       SML T/L     07 objName (TL[1] + octet_string[6]) 
    /// 2       SML data    01 00 objName Teil A und B 
    /// 4       SML data    01 08 00 FF objName Teil C, D, E und F 
    /// 1       SML T/L     64 ????
    /// 1       SML T/L     01 ????
    /// 1       SML T/L     01 status = not set 
   
    /// 2       SML T/L     01 valTime = not set 
    
    /// 1       SML T/L     62 unit (TL[1] + unsigned[1]) 
    /// 1       SML T/L     1E unit = Wh 
    /// 1       SML T/L     52 scaler (TL[1] + unsigned[1]) 
    /// 1       SML T/L     FF scaler = /10 
    /// 1       SML T/L     56 value (TL[1] + integer[32]) 
    /// 4       SML data    xx xx xx xx  value -> ‘12345678’ (Seriennummer in 8 ASCII Charakter) 
    /// 1       SML T/L     01 valueSignature = not set 
    ///    /// </summary>
    public sealed class ConsumedEnergy
    {
        private static byte[] sequence1 = { 0x77, 0x07, 0x01, 0x00,
                                            0x01, 0x08, 0x00, 0xFF,
                                            0x64, 0x01, 0x01 };
        private static byte[] sequence2 = { 0x62, 0x1E, 0x52, 0xFF, 
                                            0x56, 0x00 };

        public static double Parse(byte[] smlData, ref int currentPosition)
        {
            if (ArrayHelpers.CheckSequence(smlData, ref currentPosition, sequence1)) {
                byte valTime1 = smlData[currentPosition++];
                byte valTime2 = smlData[currentPosition++];
                if (ArrayHelpers.CheckSequence(smlData, ref currentPosition, sequence2)) {
                    byte[] result = ArrayHelpers.ReadAndCreateArray(smlData, ref currentPosition, 4);
                    if (smlData[currentPosition++] == 0x01) {
                        long test = BitConverter.ToInt32(result, 0);
                        return (double)ArrayHelpers.ConvertTo(result) / 10.0;
                    }
                }
            }
            throw new Exception("cannot parse DeviceId structure: Positon:" + currentPosition.ToString());
        }
    }
}
