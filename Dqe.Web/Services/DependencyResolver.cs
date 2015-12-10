using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Dqe.Inverter;
using IDependencyResolver = System.Web.Http.Dependencies.IDependencyResolver;

namespace Dqe.Web.Services
{
    /// <summary>
    /// COMPONENT
    /// </summary>
    public class DependencyResolver : IDependencyResolver
    {
        public void Dispose()
        {
            //Container.Dispose();
        }

        public object GetService(Type serviceType)
        {
            return Container.ResolveService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return Container.ResolveServiceCollection(serviceType);
        }

        public IDependencyScope BeginScope()
        {
            return new DependencyResolver();
        }
    }
}