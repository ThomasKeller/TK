namespace EMH_eHZ_Plugin.SmlStructures
{
    using System;

    /// <summary>
    /// Bytes 	Beschreibung 
    /// 1       SML T/L 77  valListEntry (sequence) 
    /// 1       SML T/L 07 objName (TL[1] + octet_string[6]) 
    /// 2       SML data 81 81 objName Teil A und B 
    /// 4       SML data C7 82 03 FF objName Teil C, D, E und F 
    /// 1       SML T/L 01 status = not set 
    /// 1       SML T/L 01 valTime = not set 
    /// 1       SML T/L 01 unit = not set 
    /// 1       SML T/L 01 scaler = not set 
    /// 1       SML T/L 04 value (TL[1] + octet_string[3]) 
    /// 3       SML data xx xx xx value -> z.B. ‘ITA’ füriTrona GmbH 
    /// 1       SML T/L 01 valueSignature = not set 
    /// </summary>
    public sealed class CompanyInfo
    {
        private static byte[] sequence = { 0x77, 0x07, 0x81, 0x81, 
                                           0xC7, 0x82, 0x03, 0xFF, 
                                           0x01, 0x01, 0x01, 0x01,
                                           0x04 };

        public static string Parse(byte[] smlData, ref int currentPosition)
        {
            if (ArrayHelpers.CheckSequence(smlData, ref currentPosition, sequence)) { 
                // sollten eigentlich 4 bytes sein, das passt aber sonst nicht
                byte[] result = ArrayHelpers.ReadAndCreateArray(smlData, ref currentPosition, 3);
                string companyInfo = ArrayHelpers.ConvertByteArrayToString(result);
                if (smlData[currentPosition++] == 0x01) {
                    return companyInfo;
                }
            }
            throw new Exception("cannot parse CompanyInfo structure: Positon:" + currentPosition.ToString());
        }
    }
}
