using System;
using System.Xml.Serialization;

namespace TK.TimeSeries.Core
{
    [Serializable]
    public class CompressionConditionConfig
    {
        [XmlElement(ElementName = "TagName")]
        public string MeasuredValueName { get; set; }

        [Obsolete("Property is obsolete. Please use ValueDeadBandDelta instead")]
        [System.Xml.Serialization.XmlIgnore]
        public double ValueDeadBandPercent { get; set; }

        public double ValueDeadBandDelta { get; set; }
        public TimeSpanConfig TimeDeadBand { get; set; }
        public TimeSpanConfig RewriteValueAfter { get; set; }

        public CompressionConditionConfig()
        {
            TimeDeadBand = new TimeSpanConfig();
            RewriteValueAfter = new TimeSpanConfig();
        }
        
        public void SetTimeDeadBand(TimeSpan timeDeadBand)
        {
            if (timeDeadBand == null) { throw new ArgumentNullException("timeDeadBand"); }

            TimeDeadBand.Hours = timeDeadBand.Hours;
            TimeDeadBand.Minutes = timeDeadBand.Minutes;
            TimeDeadBand.Seconds = timeDeadBand.Seconds;
        }

        public TimeSpan GetTimeDeadBand()
        {
            return new TimeSpan(TimeDeadBand.Hours, TimeDeadBand.Minutes, TimeDeadBand.Seconds);
        }

        public void SetRewriteValueAfter(TimeSpan rewriteValueAfter)
        {
            if (rewriteValueAfter == null) { throw new ArgumentNullException("rewriteValueAfter"); }

            RewriteValueAfter.Hours = rewriteValueAfter.Hours;
            RewriteValueAfter.Minutes = rewriteValueAfter.Minutes;
            RewriteValueAfter.Seconds = rewriteValueAfter.Seconds;
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
                ValueDeadBandDelta = this.ValueDeadBandDelta
            };
        }
    }
}
