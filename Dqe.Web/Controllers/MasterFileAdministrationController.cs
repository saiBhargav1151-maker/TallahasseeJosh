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

        public MasterFileAdministrationController
            (
            IDqeUserRepository dqeUserRepository,
            ICommandRepository commandRepository,
            ITransactionManager transactionManager,
            IMasterFileRepository masterFileRepository
            )
        {
            _dqeUserRepository = dqeUserRepository;
            _commandRepository = commandRepository;
            _transactionManager = transactionManager;
            _masterFileRepository = masterFileRepository;
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
                if (payload.copy > 0)
                {
                    //var copyMf = _masterFileRepository.Get((int)payload.copy);
                    //TODO: copy pay items from existing master file
                }
            }
            else
            {
                return new DqeResult(null,
                    new ClientMessage
                    {
                        Severity = ClientMessageSeverity.Error,
                        text = "The new Master File must be numeric and greater than 1"
                    });
            }
            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Success, text = "The new Master File was created" });
        }
    }
}