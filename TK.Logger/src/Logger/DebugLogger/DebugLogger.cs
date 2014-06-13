using System;
using Output = System.Diagnostics;

namespace TK.Logging.DebugLogger
{
    public class DebugLogger : ILogger
    {
        private readonly string _name;
        private const string c_Info  = "Info  ";
        private const string c_Debug = "Debug ";
        private const string c_Error = "Error ";
        private const string c_Fatal = "Fatal ";
        private const string c_Warn =  "Warning";

        public DebugLogger(string name)
        {
            _name = name;
        }

        public DebugLogger(Type type)
        {
            _name = type.Name;
        }
        
        public bool IsDebugEnabled { get { return true; } }

        public void Debug(object message)
        {
            Log(c_Debug, message);
        }

        public void Debug(object message, Exception exception)
        {
            Log(c_Debug, message, exception);
        }

        public void DebugFormat(string format, params object[] args)
        {
            Log(c_Debug, format, args);
        }


        public bool IsErrorEnabled { get { return true; } }

        public void Error(object message)
        {
            Log(c_Error, message);
        }

        public void Error(object message, Exception exception)
        {
            Log(c_Error, message, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            Log(c_Error, format, args);               
        }

        public bool IsFatalEnabled { get { return true; } }

        public void Fatal(object message)
        {
            Log(c_Fatal, message);
        }

        public void Fatal(object message, Exception exception)
        {
            Log(c_Fatal, exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            Log(c_Fatal, format, args);            
        }

        public bool IsInfoEnabled { get { return true; } }

        public void Info(object message)
        {
            Log(c_Info, message);            
        }

        public void Info(object message, Exception exception)
        {
            Log(c_Info, message, exception);            
        }

        public void InfoFormat(string format, params object[] args)
        {
            Log(c_Info, format, args);
        }

        public bool IsWarnEnabled { get { return true; } }

        public void Warn(object message)
        {
            Log(c_Warn, message);
        }

        public void Warn(object message, Exception exception)
        {
            Log(c_Warn, message, exception);            
        }

        public void WarnFormat(string format, params object[] args)
        {
            Log(c_Warn, format, args);   
        }

        private void Log(string logType, object message)
        {
            Output.Debug.WriteLine(string.Format("{0}: {1}: {2}", logType, _name, message));
        }


        private void Log(string logType, object message, Exception ex)
        {
            Output.Debug.WriteLine(string.Format("{0}: {1}: {2}\r\n      : {3}", logType, _name, message, ex.Message));
        }

        private void Log(string logType, string format, params object[] args)
        {
            Output.Debug.WriteLine(string.Format("{0}: {1}:", logType, _name));
            Output.Debug.Write("      : ");
            Output.Debug.WriteLine(format, args);
        }
    }
}
