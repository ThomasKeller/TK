using System;
using System.Collections.Generic;
using System.Reflection;
using TK.Logging;
using TK.PluginManager;
using TK.SimpleMessageQueue;
using TK.WebPageParser;

namespace TK.KostalPlugin
{
    public class KostalPlugin : SchedulePluginBase
    {
        private static readonly ILogger _Logger = TK.Logging.LoggerFactory.CreateLoggerFor(typeof(PluginConfiguration));

        private const string c_PluginName = @"KostalPlugin";
        private const string c_QueuePath = @".\Private$\KostalValues2";

        private SimpleMessageQueueWrapper<IDictionary<string, object>> _Queue;
        public string Url { get; private set; }
        public string User { get; private set; }
        public string Password { get; private set; }
        public string CronJobText { get; private set; }

        public override Dictionary<string, object> GetParameters()
        {
            var defaultParameter = new Dictionary<string, object>();
            defaultParameter.Add("URL", Url);
            defaultParameter.Add("User", User);
            defaultParameter.Add("Password", Password);
            defaultParameter.Add("CronJobText", CronJobText);
            return defaultParameter;
        }

        public override void Initialize(Dictionary<string, string> initParams)
        {
            Url = initParams.ContainsKey("URL") ? initParams["URL"] : @"http://192.168.111.34";
            User = initParams.ContainsKey("User") ? initParams["User"] : "pvserver";
            Password = initParams.ContainsKey("Password") ? initParams["Password"] : "geheim";
            CronJobText = initParams.ContainsKey("CronJob") ? initParams["CronJob"] : "0/60 * * * * ?";

            var parameters = new Dictionary<string, object>();
            parameters.Add("URL", Url);
            parameters.Add("User", User);
            parameters.Add("Password", Password);

            _Scheduler.AddJob(CronJobText, Execute, parameters, false);
        }

        public override string PluginName()
        {
            return c_PluginName;
        }

        public override string PluginVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void Execute(IDictionary<string, object> parameters)
        {
            try
            {
                _Logger.DebugFormat("Execute KostalJob. Thread ID: {0}", System.Threading.Thread.CurrentThread.ManagedThreadId);
                InitializeQueue();
                var result = ParseKostalWebPage(parameters);
                SendToQueue(result);
                _Logger.DebugFormat("KostalJob Completed. Thread ID: {0}", System.Threading.Thread.CurrentThread.ManagedThreadId);
            }
            catch (Exception ex)
            {
                _Logger.FatalFormat("Fatal Error", ex.Message);
            }
        }

        private static IDictionary<string, object> ParseKostalWebPage(IDictionary<string, object> parameters)
        {
            _Logger.DebugFormat("Read Value from Kostal - {0} ThreadId: {1}",
                        System.DateTime.Now.ToString("r"),
                        System.Threading.Thread.CurrentThread.ManagedThreadId);

            string webPage = WebPagerReader.Download(
                                parameters["URL"] as string,
                                parameters["User"] as string,
                                parameters["Password"] as string);
            if (!string.IsNullOrEmpty(webPage))
            {
                return KostalWebPageParser.Parse(webPage);
            }
            _Logger.Error("Web Page is emtpy");
            return null;
        }

        private void InitializeQueue()
        {
            try
            {
                if (_Queue == null)
                {
                    _Logger.Debug("Create new Queue");
                    _Queue = new SimpleMessageQueueWrapper<IDictionary<string, object>>() { MessageLabel = "Kostal" };
                    _Queue.Initialize(c_QueuePath);
                }
            }
            catch (Exception ex)
            {
                _Logger.FatalFormat("Failed to initialize queue: {0}", ex.Message);
                _Queue = null;
            }
        }

        private void SendToQueue(IDictionary<string, object> result)
        {
            if (result != null)
            {
                _Queue.Send(result);
            }
            else
            {
                /// ToDO: store data somewhere
            }
        }
    }
}