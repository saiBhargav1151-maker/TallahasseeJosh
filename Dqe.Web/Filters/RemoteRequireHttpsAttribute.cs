using System;
using System.Configuration;
using System.Web.Mvc;

namespace Dqe.Web.Filters
{
    public class RemoteRequireHttpsAttribute : RequireHttpsAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }
            if(filterContext.HttpContext != null && filterContext.HttpContext.Request.IsLocal)
            {
                return;
            }
            var setting = ConfigurationManager.AppSettings.Get("disableHttps");
            if (!string.IsNullOrWhiteSpace(setting))
            {
                bool disableHttps;
                if (bool.TryParse(setting, out disableHttps) && disableHttps) return;
            }
            base.OnAuthorization(filterContext);
        }
    }
}