using System.Data;
using FDOT.Enterprise;
using FDOT.Enterprise.ConnectionStrings.Client;
using NHibernate.Connection;
using Oracle.DataAccess.Client;

namespace Dqe.Infrastructure.Providers
{
    public class LreConnectionProvider : ConnectionProvider
    {
        public override IDbConnection GetConnection()
        {
            var connectionString = ChannelProvider<IConnectionStringService>.Default.GetConnectionString(ConnectionString);
            var connection = new OracleConnection(connectionString);
            connection.Open();
            return connection;
        }
    }
}