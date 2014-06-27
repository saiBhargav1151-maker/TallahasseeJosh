using System.Security.Principal;
using System.Web;
using Dqe.ApplicationServices;

namespace Dqe.WebApi.Providers
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
//#if DEBUG
//                return new TestIdentity();
//#else
                return HttpContext.Current.User.Identity;
//#endif
            }
        }
    }
}