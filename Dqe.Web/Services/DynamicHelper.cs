using System.Collections.Generic;

namespace Dqe.Web.Services
{
    public class DynamicHelper
    {
        public static bool HasNotNullProperty(dynamic expandoObject, string propertyName)
        {
            if (!((IDictionary<string, object>) expandoObject).ContainsKey(propertyName))
            {
                return false;
            }
            if (((IDictionary<string, object>) expandoObject)[propertyName] == null)
            {
                return false;
            }
            return true;
        }
    }
}