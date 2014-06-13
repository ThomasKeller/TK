using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TK.Logging;

namespace TK.LoggerTest
{
    [TestFixture]
    public class LoggerTest
    {
        [Test]
        public void BootstrapStart()
        {
            ILogger logger = LoggerFactory.CreateLoggerFor(typeof(LoggerTest));

            logger.Debug("DebugMsg 1");
            logger.Debug("DebugMsg 2", new Exception("Debug Exception"));
            logger.DebugFormat("{0} {1} {2}", 1, 2, 3);

            logger.Error("ErrorMsg 1");
            logger.Error("ErrorMsg 2", new Exception("Error Exception"));
            logger.ErrorFormat("{0} {1} {2}", 4, 5, 6);

            logger.Info("InfoMsg 1");
            logger.Info("InfoMsg 2", new Exception("Info Exception"));
            logger.InfoFormat("{0} {1} {2}", 1, 2, 3);

            logger.Warn("WarnMsg 1");
            logger.Warn("WarnMsg 2", new Exception("Warn Exception"));
            logger.WarnFormat("{0} {1} {2}", 4, 5, 6);

            logger.Fatal("FatalMsg 1");
            logger.Fatal("FatalMsg 2", new Exception("Fatal Exception"));
            logger.FatalFormat("{0} {1} {2}", 1, 2, 3);
        }
        


    }
}
