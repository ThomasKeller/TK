using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TK.Logging;
using TK.PluginManager;
using TK.SimpleMessageQueue;
using TK.TimeSeries.Core;
using TK.TimeSeries.Persistence;

namespace TK.TimeSeriesPlugin
{
    public class TimeSeriesPlugin : SchedulePluginBase
    {
        private const string c_PluginName = "TimeSeriesPlugin";
        private static ILogger _Logger = LoggerFactory.GetCurrentClassLogger();

        private CompressionConditionManager _CompressionConditionManager = new CompressionConditionManager();
        private SimpleMessageQueueWrapper<IDictionary<string, object>> _ErrorQueue;

        public string LocalDB { get; private set; }
        public string RemoteDB { get; private set; }
        public string MessageQueuePaths { get; private set; }
        public string CronJobText { get; private set; }

        public override Dictionary<string, object> GetParameters()
        {
            return new Dictionary<string, object> { { "LocalDB", LocalDB },
                                                    { "RemoteDB", RemoteDB },
                                                    { "MessageQueuePaths", MessageQueuePaths },
                                                    { "CronJobText", _CronJobText } };
        }

        public override void Initialize(Dictionary<string, string> initParams)
        {
            try
            {
                TimeSeriesPlugin._Logger.InfoFormat("Initialize {0}", c_PluginName);

                LocalDB = initParams.ContainsKey("LocalDB") ? 
                    initParams["LocalDB"] : 
                    "Data Source=MeasuredValueDB.sdf;Password=;Persist Security Info=True";
                RemoteDB = initParams.ContainsKey("RemoteDB") ?
                    initParams["RemoteDB"] :
                    @"Data Source=.;Initial Catalog=HomeAutomation;Integrated Security=True";
                MessageQueuePaths = initParams.ContainsKey("MessageQueuePaths") ?
                    initParams["MessageQueuePaths"] :
                    @".\Private$\KostalValues";
                CronJobText = initParams.ContainsKey("CronJob") ?
                    initParams["CronJob"] :
                    "0/30 * * * * ?";

                _Logger.InfoFormat("LocalDB:           {0}", LocalDB);
                _Logger.InfoFormat("RemoteDB:          {0}", RemoteDB);
                _Logger.InfoFormat("MessageQueuePaths: {0}", MessageQueuePaths);
                _Logger.InfoFormat("CronJob:           {0}", CronJobText);

                DataBaseConnection.InitSqlCeConnection(LocalDB);
                DataBaseConnection.InitSqlConnection(RemoteDB);

                string configFile = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\CompressionConfigs.xml";
                _Logger.InfoFormat("Load Config File: {0}", configFile);
                _CompressionConditionManager = CompressionConditionManager.LoadFromFile(configFile);

                _ErrorQueue = new SimpleMessageQueueWrapper<IDictionary<string, object>>();
                _ErrorQueue.Initialize(".\\Private$\\ErrorQueue");
                _ErrorQueue.MessageLabel = "Error";

                //this._JobDetail.JobDataMap.Add("CompressionConditionManager", this._CompressionConditionManager);
                //this._JobDetail.JobDataMap.Add("MessageQueuePaths", this.MessageQueuePaths);

                _Scheduler.AddJob(CronJobText, Execute, null, false);
            }
            catch (Exception ex)
            {
                _Logger.Fatal("Cannot server initialization failed. Plugin will not start.", ex);
            }
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
            _Logger.DebugFormat("Execute TimeSeriesJob. Thread ID: {0}", System.Threading.Thread.CurrentThread.ManagedThreadId);
            try
            {
                string messageQueuePaths = MessageQueuePaths;
                foreach (string messageQueuePath in messageQueuePaths.Split(';'))
                {
                    if (string.IsNullOrEmpty(messageQueuePath))
                    {
                        _Logger.Warn("Queue path is empty or null");
                        continue;
                    }
                    TransferQueueToDatebase(messageQueuePath);
                }
                _Logger.DebugFormat("TimeSeriesJob Completed. Thread ID: {0}", System.Threading.Thread.CurrentThread.ManagedThreadId);
            }
            catch (Exception ex)
            {
                _Logger.FatalFormat("Fatal Error", ex.Message);
            }
        }

        private void TransferQueueToDatebase(string messageQueuePath)
        {
            try
            {
                var simpleMessageQueueWrapper = new SimpleMessageQueueWrapper<IDictionary<string, object>>();
                simpleMessageQueueWrapper.Initialize(messageQueuePath);
                var dictionary = simpleMessageQueueWrapper.Peek();
                int num = 500;
                while (dictionary != null && num-- >= 0)
                {
                    IEnumerable<string> source =
                        from key in dictionary.Keys
                        where key.Contains("MeasureTime")
                        select key;
                    string timeKeyName = source.FirstOrDefault<string>();
                    if (timeKeyName != null)
                    {
                        DateTime dateTime = (DateTime)dictionary[timeKeyName];
                        IEnumerable<string> enumerable =
                            from key in dictionary.Keys
                            where key != timeKeyName
                            select key;
                        foreach (string current in enumerable)
                        {
                            MeasuredValue measuredValue = new MeasuredValue();
                            measuredValue.Name = current;
                            measuredValue.Quality = OPCQuality.Good;
                            measuredValue.TimeStamp = (DateTime)dictionary[timeKeyName];
                            measuredValue.Description = "";
                            measuredValue.Value = dictionary[current];
                            if (measuredValue.Value is long)
                            {
                                measuredValue.Value = Convert.ToInt32(measuredValue.Value.ToString());
                            }
                            _Logger.DebugFormat("save to local DB: {0}", measuredValue);
                            ValueTableWriter.SaveValueWhenConditionsAreMet(measuredValue, _CompressionConditionManager.GetConfigFor(current));
                        }
                        dictionary = simpleMessageQueueWrapper.Receive();
                    }
                    else
                    {
                        _Logger.Error("cannot find a 'MeasureTime' in the message directory. Send message to ErrorQueue");
                        _ErrorQueue.Send(dictionary);
                        dictionary = simpleMessageQueueWrapper.Receive();
                    }
                    dictionary = simpleMessageQueueWrapper.Peek();
                }
                ValueTableWriter.TransferDataToDestDB();
            }
            catch (Exception ex)
            {
                _Logger.Error(ex.Message, ex);
            }
        }
    }
}