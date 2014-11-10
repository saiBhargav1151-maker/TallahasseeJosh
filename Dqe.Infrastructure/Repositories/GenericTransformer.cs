using System;
using System.Collections;
using System.Dynamic;
using System.Reflection;
using NHibernate.Transform;

namespace Dqe.Infrastructure.Repositories
{
    /// <summary>
    /// COMPONENT
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericTransformer<T> : IResultTransformer
    {   
        public object TransformTuple(object[] tuple, string[] aliases)
        {
            var projection = Activator.CreateInstance<T>();
            PropertyInfo[] properties;
            var projectionType = typeof(T);
            if (!ProjectionTypeCache.Cache.TryGetValue(projectionType, out properties))
            {
                properties = projection.GetType().GetProperties();
                ProjectionTypeCache.Cache.TryAdd(projectionType, properties);
            }
            foreach (var property in properties)
            {
                property.SetValue(projection, tuple[Array.IndexOf(aliases, property.Name)], null);
            }
            return projection;
        }

        public IList TransformList(IList collection)
        {
            return collection;
        }
    }
}