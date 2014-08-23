using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using TK.Logging;

namespace TK.PluginManager
{
    public class PluginLoader
    {
        private static ILogger _Logger = LoggerFactory.CreateLoggerFor(typeof(PluginLoader));
        private Dictionary<string, PluginBase> _PluginBaseCollection;

        static PluginLoader()
        {
            _Logger.Debug(MethodBase.GetCurrentMethod().Name);
        }

        public PluginLoader(string searchPath, string searchPattern)
        {
            _Logger.Debug(MethodBase.GetCurrentMethod().Name);
            SearchForPluginAssemblies(searchPath, searchPattern);
        }

        public PluginBase this[string pluginName] 
        {
            get { return _PluginBaseCollection.ContainsKey(pluginName) ? _PluginBaseCollection[pluginName] : null; }
        }

        public string[] PluginNames
        {
            get { return _PluginBaseCollection.Keys.ToArray(); }
        }

        private void SearchForPluginAssemblies(string pluginSearchPath, string searchPattern)
        {
            _Logger.InfoFormat("{0} Directory: '{1} Pattern: {2}", MethodBase.GetCurrentMethod().Name, pluginSearchPath, searchPattern);
            _PluginBaseCollection = new Dictionary<string ,PluginBase>();
            string directoryPath = Path.GetDirectoryName(pluginSearchPath);
            if (directoryPath == null) return;
            string[] pluginFiles = Directory.GetFiles(directoryPath, searchPattern);
            foreach (string pluginFile in pluginFiles)
            {
                LoadPluginAssemblies(pluginFile);
            }
        }

        private void LoadPluginAssemblies(string pluginFile)
        {
            try
            {
                string assemblyName = Path.GetFileNameWithoutExtension(pluginFile);
                if (string.IsNullOrEmpty(assemblyName)) return;
                Assembly assembly = Assembly.Load(assemblyName);
                if (assembly == null) return;
                foreach (var type in assembly.GetTypes())
                {
                    PluginBase pluginBase = null;
                    //if (type.BaseType == typeof(QuartzPluginBase))
                    //{
                    //    pluginBase = Activator.CreateInstance(type) as QuartzPluginBase;
                    //}else
                    if (type.BaseType == typeof(PluginBase))
                    {
                        pluginBase = Activator.CreateInstance(type) as PluginBase;
                    }
                    else if (type.BaseType == typeof(SchedulePluginBase))
                    {
                        pluginBase = Activator.CreateInstance(type) as SchedulePluginBase;
                    }

                    if (pluginBase == null) continue;
                    _Logger.InfoFormat("{0}  Plugin Name: {1}", MethodBase.GetCurrentMethod().Name, pluginBase.PluginName());
                    _PluginBaseCollection.Add(pluginBase.PluginName(), pluginBase);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error(ex.Message);
            }
        }
    }
}
