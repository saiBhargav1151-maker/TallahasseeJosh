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
    //[CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator })]
    public class ProfileController : Controller
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly ISystemParametersRepository _systemParametersRepository;

        public ProfileController
            (
            IProjectRepository projectRepository, 
            IDqeUserRepository dqeUserRepository,
            ISystemParametersRepository systemParametersRepository
            )
        {
            _projectRepository = projectRepository;
            _dqeUserRepository = dqeUserRepository;
            _systemParametersRepository = systemParametersRepository;
        }

        [HttpGet]
        public ActionResult GetRecentProjects()
        {
            var currentUser = User.Identity as DqeIdentity;
            if (currentUser == null) return new DqeResult(null, JsonRequestBehavior.AllowGet);
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var projects = _projectRepository.GetProjects(currentDqeUser);

            return new DqeResult(projects.Select(i => new
            {
                id = i.Id,
                number = i.ProjectNumber,
                proposal = i.Proposals.FirstOrDefault(ii => ii.ProposalSource == ProposalSourceType.Wt) == null
                    ? string.Empty
                    : i.Proposals.First(ii => ii.ProposalSource == ProposalSourceType.Wt).ProposalNumber,
                owner = i.CustodyOwner == null ? string.Empty : i.CustodyOwner.Name
            }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult InitializeParametersFromWt()
        {
            return new DqeResult(new { loadPrices = _systemParametersRepository.Get().LoadPrices }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator })]
        public ActionResult SetParameters(dynamic parms)
        {
            var sp = _systemParametersRepository.Get();
            sp.LoadPrices = parms.loadPrices;
            return new DqeResult(null, new ClientMessage{Severity = ClientMessageSeverity.Success, text = "System Parameters updated."});
        }
    }
}