namespace EMH_eHZ_Plugin.SmlStructures
{
    using System;

    public sealed class ArrayHelpers
    {
        public static bool CheckSequence(byte[] smlData, ref int currentPosition, byte[] sequence)
        {
            for (int x = 0; x < sequence.Length; x++)
            {
                byte value = smlData[currentPosition++];
                if (sequence[x] != value)
                {
                    return false;
                }
            }
            return true;
        }

        public static byte[] ReadAndCreateArray(byte[] smlData, ref int currentPosition, int numberOfBytes)
        {
            if (numberOfBytes <= 0) throw new ArgumentOutOfRangeException("numberOfBytes <= 0");
            if (currentPosition < 0) throw new ArgumentOutOfRangeException("currentPosition < 0");
            if (numberOfBytes + currentPosition >= smlData.Length) throw new ArgumentOutOfRangeException("numberOfBytes + currentPosition >= smlData.Length");

            byte[] result = new byte[numberOfBytes];
            for (int x = 0; x < numberOfBytes; x++)
            {
                result[x] = smlData[currentPosition++];
            }
            return result;
        }

        public static string ConvertByteArrayToString(byte[] array)
        {
            return System.Text.Encoding.Default.GetString(array); ;
        }

        public static int ConvertTo(byte[] values)
        {
            int result = 0;
            int resultInt = 0;
            int shiftFactor = 0;
            for (int x = values.Length - 1; x >= 0; x--)
            {
                resultInt = values[x] << shiftFactor;
                shiftFactor += 8;
                result += resultInt;
            }
            return result;
        }
    }
}