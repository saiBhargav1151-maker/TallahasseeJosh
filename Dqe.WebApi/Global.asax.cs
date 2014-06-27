using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Dqe.Inverter;
using Dqe.WebApi.Providers;
using Dqe.WebApi.Services;
using DependencyResolver = Dqe.WebApi.Services.DependencyResolver;

namespace Dqe.WebApi
{
    public class WebApiApplication : HttpApplication
    {
        private static readonly object Lock = new object();
        private static bool _isInitialized;

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            ModelBinders.Binders.Add(typeof(object), new DynamicModelBinder());
        }

        // ReSharper disable InconsistentNaming
        protected void Application_BeginRequest()
        // ReSharper restore InconsistentNaming
        {
            if (!Request.Cookies.AllKeys.ToList().Contains(ContextService.ClientIdKey))
            {
                Response.Cookies.Add(new HttpCookie(ContextService.ClientIdKey, Guid.NewGuid().ToString()));
            }
            if (_isInitialized) return;
            lock (Lock)
            {
                if (_isInitialized) return;
                //Note: the entry-point application should provide the mechanism for aquiring an IIdentity for the current user
                Container.Initialize(new IdentityProvider(), new ContextService());
                GlobalConfiguration.Configuration.DependencyResolver = new DependencyResolver();
                ControllerBuilder.Current.SetControllerFactory(new ControllerFactory(Assembly.GetExecutingAssembly()));
                //purge temp files
                _isInitialized = true;
            }
        }

        // ReSharper disable InconsistentNaming
        protected void Application_PostAuthenticateRequest()
        // ReSharper restore InconsistentNaming
        {
            var p = Container.ResolveCurrentPrincipal();
            HttpContext.Current.User = p;
            Thread.CurrentPrincipal = p;
        }

        // ReSharper disable InconsistentNaming
        protected void Application_EndRequest()
        // ReSharper restore InconsistentNaming
        {
            Container.ResolveTransactionManager().Commit();
        }

        // ReSharper disable InconsistentNaming
        protected void Application_Error()
        // ReSharper restore InconsistentNaming
        {
            Container.ResolveTransactionManager().Abort();
        }

        // ReSharper disable InconsistentNaming
        protected void Session_Start()
        // ReSharper restore InconsistentNaming
        {
            //keep a consistent session ID
            Session["dummy"] = 1;
        }
    }
}
