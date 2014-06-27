using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Dqe.Inverter;
using Dqe.Web.App_Start;
using Dqe.Web.Factories;
using Dqe.Web.Messaging;
using Dqe.Web.Providers;
using Dqe.Web.Services;

namespace Dqe.Web
{
    public class MvcApplication : HttpApplication
    {
        private static readonly object Lock = new object();
        private static bool _isInitialized;

        protected void Application_Start()
        {
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());
            //TODO: Replace with the new owin startup implementation 
            //RouteTable.Routes.MapConnection<Dispatch>("dispatch", "/dispatch");
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
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