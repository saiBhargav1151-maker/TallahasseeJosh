using System;
using System.Configuration;
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
            var environment = Convert.ToString(ConfigurationManager.AppSettings["environment"]);
            var connectionString = environment == "web"
                ? ChannelProvider<IConnectionStringService>.Default.GetConnectionString("DQELRENET_U")
                : ChannelProvider<IConnectionStringService>.Default.GetConnectionString("DQELREBJS_U");
            var connection = new OracleConnection(connectionString);
            connection.Open();
            return connection;
        }
    }
}