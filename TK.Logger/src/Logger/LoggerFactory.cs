using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using TK.Logging.Log4Net;
using TK.Logging.DebugLogger;
using System.Runtime.CompilerServices;
using System.Diagnostics;

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

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ILogger GetCurrentClassLogger()
        {
            var frame = new StackFrame(1, false);
            var method = frame.GetMethod();
            MethodBase upperMethod = method;
            for(var offset = 2; ; offset++)
            {
                if((upperMethod == null) || !upperMethod.IsConstructor)
                {
                    break;
                }
                method = upperMethod;
                upperMethod = new StackFrame(offset, false).GetMethod();
            }
            var declaringType = method.DeclaringType;
            return CreateLoggerFor(declaringType);
        }

        public static ILogger CreateLoggerFor(string typeName)
        {
            return _logFactory.For(typeName);
        }

        public static ILogger CreateLoggerFor(Type type)
        {
            return _logFactory.For(type);
        }

        public static ILogger CreateLoggerFor<T>()
        {
            return CreateLoggerFor(typeof(T));
            
        }
    }
}