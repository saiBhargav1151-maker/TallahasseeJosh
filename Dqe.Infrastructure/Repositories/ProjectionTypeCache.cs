using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Dqe.Infrastructure.Repositories
{
    /// <summary>
    /// COMPONENT
    /// </summary>
    public static class ProjectionTypeCache
    {
        public static readonly ConcurrentDictionary<Type, PropertyInfo[]> Cache = new ConcurrentDictionary<Type, PropertyInfo[]>();
    }
}