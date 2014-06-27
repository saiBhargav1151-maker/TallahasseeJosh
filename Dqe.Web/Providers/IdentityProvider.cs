using System.Security.Principal;
using System.Web;
using Dqe.ApplicationServices;

namespace Dqe.Web.Providers
{
    /// <summary>
    /// COMPONENT TEMPLATE
    /// </summary>
    public class IdentityProvider : IIdentityProvider
    {
        /// <summary>
        /// REQUIRES IMPLEMENTATION
        /// </summary>
        public IIdentity Current
        {
            get
            {
                return HttpContext.Current.User.Identity;
            }
        }
    }
}