using System;

namespace EMH_eHZ_Plugin.SerialPort
{
    public class LastMeasurement<T>
    {
        private DateTime? _MeasuredTime;
        private T _value;

        public T Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public DateTime? MeasuredTime
        {
            get { return _MeasuredTime; }
            set { _MeasuredTime = value; }
        }
    }
}