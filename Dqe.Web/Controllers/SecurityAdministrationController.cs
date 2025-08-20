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
        private readonly IProposalRepository _proposalRepository;
        private readonly IProjectRepository _projectRepository;

        public SecurityAdministrationController
            (
            IStaffService staffService,
            IDqeUserRepository dqeUserRepository,
            ICommandRepository commandRepository,
            IProposalRepository proposalRepository,
            IProjectRepository projectRepository
            )
        {
            _staffService = staffService;
            _dqeUserRepository = dqeUserRepository;
            _commandRepository = commandRepository;
            _proposalRepository = proposalRepository;
            _projectRepository = projectRepository;
        }

        /// <summary>
        /// Gets all users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetAllUsers()
        {
            var currentUser = (DqeIdentity) User.Identity;
            var district = currentUser.District.ToUpper().Trim();
            if (district == "CO") district = string.Empty;
            return
                new DqeResult(
                    _dqeUserRepository.GetAll(currentUser.Id, district, true)
                        .Select(i => i.GetTransformer())
                        .Select(i =>
                            new
                            {
                                id = i.SrsId,
                                fullName = i.FullName,
                                district = i.District,
                                role = ((char)i.Role).ToString(),
                                costGroupAuthorization = i.CostGroupAuthorization,
                                roleAsString = i.RoleAsString,
                                selected = false
                            }), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateUser(dynamic user)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var u = _dqeUserRepository.GetBySrsId(user.id, false);
            if (u == null)
            {
                u = new DqeUser(_staffService, _dqeUserRepository, _proposalRepository, _projectRepository);
                var t = (Domain.Transformers.DqeUser)u.GetTransformer();
                t.IsActive = true;
                t.SrsId = user.id;
                t.District = user.district;
                t.Role = ((DqeRole)user.role);
                t.CostGroupAuthorization = user.costGroupAuthorization;
                u.Transform(t, currentDqeUser);
                _commandRepository.Add(u);
            }
            else
            {
                var t = (Domain.Transformers.DqeUser)u.GetTransformer();
                t.IsActive = true;
                t.District = user.district;
                t.Role = ((DqeRole)user.role);
                t.CostGroupAuthorization = user.costGroupAuthorization;
                u.Transform(t, currentDqeUser);
            }
            return new DqeResult(null, new ClientMessage{text = "User Updated"});
        }

        [HttpPost]
        public ActionResult RemoveUsers(dynamic users)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var selectedUsers = ((IEnumerable<dynamic>)users).Where(i => i.selected).ToList();
            foreach (var selectedUser in selectedUsers)
            {
                var u = (DqeUser)_dqeUserRepository.GetBySrsId(selectedUser.id);
                //if (u != null) _commandRepository.Remove(u);
                if (u != null)
                {
                    var t = u.GetTransformer();
                    t.IsActive = false;
                    u.Transform(t, currentDqeUser);
                }
            }
            return new DqeResult(null, new ClientMessage { text = "User(s) Removed" });
        }
    }
}