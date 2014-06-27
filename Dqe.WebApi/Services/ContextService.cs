using System.Configuration;
using System.Web;
using Dqe.ApplicationServices;

namespace Dqe.WebApi.Services
{
    public class ContextService : IContextService
    {
        public const string ClientIdKey = "SaaClientId";

        public string ClientId
        {
            get
            {
                var cookie = HttpContext.Current.Request.Cookies[ClientIdKey];
                return cookie == null ? string.Empty : cookie.Value;
            }
        }

        public int UserId
        {
            get
            {
                return HttpContext.Current.Items.Contains("UserId")
                           ? int.Parse(HttpContext.Current.Items["UserId"].ToString())
                           : 0;
            }
            set { HttpContext.Current.Items["UserId"] = value; }
        }

        public string SiteRootAddress
        {
            get
            {
                var siteRootAddress = ConfigurationManager.AppSettings.Get("siteRootAddress");
                return string.IsNullOrWhiteSpace(siteRootAddress)
                    ? string.Empty
                    : siteRootAddress;
            }
        }
    }
}