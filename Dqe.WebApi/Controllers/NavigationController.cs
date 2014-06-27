using System.Collections.Generic;
using System.Dynamic;
using System.Web.Http;
using System.Web.Mvc;

namespace Dqe.WebApi.Controllers
{
    public class NavigationController : ApiController
    {
        public object Get(string id)
        {
            var items = new List<ExpandoObject>();
            dynamic item = new ExpandoObject();
            item.name = "Home";
            item.location = "home";
            items.Add(item);
            if (id == "estimator" || id == "admin")
            {
                item = new ExpandoObject();
                item.name = "Profile";
                item.location = "profile";
                items.Add(item);
                if (id == "admin")
                {
                    item = new ExpandoObject();
                    item.name = "Administration";
                    item.location = "admin";
                    items.Add(item);        
                }   
            }
            item = new ExpandoObject();
            item.name = (id == "noauth") ? "Sign in" : "Sign out";
            item.location = "signin";
            items.Add(item);
            var jsonResult = new JsonResult
            {
                Data = items,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
            return jsonResult.Data;
        }
    }
}