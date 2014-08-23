using System;
using System.Collections.Generic;

namespace TK.PluginManager
{
    public class MeasureValueBox
    {
        public DateTime MeasuredUtcTime { get; set; }

        public string PluginName { get; set; }

        public IDictionary<string, object> MeasuredValues { get; set; }

        public MeasureValueBox()
        {
            MeasuredValues = new Dictionary<string, object>();
        }
    }
}