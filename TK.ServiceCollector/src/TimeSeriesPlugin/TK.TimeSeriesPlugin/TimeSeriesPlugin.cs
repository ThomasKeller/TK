using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Quartz;
using TK.Logging;
using TK.PluginManager;
using TK.TimeSeries.Core;
using TK.TimeSeries.Persistence;

namespace TK.TimeSeriesPlugin
{
    public class TimeSeriesPlugin : QuartzPluginBase
    {
        private const string c_PluginName = "TimeSeriesPlugin";
        private static ILogger _Logger = LoggerFactory.CreateLoggerFor(typeof(TimeSeriesJob));
        private CompressionConditionManager _CompressionConditionManager = new CompressionConditionManager();

        public string LocalDB { get; set; }

        public string RemoteDB { get; set; }

        public string MessageQueuePaths { get; set; }

        public override Dictionary<string, object> GetParameters()
        {
            return new Dictionary<string, object> {
                { "LocalDB", LocalDB },
                { "RemoteDB", RemoteDB },
                { "MessageQueuePaths", this.MessageQueuePaths },
				{ "CronJobText", this._CronJobText } };
        }

        public override void Initialize(Dictionary<string, string> initParams)
        {
            try
            {
                TimeSeriesPlugin._Logger.InfoFormat("Initialize {0}", new object[]
				{
					"TimeSeriesPlugin"
				});
                this.LocalDB = (initParams.ContainsKey("LocalDB") ? initParams["LocalDB"] : "Data Source=MeasuredValueDB.sdf;Password=;Persist Security Info=True");
                TimeSeriesPlugin._Logger.InfoFormat("LocalDB:           {0}", new object[]
				{
					this.LocalDB
				});
                this.RemoteDB = (initParams.ContainsKey("RemoteDB") ? initParams["RemoteDB"] : "Data Source=.;Initial Catalog=HomeAutomation;Integrated Security=True");
                TimeSeriesPlugin._Logger.InfoFormat("RemoteDB:          {0}", new object[]
				{
					this.RemoteDB
				});
                this.MessageQueuePaths = (initParams.ContainsKey("MessageQueuePaths") ? initParams["MessageQueuePaths"] : ".\\Private$\\KostalValues");
                TimeSeriesPlugin._Logger.InfoFormat("MessageQueuePaths: {0}", new object[]
				{
					this.MessageQueuePaths
				});
                this._CronJobText = (initParams.ContainsKey("CronJob") ? initParams["CronJob"] : "0/30 * * * * ?");
                TimeSeriesPlugin._Logger.InfoFormat("CronJob:           {0}", new object[]
				{
					this._CronJobText
				});
                DataBaseConnection.InitSqlCeConnection(this.LocalDB);
                DataBaseConnection.InitSqlConnection(this.RemoteDB);
                string text = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\CompressionConfigs.xml";
                TimeSeriesPlugin._Logger.InfoFormat("Load Config File: {0}", new object[]
				{
					text
				});
                this._CompressionConditionManager = CompressionConditionManager.LoadFromFile(text);
                this.InitializeJob();
            }
            catch (Exception ex)
            {
                TimeSeriesPlugin._Logger.FatalFormat("Cannot server initialization failed. Plugin will not start.", new object[]
				{
					ex.Message
				});
            }
        }

        public override string PluginName()
        {
            return "TimeSeriesPlugin";
        }

        public override string PluginVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        protected override void InitializeJob()
        {
            this._JobDetail = JobBuilder.Create<TimeSeriesJob>().Build();
            this._JobDetail.JobDataMap.Add("CompressionConditionManager", this._CompressionConditionManager);
            this._JobDetail.JobDataMap.Add("MessageQueuePaths", this.MessageQueuePaths);
            ITrigger trigger = TriggerBuilder.Create().WithCronSchedule(this._CronJobText).Build();
            this._Scheduler.ScheduleJob(this._JobDetail, trigger);
        }
    }
}