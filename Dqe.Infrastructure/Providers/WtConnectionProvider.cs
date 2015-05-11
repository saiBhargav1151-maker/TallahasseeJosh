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
            var connectionString =environment == "conversion"
                ? @"Data Source=DCS-DOT-SQL01-B.dot.dcs.sdc.state.fl.us\ENTTESTSQL;Initial Catalog=WTPSQL1 ;Integrated Security=True"
                : environment == "web"
                    ? ChannelProvider<IConnectionStringService>.Default.GetConnectionString("DQEWTNET_U")
                    : ChannelProvider<IConnectionStringService>.Default.GetConnectionString("DQEWTBJS_U");
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }
    }
}
