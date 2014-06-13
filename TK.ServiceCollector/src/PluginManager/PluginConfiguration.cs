using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace TK.PluginManager
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class PluginConfiguration
    {
        private static readonly TK.Logging.ILogger m_Logger = TK.Logging.LoggerFactory.CreateLoggerFor(typeof(PluginConfiguration));
        private readonly Dictionary<string, string> m_Parameters = new Dictionary<string, string>();

        public string PluginName { get; set; }
        public Dictionary<string, string> Paramters { get { return m_Parameters; } }
        
        public static IEnumerable<PluginConfiguration> LoadPluginsConfig(string configFile = "PluginConfig.xml")
		{
            m_Logger.Debug(System.Reflection.MethodBase.GetCurrentMethod().ToString());
            List<PluginConfiguration> list = new List<PluginConfiguration>();
			try
			{
				// Open definitions file
				XmlDocument doc = new XmlDocument();
				doc.Load (configFile);
				
				// Find root node
				XmlNode pluginsNode = doc.SelectSingleNode("Plugins") ;
				if ( pluginsNode == null )
					throw new XmlException ( "Definitions file is not valid\n" );
				
				// Scan the childs nodes
				foreach (XmlNode node in pluginsNode.ChildNodes)
				{
                    XmlAttribute attrName = node.Attributes["Name"];
                    var result = new PluginConfiguration();
                    result.PluginName = attrName.Value;
                    foreach (XmlNode parameter in node.ChildNodes)
                    {
                        XmlAttribute parameterName = parameter.Attributes["Name"];
                        string value = parameter.InnerText;
                        result.m_Parameters.Add(parameterName.Value, value);
                    }
                    list.Add(result);
				}
			}
			catch(Exception e)
			{
                m_Logger.Fatal(e.Message, e);
                throw;
			}
			return list;
		}
    }
}
