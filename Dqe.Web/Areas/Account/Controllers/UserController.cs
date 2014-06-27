using System;
using System.Security.Principal;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using Dqe.ApplicationServices;
using Dqe.Domain.Messaging;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories;
using Dqe.Domain.Repositories.Custom;
using Dqe.Web.Areas.Account.Models;
using Dqe.Web.Filters;
using Dqe.Web.Helpers;
using Dqe.Web.Messaging;

namespace Dqe.Web.Areas.Account.Controllers
{
    public class UserController : Controller
    {
        private readonly ICommandRepository _commandRepository;
        private readonly IMessenger _messenger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IPrincipal _principal;
        private readonly IContextService _contextService;

        public UserController
            (
            ICommandRepository commandRepository,
            IMessenger messenger,
            IUserAccountRepository userAccountRepository,
            IPrincipal principal,
            IContextService contextService
            )
        {
            _commandRepository = commandRepository;
            _messenger = messenger;
            _userAccountRepository = userAccountRepository;
            _principal = principal;
            _contextService = contextService;
        }

        [HttpGet]
        [RemoteRequireHttps]
        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home", new { Area = "" });
        }

        [HttpGet]
        [RemoteRequireHttps]
        public ActionResult Authenticate()
        {
            if (_principal.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
            return View(new AuthenticateViewModel());
        }

        [HttpPost]
        [RemoteRequireHttps]
        public ActionResult Authenticate(AuthenticateViewModel model)
        {
            if (_principal.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
            if (!ModelState.IsValid) return View(model);
            if (_userAccountRepository.Authenticate(model.UserEmail.ToLower().Trim(), model.Password))
            {
                FormsAuthentication.SetAuthCookie(model.UserEmail.ToLower().Trim(), false);
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
            ModelState.AddModelError<AuthenticateViewModel>(i => i.UserEmail, "Email or Password is invalid.");
            return View(model);
        }

        [HttpGet]
        [RemoteRequireHttps]
        public ActionResult Register()
        {
            if (_principal.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [RemoteRequireHttps]
        public ActionResult Register(RegisterViewModel model)
        {
            if (_principal.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
            if (!ModelState.IsValid) return View(model);
            var accounts = _userAccountRepository.Count();
            var role = (accounts == 0) 
                ? ApplicationRoles.Admin 
                : ApplicationRoles.AccountHolder;
            string token = null; 
            if (accounts > 0)
            {
                token = Utilities.PackGuid(Guid.NewGuid());
                if (_userAccountRepository.Get(model.UserEmail.ToLower().Trim()) != null)
                {
                    ModelState.AddModelError<RegisterViewModel>(i => i.UserEmail, "This account already exists.");
                    return View(model);
                }
            }
            var user = new UserAccount(_messenger)
                           {
                               Email = model.UserEmail.ToLower().Trim(),
                               AccountPassword = model.Password.Trim(),
                               FirstName = model.FirstName.Trim(),
                               LastName = model.LastName.Trim(),
                               AccountRole = role,
                               UnverifiedAccountToken = token
                           };
            _commandRepository.Add(user);
            if (token == null)
            {
                FormsAuthentication.SetAuthCookie(user.Email, false);
                return RedirectToAction("Index", "Home", new { Area = "" });    
            }
            var sb = new StringBuilder();
            sb.AppendFormat("An email has been sent to {0} with instructions on how to verify your account.", user.Email);
            Dispatch.AddMessage(_contextService.ClientId, new ClientMessage
                                                              {
                                                                  MessageHeader = "Status", 
                                                                  MessageBody = sb.ToString(),
                                                                  PersistMessage = true
                                                              });
            _messenger.Notify(new AccountVerificationEmail(user.Email, token, _contextService));
            return RedirectToAction("Authenticate", "User", new { Area = "Account" });
        }

        [HttpGet]
        public ActionResult ValidateAccount(string id)
        {
            if (_principal.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
            if (_userAccountRepository.ValidateAccount(id))
            {
                Dispatch.AddMessage(_contextService.ClientId, new ClientMessage
                                                                  {
                                                                      MessageHeader = "Status", 
                                                                      MessageBody = "Your account has been verified.",
                                                                      PersistMessage = true
                                                                  });
            }
            return RedirectToAction("Authenticate", "User", new { Area = "Account" });
        }

        [HttpGet]
        [RemoteRequireHttps]
        public ActionResult ChangePassword()
        {
            if (!_principal.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [RemoteRequireHttps]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!_principal.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
            if (!ModelState.IsValid) return View(model);
            var account = _userAccountRepository.Get(_principal.Identity.Name.Trim().ToLower());
            if (account.AccountPassword != model.CurrentPassword)
            {
                ModelState.AddModelError<ChangePasswordViewModel>(i => i.CurrentPassword, "Your current password is not correct.");
                return View(model);
            }
            account.AccountPassword = model.NewPassword;
            Dispatch.AddMessage(_contextService.ClientId, new ClientMessage
                                                              {
                                                                  MessageHeader = "Status", 
                                                                  MessageBody = "Your password has been changed."
                                                              });
            return RedirectToAction("Index", "Home", new { Area = "" });
        }

        [HttpGet]
        [RemoteRequireHttps]
        public ActionResult ChangeAccountName()
        {
            if (!_principal.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
            return View(new ChangeAccountNameViewModel());
        }

        [HttpPost]
        [RemoteRequireHttps]
        public ActionResult ChangeAccountName(ChangeAccountNameViewModel model)
        {
            if (!_principal.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
            if (!ModelState.IsValid) return View(model);
            var account = _userAccountRepository.Get(model.UserEmail.Trim().ToLower());
            if (account != null)
            {
                ModelState.AddModelError<RegisterViewModel>(i => i.UserEmail, "This account already exists.");
                return View(model);
            }
            account = _userAccountRepository.Get(_principal.Identity.Name.Trim().ToLower());
            account.Email = model.UserEmail;
            account.UnverifiedAccountToken = Utilities.PackGuid(Guid.NewGuid());
            var sb = new StringBuilder();
            sb.AppendFormat("An email has been sent to {0} with instructions on how to verify your account.", account.Email);
            Dispatch.AddMessage(_contextService.ClientId, new ClientMessage
                                                              {
                                                                  MessageHeader = "Status", 
                                                                  MessageBody = sb.ToString(),
                                                                  PersistMessage = true
                                                              });
            _messenger.Notify(new AccountVerificationEmail(account.Email, account.UnverifiedAccountToken, _contextService));
            FormsAuthentication.SignOut();
            return RedirectToAction("Authenticate", "User", new { Area = "Account" });
        }

        [HttpGet]
        [RemoteRequireHttps]
        public ActionResult UserProfile()
        {
            if (!_principal.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
            var account = _userAccountRepository.Get(_principal.Identity.Name.Trim().ToLower());
            var model = new UserProfileViewModel
                            {
                                FirstName = account.FirstName, 
                                LastName = account.LastName
                            };
            return View(model);
        }

        [HttpPost]
        [RemoteRequireHttps]
        public ActionResult UserProfile(UserProfileViewModel model)
        {
            if (!_principal.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
            if (!ModelState.IsValid) return View(model);
            var account = _userAccountRepository.Get(_principal.Identity.Name.Trim().ToLower());
            account.FirstName = model.FirstName;
            account.LastName = model.LastName;
            Dispatch.AddMessage(_contextService.ClientId, new ClientMessage
                                                              {
                                                                  MessageHeader = "Status", 
                                                                  MessageBody = "Your profile was updated."
                                                              });
            return RedirectToAction("Index", "Home", new { Area = "" });
        }

        [HttpGet]
        [RemoteRequireHttps]
        public ActionResult Reset()
        {
            if (_principal.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
            return View(new ResetViewModel());
        }

        [HttpPost]
        [RemoteRequireHttps]
        public ActionResult Reset(ResetViewModel model)
        {
            if (_principal.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
            if (!ModelState.IsValid) return View(model);
            var account = _userAccountRepository.Get(model.UserEmail.Trim().ToLower());
            if (account == null)
            {
                ModelState.AddModelError<ResetViewModel>(i => i.UserEmail, "This account does not exist.");
                return View(model);
            }
            var sb = new StringBuilder();
            if (account.UnverifiedAccountToken == null)
            {
                sb.AppendFormat("An email has been sent to {0} with your account credentials.", account.Email);
                Dispatch.AddMessage(_contextService.ClientId, new ClientMessage
                                                                  {
                                                                      MessageHeader = "Status", 
                                                                      MessageBody = sb.ToString(),
                                                                      PersistMessage = true
                                                                  });
                _messenger.Notify(new AccountResetEmail(account.Email, account.AccountPassword, _contextService));
            }
            if (account.UnverifiedAccountToken != null)
            {
                account.UnverifiedAccountToken = Utilities.PackGuid(Guid.NewGuid());
                sb.AppendFormat("An email has been sent to {0} with instructions on how to verify your account.", account.Email);
                Dispatch.AddMessage(_contextService.ClientId, new ClientMessage
                                                                  {
                                                                      MessageHeader = "Status", 
                                                                      MessageBody = sb.ToString(),
                                                                      PersistMessage = true
                                                                  });
                _messenger.Notify(new AccountVerificationEmail(account.Email, account.UnverifiedAccountToken, _contextService));
            }
            return RedirectToAction("Authenticate", "User", new { Area = "Account" });
        }
    }
}
