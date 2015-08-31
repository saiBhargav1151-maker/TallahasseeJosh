using Dqe.ApplicationServices;
using FDOT.Enterprise;
using FDOT.Enterprise.ConnectionStrings.Client;

namespace Dqe.Infrastructure.Providers
{
    public class SsrsConnectionProvider : ISsrsConnectionProvider
    {
        public string[] GetConnection()
        {
            var connectionString = ChannelProvider<IConnectionStringService>.Default.GetConnectionString("DQESRVREP_R");

            var splitConnection = connectionString.Split(';');
            var userName = splitConnection[0].Substring(splitConnection[0].IndexOf('=') + 1, splitConnection[0].Length - (splitConnection[0].IndexOf('=') + 1));
            var passWord = splitConnection[1].Substring(splitConnection[1].IndexOf('=') + 1, splitConnection[1].Length - (splitConnection[1].IndexOf('=') + 1));

            return new[] { userName, passWord };
        }
    }
}
