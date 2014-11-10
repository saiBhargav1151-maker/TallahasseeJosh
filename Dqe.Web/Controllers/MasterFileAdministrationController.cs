using System;
using System.Linq;
using System.Threading.Tasks;
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
    public class MasterFileAdministrationController : Controller
    {
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IMasterFileRepository _masterFileRepository;
        private readonly ITaskRunner _taskRunner;
        private readonly ICommandRepository _commandRepository;
        private readonly ISystemTaskRepository _systemTaskRepository;
       
        public MasterFileAdministrationController
            (
            IDqeUserRepository dqeUserRepository,
            ITransactionManager transactionManager,
            IMasterFileRepository masterFileRepository,
            ITaskRunner taskRunner,
            ICommandRepository commandRepository,
            ISystemTaskRepository systemTaskRepository
            )
        {
            _dqeUserRepository = dqeUserRepository;
            _transactionManager = transactionManager;
            _masterFileRepository = masterFileRepository;
            _taskRunner = taskRunner;
            _commandRepository = commandRepository;
            _systemTaskRepository = systemTaskRepository;
        }

        [HttpGet]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
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

        [HttpGet]
        public ActionResult IsCopyInProcess()
        {
            var val = _systemTaskRepository.GetByTaskId(ApplicationConstants.Tasks.CopyMasterFile);
            return val == null 
                ? new DqeResult(false, JsonRequestBehavior.AllowGet) 
                : new DqeResult(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult AddMasterFile(dynamic payload)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            int fileNumber;
            if (int.TryParse(payload.add.ToString(), out fileNumber))
            {
                if (!DynamicHelper.HasNotNullProperty(payload, "copy") || payload.copy <= 0)
                {
                    var mf = new MasterFile(_masterFileRepository);
                    var mft = mf.GetTransformer();
                    mft.FileNumber = fileNumber;
                    mf.Transform(mft, currentDqeUser);
                    var r = EntityValidator.Validate(_transactionManager, mf);
                    if (r == null)
                    {
                        _commandRepository.Add(mf);
                        return new DqeResult(true, new ClientMessage { Severity = ClientMessageSeverity.Success, text = "The new Master File was created" });
                    }
                    return r;
                }
                var parsedEffectiveDate = DateTime.MinValue;
                if (DynamicHelper.HasNotNullProperty(payload, "effectiveDate") && !string.IsNullOrWhiteSpace(payload.effectiveDate.ToString()))
                {
                    if (!DateTime.TryParse(payload.effectiveDate.ToString(), out parsedEffectiveDate))
                    {
                        return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Effective Date is invalid." });
                    }
                }
                if (parsedEffectiveDate == DateTime.MinValue)
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Effective Date is required." });
                }
                CopyMasterFile((int)payload.copy, parsedEffectiveDate.Date, currentUser.SrsId, fileNumber);
                var val = _systemTaskRepository.GetByTaskId(ApplicationConstants.Tasks.CopyMasterFile);
                if (val == null)
                {
                    var t = new SystemTask { TaskId = ApplicationConstants.Tasks.CopyMasterFile };
                    _commandRepository.Add(t);
                }
            }
            else
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "The new Master File must be numeric and greater than 1" });
            }
            return new DqeResult(null);
        }

        private void CopyMasterFile(int masterFileId, DateTime effectiveDate, int currentUserSrsId, int fileNumber)
        {
            Task.Run(() => _taskRunner.CopyMasterFile(User.Identity.Name, masterFileId, effectiveDate, currentUserSrsId, fileNumber));
        }
    }
}