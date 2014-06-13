using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TK.PluginManager;

namespace PluginTest
{
    class Program
    {
        static void Main(string[] args)
        {
            List<PluginConfiguration> configs = new List<PluginConfiguration>(PluginConfiguration.LoadPluginsConfig());
            var pluginLoader = new PluginLoader(AppDomain.CurrentDomain.BaseDirectory, "*Plugin.dll");
            foreach (string name in pluginLoader.PluginNames)
            {
                PluginBase plugin = pluginLoader[name];
                Console.WriteLine(string.Format("{0} {1}", plugin.PluginName(), plugin.PluginVersion()));
                var pluginConfig = configs.Where(c => c.PluginName == plugin.PluginName())
                                          .FirstOrDefault();

                plugin.Initialize(pluginConfig.Paramters);
                var defaultParameter = plugin.GetParameters();
                foreach (string key in defaultParameter.Keys.ToArray())
                {
                    Console.WriteLine(string.Format("Parameter: {0}, Value: {1}", key, defaultParameter[key]));
                }
                plugin.Start();
            }
            Console.ReadKey();

            foreach (string name in pluginLoader.PluginNames) 
            {
                pluginLoader[name].Stop();
            }

        }
    }
}
