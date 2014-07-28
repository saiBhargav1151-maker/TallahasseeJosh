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
            return File(Server.MapPath("~/Views/index.html"), "text/html");
        }
    }
}
