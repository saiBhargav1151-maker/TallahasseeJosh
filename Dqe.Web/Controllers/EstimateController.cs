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
        
        public EstimateController
            (
            IDqeUserRepository dqeUserRepository
            )
        {
            _dqeUserRepository = dqeUserRepository;
        }

        public ActionResult LoadEstimate()
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            if (currentDqeUser.MyRecentProjectSnapshot == null)
            {
                return new DqeResult(null, JsonRequestBehavior.AllowGet);
            }
            var project = currentDqeUser.MyRecentProjectSnapshot.MyProjectVersion.MyProject;
            if (project.CustodyOwner != currentDqeUser)
            {
                return new DqeResult(null, JsonRequestBehavior.AllowGet);
            }
            return new DqeResult(new
            {
                id = project.Id,
                number = project.ProjectNumber,
                description = project.Description,
                county = project.MyCounty.Name,
                groups = currentDqeUser.MyRecentProjectSnapshot.EstimateGroups.Select(i => new
                {
                    id = i.Id,
                    description = i.Description,
                    payItems = i.ProjectItems.Select(ii => new
                    {
                        id = ii.Id,
                        number = ii.PayItemNumber,
                        description = ii.PayItemDescription,
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
            if (currentDqeUser.MyRecentProjectSnapshot == null)
            {
                return new DqeResult(null, JsonRequestBehavior.AllowGet);
            }
            var project = currentDqeUser.MyRecentProjectSnapshot.MyProjectVersion.MyProject;
            if (project.CustodyOwner != currentDqeUser)
            {
                return new DqeResult(null, JsonRequestBehavior.AllowGet);
            }
            //var t = project.GetTransformer();
            //project.Transform(t, currentDqeUser);
            foreach (var group in estimate.groups)
            {
                var g = group;
                var eg = currentDqeUser.MyRecentProjectSnapshot.EstimateGroups.SingleOrDefault(i => i.Id == (int)g.id);
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