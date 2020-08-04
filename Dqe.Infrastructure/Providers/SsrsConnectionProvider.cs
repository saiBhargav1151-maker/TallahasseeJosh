using Dqe.ApplicationServices;
using FDOT.Enterprise;
using FDOT.Enterprise.ConnectionStrings.Client;

namespace Dqe.Infrastructure.Providers
{
    public class SsrsConnectionProvider : ISsrsConnectionProvider
    {
        public string[] GetConnection()
        {
            //get connectinString from cache if exist else call FEL GetConnectionString
            string connectionLabel = "DQESRVREP_R";
            var connectionString = Initializer.ConnectionStringCache.ContainsKey(connectionLabel)
                    ? Initializer.ConnectionStringCache[connectionLabel]
                    : ChannelProvider<IConnectionStringService>.Default.GetConnectionString(connectionLabel);

            //add connection connectionLabel and conectionString to cache if not exist
            if (!string.IsNullOrEmpty(connectionString) && !Initializer.ConnectionStringCache.ContainsKey(connectionLabel))
                Initializer.ConnectionStringCache.Add(connectionLabel, connectionString);

            var splitConnection = connectionString.Split(';');
            var userName = splitConnection[0].Substring(splitConnection[0].IndexOf('=') + 1, splitConnection[0].Length - (splitConnection[0].IndexOf('=') + 1));
            var passWord = splitConnection[1].Substring(splitConnection[1].IndexOf('=') + 1, splitConnection[1].Length - (splitConnection[1].IndexOf('=') + 1));

            return new[] { userName, passWord };
        }
    }
}
