using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories;
using Dqe.Domain.Repositories.Custom;
using Dqe.Domain.Services;

namespace Dqe.WebApi.Controllers
{
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
            return
                Json(
                    _dqeUserRepository.GetAll()
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
        public void UpdateUser(dynamic user)
        {
            var u = _dqeUserRepository.GetBySrsId(user.id);
            if (u == null)
            {
                u = new DqeUser(_staffService);
                var t = u.GetTransformer();
                t.IsActive = true;
                t.SrsId = user.id;
                t.District = user.district;
                t.Role = (DqeRole)user.role;
                u.Transform(t, null);
                _commandRepository.Add(u);
            }
            else
            {
                var t = u.GetTransformer();
                t.IsActive = true;
                t.District = user.district;
                t.Role = (DqeRole)user.role;
                u.Transform(t, null);
            }
        }

        [HttpPost]
        public void RemoveUsers(dynamic users)
        {
            var selectedUsers = ((IEnumerable<dynamic>)users).Where(i => i.selected).ToList();
            foreach (var selectedUser in selectedUsers)
            {
                var u = _dqeUserRepository.GetBySrsId(selectedUser.id);
                if (u != null) _commandRepository.Remove(u);
            }
        }
    }
}