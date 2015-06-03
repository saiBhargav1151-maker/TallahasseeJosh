using System;
using System.Configuration;
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
            var environment = Convert.ToString(ConfigurationManager.AppSettings["environment"]);
            var connectionString = environment == "conversion"
                ? Convert.ToString(ConfigurationManager.AppSettings["wtConnection"])
                : environment == "web"
                    ? ChannelProvider<IConnectionStringService>.Default.GetConnectionString("DQEWTNET_U")
                    : ChannelProvider<IConnectionStringService>.Default.GetConnectionString("DQEWTBJS_U");
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }
    }
}
