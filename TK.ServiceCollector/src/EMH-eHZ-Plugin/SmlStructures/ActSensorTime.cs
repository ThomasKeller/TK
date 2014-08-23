namespace EMH_eHZ_Plugin.SmlStructures
{
    using System;

    /// <summary>
    /// Bytes 	Beschreibung 
    /// 1		SML T/L 	72	actSensorTime (choice). 
    /// 1		SML T/L 	62  actSensorTimer (TL[1] + unsigned[8]). 
    /// 1		SML data 	01  actSensorTimer = 01. 
    /// 1		SML data 	65	secIndex (TL[1] + unsigned[32]). 
    /// 4		SML data 	xx xx xx xx 
    /// 
    /// secIndex -> 4D47’C169h -> 129’654’8201 Sekunden 
    /// -> actSensorTime: 01.02.2011 08:16:41 
    /// </summary>
    public sealed class ActSensorTime
    {
        private static byte[] sequence = { 0x72, 0x62, 0x01, 0x65 };

        public static byte[] Parse(byte[] smlData, ref int currentPosition)
        {
            if (ArrayHelpers.CheckSequence(smlData, ref currentPosition, sequence)) { 
                // sollten eigentlich 4 bytes sein, das passt aber sonst nicht
                return ArrayHelpers.ReadAndCreateArray(smlData, ref currentPosition, 5);
            }
            throw new Exception("cannot parse ActSensorTime structure: Positon:" + currentPosition.ToString());
        }
    }
}
