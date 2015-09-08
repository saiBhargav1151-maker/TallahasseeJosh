using System.Security.Principal;

namespace Dqe.ApplicationServices
{
    /// <summary>
    /// COMPONENT 
    /// </summary>
    public interface IIdentityProvider
    {
        IIdentity Current { get; }
    }
}