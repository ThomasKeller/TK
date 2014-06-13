using System;

namespace TK.TimeSeries.Core
{
    /// <summary>
    /// This is just a container for the XML serialization
    /// </summary>
    [Serializable]
    public class CompressionConditionConfigs
    {
        public CompressionConditionConfig[] Items;
    }
}
