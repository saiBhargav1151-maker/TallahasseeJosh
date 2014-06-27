using System.Web.Mvc;

namespace Dqe.Web.Controllers
{
    public class SiteController : Controller
    {
        [ChildActionOnly]
        public ActionResult Navigation()
        {
            return PartialView();
        }
    }
}
