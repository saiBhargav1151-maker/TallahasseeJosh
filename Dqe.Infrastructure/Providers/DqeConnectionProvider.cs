using System;
using System.Configuration;
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
            var environment = Convert.ToString(ConfigurationManager.AppSettings["environment"]);
            var connectionString = environment == "conversion"
                ? @"Data Source=DCS-DOT-SQL01-B.dot.dcs.sdc.state.fl.us\ENTTESTSQL;Initial Catalog=DQESQL1 ;Integrated Security=True"
                : environment == "web"
                    ? ChannelProvider<IConnectionStringService>.Default.GetConnectionString("DQENET_U")
                    : ChannelProvider<IConnectionStringService>.Default.GetConnectionString("DQEBJS_U");
            var connection = new SqlConnection(connectionString);
#endif
            connection.Open();
            return connection;
        }
    }
}