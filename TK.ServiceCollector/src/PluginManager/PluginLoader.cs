// -----------------------------------------------------------------------
// <copyright file="PluginLoader.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace TK.PluginManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class PluginLoader
    {
        private Dictionary<string, PluginBase> m_PluginBaseCollection;

        static PluginLoader()
        {
            //s_Log.Debug(MethodBase.GetCurrentMethod().Name);
        }

        public PluginLoader(string searchPath, string searchPattern)
        {
            //s_Log.Debug(MethodBase.GetCurrentMethod().Name);
            //m_FtpcProxy = ftpcProxy;
            SearchForPluginAssemblies(searchPath, searchPattern);
        }

        public PluginBase this[string pluginName] 
        {
            get { return m_PluginBaseCollection.ContainsKey(pluginName) ? m_PluginBaseCollection[pluginName] : null; }
        }

        public string[] PluginNames
        {
            get { return m_PluginBaseCollection.Keys.ToArray(); }
        }

        private void SearchForPluginAssemblies(string pluginSearchPath, string searchPattern)
        {
            //s_Log.InfoFormat("{0} Directory: '{1} Pattern: {2}", MethodBase.GetCurrentMethod().Name, pluginSearchPath, searchPattern);
            m_PluginBaseCollection = new Dictionary<string ,PluginBase>();
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
                    if (type.BaseType == typeof(QuartzPluginBase))
                    {
                        pluginBase = Activator.CreateInstance(type) as QuartzPluginBase;
                    }
                    else if (type.BaseType == typeof(PluginBase))
                    {
                        pluginBase = Activator.CreateInstance(type) as PluginBase;
                    }
                    if (pluginBase == null) continue;
                    //s_Log.InfoFormat("{0}  Plugin Name: {1}", MethodBase.GetCurrentMethod().Name, pluginBase.GetPluginName());
                    m_PluginBaseCollection.Add(pluginBase.PluginName(), pluginBase);
                }
            }
            catch (Exception ex)
            {
                //s_Log.Error(ex.Message);
            }
        }
    }
}
