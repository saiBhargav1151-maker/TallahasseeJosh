using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Dqe.ApplicationServices;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories;
using Dqe.Domain.Repositories.Custom;
using Dqe.Domain.Services;
using Dqe.Web.ActionResults;
using Dqe.Web.Attributes;

namespace Dqe.Web.Controllers
{
    [RemoteRequireHttps]
    [CustomAuthorize(Roles = new [] {DqeRole.Administrator, DqeRole.DistrictAdministrator})]
    public class SecurityAdministrationController : Controller
    {
        private readonly IStaffService _staffService;
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly ICommandRepository _commandRepository;

        public SecurityAdministrationController
            (
            IStaffService staffService,
            IDqeUserRepository dqeUserRepository,
            ICommandRepository commandRepository
            )
        {
            _staffService = staffService;
            _dqeUserRepository = dqeUserRepository;
            _commandRepository = commandRepository;
        }

        [HttpGet]
        public ActionResult GetAllUsers()
        {
            var currentUser = (DqeIdentity) User.Identity;
            var district = currentUser.District.ToUpper().Trim();
            if (district == "CO") district = string.Empty;
            return
                new DqeResult(
                    _dqeUserRepository.GetAll(currentUser.Id, district)
                        .Select(i => i.GetTransformer())
                        .Select(i =>
                            new
                            {
                                id = i.SrsId,
                                fullName = i.FullName,
                                district = i.District,
                                role = i.Role,
                                roleAsString = i.RoleAsString,
                                selected = false
                            }), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateUser(dynamic user)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var u = _dqeUserRepository.GetBySrsId(user.id);
            if (u == null)
            {
                u = new DqeUser(_staffService, _dqeUserRepository);
                var t = u.GetTransformer();
                t.IsActive = true;
                t.SrsId = user.id;
                t.District = user.district;
                t.Role = (DqeRole)user.role;
                u.Transform(t, currentDqeUser);
                _commandRepository.Add(u);
            }
            else
            {
                var t = u.GetTransformer();
                t.IsActive = true;
                t.District = user.district;
                t.Role = (DqeRole)user.role;
                u.Transform(t, currentDqeUser);
            }
            return new DqeResult(null, new ClientMessage{text = "User Updated"});
        }

        [HttpPost]
        public ActionResult RemoveUsers(dynamic users)
        {
            var selectedUsers = ((IEnumerable<dynamic>)users).Where(i => i.selected).ToList();
            foreach (var selectedUser in selectedUsers)
            {
                var u = _dqeUserRepository.GetBySrsId(selectedUser.id);
                if (u != null) _commandRepository.Remove(u);
            }
            return new DqeResult(null, new ClientMessage { text = "User(s) Removed" });
        }
    }
}