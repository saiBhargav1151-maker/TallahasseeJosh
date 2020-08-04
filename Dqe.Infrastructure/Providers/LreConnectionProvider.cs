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

            string connectionLabel = environment == "web"
                            ? "DQELRENET_U"
                            : "DQELREBJS_U";

            //get connectinString from cache if exist else call FEL GetConnectionString
            var connectionString = Initializer.ConnectionStringCache.ContainsKey(connectionLabel)
                    ? Initializer.ConnectionStringCache[connectionLabel]
                    : ChannelProvider<IConnectionStringService>.Default.GetConnectionString(connectionLabel);

            //add connection connectionLabel and conectionString to cache if not exist
            if (!string.IsNullOrEmpty(connectionString) && !Initializer.ConnectionStringCache.ContainsKey(connectionLabel))
                Initializer.ConnectionStringCache.Add(connectionLabel, connectionString);

            var connection = new OracleConnection(connectionString);
            connection.Open();
            return connection;
        }
    }
}