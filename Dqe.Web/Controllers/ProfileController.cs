using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Dqe.ApplicationServices;
using Dqe.Domain.Fdot;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories;
using Dqe.Domain.Repositories.Custom;
using Dqe.Web.ActionResults;
using Dqe.Web.Attributes;
using Dqe.Web.Services;
using Project = Dqe.Domain.Model.Project;
using Proposal = Dqe.Domain.Model.Proposal;

namespace Dqe.Web.Controllers
{
    [RemoteRequireHttps]
    //[CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator })]
    public class ProfileController : Controller
    {
        private readonly IWebTransportService _webTransportService;
        private readonly IProjectRepository _projectRepository;
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly IMasterFileRepository _masterFileRepository;
        private readonly ICommandRepository _commandRepository;
        private readonly IMarketAreaRepository _marketAreaRepository;
        private readonly IProposalRepository _proposalRepository;
        private readonly ITransactionManager _transactionManager;

        public ProfileController
            (
            IWebTransportService webTransportService,
            IProjectRepository projectRepository,
            IDqeUserRepository dqeUserRepository,
            IMasterFileRepository masterFileRepository,
            IMarketAreaRepository marketAreaRepository,
            IProposalRepository proposalRepository,
            ICommandRepository commandRepository,
            ITransactionManager transactionManager
            )
        {
            _webTransportService = webTransportService;
            _projectRepository = projectRepository;
            _dqeUserRepository = dqeUserRepository;
            _masterFileRepository = masterFileRepository;
            _marketAreaRepository = marketAreaRepository;
            _proposalRepository = proposalRepository;
            _commandRepository = commandRepository;
            _transactionManager = transactionManager;
        }

        [HttpGet]
        public ActionResult GetRecentProjects()
        {
            var currentUser = (DqeIdentity)User.Identity;
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
    }
}