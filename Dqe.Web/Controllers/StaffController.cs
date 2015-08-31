using System.Linq;
using System.Web.Mvc;
using Dqe.Domain.Repositories.Custom;
using Dqe.Domain.Services;
using Dqe.Web.Attributes;

namespace Dqe.Web.Controllers
{
    [RemoteRequireHttps]
    public class StaffController : Controller
    {
        private readonly IStaffService _staffService;
        private readonly IDqeUserRepository _dqeUserRepository;

        public StaffController
            (
            IStaffService staffService,
            IDqeUserRepository dqeUserRepository
            )
        {
            _staffService = staffService;
            _dqeUserRepository = dqeUserRepository;
        }


        [HttpGet]
        public JsonResult GetStaffByName(string id)
        {
            var staffByName = _staffService.GetStaffByName(id).ToList();
            var staffByRacf = _staffService.GetStaffByRacf(id);

            if (staffByRacf != null)
            {
                if (staffByName.All(i => i.Id != staffByRacf.Id))
                    staffByName.Add(staffByRacf);
            }

            return Json(staffByName
                .Select(i => new
                {
                    id = i.Id,
                    fullName = i.FullName,
                    district = i.District,
                    email = i.Email,
                    phoneNumber = i.PhoneAndExtension
                }),
                JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult GetDqeStaffByName(string id)
        {
            var dqeUserIds = _dqeUserRepository.GetAll().Select(i => i.SrsId).Distinct();
            var staffUsers = _staffService.GetStaffByName(id).ToList();
            return Json(staffUsers.Where(i => dqeUserIds.Contains(i.Id))
                .Select(i => new
                {
                    id = i.Id,
                    fullName = i.FullName,
                    district = i.District,
                    email = i.Email,
                    phoneNumber = i.PhoneAndExtension
                }).Distinct(),
                JsonRequestBehavior.AllowGet);

        }
    }
}