using System;
using System.Collections.Generic;
using System.Reflection;
using TK.Logging;
using TK.PluginManager;
using TK.WebPageParser;

namespace TK.WebPageParserPlugin
{
    public class WebPageParserPlugin : SchedulePluginBase
    {
        private const string c_PluginName = "WebPageParserPlugin";
        private const string c_QueuePath = @".\Private$\WebPageParserPlugin";

        private static readonly ILogger _Logger = LoggerFactory.GetCurrentClassLogger();

        public string UrlSolarWetter { get; private set; }
        public string CronJobText { get; private set; }

        public override Dictionary<string, object> GetParameters()
        {
            return new Dictionary<string, object> { { "UrlSolarWetter", UrlSolarWetter },
                                				    { "CronJobText", CronJobText } };
        }

        public override void Initialize(Dictionary<string, string> initParams)
        {
            UrlSolarWetter = (initParams.ContainsKey("UrlSolarWetter") ? 
                initParams["UrlSolarWetter"] :
                @"http://www.vorhersage-plz-bereich.solar-wetter.com/html/410.html");
            CronJobText = (initParams.ContainsKey("CronJob") ? 
                initParams["CronJob"] :
                "0 0 5,9,13,17 * * ?");

            var parameters = new Dictionary<string, object>();
            parameters.Add("UrlSolarWetter", UrlSolarWetter);
            _Scheduler.AddJob(CronJobText, Execute, parameters, false);
            InitQueue(c_QueuePath, c_PluginName);
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
                _Logger.DebugFormat("Execute Job. Thread ID: {0}", System.Threading.Thread.CurrentThread.ManagedThreadId);
                var result = ParseWebPage(parameters);
                result.PluginName = c_PluginName;
                SendToQueue(result);
                _Logger.DebugFormat("Job Completed. Thread ID: {0}", System.Threading.Thread.CurrentThread.ManagedThreadId);
            }
            catch (Exception ex)
            {
                _Logger.FatalFormat("Fatal Error", ex.Message);
            }
        }

        private static MeasureValueBox ParseWebPage(IDictionary<string, object> parameters)
        {
            _Logger.DebugFormat("Read Value from Web Page - {0} ThreadId: {1}",
                        System.DateTime.Now.ToString("r"),
                        System.Threading.Thread.CurrentThread.ManagedThreadId);

            string webPage = WebPagerReader.Download(parameters["UrlSolarWetter"] as string);
            if (!string.IsNullOrEmpty(webPage))
            {
                return SolarWetterWebPageParser.Parse(webPage);
            }
            _Logger.Error("Web Page is emtpy");
            return null;
        }
    }
}