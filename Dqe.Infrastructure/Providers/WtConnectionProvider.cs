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

            string connectionLabel = environment == "web"
                            ? "DQEWTNET_U"
                            : "DQEWTBJS_U";

            //get connectinString from cache if exist else call FEL GetConnectionString
            var connectionString = environment == "conversion"
                ? Convert.ToString(ConfigurationManager.AppSettings["wtConnection"])
                : Initializer.ConnectionStringCache.ContainsKey(connectionLabel)
                    ? Initializer.ConnectionStringCache[connectionLabel]
                    : ChannelProvider<IConnectionStringService>.Default.GetConnectionString(connectionLabel);

            //add connection connectionLabel and conectionString to cache if not exist
            if (!string.IsNullOrEmpty(connectionString) && !Initializer.ConnectionStringCache.ContainsKey(connectionLabel))
                Initializer.ConnectionStringCache.Add(connectionLabel, connectionString);
            
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }
    }
}
