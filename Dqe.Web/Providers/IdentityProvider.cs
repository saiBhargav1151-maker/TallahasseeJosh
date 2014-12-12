using System;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using Dqe.ApplicationServices;
using Newtonsoft.Json;

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
                var httpCookie = HttpContext.Current.Request.Cookies.Get("DQE_AUTH_TICKET");
                if (httpCookie == null) return HttpContext.Current.User.Identity;
                var account = httpCookie.Value;
                if (account == null) return HttpContext.Current.User.Identity;
                var ticket = FormsAuthentication.Decrypt(account);
                if (ticket == null) throw new InvalidOperationException("Authentication ticket cannot be null");
                return JsonConvert.DeserializeObject<DqeIdentity>(ticket.UserData);
            }
        }
    }
}