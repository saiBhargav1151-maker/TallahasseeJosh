using System.Web.Mvc;

namespace Dqe.WebApi.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return File(Server.MapPath("~/Views/index.html"), "text/html");
        }
    }
}
