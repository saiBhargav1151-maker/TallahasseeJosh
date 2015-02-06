using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using NHibernate.Transform;

namespace Dqe.Infrastructure.Repositories
{
    public class DynamicTransformer : IResultTransformer
    {
        public object TransformTuple(object[] tuple, string[] aliases)
        {
            var projection = new ExpandoObject();
            for (var i = 0; i < aliases.Length; i++)
            {
                ((IDictionary<string, object>)projection).Add(aliases[i], tuple[i]);
            }
            return projection;
        }

        public IList TransformList(IList collection)
        {
            return collection;
        }
    }
}