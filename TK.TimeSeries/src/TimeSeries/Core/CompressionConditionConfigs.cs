using System;
using System.Xml.Serialization;

namespace TK.TimeSeries.Core
{
    /// <summary>
    /// This is just a container for the XML serialization
    /// </summary>
    [Serializable]
    public class CompressionConditionConfigs
    {
        [XmlElement(ElementName = "CompressionCondition")]
        public CompressionConditionConfig[] Items;
    }
}
