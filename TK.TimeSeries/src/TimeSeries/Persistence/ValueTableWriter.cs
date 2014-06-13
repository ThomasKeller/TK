using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Diagnostics.Contracts;
using TK.TimeSeries.Core;
using TK.Logging;

namespace TK.TimeSeries.Persistence
{
    public enum CalculateValue
    {
        NoCalculation,
        DeltaValuesTimeDivided
    }

    public class ValueTableWriter
    {
        static private ILogger _logger = LoggerFactory.CreateLoggerFor(typeof(ValueTableWriter));

        /// <summary>
        /// Reads the last measured value from the local database.
        /// If there is no value found a instance of MeasuredValue with an 
        /// OPCQuality of OPCQuality.NoValue is returned.
        /// </summary>
        /// <param name="valueName">name of the measured value (tag name)</param>
        /// <param name="typeCode">indicates the table which is been used. e.g. TypeCode.String => Table: StringValues</param>
        /// <returns>measured values</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static MeasuredValue ReadLastMeasuredValueFromLocalDB(string valueName, TypeCode typeCode)
        {
            Contract.Requires<ArgumentException>(false == string.IsNullOrEmpty(valueName));
            MeasuredValue value = new MeasuredValue();
            string sql = "SELECT Name, MeasuredDate, Value, Quality, Remark FROM {0} " +
                          "WHERE Name = @Name AND MeasuredDate IN" +
                          " (SELECT max(MeasuredDate) FROM {0} WHERE Name = @Name)";
            sql = string.Format(sql, GetTableNameFor(typeCode));

            using (var command = new SqlCeCommand(sql, DataBaseConnection.GetSqlCeConnection())) {
                command.Parameters.AddWithValue("Name", valueName);
                using(SqlCeDataReader sr =  command.ExecuteReader(System.Data.CommandBehavior.SingleRow)) {
                    if (sr.Read()) {
                        value.Name = (string)sr["Name"];
                        value.TimeStamp = (DateTime)sr["MeasuredDate"];
                        value.Value = sr["Value"];
                        value.Quality = (OPCQuality)(byte)sr["Quality"];
                        value.Description = (string)sr["Remark"];
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// save the measured value to the local database if the
        /// conditions were met.
        /// </summary>
        /// <param name="currentValue">current value</param>
        /// <param name="condition">condition to check</param>
        /// <returns>measured values</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static int SaveValueWhenConditionsAreMet(MeasuredValue currentValue, CompressionCondition condition)
        {
            Contract.Requires(currentValue != null);
            
            // if the current value has the OPCQuality of "NoValue"
            // return without writting the value
            if (currentValue.Quality == OPCQuality.NoValue)
            {
                _logger.DebugFormat("current value {0} has the OPCQuality of \"NoValue\"", currentValue.Name);
                return 0;
            }

            TypeCode typeCode = currentValue.GetTypeCode();
            MeasuredValue previousValue = ReadLastMeasuredValueFromLocalDB(currentValue.Name, typeCode);
            if (false == condition.ShouldCurrentValueBeWritten(currentValue, previousValue))
                return 0;
            int rowAffected = InsertIntoDatabase(currentValue, typeCode);
            return rowAffected;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private static int InsertIntoDatabase(MeasuredValue currentValue, TypeCode typeCode)
        {
            string sql = "INSERT INTO {0} (Name, MeasuredDate, Value, Quality, Remark) " +
                          "values(@Name, @MeasuredDate, @Value, @Quality, @Remark);";
            sql = string.Format(sql, GetTableNameFor(typeCode));

            int rowAffected = 0;
            using (var command = new SqlCeCommand(sql, DataBaseConnection.GetSqlCeConnection())) {
                command.Parameters.AddWithValue("Name", currentValue.Name);
                command.Parameters.AddWithValue("MeasuredDate", currentValue.TimeStamp);
                command.Parameters.AddWithValue("Value", currentValue.Value);
                command.Parameters.AddWithValue("Quality", (byte)currentValue.Quality);
                command.Parameters.AddWithValue("Remark", currentValue.Description);
                rowAffected = command.ExecuteNonQuery();
            }

            _logger.DebugFormat("Row affected: {0} - {1}", rowAffected, sql);
            return rowAffected;
        }

        /// <summary>
        /// Transfer all measured values to the remote database that 
        /// are not there.
        /// </summary>
        public static void TransferDataToDestDB()
        {
            SyncData(TypeCode.Boolean);
            SyncData(TypeCode.Double);
            SyncData(TypeCode.String);
            SyncData(TypeCode.Int32);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private static IDictionary<string, DateTime> ReadLastMeasuredDatesFromLocalDB(TypeCode typeCode)
        {
            var dic = new Dictionary<string, DateTime>();
            string sql = "SELECT MAX(MeasuredDate) AS MaxMeasuredDate, Name " +
                         "FROM {0} " +
                         "GROUP BY Name";
            sql = string.Format(sql, GetTableNameFor(typeCode));
            using (var command = new SqlCeCommand(sql, DataBaseConnection.GetSqlCeConnection())) {
                using (SqlCeDataReader sr = command.ExecuteReader()) {
                    while (sr.Read()) {
                        dic.Add(
                            (string)sr["Name"],
                            (DateTime)sr["MaxMeasuredDate"]);
                    }
                }
            }
            return dic;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private static IDictionary<string, DateTime> ReadLastMeasuredDatesFromDestDB(TypeCode typeCode)
        {
            var dic = new Dictionary<string, DateTime>();
            string sql = "SELECT MAX(MeasuredDate) AS MaxMeasuredDate, Name " +
                         "FROM {0} " +
                         "GROUP BY Name";
            sql = string.Format(sql, GetTableNameFor(typeCode));
            using (var command = new SqlCommand(sql, DataBaseConnection.GetSqlConnection())) {
                using (SqlDataReader sr = command.ExecuteReader()) {
                    while (sr.Read()) {
                        dic.Add(
                            (string)sr["Name"],
                            (DateTime)sr["MaxMeasuredDate"]);
                    }
                }
            }
            return dic;
        }

        private static string GetTableNameFor(TypeCode typeCode)
        {
            switch (typeCode) {
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.DBNull:
                case TypeCode.Object:
                case TypeCode.DateTime:
                case TypeCode.Empty:
                case TypeCode.SByte:
                case TypeCode.Decimal:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    throw new Exception("not implemented");
                case TypeCode.Boolean:
                    return "BoolValues";
                case TypeCode.Double:
                    return "DoubleValues";
                case TypeCode.Single:
                    throw new Exception("not implemented: use Double");
                case TypeCode.Int16:
                case TypeCode.Int64:
                    throw new Exception("not implemented: use Int32");
                case TypeCode.Int32:
                    return "IntValues";
                    throw new Exception("not implemented");
                case TypeCode.String:
                    return "StringValues";
            }
            throw new Exception("not implemented");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private static void SyncData(TypeCode typeCode)
        {
            var sourceElements = ReadLastMeasuredDatesFromLocalDB(typeCode);
            var destElements = ReadLastMeasuredDatesFromDestDB(typeCode);
            var syncElements = new Dictionary<string,DateTime>();

            foreach (var sourceElement in sourceElements) {
                if (destElements.ContainsKey(sourceElement.Key)) {
                    if (sourceElement.Value > destElements[sourceElement.Key]) {
                        syncElements.Add(sourceElement.Key, destElements[sourceElement.Key]);    
                    }
                }
                else {
                    // destination doesn't contain the tagname
                    // therefore we copy all tags of that name
                    syncElements.Add(sourceElement.Key, new DateTime(2013, 01, 01));
                }
            }

            foreach (var syncElement in syncElements) {
                string sql = "SELECT MeasuredDate, Name, Value, Quality, Remark FROM {0} " +
                             "WHERE Name = @Name AND MeasuredDate > @MeasureDate";
                sql = string.Format(sql, GetTableNameFor(typeCode));
                using (var command = new SqlCeCommand(sql, DataBaseConnection.GetSqlCeConnection())) {
                    command.Parameters.AddWithValue("Name", syncElement.Key); 
                    command.Parameters.AddWithValue("MeasureDate", syncElement.Value);
                    using (SqlCeDataReader sr = command.ExecuteReader()) {

                        using (var sqlBulkCopy = new SqlBulkCopy(
                            DataBaseConnection.GetSqlConnection())) {
                            sqlBulkCopy.DestinationTableName = GetTableNameFor(typeCode);
                            _logger.DebugFormat("BulkCopy: {0}", sqlBulkCopy.DestinationTableName);
                            sqlBulkCopy.WriteToServer(sr);
                        }
                    }
                }
            }
        }
    }
}
