using System.Data;
using System.Data.SqlClient;
using FDOT.Enterprise;
using FDOT.Enterprise.ConnectionStrings.Client;
using NHibernate.Connection;

namespace Dqe.Infrastructure.Providers
{
    public class DqeConnectionProvider : ConnectionProvider
    {
        public override IDbConnection GetConnection()
        {
#if DEBUG
            var connection = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=DQE;Integrated Security=True");
#else
            var connectionString = ChannelProvider<IConnectionStringService>.Default.GetConnectionString(ConnectionString);
            var connection = new SqlConnection(connectionString);
#endif
            connection.Open();
            return connection;
        }
    }
}