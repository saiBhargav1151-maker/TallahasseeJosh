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

namespace Dqe.Web.Controllers
{
    [RemoteRequireHttps]
    [CustomAuthorize(Roles = new [] {DqeRole.Administrator})]
    public class CodeAdministrationController : Controller
    {
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly IDqeCodeRepository _dqeCodeRepository;
        private readonly ICommandRepository _commandRepository;

        public CodeAdministrationController
            (
            IDqeUserRepository dqeUserRepository,
            IDqeCodeRepository dqeCodeRepository,
            ICommandRepository commandRepository
            )
        {
            _dqeUserRepository = dqeUserRepository;
            _dqeCodeRepository = dqeCodeRepository;
            _commandRepository = commandRepository;
        }

        [HttpGet]
        public ActionResult GetCodes(string codeType)
        {
            Func<IEnumerable<DqeCode>> func;
            switch (codeType)
            {
                //case "PU":
                //    func = _dqeCodeRepository.GetPrimaryUnits;
                //    break;
                //case "SU":
                //    func = _dqeCodeRepository.GetSecondaryUnits;
                //    break;
                default:
                    throw new ArgumentOutOfRangeException("codeType");
            }
            var results = func.Invoke().ToList();
            if (!results.Any())
            {
                return new DqeResult(new object[] {}, new ClientMessage {text = "No codes defined"}, JsonRequestBehavior.AllowGet);
            }
            return
                new DqeResult(results.Select(i =>
                    new
                    {
                        id = i.Id,
                        name = i.Name,
                        isActive = i.IsActive
                    }), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateCodes(dynamic codeSet)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var codeType = (string)codeSet.codeType;
            var codes = ((IEnumerable<dynamic>)codeSet.codes).ToList();
            var newCode = codes.FirstOrDefault(i => i.id == 0);
            if (newCode != null)
            {
                //new code to create
                DqeCode nc;
                switch (codeType)
                {
                    //case "PU":
                    //    nc = new PrimaryUnit();
                    //    break;
                    //case "SU":
                    //    nc = new SecondaryUnit();
                    //    break;
                    default:
                        throw new ArgumentOutOfRangeException("codeSet");
                }
                var t = nc.GetTransformer();
                t.IsActive = newCode.isActive;
                t.Name = newCode.name;
                nc.Transform(t, currentDqeUser);
                _commandRepository.Add(nc);
            }
            var existingCodes = codes.Where(i => i.id != 0).ToList();
            foreach (var existingCode in existingCodes)
            {
                //update
                if (!string.IsNullOrWhiteSpace((string)existingCode.name))
                {
                    var dqeCode = _dqeCodeRepository.Get((int)existingCode.id);
                    if (dqeCode != null)
                    {
                        var t = dqeCode.GetTransformer();
                        t.Name = existingCode.name;
                        //TODO: what about deleting inactive codes that have never been used?
                        t.IsActive = existingCode.isActive;
                        dqeCode.Transform(t, currentDqeUser);
                    }
                }
            }
            return GetCodes(codeType);
        }
    }
}