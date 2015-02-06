using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Dqe.Inverter;

namespace Dqe.Web.Factories
{
    public class ControllerFactory : DefaultControllerFactory
    {
        public ControllerFactory(Assembly assemblyToRegister)
        {
            var controllerTypes = from t in assemblyToRegister.GetTypes()
                where typeof(IController).IsAssignableFrom(t)
                select t;
            foreach (var t in controllerTypes) Container.BindController(t);
        }

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            return controllerType == null
                ? null
                : (IController)Container.ResolveController(controllerType);
        }
    }
}