using System;
using System.Security.Cryptography;
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
                if (string.IsNullOrWhiteSpace(account)) return HttpContext.Current.User.Identity;
                FormsAuthenticationTicket ticket;
                try
                {
                    ticket = FormsAuthentication.Decrypt(account);
                }
                catch (CryptographicException)
                {
                    return HttpContext.Current.User.Identity;
                }
                if (ticket == null) throw new InvalidOperationException("Authentication ticket cannot be null");
                if (ticket.Expired)
                {
                    return HttpContext.Current.User.Identity;
                }
                var id = JsonConvert.DeserializeObject<DqeIdentity>(ticket.UserData);
                if (HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath != null &&
                    HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.ToLower() != "~/security/gettimeout")
                {
                    ticket = new FormsAuthenticationTicket(
                        1,
                        id.Name,
                        DateTime.Now,
                        DateTime.Now.AddMinutes(60),
                        false,
                        JsonConvert.SerializeObject(id)
                        );
                    var encryptedTicket = FormsAuthentication.Encrypt(ticket);            
                    HttpContext.Current.Response.Cookies.Add(new HttpCookie("DQE_AUTH_TICKET", encryptedTicket));
                }
                return id;
            }
        }
    }
}