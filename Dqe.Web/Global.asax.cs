using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Dqe.Inverter;
using Dqe.Web.Factories;
using Dqe.Web.Hubs;
using Dqe.Web.ModelBinders;
using Dqe.Web.Providers;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using DependencyResolver = Dqe.Web.Services.DependencyResolver;

namespace Dqe.Web
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
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(object), new DynamicModelBinder());
        }

        // ReSharper disable InconsistentNaming
        protected void Application_BeginRequest()
        // ReSharper restore InconsistentNaming
        {
            if (_isInitialized) return;
            lock (Lock)
            {
                if (_isInitialized) return;
                //Note: the entry-point application should provide the mechanism for aquiring an IIdentity for the current user
                Container.Initialize(new IdentityProvider(), new DeferredTaskHubContext());
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
