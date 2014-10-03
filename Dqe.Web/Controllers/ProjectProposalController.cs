using System;
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

namespace Dqe.Web.Controllers
{
    [RemoteRequireHttps]
    [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator })]
    public class ProjectProposalController : Controller
    {
        private readonly IWebTransportService _webTransportService;
        private readonly IProjectRepository _projectRepository;
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly IMasterFileRepository _masterFileRepository;
        private readonly ICommandRepository _commandRepository;
        private readonly IMarketAreaRepository _marketAreaRepository;
        private readonly IProposalRepository _proposalRepository;

        public ProjectProposalController
            (
            IWebTransportService webTransportService,
            IProjectRepository projectRepository,
            IDqeUserRepository dqeUserRepository,
            IMasterFileRepository masterFileRepository,
            IMarketAreaRepository marketAreaRepository,
            IProposalRepository proposalRepository,
            ICommandRepository commandRepository
            )
        {
            _webTransportService = webTransportService;
            _projectRepository = projectRepository;
            _dqeUserRepository = dqeUserRepository;
            _masterFileRepository = masterFileRepository;
            _marketAreaRepository = marketAreaRepository;
            _proposalRepository = proposalRepository;
            _commandRepository = commandRepository;
        }

        [HttpGet]
        public ActionResult GetRecentProject()
        {
            //var wtProjectEstimate = _webTransportService.ExportProject("200");
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            if (currentDqeUser.MyRecentProjectEstimate == null)
            {
                return new DqeResult(null, JsonRequestBehavior.AllowGet);
            }
            var result = ResultStructureFromSnapshot(currentDqeUser.MyRecentProjectEstimate, currentDqeUser);
            return result;
        }

        [HttpGet]
        public ActionResult GetProject(string number)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            //is project in DQE?
            var project  = _projectRepository.GetByProjectNumber(number);
            if (project == null)
            {
                //not in DQE
                var wtProjectEstimate = _webTransportService.ExportProject(number);
                if (wtProjectEstimate == null)
                {
                    return new DqeResult(null,
                        new ClientMessage
                        {
                            Severity = ClientMessageSeverity.Error,
                            text = string.Format("Project {0} was not found in Web Trnsport", number)
                        },
                        JsonRequestBehavior.AllowGet);
                }
                int mfid;
                if (!int.TryParse(wtProjectEstimate.SpecYear.Trim(), out mfid))
                {
                    throw new InvalidOperationException(string.Format("Could not parse spec year from WT project {0}",
                        wtProjectEstimate.EstimateId));
                }
                var mf = _masterFileRepository.GetByFileNumber(mfid);
                if (mf == null)
                {
                    return new DqeResult(null,
                        new ClientMessage
                        {
                            Severity = ClientMessageSeverity.Error,
                            text = string.Format("DQE does not contain Master File {0} for Project {1}", mfid, number)
                        },
                        JsonRequestBehavior.AllowGet);
                }
                var p = new Project(_projectRepository, _commandRepository);
                var t = p.GetTransformer();
                t.Description = wtProjectEstimate.Description;
                t.ProjectNumber = wtProjectEstimate.EstimateId;
                p.Transform(t, currentDqeUser);
                var county = _marketAreaRepository.GetCountyByCode(wtProjectEstimate.County);
                if (county == null)
                    throw new InvalidOperationException(string.Format("County {0} was not found in DQE",
                        wtProjectEstimate.County));
                p.SetCounty(county);
                mf.AddProject(p, currentDqeUser);
                _commandRepository.Add(p);
                //does WT have a proposal
                var wtp = _webTransportService.GetProject(p.ProjectNumber);
                if (wtp != null && wtp.MyProposal != null)
                {
                    AddProposalToProject(wtp.MyProposal, p, currentDqeUser);
                }
                return ResultStructureFromProjectSelection(p, currentDqeUser);
            }
            else
            {
                var wtp = _webTransportService.GetProject(project.ProjectNumber);
                var currentProp = project.Proposals.FirstOrDefault(i => i.ProposalSource == ProposalSourceType.Wt);
                if (currentProp == null)
                {
                    if (wtp != null && wtp.MyProposal != null)
                    {
                        //create WT proposal in DQE
                        AddProposalToProject(wtp.MyProposal, project, currentDqeUser);
                    }
                }
                else
                {
                    if (wtp != null && wtp.MyProposal != null)
                    {
                        //proosal in DQE and WT
                        if (currentProp.ProposalNumber != wtp.MyProposal.ProposalNumber)
                        {
                            //not same proposal - convert WT proposal to Gaming
                            currentProp.ConvertToGaming();
                            //create WT proposal in DQE
                            AddProposalToProject(wtp.MyProposal, project, currentDqeUser);
                        }
                    }
                }
            }
            //in DQE
            return ResultStructureFromProjectSelection(project, currentDqeUser);
        }

        private void AddProposalToProject(Domain.Model.Wt.Proposal wtp, Project project, DqeUser currentDqeUser)
        {
            var prop = new Proposal(_proposalRepository);
            var propt = prop.GetTransformer();
            propt.ProposalNumber = wtp.ProposalNumber;
            propt.ProposalSource = ProposalSourceType.Wt;
            propt.Comment = "Loaded from Web Trns*Port";
            prop.Transform(propt, currentDqeUser);
            project.AddProposal(prop, currentDqeUser);
        }

        [HttpPost]
        public ActionResult CreateProjectVersionFromWt(string number)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var project = _projectRepository.GetByProjectNumber(number);
            if (project == null) throw new InvalidOperationException(string.Format("Project {0} was not found in DQE", number));
            var wtProjectEstimate = _webTransportService.ExportProject(number);
            if (wtProjectEstimate == null)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Project {0} was not found in Web Trnsport", number) }, JsonRequestBehavior.AllowGet);
            }
            var ss = project.CreateNewVersionFromWt(string.Empty, wtProjectEstimate, currentDqeUser);
            return ResultStructureFromSnapshot(ss, currentDqeUser);
        }

        [HttpPost]
        public ActionResult ReleaseCustody(dynamic project)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var p = _projectRepository.Get((int)project.id);
            if (p.CustodyOwner != currentDqeUser)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Project {0} is not owned by you", project.number) });    
            }
            p.ReleaseCustody(currentDqeUser);
            return ResultStructureFromProjectSelection(p, currentDqeUser);
        }

        [HttpPost]
        public ActionResult AquireCustody(dynamic project)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var p = _projectRepository.Get((int)project.id);
            if (p.CustodyOwner != null)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Project {0} is currently owned by {1}", project.number, p.CustodyOwner.Name) });
            }
            p.AquireCustody(currentDqeUser);
            return ResultStructureFromProjectSelection(p, currentDqeUser);
        }

        [HttpPost]
        public ActionResult SnapshotWorkingEstimate(dynamic project)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var p = _projectRepository.Get((int)project.id);
            if (p.CustodyOwner != currentDqeUser)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Project {0} is not currently owned by {1}", project.number, p.CustodyOwner.Name) });
            }
            p.SnapshotWorkingEstimate(currentDqeUser, DynamicHelper.HasNotNullProperty(project, "takeLabeledSnapshot") && project.takeLabeledSnapshot);
            return ResultStructureFromProjectSelection(p, currentDqeUser);
        }

        [HttpPost]
        public ActionResult AssignWorkingEstimate(dynamic version)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var p = _projectRepository.GetByVersionId((int)version.projectVersionId);
            if (p.CustodyOwner != currentDqeUser)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Project {0} is not currently owned by {1}", p.ProjectNumber, p.CustodyOwner.Name) });
            }
            p.AssignWorkingEstimate(p.ProjectVersions.FirstOrDefault(i => i.Id == version.projectVersionId), currentDqeUser);
            return ResultStructureFromProjectSelection(p, currentDqeUser);
        }

        [HttpPost]
        public ActionResult CreateProjectVersionFromEstimate(dynamic snapshot)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var p = _projectRepository.GetByEstimateId((int)snapshot.projectSnapshotId);
            var sourceVersion = p.ProjectVersions.First(i => i.ProjectEstimates.Any(ii => ii.Id == (int)snapshot.projectSnapshotId));
            var source = sourceVersion.ProjectEstimates.First(i => i.Id == (int)snapshot.projectSnapshotId);
            p.CreateNewVersionFromSnapshot(string.Empty, source, currentDqeUser);
            return ResultStructureFromProjectSelection(p, currentDqeUser);
        }

        [HttpPost]
        public ActionResult SaveComment(dynamic project)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var p = _projectRepository.Get((int)project.id);
            currentDqeUser.MyRecentProjectEstimate.SetComment(project.comment, currentDqeUser);
            return ResultStructureFromProjectSelection(p, currentDqeUser);
        }

        [HttpGet]
        public ActionResult GetProjects(string number)
        {
            var projects = _webTransportService.GetProjects(number);
            return Json(projects
                .Select(i => new
                {
                    id = i.Id,
                    number = i.ProjectNumber,
                }),
                JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetProposals(string number)
        {
            var proposals = _webTransportService.GetProposals(number);
            return Json(proposals
                .Select(i => new
                {
                    id = i.Id,
                    number = i.ProposalNumber,
                }),
                JsonRequestBehavior.AllowGet);
        }

        private ActionResult ResultStructureFromProjectSelection(Project project, DqeUser currentUser)
        {
            var version = project.ProjectVersions.Where(i => i.VersionOwner == currentUser).FirstOrDefault(i => i.ProjectEstimates.FirstOrDefault(ii => ii.IsWorkingEstimate) != null);
            ProjectEstimate snapshot = null;
            if (version != null)
            {
                snapshot = version.ProjectEstimates.FirstOrDefault(ii => ii.IsWorkingEstimate);
            }
            return new DqeResult(new
            {
                project = new
                {
                    id = project.Id,
                    number = project.ProjectNumber,
                    description = project.Description,
                    district = project.District,
                    county = project.MyCounty.Name,
                    comment = snapshot == null ? string.Empty : snapshot.EstimateComment,
                    lettingDate = project.LettingDate,
                    isAvailable = project.CustodyOwner == null,
                    userHasCustody = project.CustodyOwner == currentUser,
                    custodyOwner = project.CustodyOwner == null ? string.Empty : project.CustodyOwner.Name,
                    nextLabel = project.GetNextSnapshotLabel() == SnapshotLabel.Phase2 ? "Phase II" : project.GetNextSnapshotLabel() == SnapshotLabel.Phase3 ? "Phase III" : string.Empty
                },
                proposals = project.Proposals.Select(i => new
                {
                    number = i.ProposalNumber,
                    source = i.ProposalSource == ProposalSourceType.Wt ? "Web Trns*Port" : "Gaming",
                    comment = i.Comment,
                    created = i.Created,
                    lastUpdated = i.LastUpdated
                }),
                workingEstimate = snapshot == null
                ? new
                {
                    projectVersionId = 0,
                    projectSnapshotId = 0,
                    projectVersion = 0,
                    projectSnapshot = 0,
                    label = string.Empty,
                    created = string.Empty,
                    lastUpdated = string.Empty,
                    estimate = (decimal)0,
                    comment = string.Empty,
                    owner = string.Empty,
                    source = string.Empty
                }
                : new
                {
                    projectVersionId = snapshot.MyProjectVersion.Id,
                    projectSnapshotId = snapshot.Id,
                    projectVersion = snapshot.MyProjectVersion.Version,
                    projectSnapshot = snapshot.Estimate,
                    label = string.Empty,
                    created = string.Format("{0} @ {1}", snapshot.Created.ToShortDateString(), snapshot.Created.ToShortTimeString()),
                    lastUpdated = string.Format("{0} @ {1}", snapshot.LastUpdated.ToShortDateString(), snapshot.LastUpdated.ToShortTimeString()),
                    estimate = snapshot.EstimateGroups.Sum(i => i.ProjectItems.Sum(ii => ii.Price * ii.Quantity)),
                    comment = snapshot.EstimateComment,
                    owner = snapshot.MyProjectVersion.VersionOwner.Name,
                    source = snapshot.MyProjectVersion.ProjectSource == ProjectSourceType.Lre
                        ? "LRE"
                        : snapshot.MyProjectVersion.ProjectSource == ProjectSourceType.Wt
                            ? "Web Trans*Port"
                            : string.Format("Version {0} Estimate {1}", snapshot.MyProjectVersion.EstimateSource.MyProjectVersion.Version, snapshot.MyProjectVersion.EstimateSource.Estimate)
                },
                versions = project
                    .ProjectVersions
                    .OrderBy(i => i.Version)
                    .Select(i => new
                    {
                        projectVersionId = i.Id,
                        owner = i.VersionOwner.Name,
                        isCurrentOwner = i.VersionOwner == currentUser,
                        projectVersion = i.Version,
                        source = i.ProjectSource == ProjectSourceType.Lre
                            ? "LRE"
                            : i.ProjectSource == ProjectSourceType.Wt
                                ? "Web Trns*port"
                                : string.Format("Version {0} Estimate {1}", i.EstimateSource.MyProjectVersion.Version, i.EstimateSource.Estimate),
                        hasWorkingEstimate = i.ProjectEstimates.Any(ii => ii.IsWorkingEstimate),
                        snapshots = i
                            .ProjectEstimates
                            .OrderBy(ii => ii.Estimate)
                            .Select(ii => new
                            {
                                projectSnapshotId = ii.Id,
                                projectSnapshot = ii.Estimate,
                                created = string.Format("{0} @ {1}", ii.Created.ToShortDateString(), ii.Created.ToShortTimeString()),
                                lastUpdated = string.Format("{0} @ {1}", ii.LastUpdated.ToShortDateString(), ii.LastUpdated.ToShortTimeString()),
                                comment = ii.EstimateComment,
                                estimate = ii.EstimateGroups.Sum(iii => iii.ProjectItems.Sum(iiii => iiii.Price * iiii.Quantity)),
                                label = ii.Label == SnapshotLabel.Phase2
                                        ? "Phase II"
                                        : ii.Label == SnapshotLabel.Phase3 ? "Phase III" : string.Empty,
                                isWorkingEstimate = ii.IsWorkingEstimate ? "Yes" : string.Empty
                            })
                    })
            },
                new ClientMessage
                {
                    Severity = ClientMessageSeverity.Success,
                    text = string.Empty
                },
                JsonRequestBehavior.AllowGet);
        }

        private ActionResult ResultStructureFromSnapshot(ProjectEstimate snapshot, DqeUser currentUser)
        {
            return new DqeResult(new
            {
                project = new
                {
                    id = snapshot.MyProjectVersion.MyProject.Id,
                    number = snapshot.MyProjectVersion.MyProject.ProjectNumber,
                    description = snapshot.MyProjectVersion.MyProject.Description,
                    district = snapshot.MyProjectVersion.MyProject.District,
                    county = snapshot.MyProjectVersion.MyProject.MyCounty.Name,
                    comment = snapshot.EstimateComment,
                    lettingDate = snapshot.MyProjectVersion.MyProject.LettingDate,
                    isAvailable = snapshot.MyProjectVersion.MyProject.CustodyOwner == null,
                    userHasCustody = snapshot.MyProjectVersion.MyProject.CustodyOwner == currentUser,
                    custodyOwner = snapshot.MyProjectVersion.MyProject.CustodyOwner == null ? string.Empty : snapshot.MyProjectVersion.MyProject.CustodyOwner.Name,
                    nextLabel = snapshot.MyProjectVersion.MyProject.GetNextSnapshotLabel() == SnapshotLabel.Phase2 ? "Phase II" : snapshot.MyProjectVersion.MyProject.GetNextSnapshotLabel() == SnapshotLabel.Phase3 ? "Phase III" : string.Empty
                },
                proposals = snapshot.MyProjectVersion.MyProject.Proposals.Select(i => new
                {
                    number = i.ProposalNumber,
                    source = i.ProposalSource == ProposalSourceType.Wt ? "Web Trns*Port" : "Gaming",
                    comment = i.Comment,
                    created = i.Created,
                    lastUpdated = i.LastUpdated
                }),
                workingEstimate = new
                {
                    projectVersionId = snapshot.MyProjectVersion.Id,
                    projectSnapshotId = snapshot.Id,
                    projectVersion = snapshot.MyProjectVersion.Version,
                    projectSnapshot = snapshot.Estimate,
                    label = string.Empty,
                    created = string.Format("{0} @ {1}", snapshot.Created.ToShortDateString(), snapshot.Created.ToShortTimeString()),
                    lastUpdated = string.Format("{0} @ {1}", snapshot.LastUpdated.ToShortDateString(), snapshot.LastUpdated.ToShortTimeString()),
                    estimate = snapshot.EstimateGroups.Sum(i => i.ProjectItems.Sum(ii => ii.Price * ii.Quantity)),
                    comment = snapshot.EstimateComment,
                    owner = snapshot.MyProjectVersion.VersionOwner.Name,
                    source = snapshot.MyProjectVersion.ProjectSource == ProjectSourceType.Lre
                        ? "LRE"
                        : snapshot.MyProjectVersion.ProjectSource == ProjectSourceType.Wt
                            ? "Web Trns*port"
                            : string.Format("Version {0} Estimate {1}", snapshot.MyProjectVersion.EstimateSource.MyProjectVersion.Version, snapshot.MyProjectVersion.EstimateSource.Estimate)
                },
                versions = snapshot
                    .MyProjectVersion
                    .MyProject
                    .ProjectVersions
                    .OrderBy(i => i.Version)
                    .Select(i => new
                    {
                        projectVersionId = i.Id,
                        owner = i.VersionOwner.Name,
                        isCurrentOwner = i.VersionOwner == currentUser,
                        projectVersion = i.Version,
                        source = i.ProjectSource == ProjectSourceType.Lre
                            ? "LRE"
                            : i.ProjectSource == ProjectSourceType.Wt
                                ? "Web Trns*port"
                                : string.Format("Version {0} Estimate {1}", i.EstimateSource.MyProjectVersion.Version, i.EstimateSource.Estimate),
                        hasWorkingEstimate = i.ProjectEstimates.Any(ii => ii.IsWorkingEstimate),
                        snapshots = i
                            .ProjectEstimates
                            .OrderBy(ii => ii.Estimate)
                            .Select(ii => new
                            {
                                projectSnapshotId = ii.Id,
                                projectSnapshot = ii.Estimate,
                                created = string.Format("{0} @ {1}", ii.Created.ToShortDateString(), ii.Created.ToShortTimeString()),
                                lastUpdated = string.Format("{0} @ {1}", ii.LastUpdated.ToShortDateString(), ii.LastUpdated.ToShortTimeString()),
                                comment = ii.EstimateComment,
                                estimate = ii.EstimateGroups.Sum(iii => iii.ProjectItems.Sum(iiii => iiii.Price * iiii.Quantity)),
                                label = ii.Label == SnapshotLabel.Phase2
                                        ? "Phase II"
                                        : ii.Label == SnapshotLabel.Phase3 ? "Phase III" : string.Empty,
                                isWorkingEstimate = ii.IsWorkingEstimate ? "Yes" : string.Empty
                            })
                    })
            },
                new ClientMessage
                {
                    Severity = ClientMessageSeverity.Success,
                    text = string.Empty
                },
                JsonRequestBehavior.AllowGet);
        }
    }
}