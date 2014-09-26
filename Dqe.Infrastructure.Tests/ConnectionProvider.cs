using System;
using System.Data;
using System.Data.SqlClient;
using NHibernate.Connection;

namespace Dqe.Infrastructure.Tests
{
    public class CustomConnectionProvider : ConnectionProvider
    {
        public override IDbConnection GetConnection()
        {
            var connection = new SqlConnection(string.Format(@"Data Source=(LocalDB)\v11.0;AttachDbFilename={0}\Database1.mdf;Integrated Security=True", AppDomain.CurrentDomain.BaseDirectory));
            connection.Open();
            return connection;
        }
    }
}
