using System.Web.Mvc;
using Dqe.Web.Attributes;

namespace Dqe.Web.Controllers
{
    [RemoteRequireHttps]
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
#if DEBUG
            return File(Server.MapPath("~/Views/indexDebug.htm"), "text/html");
#else
            return File(Server.MapPath("~/Views/index.htm"), "text/html");
#endif
        }
    }
}
