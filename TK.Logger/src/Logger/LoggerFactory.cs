using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using TK.Logging.Log4Net;
using TK.Logging.DebugLogger;

namespace TK.Logging
{
    public enum LoggerType
    {
        Log4Net,
        DebugLogger,
        NoLogger
    }

    public static class LoggerFactory
    {
        private static readonly ILoggerFactory _logFactory;

        // Empty static constructor - forces laziness
        static LoggerFactory()
        {
            LoggerType loggerType = LoggerType.NoLogger;
            // read the logger name from the assembly configuration file
            string loggerName = ConfigurationManager.AppSettings["Logger"];

            // parse logger name if available
            if (!string.IsNullOrEmpty(loggerName))
            {
                if (!Enum.TryParse<LoggerType>(loggerName, out loggerType))
                {
                    loggerType = LoggerType.DebugLogger;
                }
            }

            if (loggerType == LoggerType.DebugLogger)
            {
                _logFactory = new DebugLoggerFactory();
            }
            else
            {
                Assembly assembly = Assembly.GetEntryAssembly();
                string path = string.Empty;
                if (assembly != null)
                {
                    path = Path.GetDirectoryName(assembly.Location) + "\\";
                }
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(path + "log4net.def");
                _logFactory = new Log4NetFactory(fileInfo.FullName);
            }
            
            
            // create the right factory instance
            switch (loggerType)
            {
                case LoggerType.DebugLogger: ;
                    break;
                default: 
                    break;
            }
        }

        public static ILogger CreateLoggerFor(string typeName)
        {
            return _logFactory.For(typeName);
        }

        public static ILogger CreateLoggerFor(Type type)
        {
            return _logFactory.For(type);
        }
    }
}