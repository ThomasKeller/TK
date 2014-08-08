using System;
using System.Collections.Generic;
using System.Threading;
using Quartz;
using TK.Logging;
using TK.SimpleMessageQueue;
using TK.WebPageParser;

namespace TK.WebPageParserPlugin
{
    public class ParserJob : IJob
    {
        private static ILogger _Logger = LoggerFactory.CreateLoggerFor(typeof(ParserJob));
        private static SimpleMessageQueueWrapper<IDictionary<string, object>> _Queue;
        private static string _QueuePath = ".\\Private$\\WebPageValues";

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                ParserJob._Logger.DebugFormat("Execute ParserJob. Thread ID: {0}", Thread.CurrentThread.ManagedThreadId);
                ParserJob.InitializeQueue();
                IDictionary<string, object> result = this.ParseWebPages(context);
                ParserJob.SendToQueue(result);
                ParserJob._Logger.DebugFormat("ParserJob Completed. Thread ID: {0}", Thread.CurrentThread.ManagedThreadId);
            }
            catch (Exception ex)
            {
                ParserJob._Logger.FatalFormat("Fatal Error", ex.Message);
            }
        }

        private IDictionary<string, object> ParseWebPages(IJobExecutionContext context)
        {
            string webPage = ParserJob.LoadWebPage(context.JobDetail.JobDataMap["UrlSolarWetter"] as string);
            if (!string.IsNullOrEmpty(webPage))
            {
                return SolarWetterWebPageParser.Parse(webPage);
            }
            ParserJob._Logger.Error("Web Page is emtpy");
            return null;
        }

        private static void InitializeQueue()
        {
            try
            {
                if (ParserJob._Queue == null)
                {
                    ParserJob._Logger.Debug("Create new Queue");
                    ParserJob._Queue = new SimpleMessageQueueWrapper<IDictionary<string, object>>
                    {
                        MessageLabel = "WebPages"
                    };
                    ParserJob._Queue.Initialize(ParserJob._QueuePath);
                }
            }
            catch (Exception ex)
            {
                ParserJob._Logger.FatalFormat("Failed to initialize queue: {0}", new object[]
				{
					ex.Message
				});
                ParserJob._Queue = null;
            }
        }

        private static void SendToQueue(IDictionary<string, object> result)
        {
            if (result != null)
            {
                ParserJob._Queue.Send(result);
            }
        }

        private static string LoadWebPage(string url)
        {
            Console.WriteLine(string.Format("Read Value from Web Pages - {0} {1}", DateTime.Now.ToString("r"), Thread.CurrentThread.ManagedThreadId));
            return WebPagerReader.Download(url, "", "");
        }
    }
}