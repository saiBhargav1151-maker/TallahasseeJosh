using System;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DocumentFormat.OpenXml.EMMA;
using Dqe.ApplicationServices;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories;
using Dqe.Domain.Repositories.Custom;
using Dqe.Domain.Services;
using Dqe.Domain.Transformers;
using Dqe.Web.ActionResults;
using Dqe.Web.Attributes;
using Newtonsoft.Json;
using DqeUser = Dqe.Domain.Model.DqeUser;

namespace Dqe.Web.Controllers
{
    [RemoteRequireHttps]
    public class SecurityController : Controller
    {
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly ICommandRepository _commandRepository;
        private readonly IStaffService _staffService;
        private readonly IProposalRepository _proposalRepository;
        private readonly IProjectRepository _projectRepository;

        public SecurityController
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
        public void SignOut()
        {
            var cookie = Response.Cookies["DQE_AUTH_TICKET"];
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddDays(-1);    
            }
        }

        [HttpPost]
        public ActionResult GetTimeout()
        {
            var cookie = Request.Cookies["DQE_AUTH_TICKET"];
            if (cookie != null && !string.IsNullOrWhiteSpace(cookie.Value))
            {
                var ticket = FormsAuthentication.Decrypt(cookie.Value);
                if (ticket != null)
                {
                    if (!ticket.Expired)
                    {
                        var span = ticket.Expiration - DateTime.Now;
                        return new DqeResult(new
                        {
                            hours = span.Hours,
                            minutes = span.Minutes,
                            redirect = false
                        });
                    }
                    cookie = Response.Cookies["DQE_AUTH_TICKET"];
                    if (cookie != null)
                    {
                        cookie.Expires = DateTime.Now.AddDays(-1);
                    }
                    return new DqeResult(new
                    {
                        hours = 0,
                        minutes = 0,
                        redirect = true
                    });
                }
            }
            return new DqeResult(null);
        }

        [HttpPost]
        public ActionResult CanImpersonate(dynamic user)
        {
            return new DqeResult(new
            {
                canImpersonate = true
            });
        }

        [HttpPost]
        public void ImpersonateUser(dynamic user)
        {
            var u = _dqeUserRepository.GetBySrsId(user.id);
            var sys = _dqeUserRepository.GetSystemAccount();
            if (u == null)
            {
                u = new DqeUser(_staffService, _dqeUserRepository, _proposalRepository, _projectRepository);
                var t = (Domain.Transformers.DqeUser)u.GetTransformer();
                t.IsActive = true;
                t.SrsId = user.id;
                t.District = user.district;
                t.CostGroupAuthorization = "U";
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

        [HttpPost]
        public ActionResult AuthenticateUser(dynamic user)
        {
            var token = (AuthenticationToken)_staffService.AuthenticateUser(user.id, user.password);
#if !DEBUG
            if (!token.IsAuthenticated)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = token.Message });
            }
#endif
            var staff = (Staff)_staffService.GetStaffByRacf(user.id);
            if (staff == null)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Staff record not found" });
            }
            var u = _dqeUserRepository.GetBySrsId(staff.Id);
            if (u == null || !u.IsActive)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "You are not authorized for access to DQE" });      
            }
            var encryptedTicket = CreateAuthenticationTicket(u);
            Response.Cookies.Add(new HttpCookie("DQE_AUTH_TICKET", encryptedTicket));
            return new DqeResult(null);
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