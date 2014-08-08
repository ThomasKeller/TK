using System;
using System.Xml.Serialization;

namespace TK.TimeSeries.Core
{
    [Serializable]
    public class TimeSpanConfig
    {
        [XmlAttribute]
        public int Hours { get; set; }

        [XmlAttribute]
        public int Minutes { get; set; }

        [XmlAttribute]
        public int Seconds { get; set; }
    }
}