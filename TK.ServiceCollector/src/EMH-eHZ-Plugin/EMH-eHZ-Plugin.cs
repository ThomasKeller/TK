using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EMH_eHZ_Plugin.SerialPort;
using TK.PluginManager;

namespace EMH_eHZ_Plugin
{
    public class EMH_eHZ_Plugin : PluginBase
    {
        private const string c_PluginName = @"EMH-eHZ-Plugin";
        private const string c_QueuePath = @".\Private$\EMH_eHZ_PlugIn";
        private SerialPortLogger _SerialPortLogger;
        
        public string ComPort { get; private set; }

        public override string PluginName()
        {
            return c_PluginName;
        }

        public override string PluginVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public override Dictionary<string, object> GetParameters()
        {
            var result = new Dictionary<string, object>();
            result.Add("ComPort", ComPort);
            return result;
        }

        public override void Initialize(Dictionary<string, string> initParams)
        {
            ComPort = initParams.ContainsKey("ComPort") ? initParams["ComPort"] : @"COM4";
            var parameters = new Dictionary<string, object>();
            parameters.Add("ComPort", ComPort);
        }

        public override void Start()
        {
            _SerialPortLogger = new SerialPortLogger(ComPort, 30);
        }

        public override void Pause()
        {
            Stop();
        }

        public override void Resume()
        {
            Start();
        }

        public override void Stop()
        {
            _SerialPortLogger = null;
        }
    }
}
