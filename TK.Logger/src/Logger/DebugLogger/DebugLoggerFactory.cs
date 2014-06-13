namespace TK.Logging.DebugLogger
{
    public class DebugLoggerFactory : ILoggerFactory
    {
        public ILogger For(System.Type type)
        {
            return new DebugLogger(type);
        }

        public ILogger For(string typeName)
        {
            return new DebugLogger(typeName);
        }
    }
}
