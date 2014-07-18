using System;
using Quartz;
using TK.Logging;
using TK.WebPageParser;
using TK.SimpleMessageQueue;
using System.Collections.Generic;

namespace TK.KostalPlugin
{
    public class KostalJob : IJob
    {
        #region Fields

        private static ILogger _Logger = LoggerFactory.CreateLoggerFor(typeof(KostalJob));
        private static SimpleMessageQueueWrapper<IDictionary<string, object>> _Queue;
        private static string _QueuePath = @".\Private$\KostalValues";

        #endregion Fields

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                _Logger.DebugFormat("Execute KostalJob. Thread ID: {0}", System.Threading.Thread.CurrentThread.ManagedThreadId);
                InitializeQueue();
                var result = ParseKostalWebPage(context);
                SendToQueue(result);
                _Logger.DebugFormat("KostalJob Completed. Thread ID: {0}", System.Threading.Thread.CurrentThread.ManagedThreadId);
            }
            catch (Exception ex)
            {
                _Logger.FatalFormat("Fatal Error", ex.Message);
            }
        }

        private IDictionary<string, object> ParseKostalWebPage(IJobExecutionContext context)
        {
            string webPage = LoadWebPage(
                context.JobDetail.JobDataMap["URL"] as string,
                context.JobDetail.JobDataMap["User"] as string,
                context.JobDetail.JobDataMap["Password"] as string);

            if (!string.IsNullOrEmpty(webPage))
            {
                return KostalWebPageParser.Parse(webPage);
            }
            _Logger.Error("Web Page is emtpy");
            return null;
        }

        private static void InitializeQueue()
        {
            try
            {
                if (_Queue == null)
                {
                    _Logger.Debug("Create new Queue");
                    _Queue = new SimpleMessageQueueWrapper<IDictionary<string, object>>() { MessageLabel = "Kostal" };
                    _Queue.Initialize(_QueuePath);
                }
            }
            catch (Exception ex)
            {
                _Logger.FatalFormat("Failed to initialize queue: {0}", ex.Message);
                _Queue = null;
            }
        }

        private static void SendToQueue(IDictionary<string, object> result)
        {
            if (result != null)
            {
                _Queue.Send(result);
            }
        }

        private static string LoadWebPage(string url, string user, string password)
        {
            Console.WriteLine(string.Format("Read Value from Kostal Web Server - {0} {1}",
                System.DateTime.Now.ToString("r"),
                System.Threading.Thread.CurrentThread.ManagedThreadId));
            return WebPagerReader.Download(url, user, password);
        }
    }
}