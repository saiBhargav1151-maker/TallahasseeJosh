using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Dqe.ApplicationServices;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories;
using Dqe.Domain.Repositories.Custom;
using Dqe.Web.ActionResults;
using Dqe.Web.Attributes;
using Dqe.Web.Services;

namespace Dqe.Web.Controllers
{
    [RemoteRequireHttps]
    [CustomAuthorize(Roles = new [] {DqeRole.Administrator, DqeRole.PayItemAdministrator })]
    public class WebLinkAdministrationController : Controller
    {
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly IDqeWebLinkRepository _dqeWebLinkRepository;
        private readonly ICommandRepository _commandRepository;
        private readonly ITransactionManager _transactionManager;

        public WebLinkAdministrationController
            (
            IDqeUserRepository dqeUserRepository,
            IDqeWebLinkRepository dqeWebLinkRepository,
            ICommandRepository commandRepository,
            ITransactionManager transactionManager
            )
        {
            _dqeUserRepository = dqeUserRepository;
            _dqeWebLinkRepository = dqeWebLinkRepository;
            _commandRepository = commandRepository;
            _transactionManager = transactionManager;
        }

        [HttpGet]
        public ActionResult SearchWebLinks(string linkType, string val)
        {
            var links = _dqeWebLinkRepository.GetWebLinks(linkType, val).ToList();
            return
                new DqeResult(links.Select(i =>
                    new
                    {
                        id = i.Id,
                        name = i.Name,
                        webLink = i.WebLink
                    }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetWebLinks(string linkType)
        {
            Func<IEnumerable<DqeWebLink>> func;
            switch (linkType)
            {
                case "OR":
                    func = _dqeWebLinkRepository.GetOtherReferences;
                    break;
                case "PC":
                    func = _dqeWebLinkRepository.GetPpmChapters;
                    break;
                case "PD":
                    func = _dqeWebLinkRepository.GetPrepAndDocChapters;
                    break;
                case "SP":
                    func = _dqeWebLinkRepository.GetSpecifications;
                    break;
                case "ST":
                    func = _dqeWebLinkRepository.GetSpecTypes;
                    break;
                case "SD":
                    func = _dqeWebLinkRepository.GetStandards;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("linkType");
            }
            var results = func.Invoke().ToList();
            if (!results.Any())
            {
                return new DqeResult(new object[] {}, new ClientMessage {text = "No web links defined"}, JsonRequestBehavior.AllowGet);
            }
            return
                new DqeResult(results.Select(i =>
                    new
                    {
                        id = i.Id,
                        name = i.Name,
                        webLink = i.WebLink,
                        selected = false
                    }), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateWebLinks(dynamic linkSet)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var linkType = (string)linkSet.linkType;
            var links = ((IEnumerable<dynamic>)linkSet.links).ToList();
            var newLink = links.FirstOrDefault(i => i.id == 0);
            if (newLink != null)
            {
                //new code to create
                DqeWebLink nl;
                switch (linkType)
                {
                    case "OR":
                        nl = new OtherReferenceWebLink();
                        break;
                    case "PC":
                        nl = new PpmChapterWebLink();
                        break;
                    case "PD":
                        nl = new PrepAndDocChapterWebLink();
                        break;
                    case "SP":
                        nl = new SpecificationWebLink();
                        break;
                    case "ST":
                        nl = new SpecTypeWebLink();
                        break;
                    case "SD":
                        nl = new StandardWebLink();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("linkSet");
                }
                var t = nl.GetTransformer();
                t.Name = newLink.name;
                t.WebLink = newLink.webLink;
                nl.Transform(t, currentDqeUser);
                var r = EntityValidator.Validate(_transactionManager, nl);
                if (r != null) return r;
                _commandRepository.Add(nl);
            }
            var existingLinks = links.Where(i => i.id != 0).ToList();
            foreach (var existingLink in existingLinks)
            {
                //update
                if (string.IsNullOrWhiteSpace((string) existingLink.name) &&
                    string.IsNullOrWhiteSpace((string) existingLink.webLink))
                {
                    //TODO: disassociate / delete
                }
                else
                {
                    var dqeLink = _dqeWebLinkRepository.Get((int)existingLink.id);
                    if (dqeLink != null)
                    {
                        var t = dqeLink.GetTransformer();
                        t.Name = existingLink.name;
                        t.WebLink = existingLink.webLink;
                        dqeLink.Transform(t, currentDqeUser);
                        var r = EntityValidator.Validate(_transactionManager, dqeLink);
                        if (r != null) return r;
                    }
                }
            }
            return GetWebLinks(linkType);
        }

        [HttpPost]
        public ActionResult RemoveLinks(dynamic links)
        {
            var selectedLinks = ((IEnumerable<dynamic>)links).Where(i => i.selected).ToList();
            foreach (var selectedLink in selectedLinks)
            {
                var u = _dqeWebLinkRepository.Get(selectedLink.id);
                if (u != null) _commandRepository.Remove(u);
            }
            return new DqeResult(null, new ClientMessage { text = "Link(s) Removed" });
        }
    }
}