using System;

namespace TK.Logging
{
    /// <summary>
    /// Factory to create ILog instances
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        /// Gets the logger.
        /// </summary>
        ILogger For(Type type);

        /// <summary>
        /// Gets the logger.
        /// </summary>
        ILogger For(string typeName);
    }
}