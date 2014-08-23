using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TK.PluginManager;
using TK.Logging;

namespace PluginTest
{
    class Program
    {
        public static void MyTask(IDictionary<string, object> parameters)
        {
            Console.Write(DateTime.Now.ToLongTimeString());
            Console.Write("executing my task !...");
            System.Threading.Thread.Sleep(2000);
            Console.WriteLine(DateTime.Now.ToLongTimeString());
        }


        static void Main(string[] args)
        {
            TK.Logging.ILogger logger = LoggerFactory.CreateLoggerFor("main");

            /*var scheduler = new SimpleSchedulerWrapper();

            scheduler.AddJob("0/60 * * * * ?", MyTask, new Dictionary<string, object>(), false);
            scheduler.AddJob("0/1 * * * * ?", MyTask, new Dictionary<string, object>(), true);

            scheduler.Start();
            Console.ReadLine();
            return;
            */

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
