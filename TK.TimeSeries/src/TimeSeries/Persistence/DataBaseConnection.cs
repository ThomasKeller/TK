using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Diagnostics.CodeAnalysis;
using TK.Logging;

namespace TK.TimeSeries.Persistence
{
    public class DataBaseConnection
    {
        private static ILogger _logger = LoggerFactory.CreateLoggerFor(typeof(DataBaseConnection));
        private static readonly object _lockLocal = new object();
        private static readonly object _lockRemote = new object();
        private static SqlCeConnection _sqlCeConnection;
        private static SqlConnection _sqlConnection;

        public static void InitSqlCeConnection(string connectionString)
        {
            _logger.DebugFormat("Connection: {0}", connectionString);
            lock (_lockLocal)
            {
                _sqlCeConnection = new SqlCeConnection(connectionString);
            }
        }

        public static void InitSqlConnection(string connectionString)
        {
            lock (_lockRemote)
            {
                _logger.DebugFormat("Connection: {0}", connectionString);
                _sqlConnection = new SqlConnection(connectionString);
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static SqlCeConnection GetSqlCeConnection()
        {
            lock (_lockLocal)
            {
                if (_sqlCeConnection == null)
                    throw new Exception("SqlCE connection not initialized: use InitSqlConnection");
                if (_sqlCeConnection.State == ConnectionState.Closed ||
                    _sqlCeConnection.State == ConnectionState.Broken)
                {
                    _logger.Info("Open connection to local database:");
                    try
                    {
                        _sqlCeConnection.Open();
                    }
                    catch (SqlCeInvalidDatabaseFormatException ex)
                    {
                        _logger.ErrorFormat("Cannot open local database: {0}", ex.Message);
                        var sqlEngine = new SqlCeEngine(_sqlCeConnection.ConnectionString);
                        _logger.Info("Upgrade local database");
                        sqlEngine.Upgrade();
                        _logger.Debug("Open connection to local database after upgrade:");
                        _sqlCeConnection.Open();
                    }
                }
            }
            return _sqlCeConnection;
        }

        public static SqlConnection GetSqlConnection()
        {
            lock (_lockRemote)
            {
                if (_sqlConnection == null)
                    throw new Exception("Sql connection not initialized: use InitSqlConnection");
                if (_sqlConnection.State == ConnectionState.Closed ||
                    _sqlConnection.State == ConnectionState.Broken)
                {
                    _logger.Debug("Open connection to remote database:");
                    _sqlConnection.Open();
                }
            }
            return _sqlConnection;
        }
    }
}