using System.Data;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;

namespace Dqe.Infrastructure.Driver
{
    public class ExtendedSqlDriver2008 : NHibernate.Driver.Sql2008ClientDriver
    {
        public override IDbCommand GenerateCommand(CommandType type, SqlString sqlString, SqlType[] parameterTypes)
        {
            //var cmd = new SqlDbCommand(base.GenerateCommand(type, sqlString, parameterTypes), sqlString);
            var cmd = base.GenerateCommand(type, sqlString, parameterTypes);
            //if (SqlReviewHelper.Current != null) SqlReviewHelper.Current.ExpandQueryParameters(cmd, sqlString, SqlReviewHelper.FormatForEnum.SqlServer);
            return cmd;
        }

        public override void ExpandQueryParameters(IDbCommand cmd, SqlString sqlString)
        {
            if (SqlReviewHelper.Current != null) SqlReviewHelper.Current.ExpandQueryParameters(cmd, sqlString, SqlReviewHelper.FormatForEnum.SqlServer);
            base.ExpandQueryParameters(cmd, sqlString);
        }
    }

    //public class SqlDbCommand : IDbCommand
    //{
    //    //System.Data.SqlClient.SqlCommand
    //    private readonly IDbCommand _command;
    //    private readonly SqlString _sqlString;

    //    public SqlDbCommand(IDbCommand command, SqlString sqlString)
    //    {
    //        _command = command;
    //        _sqlString = sqlString;
    //    }

    //    public void Dispose()
    //    {
    //        _command.Dispose();
    //    }

    //    public void Prepare()
    //    {
    //        _command.Prepare();
    //    }
    //    public virtual void ExpandQueryParameters(IDbCommand cmd, SqlString sqlString)
    //    {
    //        if (SqlReviewHelper.Current != null)
    //            SqlReviewHelper.Current.ExpandQueryParameters(cmd, sqlString, SqlReviewHelper.FormatForEnum.SqlServer);
    //    }

    //    public void Cancel()
    //    {
    //        _command.Cancel();
    //    }

    //    public IDbDataParameter CreateParameter()
    //    {
    //        return _command.CreateParameter();
    //    }

    //    public int ExecuteNonQuery()
    //    {
    //        ExpandQueryParameters(_command, _sqlString);
    //        return _command.ExecuteNonQuery();
    //    }

    //    public IDataReader ExecuteReader()
    //    {
    //        ExpandQueryParameters(_command, _sqlString);
    //        return _command.ExecuteReader();
    //    }

    //    public IDataReader ExecuteReader(CommandBehavior behavior)
    //    {
    //        return _command.ExecuteReader(behavior);
    //    }

    //    public object ExecuteScalar()
    //    {
    //        return _command.ExecuteScalar();
    //    }

    //    public IDbConnection Connection
    //    {
    //        get { return _command.Connection; }
    //        set { _command.Connection = value; }
    //    }

    //    public IDbTransaction Transaction
    //    {
    //        get { return _command.Transaction; }
    //        set { _command.Transaction = value; }
    //    }
    //    public string CommandText
    //    {
    //        get { return _command.CommandText; }
    //        set { _command.CommandText = value; }
    //    }
    //    public int CommandTimeout
    //    {
    //        get { return _command.CommandTimeout; }
    //        set { _command.CommandTimeout = value; }
    //    }
    //    public CommandType CommandType
    //    {
    //        get { return _command.CommandType; }
    //        set { _command.CommandType = value; }
    //    }
    //    public IDataParameterCollection Parameters
    //    {
    //        get { return _command.Parameters; }
    //    }
    //    public UpdateRowSource UpdatedRowSource
    //    {
    //        get { return _command.UpdatedRowSource; }
    //        set { _command.UpdatedRowSource = value; }
    //    }
    //}
}
