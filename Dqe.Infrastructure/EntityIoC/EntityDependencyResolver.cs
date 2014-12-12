using System;
using NHibernate.DependencyInjection;

namespace Dqe.Infrastructure.EntityIoC
{
    /// <summary>
    /// COMPONENT
    /// </summary>
    public static class EntityDependencyResolver
    {
        public static event ResolveConstructorArguments OnResolveConstructorArguments;

        public static object[] Resolve(IEntityInjector injector, Type type)
        {
            var handler = OnResolveConstructorArguments;
            if (handler != null)
            {
                return handler.Invoke(injector, new ResolveConstructorArgumentsArgs { EntityType = type });
            }
            return null;
        }
    }
}