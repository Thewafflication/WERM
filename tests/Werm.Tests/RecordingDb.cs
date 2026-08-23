using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Werm.Data.Connection;

namespace Werm.Tests
{
    internal sealed class RecordingDbConnectionFactory : IWermDbConnectionFactory
    {
        private readonly RecordingDbConnection _connection;

        public RecordingDbConnectionFactory(RecordingDbConnection connection)
        {
            _connection = connection;
        }

        public IDbConnection OpenConnection()
        {
            return _connection;
        }
    }

    internal sealed class RecordingDbConnection : IDbConnection
    {
        public RecordingDbConnection()
        {
            Commands = new List<RecordingDbCommand>();
            State = ConnectionState.Open;
        }

        public Func<RecordingDbCommand, int> NonQueryHandler { get; set; }
        public Func<RecordingDbCommand, object> ScalarHandler { get; set; }
        public Func<RecordingDbCommand, DataTable> ReaderHandler { get; set; }
        public List<RecordingDbCommand> Commands { get; private set; }
        public RecordingDbTransaction LastTransaction { get; private set; }

        public string ConnectionString { get; set; }
        public int ConnectionTimeout { get { return 30; } }
        public string Database { get { return "recording"; } }
        public ConnectionState State { get; private set; }

        public IDbTransaction BeginTransaction()
        {
            return BeginTransaction(IsolationLevel.Unspecified);
        }

        public IDbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            LastTransaction = new RecordingDbTransaction(this, isolationLevel);
            return LastTransaction;
        }

        public void ChangeDatabase(string databaseName)
        {
        }

        public void Close()
        {
            State = ConnectionState.Closed;
        }

        public IDbCommand CreateCommand()
        {
            var command = new RecordingDbCommand(this);
            Commands.Add(command);
            return command;
        }

        public void Open()
        {
            State = ConnectionState.Open;
        }

        public void Dispose()
        {
            Close();
        }

        internal int ExecuteNonQuery(RecordingDbCommand command)
        {
            return NonQueryHandler == null ? 1 : NonQueryHandler(command);
        }

        internal object ExecuteScalar(RecordingDbCommand command)
        {
            return ScalarHandler == null ? 1L : ScalarHandler(command);
        }

        internal IDataReader ExecuteReader(RecordingDbCommand command)
        {
            DataTable table = ReaderHandler == null ? new DataTable() : ReaderHandler(command);
            return table.CreateDataReader();
        }
    }

    internal sealed class RecordingDbTransaction : IDbTransaction
    {
        public RecordingDbTransaction(IDbConnection connection, IsolationLevel isolationLevel)
        {
            Connection = connection;
            IsolationLevel = isolationLevel;
        }

        public IDbConnection Connection { get; private set; }
        public IsolationLevel IsolationLevel { get; private set; }
        public int CommitCount { get; private set; }
        public int RollbackCount { get; private set; }

        public void Commit()
        {
            CommitCount++;
        }

        public void Rollback()
        {
            RollbackCount++;
        }

        public void Dispose()
        {
        }
    }

    internal sealed class RecordingDbCommand : IDbCommand
    {
        private readonly RecordingDbConnection _recordingConnection;

        public RecordingDbCommand(RecordingDbConnection connection)
        {
            _recordingConnection = connection;
            Connection = connection;
            Parameters = new RecordingParameterCollection();
            CommandType = CommandType.Text;
        }

        public string CommandText { get; set; }
        public int CommandTimeout { get; set; }
        public CommandType CommandType { get; set; }
        public IDbConnection Connection { get; set; }
        public IDataParameterCollection Parameters { get; private set; }
        public IDbTransaction Transaction { get; set; }
        public UpdateRowSource UpdatedRowSource { get; set; }

        public void Cancel()
        {
        }

        public IDbDataParameter CreateParameter()
        {
            return new RecordingDbParameter();
        }

        public int ExecuteNonQuery()
        {
            return _recordingConnection.ExecuteNonQuery(this);
        }

        public IDataReader ExecuteReader()
        {
            return _recordingConnection.ExecuteReader(this);
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            return ExecuteReader();
        }

        public object ExecuteScalar()
        {
            return _recordingConnection.ExecuteScalar(this);
        }

        public void Prepare()
        {
        }

        public void Dispose()
        {
        }
    }

    internal sealed class RecordingDbParameter : IDbDataParameter
    {
        public DbType DbType { get; set; }
        public ParameterDirection Direction { get; set; }
        public bool IsNullable { get { return true; } }
        public string ParameterName { get; set; }
        public string SourceColumn { get; set; }
        public DataRowVersion SourceVersion { get; set; }
        public object Value { get; set; }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public int Size { get; set; }
    }

    internal sealed class RecordingParameterCollection : ArrayList, IDataParameterCollection
    {
        public object this[string parameterName]
        {
            get { return this[IndexOf(parameterName)]; }
            set
            {
                int index = IndexOf(parameterName);
                if (index < 0)
                {
                    Add(value);
                }
                else
                {
                    this[index] = value;
                }
            }
        }

        public bool Contains(string parameterName)
        {
            return IndexOf(parameterName) >= 0;
        }

        public int IndexOf(string parameterName)
        {
            for (int index = 0; index < Count; index++)
            {
                var parameter = this[index] as IDataParameter;
                if (parameter != null && string.Equals(
                    parameter.ParameterName, parameterName, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }
            return -1;
        }

        public void RemoveAt(string parameterName)
        {
            int index = IndexOf(parameterName);
            if (index >= 0)
            {
                RemoveAt(index);
            }
        }
    }
}
