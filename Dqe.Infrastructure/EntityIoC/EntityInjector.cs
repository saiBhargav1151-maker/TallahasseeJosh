using System;
using NHibernate.DependencyInjection;

namespace Dqe.Infrastructure.EntityIoC
{
    /// <summary>
    /// COMPONENT
    /// </summary>
    public class EntityInjector : IEntityInjector
    {
        public object[] GetConstructorParameters(Type type)
        {
            return EntityDependencyResolver.Resolve(this, type);
        }
    }
}