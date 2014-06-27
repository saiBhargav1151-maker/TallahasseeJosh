using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dqe.Inverter;

namespace Dqe.Web.Filters
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (String.IsNullOrWhiteSpace(Roles)) return base.AuthorizeCore(httpContext);
            var roles = Roles.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            return (roles.Any(i => Container.ResolveCurrentPrincipal().IsInRole(i)));
        }
    }
}