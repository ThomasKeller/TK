using System;
using System.Xml.Serialization;
using System.Diagnostics.Contracts;

namespace TK.TimeSeries.Core
{
    [Serializable]
    public class CompressionConditionConfig
    {
        [XmlElement(ElementName = "TagName")]
        public string MeasuredValueName { get; set; }

        public double ValueDeadBandPercent { get; set; }
        public TimeSpanConfig TimeDeadBand { get; set; }
        public TimeSpanConfig RewriteValueAfter { get; set; }

        public CompressionConditionConfig()
        {
            TimeDeadBand = new TimeSpanConfig();
            RewriteValueAfter = new TimeSpanConfig();
        }
        
        public void SetTimeDeadBand(TimeSpan sp)
        {
            Contract.Requires<ArgumentNullException>(TimeDeadBand != null, "parameter TimeDeadBand should not be null");
            TimeDeadBand.Hours = sp.Hours;
            TimeDeadBand.Minutes = sp.Minutes;
            TimeDeadBand.Seconds = sp.Seconds;
        }

        public TimeSpan GetTimeDeadBand()
        {
            return new TimeSpan(TimeDeadBand.Hours, TimeDeadBand.Minutes, TimeDeadBand.Seconds);
        }

        public void SetRewriteValueAfter(TimeSpan sp)
        {
            Contract.Requires<ArgumentNullException>(RewriteValueAfter != null, "parameter RewriteValueAfter should not be null");
            RewriteValueAfter.Hours = sp.Hours;
            RewriteValueAfter.Minutes = sp.Minutes;
            RewriteValueAfter.Seconds = sp.Seconds;
        }

        public TimeSpan GetRewriteValueAfter()
        {
            return new TimeSpan(RewriteValueAfter.Hours, RewriteValueAfter.Minutes, RewriteValueAfter.Seconds);
        }

        public CompressionCondition ToCompressionCondition()
        {
            return new CompressionCondition() {
                RewriteValueAfter = GetRewriteValueAfter(),
                TimeDeadBand = GetTimeDeadBand(),
                ValueDeadBandPercent = this.ValueDeadBandPercent
            };
        }
    }

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
