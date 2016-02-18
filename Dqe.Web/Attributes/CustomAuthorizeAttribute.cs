using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dqe.Domain.Model;
using Dqe.Inverter;

namespace Dqe.Web.Attributes
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return !Roles.Any() 
                ? base.AuthorizeCore(httpContext)
                : (Roles.Any(i => Container.ResolveCurrentPrincipal().IsInRole(i.ToString())));
        }

        public new DqeRole[] Roles { get; set; }
    }
}