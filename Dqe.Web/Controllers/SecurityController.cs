using System;
using System.Configuration;
using System.Security.Cryptography;
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
using Dqe.Inverter;
using Dqe.Web.ActionResults;
using Dqe.Web.Attributes;
using Microsoft.Ajax.Utilities;
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
        private readonly IEnvironmentProvider _environmentProvider;

        public SecurityController
            (
            IStaffService staffService,
            IDqeUserRepository dqeUserRepository,
            ICommandRepository commandRepository,
            IProposalRepository proposalRepository,
            IProjectRepository projectRepository,
            IEnvironmentProvider environmentProvider
            )
        {
            _staffService = staffService;
            _dqeUserRepository = dqeUserRepository;
            _commandRepository = commandRepository;
            _proposalRepository = proposalRepository;
            _projectRepository = projectRepository;
            _environmentProvider = environmentProvider;
        }

        public ActionResult GetEnvironment()
        {
#if DEBUG
            return new DqeResult(new
            {
                showEnvironmentWarning = true,
                environment = "LOCAL"
            }, JsonRequestBehavior.AllowGet);

            //return new DqeResult(new
            //{
            //    showEnvironmentWarning = false,
            //    environment = string.Empty
            //}, JsonRequestBehavior.AllowGet);
#else
            var ev = _environmentProvider.GetEnvironment().ToUpper();
            if (ev.StartsWith("U"))
            {
                return new DqeResult(new
                {
                    showEnvironmentWarning = true,
                    environment = "UNIT TEST"
                }, JsonRequestBehavior.AllowGet);
            }
            if (ev.StartsWith("S"))
            {
                return new DqeResult(new
                {
                    showEnvironmentWarning = true,
                    environment = "SYSTEM TEST"
                }, JsonRequestBehavior.AllowGet);
            }
            return new DqeResult(new
            {
                showEnvironmentWarning = false,
                environment = string.Empty
            }, JsonRequestBehavior.AllowGet);
#endif
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
            var user = _dqeUserRepository.Get(dqeIdenetity.Id);
            var role = user.Role == DqeRole.Administrator
                ? "A"
                : user.Role == DqeRole.DistrictAdministrator
                    ? "D"
                    : user.Role == DqeRole.CostBasedTemplateAdministrator
                        ? "T"
                        : user.Role == DqeRole.Estimator
                            ? "E"
                            : user.Role == DqeRole.PayItemAdministrator
                                ? "P"
                                : string.Empty;
            var roleName = user.Role == DqeRole.Administrator
                ? "Administrator"
                : user.Role == DqeRole.DistrictAdministrator
                    ? "District Coordinator"
                    : user.Role == DqeRole.CostBasedTemplateAdministrator
                        ? "Cost Based Template Administrator"
                        : user.Role == DqeRole.Estimator
                            ? "Estimator"
                            : user.Role == DqeRole.PayItemAdministrator
                                ? "Pay Item Administrator"
                                : string.Empty;
            return
                Json(
                    new
                    {
                        id = dqeIdenetity.Id,
                        isAuthenticated = dqeIdenetity.IsAuthenticated,
                        role,
                        name = dqeIdenetity.Name,
                        district = dqeIdenetity.District,
                        userNameAndRole = string.Format("{0} - {1} {2}", dqeIdenetity.Name, dqeIdenetity.District, roleName)
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
        public void DumpSql()
        {
            Container.DumpSql();
        }

        [HttpPost]
        public ActionResult GetTimeout()
        {
            var cookie = Request.Cookies["DQE_AUTH_TICKET"];
            if (cookie != null && !string.IsNullOrWhiteSpace(cookie.Value))
            {
                FormsAuthenticationTicket ticket;
                try
                {
                    ticket = FormsAuthentication.Decrypt(cookie.Value);
                }
                catch (CryptographicException)
                {
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
#if DEBUG
            return new DqeResult(new
            {
                canImpersonate = true
            });
#else
            //var ev = _environmentProvider.GetEnvironment().ToUpper();
            //if (ev.StartsWith("U"))
            //{
            //    return new DqeResult(new
            //    {
            //        canImpersonate = true
            //    });
            //}
            var ev = _environmentProvider.GetEnvironment().ToUpper();
            return new DqeResult(new
            {
                //canImpersonate = false
                canImpersonate = !ev.StartsWith("P") && Convert.ToBoolean(ConfigurationManager.AppSettings["allowImpersonation"])
            });
#endif
        }

        [HttpPost]
        public void ImpersonateUser(dynamic user)
        {
#if !DEBUG
            var ev = _environmentProvider.GetEnvironment().ToUpper();
            if (ev.StartsWith("P") || !Convert.ToBoolean(ConfigurationManager.AppSettings["allowImpersonation"]))
            {
                throw new InvalidOperationException("Impersonation is not allowed in this environment");
            }
#endif
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
                t.Role = user.role == "A"
                    ? DqeRole.Administrator
                    : user.role == "D"
                        ? DqeRole.DistrictAdministrator
                        : user.role == "P"
                            ? DqeRole.PayItemAdministrator
                            : user.role == "T"
                                ? DqeRole.CostBasedTemplateAdministrator
                                : DqeRole.Estimator;
                u.Transform(t, sys);
                _commandRepository.Add(u);
            }
            else
            {
                var t = u.GetTransformer();
                t.IsActive = true;
                t.District = user.district;
                t.Role = user.role == "A"
                    ? DqeRole.Administrator
                    : user.role == "D"
                        ? DqeRole.DistrictAdministrator
                        : user.role == "P"
                            ? DqeRole.PayItemAdministrator
                            : user.role == "T"
                                ? DqeRole.CostBasedTemplateAdministrator
                                : DqeRole.Estimator;
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
            var expireTimeMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["expireTimeMinutes"]);
            var identity = new DqeIdentity(user.Id, user.SrsId, user.Name, user.District);
            var authenticationTicket = new FormsAuthenticationTicket(
                1,
                user.Name,
                DateTime.Now,
                DateTime.Now.AddMinutes(expireTimeMinutes),
                false,
                JsonConvert.SerializeObject(identity)
                );
            return FormsAuthentication.Encrypt(authenticationTicket);
        }
    }
}