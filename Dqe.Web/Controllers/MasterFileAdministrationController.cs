using System;
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
    [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
    public class MasterFileAdministrationController : Controller
    {
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly ICommandRepository _commandRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IMasterFileRepository _masterFileRepository;
        private readonly IPayItemRepository _payItemRepository;

        public MasterFileAdministrationController
            (
            IDqeUserRepository dqeUserRepository,
            ICommandRepository commandRepository,
            ITransactionManager transactionManager,
            IMasterFileRepository masterFileRepository,
            IPayItemRepository payItemRepository
            )
        {
            _dqeUserRepository = dqeUserRepository;
            _commandRepository = commandRepository;
            _transactionManager = transactionManager;
            _masterFileRepository = masterFileRepository;
            _payItemRepository = payItemRepository;
        }

        [HttpGet]
        public ActionResult GetMasterFiles()
        {
            var items = _masterFileRepository.GetAll()
                .Select(i => new
                {
                    id = i.Id,
                    fileNumber = i.FileNumber,
                })
                .ToList();
            return new DqeResult(items, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddMasterFile(dynamic payload)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var mf = new MasterFile(_masterFileRepository);
            var mft = mf.GetTransformer();
            int fileNumber;
            if (int.TryParse(payload.add.ToString(), out fileNumber))
            {
                mft.FileNumber = fileNumber;
                mf.Transform(mft, currentDqeUser);
                var r = EntityValidator.Validate(_transactionManager, mf);
                if (r != null) return r;
                _commandRepository.Add(mf);
                if (!DynamicHelper.HasNotNullProperty(payload, "copy") || payload.copy <= 0) return new DqeResult(null, new ClientMessage{ Severity = ClientMessageSeverity.Success, text = "The new Master File was created" });
                var parsedEffectiveDate = DateTime.MinValue;
                if (DynamicHelper.HasNotNullProperty(payload, "effectiveDate") && !string.IsNullOrWhiteSpace(payload.effectiveDate.ToString()))
                {
                    if (!DateTime.TryParse(payload.effectiveDate.ToString(), out parsedEffectiveDate))
                    {
                        return new DqeResult(null, new ClientMessage{ Severity = ClientMessageSeverity.Error, text = "Effective Date is invalid." });
                    }
                }
                if (parsedEffectiveDate == DateTime.MinValue)
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Effective Date is required." });
                }
                var copyMf = _masterFileRepository.Get((int)payload.copy);
                foreach (var piCopy in copyMf.PayItems)
                {
                    var t = piCopy.GetTransformer();
                    if (piCopy.EffectiveDate.HasValue)
                    {
                        if (!piCopy.ObsoleteDate.HasValue)
                        {
                            t.ObsoleteDate = parsedEffectiveDate.AddDays(-1).Date;
                            piCopy.Transform(t, currentDqeUser);
                            r = EntityValidator.Validate(_transactionManager, piCopy);
                            if (r != null) return r;
                        }
                    }
                    var piNew = new PayItem(_payItemRepository);
                    t.ObsoleteDate = null;
                    t.EffectiveDate = parsedEffectiveDate.Date;
                    piNew.AssociatePayItemToStructureAndMasterFile(piCopy.MyPayItemStructure, mf);
                    piNew.Transform(t, currentDqeUser);
                    _commandRepository.Add(piNew);
                    r = EntityValidator.Validate(_transactionManager, piNew);
                    if (r != null) return r;
                }
            }
            else
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "The new Master File must be numeric and greater than 1" });
            }
            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Success, text = "The new Master File was created" });
        }
    }
}