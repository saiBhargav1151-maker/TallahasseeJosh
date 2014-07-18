using System.Linq;
using System.Web.Mvc;
using Dqe.Domain.Services;
using Dqe.Web.Attributes;

namespace Dqe.Web.Controllers
{
    [RemoteRequireHttps]
    public class StaffController : Controller
    {
        private readonly IStaffService _staffService;

        public StaffController
            (
            IStaffService staffService
            )
        {
            _staffService = staffService;
        }

        [HttpGet]
        public JsonResult GetStaffByName(string id)
        {
            return Json(_staffService.GetStaffByName(id).Select(i => new {id = i.Id, fullName = i.FullName, district = i.District}), JsonRequestBehavior.AllowGet);
                
        }
    }
}