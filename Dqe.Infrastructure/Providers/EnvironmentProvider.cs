using Dqe.ApplicationServices;
using FDOT.Enterprise;

namespace Dqe.Infrastructure.Providers
{
    public class EnvironmentProvider : IEnvironmentProvider
    {
        public string GetEnvironment()
        {
            return LocalSettings.EnvironmentLevel.ToString();
        }
    }
}
