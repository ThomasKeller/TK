using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Xml.Serialization;
using System.IO;
using TK.Logging;

namespace TK.TimeSeries.Core
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class CompressionConditionManager
    {
        private static ILogger _logger = LoggerFactory.CreateLoggerFor(typeof(CompressionConditionManager));
        private Dictionary<string, CompressionConditionConfig> _configItems = new Dictionary<string, CompressionConditionConfig>();
        
        public CompressionConditionManager()
        { 
        
        }

        public CompressionConditionManager(CompressionConditionConfigs configs)
        {
            Contract.Requires(configs != null);
            Contract.Requires(configs.Items != null);
            AddRange(configs.Items);
        }

        public void Add(CompressionConditionConfig config)
        {
            Contract.Requires(config != null);
            _configItems.Add(config.MeasuredValueName, config);
        }

        public void AddRange(IEnumerable<CompressionConditionConfig> configs)
        {
            Contract.Requires(configs != null);
            foreach(var item in configs)
            {
                if (false == _configItems.ContainsKey(item.MeasuredValueName)) {
                    _configItems.Add(item.MeasuredValueName, item);
                }
            }
        }

        public void Remove(string name)
        {
            _configItems.Remove(name);
        }

        public CompressionCondition GetConfigFor(string tagName)
        {
            if (_configItems.ContainsKey(tagName)) {
                return _configItems[tagName].ToCompressionCondition();
            }
            else {
                return CompressionCondition.GetDefaultCondition();
            }
        }

        public static CompressionConditionManager LoadFromFile(string path)
        {
            CompressionConditionManager manager = new CompressionConditionManager();
            try {
                XmlSerializer serializer = new XmlSerializer(typeof(CompressionConditionConfigs), "TimeSeries");
                using (TextReader reader = new StreamReader(path)) {
                    CompressionConditionConfigs configs = (CompressionConditionConfigs)serializer.Deserialize(reader);
                    manager.AddRange(configs.Items);
                }
            }
            catch (Exception ex) {
                _logger.Error(string.Format("LoadFromFile: {0}", path), ex); 
            }
            return manager;
        }
    }
}
