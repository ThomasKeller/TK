using System;

namespace TK.TimeSeries.Core
{
    public class MeasuredValue
    {
        #region Fields

        private object _value = null;
        private OPCQuality _quality = OPCQuality.Good;

        #endregion Fields

        #region Properties

        public string Name { get; set; }

        public DateTime TimeStamp { get; set; }

        public OPCQuality Quality { get { return GetQuality(); } set { _quality = value; } }

        public object Value { get { return _value; } set { _value = value; } }

        public string Description { get; set; }

        #endregion Properties

        public bool IsValueNull()
        {
            return _value == null;
        }

        public bool IsValid()
        {
            return IsValueNull() == false && TimeStamp.Ticks > 0;
        }

        public override string ToString()
        {
            return string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{0}, {1}, {2:yyyy-MM-dd HH:mm:ss}, {3}, {4}",
                Name,
                Value,
                TimeStamp,
                Quality.ToString(),
                Description);
        }

        public TypeCode GetTypeCode()
        {
            if (_value == null)
                return TypeCode.Empty;
            return Type.GetTypeCode(_value.GetType());
        }

        private OPCQuality GetQuality()
        {
            if (IsValueNull())
                return OPCQuality.NoValue;
            else
                return _quality;
        }
    }
}