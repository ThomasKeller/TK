namespace EMH_eHZ_Plugin.SerialPort
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Diagnostics.Contracts;

    public class SmlMessageFinder
    {
        private bool _startSequenceFound = false;
        private bool _found = false;
        private byte[] _startSequence = new byte[] { 0x1B, 0x1B, 0x1B, 0x1B, 0x01, 0x01, 0x01, 0x01 };
        private byte[] _endSequence = new byte[] { 0x1B, 0x1B, 0x1B, 0x1B, 0x1A };
        private IStreamReader _streamReader;
        private List<byte> _buffer = new List<byte>(400);

        public bool Found { get { return _found; } }
        public byte[] Message { get { return _buffer.ToArray(); } }
        public DateTime MeasuredTime { get; set; }

        public SmlMessageFinder(IStreamReader streamReader)
        {
            Contract.Requires(streamReader != null);
            if (streamReader.CurrentBufferSize < 392)
            {
                return;
            }
            _streamReader = streamReader;
            if (SearchStartSequence())
            {
                if (SearchEndSequence())
                {
                    _found = true;
                }
            }
        }

        private bool SearchSequence(byte[] sequence)
        {
            while (_streamReader.CurrentBufferSize >= sequence.Length)
            {
                bool found = true;
                for (int x = 0; x < sequence.Length; x++)
                {
                    byte value = _streamReader.ReadByte();
                    // add value to buffer if start sequence is already found
                    if (_startSequenceFound)
                    {
                        _buffer.Add(value);
                    }
                    if (sequence[x] != value)
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return true;
                }
            }
            return false;
        }

        private bool SearchStartSequence()
        {
            if (SearchSequence(_startSequence))
            {
                _buffer.AddRange(_startSequence);
                _startSequenceFound = true;
                MeasuredTime = DateTime.Now;
                return true;
            }
            return false;
        }

        private bool SearchEndSequence()
        {
            if (SearchSequence(_endSequence))
            {
                for (int x = 0; x < 3; x++)
                {
                    _buffer.Add(_streamReader.ReadByte());
                }
                return true;
            }
            return false;
        }
    }
}
