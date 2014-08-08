using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TK.Logging;
using TK.PluginManager;
using TK.TimeSeries.Core;
using TK.TimeSeries.Persistence;
using TK.SimpleMessageQueue;

namespace TK.TimeSeriesPlugin
{
	[DisallowConcurrentExecution]
	public class TimeSeriesJob : IJob
	{
		private static ILogger _Logger;
		private static SimpleMessageQueueWrapper<IDictionary<string, object>> _ErrorQueue;

		static TimeSeriesJob()
		{
			TimeSeriesJob._Logger = LoggerFactory.CreateLoggerFor(typeof(TimeSeriesJob));
			TimeSeriesJob._ErrorQueue = new SimpleMessageQueueWrapper<IDictionary<string, object>>();
			TimeSeriesJob._ErrorQueue.Initialize(".\\Private$\\ErrorQueue");
			TimeSeriesJob._ErrorQueue.MessageLabel = "Error";
		}
		public void Execute(IJobExecutionContext context)
		{
			try
			{
				TimeSeriesJob._Logger.DebugFormat("Execute TimeSeriesJob. Thread ID: {0}", new object[]
				{
					Thread.CurrentThread.ManagedThreadId
				});
				string text = context.JobDetail.JobDataMap["MessageQueuePaths"] as string;
				CompressionConditionManager compressionConditionManager = context.JobDetail.JobDataMap["CompressionConditionManager"] as CompressionConditionManager;
				string[] array = text.Split(new char[]
				{
					';'
				});
				for (int i = 0; i < array.Length; i++)
				{
					string text2 = array[i];
					try
					{
						if (string.IsNullOrEmpty(text2))
						{
							TimeSeriesJob._Logger.Warn("Queue path is empty ot null");
						}
						else
						{
							SimpleMessageQueueWrapper<IDictionary<string, object>> simpleMessageQueueWrapper = new SimpleMessageQueueWrapper<IDictionary<string, object>>();
							simpleMessageQueueWrapper.Initialize(text2);
							IDictionary<string, object> dictionary = simpleMessageQueueWrapper.Peek();
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
										TimeSeriesJob._Logger.DebugFormat("save to local DB: {0}", new object[]
										{
											measuredValue.ToString()
										});
										ValueTableWriter.SaveValueWhenConditionsAreMet(measuredValue, compressionConditionManager.GetConfigFor(current));
									}
									dictionary = simpleMessageQueueWrapper.Receive();
								}
								else
								{
									TimeSeriesJob._Logger.Error("cannot find a 'MeasureTime' in the message directory. Send message to ErrorQueue");
									TimeSeriesJob._ErrorQueue.Send(dictionary);
									dictionary = simpleMessageQueueWrapper.Receive();
								}
								dictionary = simpleMessageQueueWrapper.Peek();
							}
							ValueTableWriter.TransferDataToDestDB();
						}
					}
					catch (Exception ex)
					{
						TimeSeriesJob._Logger.Error(ex.Message, ex);
					}
				}
				TimeSeriesJob._Logger.DebugFormat("TimeSeriesJob Completed. Thread ID: {0}", new object[]
				{
					Thread.CurrentThread.ManagedThreadId
				});
			}
			catch (Exception ex)
			{
				TimeSeriesJob._Logger.FatalFormat("Fatal Error", new object[]
				{
					ex.Message
				});
			}
		}
	}
}
