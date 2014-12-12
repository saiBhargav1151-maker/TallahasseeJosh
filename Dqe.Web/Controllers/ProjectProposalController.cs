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
        private readonly ITransactionManager _transactionManager;

        public ProjectProposalController
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
        public ActionResult GetRecentProject()
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            if (currentDqeUser.MyRecentProjectEstimate == null)
            {
                return new DqeResult(null, JsonRequestBehavior.AllowGet);
            }
            //sync project header
            var r = GetProject(currentDqeUser.MyRecentProjectEstimate.MyProjectVersion.MyProject.ProjectNumber);
            if (!((DqeResult) r).IsValid()) return r;
            var result = ResultStructureFromSnapshot(currentDqeUser.MyRecentProjectEstimate, currentDqeUser);
            return result;
        }

        [HttpGet]
        public ActionResult GetRecentProposal()
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            if (currentDqeUser.MyRecentProposal == null)
            {
                return new DqeResult(null, JsonRequestBehavior.AllowGet);
            }
            var result = ResultStructureFromProposal(currentDqeUser.MyRecentProposal, currentDqeUser);
            return result;
        }

        [HttpGet]
        public ActionResult GetProposal(string number)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            //is proposal in DQE?
            var prop = _proposalRepository.GetWtByNumber(number);
            if (prop == null)
            {
                //create proposal and load projects
                var p = _webTransportService.GetProposal(number);
                if (p == null) throw new InvalidOperationException(string.Format("Proposal {0} was not found in WT", number));
                var pro = AddProposal(p, currentDqeUser);
                if (pro as DqeResult != null)
                {
                    _transactionManager.Abort();
                    return (DqeResult) pro;
                }
                currentDqeUser.SetRecentProposal((Proposal)pro);
                return ResultStructureFromProposal((Proposal)pro, currentDqeUser);
            }
            currentDqeUser.SetRecentProposal(prop);
            return ResultStructureFromProposal(prop, currentDqeUser);
        }

        [HttpGet]
        public ActionResult IsProjectSynced(int projectId)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            if (currentDqeUser.MyRecentProjectEstimate == null) return new DqeResult(new { isSynced = false }, JsonRequestBehavior.AllowGet);
            var p = _projectRepository.Get(projectId);
            if (p == null) return new DqeResult(new { isSynced = false }, JsonRequestBehavior.AllowGet);
            var vs = p.ProjectVersions.Where(i => i.VersionOwner == currentDqeUser).Distinct().ToList();
            if (vs.Count == 0) return new DqeResult(new { isSynced = false }, JsonRequestBehavior.AllowGet);
            var v = vs.FirstOrDefault(i => i.ProjectEstimates.Any(ii => ii.IsWorkingEstimate));
            if (v == null) return new DqeResult(new { isSynced = false }, JsonRequestBehavior.AllowGet);
            var est = v.ProjectEstimates.FirstOrDefault(i => i.IsWorkingEstimate);
            if (est == null) return new DqeResult(new { isSynced = false }, JsonRequestBehavior.AllowGet);
            var isSynced = est.IsSyncedWithWt();
            return new DqeResult(new { isSynced }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SyncWorkingEstimate(dynamic estimate)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var estimateId = (int)estimate.projectSnapshotId;
            var p = _projectRepository.GetByEstimateId(estimateId);
            var v = p.ProjectVersions.FirstOrDefault(i => i.ProjectEstimates.Any(ii => ii.Id == estimateId));
            if (v == null) throw new InvalidOperationException("Version for estimate not found");
            var e = v.ProjectEstimates.FirstOrDefault(i => i.Id == estimateId);
            if (e == null) throw new InvalidOperationException("Estimate not found");
            e.SyncWithWt(currentDqeUser);
            return new DqeResult(null, new ClientMessage{ Severity = ClientMessageSeverity.Success, text = "Your working estimate is now synchronized with Web Trns*Port"});
        }

        [HttpPost]
        public ActionResult AuthorizeUser(dynamic user)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var assignmentUser = _dqeUserRepository.GetBySrsId((int)user.id);
            var project = _projectRepository.Get((int)user.project.id);
            if (assignmentUser == null)
            {
                throw new InvalidOperationException(string.Format("User {0} not found", user.name));
            }
            if (project == null)
            {
                throw new InvalidOperationException(string.Format("Project {0} not found", user.project.number));
            }
            currentDqeUser.AssignProjectToUser(project, assignmentUser);
            _commandRepository.Flush();
            var authorizedUsers = project.AssignedUsers.Select(i => new
            {
                id = i.Id,
                name = i.Name,
                district = i.District
            });
            return new DqeResult(authorizedUsers, new ClientMessage { Severity = ClientMessageSeverity.Success, text = "User is now authorized to work on the project" });
        }

        [HttpPost]
        public ActionResult DeauthorizeUser(dynamic user)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var assignmentUser = _dqeUserRepository.Get((int)user.id);
            var project = _projectRepository.Get((int)user.project.id);
            if (assignmentUser == null)
            {
                throw new InvalidOperationException(string.Format("User {0} not found", user.name));
            }
            if (project == null)
            {
                throw new InvalidOperationException(string.Format("Project {0} not found", user.project.number));
            }
            currentDqeUser.UnassignProjectToUser(project, assignmentUser);
            _commandRepository.Flush();
            var authorizedUsers = project.AssignedUsers.Select(i => new
            {
                id = i.Id,
                name = i.Name,
                district = i.District
            });
            return new DqeResult(authorizedUsers, new ClientMessage { Severity = ClientMessageSeverity.Success, text = "User is no longer authorized to work on the project" });
        }

        private ActionResult ResultStructureFromProposal(Proposal prop, DqeUser currentDqeUser)
        {
            var nextSnapshot = prop.GetNextSnapshotLabel();
            return new DqeResult(new
            {
                proposal = new
                {
                    id = prop.Id,
                    wtId = prop.Id,
                    created = prop.Created,
                    lastUpdated = prop.LastUpdated,
                    number = prop.ProposalNumber,
                    description = prop.Description,
                    district = prop.District,
                    county = prop.County.Name,
                    lettingDate = prop.LettingDate.HasValue ? prop.LettingDate.Value.ToShortDateString() : string.Empty,
                    comment = prop.Comment,
                    hasCustody = prop.Projects.All(i => i.CustodyOwner == currentDqeUser),
                    canSnapshot = prop.Projects.All(i => i.ProjectHasWorkingEstimateForUser(currentDqeUser)),
                    nextEstimate = nextSnapshot == SnapshotLabel.Official
                        ? "Official"
                        : nextSnapshot == SnapshotLabel.Authorization
                            ? "Authorization"
                            : nextSnapshot == SnapshotLabel.Phase3
                                ? "Phase III"
                                : nextSnapshot == SnapshotLabel.Phase2
                                    ? "Phase II"
                                    : string.Empty
                },
                projects = prop.Projects.OrderBy(i => i.ProjectNumber).Select(i => new
                {
                    id = i.Id,
                    wtId = i.WtId,
                    number = i.ProjectNumber,
                    district = i.District,
                    description = i.Description,
                    designer = i.DesignerName,
                    county = i.MyCounty.Name,
                    owner = i.CustodyOwner == null ? string.Empty : i.CustodyOwner.Name,
                    hasCustody = i.CustodyOwner == currentDqeUser,
                    label = i.GetCurrentSnapshotLabel() == SnapshotLabel.Official
                        ? "Official"
                        : i.GetCurrentSnapshotLabel() == SnapshotLabel.Authorization
                            ? "Authorization"
                            : i.GetCurrentSnapshotLabel() == SnapshotLabel.Phase3
                                ? "Phase III"
                                : i.GetCurrentSnapshotLabel() == SnapshotLabel.Phase2
                                    ? "Phase II"
                                    : string.Empty,
                    hasWorkingEstimate = i.ProjectHasWorkingEstimateForUser(currentDqeUser)
                })
            },
                new ClientMessage
                {
                    Severity = ClientMessageSeverity.Success,
                    text = string.Empty
                },
                JsonRequestBehavior.AllowGet);
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
                var r = AddProjectToDqe(number, currentDqeUser);
                if (r as DqeResult != null)
                {
                    _transactionManager.Abort();
                    return (DqeResult) r;
                }
                var p = r as Project;
                if (p == null) throw new InvalidOperationException("Expected project cast");
                //does WT have a proposal
                var wtp = _webTransportService.GetProject(p.ProjectNumber);
                if (wtp != null && wtp.MyProposal != null)
                {
                    var result = AddProposal(wtp.MyProposal, currentDqeUser);
                    if (result as DqeResult != null)
                    {
                        _transactionManager.Abort();
                        return (DqeResult)result;
                    }
                }
                return ResultStructureFromProjectSelection(p, currentDqeUser);
            }
            else
            {
                currentDqeUser.SetRecentProject(project);
                var wtp = _webTransportService.GetProject(project.ProjectNumber);
                if (wtp != null)
                {
                    //sync project
                    int mfid;
                    if (!int.TryParse(wtp.SpecBook.Trim(), out mfid))
                    {
                        throw new InvalidOperationException(string.Format("Could not parse spec year from WT project {0}", wtp.ProjectNumber));
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
                    //var p = new Project(_projectRepository, _commandRepository, _webTransportService);
                    var t = project.GetTransformer();
                    t.WtId = wtp.Id;
                    t.Description = wtp.Description;
                    t.ProjectNumber = wtp.ProjectNumber;
                    var district = wtp.Districts.FirstOrDefault(i => i.PrimaryDistrict);
                    if (district == null)
                    {
                        throw new InvalidOperationException(string.Format("Primary District was not found for project {0}", wtp.ProjectNumber));
                    }
                    t.District = district.MyRefDistrict.Name;
                    //t.LettingDate = wtProjectEstimate.MyProposal == null || wtProjectEstimate.MyProposal.MyLetting == null
                    //    ? (DateTime?)null
                    //    : wtProjectEstimate.MyProposal.MyLetting.LettingDate;
                    project.Transform(t, currentDqeUser);
                    var projectCounty = wtp.Counties.FirstOrDefault(i => i.PrimaryCounty);
                    if (projectCounty == null)
                    {
                        throw new InvalidOperationException(string.Format("Primary County was not found for project {0}", wtp.ProjectNumber));
                    }
                    var county = _marketAreaRepository.GetCountyByCode(projectCounty.MyRefCounty.Name);
                    if (county == null)
                    {
                        throw new InvalidOperationException(string.Format("County {0} was not found in DQE", projectCounty.MyRefCounty.Name));
                    }
                    project.SetCounty(county);
                }
                var currentProp = project.Proposals.FirstOrDefault(i => i.ProposalSource == ProposalSourceType.Wt);
                if (currentProp == null)
                {
                    if (wtp != null && wtp.MyProposal != null)
                    {
                        //create WT proposal in DQE
                        var result = AddProposal(wtp.MyProposal, currentDqeUser);
                        if (result as DqeResult != null)
                        {
                            _transactionManager.Abort();
                            return (DqeResult)result;
                        }
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
                            var result = AddProposal(wtp.MyProposal, currentDqeUser);
                            if (result as DqeResult != null)
                            {
                                _transactionManager.Abort();
                                return (DqeResult)result;
                            }
                        }
                    }
                    if (wtp != null && wtp.MyProposal == null)
                    {
                        //proposal in WT removed - convert WT proposal to Gaming
                        currentProp.ConvertToGaming();
                    }
                }
            }
            //in DQE
            return ResultStructureFromProjectSelection(project, currentDqeUser);
        }

        private object AddProjectToDqe(string number, DqeUser currentDqeUser)
        {
            //not in DQE
            var wtProjectEstimate = _webTransportService.ExportProject(number);
            if (wtProjectEstimate == null)
            {
                return new DqeResult(null,
                    new ClientMessage
                    {
                        Severity = ClientMessageSeverity.Error,
                        text = string.Format("Project {0} was not found in Web Trns*Port", number)
                    },
                    JsonRequestBehavior.AllowGet);
            }
            int mfid;
            if (!int.TryParse(wtProjectEstimate.SpecBook.Trim(), out mfid))
            {
                throw new InvalidOperationException(string.Format("Could not parse spec year from WT project {0}",
                    wtProjectEstimate.ProjectNumber));
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
            var p = new Project(_projectRepository, _commandRepository, _webTransportService);
            var t = p.GetTransformer();
            t.WtId = wtProjectEstimate.Id;
            t.WtLsDbId = wtProjectEstimate.LsDbId;
            t.LsDbCode = string.IsNullOrWhiteSpace(wtProjectEstimate.LsDbCode) ? string.Empty : wtProjectEstimate.LsDbCode;
            t.Description = wtProjectEstimate.Description;
            t.ProjectNumber = wtProjectEstimate.ProjectNumber;
            var district = wtProjectEstimate.Districts.FirstOrDefault(i => i.PrimaryDistrict);
            if (district == null)
            {
                throw new InvalidOperationException(string.Format("Primary District was not found for project {0}", wtProjectEstimate.ProjectNumber));
            }
            t.District =  district.MyRefDistrict.Name;
            //t.LettingDate = wtProjectEstimate.MyProposal == null || wtProjectEstimate.MyProposal.MyLetting == null
            //    ? (DateTime?)null
            //    : wtProjectEstimate.MyProposal.MyLetting.LettingDate;
            p.Transform(t, currentDqeUser);
            var projectCounty = wtProjectEstimate.Counties.FirstOrDefault(i => i.PrimaryCounty);
            if (projectCounty == null)
            {
                throw new InvalidOperationException(string.Format("Primary County was not found for project {0}", wtProjectEstimate.ProjectNumber));
            }
            var county = _marketAreaRepository.GetCountyByCode(projectCounty.MyRefCounty.Name);
            if (county == null)
            {
                throw new InvalidOperationException(string.Format("County {0} was not found in DQE", projectCounty.MyRefCounty.Name));
            }
            p.SetCounty(county);
            mf.AddProject(p, currentDqeUser);
            _commandRepository.Add(p);
            return p;
        }

        private object AddProposal(Domain.Model.Wt.Proposal wtp, DqeUser currentDqeUser)
        {
            var prop = new Proposal(_proposalRepository);
            var propt = prop.GetTransformer();
            propt.ProposalNumber = wtp.ProposalNumber;
            propt.ProposalSource = ProposalSourceType.Wt;
            propt.Comment = "Loaded from Web Trns*Port";
            propt.Description = wtp.Description;
            propt.WtId = wtp.Id;
            propt.LettingDate = wtp.MyLetting == null ? (DateTime?)null : wtp.MyLetting.LettingDate;
            propt.District = wtp.District.Name;
            prop.Transform(propt, currentDqeUser);
            var proposalCounty = wtp.County;
            var county = _marketAreaRepository.GetCountyByCode(proposalCounty.Name);
            if (county == null)
            {
                throw new InvalidOperationException(string.Format("County {0} was not found in DQE", proposalCounty.Name));
            }
            prop.SetCounty(county);
            var projects = _webTransportService.GetProjectsByProposalId(wtp.Id);
            foreach (var p in projects)
            {
                var ep = _projectRepository.GetByProjectNumber(p.ProjectNumber);
                if (ep != null)
                {
                    ep.AddProposal(prop, currentDqeUser);
                    continue;
                }
                var r = AddProjectToDqe(p.ProjectNumber, currentDqeUser);
                if (r as DqeResult != null) return r; 
                if (r as Project != null) ((Project) r).AddProposal(prop, currentDqeUser);
            }
            return prop;
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
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Project {0} was not found in Web Trns*Port", number) }, JsonRequestBehavior.AllowGet);
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
            if (p.CustodyOwner == currentDqeUser 
                || currentDqeUser.Role == DqeRole.Administrator 
                || (currentDqeUser.Role == DqeRole.DistrictAdministrator && currentDqeUser.IsInDqeDistrict((((int)project.district).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')))))
            {
                p.ReleaseCustody(currentDqeUser);
                return ResultStructureFromProjectSelection(p, currentDqeUser);       
            }
            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Project {0} is not owned by you", project.number) }); 
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
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Project {0} is not currently owned by {1}", project.number, currentDqeUser.Name) });
            }
            p.SnapshotWorkingEstimate(currentDqeUser, DynamicHelper.HasNotNullProperty(project, "takeLabeledSnapshot") && project.takeLabeledSnapshot);
            return ResultStructureFromProjectSelection(p, currentDqeUser);
        }

        [HttpPost]
        public ActionResult SnapshotProposalWorkingEstimate(dynamic proposal)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var p = currentDqeUser.MyRecentProposal;
            if (!currentDqeUser.CanEstimateRecentProposal())
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Proposal {0} is not currently owned by {1}", proposal.number, currentDqeUser.Name) });
            }
            p.SnapshotWorkingEstimate(currentDqeUser, DynamicHelper.HasNotNullProperty(proposal, "takeLabeledSnapshot") && proposal.takeLabeledSnapshot);
            return ResultStructureFromProposal(p, currentDqeUser);
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
            var wtProposal = project.Proposals.FirstOrDefault(i => i.ProposalSource == ProposalSourceType.Wt);
            var version = project.ProjectVersions.Where(i => i.VersionOwner == currentUser).FirstOrDefault(i => i.ProjectEstimates.FirstOrDefault(ii => ii.IsWorkingEstimate) != null);
            ProjectEstimate snapshot = null;
            if (version != null)
            {
                snapshot = version.ProjectEstimates.FirstOrDefault(ii => ii.IsWorkingEstimate);
            }

            return new DqeResult(new
            {
                security = new
                {
                    isAuthorized = currentUser.Role == DqeRole.Administrator || currentUser.IsInDqeDistrict(project.District) || currentUser.IsAuthorizedOnProject(project),
                    isSystemAdmin = currentUser.Role == DqeRole.Administrator,
                    isDistrictAdmin = currentUser.Role == DqeRole.DistrictAdministrator && currentUser.IsInDqeDistrict(project.District)    
                },
                authorizedUsers = project.AssignedUsers.Select(i => new
                {
                    id = i.Id,
                    name = i.Name,
                    district = i.District
                }),
                project = new
                {
                    id = project.Id,
                    wtId = project.WtId,
                    number = project.ProjectNumber,
                    description = project.Description,
                    district = project.District,
                    designer = project.DesignerName,
                    county = project.MyCounty.Name,
                    comment = snapshot == null ? string.Empty : snapshot.EstimateComment,
                    lettingDate = wtProposal == null ? string.Empty : wtProposal.LettingDate.HasValue ? wtProposal.LettingDate.Value.ToShortDateString() : string.Empty,
                    isAvailable = project.CustodyOwner == null,
                    userHasCustody = project.CustodyOwner == currentUser,
                    custodyOwner = project.CustodyOwner == null ? string.Empty : project.CustodyOwner.Name,
                    nextLabel = project.GetNextSnapshotLabel() == SnapshotLabel.Phase2 ? "Phase II" : project.GetNextSnapshotLabel() == SnapshotLabel.Phase3 ? "Phase III" : string.Empty
                },
                proposals = project.Proposals.Select(i => new
                {
                    id = i.Id,
                    wtId = i.WtId,
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
                    estimate = new EstimateTotal(),
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
                    estimate = snapshot.GetEstimateTotal(),
                    comment = snapshot.EstimateComment,
                    owner = snapshot.MyProjectVersion.VersionOwner.Name,
                    source = snapshot.MyProjectVersion.ProjectSource == ProjectSourceType.Lre
                        ? "LRE"
                        : snapshot.MyProjectVersion.ProjectSource == ProjectSourceType.Wt
                            ? "Web Trns*Port"
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
                                ? "Web Trns*Port"
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
                                estimate = ii.GetEstimateTotal(),
                                label = ii.Label == SnapshotLabel.Phase2
                                        ? "Phase II"
                                        : ii.Label == SnapshotLabel.Phase3
                                            ? "Phase III"
                                            : ii.Label == SnapshotLabel.Authorization
                                                ? "Authorization"
                                                : ii.Label == SnapshotLabel.Official
                                                    ? "Official"
                                                    : string.Empty,
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
            var wtProposal = snapshot.MyProjectVersion.MyProject.Proposals.FirstOrDefault(i => i.ProposalSource == ProposalSourceType.Wt);
            return new DqeResult(new
            {
                security = new
                {
                    isAuthorized = currentUser.Role == DqeRole.Administrator || currentUser.IsInDqeDistrict(snapshot.MyProjectVersion.MyProject.District) || currentUser.IsAuthorizedOnProject(snapshot.MyProjectVersion.MyProject),
                    isSystemAdmin = currentUser.Role == DqeRole.Administrator,
                    isDistrictAdmin = currentUser.Role == DqeRole.DistrictAdministrator && currentUser.IsInDqeDistrict(snapshot.MyProjectVersion.MyProject.District)    
                },
                authorizedUsers = snapshot.MyProjectVersion.MyProject.AssignedUsers.Select(i => new
                {
                    id = i.Id,
                    name = i.Name,
                    district = i.District
                }),
                project = new
                {
                    id = snapshot.MyProjectVersion.MyProject.Id,
                    wtId = snapshot.MyProjectVersion.MyProject.WtId,
                    number = snapshot.MyProjectVersion.MyProject.ProjectNumber,
                    description = snapshot.MyProjectVersion.MyProject.Description,
                    district = snapshot.MyProjectVersion.MyProject.District,
                    designer = snapshot.MyProjectVersion.MyProject.DesignerName,
                    county = snapshot.MyProjectVersion.MyProject.MyCounty.Name,
                    comment = snapshot.EstimateComment,
                    lettingDate = wtProposal == null ? string.Empty : wtProposal.LettingDate.HasValue ? wtProposal.LettingDate.Value.ToShortDateString() : string.Empty,
                    isAvailable = snapshot.MyProjectVersion.MyProject.CustodyOwner == null,
                    userHasCustody = snapshot.MyProjectVersion.MyProject.CustodyOwner == currentUser,
                    custodyOwner = snapshot.MyProjectVersion.MyProject.CustodyOwner == null ? string.Empty : snapshot.MyProjectVersion.MyProject.CustodyOwner.Name,
                    nextLabel = snapshot.MyProjectVersion.MyProject.GetNextSnapshotLabel() == SnapshotLabel.Phase2 ? "Phase II" : snapshot.MyProjectVersion.MyProject.GetNextSnapshotLabel() == SnapshotLabel.Phase3 ? "Phase III" : string.Empty
                },
                proposals = snapshot.MyProjectVersion.MyProject.Proposals.Select(i => new
                {
                    id = i.Id,
                    wtId = i.WtId,
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
                    estimate = snapshot.GetEstimateTotal(),
                    comment = snapshot.EstimateComment,
                    owner = snapshot.MyProjectVersion.VersionOwner.Name,
                    source = snapshot.MyProjectVersion.ProjectSource == ProjectSourceType.Lre
                        ? "LRE"
                        : snapshot.MyProjectVersion.ProjectSource == ProjectSourceType.Wt
                            ? "Web Trns*Port"
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
                                ? "Web Trns*Port"
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
                                estimate = ii.GetEstimateTotal(),
                                label = ii.Label == SnapshotLabel.Phase2
                                        ? "Phase II"
                                        : ii.Label == SnapshotLabel.Phase3 
                                            ? "Phase III"
                                            : ii.Label == SnapshotLabel.Authorization
                                                ? "Authorization"
                                                : ii.Label == SnapshotLabel.Official
                                                    ? "Official"
                                                    : string.Empty,
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