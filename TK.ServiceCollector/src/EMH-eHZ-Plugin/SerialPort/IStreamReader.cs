namespace EMH_eHZ_Plugin.SerialPort
{
    public interface IStreamReader
    {
        long CurrentBufferSize { get; }
        byte ReadByte();
        byte[] ReadBytes(int numberOfBytes);
    }
}
