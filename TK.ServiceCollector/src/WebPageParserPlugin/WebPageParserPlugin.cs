using System.Collections.Generic;
using System.Reflection;
using Quartz;
using TK.PluginManager;

namespace TK.WebPageParserPlugin
{
    public class WebPageParserPlugin : QuartzPluginBase
    {
        private const string c_PluginName = "WebPageParserPlugin";

        public string UrlSolarWetter { get; set; }

        public override Dictionary<string, object> GetParameters()
        {
            return new Dictionary<string, object> { 
                    {
					    "UrlSolarWetter",
					    this.UrlSolarWetter
				    },
				    {
					    "CronJobText",
					    this._CronJobText
				    }
	    		};
        }

        public override void Initialize(Dictionary<string, string> initParams)
        {
            UrlSolarWetter = (initParams.ContainsKey("UrlSolarWetter") ? initParams["UrlSolarWetter"] : "http://www.vorhersage-plz-bereich.solar-wetter.com/html/410.html");
            _CronJobText = (initParams.ContainsKey("CronJob") ? initParams["CronJob"] : "0 0 5,9,13,17 * * ?");
            InitializeJob();
        }

        public override string PluginName()
        {
            return "WebPageParserPlugin";
        }

        public override string PluginVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        protected override void InitializeJob()
        {
            _JobDetail = JobBuilder.Create<ParserJob>().Build();
            _JobDetail.JobDataMap.Add("UrlSolarWetter", UrlSolarWetter);
            ITrigger trigger = TriggerBuilder.Create().WithCronSchedule(_CronJobText).Build();
            _Scheduler.ScheduleJob(_JobDetail, trigger);
        }
    }
}