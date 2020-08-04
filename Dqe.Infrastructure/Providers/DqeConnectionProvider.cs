using System;
using System.Configuration;
using System.Collections.Generic;
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
            var environment = Convert.ToString(ConfigurationManager.AppSettings["environment"]);            

            string connectionLabel = environment == "web"
                            ? "DQENET_U"
                            : "DQEBJS_U";

            //get connectinString from cache if exist else call FEL GetConnectionString
            var connectionString = environment == "conversion"
                ? Convert.ToString(ConfigurationManager.AppSettings["dqeConnection"])
                : Initializer.ConnectionStringCache.ContainsKey(connectionLabel)
                    ? Initializer.ConnectionStringCache[connectionLabel]
                    : ChannelProvider<IConnectionStringService>.Default.GetConnectionString(connectionLabel);

            //add connection connectionLabel and conectionString to cache if not exist
            if (!string.IsNullOrEmpty(connectionString) && !Initializer.ConnectionStringCache.ContainsKey(connectionLabel))
                Initializer.ConnectionStringCache.Add(connectionLabel, connectionString);

//#if DEBUG
//            var connection = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=DQE;Integrated Security=True");
//#else
            var connection = new SqlConnection(connectionString);
//#endif
            connection.Open();
            return connection;
        }
    }
}