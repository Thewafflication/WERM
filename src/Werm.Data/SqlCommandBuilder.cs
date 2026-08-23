using System;
using System.Data;
using System.Globalization;

namespace Werm.Data
{
    internal struct SqlParameterValue
    {
        public SqlParameterValue(DbType type, object value)
        {
            Type = type;
            Value = value ?? DBNull.Value;
        }

        public DbType Type { get; private set; }
        public object Value { get; private set; }
    }

    internal static class SqlCommandBuilder
    {
        public static IDbCommand Create(
            IDbConnection connection,
            IDbTransaction transaction,
            string commandText,
            params SqlParameterValue[] values)
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandTimeout = 30;
            command.Transaction = transaction;

            for (int index = 0; index < values.Length; index++)
            {
                IDbDataParameter parameter = command.CreateParameter();
                parameter.ParameterName = "p" + index.ToString(CultureInfo.InvariantCulture);
                parameter.DbType = values[index].Type;
                parameter.Value = values[index].Value;
                command.Parameters.Add(parameter);
            }

            return command;
        }

        public static SqlParameterValue Text(string value)
        {
            return new SqlParameterValue(DbType.String, value);
        }

        public static SqlParameterValue Integer(long value)
        {
            return new SqlParameterValue(DbType.Int64, value);
        }

        public static SqlParameterValue NullableInteger(long? value)
        {
            return new SqlParameterValue(DbType.Int64, value.HasValue ? (object)value.Value : null);
        }

        public static SqlParameterValue Boolean(bool value)
        {
            return Integer(value ? 1 : 0);
        }

        public static string FormatUtc(DateTimeOffset value)
        {
            return value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", CultureInfo.InvariantCulture);
        }

        public static DateTimeOffset ParseUtc(object value)
        {
            return DateTimeOffset.Parse(
                Convert.ToString(value, CultureInfo.InvariantCulture),
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        }
    }
}
