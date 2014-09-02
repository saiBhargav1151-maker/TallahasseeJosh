using System.Linq;
using System.Web.Mvc;
using Dqe.ApplicationServices;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using Dqe.Web.ActionResults;
using Dqe.Web.Attributes;

namespace Dqe.Web.Controllers
{
    [RemoteRequireHttps]
    [CustomAuthorize(Roles = new[] {DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator})]
    public class EstimateController : Controller
    {
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly ISystemTaskRepository _systemTaskRepository;
        
        public EstimateController
            (
            IDqeUserRepository dqeUserRepository,
            ISystemTaskRepository systemTaskRepository
            )
        {
            _dqeUserRepository = dqeUserRepository;
            _systemTaskRepository = systemTaskRepository;
        }

        public ActionResult LoadEstimate()
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            if (currentDqeUser.MyRecentProject == null)
            {
                return new DqeResult(null, JsonRequestBehavior.AllowGet);
            }
            var copyMasterFileInProcess = _systemTaskRepository.GetByTaskId(ApplicationConstants.Tasks.CopyMasterFile) != null;
            return new DqeResult(new
            {
                id = currentDqeUser.MyRecentProject.Id,
                number = currentDqeUser.MyRecentProject.ProjectNumber,
                description = currentDqeUser.MyRecentProject.Description,
                county = currentDqeUser.MyRecentProject.County,
                groups = currentDqeUser.MyRecentProject.EstimateGroups.Select(i => new
                {
                    id = i.Id,
                    description = i.Description,
                    payItems = i.ProjectItems.Select(ii => new
                    {
                        id = ii.Id,
                        number = copyMasterFileInProcess ? ii.UnknownPayItemNumber : ii.MyPayItem == null ? ii.UnknownPayItemNumber : ii.MyPayItem.PayItemId,
                        description = copyMasterFileInProcess ? ii.UnknownPayItemDescription : ii.MyPayItem == null ? ii.UnknownPayItemDescription : ii.MyPayItem.Description,
                        quantity = ii.Quantity,
                        price = ii.Price
                    })
                })
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveEstimate(dynamic estimate)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            if (currentDqeUser.MyRecentProject == null)
            {
                return new DqeResult(null, JsonRequestBehavior.AllowGet);
            }
            foreach (var group in estimate.groups)
            {
                var g = group;
                var eg = currentDqeUser.MyRecentProject.EstimateGroups.SingleOrDefault(i => i.Id == (int)g.id);
                if (eg == null) continue;
                foreach (var payItem in g.payItems)
                {
                    var p = payItem;
                    var pi = eg.ProjectItems.SingleOrDefault(i => i.Id == (int)p.id);
                    if (pi == null) continue;
                    var pit = pi.GetTransformer();
                    pit.Price = (decimal)p.price;
                    pi.Transform(pit, currentDqeUser);
                }
            }
            return new DqeResult(null, new ClientMessage{ Severity = ClientMessageSeverity.Success, text = "Estimate saved"}, JsonRequestBehavior.AllowGet);
        }
    }
}