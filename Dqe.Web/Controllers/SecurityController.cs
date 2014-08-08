using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Dqe.ApplicationServices;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories;
using Dqe.Domain.Repositories.Custom;
using Dqe.Domain.Services;
using Dqe.Web.Attributes;
using Newtonsoft.Json;

namespace Dqe.Web.Controllers
{
    [RemoteRequireHttps]
    public class SecurityController : Controller
    {
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly ICommandRepository _commandRepository;
        private readonly IStaffService _staffService;

        public SecurityController
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
        public ActionResult GetCurrentUser()
        {
            var dqeIdenetity = User.Identity as DqeIdentity;
            if (dqeIdenetity == null)
            {
                return
                Json(
                    new
                    {
                        id = 0,
                        isAuthenticated = User.Identity.IsAuthenticated,
                        role = string.Empty,
                        name = string.Empty,
                        district = string.Empty,
                        userNameAndRole = string.Empty
                    }, JsonRequestBehavior.AllowGet);    
            }
            return
                Json(
                    new
                    {
                        id = dqeIdenetity.Id,
                        isAuthenticated = dqeIdenetity.IsAuthenticated,
                        role = _dqeUserRepository.Get(dqeIdenetity.Id).Role,
                        name = dqeIdenetity.Name,
                        district = dqeIdenetity.District,
                        userNameAndRole = string.Format("{0} - {1} {2}", dqeIdenetity.Name, dqeIdenetity.District, _dqeUserRepository.Get(dqeIdenetity.Id).Role)
                    }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void ImpersonateUser(dynamic user)
        {
            var u = _dqeUserRepository.GetBySrsId(user.id);
            var sys = _dqeUserRepository.GetSystemAccount();
            if (u == null)
            {
                u = new DqeUser(_staffService, _dqeUserRepository);
                var t = u.GetTransformer();
                t.IsActive = true;
                t.SrsId = user.id;
                t.District = user.district;
                t.Role = (DqeRole)user.role;
                u.Transform(t, sys);
                _commandRepository.Add(u);
            }
            else
            {
                var t = u.GetTransformer();
                t.IsActive = true;
                t.District = user.district;
                t.Role = (DqeRole)user.role;
                u.Transform(t, sys);
            }
            var encryptedTicket = CreateAuthenticationTicket(u);
            Response.Cookies.Add(new HttpCookie("DQE_AUTH_TICKET", encryptedTicket));
        }

        private string CreateAuthenticationTicket(DqeUser user)
        {
            var identity = new DqeIdentity(user.Id, user.SrsId, user.Name, user.District);
            var authenticationTicket = new FormsAuthenticationTicket(
                1,
                user.Name,
                DateTime.Now,
                DateTime.Now.AddMinutes(30),
                false,
                JsonConvert.SerializeObject(identity)
                );
            return FormsAuthentication.Encrypt(authenticationTicket);
        }
    }
}