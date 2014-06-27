using System.Web.Mvc;
using Dqe.Web.Models;

namespace Dqe.Web.Controllers
{
    public class HomeController : Controller
    {   
        [HttpGet]
        public ActionResult Index()
        {
            return View(new HomeViewModel());
        }
    }
}
