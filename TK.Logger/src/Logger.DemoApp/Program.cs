using System;
using TK.Logging;

namespace TK.LoggerDemoApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var logger = LoggerFactory.CreateLoggerFor(typeof(Program));

            logger.InfoFormat("Program started {0}", DateTime.Now);
        }
    }
}