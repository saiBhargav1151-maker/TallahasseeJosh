using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dqe.Web.ModelBinders
{
    public class DynamicModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (!controllerContext.HttpContext.Request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
            {
                return base.BindModel(controllerContext, bindingContext);
            }
            controllerContext.HttpContext.Request.InputStream.Position = 0;
            using (var reader = new StreamReader(controllerContext.HttpContext.Request.InputStream))
            {
                var json = reader.ReadToEnd();
                if (string.IsNullOrEmpty(json))
                {
                    return null;
                }
                var deserializedObject = JsonConvert.DeserializeObject(json);
                var array = deserializedObject as JArray;
                dynamic result = new ExpandoObject();
                if (deserializedObject == null) return result;
                if (array != null)
                {
                    var results = new List<ExpandoObject>();
                    foreach (var item in array)
                    {
                        deserializedObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(item.ToString());
                        if (deserializedObject != null)
                        {
                            foreach (var prop in (Dictionary<string, object>)deserializedObject)
                            {
                                int num;
                                ((IDictionary<string, object>)result).Add(prop.Key, int.TryParse(prop.Value.ToString(), out num) ? num : prop.Value);
                            }
                            results.Add(result);
                            result = new ExpandoObject();
                        }
                    }
                    return results;
                }
                var o = (JObject) deserializedObject;
                foreach (var prop in o.Properties())
                {
                    int num;
                    ((IDictionary<string, object>)result).Add(prop.Name, int.TryParse(prop.Value.ToObject<object>().ToString(), out num) ? num : prop.Value.ToObject<object>());
                }
                return result;
            }
        }
    }
}