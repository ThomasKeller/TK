using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TK.Logging;
using System.Threading;
using TK.TimeSeries.Core;

namespace TK.TimeSeries.Persistence
{
    public class SqlServerSynchronizer
    {
        private static ILogger _logger = LoggerFactory.CreateLoggerFor(typeof(SqlServerSynchronizer));
        private static Thread _thread = null;
        private static int _interval = 30 * 1000;

        public static void Start(string remoteConnectionString, int interval_sec)
        {
            if (_thread == null)
            {
                _interval = interval_sec * 1000;
                DataBaseConnection.InitSqlConnection(remoteConnectionString);
                _thread = new Thread(new ThreadStart(SqlServerSynchronizer.ThreadEntryPoint));
                _thread.Start();
            }
        }

        public static void Stop()
        {
            if (_thread != null)
            {
                _thread.Abort();
                while(_thread.IsAlive)
                {
                    Thread.Sleep(100);   
                }
                _thread = null;
            }
        }

        private static void ThreadEntryPoint()
        {
            while (_thread.IsAlive)
            {
                DoWork();
                Thread.Sleep(_interval);
            }
        }

        private static void DoWork()
        {
            try
            {
                ValueTableWriter.TransferDataToDestDB();
            }
            catch (Exception ex)
            {
                _logger.Error("Exception:", ex);
                Console.WriteLine(ex.Message);
            }
        }
    }
}
