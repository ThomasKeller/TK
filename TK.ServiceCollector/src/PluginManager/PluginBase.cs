using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace TK.PluginManager
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public abstract class PluginBase
    {
        public abstract string PluginName();
        public abstract string PluginVersion();
        public abstract Dictionary<string, object> GetParameters();
        public abstract void Initialize(Dictionary<string, string> initParams);
        public abstract void Start();
        public abstract void Pause();
        public abstract void Resume();
        public abstract void Stop();
    }
}
