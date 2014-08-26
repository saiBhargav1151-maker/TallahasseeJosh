using System.Data;
using System.Data.SqlClient;
using FDOT.Enterprise;
using FDOT.Enterprise.ConnectionStrings.Client;
using NHibernate.Connection;

namespace Dqe.Infrastructure.Providers
{
    public class WtConnectionProvider : ConnectionProvider
    {
        public override IDbConnection GetConnection()
        {
            var connectionString = ChannelProvider<IConnectionStringService>.Default.GetConnectionString(ConnectionString);
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }
    }    
}
